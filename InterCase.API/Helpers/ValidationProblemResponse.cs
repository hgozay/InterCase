using InterCase.Common.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InterCase.API.Helpers
{
    public static class ValidationProblemResponse
    {
        public static IActionResult Create(ActionContext context)
        {
            var modelState = new ModelStateDictionary();

            foreach (var (key, entry) in context.ModelState)
            {
                var field = ToFieldName(key);

                foreach (var error in entry.Errors)
                    modelState.AddModelError(field, string.IsNullOrEmpty(error.ErrorMessage) ? DescribeInvalidInput(field) : error.ErrorMessage);
            }

            var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problem = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, modelState);

            return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
        }

        private static string ToFieldName(string key)
        {
            var field = key.TrimStart('$').TrimStart('.');
            return string.IsNullOrEmpty(field)
                ? key
                : string.Join('.', field.Split('.').Select(ToCamelCase));
        }

        private static string ToCamelCase(string segment) =>
            segment.Length == 0 || char.IsLower(segment[0])
                ? segment
                : char.ToLowerInvariant(segment[0]) + segment[1..];

        private static string DescribeInvalidInput(string field) =>
            field.StartsWith('$') || string.IsNullOrEmpty(field)
                ? Localization.Get(Localization.Keys.InvalidRequestBody)
                : Localization.Format(Localization.Keys.InvalidValue, field);
    }
}
