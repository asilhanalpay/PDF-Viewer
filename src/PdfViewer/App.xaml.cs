using Microsoft.UI.Xaml;
namespace PdfViewer;
public partial class App : Application
{
    private Window? window;
    public App()
    {
        UnhandledException += (_, args) =>
        {
            try
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PDF Viewer");
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, "startup-error.log"), args.Exception.ToString());
            }
            catch { /* Preserve the original failure if logging is unavailable. */ }
        };
        InitializeComponent();
    }
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        window = new MainWindow();
        window.Activate();
    }
}
