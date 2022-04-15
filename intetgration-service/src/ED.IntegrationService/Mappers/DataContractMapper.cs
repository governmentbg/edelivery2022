using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using EDelivery.Common.Enums;

namespace ED.IntegrationService
{
    public partial class DataContractMapper
    {
        public static eProfileType ToeProfileType(int targetGroupId)
        {
            switch (targetGroupId)
            {
                case 1:
                    return eProfileType.Person;
                case 2:
                    return eProfileType.LegalPerson;
                case 3:
                case 4:
                    return eProfileType.Institution;
                default:
                    throw new ArgumentException("Unsupported target group");
            }
        }

        public static eInstitutionType? ToeInstitutionType(int targetGroupId)
        {
            switch (targetGroupId)
            {
                case 3:
                    return eInstitutionType.StateAdministraation;
                case 4:
                    return eInstitutionType.SocialOrganisations;
                default:
                    return null;
            }
        }

        public static int ToTargetGroupId(eProfileType profileType)
        {
            switch (profileType)
            {
                case eProfileType.Person:
                    return 1;
                case eProfileType.LegalPerson:
                    return 2;
                case eProfileType.Institution:
                    return 3;
                case eProfileType.Administrator:
                default:
                    throw new ArgumentException("Unsupported eProfileType");
            }
        }

        private static string ExtractEgnFromSubjectOrExtensions(
            string subject,
            X509ExtensionCollection col)
        {
            var egn = ExtractEgnFromSubject(subject);
            if (egn == null)
            {
                //check extensions
                foreach (var ex in col)
                {
                    egn = ExtractEgnFromSubject(ex.Format(false));
                    if (!string.IsNullOrEmpty(egn))
                    {
                        break;
                    }
                }
            }
            return egn;
        }

        private static readonly Regex egnExtract =
            new Regex(@"(EGN:)?s?(?<egn>\d{10})\b", RegexOptions.IgnoreCase);

        private static string ExtractEgnFromSubject(string subjectName)
        {
            var egnValidator = new EGNValidator();
            var matchs = egnExtract.Matches(subjectName);
            Match matchEgn = null;
            if (matchs.Count > 1)
            {
                //what should we do
                for (int i = 0; i < matchs.Count; i++)
                {
                    if (egnValidator.IsValidEGN(matchs[i].Value))
                    {
                        matchEgn = matchs[i];
                        break;
                    }
                }
            }
            else if (matchs.Count == 1)
            {
                matchEgn = matchs[0];
            }

            if (matchEgn != null)
            {
                var egn = matchEgn.Groups["egn"].Value;
                if (egnValidator.IsValidEGN(egn))
                    return egn;
            }

            return null;
        }
    }
}
