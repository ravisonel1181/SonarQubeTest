using Microsoft.Extensions.Configuration;
using NEHGSWebApiCore.Api.Middleware;
using NEHGSWebApiCore.Application.Common.Mappings;
using NEHGSWebApiCore.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Register All Services
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// use middleware
app.UseApplicationMiddleware();

app.Run();