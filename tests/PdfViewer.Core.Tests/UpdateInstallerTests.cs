using PdfViewer.Services;
using Xunit;
namespace PdfViewer.Tests;
public class UpdateInstallerTests
{
    [Theory]
    [InlineData("PDF Viewer Existing", false)]
    [InlineData("Özel Klasör/PDF Viewer", true)]
    public void UpdateTargetsExistingFolderAsOneArgument(string name, bool trailingSeparator)
    {
        var folder = Path.Combine(Path.GetTempPath(), name);
        var start = UpdateInstaller.CreateStartInfo("setup.exe", trailingSeparator ? folder + Path.DirectorySeparatorChar : folder);
        Assert.Equal("setup.exe", start.FileName);
        Assert.Contains("/DIR=" + Path.GetFullPath(folder), start.ArgumentList);
        Assert.Contains("/UPDATE=1", start.ArgumentList);
        Assert.Contains("/SILENT", start.ArgumentList);
        Assert.Contains("/NORESTART", start.ArgumentList);
        Assert.Single(start.ArgumentList.Where(arg => arg.StartsWith("/DIR=")));
        Assert.True(start.UseShellExecute);
    }
}
