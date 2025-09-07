﻿using LevelUpClone.Api.Middlewares;
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

builder.Services.AddCors(o =>
{
    o.AddPolicy("Open", p => p
        .AllowAnyOrigin()      // ou SetIsOriginAllowed(_ => true)+AllowCredentials()
        .AllowAnyHeader()
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
app.MapHealthChecks("/health/ready"); 
app.UseMiddleware<GeoClientLoggingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("Open");            // <- tem que vir ANTES dos endpoints
app.MapControllers().RequireCors("Open");
app.UseAuthentication();
app.UseAuthorization();
app.MapMethods("{*path}", new[] { "OPTIONS" }, () => Results.Ok())
   .RequireCors("Open");

app.Run();
