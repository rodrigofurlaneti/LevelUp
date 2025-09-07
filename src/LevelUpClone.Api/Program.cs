using LevelUpClone.Application.Abstractions;
using LevelUpClone.Application.Cqrs.Activities;
using LevelUpClone.Application.Cqrs.Logs;
using LevelUpClone.Application.Cqrs.Scores;
using LevelUpClone.Application.Cqrs.Users;
using LevelUpClone.Infrastructure.Persistence;
using LevelUpClone.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog (básico)
builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console();
});

// CORS (ajuste os origins conforme seu front)
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p =>
        p.WithOrigins(
            "http://localhost:5000", 
            "http://localhost:5173", 
            "http://localhost:5282", 
            "https://localhost:5282")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
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

// === Infra: DI correto ===
var dbSection = builder.Configuration.GetSection("Database");
var pgConnString = dbSection["Postgres"];

if (string.IsNullOrWhiteSpace(pgConnString))
    throw new InvalidOperationException("Database:Postgres connection string not found. Check appsettings.json.");

builder.Services.AddSingleton(new PostgresConnectionFactory(pgConnString));
builder.Services.AddSingleton<ConnectionFactory>(sp =>
    new ConnectionFactory(sp.GetRequiredService<PostgresConnectionFactory>()));

builder.Services.AddTransient<LevelUpClone.Application.Abstractions.IUserService, UserServicePg>();

// Repositórios concretos (se realmente precisa das versões “genéricas”)
builder.Services.AddTransient<UserRepository>();
builder.Services.AddTransient<ActivityRepository>();
builder.Services.AddTransient<ActivityLogRepository>();

// Bindings de domínio → implementações Postgres
builder.Services.AddTransient<LevelUpClone.Domain.Interfaces.IUserRepository, UserRepositoryPg>();
builder.Services.AddTransient<LevelUpClone.Domain.Interfaces.IActivityRepository, ActivityRepositoryPg>();
builder.Services.AddTransient<LevelUpClone.Domain.Interfaces.IActivityLogRepository, ActivityLogRepositoryPg>();

// CQRS
builder.Services.AddSingleton<IDispatcher, CommandDispatcher>();
builder.Services.AddTransient<UpsertUserHandler>();
builder.Services.AddTransient<CreateActivityHandler>();
builder.Services.AddTransient<LogFundamentalHandler>();
builder.Services.AddTransient<LogCustomHandler>();
builder.Services.AddTransient<GetDailyScoreHandler>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseSerilogRequestLogging();
app.UseCors();                 
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
