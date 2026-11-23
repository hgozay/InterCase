using InterCase.Common;
using InterCase.Common.Helpers;

namespace InterCase.API.Helpers
{
    public static class ProblemDetailsCustomizer
    {
        public static void Customize(ProblemDetailsContext context)
        {
            var problem = context.ProblemDetails;
            problem.Extensions["correlationId"] = context.HttpContext.RequestServices
                .GetRequiredService<ICorrelationContext>().CorrelationId;

            var titleKey = problem is HttpValidationProblemDetails
                ? Localization.Keys.ValidationFailed
                : problem.Status switch
                {
                    StatusCodes.Status400BadRequest => Localization.Keys.BadRequest,
                    StatusCodes.Status401Unauthorized => Localization.Keys.Unauthorized,
                    StatusCodes.Status403Forbidden => Localization.Keys.Forbidden,
                    StatusCodes.Status404NotFound => Localization.Keys.ResourceNotFound,
                    StatusCodes.Status500InternalServerError => Localization.Keys.UnexpectedError,
                    _ => null
                };

            if (titleKey is not null)
                problem.Title = Localization.Get(titleKey);
        }
    }
}
