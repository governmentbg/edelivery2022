using System;

namespace ED.IntegrationService
{
    public sealed class EGNValidator : IEGNValidator
    {
        public static System.Text.RegularExpressions.Regex regexEgn = new System.Text.RegularExpressions.Regex(@"^\d{10}$");
        private static readonly int[] weight = new int[9] { 2, 4, 8, 5, 10, 9, 7, 3, 6, };
        // private static readonly int controlDevideNumber = 11;

        /// <summary>
        /// Validate an egn
        /// </summary>
        /// <param name="egnString"></param>
        /// <returns></returns>
        public bool IsValidEGN(string egnString)
        {
            if (string.IsNullOrEmpty(egnString))
                return false;
            if (!regexEgn.IsMatch(egnString))
                return false;

            var sum = 0;
            for (int i = 0; i < 9; i++)
            {
                var num = Int32.Parse(egnString[i].ToString()) * weight[i];
                sum += num;
            }

            var control = sum % 11;
            if (control == 10)
                control = 0;

            return control == Int32.Parse(egnString[9].ToString());

        }
    }
}
