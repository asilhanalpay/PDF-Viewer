namespace PdfViewer.Core;

public static class DocumentPolicy
{
    public const ulong MaximumFileBytes = 200UL * 1024 * 1024;
    public const uint MaximumPages = 20_000;
    public const int MaximumRasterSide = 2560;
    public static double ClampZoom(double zoom) => double.IsFinite(zoom) ? Math.Clamp(zoom, .25, 3) : 1;
    public static int ClampPage(int page, int count) => count <= 0 ? 0 : Math.Clamp(page, 1, count);
    public static (uint Width, uint Height) RasterSize(double width, double height, double scale)
    {
        if (!double.IsFinite(width) || !double.IsFinite(height) || width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        var factor = Math.Min(Math.Max(scale, .1), MaximumRasterSide / Math.Max(width, height));
        return ((uint)Math.Max(1, Math.Round(width * factor)), (uint)Math.Max(1, Math.Round(height * factor)));
    }
}
