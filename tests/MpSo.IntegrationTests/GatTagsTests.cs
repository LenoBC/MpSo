using FluentAssertions;
using MpSo.Common.Exceptions;
using MpSo.Features.Tags;

namespace MpSo.IntegrationTests;

public class GatTagsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetTags_ReturnsTagsSortedByPercentageShareDescending()
    {
        // Arrange
        var query = new GetTagsQuery { PageNumber = 1, PageSize = 10, Sort = "percentageShare", Order = "desc" };

        // Act
        var result = await Sender.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNullOrEmpty();
        result.Items.Should().HaveCountLessThanOrEqualTo(10);
        result.Items.Should().BeInDescendingOrder(x => x.PercentageShare);
    }

    [Fact]
    public async Task GetTags_ReturnsTagsSortedByPercentageShareAscending()
    {
        // Arrange
        var query = new GetTagsQuery { PageNumber = 1, PageSize = 10, Sort = "percentageShare", Order = "asc" };

        // Act
        var result = await Sender.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNullOrEmpty();
        result.Items.Should().HaveCountLessThanOrEqualTo(10);
        result.Items.Should().BeInAscendingOrder(x => x.PercentageShare);
    }

    [Theory]
    [InlineData(1, 10, "invalid", "desc")] // Invalid sort value
    [InlineData(1, 10, "percentageShare", "invalid")] // Invalid order value
    [InlineData(-10, 10, "percentageShare", "desc")] // Invalid page number
    [InlineData(1, 0, "percentageShare", "desc")] // Invalid page size
    [InlineData(1, 201, "percentageShare", "desc")] // Invalid page size greater than 200
    public async Task GetTags_WithInvalidParameters_ThrowsValidationException(
    int pageNumber, int pageSize, string sort, string order)
    {
        // Arrange
        var query = new GetTagsQuery { PageNumber = pageNumber, PageSize = pageSize, Sort = sort, Order = order };

        // Act
        Func<Task> action = async () => await Sender.Send(query);

        // Assert
        await action.Should().ThrowAsync<ValidationException>();
    }
}
