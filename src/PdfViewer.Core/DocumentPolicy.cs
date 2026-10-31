namespace PdfViewer.Core;

public static class DocumentPolicy
{
    public const ulong MaximumFileBytes = 200UL * 1024 * 1024;
    public const uint MaximumPages = 20_000;
    public const int MaximumRasterSide = 2560;
    public static double ClampZoom(double zoom) => double.IsFinite(zoom) ? Math.Clamp(zoom, .25, 3) : 1;
    public static int ClampPage(int page, int count) => count <= 0 ? 0 : Math.Clamp(page, 1, count);
    public static double? ZoomStep(double current, double factor)
    {
        var next = ClampZoom(current * factor);
        // Fit-to-width can be outside manual zoom bounds; a click must never reverse direction.
        if (factor > 1 && next <= current || factor < 1 && next >= current) return null;
        return next;
    }
    public static double PageWidth(double naturalWidth, double viewportWidth, bool fit, double zoom)
    {
        if (!double.IsFinite(naturalWidth) || naturalWidth <= 0) throw new ArgumentOutOfRangeException(nameof(naturalWidth));
        return fit ? Math.Max(100, double.IsFinite(viewportWidth) ? viewportWidth - 32 : naturalWidth) : naturalWidth * ClampZoom(zoom);
    }
    public static (uint Width, uint Height) RasterSize(double width, double height, double scale)
    {
        if (!double.IsFinite(width) || !double.IsFinite(height) || width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        var factor = Math.Min(Math.Max(double.IsFinite(scale) ? scale : 1, .1), MaximumRasterSide / Math.Max(width, height));
        return ((uint)Math.Max(1, Math.Round(width * factor)), (uint)Math.Max(1, Math.Round(height * factor)));
    }
}
