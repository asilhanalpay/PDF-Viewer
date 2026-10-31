using System.Net;
using System.Text.Json;
using PdfViewer.Services;
using Xunit;
namespace PdfViewer.Tests;
public class UpdateServiceTests
{
    private sealed class Handler(string json, HttpStatusCode code = HttpStatusCode.OK) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(code) { Content = new StringContent(json) });
    }
    private static string Release(string url, string digest, bool prerelease = false, string tag = "V99.0.0") => JsonSerializer.Serialize(new
    {
        tag_name = tag, draft = false, prerelease,
        assets = new[] { new { name = "PDF-Viewer-Setup-x64.exe", browser_download_url = url, digest } }
    });
    private const string Url = "https://github.com/asilhanalpay/PDF-Viewer/releases/download/v99.0.0/PDF-Viewer-Setup-x64.exe";
    [Theory] [InlineData("V99.0.0")] [InlineData("v99.0.0")]
    public async Task RecognizesNewStableRelease(string tag)
    {
        using var service = new UpdateService(new Handler(Release(Url,"sha256:"+new string('a',64), tag: tag)));
        var result = await service.CheckAsync(default);
        Assert.NotNull(result); Assert.Equal(new Version(99,0,0),result.Version);
    }
    [Theory] [InlineData("http://github.com/asilhanalpay/PDF-Viewer/releases/download/a/b")] [InlineData("https://example.com/setup.exe")]
    public async Task RejectsUntrustedInstaller(string url)
    {
        using var service = new UpdateService(new Handler(Release(url,"sha256:"+new string('a',64))));
        await Assert.ThrowsAsync<InvalidDataException>(() => service.CheckAsync(default));
    }
    [Fact] public async Task RejectsMissingDigest()
    {
        using var service = new UpdateService(new Handler(Release(Url,"")));
        await Assert.ThrowsAsync<InvalidDataException>(() => service.CheckAsync(default));
    }
    [Fact] public async Task SkipsPrerelease()
    {
        using var service = new UpdateService(new Handler(Release(Url,"",true)));
        Assert.Null(await service.CheckAsync(default));
    }
    [Fact] public async Task RejectsDownloadWithWrongHash()
    {
        using var service = new UpdateService(new Handler("invalid installer"));
        await Assert.ThrowsAsync<InvalidDataException>(() => service.DownloadAsync(new(new Version(99,0,0), new Uri(Url), new string('0',64),"test.exe"),default));
    }
}

public class AutomaticUpdateTests
{
    private sealed class ProbeHandler(string tag) : HttpMessageHandler
    {
        public int ApiCalls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request.Method == HttpMethod.Head)
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = new(HttpMethod.Head, $"https://github.com/asilhanalpay/PDF-Viewer/releases/tag/{tag}") });
            ApiCalls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"draft\":false,\"prerelease\":true}") });
        }
    }
    [Fact] public async Task CurrentVersionDoesNotUseApiQuota()
    {
        var handler = new ProbeHandler("V0.0.1");
        using var service = new UpdateService(handler);
        Assert.Null(await service.CheckAutomaticAsync(default));
        Assert.Equal(0, handler.ApiCalls);
    }
    [Fact] public async Task NewVersionRequiresApiValidation()
    {
        var handler = new ProbeHandler("V99.0.0");
        using var service = new UpdateService(handler);
        Assert.Null(await service.CheckAutomaticAsync(default)); // API rejects the prerelease.
        Assert.Equal(1, handler.ApiCalls);
    }
}
