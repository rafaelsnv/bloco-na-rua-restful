using System.Security.Claims;
using System.Text.Json;
using Asp.Versioning;
using BlocoNaRua.Data.Extensions;
using BlocoNaRua.Restful.Middleware;
using BlocoNaRua.Services.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddEnvironmentVariables();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Model.Validation", LogLevel.Warning);

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
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    options.OperationFilter<BlocoNaRua.Restful.Swagger.FormFieldExamplesFilter>();
});
builder.Services.AddMemoryCache();
var supabaseUrl = configuration["Supabase:Url"] ?? "";
var supabaseIssuer = $"{supabaseUrl}/auth/v1";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = supabaseIssuer;
        options.MetadataAddress = $"{supabaseIssuer}/.well-known/openid-configuration";
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = "authenticated",
            ValidIssuer = supabaseIssuer
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("JWT auth failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("role", "admin"));
});
builder.Services.AddEntityFramework(configuration, builder.Environment);
builder.Services.AddRepositories();
builder.Services.AddServices();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
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

app.UseAuthentication();
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
            var errorLogger = context.RequestServices.GetService<BlocoNaRua.Services.Interfaces.IErrorLogger>();
            if (errorLogger != null)
            {
                var exception = error.Error as Exception;
                var isCritical = exception is OutOfMemoryException
                    or StackOverflowException
                    or AccessViolationException;

                var logEntry = new BlocoNaRua.Services.Interfaces.ErrorLogEntry
                {
                    Level = isCritical
                        ? BlocoNaRua.Services.Interfaces.ErrorLevel.Critical
                        : BlocoNaRua.Services.Interfaces.ErrorLevel.Error,
                    Source = "ExceptionHandler",
                    Message = exception?.InnerException?.Message ?? exception?.Message ?? "Unknown error",
                    StackTrace = exception?.InnerException?.StackTrace ?? exception?.StackTrace,
                    RequestPath = context.Request.Path.Value,
                    RequestMethod = context.Request.Method,
                    StatusCode = context.Response.StatusCode,
                    UserId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                };

                _ = errorLogger.LogAsync(logEntry);
            }

            var errorDetails = new
            {
                StatusCode = context.Response.StatusCode,
                DateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Message = "Internal Server Error.",
                Detailed = error.Error?.Message ?? "An unexpected error occurred.",
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

// Make implicit Program class public for WebApplicationFactory access
public partial class Program { }
