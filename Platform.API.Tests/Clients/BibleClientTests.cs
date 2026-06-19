using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Platform.API.Clients;
using Platform.API.Exceptions;
using Platform.API.Tests.Fakes;
using Xunit;

namespace Platform.API.Tests.Clients;

public sealed class BibleClientTests
{
    // -------------------------------------------------------------------------
    // GetVersionsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetVersionsAsync_ReturnsVersionSummaries_WhenApiSucceeds()
    {
        // Arrange
        const string json = """
            {
              "data": [
                { "id": 3034, "abbreviation": "BSB", "localized_abbreviation": "BSB",
                  "title": "Berean Standard Bible", "localized_title": "Berean Standard Bible",
                  "language_tag": "en" }
              ],
              "next_page_token": null
            }
            """;
        var client = BuildClient(HttpStatusCode.OK, json);

        // Act
        var result = await client.GetVersionsAsync("en");

        // Assert
        result.Data.Should().HaveCount(1);
        result.Data[0].Id.Should().Be(3034);
        result.Data[0].Abbreviation.Should().Be("BSB");
        result.NextPageToken.Should().BeNull();
    }

    [Fact]
    public async Task GetVersionsAsync_ReturnsEmptyResult_WhenApiReturnsEmptyData()
    {
        var client = BuildClient(HttpStatusCode.OK, """{"data":[],"next_page_token":null}""");
        var result = await client.GetVersionsAsync("en");
        result.Data.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetVersionsAsync_ThrowsArgumentException_WhenLanguageRangeIsInvalid(string languageRange)
    {
        var client = BuildClient(HttpStatusCode.OK, """{"data":[],"next_page_token":null}""");
        var act = () => client.GetVersionsAsync(languageRange);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetVersionsAsync_ThrowsArgumentOutOfRangeException_WhenPageSizeIsNotPositive()
    {
        var client = BuildClient(HttpStatusCode.OK, """{"data":[],"next_page_token":null}""");
        var act = () => client.GetVersionsAsync("en", pageSize: 0);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetVersionsAsync_IncludesPageToken_WhenProvided()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"data":[],"next_page_token":null}""");
        var client = BuildClientFromHandler(handler);

        await client.GetVersionsAsync("en", pageToken: "tok123");

        handler.LastRequest!.RequestUri!.Query.Should().Contain("page_token=tok123");
    }

    [Fact]
    public async Task GetVersionsAsync_IncludesPageSize_WhenProvided()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"data":[],"next_page_token":null}""");
        var client = BuildClientFromHandler(handler);

        await client.GetVersionsAsync("en", pageSize: 5);

        handler.LastRequest!.RequestUri!.Query.Should().Contain("page_size=5");
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GetVersionsAsync_ThrowsYouVersionApiException_WhenApiReturnsError(HttpStatusCode statusCode)
    {
        var client = BuildClient(statusCode, """{"error":"fail"}""");
        var act = () => client.GetVersionsAsync("en");
        await act.Should().ThrowAsync<YouVersionApiException>()
            .Where(e => e.StatusCode == statusCode);
    }

    // -------------------------------------------------------------------------
    // GetVersionAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetVersionAsync_ReturnsVersion_WhenApiSucceeds()
    {
        const string json = """
            {
              "id": 3034, "abbreviation": "BSB", "localized_abbreviation": "BSB",
              "title": "Berean Standard Bible", "localized_title": "Berean Standard Bible",
              "language_tag": "en", "copyright": "Public Domain",
              "books": ["GEN","EXO","REV"]
            }
            """;
        var client = BuildClient(HttpStatusCode.OK, json);

        var version = await client.GetVersionAsync(3034);

        version.Id.Should().Be(3034);
        version.Abbreviation.Should().Be("BSB");
        version.Books.Should().Contain("GEN").And.Contain("REV");
        version.Copyright.Should().Be("Public Domain");
    }

    [Fact]
    public async Task GetVersionAsync_ThrowsYouVersionApiException_WhenNotFound()
    {
        var client = BuildClient(HttpStatusCode.NotFound, """{"error":"not found"}""");
        var act = () => client.GetVersionAsync(9999);
        await act.Should().ThrowAsync<YouVersionApiException>()
            .Where(e => e.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetVersionAsync_ThrowsArgumentOutOfRangeException_WhenVersionIdIsNotPositive()
    {
        var client = BuildClient(HttpStatusCode.OK, "{}");
        var act = () => client.GetVersionAsync(0);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    // -------------------------------------------------------------------------
    // GetBooksAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetBooksAsync_ReturnsBooksFromVersionMetadata()
    {
        const string json = """
            {
              "id": 3034, "abbreviation": "BSB", "localized_abbreviation": "BSB",
              "title": "Berean Standard Bible", "localized_title": "Berean Standard Bible",
              "language_tag": "en", "copyright": "Public Domain",
              "books": ["GEN","EXO","LEV"]
            }
            """;
        var client = BuildClient(HttpStatusCode.OK, json);

        var books = await client.GetBooksAsync(3034);

        books.Should().HaveCount(3);
        books[0].Usfm.Should().Be("GEN");
        books[2].Usfm.Should().Be("LEV");
    }

    [Fact]
    public async Task GetBooksAsync_ThrowsArgumentOutOfRangeException_WhenVersionIdIsNotPositive()
    {
        var client = BuildClient(HttpStatusCode.OK, "{}");
        var act = () => client.GetBooksAsync(0);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetBooksAsync_ThrowsArgumentNullException_WhenVersionIsNull()
    {
        var client = BuildClient(HttpStatusCode.OK, "{}");
        var act = () => client.GetBooksAsync((Platform.API.Models.BibleVersion)null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static BibleClient BuildClient(HttpStatusCode status, string json)
        => BuildClientFromHandler(new FakeHttpMessageHandler(status, json));

    private static BibleClient BuildClientFromHandler(FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new System.Uri("https://api.youversion.com") };
        return new BibleClient(httpClient, NullLogger<BibleClient>.Instance);
    }
}
