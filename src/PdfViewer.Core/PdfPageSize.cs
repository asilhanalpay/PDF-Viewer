namespace PdfViewer.Core;

public readonly record struct PdfPageSize(double Width, double Height)
{
    public double Aspect => Height / Width;
    public void Validate()
    {
        if (!double.IsFinite(Width) || !double.IsFinite(Height) || Width <= 0 || Height <= 0)
            throw new InvalidDataException("PDF sayfasının boyutu geçersiz.");
    }
}
