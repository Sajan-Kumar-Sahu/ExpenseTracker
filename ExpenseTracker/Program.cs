using ExpenseTracker.API.Exceptions;
using ExpenseTracker.Application;
using ExpenseTracker.Persistence;
using Serilog;

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

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

builder.Services
    .AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!);

#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Expense Tracker API");
    });
}
#region Middleware

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseAuthorization();

#endregion

#region Endpoints

app.MapControllers();

app.MapHealthChecks("/health");

#endregion

app.Run();