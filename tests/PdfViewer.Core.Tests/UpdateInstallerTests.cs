using PdfViewer.Services;
using Xunit;
namespace PdfViewer.Tests;
public class UpdateInstallerTests
{
    [Fact] public void UpdateTargetsExistingFolderAsOneArgument()
    {
        var folder = Path.Combine(Path.GetTempPath(), "PDF Viewer Existing");
        var start = UpdateInstaller.CreateStartInfo("setup.exe", folder);
        Assert.Equal("setup.exe", start.FileName);
        Assert.Contains("/DIR=" + Path.GetFullPath(folder), start.ArgumentList);
        Assert.Contains("/UPDATE=1", start.ArgumentList);
        Assert.Contains("/SILENT", start.ArgumentList);
        Assert.Contains("/NORESTART", start.ArgumentList);
    }
}
