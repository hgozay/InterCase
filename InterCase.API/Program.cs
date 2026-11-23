using System.Text.Json.Serialization;
using InterCase.API.Helpers;
using InterCase.Common.Helpers;
using InterCase.Repository;
using InterCase.Repository.Context;
using InterCase.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var postgresConnection = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres is not configured.");

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(postgresConnection)
    .UseSnakeCaseNamingConvention());
builder.Services.AddScoped<IRepositoryManager, RepositoryManager>();

builder.Services.Configure<RequestLocalizationOptions>(options => options
    .SetDefaultCulture(Localization.DefaultLanguage)
    .AddSupportedCultures(Localization.SupportedLanguages)
    .AddSupportedUICultures(Localization.SupportedLanguages));

builder.Services.AddControllers(options =>
    {
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

        var messages = options.ModelBindingMessageProvider;
        messages.SetAttemptedValueIsInvalidAccessor((_, field) => Localization.Format(Localization.Keys.InvalidValue, field));
        messages.SetUnknownValueIsInvalidAccessor(field => Localization.Format(Localization.Keys.InvalidValue, field));
        messages.SetValueMustNotBeNullAccessor(field => Localization.Format(Localization.Keys.InvalidValue, field));
        messages.SetValueIsInvalidAccessor(_ => Localization.Get(Localization.Keys.InvalidRequestBody));
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(_ => Localization.Get(Localization.Keys.InvalidRequestBody));
        messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => Localization.Get(Localization.Keys.InvalidRequestBody));
        messages.SetMissingRequestBodyRequiredValueAccessor(() => Localization.Get(Localization.Keys.InvalidRequestBody));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.AllowInputFormatterExceptionMessages = false;
    })
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (_, _) => new LocalizationStringLocalizer());
builder.Services.AddSingleton<IValidationAttributeAdapterProvider, LocalizedValidationAttributeAdapterProvider>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.InvalidModelStateResponseFactory = ValidationProblemResponse.Create);
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
