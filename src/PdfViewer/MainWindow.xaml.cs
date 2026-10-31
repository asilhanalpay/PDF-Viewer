using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using PdfViewer.Models;
using PdfViewer.Core;
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
    private bool updateBusy;
    private readonly CancellationTokenSource lifetime = new();
    private readonly DispatcherTimer updateTimer = new() { Interval = TimeSpan.FromMinutes(1) };
    private readonly DispatcherTimer resizeTimer = new() { Interval = TimeSpan.FromMilliseconds(120) };
    private double rasterScale = 1;
    public MainWindow()
    {
        InitializeComponent();
        PageList.SizeChanged += (_, _) => QueueResize();
        resizeTimer.Tick += (_, _) => { resizeTimer.Stop(); ResizePages(); };
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1150, 850));
        PageList.ItemsSource = ViewModel.Pages;
        Root.Loaded += OnRootLoaded;
        updateTimer.Tick += async (_, _) => await CheckForUpdatesAsync();
        Closed += async (_, _) =>
        {
            closed = true;
            updateTimer.Stop();
            resizeTimer.Stop();
            lifetime.Cancel();
            CancelRendering();
            if (session is not null) await session.DisposeAsync();
        };
    }
    private async void OnRootLoaded(object sender, RoutedEventArgs e)
    {
        Root.Loaded -= OnRootLoaded;
        rasterScale = Root.XamlRoot.RasterizationScale;
        Root.XamlRoot.Changed += (_, _) =>
        {
            var scale = Root.XamlRoot.RasterizationScale;
            if (Math.Abs(scale - rasterScale) < .001) return;
            rasterScale = scale;
            QueueResize();
        };
        updateTimer.Start();
        await CheckForUpdatesAsync();
    }
    private async Task CheckForUpdatesAsync()
    {
        if (closed || updateBusy) return;
        updateBusy = true;
        UpdateButton.IsEnabled = false;
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
            timeout.CancelAfter(TimeSpan.FromSeconds(15));
            using var updates = new UpdateService();
            var update = await updates.CheckAutomaticAsync(timeout.Token);
            if (closed) return;
            if (update is not null)
            {
                updateTimer.Stop();
                UpdateNotice.Message = $"V{update.Version} indirilmeye hazır. Mevcut uygulamanız güncellenecek.";
                UpdateNotice.IsOpen = true;
            }
            else UpdateNotice.IsOpen = false;
        }
        catch { /* Offline checks stay quiet; manual checks show errors. */ }
        finally { updateBusy = false; if (!closed) UpdateButton.IsEnabled = true; }
    }
    private async void Update_Click(object sender, RoutedEventArgs e)
    {
        if (updateBusy || closed) return;
        updateBusy = true;
        UpdateButton.IsEnabled = false;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        cancellation.CancelAfter(TimeSpan.FromMinutes(10));
        using var updates = new UpdateService();
        try
        {
            UpdateButton.Text = "Denetleniyor…";
            var update = await updates.CheckAsync(cancellation.Token);
            if (closed) return;
            var dialog = new ContentDialog
            {
                XamlRoot = Root.XamlRoot,
                Title = update is null ? "PDF Viewer güncel" : "Yeni sürüm mevcut",
                Content = update is null ? "Kullanılabilir yeni sürüm bulunamadı." : $"V{update.Version} yüklenecek. PDF Viewer kapanacak, mevcut kurulum güncellenecek ve uygulama yeniden açılacak. Açık belgeler kapanacak.",
                CloseButtonText = "Daha sonra",
                PrimaryButtonText = update is null ? "" : "Güncelle"
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary || update is null) return;
            UpdateButton.Text = "İndiriliyor…";
            var installer = await updates.DownloadAsync(update, cancellation.Token);
            if (closed) return;
            System.Diagnostics.Process.Start(UpdateInstaller.CreateStartInfo(installer, AppContext.BaseDirectory));
            Close();
        }
        catch (OperationCanceledException) when (closed) { }
        catch (Exception ex)
        {
            if (!closed) ViewModel.Error = ex is InvalidDataException ? ex.Message : "Güncelleme denetlenemedi veya indirilemedi. İnternet bağlantısını kontrol edip yeniden deneyin.";
        }
        finally { updateBusy = false; if (!closed) { UpdateButton.Text = "Güncellemeleri denetle"; UpdateButton.IsEnabled = true; } }
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
            opened = await PdfSession.OpenAsync(file, lifetime.Token);
            if (closed) return;
            CancelRendering();
            realized.Clear();
            var previous = session;
            PageList.ItemsSource = null;
            session = opened;
            opened = null;
            ViewModel.Fit();
            ViewModel.SetDocument(file.Name, session.Pages);
            PageList.Visibility = Visibility.Visible;
            DocumentToolbar.Visibility = Visibility.Visible;
            PageNavigation.Visibility = Visibility.Visible;
            ResizePages(false);
            PageList.ItemsSource = ViewModel.Pages;
            PageList.UpdateLayout();
            AttachScroll();
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
    private void CancelRendering(bool clearImages = true)
    {
        foreach (var pair in rendering) pair.Value.Cancel();
        foreach (var page in realized) { if (clearImages) page.Image = null; page.IsRendering = false; }
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
            page.IsRendering = false;
            return;
        }
        realized.Add(page);
        _ = RenderPageAsync(page);
    }
    private async Task RenderPageAsync(PdfPageItem page, bool force = false)
    {
        var source = session;
        if (source is null || closed || rendering.ContainsKey(page) || (!force && page.Image is not null)) return;
        var pending = new CancellationTokenSource();
        rendering.Add(page, pending);
        page.Error = "";
        page.IsRendering = page.Image is null;
        try
        {
            var result = await source.RenderAsync(page.Index, page.Width, rasterScale, pending.Token);
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
            page.Image = bitmap;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (!pending.IsCancellationRequested && !closed) page.Error = "Bu sayfa görüntülenemedi. Yeniden deneyin veya farklı bir PDF açın.";
        }
        finally
        {
            if (rendering.TryGetValue(page, out var active) && active == pending)
            { rendering.Remove(page); page.IsRendering = false; }
            pending.Dispose();
        }
    }
    private void QueueResize()
    {
        if (!ViewModel.HasDocument || closed) return;
        resizeTimer.Stop(); resizeTimer.Start();
    }
    private void ResizePages(bool preservePosition = true)
    {
        if (!ViewModel.HasDocument || closed) return;
        var anchor = ViewModel.Pages[Math.Max(0, ViewModel.CurrentPage - 1)];
        var oldContainer = PageList.ContainerFromItem(anchor) as FrameworkElement;
        var oldHeight = oldContainer?.ActualHeight ?? 0;
        var oldTop = oldContainer?.TransformToVisual(PageList).TransformPoint(new(0, 0)).Y ?? 0;
        var visible = realized.ToArray();
        CancelRendering(false);
        var viewport = scroll is { ViewportWidth: > 0 } ? scroll.ViewportWidth : PageList.ActualWidth;
        foreach (var page in ViewModel.Pages)
            page.Resize(DocumentPolicy.PageWidth(page.Size.Width, viewport, ViewModel.FitWidth, ViewModel.Zoom));
        PageList.UpdateLayout();
        if (preservePosition && scroll is not null && oldHeight > 0 && PageList.ContainerFromItem(anchor) is FrameworkElement container)
        {
            var top = container.TransformToVisual(PageList).TransformPoint(new(0, 0)).Y;
            var targetTop = oldTop * container.ActualHeight / oldHeight;
            scroll.ChangeView(ViewModel.FitWidth ? 0 : null, Math.Max(0, scroll.VerticalOffset + top - targetTop), null, true);
        }
        foreach (var page in visible.Where(realized.Contains)) _ = RenderPageAsync(page, true);
        UpdateZoomLabel();
    }
    private void UpdateZoomLabel()
    {
        if (!ViewModel.HasDocument) return;
        var page = ViewModel.Pages[Math.Max(0, ViewModel.CurrentPage - 1)];
        ZoomLabel.Text = $"{page.Width / page.Size.Width:P0}";
    }
    private void Fit_Click(object sender, RoutedEventArgs e) { ViewModel.Fit(); ResizePages(); }
    private void ChangeZoom(double factor)
    {
        if (!ViewModel.HasDocument) return;
        var page = ViewModel.Pages[Math.Max(0, ViewModel.CurrentPage - 1)];
        var next = DocumentPolicy.ZoomStep(page.Width / page.Size.Width, factor);
        if (next is null) return;
        ViewModel.SetZoom(next.Value);
        ResizePages();
    }
    private void ZoomIn_Click(object sender, RoutedEventArgs e) => ChangeZoom(1.2);
    private void ZoomOut_Click(object sender, RoutedEventArgs e) => ChangeZoom(1 / 1.2);
    private void Retry_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: PdfPageItem page }) _ = RenderPageAsync(page, true);
    }
    private void Error_Close(InfoBar sender, object args) => ViewModel.Error = "";
    private void Open_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    { args.Handled = true; Open_Click(OpenButton, new RoutedEventArgs()); }
    private void Navigate(int number)
    {
        ViewModel.CurrentPage = number;
        if (ViewModel.CurrentPage > 0) PageList.ScrollIntoView(ViewModel.Pages[ViewModel.CurrentPage - 1], ScrollIntoViewAlignment.Leading);
        PageNumber.Text = ViewModel.CurrentPage.ToString();
        UpdateZoomLabel();
    }
    private void Previous_Click(object sender, RoutedEventArgs e) => Navigate(ViewModel.CurrentPage - 1);
    private void Next_Click(object sender, RoutedEventArgs e) => Navigate(ViewModel.CurrentPage + 1);
    private void PageNumber_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter) return;
        if (int.TryParse(PageNumber.Text, out var number)) Navigate(number);
        else PageNumber.Text = ViewModel.CurrentPage.ToString();
        e.Handled = true;
    }
    private void PageNumber_LostFocus(object sender, RoutedEventArgs e) => PageNumber.Text = ViewModel.CurrentPage.ToString();
    private void Theme_Click(object sender, RoutedEventArgs e)
    {
        SystemTheme.IsChecked = sender == SystemTheme;
        LightTheme.IsChecked = sender == LightTheme;
        DarkTheme.IsChecked = sender == DarkTheme;
        Root.RequestedTheme = sender == LightTheme ? ElementTheme.Light : sender == DarkTheme ? ElementTheme.Dark : ElementTheme.Default;
    }
    private void PageList_Loaded(object sender, RoutedEventArgs e) => AttachScroll();
    private void AttachScroll()
    {
        var found = FindScroll(PageList);
        if (found is null || found == scroll) return;
        scroll = found;
        scroll.ViewChanged += (_, _) =>
        {
            var first = realized.Where(p => PageList.ContainerFromItem(p) is FrameworkElement container &&
            container.ActualHeight > 0 &&
            container.TransformToVisual(PageList).TransformPoint(new(0, container.ActualHeight)).Y > 0 &&
            container.TransformToVisual(PageList).TransformPoint(new(0, 0)).Y < PageList.ActualHeight)
            .OrderBy(p => p.Index).FirstOrDefault();
            if (first is not null) { ViewModel.CurrentPage = first.Index + 1; UpdateZoomLabel(); }
        };
        QueueResize();
    }
    private static ScrollViewer? FindScroll(DependencyObject parent)
    {
        if (parent is ScrollViewer viewer) return viewer;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            if (FindScroll(VisualTreeHelper.GetChild(parent, i)) is { } result) return result;
        return null;
    }
}
