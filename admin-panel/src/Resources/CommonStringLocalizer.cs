using System.Collections.Generic;
using Microsoft.Extensions.Localization;

#nullable enable

namespace ED.AdminPanel.Resources
{
    public class CommonStringLocalizer : IStringLocalizer
    {
        private readonly IStringLocalizer localizer;
        public CommonStringLocalizer(IStringLocalizerFactory factory)
        {
            this.localizer = factory.Create(typeof(CommonResources));
        }

        public LocalizedString this[string name]
            => this.localizer[name];

        public LocalizedString this[string name, params object[] arguments]
            => this.localizer[name, arguments];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => this.localizer.GetAllStrings(includeParentCultures);
    }
}
