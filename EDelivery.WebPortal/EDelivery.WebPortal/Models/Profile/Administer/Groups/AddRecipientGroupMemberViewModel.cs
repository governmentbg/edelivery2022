using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Models
{
    public class AddRecipientGroupMemberViewModel
    {
        public AddRecipientGroupMemberViewModel()
        {
        }

        public AddRecipientGroupMemberViewModel(int recipientGroupId)
        {
            this.RecipientGroupId = recipientGroupId;
        }

        public int RecipientGroupId { get; set; }

        public bool CanSendToIndividuals { get; set; }

        public bool CanSendToLegalEntities { get; set; }

        public List<SelectListItem> TargetGroups { get; set; }
    }

    public class AddRecipientGroupMemberIndividualViewModel
    {
        [RequiredRes]
        [RegularExpression(SystemConstants.CyrilicPattern, ErrorMessageResourceName = "InfoPersonalData", ErrorMessageResourceType = typeof(EDeliveryResources.Common))]
        public string FirstName { get; set; }

        [RequiredRes]
        [RegularExpression(SystemConstants.CyrilicPattern, ErrorMessageResourceName = "InfoPersonalData", ErrorMessageResourceType = typeof(EDeliveryResources.Common))]
        public string LastName { get; set; }

        [RequiredRes]
        public string Identifier { get; set; }

        public int SelectedIndividualProfileId { get; set; }

        public string SelectedIndividualProfileName { get; set; }
    }

    public class AddRecipientGroupMemberLegalEntityViewModel
    {
        [RequiredRes]
        public string CompanyRegistrationNumber { get; set; }

        public int SelectedLegalEntityProfileId { get; set; }

        public string SelectedLegalEntityProfileName { get; set; }
    }
}