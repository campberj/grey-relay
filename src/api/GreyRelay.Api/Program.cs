using System.Text.Json.Serialization;
using GreyRelay.Api.Endpoints;
using GreyRelay.Application;
using GreyRelay.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict);
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/api/health");
app.MapCampaignEndpoints();

await app.RunAsync().ConfigureAwait(false);

/// <summary>Entry point; public so integration tests can host the app in-process.</summary>
public partial class Program;
