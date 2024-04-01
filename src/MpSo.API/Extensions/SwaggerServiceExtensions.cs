using Microsoft.OpenApi.Models;

namespace MpSo.API.Extensions;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(swagger =>
        {
            swagger.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1.0",
                Title = "MpSo.API v1",
                Description = "MpSo API",
            });
        });

        return services;
    }
}
