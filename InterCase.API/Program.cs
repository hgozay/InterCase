using System.Text.Json.Serialization;
using InterCase.API.Helpers;
using InterCase.Common;
using InterCase.Common.Helpers;
using InterCase.Repository;
using InterCase.Repository.Context;
using InterCase.Repository.Interfaces;
using InterCase.Service;
using InterCase.Service.Factories;
using InterCase.Service.Interfaces;
using InterCase.Service.Managers;
using InterCase.Service.Mappings;
using InterCase.Service.Services.OtherServices;
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

builder.Services.AddOptions<CacheOptions>()
    .Bind(builder.Configuration.GetSection(CacheOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<QueueOptions>()
    .Bind(builder.Configuration.GetSection(QueueOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAutoMapper(typeof(MappingProfile));

// Ana servisler (CacheService, QueueService) geneldir; arkalarında çalışan manager appsettings'teki Cache:Provider ve Queue:Provider ayarından seçilir ve yalnızca seçilen manager ayağa kalkar.
// Bugün Redis ve RabbitMQ; yeni bir sağlayıcı kendi manager sınıfı ve anahtarıyla buraya kaydedilir, factory'ler ve servisler değişmez.
builder.Services.AddKeyedSingleton<ICacheManager, RedisCacheManager>(CacheProvider.Redis);
builder.Services.AddKeyedSingleton<IQueueManager, RabbitMqQueueManager>(QueueProvider.RabbitMq);
builder.Services.AddSingleton<ICacheManager>(CacheManagerFactory.Create);
builder.Services.AddSingleton<IQueueManager>(QueueManagerFactory.Create);

builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IQueueService, QueueService>();
builder.Services.AddScoped<ICorrelationContext, CorrelationContext>();
builder.Services.AddScoped<IServiceManager, ServiceManager>();

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
