// Copyright (c) Arctium.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Arctium.SimpleRegistration.Components;
using Arctium.SimpleRegistration.Services;

using Microsoft.FluentUI.AspNetCore.Components;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(prefix: "ASR_SWEB_");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
//.AddInteractiveWebAssemblyComponents();

// FluentUI registrations.
builder.Services.AddHttpClient();
builder.Services.AddFluentUIComponents();

var registrationServiceName = builder.Configuration["RegistrationService"];

// Register the configured registration service.
var registrationServiceTypeName = $"Arctium.SimpleRegistration.Services.{registrationServiceName}";
var registrationServiceType = Type.GetType(registrationServiceTypeName, throwOnError: true);

// Own services.
builder.Services.AddTransient(typeof(IRegistrationService), registrationServiceType!);

// Register a default HttpClient for the RegistrationService if the used protocol is http(s).
IConfigurationSection registrationServiceSettings = builder.Configuration.GetSection(registrationServiceName!);
var registrationServiceProtocol = registrationServiceSettings?["Protocol"];

if (registrationServiceProtocol?.Equals("http", StringComparison.OrdinalIgnoreCase) == true ||
    registrationServiceProtocol?.Equals("https", StringComparison.OrdinalIgnoreCase) == true)
{
    _ = builder.Services.AddHttpClient(registrationServiceName!, client =>
    {
        var host = registrationServiceSettings?["HttpClientSettings:Host"];

        client.BaseAddress = new Uri($"{registrationServiceProtocol}://{host}");
    });
}

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
//.AddInteractiveWebAssemblyRenderMode();

app.Run();
