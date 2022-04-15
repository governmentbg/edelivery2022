using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace EDelivery.WebPortal.Utils
{
    public static class CsvHelper
    {
        private static readonly ILog logger = LogManager.GetLogger("CsvHelper");

        public static string GenerateCSV<T>(List<T> list, Dictionary<string, string> columns)
        {
            try
            {
                var propertyNames = columns.Keys;
                var columnTitles = columns.Values;

                StringBuilder sb = new StringBuilder();

                // Get column names
                string[] columnHeaders = columnTitles.ToArray();
                sb.AppendLine(string.Join(",", columnHeaders));

                //Loop through the collection, then the properties and add the values
                for (int i = 0; i <= list.Count - 1; i++)
                {
                    var columnCount = propertyNames.Count;
                    T item = list[i];

                    foreach (var prop in propertyNames)
                    {
                        var value = item.GetType().GetProperty(prop).GetValue(item, null);
                        if (value != null)
                        {
                            var valueAsString = CsvEcapeContent(value.ToString());
                            sb.Append(valueAsString);
                        }

                        //add ,
                        columnCount--;
                        if (columnCount > 0)
                        {
                            sb.Append(",");
                        }
                    }

                    sb.AppendLine();
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                logger.Error("Error in generateCsv", ex);
            }
            return string.Empty;
        }



        private static string CsvEcapeContent(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            //Check if the value contans a comma and place it in quotes if so
            if (input.Contains(","))
            {
                input = string.Concat("\"", input, "\"");
            }

            //Replace any \r or \n special characters from a new line with a space
            if (input.Contains("\r"))
            {
                input = input.Replace("\r", " ");
            }
            if (input.Contains("\n"))
            {
                input = input.Replace("\n", " ");
            }

            return System.Text.RegularExpressions.Regex.Replace(input, @"<(.|\n)*?>", string.Empty);
        }
    }
}