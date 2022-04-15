﻿namespace ED.AdminPanel.Blazor.Pages.Templates.Components.Models
{
    public class SelectComponent : ValueComponent
    {
        public SelectComponent()
        {
            this.Type = ComponentType.select;
        }

        public string Placeholder { get; set; }

        public string Url { get; set; }

        public string Options { get; set; }
    }
}
