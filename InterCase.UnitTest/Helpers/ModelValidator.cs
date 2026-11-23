using System.ComponentModel.DataAnnotations;

namespace InterCase.UnitTest.Helpers
{
    public static class ModelValidator
    {
        public static IReadOnlyList<string> Validate(object model)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
            return results.Select(r => r.ErrorMessage ?? string.Empty).ToList();
        }
    }
}
