using ExpenseTracker.API.BackgroundServices;
using ExpenseTracker.API.Exceptions;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Application;
using ExpenseTracker.Infrastructure;
using ExpenseTracker.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Logging

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

#endregion

#region Services

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddControllers();

#region Authentication

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

#endregion

#region Swagger

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Expense Tracker API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token (without the 'Bearer' prefix)."
    });

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", doc),
            new List<string>()
        }
    });
});

#endregion

builder.Services.AddHostedService<ReminderBackgroundService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

builder.Services
    .AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!);

#endregion

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ExpenseTracker.Persistence.Context.ExpenseTrackerDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Expense Tracker API v1");
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

#region Middleware

app.UseExceptionHandler();


app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

#endregion

#region Endpoints

app.MapControllers();

app.MapHealthChecks("/health");

app.MapMethods("/ping", ["GET", "HEAD"], async (ExpenseTracker.Persistence.Context.ExpenseTrackerDbContext db) =>
{
    await db.Categories.AnyAsync();
    return Results.Ok();
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

#endregion

app.Run();
