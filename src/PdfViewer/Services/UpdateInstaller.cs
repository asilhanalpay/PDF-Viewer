using System.Diagnostics;
namespace PdfViewer.Services;
public static class UpdateInstaller
{
    public static ProcessStartInfo CreateStartInfo(string installer, string installDirectory)
    {
        var start = new ProcessStartInfo(installer) { UseShellExecute = true };
        start.ArgumentList.Add("/SILENT");
        start.ArgumentList.Add("/SUPPRESSMSGBOXES");
        start.ArgumentList.Add("/NORESTART");
        start.ArgumentList.Add("/CLOSEAPPLICATIONS");
        start.ArgumentList.Add("/UPDATE=1");
        start.ArgumentList.Add("/DIR=" + Path.GetFullPath(installDirectory).TrimEnd(Path.DirectorySeparatorChar));
        return start;
    }
}
