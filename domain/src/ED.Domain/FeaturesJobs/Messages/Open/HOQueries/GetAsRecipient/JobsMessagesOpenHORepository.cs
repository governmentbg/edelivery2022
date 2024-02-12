using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static ED.Domain.IProfilesService;

namespace ED.Domain
{
    partial class JobsMessagesOpenHORepository : IJobsMessagesOpenHORepository
    {
        record AttachementDO(string FileName);

        public async Task<string> GetAsRecipientAsync(
            int messageId,
            int profileId,
            CancellationToken ct)
        {
            IJobsMessagesOpenQueryRepository.GetAsRecipientVO messageVO =
                await this.jobsMessagesOpenQueryRepository.GetAsRecipientAsync(
                    messageId,
                    profileId,
                    ct);

            ProfileKeyVO profileKey =
              await this.profilesService.GetProfileKeyAsync(
                  messageVO.RecipientProfileKeyId,
                  ct);

            Keystore.DecryptWithRsaKeyResponse decryptedKeyResp =
              await this.keystoreClient.DecryptWithRsaKeyAsync(
                  request: new Keystore.DecryptWithRsaKeyRequest
                  {
                      Key = new Keystore.RsaKey
                      {
                          Provider = profileKey.Provider,
                          KeyName = profileKey.KeyName,
                          OaepPadding = profileKey.OaepPadding,
                      },
                      EncryptedData = ByteString.CopyFrom(messageVO.RecipientEncryptedKey)
                  },
                  cancellationToken: ct);

            IEncryptor encryptor = this.encryptorFactory.CreateEncryptor(
                decryptedKeyResp.Plaintext.ToByteArray(),
                messageVO.IV);

            string decryptedBody =
                Encoding.UTF8.GetString(encryptor.Decrypt(messageVO.Body));

            IJobsMessagesOpenQueryRepository.GetTemplateContentVO template =
                await this.jobsMessagesOpenQueryRepository.GetTemplateContentAsync(
                    messageVO.TemplateId,
                    ct);

            Dictionary<Guid, object> valuesDictionary =
                JsonConvert.DeserializeObject<Dictionary<Guid, object>>(decryptedBody)!;

            IList<BaseComponent> templateComponents =
                TemplateSerializationHelper.DeserializeModel(template.Content);

            StringBuilder sb = new();

            sb.AppendLine(messageVO.SenderProfileName);
            sb.AppendLine(messageVO.Subject);
            sb.AppendLine(messageVO.DateSent.ToString());
            sb.AppendLine(messageVO.DateReceived.ToString());
            sb.AppendLine(messageVO.TemplateName);

            foreach (BaseComponent component in templateComponents)
            {
                object? value = null;
                if (valuesDictionary.ContainsKey(component.Id))
                {
                    value = valuesDictionary[component.Id];
                }

                switch (component.Type)
                {
                    case ComponentType.checkbox:
                        CheckboxComponent checkboxComponent = (CheckboxComponent)component;
                        bool boolValue = Convert.ToBoolean(value);

                        sb.AppendLine($"{component.Label} - {boolValue}");
                        break;
                    case ComponentType.file:
                        AttachementDO[] blobs = value != null
                            ? ((JArray)value).ToObject<AttachementDO[]>()
                                ?? Array.Empty<AttachementDO>()
                            : Array.Empty<AttachementDO>();

                        sb.AppendLine($"{component.Label} - {string.Join(",", blobs.Select(e => e.FileName))}");
                        break;
                    case ComponentType.hidden:
                        break;
                    case ComponentType.markdown:
                        break;
                    case ComponentType.select:
                    case ComponentType.datetime:
                    case ComponentType.textfield:
                    case ComponentType.textarea:
                        sb.AppendLine($"{component.Label} - {value?.ToString() ?? string.Empty}");
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return sb.ToString();
        }
    }
}
