using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MpSo.Common.Exceptions;
using MpSo.Common.Interfaces;
using MpSo.Infrastructure.Persistence;

namespace MpSo.Infrastructure.Services;

public class DataInitializationService(
    ITagService tagService,
    IServiceProvider serviceProvider,
    ILogger<DataInitializationService> logger) : IHostedService
{
    private readonly ITagService _tagService = tagService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<DataInitializationService> _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await context.Tags.AnyAsync(cancellationToken))
        {
            return;
        }

        try
        {
            var tags = await _tagService.GetTagsAsync();
            await context.Tags.AddRangeAsync(tags, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (FailedToFetchTagsException ex)
        {
            _logger.LogError(ex, "An error occurred while fetching tags from the API");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while initializing data");
        }

        _logger.LogInformation("Data Initialization: Tags added to the database");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
