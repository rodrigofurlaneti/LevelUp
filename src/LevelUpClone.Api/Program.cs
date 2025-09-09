using System.Text;
using System.Text.Json;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using LevelUpClone.Infrastructure.Security;
using static LevelUpClone.Infrastructure.Repositories.Postgres.UserServicePg;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    var config = builder.Configuration;

    // Serilog básico
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Controllers + Validation
    builder.Services.AddControllers();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

    // JWT
    // Swagger (uma única vez)
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(opt =>
    {
        opt.SwaggerDoc("v1", new OpenApiInfo { Title = "LevelUp API", Version = "v1" });
        opt.CustomSchemaIds(t => t.FullName);

        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Insira o token JWT como: Bearer {seu_token}",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        };
        opt.AddSecurityDefinition("Bearer", securityScheme);
        opt.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });
    });

    // CORS
    builder.Services.AddCors(opt =>
    {
        opt.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    });

    // ===== JWT =====
    var jwt = config.GetSection("Jwt");
    var issuer = jwt["Issuer"] ?? "LevelUp";
    var audience = jwt["Audience"] ?? "LevelUpClients";
    var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"] ?? "p3rsonalFinance!Portable-2025-StrongSecret-Key-DoNotShare!!");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

    // Conexão / Health
    var configuration = builder.Configuration;
    var pgConnString = configuration.GetConnectionString("Postgres")
                     ?? configuration["Database:PostGres"]
                     ?? throw new InvalidOperationException("Connection string Postgres não configurada.");

    builder.Services.AddSingleton(new PostgresConnectionFactory(pgConnString));
    builder.Services.AddSingleton<DbHealthChecker>();

    // Kestrel ouvindo na 5002 (atrás do Nginx)
    builder.WebHost.UseUrls("http://localhost:5002");

    // DI de Repositórios/Serviços/Handlers
    builder.Services.AddScoped<IUserService, UserServicePg>();
    builder.Services.AddTransient<IUserRepository, UserRepositoryPg>();
    builder.Services.AddTransient<IActivityRepository, ActivityRepositoryPg>();
    builder.Services.AddTransient<IActivityLogRepository, ActivityLogRepositoryPg>();
    builder.Services.AddTransient<IGeoClientLogRepository, GeoClientLogRepositoryPg>();

    builder.Services.AddSingleton<IDispatcher, CommandDispatcher>();
    builder.Services.AddTransient<UpsertUserHandler>();
    builder.Services.AddTransient<CreateActivityHandler>();
    builder.Services.AddTransient<LogFundamentalHandler>();
    builder.Services.AddTransient<LogCustomHandler>();
    builder.Services.AddTransient<GetDailyScoreHandler>();

    builder.Services.AddHttpContextAccessor();

    // Health checks: live (processo) e ready (Postgres)
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("OK"), tags: new[] { "live" })
        .AddNpgSql(connectionString: pgConnString, name: "postgres", tags: new[] { "ready" });

    var app = builder.Build();

    // Respeitar cabeçalhos do proxy (X-Forwarded-Proto/For)
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    // Middleware de exceções (GeoClientLoggingMiddleware)
    app.UseMiddleware<GeoClientLoggingMiddleware>();

    // Middleware de exceções (global)
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Swagger SEM condicionar por ambiente
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "LevelUp API v1");
    });

    app.UseSerilogRequestLogging();

    // Ordem recomendada
    // NÃO usar UseHttpsRedirection aqui (TLS termina no Nginx)
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    // Ping básico
    app.MapGet("/", () => Results.Ok(new { name = "FSI.PersonalFinancePortable.Api", status = "Up" }));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Falha fatal na inicialização do host");
}
finally
{
    Log.CloseAndFlush();
}