using System.ComponentModel.DataAnnotations;

namespace InterCase.Common.Helpers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class NotEmptyGuidAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value) => value is not Guid guid || guid != Guid.Empty;
    }
}
