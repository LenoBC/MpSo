using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MpSo.Entities;
using MpSo.Infrastructure.Services;
using NSubstitute;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace MpSo.UnitTests;

public class TagServiceTests
{
    private readonly TagService _tagService;
    private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
    private readonly IOptionsMonitor<TagFetchSettings> _tagFetchSettings = Substitute.For<IOptionsMonitor<TagFetchSettings>>();
    private readonly ILogger<TagService> _logger = Substitute.For<ILogger<TagService>>();
    private readonly HttpClient _client;
    public TagServiceTests()
    {
        _client = new HttpClient(new HttpMessageHandlerMock())
        {
            BaseAddress = new Uri("https://api.stackexchange.com/2.3")
        };
        _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(_client);
        _tagFetchSettings.CurrentValue.Returns(new TagFetchSettings { PagesToFetch = 1, PageSize = 3 });
        _tagService = new TagService(_httpClientFactory, _tagFetchSettings, _logger);
    }

    [Fact]
    public async Task FetchTagsFromApiAsync_ShouldReturnTags()
    {
        // Act
        var tags = await _tagService.FetchTagsFromApiAsync(1, 3);

        // Assert
        tags.Should().NotBeNull();
        tags.Should().NotBeEmpty().And.HaveCount(3);
        tags.Should().AllBeOfType<Tag>();
    }

    [Fact]
    public async Task GetTagsAsync_ShouldCalculatePercentageShare()
    {
        // Act
        var tags = await _tagService.GetTagsAsync();
        var totalTagCount = tags.Sum(tag => tag.Count);

        // Assert
        tags.Should().NotBeNull();
        tags.Should().NotBeEmpty().And.HaveCount(3);
        tags.Should().AllBeOfType<Tag>();
        tags.Should().Contain(tag => tag.PercentageShare > 0);
        tags.FirstOrDefault(tag => tag.Name == "javascript")?.PercentageShare.Should().BeApproximately(2529083d / totalTagCount * 100, 0.1);
        tags.FirstOrDefault(tag => tag.Name == "python")?.PercentageShare.Should().BeApproximately(2192636d / totalTagCount * 100, 0.1);
        tags.FirstOrDefault(tag => tag.Name == "java")?.PercentageShare.Should().BeApproximately(1917411d / totalTagCount * 100, 0.1);
    }
}

public class HttpMessageHandlerMock : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri!.ToString().Contains("/tags"))
        {
            var jsonResponse = @"
            {
                ""items"": [
                    {
                        ""has_synonyms"": true,
                        ""is_moderator_only"": false,
                        ""is_required"": false,
                        ""count"": 2529083,
                        ""name"": ""javascript""
                    },
                    {
                        ""has_synonyms"": true,
                        ""is_moderator_only"": false,
                        ""is_required"": false,
                        ""count"": 2192636,
                        ""name"": ""python""
                    },
                    {
                        ""has_synonyms"": true,
                        ""is_moderator_only"": false,
                        ""is_required"": false,
                        ""count"": 1917411,
                        ""name"": ""java""
                    }
                ],
                ""has_more"": true,
                ""quota_max"": 300,
                ""quota_remaining"": 145,
                ""page"": 1,
                ""page_size"": 3,
                ""total"": 65715
            }";

            var gzipContent = CompressString(jsonResponse);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(gzipContent)
            };
            responseMessage.Content.Headers.ContentEncoding.Add("gzip");
            responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await Task.FromResult(responseMessage);
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("Not Found")
        };
    }

    private static byte[] CompressString(string str)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionMode.Compress))
        using (var writer = new StreamWriter(gzip, Encoding.UTF8))
        {
            writer.Write(str);
        }

        return output.ToArray();
    }
}

