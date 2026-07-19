using System.Text.Json;
using System.Text;
using Asp.Versioning;
using BlocoNaRua.Data.Extensions;
using BlocoNaRua.Services.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
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
builder.Services.AddMemoryCache();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Supabase:Url"],
            ValidAudience = configuration["Supabase:Url"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Supabase:JwtSecret"] ?? ""))
        };
    });
builder.Services.AddEntityFramework(configuration, builder.Environment);
builder.Services.AddRepositories();
builder.Services.AddServices();

var app = builder.Build();

app.UseForwardedHeaders();

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
