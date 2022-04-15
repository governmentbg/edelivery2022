using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace ED.AdminPanel.Blazor.Shared
{
    public class ServerSideValidator : ComponentBase
    {
        private ValidationMessageStore messageStore;

        [CascadingParameter]
        private EditContext CurrentEditContext { get; set; }

        protected override void OnInitialized()
        {
            if (this.CurrentEditContext == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ServerSideValidator)} requires a cascading " +
                    $"parameter of type {nameof(EditContext)}. " +
                    $"For example, you can use {nameof(ServerSideValidator)} " +
                    $"inside an {nameof(EditForm)}.");
            }

            this.messageStore = new(this.CurrentEditContext);

            this.CurrentEditContext.OnValidationRequested += (s, e) =>
                this.messageStore.Clear();
            this.CurrentEditContext.OnFieldChanged += (s, e) =>
                this.messageStore.Clear(e.FieldIdentifier);
        }

        public void DisplayErrors(Dictionary<string, List<string>> errors)
        {
            foreach (var err in errors)
            {
                this.messageStore.Add(
                    this.CurrentEditContext.Field(err.Key), err.Value);
            }

            this.CurrentEditContext.NotifyValidationStateChanged();
        }

        public void ClearErrors()
        {
            this.messageStore.Clear();
            this.CurrentEditContext.NotifyValidationStateChanged();
        }
    }
}
