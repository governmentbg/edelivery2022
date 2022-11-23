namespace ED.IntegrationService
{
    internal class AuthenticationInfo
    {
        public AuthenticationInfo(int profileId, int loginId, int? operatorLoginId)
        {
            this.ProfileId = profileId;
            this.LoginId = loginId;
            this.OperatorLoginId = operatorLoginId;
        }

        public int ProfileId { get; set; }

        public int LoginId { get; set; }

        public int? OperatorLoginId { get; set; }
    }
}
