using MpSo.Entities;

namespace MpSo.Dtos;

public record TagSoDto
{
    public int Count { get; init; }
    public bool HasSynonyms { get; init; }
    public bool IsRequired { get; init; }
    public bool IsModeratorOnly { get; init; }
    public required string Name { get; init; }

    public static Func<TagSoDto, Tag> ToEntity => dto => new Tag
    {
        Count = dto.Count,
        HasSynonyms = dto.HasSynonyms,
        IsRequired = dto.IsRequired,
        IsModeratorOnly = dto.IsModeratorOnly,
        Name = dto.Name
    };
}
