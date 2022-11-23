using ED.DomainServices.IntegrationService;

namespace ED.IntegrationService
{
    internal class CheckOperatorAccess
    {
        public CheckOperatorAccess(CheckProfileOperatorAccessResponse resp)
        {
            this.HasAccess = resp.HasAccess;
            this.OperatorLoginId = resp.OperatorLoginId;
        }

        public bool HasAccess { get; set; }

        public int? OperatorLoginId { get; set; }
    }
}
