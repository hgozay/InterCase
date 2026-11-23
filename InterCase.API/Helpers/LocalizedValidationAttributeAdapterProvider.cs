using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace InterCase.API.Helpers
{
    public sealed class LocalizedValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider
    {
        private readonly ValidationAttributeAdapterProvider _defaultProvider = new();

        public IAttributeAdapter? GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer) =>
            _defaultProvider.GetAttributeAdapter(attribute, stringLocalizer)
            ?? new LocalizedAttributeAdapter(attribute, stringLocalizer);

        private sealed class LocalizedAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer)
            : AttributeAdapterBase<ValidationAttribute>(attribute, stringLocalizer)
        {
            public override void AddValidation(ClientModelValidationContext context)
            {
            }

            public override string GetErrorMessage(ModelValidationContextBase validationContext) =>
                GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
        }
    }
}
