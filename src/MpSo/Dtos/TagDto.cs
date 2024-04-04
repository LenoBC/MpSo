using MpSo.Entities;
using System.Linq.Expressions;

namespace MpSo.Dtos;

public record TagDto
{
    public long Id { get; init; }
    public required string Name { get; init; }
    public int Count { get; init; }
    public double PercentageShare { get; init; }
    public bool HasSynonyms { get; init; }
    public bool IsRequired { get; init; }
    public bool IsModeratorOnly { get; init; }

    public static Expression<Func<Tag, TagDto>> ToDto => tag => new TagDto
    {
        Id = tag.Id,
        Name = tag.Name,
        Count = tag.Count,
        PercentageShare = tag.PercentageShare,
        HasSynonyms = tag.HasSynonyms,
        IsRequired = tag.IsRequired,
        IsModeratorOnly = tag.IsModeratorOnly
    };
}
