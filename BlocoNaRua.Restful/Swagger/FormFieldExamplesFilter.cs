using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BlocoNaRua.Restful.Swagger;

public class FormFieldExamplesFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content == null)
            return;

        // Only apply to login endpoint
        if (!context.ApiDescription.RelativePath?.Contains("login", StringComparison.OrdinalIgnoreCase) ?? true)
            return;

        foreach (var mediaType in operation.RequestBody.Content.Values)
        {
            if (mediaType.Schema?.Properties == null)
                continue;

            foreach (var prop in mediaType.Schema.Properties)
            {
                if (prop.Value.Type == "string")
                {
                    if (prop.Key.Equals("Email", StringComparison.OrdinalIgnoreCase))
                        prop.Value.Example = new OpenApiString("teste@teste.com");
                    else if (prop.Key.Equals("Password", StringComparison.OrdinalIgnoreCase))
                        prop.Value.Example = new OpenApiString("123456");
                }
            }
        }
    }
}
