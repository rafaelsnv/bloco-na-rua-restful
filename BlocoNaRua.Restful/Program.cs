using System.Text.Json;
using Asp.Versioning;
using BlocoNaRua.Data.Extensions;
using BlocoNaRua.Services.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
});

IConfiguration configuration = builder.Configuration;

builder.Services.AddControllers();
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
builder.Services.AddServices();

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger/index.html"))
       .ExcludeFromDescription();

    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "BlocoNaRua API";
        options.ConfigObject.ShowExtensions = true;
        options.ConfigObject.ShowCommonExtensions = true;
        options.ConfigObject.DisplayRequestDuration = true;
        options.ConfigObject.DeepLinking = true;
        options.ConfigObject.DocExpansion = DocExpansion.None;
    });
}
else
{
    app.UseHttpsRedirection();
}

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

if (!app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Content("Welcome to BlocoNaRua API!"))
       .ExcludeFromDescription();
}

app.Run();
