using System.Globalization;
using InterCase.API.Handlers;
using InterCase.API.Helpers;
using InterCase.Common;
using InterCase.Common.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace InterCase.UnitTest.Api
{
    [TestClass]
    public sealed class ErrorResponseTests
    {
        private sealed class RecordingProblemDetailsService : IProblemDetailsService
        {
            public ProblemDetailsContext? Written { get; private set; }

            public ValueTask WriteAsync(ProblemDetailsContext context)
            {
                Written = context;
                return ValueTask.CompletedTask;
            }

            public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
            {
                Written = context;
                return ValueTask.FromResult(true);
            }
        }

        private static readonly Lazy<IServiceProvider> MvcServices = new(() => new ServiceCollection()
            .AddLogging()
            .AddMvcCore()
            .Services
            .BuildServiceProvider());

        private static void UseLanguage(string language)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(language);
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
        }

        private static ValidationProblemDetails ValidationProblemFor(params (string Key, string Message)[] errors)
        {
            var modelState = new ModelStateDictionary();
            foreach (var (key, message) in errors)
                modelState.AddModelError(key, message);

            var httpContext = new DefaultHttpContext { RequestServices = MvcServices.Value };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), modelState);

            var result = ValidationProblemResponse.Create(actionContext);

            var badRequest = (BadRequestObjectResult)result;
            Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
            Assert.Contains("application/problem+json", badRequest.ContentTypes);
            return (ValidationProblemDetails)badRequest.Value!;
        }

        [TestMethod]
        public void ValidationProblem_QueryKeysAreCamelCasedAndEmptyMessagesAreFilled()
        {
            UseLanguage(Localization.English);

            var problem = ValidationProblemFor(("Page", string.Empty), ("PageSize", "pageSize must be between 1 and 100."));

            CollectionAssert.AreEquivalent(new[] { "page", "pageSize" }, problem.Errors.Keys.ToArray());
            Assert.AreEqual("page has an invalid value.", Assert.ContainsSingle(problem.Errors["page"]));
            Assert.AreEqual("pageSize must be between 1 and 100.", Assert.ContainsSingle(problem.Errors["pageSize"]));
        }

        [TestMethod]
        public void ValidationProblem_JsonPathKeysLoseTheirPrefix()
        {
            UseLanguage(Localization.English);

            var problem = ValidationProblemFor(("$.quantity", string.Empty), ("Items[0].ProductId", string.Empty));

            CollectionAssert.AreEquivalent(new[] { "quantity", "items[0].productId" }, problem.Errors.Keys.ToArray());
            Assert.AreEqual("quantity has an invalid value.", Assert.ContainsSingle(problem.Errors["quantity"]));
        }

        [TestMethod]
        public void ValidationProblem_WhenTheBodyItselfIsInvalid_UsesTheRequestBodyMessage()
        {
            UseLanguage(Localization.English);

            var problem = ValidationProblemFor(("$", string.Empty));

            Assert.AreEqual(Localization.Get(Localization.Keys.InvalidRequestBody, Localization.English), Assert.ContainsSingle(problem.Errors["$"]));
        }

        [TestMethod]
        public void ValidationProblem_UsesTheRequestLanguage()
        {
            UseLanguage(Localization.Turkish);

            var problem = ValidationProblemFor(("Page", string.Empty));

            Assert.AreEqual("page için geçersiz bir değer gönderildi.", Assert.ContainsSingle(problem.Errors["page"]));
        }

        private static async Task<(DefaultHttpContext HttpContext, ProblemDetails? Problem, bool Handled)> HandleAsync(Exception exception, bool clientAborted = false)
        {
            var writer = new RecordingProblemDetailsService();
            var handler = new GlobalExceptionHandler(writer, NullLogger<GlobalExceptionHandler>.Instance);
            var httpContext = new DefaultHttpContext();
            if (clientAborted)
                httpContext.RequestAborted = new CancellationToken(canceled: true);

            var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);
            return (httpContext, writer.Written?.ProblemDetails, handled);
        }

        [TestMethod]
        public async Task ExceptionHandler_NotFound_Returns404()
        {
            var (httpContext, problem, handled) = await HandleAsync(new NotFoundException("Order x was not found."));

            Assert.IsTrue(handled);
            Assert.AreEqual(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
            Assert.AreEqual(StatusCodes.Status404NotFound, problem!.Status);
            Assert.IsNull(problem.Detail);
        }

        [TestMethod]
        public async Task ExceptionHandler_Conflict_Returns409WithLocalizedDetail()
        {
            UseLanguage(Localization.Turkish);

            var (httpContext, problem, _) = await HandleAsync(new ConflictException(Localization.Keys.OrderNotPending));

            Assert.AreEqual(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
            Assert.AreEqual(Localization.Get(Localization.Keys.OrderNotPending, Localization.Turkish), problem!.Detail);
        }

        [TestMethod]
        public async Task ExceptionHandler_BadHttpRequest_Returns400WithRequestBodyMessage()
        {
            UseLanguage(Localization.English);

            var (httpContext, problem, _) = await HandleAsync(new BadHttpRequestException("Unexpected end of request content."));

            Assert.AreEqual(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
            Assert.AreEqual(Localization.Get(Localization.Keys.InvalidRequestBody, Localization.English), problem!.Detail);
        }

        [TestMethod]
        public async Task ExceptionHandler_UnexpectedException_Returns500WithoutInternalDetails()
        {
            var (httpContext, problem, _) = await HandleAsync(new InvalidOperationException("connection string: secret"));

            Assert.AreEqual(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
            Assert.IsNull(problem!.Detail);
        }

        [TestMethod]
        public async Task ExceptionHandler_WhenClientDisconnected_Returns499WithoutBody()
        {
            var (httpContext, problem, handled) = await HandleAsync(new OperationCanceledException(), clientAborted: true);

            Assert.IsTrue(handled);
            Assert.AreEqual(StatusCodes.Status499ClientClosedRequest, httpContext.Response.StatusCode);
            Assert.IsNull(problem);
        }

        [TestMethod]
        public async Task ExceptionHandler_CancellationWhileClientIsConnected_Returns500()
        {
            var (httpContext, _, _) = await HandleAsync(new OperationCanceledException());

            Assert.AreEqual(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        }

        private static ProblemDetailsContext ProblemContext(ProblemDetails problem)
        {
            var services = new ServiceCollection()
                .AddSingleton<ICorrelationContext>(new CorrelationContext { CorrelationId = "corr-42" })
                .BuildServiceProvider();
            return new ProblemDetailsContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services },
                ProblemDetails = problem
            };
        }

        [TestMethod]
        [DataRow(StatusCodes.Status400BadRequest, Localization.Keys.BadRequest)]
        [DataRow(StatusCodes.Status401Unauthorized, Localization.Keys.Unauthorized)]
        [DataRow(StatusCodes.Status403Forbidden, Localization.Keys.Forbidden)]
        [DataRow(StatusCodes.Status404NotFound, Localization.Keys.ResourceNotFound)]
        [DataRow(StatusCodes.Status409Conflict, Localization.Keys.Conflict)]
        [DataRow(StatusCodes.Status500InternalServerError, Localization.Keys.UnexpectedError)]
        public void ProblemDetailsCustomizer_SetsLocalizedTitleAndCorrelationId(int status, string titleKey)
        {
            UseLanguage(Localization.Turkish);
            var context = ProblemContext(new ProblemDetails { Status = status });

            ProblemDetailsCustomizer.Customize(context);

            Assert.AreEqual(Localization.Get(titleKey, Localization.Turkish), context.ProblemDetails.Title);
            Assert.AreEqual("corr-42", context.ProblemDetails.Extensions["correlationId"]);
        }

        [TestMethod]
        public void ProblemDetailsCustomizer_ValidationProblemGetsValidationTitle()
        {
            UseLanguage(Localization.English);
            var context = ProblemContext(new HttpValidationProblemDetails { Status = StatusCodes.Status400BadRequest });

            ProblemDetailsCustomizer.Customize(context);

            Assert.AreEqual(Localization.Get(Localization.Keys.ValidationFailed, Localization.English), context.ProblemDetails.Title);
        }

        [TestMethod]
        public void ProblemDetailsCustomizer_UnknownStatusKeepsItsTitle()
        {
            var context = ProblemContext(new ProblemDetails { Status = StatusCodes.Status418ImATeapot, Title = "teapot" });

            ProblemDetailsCustomizer.Customize(context);

            Assert.AreEqual("teapot", context.ProblemDetails.Title);
            Assert.AreEqual("corr-42", context.ProblemDetails.Extensions["correlationId"]);
        }
    }
}
