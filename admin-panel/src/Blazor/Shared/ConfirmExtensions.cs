using System;
using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;

#nullable enable

namespace ED.AdminPanel.Blazor.Shared
{
    public static class ConfirmExtensions
    {
        public static Task<bool> ShowConfirmModal(
            this IModalService modalService,
            string message)
        {
            return ShowConfirmModal(
                modalService,
                message,
                null,
                null,
                null);
        }

        public static Task<bool> ShowConfirmDangerModal(
            this IModalService modalService,
            string message)
        {
            return ShowConfirmModal(
                modalService,
                message,
                null,
                "warn",
                null);
        }

        public static async Task<bool> ShowConfirmModal(
            this IModalService modalService,
            string message,
            string? confirmButtonText,
            string? confirmButtonClass,
            string? cancelButtonText)
        {
            modalService = modalService ?? throw new ArgumentNullException(nameof(modalService));
            message = message ?? throw new ArgumentNullException(nameof(message));

            ModalParameters parameters = new();
            parameters.Add(nameof(ConfirmModal.Message), message);
            parameters.Add(nameof(ConfirmModal.ConfirmButtonText), confirmButtonText);
            parameters.Add(nameof(ConfirmModal.ConfirmButtonClass), confirmButtonClass);
            parameters.Add(nameof(ConfirmModal.CancelButtonText), cancelButtonText);

            var componentModal = modalService.Show<ConfirmModal>(string.Empty, parameters);
            ModalResult result = await componentModal.Result;

            return !result.Cancelled;
        }
    }
}
