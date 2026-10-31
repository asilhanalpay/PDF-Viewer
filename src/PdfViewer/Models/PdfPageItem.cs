using Microsoft.UI.Xaml.Media.Imaging;
using PdfViewer.ViewModels;
namespace PdfViewer.Models;
public sealed class PdfPageItem(int index) : ObservableObject
{
    public int Index { get; } = index;
    public string Label => $"Sayfa {Index + 1}";
    private double aspect = 1.4142, width = 800;
    public double Width => width;
    public double Height => width * aspect;
    private BitmapImage? image;
    public BitmapImage? Image { get => image; set { image = value; Changed(); } }
    private string error = "";
    public string Error { get => error; set { error = value; Changed(); } }
    public void Resize(double value) { width = value; Changed(nameof(Width)); Changed(nameof(Height)); }
    public void SetAspect(double value) { aspect = value; Changed(nameof(Height)); }
}
