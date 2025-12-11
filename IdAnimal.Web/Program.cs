using IdAnimal.Web.Components;
using IdAnimal.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Bold.Licensing;

var builder = WebApplication.CreateBuilder(args);
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF1cXGFCf1FpRGRGfV5ycUVHYVZVTHxfRk0DNHVRdkdmWH1fcHZWQmhcVUByWUtWYEg=");
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add authentication and authorization
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<AuthStateProvider>());
builder.Services.AddAuthorization();
builder.Services.AddScoped<ReportGenerationService>();

// Add HTTP clients
builder.Services.AddHttpClient<AuthService>();
builder.Services.AddHttpClient<ApiClient>();

// Add application services
builder.Services.AddScoped<EstablishmentService>();
builder.Services.AddScoped<CattleService>();
builder.Services.AddScoped<CustomDataService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
