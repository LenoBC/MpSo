using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MpSo.Common;
using MpSo.Common.Mappings;
using MpSo.Common.Models;
using MpSo.Dtos;
using MpSo.Entities;
using MpSo.Infrastructure.Persistence;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace MpSo.Features.Tags;

public partial class TagController : ApiControllerBase
{
    [HttpGet(ApiRoutes.Tags.Get)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<PaginatedList<TagDto>> GetTags([FromQuery] GetTagsQuery query)
    {
        return await Mediator.Send(query);
    }
}

public record GetTagsQuery : IRequest<PaginatedList<TagDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string Sort { get; init; } = "percentageShare";
    public string Order { get; init; } = "desc";
}

public class GetTagsQueryValidator : AbstractValidator<GetTagsQuery>
{
    public GetTagsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("PageSize must be between 1 and 200.");

        RuleFor(x => x.Sort)
            .Matches("^(name|percentageShare)$", RegexOptions.IgnoreCase).WithMessage("Invalid sort value.");

        RuleFor(x => x.Order)
            .Matches("^(asc|desc)$", RegexOptions.IgnoreCase).WithMessage("Invalid order value.");
    }
}

internal sealed class GetTagsQueryHandler(AppDbContext context) : IRequestHandler<GetTagsQuery, PaginatedList<TagDto>>
{
    private readonly AppDbContext _context = context;

    public async Task<PaginatedList<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Tags
            .AsNoTracking()
            .OrderBySortCriteria(request)
            .Select(TagDto.ToDto)
            .PaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}

public static class TagExtensions
{
    public static IQueryable<Tag> OrderBySortCriteria(this IQueryable<Tag> query, GetTagsQuery request)
    {
        var sortProperty = GetSortProperty(request);
        return request.Order.ToLower() switch
        {
            "asc" => query.OrderBy(sortProperty),
            _ => query.OrderByDescending(sortProperty),
        };
    }

    private static Expression<Func<Tag, object>> GetSortProperty(GetTagsQuery request)
    {
        return request.Sort.ToLower() switch
        {
            "name" => tag => tag.Name,
            "percentageshare" => tag => tag.PercentageShare,
            _ => tag => tag.Count,
        };
    }
}
