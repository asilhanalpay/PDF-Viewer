using System.Runtime.InteropServices.WindowsRuntime;
using PdfViewer.Core;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
namespace PdfViewer.Services;

// All native document access is serialized, including disposal. No active content API is exposed.
public sealed class PdfSession : IAsyncDisposable
{
    private PdfDocument? document;
    private readonly SemaphoreSlim gate = new(1);
    private bool disposed;
    public uint PageCount => document!.PageCount;
    private PdfSession(PdfDocument document) => this.document = document;
    public static async Task<PdfSession> OpenAsync(StorageFile file)
    {
        if (!file.FileType.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Lütfen bir PDF dosyası seçin.");
        var properties = await file.GetBasicPropertiesAsync();
        if (properties.Size == 0 || properties.Size > DocumentPolicy.MaximumFileBytes)
            throw new InvalidDataException("Dosya boş veya 200 MB sınırını aşıyor.");
        var doc = await PdfDocument.LoadFromFileAsync(file);
        if (doc.PageCount == 0 || doc.PageCount > DocumentPolicy.MaximumPages)
        { throw new InvalidDataException("Belge boş veya 20.000 sayfa sınırını aşıyor."); }
        return new(doc);
    }
    public async Task<(byte[] Bytes, double Aspect)> RenderAsync(int index, double width, double dpi, CancellationToken token)
    {
        await gate.WaitAsync(token);
        try
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            return await Task.Run(async () =>
            {
                token.ThrowIfCancellationRequested();
                using var page = document!.GetPage((uint)index);
                var size = DocumentPolicy.RasterSize(page.Size.Width, page.Size.Height, width * dpi / page.Size.Width);
                using var stream = new InMemoryRandomAccessStream();
                await page.RenderToStreamAsync(stream, new PdfPageRenderOptions { DestinationWidth = size.Width, DestinationHeight = size.Height }).AsTask(token);
                token.ThrowIfCancellationRequested();
                stream.Seek(0);
                var bytes = new byte[checked((int)stream.Size)];
                using var reader = new DataReader(stream);
                await reader.LoadAsync((uint)bytes.Length).AsTask(token);
                reader.ReadBytes(bytes);
                return (bytes, page.Size.Height / page.Size.Width);
            }, token);
        }
        finally { gate.Release(); }
    }
    public async ValueTask DisposeAsync()
    {
        await gate.WaitAsync();
        try { if (!disposed) { disposed = true; document = null; } }
        finally { gate.Release(); }
    }
}
