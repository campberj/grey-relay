using System.Text.Json.Serialization;
using GreyRelay.Api.Endpoints;
using GreyRelay.Api.Logging;
using GreyRelay.Application;
using GreyRelay.Infrastructure;
using GreyRelay.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var databasePath = DatabasePathResolver.Resolve(
    builder.Configuration["Storage:DatabasePath"],
    builder.Environment.ContentRootPath);

builder.Services
    .AddApplication()
    .AddInfrastructure(databasePath);
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);
builder.Services.AddHealthChecks()
    .AddDbContextCheck<GreyRelayDbContext>("database");
builder.Services.AddOpenApi();

var app = builder.Build();

app.Logger.CampaignDatabase(databasePath);
await app.Services.InitializeDatabaseAsync(app.Lifetime.ApplicationStopping).ConfigureAwait(false);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/api/health");
app.MapCampaignEndpoints();

await app.RunAsync().ConfigureAwait(false);

/// <summary>Entry point; public so integration tests can host the app in-process.</summary>
public partial class Program;
