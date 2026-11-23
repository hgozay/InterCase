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
                foreach (var error in entry.Errors)
                    modelState.AddModelError(key, string.IsNullOrEmpty(error.ErrorMessage) ? DescribeInvalidInput(key) : error.ErrorMessage);
            }

            var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problem = problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, modelState);

            return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
        }

        private static string DescribeInvalidInput(string key)
        {
            var field = key.TrimStart('$').TrimStart('.');

            return string.IsNullOrEmpty(field)
                ? Localization.Get(Localization.Keys.InvalidRequestBody)
                : Localization.Format(Localization.Keys.InvalidValue, field);
        }
    }
}
