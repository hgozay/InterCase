using InterCase.Common;
using InterCase.Common.Helpers;
using InterCase.UI;
using InterCase.UI.Clients;
using InterCase.UI.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<ApiOptions>()
    .Bind(builder.Configuration.GetSection(ApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.Configure<DemoUserOptions>(builder.Configuration.GetSection(DemoUserOptions.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ApiRequestHandler>();
builder.Services.AddHttpClient<ApiClient>((services, client) =>
    {
        var options = services.GetRequiredService<IOptions<ApiOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { UseCookies = false })
    .AddHttpMessageHandler<ApiRequestHandler>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = UiCookieNames.Auth;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/login";
        options.SlidingExpiration = false;
    });
builder.Services.AddAuthorization();

builder.Services.Configure<RequestLocalizationOptions>(options => options
    .SetDefaultCulture(Localization.Turkish)
    .AddSupportedCultures(Localization.SupportedLanguages)
    .AddSupportedUICultures(Localization.SupportedLanguages));

builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/error");

app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseRequestLocalization();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
