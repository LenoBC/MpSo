using MpSo.Entities;

namespace MpSo.Common.Interfaces;

public interface ITagService
{
    Task<IEnumerable<Tag>> GetTagsAsync();
}
