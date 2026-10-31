using PdfViewer.Core;
using PdfViewer.Services;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Xunit;

namespace PdfViewer.Windows.Tests;
public class PdfRenderingTests
{
    private static string Fixture(string name) => Path.Combine(Environment.GetEnvironmentVariable("PDF_VIEWER_FIXTURES") ?? throw new InvalidOperationException("PDF_VIEWER_FIXTURES eksik"), name);
    private static async Task<PdfSession> Open(string name) => await PdfSession.OpenAsync(await StorageFile.GetFileFromPathAsync(Fixture(name)));
    private static async Task<(uint Width, uint Height)> Decode(byte[] bytes)
    {
        Assert.True(bytes.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}));
        using var stream = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(stream.GetOutputStreamAt(0))) { writer.WriteBytes(bytes); await writer.StoreAsync(); }
        stream.Seek(0);
        var decoder = await BitmapDecoder.CreateAsync(stream);
        var pixels = (await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, new BitmapTransform(), ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage)).DetachPixelData();
        bool dark = false, light = false;
        for (int i = 0; i < pixels.Length; i += 4)
        {
            dark |= pixels[i] < 150 && pixels[i+1] < 150 && pixels[i+2] < 150 && pixels[i+3] > 0;
            light |= pixels[i] > 230 && pixels[i+1] > 230 && pixels[i+2] > 230;
        }
        Assert.True(dark && light, "Sayfa boş veya okunabilir içerik üretilmedi.");
        return (decoder.PixelWidth, decoder.PixelHeight);
    }
    [Theory] [InlineData(1)] [InlineData(100)] [InlineData(1000)]
    public async Task FirstAndLastPagesRenderRealContent(int count)
    {
        await using var session = await Open($"{count}-pages.pdf");
        Assert.Equal((uint)count, session.PageCount);
        foreach (var index in new[] { 0, count - 1 }.Distinct())
        {
            var size = session.Pages[index]; size.Validate();
            var rendered = await session.RenderAsync(index, 500, 1, default);
            var actual = await Decode(rendered.Bytes);
            Assert.Equal(DocumentPolicy.RasterSize(size.Width, size.Height, 500 / size.Width), actual);
        }
    }
    [Fact] public async Task MixedAndRotatedPagesHaveStableGeometry()
    {
        await using var session = await Open("rotated.pdf");
        Assert.True(session.Pages[0].Height > session.Pages[0].Width);
        Assert.True(session.Pages[1].Width > session.Pages[1].Height);
        Assert.True(session.Pages[2].Width > session.Pages[2].Height);
        for (int i = 0; i < 3; i++)
        {
            var geometry = session.Pages[i];
            var result = await session.RenderAsync(i, 400, 1, default);
            var size = await Decode(result.Bytes);
            Assert.Equal(geometry.Aspect, (double)size.Height / size.Width, 2);
            Assert.Equal(geometry, session.Pages[i]);
        }
    }
    [Theory] [InlineData("empty.pdf")] [InlineData("broken.pdf")] [InlineData("encrypted.pdf")]
    public async Task InvalidOrPasswordProtectedFilesAreRejected(string name)
    {
        await Assert.ThrowsAnyAsync<Exception>(async () => { await using var ignored = await Open(name); });
    }
    [Fact] public async Task FileSizeLimitRunsBeforeParsing()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid()+".pdf");
        try
        {
            using (var stream = File.Create(path)) stream.SetLength((long)DocumentPolicy.MaximumFileBytes + 1);
            await Assert.ThrowsAsync<InvalidDataException>(async () => { await using var ignored = await PdfSession.OpenAsync(await StorageFile.GetFileFromPathAsync(path)); });
        }
        finally { File.Delete(path); }
    }
    [Fact] public async Task UnsupportedExtensionIsRejected()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid()+".txt");
        try
        {
            File.Copy(Fixture("1-pages.pdf"), path);
            await Assert.ThrowsAsync<InvalidDataException>(async () => { await using var ignored = await PdfSession.OpenAsync(await StorageFile.GetFileFromPathAsync(path)); });
        }
        finally { File.Delete(path); }
    }
    [Fact] public async Task PageBoundsAreCheckedBeforeNativeAccess()
    {
        await using var session = await Open("1-pages.pdf");
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => session.RenderAsync(-1, 500, 1, default));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => session.RenderAsync(1, 500, 1, default));
        await Decode((await session.RenderAsync(0, 500, 1, default)).Bytes);
    }
    [Fact] public async Task CancelThenRenderAndDisposePreservesNativeLifetime()
    {
        var session = await Open("100-pages.pdf");
        try
        {
            for (int i = 0; i < 8; i++)
            {
                using var cancellation = new CancellationTokenSource();
                var obsolete = session.RenderAsync(i, 2200, 1, cancellation.Token);
                cancellation.Cancel();
                var latest = session.RenderAsync(i+1, 400, 1, default);
                try { await obsolete; } catch (OperationCanceledException) { }
                await Decode((await latest).Bytes);
            }
            var final = session.RenderAsync(0, 500, 1, default);
            var disposal = session.DisposeAsync().AsTask();
            await Decode((await final).Bytes);
            await disposal;
            await Assert.ThrowsAsync<ObjectDisposedException>(() => session.RenderAsync(0, 500, 1, default));
        }
        finally { await session.DisposeAsync(); }
    }
}
