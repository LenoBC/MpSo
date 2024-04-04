using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using MpSo.Common.Behaviours;
using MpSo.Common.Interfaces;
using MpSo.Infrastructure.Persistence;
using MpSo.Infrastructure.Services;

namespace MpSo;

public static class DependencyInjection
{
    public static IServiceCollection AddMpSo(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddMediatR(opt =>
        {
            opt.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            opt.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            opt.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
            opt.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
        });

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlite(configuration.GetConnectionString("DefaultConnection"),
            x => x.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        using var scope = services.BuildServiceProvider().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        var baseUrl = configuration.GetValue<string>("TagFetchSettings:BaseUrl");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "https://api.stackexchange.com/2.3";
        }

        services.AddHttpClient();
        services.AddHttpClient("StackOverflow", opt =>
        {
            opt.BaseAddress = new Uri(baseUrl);
            opt.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            opt.DefaultRequestHeaders.Add(HeaderNames.AcceptCharset, "utf-8");
        });

        services.AddSingleton<ITagService, TagService>();
        services.Configure<TagFetchSettings>(configuration.GetSection("TagFetchSettings"));

        services.AddHostedService<DataInitializationService>();

        return services;
    }
}
