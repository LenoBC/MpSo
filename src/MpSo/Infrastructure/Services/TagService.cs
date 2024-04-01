using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MpSo.Common.Exceptions;
using MpSo.Common.Interfaces;
using MpSo.Common.Models;
using MpSo.Dtos;
using MpSo.Entities;
using System.IO.Compression;
using System.Text.Json;

namespace MpSo.Infrastructure.Services;

public class TagService(IHttpClientFactory httpClientFactory, IOptionsMonitor<TagFetchSettings> tagFetchSettings, ILogger<TagService> logger) : ITagService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("StackOverflow");
    private readonly IOptionsMonitor<TagFetchSettings> _tagFetchSettings = tagFetchSettings;
    private readonly ILogger<TagService> _logger = logger;

    public async Task<IEnumerable<Tag>> GetTagsAsync()
    {
        var (pagesToFetch, pageSize) = GetTagFetchSettings();
        var allTags = await FetchTagsFromApiAsync(pagesToFetch, pageSize);

        var totalTagCount = allTags.Sum(tag => tag.Count);
        allTags.ForEach(tag => tag.PercentageShare = (double)tag.Count / totalTagCount * 100);

        return allTags;
    }

    private (int pagesToFetch, int pageSize) GetTagFetchSettings()
    {
        var settings = _tagFetchSettings.CurrentValue;
        if (settings.PagesToFetch <= 0 || settings.PageSize <= 0 || settings.PagesToFetch * settings.PageSize < AppConstants.MinTagsToFetch)
        {
            settings.PagesToFetch = AppConstants.DefaultPagesToFetch;
            settings.PageSize = AppConstants.DefaultPageSize;

            _logger.LogWarning("Invalid TagFetchSettings detected. Using default values: {PagesToFetch}, {PageSize}", settings.PagesToFetch, settings.PageSize);
        }

        _logger.LogInformation("Fetching {PagesToFetch} pages of tags with {PageSize} tags each.", settings.PagesToFetch, settings.PageSize);
        return (settings.PagesToFetch, settings.PageSize);
    }

    private async Task<List<Tag>> FetchTagsFromApiAsync(int pagesToFetch, int pageSize)
    {
        var allTags = new List<Tag>();

        for (uint page = 1; page <= pagesToFetch; ++page)
        {
            var requestUrl = $"/tags?page={page}&pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow&filter=!*MO(WlYfplYE4nxA";
            var responseMessage = await _client.GetAsync(requestUrl);
            responseMessage.EnsureSuccessStatusCode();

            var stream = await responseMessage.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
            ApiResponse? responseObj = null;

            if (responseMessage.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                using var decompressedStream = new GZipStream(stream, CompressionMode.Decompress);
                responseObj = await JsonSerializer.DeserializeAsync<ApiResponse>(decompressedStream, options);
            }
            else
            {
                responseObj = await JsonSerializer.DeserializeAsync<ApiResponse>(stream, options);
            }

            if (responseObj?.Items is null)
            {
                throw new FailedToFetchTagsException($"Failed to fetch tags from StackOverflow API. Status code: {responseMessage.StatusCode}, Page: {page}");
            }

            allTags.AddRange(responseObj.Items.Select(TagSoDto.ToEntity));
        }

        return allTags;
    }

    private record ApiResponse
    {
        public List<TagSoDto> Items { get; init; } = null!;
        public bool HasMore { get; init; }
        public int QuotaMax { get; init; }
        public int QuotaRemaining { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int Total { get; init; }
    }
}

public class TagFetchSettings
{
    public int PagesToFetch { get; set; }
    public int PageSize { get; set; }
}
