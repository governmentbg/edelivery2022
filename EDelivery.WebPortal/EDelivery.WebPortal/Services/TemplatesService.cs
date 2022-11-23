using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using EDelivery.WebPortal.Models.Messages;
using EDelivery.WebPortal.Models.Templates.Components;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EDelivery.WebPortal
{
    public class FileObject
    {
        public int? FileId { get; set; }

        public string FileName { get; set; }

        public string FileHash { get; set; }
    }

    public class FieldObject
    {
        public FieldObject(string label, ComponentType type, object value)
        {
            this.Label = label;
            this.Type = type;
            this.Value = value;
        }

        public string Label { get; set; }

        public ComponentType Type { get; set; }

        public object Value { get; set; }
    }

    public class TemplatesService
    {
        public static List<BaseComponent> ParseJsonWithValues(
            string json,
            Dictionary<string, string[]> values)
        {
            List<BaseComponent> components = ParseJson(json);

            if (values.Any())
            {
                foreach (BaseComponent component in components)
                {
                    if (values.ContainsKey($"{component.Id}")
                        && values[$"{component.Id}"].Any())
                    {
                        switch (component.Type)
                        {
                            case ComponentType.textfield:
                                var textfield = component as TextFieldComponent;
                                textfield.Value = values[$"{component.Id}"].FirstOrDefault();

                                break;
                            case ComponentType.hidden:
                                var hidden = component as HiddenComponent;
                                hidden.Value = values[$"{component.Id}"].FirstOrDefault();

                                break;
                            case ComponentType.textarea:
                                var textarea = component as TextAreaComponent;
                                textarea.Value = values[$"{component.Id}"].FirstOrDefault();

                                break;
                            case ComponentType.datetime:
                                var datetime = component as DateTimeComponent;
                                datetime.Value = values[$"{component.Id}"].FirstOrDefault();

                                break;
                            case ComponentType.select:
                                var select = component as SelectComponent;
                                select.Value = values[$"{component.Id}"].FirstOrDefault();

                                break;
                            case ComponentType.checkbox:
                                var checkbox = component as CheckboxComponent;
                                checkbox.Value =
                                    bool.Parse(values[$"{component.Id}"].FirstOrDefault() ?? "false");

                                break;
                            case ComponentType.file:
                                var file = component as FileComponent;

                                string[] splitFileIds = values[$"{component.Id}"];
                                string[] splitFileNames = values[$"{component.Id}-FileName"];
                                string[] splitFileHashes = values[$"{component.Id}-FileHash"];

                                for (int i = 0; i < splitFileIds.Length; i++)
                                {
                                    file.FileIds[i] = string.IsNullOrWhiteSpace(splitFileIds[i])
                                        ? (int?)null
                                        : int.Parse(splitFileIds[i]);

                                    file.FileNames[i] = splitFileNames[i];
                                    file.FileHashes[i] = splitFileHashes[i];
                                }

                                break;
                            case ComponentType.markdown:
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
            }

            return components;
        }

        /// <summary>
        /// Convert FormCollection to typed dictionary
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public static Dictionary<string, string[]> ToDictionary(FormCollection form)
        {
            return form.AllKeys.ToDictionary(k => k, v => form.GetValues(v));
        }

        /// <summary>
        /// Validate values according to template validation rules
        /// </summary>
        /// <param name="form">form values</param>
        /// <param name="templateJson">template json</param>
        /// <returns></returns>
        public static Dictionary<string, string> Validate(
            FormCollection form,
            string templateJson)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();

            List<BaseComponent> components = ParseJson(templateJson);

            string[] values;

            foreach (var component in components.Where(e => e.IsRequired))
            {
                switch (component.Type)
                {
                    case ComponentType.file:
                        values = form.GetValues($"{component.Id}");

                        if (values == null
                            || !values.Any(e => !string.IsNullOrEmpty(e)))
                        {
                            if (!errors.ContainsKey($"{component.Id}_0"))
                            {
                                // file inputs  are rendered as name_[number]
                                // that's why we add an error to the first file input
                                errors.Add($"{component.Id}_0", "Задължително поле.");
                            }
                        }
                        break;
                    case ComponentType.checkbox:
                    case ComponentType.datetime:
                    case ComponentType.hidden:
                    case ComponentType.select:
                    case ComponentType.textarea:
                    case ComponentType.textfield:
                        values = form.GetValues($"{component.Id}");

                        if (values == null
                            || !values.Any(e => !string.IsNullOrEmpty(e)))
                        {
                            if (!errors.ContainsKey($"{component.Id}"))
                            {
                                errors.Add($"{component.Id}", "Задължително поле.");
                            }
                        }
                        break;
                    case ComponentType.markdown:
                    default:
                        break;
                }
            }

            return errors;
        }

        /// <summary>
        /// Extract 3 values (body as json, metafields as json, array of blob ids)
        /// that will be recorded in the dabase
        /// </summary>
        /// <param name="templateJson"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public static (string, string, int[]) ExtractPayloads(
            string templateJson,
            FormCollection form)
        {
            Dictionary<Guid, object> payload =
                new Dictionary<Guid, object>();

            Dictionary<Guid, LabelValuePair> metaFields =
                new Dictionary<Guid, LabelValuePair>();

            List<BaseComponent> components = ParseJson(templateJson);

            List<int> blobs = new List<int>();

            foreach (string key in form.Keys)
            {
                if (Guid.TryParse(key, out Guid propertyId))
                {
                    BaseComponent component = components.First(e => e.Id == propertyId);

                    string[] values = form.GetValues(key);

                    object payloadValue = null;
                    object metaFieldValue = null;

                    switch (component.Type)
                    {
                        case ComponentType.file:
                            string[] splitFileNames = form.GetValues($"{key}-FileName");
                            string[] splitFileHashes = form.GetValues($"{key}-FileHash");

                            List<int> fileIds = new List<int>();
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(values[i]))
                                {
                                    int blobId = int.Parse(values[i]);

                                    blobs.Add(blobId);
                                    fileIds.Add(blobId);
                                }
                            }

                            string[] fileNames = splitFileNames
                                .Where(e => !string.IsNullOrEmpty(e))
                                .ToArray();

                            string[] fileHashes = splitFileHashes
                                .Where(e => !string.IsNullOrEmpty(e))
                                .ToArray();

                            if (fileNames.Any())
                            {
                                payloadValue = fileNames
                                    .Zip(fileHashes, (fn, fh) => new
                                    {
                                        FileName = fn,
                                        FileHash = fh
                                    })
                                    .Zip(fileIds, (ff, fids) => new
                                    {
                                        FileId = fids,
                                        ff.FileName,
                                        ff.FileHash,
                                    });

                                if (!component.IsEncrypted)
                                {
                                    metaFieldValue = fileNames
                                        .Zip(fileHashes, (fn, fh) => new
                                        {
                                            FileName = fn,
                                            FileHash = fh
                                        })
                                        .Zip(fileIds, (ff, fids) => new
                                        {
                                            FileId = fids,
                                            ff.FileName,
                                            ff.FileHash,
                                        });
                                }
                            }
                            break;
                        case ComponentType.checkbox:
                            bool.TryParse(values.FirstOrDefault(), out bool checkboxValue);

                            payloadValue = checkboxValue;

                            if (!component.IsEncrypted)
                            {
                                metaFieldValue = payloadValue;
                            }
                            break;
                        case ComponentType.datetime:
                        case ComponentType.hidden:
                        case ComponentType.select:
                        case ComponentType.textarea:
                        case ComponentType.textfield:
                            payloadValue = values.FirstOrDefault() ?? null;

                            if (!component.IsEncrypted)
                            {
                                metaFieldValue = payloadValue;
                            }
                            break;
                        case ComponentType.markdown:
                        default:
                            break;
                    }

                    if (payloadValue != null)
                    {
                        payload.Add(propertyId, payloadValue);
                    }

                    if (metaFieldValue != null)
                    {
                        metaFields.Add(
                            propertyId,
                            new LabelValuePair
                            {
                                Label = component.Label,
                                Value = metaFieldValue,
                            });
                    }
                }
            }

            string jsonMetaFields = JsonConvert.SerializeObject(metaFields);
            string jsonPayload = JsonConvert.SerializeObject(payload);

            return (jsonMetaFields, jsonPayload, blobs.ToArray());
        }

        /// <summary>
        /// Called when populating VM for message view/open
        /// </summary>
        /// <param name="messageJson">message values</param>
        /// <param name="templateJson">template json</param>
        /// <returns></returns>
        public static Dictionary<Guid, FieldObject> GetFields(
            string messageJson,
            string templateJson)
        {
            Dictionary<Guid, FieldObject> fields =
                new Dictionary<Guid, FieldObject>();

            Dictionary<Guid, object> valuesDict =
                JsonConvert.DeserializeObject<Dictionary<Guid, object>>(messageJson);

            List<BaseComponent> components = ParseJson(templateJson);

            foreach (BaseComponent component in components)
            {
                switch (component.Type)
                {
                    case ComponentType.checkbox:
                    case ComponentType.hidden:
                    case ComponentType.textfield:
                    case ComponentType.textarea:
                    case ComponentType.datetime:
                    case ComponentType.select:
                        if (valuesDict.ContainsKey(component.Id))
                        {
                            fields.Add(
                                component.Id,
                                new FieldObject(
                                    component.Label,
                                    component.Type,
                                    valuesDict[component.Id]));
                        }
                        else
                        {
                            fields.Add(
                                component.Id,
                                new FieldObject(
                                    component.Label,
                                    component.Type,
                                    string.Empty));
                        }

                        break;
                    case ComponentType.file:
                        if (valuesDict.ContainsKey(component.Id))
                        {
                            fields.Add(
                                component.Id,
                                new FieldObject(
                                    component.Label,
                                    component.Type,
                                    ((JArray)valuesDict[component.Id]).ToObject<FileObject[]>()));
                        }
                        else
                        {
                            fields.Add(
                                component.Id,
                                new FieldObject(
                                    component.Label,
                                    component.Type,
                                    Array.Empty<FileObject>()));
                        }

                        break;
                    case ComponentType.markdown:
                        MarkdownComponent markdownComponent = (MarkdownComponent)component;
                        fields.Add(
                            markdownComponent.Id,
                            new FieldObject(
                                markdownComponent.Label,
                                markdownComponent.Type,
                                markdownComponent.Value));

                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return fields;
        }

        /// <summary>
        /// Parsing template json to collection of classes
        /// </summary>
        /// <param name="json">template json</param>
        /// <returns></returns>
        private static List<BaseComponent> ParseJson(string json)
        {
            JArray arr = JArray.Parse(json);

            List<BaseComponent> result = new List<BaseComponent>();

            foreach (var item in arr.Children<JObject>())
            {
                var props = item.Properties();
                var type = props.Single(e => e.Name == "Type").Value.Value<string>();

                ComponentType typeEnum =
                    (ComponentType)Enum.Parse(typeof(ComponentType), type);

                switch (typeEnum)
                {
                    case ComponentType.hidden:
                        var hiddenComponent = item.ToObject<HiddenComponent>();
                        result.Add(hiddenComponent);

                        break;
                    case ComponentType.textfield:
                        var textFieldComponent = item.ToObject<TextFieldComponent>();
                        result.Add(textFieldComponent);

                        break;
                    case ComponentType.textarea:
                        var textAreaComponent = item.ToObject<TextAreaComponent>();
                        result.Add(textAreaComponent);

                        break;
                    case ComponentType.datetime:
                        var datetimeComponent = item.ToObject<DateTimeComponent>();
                        result.Add(datetimeComponent);

                        break;
                    case ComponentType.select:
                        var selectComponent = item.ToObject<SelectComponent>();
                        result.Add(selectComponent);

                        break;
                    case ComponentType.checkbox:
                        var checkboxComponent = item.ToObject<CheckboxComponent>();
                        result.Add(checkboxComponent);

                        break;
                    case ComponentType.file:
                        var fileComponent = item.ToObject<FileComponent>();
                        fileComponent.ForceInit();
                        result.Add(fileComponent);

                        break;
                    case ComponentType.markdown:
                        var markdownComponent = item.ToObject<MarkdownComponent>();
                        result.Add(markdownComponent);

                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return result;
        }
    }
}
