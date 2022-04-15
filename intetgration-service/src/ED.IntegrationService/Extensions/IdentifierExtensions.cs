namespace ED.IntegrationService
{
    public class IdentifierExtensions
    {
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
    }
}
