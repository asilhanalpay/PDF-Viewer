using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using PdfViewer.Core;
using PdfViewer.ViewModels;
namespace PdfViewer.Models;
public sealed class PdfPageItem(int index, PdfPageSize size) : ObservableObject
{
    public int Index { get; } = index;
    public PdfPageSize Size { get; } = size;
    public string Label => $"Sayfa {Index + 1}";
    private double width = size.Width;
    public double Width => width;
    public double Height => width * Size.Aspect;
    private BitmapImage? image;
    public BitmapImage? Image { get => image; set { image = value; Changed(); } }
    private bool isRendering;
    public bool IsRendering { get => isRendering; set { isRendering = value; Changed(); } }
    private string error = "";
    public string Error { get => error; set { error = value; Changed(); Changed(nameof(ErrorVisibility)); } }
    public Visibility ErrorVisibility => Error.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
    public void Resize(double value)
    {
        if (Math.Abs(width - value) < .01) return;
        width = value; Changed(nameof(Width)); Changed(nameof(Height));
    }
}
