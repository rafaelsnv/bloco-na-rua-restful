using BlocoNaRua.Data.Extensions;

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

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("/swagger/index.html"))
   .ExcludeFromDescription();

// Configure the HTTP request pipeline.
app.UseSwagger();
// if (app.Environment.IsDevelopment())
// {
app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
