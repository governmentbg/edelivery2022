using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EDelivery.WebPortal.Utils
{
    public class TextHelper
    {
        public static readonly DateTime DefaultDate = SqlDateTime.MinValue.Value;
        public static readonly DateTime Date19000101 = new DateTime(1900, 1, 1);
        public static readonly string ParsingFormat = "yyyy-MM-dd";

        /// <summary>
        /// Get the BirthDate from Person EGN
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static DateTime GetBirthDateFromEGN(string egn)
        {
            egn = egn.Trim();
            if (egn.Length != 10 || (!IsEGN(egn) && IsLNCh(egn)))
            {
                return DefaultDate;
            }

            (int year, int month, int day) = GetDateComponents(egn);

            string date = GetDateInParsingFormat(year, month, day);

            bool isValidDate = DateTime.TryParseExact(
                date,
                ParsingFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt);
            if (isValidDate)
            {
                return dt;
            }

            return DefaultDate;
        }

        /// <summary>
        /// Checks is a personal identifier is lnc
        /// </summary>
        /// <param name="personalIdentifier"></param>
        /// <returns></returns>
        public static bool IsLNCh(string personalIdentifier)
        {
            if (string.IsNullOrEmpty(personalIdentifier)
                || personalIdentifier.Length != 10
                || !System.Text.RegularExpressions.Regex.IsMatch(personalIdentifier, "\\d{10}"))
            {
                return false;
            }

            var weights = new int[] { 21, 19, 17, 13, 11, 9, 7, 3, 1 };
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += weights[i] * (short.Parse(personalIdentifier[i].ToString()));
            }

            bool result =
                sum % 10 == short.Parse(personalIdentifier[9].ToString());

            return result;
        }

        /// <summary>
        /// Checks is a personal identifier is EGN
        /// </summary>
        /// <param name="personalIdentifier"></param>
        /// <returns></returns>
        public static bool IsEGN(string personalIdentifier)
        {
            if (string.IsNullOrEmpty(personalIdentifier)
                || personalIdentifier.Length != 10
                || !System.Text.RegularExpressions.Regex.IsMatch(personalIdentifier, "\\d{10}"))
            {
                return false;
            }

            (int year, int month, int day) = GetDateComponents(personalIdentifier);

            string date = GetDateInParsingFormat(year, month, day);

            bool isValidDate = DateTime.TryParseExact(
                date,
                ParsingFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
            if (!isValidDate)
            {
                return false;
            }

            var weights = new int[] { 2, 4, 8, 5, 10, 9, 7, 3, 6 };
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += weights[i] * (short.Parse(personalIdentifier[i].ToString()));
            }

            var controlPart = sum % 11;
            if (controlPart == 10)
            {
                controlPart = 0;
            }

            bool result =
                controlPart == short.Parse(personalIdentifier[9].ToString());

            return result;
        }

        internal static string[] ParseLatinNames(string latinNames)
        {
            if (string.IsNullOrEmpty(latinNames)) return null;

            var names = System.Text.RegularExpressions.Regex.Split(latinNames, "\\s+");
            for (int i = 0; i < names.Length; i++)
            {
                //if there are latin chars in the name, then convert them to cyr
                if (System.Text.RegularExpressions.Regex.IsMatch(names[i], latinCharsPattern))
                {
                    //convert to cyr
                    names[i] = ConvertToCyrilic(names[i]);
                }
            }

            return names;
        }

        private static string GetDateInParsingFormat(
            int year,
            int month,
            int day)
        {
            string date = $"{year}-{month.ToString().PadLeft(2, '0')}-{day.ToString().PadLeft(2, '0')}";

            return date;
        }

        private static (int, int, int) GetDateComponents(
            string personalIdentifier)
        {
            int year = int.Parse(personalIdentifier.Substring(0, 2));
            int month = int.Parse(personalIdentifier.Substring(2, 2));
            int day = int.Parse(personalIdentifier.Substring(4, 2));

            if (month >= 1 && month <= 12)
            {
                year += 1900;
            }

            if (month >= 21 && month <= 32)
            {
                year += 1800;
                month -= 20;
            }

            if (month >= 41 && month <= 52)
            {
                year += 2000;
                month -= 40;
            }

            return (year, month, day);
        }

        private static string latinCharsPattern = @"ya|yu|sht|sh|ch|ts|zh|[a-z]";

        private static string ConvertToCyrilic(string latinName)
        {
            latinName = latinName.Trim().ToLower();
            StringBuilder sb = new StringBuilder();
            string endChars = string.Empty;

            if (latinName.EndsWith("ia"))
            {
                endChars = "ия";
                latinName = latinName.Remove(latinName.Length - 3, 2);
            }

            System.Text.RegularExpressions.Regex.Replace(latinName, latinCharsPattern, m =>
            {
                sb.Append(GetCyrLetterFromLatin(m.Value));

                return string.Empty;
            });

            sb.Append(endChars);
            var firstLetter = sb[0].ToString().ToUpper();
            sb.Remove(0, 1);
            sb.Insert(0, firstLetter);

            return sb.ToString();
        }

        private static string GetCyrLetterFromLatin(string latinLetter)
        {
            switch (latinLetter)
            {
                case "a":
                    return "а";
                case "b":
                    return "б";
                case "v":
                    return "в";
                case "g":
                    return "г";
                case "d":
                    return "д";
                case "e":
                    return "е";
                case "zh":
                    return "ж";
                case "z":
                    return "з";
                case "i":
                    return "и";
                case "y":
                    return "й";
                case "k":
                    return "к";
                case "l":
                    return "л";
                case "m":
                    return "м";
                case "n":
                    return "н";
                case "o":
                    return "о";
                case "p":
                    return "п";
                case "r":
                    return "р";
                case "s":
                    return "с";
                case "t":
                    return "т";
                case "u":
                    return "у";
                case "f":
                    return "ф";
                case "h":
                    return "х";
                case "ts":
                    return "ц";
                case "ch":
                    return "ч";
                case "sh":
                    return "ш";
                case "sht":
                    return "щ";
                case "yu":
                    return "ю";
                case "ya":
                    return "я";
            }
            return latinLetter;
        }

        private static string dashCharacters =
            new string(
                new char[] {
                    '\u002D', // - : HYPHEN-MINUS {hyphen or minus sign}
                    '\u2010', // ‐ : HYPHEN
                    '\u2011', // ‑ : NON-BREAKING HYPHEN
                    '\u2012', // ‒ : FIGURE DASH
                    '\u2013', // – : EN DASH
                    '\u2014'  // — : EM DASH
                });

        public static string GetSignatureSubjectShortName(string signatureSubject)
        {
            if (signatureSubject == null)
            {
                return string.Empty;
            }

            var r = Regex.Match(
                signatureSubject,
                "CN=(?<name>[\\w\\s" + dashCharacters + "]+)",
                RegexOptions.IgnoreCase);
            if (r.Groups["name"].Success)
            {
                return r.Groups["name"].Value;
            }

            return signatureSubject.Substring(
                0,
                Math.Min(signatureSubject.Length, 20));
        }
    }
}