using System.Globalization;
using Hangfire;
using Hangfire.PostgreSql;
using InterCase.Common;
using InterCase.Repository;
using InterCase.Repository.Context;
using InterCase.Repository.Interfaces;
using InterCase.Service;
using InterCase.Service.Consumers;
using InterCase.Service.Factories;
using InterCase.Service.Interfaces;
using InterCase.Service.Jobs;
using InterCase.Service.Managers;
using InterCase.Service.Mappings;
using InterCase.Service.Services.OtherServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

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
    builder.Services.AddOptions<HangfireOptions>()
        .Bind(builder.Configuration.GetSection(HangfireOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddAutoMapper(typeof(MappingProfile));

    builder.Services.AddKeyedSingleton<ICacheManager, RedisCacheManager>(CacheProvider.Redis);
    builder.Services.AddKeyedSingleton<IQueueManager, RabbitMqQueueManager>(QueueProvider.RabbitMq);
    builder.Services.AddSingleton<ICacheManager>(CacheManagerFactory.Create);
    builder.Services.AddSingleton<IQueueManager>(QueueManagerFactory.Create);

    builder.Services.AddSingleton<ICacheService, CacheService>();
    builder.Services.AddSingleton<IQueueService, QueueService>();
    builder.Services.AddScoped<ICorrelationContext, CorrelationContext>();
    builder.Services.AddScoped<IServiceManager, ServiceManager>();

    builder.Services.AddScoped<IOrderProcessingJob, OrderProcessingJob>();
    builder.Services.AddScoped<IRepublishStaleOrdersJob, RepublishStaleOrdersJob>();

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
    builder.Services.AddHangfireServer((services, options) =>
    {
        options.WorkerCount = services.GetRequiredService<IOptions<HangfireOptions>>().Value.WorkerCount;
        options.ServerName = "intercase-worker";
    });

    builder.Services.AddHostedService<OrderPlacedConsumer>();

    var host = builder.Build();

    host.Services.GetRequiredService<IRecurringJobManager>().AddOrUpdate<IRepublishStaleOrdersJob>(
        RecurringJobIds.RepublishStaleOrders,
        job => job.RunAsync(CancellationToken.None),
        Cron.Minutely());

    await host.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "InterCase.Worker terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
