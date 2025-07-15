using BlocoNaRua.Data.Extensions;
using BlocoNaRua.Restful.Services;
using BlocoNaRua.Restful.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

IConfiguration configuration = builder.Configuration;

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    setup.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BlocoNaRua API",
        Version = "v1",
        Description = "API for managing carnival blocks and meetings in BlocoNaRua application."
    });

});
builder.Services.AddEntityFramework(configuration);
builder.Services.AddRepositories();

// Register application services
builder.Services.AddScoped<ICarnivalBlockService, CarnivalBlockService>();
builder.Services.AddScoped<IMemberService, MemberService>();

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("/swagger/index.html"))
   .ExcludeFromDescription();

// Configure the HTTP request pipeline.
app.UseSwagger();
// if (app.Environment.IsDevelopment())
// {
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "BlocoNaRua API";
    options.ConfigObject.ShowExtensions = true;
    options.ConfigObject.ShowCommonExtensions = true;
    options.ConfigObject.DisplayRequestDuration = true;
});
// }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
