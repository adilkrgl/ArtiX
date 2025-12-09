using ArtiX.WebAdmin.Services.Api;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<ApiAuthenticationDelegatingHandler>();

builder.Services.AddHttpClient("ApiClient", (sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration.GetValue<string>("ApiBaseUrl");
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
}).AddHttpMessageHandler<ApiAuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<ICompanyApiClient, CompanyApiClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration.GetValue<string>("ApiBaseUrl");
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
}).AddHttpMessageHandler<ApiAuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<ICustomerApiClient, CustomerApiClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration.GetValue<string>("ApiBaseUrl");
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
}).AddHttpMessageHandler<ApiAuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IInvoiceApiClient, InvoiceApiClient>((sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var baseUrl = configuration.GetValue<string>("ApiBaseUrl");
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
}).AddHttpMessageHandler<ApiAuthenticationDelegatingHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
