using System.Text.Json;
using Asp.Versioning;
using BlocoNaRua.Data.Extensions;
using BlocoNaRua.Services.Implementations;
using BlocoNaRua.Services.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Configuration.AddEnvironmentVariables();

IConfiguration configuration = builder.Configuration;

// Validate required environment variables
if (string.IsNullOrEmpty(configuration.GetConnectionString("DefaultConnection")))
{
    throw new InvalidOperationException("DefaultConnection string is not configured");
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEntityFramework(configuration);
builder.Services.AddRepositories();

// Register application services
builder.Services.AddScoped<ICarnivalBlockService, CarnivalBlockService>();
builder.Services.AddScoped<IMembersService, MembersService>();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.MapGet("/", () => Results.Redirect("/swagger/index.html"))
   .ExcludeFromDescription();

// Configure the HTTP request pipeline.
app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "BlocoNaRua API";
        options.ConfigObject.ShowExtensions = true;
        options.ConfigObject.ShowCommonExtensions = true;
        options.ConfigObject.DisplayRequestDuration = true;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            var errorDetails = new
            {
                StatusCode = context.Response.StatusCode,
                DateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Message = "Internal Server Error.",
                Detailed = error.Error.Message,
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorDetails));
        }
    });
});

app.MapControllers();

app.Run();
