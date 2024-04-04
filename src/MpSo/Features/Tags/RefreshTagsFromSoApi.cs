using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MpSo.Common;
using MpSo.Common.Exceptions;
using MpSo.Common.Interfaces;
using MpSo.Infrastructure.Persistence;

namespace MpSo.Features.Tags;

public partial class TagController
{
    [HttpPost(ApiRoutes.Tags.RefreshFromSoApi)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> RefreshTagsFromSoApi()
    {
        await Mediator.Send(new RefreshTagsFromSoApiCommand());

        return NoContent();
    }
}

public record RefreshTagsFromSoApiCommand : IRequest<Unit>
{
}

internal sealed class RefreshTagsFromSoApiCommandHandler(
    AppDbContext context,
    ITagService tagService,
    ILogger<RefreshTagsFromSoApiCommandHandler> logger) : IRequestHandler<RefreshTagsFromSoApiCommand, Unit>
{
    private readonly AppDbContext _context = context;
    private readonly ITagService _tagService = tagService;
    private readonly ILogger<RefreshTagsFromSoApiCommandHandler> _logger = logger;
    public async Task<Unit> Handle(RefreshTagsFromSoApiCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var tags = await _tagService.GetTagsAsync();
            await _context.Tags.ExecuteDeleteAsync(cancellationToken);

            await _context.Tags.AddRangeAsync(tags, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Tags refreshed from the API");

            return Unit.Value;
        }
        catch (FailedToFetchTagsException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "An error occurred while fetching tags from the API");
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "An unexpected error occurred while refreshing tags from the API");
            throw new ConflictException("An unexpected error occurred while refreshing tags from the API", ex);
        }
    }
}
