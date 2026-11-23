using InterCase.Common.Helpers;
using Microsoft.Extensions.Localization;

namespace InterCase.API.Helpers
{
    public sealed class LocalizationStringLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name] =>
            new(name, Localization.Get(name), resourceNotFound: !Localization.HasKey(name));

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var localized = this[name];
                return new LocalizedString(name, string.Format(localized.Value, arguments), localized.ResourceNotFound);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            Localization.GetAll(Localization.CurrentLanguage).Select(m => new LocalizedString(m.Key, m.Value));
    }
}
