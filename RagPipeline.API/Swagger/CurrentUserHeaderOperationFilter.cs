using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RagPipeline.API.Swagger
{
    public class CurrentUserHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-User-Id",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Development user identity. Replace this with authenticated claims when JWT authentication is enabled.",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });
        }
    }
}
