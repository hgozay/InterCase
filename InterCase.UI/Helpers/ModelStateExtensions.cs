using InterCase.UI.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InterCase.UI.Helpers
{
    public static class ModelStateExtensions
    {
        public static void AddApiErrors(this ModelStateDictionary modelState, ApiProblem? problem, string prefix, params string[] fields)
        {
            if (problem?.Errors is null)
                return;

            foreach (var (key, messages) in problem.Errors)
            {
                var field = fields.FirstOrDefault(f => string.Equals(f, key, StringComparison.OrdinalIgnoreCase));
                var modelKey = field is null ? string.Empty : string.IsNullOrEmpty(prefix) ? field : $"{prefix}.{field}";

                foreach (var message in messages)
                    modelState.AddModelError(modelKey, message);
            }
        }
    }
}
