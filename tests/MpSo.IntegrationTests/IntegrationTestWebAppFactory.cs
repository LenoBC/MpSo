using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MpSo.Infrastructure.Persistence;
using System.Data.Common;
using System.Reflection;

namespace MpSo.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                               d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            var dbConnectionDescriptor = services.SingleOrDefault(
               d => d.ServiceType ==
                   typeof(DbConnection));

            if (dbConnectionDescriptor is not null)
            {
                services.Remove(dbConnectionDescriptor);
            }

            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("Data Source=tests.db");
                connection.Open();

                return connection;
            });

            services.AddDbContext<AppDbContext>((container, opt) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                var projectAssembly = Assembly.GetAssembly(typeof(IntegrationTestWebAppFactory))!.GetName().Name;
                opt.UseSqlite(connection,
                x => x.MigrationsAssembly(projectAssembly));
            });

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        });

        builder.UseEnvironment("Development");
    }
}
