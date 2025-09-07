using LevelUpClone.Application.Abstractions;
using LevelUpClone.Application.Cqrs.Activities;
using LevelUpClone.Application.Cqrs.Logs;
using LevelUpClone.Application.Cqrs.Scores;
using LevelUpClone.Application.Cqrs.Users;
using LevelUpClone.Infrastructure.Persistence;
using LevelUpClone.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog (basic)
builder.Host.UseSerilog((ctx, cfg) => cfg.WriteTo.Console());

// JWT
var jwt = builder.Configuration.GetSection("Jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"], ValidAudience = jwt["Audience"], IssuerSigningKey = key
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infra
var cs = builder.Configuration.GetConnectionString("SqlServer")!;
builder.Services.AddSingleton(new ConnectionFactory(cs));
builder.Services.AddTransient<UserRepository>();
builder.Services.AddTransient<ActivityRepository>();
builder.Services.AddTransient<ActivityLogRepository>();

// Domain interfaces bindings
builder.Services.AddTransient<LevelUpClone.Domain.Interfaces.IUserRepository, UserRepository>();
builder.Services.AddTransient<LevelUpClone.Domain.Interfaces.IActivityRepository, ActivityRepository>();
builder.Services.AddTransient<LevelUpClone.Domain.Interfaces.IActivityLogRepository, ActivityLogRepository>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
