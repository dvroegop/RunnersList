using System.Net;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using RunnersListLibrary.Secrets;
using RunnersListLibrary.SongBpm;

namespace RunnersListLibrary.Tests;

public class SongBpmConnectorTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly SongBpmConnector _songBpmConnector;
    private readonly Mock<IOptions<SongBpmSecrets>> _songBpmSecretsMock;

    public SongBpmConnectorTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _songBpmSecretsMock = new Mock<IOptions<SongBpmSecrets>>();
        _songBpmSecretsMock.Setup(s => s.Value).Returns(new SongBpmSecrets { ApiKey = "test_api_key" });

        var httpClientHandlerMock = new Mock<HttpMessageHandler>();
        httpClientHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"search\":[{\"tempo\":\"120\"}]}")
            });

        var httpClient = new HttpClient(httpClientHandlerMock.Object);
        _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _songBpmConnector = new SongBpmConnector(_httpClientFactoryMock.Object, _songBpmSecretsMock.Object);
    }

    [Fact]
    public async Task GetSongBpmAsync_ReturnsBpm_WhenApiResponseIsValid()
    {
        var bpm = await _songBpmConnector.GetSongBpmAsync("artist", "title");
        Assert.Equal(120, bpm);
    }

    [Fact]
    public async Task GetSongBpmAsync_ReturnsMinusOne_WhenApiResponseIsInvalid()
    {
        var httpClientHandlerMock = new Mock<HttpMessageHandler>();
        httpClientHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"search\":[]}")
            });

        var httpClient = new HttpClient(httpClientHandlerMock.Object);
        _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var bpm = await _songBpmConnector.GetSongBpmAsync("artist", "title");
        Assert.Equal(-1, bpm);
    }

    [Fact]
    public async Task GetSongBpmAsync_ReturnsMinusOne_WhenApiResponseIsError()
    {
        var httpClientHandlerMock = new Mock<HttpMessageHandler>();
        httpClientHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        var httpClient = new HttpClient(httpClientHandlerMock.Object);
        _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var bpm = await _songBpmConnector.GetSongBpmAsync("artist", "title");
        Assert.Equal(-1, bpm);
    }
}