using System.Collections.ObjectModel;
using PdfViewer.Core;
using PdfViewer.Models;
namespace PdfViewer.ViewModels;
public sealed class ViewerViewModel : ObservableObject
{
    public ObservableCollection<PdfPageItem> Pages { get; } = [];
    private string fileName = "Belge açılmadı", error = "";
    private bool busy;
    private int currentPage;
    public string FileName { get => fileName; set { fileName = value; Changed(); } }
    public string Error { get => error; set { error = value; Changed(); Changed(nameof(HasError)); } }
    public bool HasError => Error.Length > 0;
    public bool IsBusy { get => busy; set { busy = value; Changed(); Changed(nameof(CanOpen)); } }
    public bool CanOpen => !IsBusy;
    public bool HasDocument => Pages.Count > 0;
    public int PageCount => Pages.Count;
    public int CurrentPage { get => currentPage; set { currentPage = DocumentPolicy.ClampPage(value, PageCount); Changed(); } }
    public double Zoom { get; private set; } = 1;
    public bool FitWidth { get; private set; } = true;
    public void SetZoom(double zoom) { Zoom = DocumentPolicy.ClampZoom(zoom); FitWidth = false; }
    public void Fit() => FitWidth = true;
    public void SetDocument(string name, uint count)
    {
        Pages.Clear();
        for (int i = 0; i < count; i++) Pages.Add(new(i));
        FileName = name; CurrentPage = 1;
        Changed(nameof(PageCount)); Changed(nameof(HasDocument));
    }
}
