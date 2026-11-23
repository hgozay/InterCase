using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.PostgreSql;
using InterCase.API.Handlers;
using InterCase.API.Helpers;
using InterCase.API.Middlewares;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, configuration) => configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    var postgresConnection = builder.Configuration.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException("ConnectionStrings:Postgres is not configured.");

    builder.Services.AddDbContext<AppDbContext>(options => options
        .UseNpgsql(postgresConnection)
        .UseSnakeCaseNamingConvention());
    builder.Services.AddScoped<IRepositoryManager, RepositoryManager>();

    builder.Services.AddOptions<JwtOptions>()
        .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();
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
    builder.Services.AddOptions<ProcessingOptions>()
        .Bind(builder.Configuration.GetSection(ProcessingOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services.AddSingleton(TimeProvider.System);
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

    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(
            options => options.UseNpgsqlConnection(postgresConnection),
            new PostgreSqlStorageOptions
            {
                SchemaName = "hangfire",
                QueuePollInterval = TimeSpan.FromSeconds(1)
            }));

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
    builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
        .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
        {
            bearer.MapInboundClaims = false;
            bearer.TokenValidationParameters = JwtTokenHelper.CreateValidationParameters(jwt.Value);
        });
    builder.Services.AddAuthorization();

    builder.Services.Configure<RequestLocalizationOptions>(options => options
        .SetDefaultCulture(Localization.DefaultLanguage)
        .AddSupportedCultures(Localization.SupportedLanguages)
        .AddSupportedUICultures(Localization.SupportedLanguages));

    builder.Services.AddControllers(options =>
        {
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());

            var messages = options.ModelBindingMessageProvider;
            messages.SetAttemptedValueIsInvalidAccessor((_, field) => Localization.Format(Localization.Keys.InvalidValue, field));
            messages.SetUnknownValueIsInvalidAccessor(field => Localization.Format(Localization.Keys.InvalidValue, field));
            messages.SetValueMustNotBeNullAccessor(field => Localization.Format(Localization.Keys.InvalidValue, field));
            messages.SetValueIsInvalidAccessor(_ => string.Empty);
            messages.SetNonPropertyAttemptedValueIsInvalidAccessor(_ => string.Empty);
            messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => string.Empty);
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

    builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsCustomizer.Customize);
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "InterCase API", Version = "v1" });
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "POST /auth/token ile alınan token'ı Bearer öneki olmadan yapıştırın."
        });
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
    });

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseRequestLocalization();
    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHangfireDashboard("/hangfire");

    if (app.Environment.IsDevelopment())
    {
        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "InterCase.API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
