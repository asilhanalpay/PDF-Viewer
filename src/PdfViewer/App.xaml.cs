using Microsoft.UI.Xaml;
using PdfViewer.Services;
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
                File.WriteAllText(Path.Combine(folder, "startup-error.log"), args.Message + Environment.NewLine + args.Exception.ToString());
            }
            catch { /* Preserve the original failure if logging is unavailable. */ }
        };
        DebugSettings.IsXamlResourceReferenceTracingEnabled = true;
        DebugSettings.XamlResourceReferenceFailed += (_, args) =>
        {
            try
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PDF Viewer");
                Directory.CreateDirectory(folder);
                File.AppendAllText(Path.Combine(folder, "resource-error.log"), args.Message + Environment.NewLine);
            }
            catch { }
        };
        InitializeComponent();
    }
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        ApplicationIdentity.Initialize();
        window = new MainWindow();
        window.Activate();
    }
}
