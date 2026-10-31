using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using PdfViewer.Models;
using PdfViewer.Services;
using PdfViewer.ViewModels;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;

namespace PdfViewer;
public sealed partial class MainWindow : Window
{
    public ViewerViewModel ViewModel { get; } = new();
    private PdfSession? session;
    private readonly Dictionary<PdfPageItem, CancellationTokenSource> rendering = [];
    private readonly HashSet<PdfPageItem> realized = [];
    private ScrollViewer? scroll;
    private bool closed;
    private double pageWidth = 800;
    public MainWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1150, 850));
        PageList.ItemsSource = ViewModel.Pages;
        Closed += async (_, _) =>
        {
            closed = true;
            CancelRendering();
            if (session is not null) await session.DisposeAsync();
        };
    }
    private async void Open_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsBusy) return;
        ViewModel.IsBusy = true;
        ViewModel.Error = "";
        PdfSession? opened = null;
        try
        {
            var picker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            picker.FileTypeFilter.Add(".pdf");
            var file = await picker.PickSingleFileAsync();
            if (file is null || closed) return;
            opened = await PdfSession.OpenAsync(file);
            if (closed) return;
            CancelRendering();
            realized.Clear();
            var previous = session;
            session = opened;
            opened = null;
            ViewModel.Fit();
            ViewModel.SetDocument(file.Name, session.PageCount);
            ResizePages();
            Welcome.Visibility = Visibility.Collapsed;
            Title = "PDF Viewer";
            scroll?.ChangeView(0, 0, null, true);
            if (previous is not null) await previous.DisposeAsync();
        }
        catch (Exception ex) { if (!closed) ViewModel.Error = FriendlyError(ex); }
        finally
        {
            if (opened is not null) await opened.DisposeAsync();
            ViewModel.IsBusy = false;
        }
    }
    private static string FriendlyError(Exception ex) => ex switch
    {
        InvalidDataException => ex.Message,
        UnauthorizedAccessException => "Dosyaya erişilemiyor. Dosya izinlerini kontrol edin.",
        FileNotFoundException => "Dosya bulunamadı. Yeniden seçin.",
        _ => "PDF açılamadı veya görüntülenemedi. Dosya bozuk, şifreli ya da desteklenmeyen bir biçimde olabilir."
    };
    private void CancelRendering()
    {
        foreach (var pair in rendering) pair.Value.Cancel();
        foreach (var page in realized) page.Image = null;
        rendering.Clear();
    }
    private void Page_Changing(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.Item is not PdfPageItem page) return;
        if (args.InRecycleQueue)
        {
            realized.Remove(page);
            if (rendering.Remove(page, out var pending)) pending.Cancel();
            page.Image = null;
            return;
        }
        realized.Add(page);
        page.Resize(pageWidth);
        StartRender(page);
    }
    private async void StartRender(PdfPageItem page)
    {
        var source = session;
        if (source is null || closed || rendering.ContainsKey(page) || page.Image is not null) return;
        var pending = new CancellationTokenSource();
        rendering.Add(page, pending);
        page.Error = "";
        try
        {
            var result = await source.RenderAsync(page.Index, pageWidth, Root.XamlRoot?.RasterizationScale ?? 1, pending.Token);
            pending.Token.ThrowIfCancellationRequested();
            using var stream = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
            {
                writer.WriteBytes(result.Bytes);
                await writer.StoreAsync();
            }
            stream.Seek(0);
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            pending.Token.ThrowIfCancellationRequested();
            if (source != session || closed) return;
            page.SetAspect(result.Aspect);
            page.Image = bitmap;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (!pending.IsCancellationRequested && !closed) page.Error = FriendlyError(ex);
        }
        finally
        {
            if (rendering.TryGetValue(page, out var active) && active == pending) rendering.Remove(page);
            pending.Dispose();
        }
    }
    private void ResizePages()
    {
        if (!ViewModel.HasDocument) return;
        pageWidth = ViewModel.FitWidth ? Math.Max(100, PageList.ActualWidth - 64) : 800 * ViewModel.Zoom;
        var visible = realized.ToArray();
        CancelRendering();
        foreach (var page in ViewModel.Pages) page.Resize(pageWidth);
        foreach (var page in visible) StartRender(page);
    }
    private void Viewport_Changed(object sender, SizeChangedEventArgs e) { if (ViewModel.FitWidth) ResizePages(); }
    private void Fit_Click(object sender, RoutedEventArgs e) { ViewModel.Fit(); ResizePages(); }
    private void ZoomIn_Click(object sender, RoutedEventArgs e) { ViewModel.SetZoom(pageWidth / 800 * 1.2); ResizePages(); }
    private void ZoomOut_Click(object sender, RoutedEventArgs e) { ViewModel.SetZoom(pageWidth / 800 / 1.2); ResizePages(); }
    private void Navigate(int number)
    {
        ViewModel.CurrentPage = number;
        if (ViewModel.CurrentPage > 0) PageList.ScrollIntoView(ViewModel.Pages[ViewModel.CurrentPage - 1], ScrollIntoViewAlignment.Leading);
    }
    private void Previous_Click(object sender, RoutedEventArgs e) => Navigate(ViewModel.CurrentPage - 1);
    private void Next_Click(object sender, RoutedEventArgs e) => Navigate(ViewModel.CurrentPage + 1);
    private void PageNumber_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter && double.IsFinite(PageNumber.Value))
        { Navigate((int)Math.Clamp(PageNumber.Value, 1, Math.Max(1, ViewModel.PageCount))); e.Handled = true; }
    }
    private void Theme_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (Root is not null) Root.RequestedTheme = ThemePicker.SelectedIndex switch { 1 => ElementTheme.Light, 2 => ElementTheme.Dark, _ => ElementTheme.Default };
    }
    private void PageList_Loaded(object sender, RoutedEventArgs e)
    {
        scroll = FindScroll(PageList);
        if (scroll is not null) scroll.ViewChanged += (_, _) =>
        {
            var first = realized.Where(p => PageList.ContainerFromItem(p) is FrameworkElement container &&
                container.TransformToVisual(PageList).TransformPoint(new(0, container.ActualHeight)).Y > 0)
                .OrderBy(p => p.Index).FirstOrDefault();
            if (first is not null) ViewModel.CurrentPage = first.Index + 1;
        };
    }
    private static ScrollViewer? FindScroll(DependencyObject parent)
    {
        if (parent is ScrollViewer viewer) return viewer;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            if (FindScroll(VisualTreeHelper.GetChild(parent, i)) is { } result) return result;
        return null;
    }
}
