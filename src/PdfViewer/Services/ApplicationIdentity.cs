using System.Runtime.InteropServices;
namespace PdfViewer.Services;

internal static class ApplicationIdentity
{
    // Keep this ID identical to the Inno Setup shortcut's AppUserModelID across updates.
    public const string AppId = "PDFViewer.Desktop";
    public static string IconPath => Path.Combine(AppContext.BaseDirectory, "Assets", "PdfViewer.ico");
    public static void Initialize() => Marshal.ThrowExceptionForHR(SetCurrentProcessExplicitAppUserModelID(AppId));

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appId);
}
