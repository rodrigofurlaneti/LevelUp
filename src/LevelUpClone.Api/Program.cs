using LevelUpClone.Api.Middlewares;
using LevelUpClone.Application.Abstractions;
using LevelUpClone.Application.Cqrs.Activities;
using LevelUpClone.Application.Cqrs.Logs;
using LevelUpClone.Application.Cqrs.Scores;
using LevelUpClone.Application.Cqrs.Users;
using LevelUpClone.Domain.Interfaces;
using LevelUpClone.Infrastructure.Diagnostics;
using LevelUpClone.Infrastructure.Persistence;
using LevelUpClone.Infrastructure.Repositories.Postgres;
using LevelUpClone.Infrastructure.Repositories.SqlServer;
using LevelUpClone.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using LevelUpClone.Infrastructure.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using static LevelUpClone.Infrastructure.Repositories.Postgres.UserServicePg;

var builder = WebApplication.CreateBuilder(args);

// Serilog (básico)
builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console();
});

// CORS ABERTO (qualquer origem, sem credenciais)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .WithHeaders("Content-Type", "X-Correlation-Id") // <- libere o header custom
              .AllowAnyMethod());
});

// JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;
var getConnectionString = configuration.GetConnectionString("Postgres")
?? configuration["Database:PostGres"] ?? throw new InvalidOperationException("Connection string Postgres não configurada.");

builder.Services.AddSingleton(new PostgresConnectionFactory(getConnectionString));
builder.Services.AddSingleton<DbHealthChecker>();
builder.Services.AddScoped<IUserService, UserServicePg>();
builder.Services.AddTransient<IUserRepository, UserRepositoryPg>();
builder.Services.AddTransient<IActivityRepository, ActivityRepositoryPg>();
builder.Services.AddTransient<IActivityLogRepository, ActivityLogRepositoryPg>();
builder.Services.AddSingleton<IDispatcher, CommandDispatcher>();
builder.Services.AddTransient<UpsertUserHandler>();
builder.Services.AddTransient<CreateActivityHandler>();
builder.Services.AddTransient<LogFundamentalHandler>();
builder.Services.AddTransient<LogCustomHandler>();
builder.Services.AddTransient<GetDailyScoreHandler>();
builder.Services.AddTransient<IGeoClientLogRepository, GeoClientLogRepositoryPg>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: getConnectionString,
        name: "postgres",
        tags: new[] { "ready" });

var app = builder.Build();
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.TryGetValue("X-Correlation-Id", out var cid) || string.IsNullOrWhiteSpace(cid))
    {
        cid = Guid.NewGuid().ToString("D");
        context.Request.Headers["X-Correlation-Id"] = cid;
    }

    // devolve no response (útil para troubleshooting no front / Postman)
    context.Response.Headers["X-Correlation-Id"] = cid.ToString();

    await next();
});
app.MapHealthChecks("/health/ready"); 
app.UseMiddleware<GeoClientLoggingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(); 
app.MapControllers(); 
app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.Ok()).RequireCors(); 
app.MapGet("/", () => Results.Ok(new { name = "LevelUpClone.Api", status = "Up" }));
app.Run();
