using System.Data;
using CartServicePoc;
using CartServicePoc.Api.Middleware;
using CartServicePoc.Api.Validators;
using CartServicePoc.Application.Interfaces;
using CartServicePoc.Infrastructure.Cache;
using CartServicePoc.Infrastructure.Database;
using CartServicePoc.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

// SQL konekcija
builder.Services.AddTransient<IDbConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories i Services
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartServicePoc.Application.Services.CartService>();

// Controllers i Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Validators
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<AddCartItemRequestValidator>();

// Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "CartService:";
});

builder.Services.AddSingleton<ICacheService, CacheService>();


var app = builder.Build();

//Middleware za globalno hvatanje grešaka
app.UseMiddleware<ErrorHandlingMiddleware>();

// Migracije
DbMigrator.MigrateDatabase(
    builder.Configuration.GetConnectionString("DefaultConnection")!);


app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();