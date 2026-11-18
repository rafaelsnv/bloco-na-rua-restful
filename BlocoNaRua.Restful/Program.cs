using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Asp.Versioning;
using BlocoNaRua.Data.Extensions;
using BlocoNaRua.Restful.Middleware;
using BlocoNaRua.Services.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
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

// Self-issued HS256 tokens (plan §1). Issuer/audience/signing key come from the Auth section;
// the defaults match TokenService so issued and validated tokens always agree.
const string defaultAuthIssuer = "bloconarua-api";
const string defaultAuthAudience = "bloconarua-api";
// The Google handler needs a sign-in scheme to persist the OAuth state; it is a short-lived
// state cookie only — the API itself authenticates exclusively with the Bearer JWT.
const string googleStateScheme = "GoogleState";

var authIssuer = configuration["Auth:Issuer"] ?? defaultAuthIssuer;
var authAudience = configuration["Auth:Audience"] ?? defaultAuthAudience;
var uploadsBasePath = configuration["Uploads:BasePath"] ?? "/app/uploads";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authIssuer,
            ValidateAudience = true,
            ValidAudience = authAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(ResolveSigningKeyBytes(configuration["Auth:SigningKey"])),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
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
    })
    .AddCookie(googleStateScheme, options =>
    {
        options.Cookie.Name = "bnr_google_state";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = false;
    })
    .AddGoogle("Google", options =>
    {
        options.ClientId = configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? "";
        options.CallbackPath = "/api/v1/auth/google/callback";
        options.SignInScheme = googleStateScheme;
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

app.UseAuthentication();
app.UseAuthorization();

// Serve uploaded media from the volume mount (plan §4). Public read: avatars are not sensitive.
try
{
    Directory.CreateDirectory(uploadsBasePath);
    app.UseStaticFiles(new StaticFileOptions
    {
        RequestPath = "/uploads",
        FileProvider = new PhysicalFileProvider(uploadsBasePath)
    });
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Static file serving for /uploads is disabled: '{UploadsBasePath}' is unavailable.", uploadsBasePath);
}

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
public partial class Program
{
    private const int MinimumSigningKeyBytes = 32;

    /// <summary>
    /// Resolves the HS256 signing key bytes. Mirrors <c>TokenService.DecodeSigningKey</c> so the
    /// validator and the token issuer always use the very same bytes: base64 first (deployment
    /// format), raw secret bytes as fallback. A missing key is a loud failure, never a default.
    /// </summary>
    private static byte[] ResolveSigningKeyBytes(string? signingKey)
    {
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                "Auth:SigningKey is missing. Set the Auth__SigningKey environment variable to the secret used to sign the API tokens.");
        }

        try
        {
            var decoded = Convert.FromBase64String(signingKey);
            if (decoded.Length >= MinimumSigningKeyBytes)
            {
                return decoded;
            }
        }
        catch (FormatException)
        {
            // Not base64 — fall back to the raw secret bytes below.
        }

        var keyBytes = Encoding.UTF8.GetBytes(signingKey);
        if (keyBytes.Length < MinimumSigningKeyBytes)
        {
            throw new InvalidOperationException(
                $"Auth:SigningKey must decode to at least {MinimumSigningKeyBytes} bytes to sign tokens with HS256.");
        }

        return keyBytes;
    }
}
