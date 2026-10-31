using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
namespace PdfViewer.Services;

public sealed record AvailableUpdate(Version Version, Uri Installer, string Sha256, string FileName);
public sealed class UpdateService : IDisposable
{
    // Public distribution repository; no account credentials are stored in the application.
    public const string Repository = "asilhanalpay/PDF-Viewer";
    private readonly HttpClient client = new() { Timeout = TimeSpan.FromMinutes(10) };
    private const long MaxInstallerBytes = 600L * 1024 * 1024;
    public UpdateService() => client.DefaultRequestHeaders.UserAgent.ParseAdd("PDF-Viewer/0.1");
    public async Task<AvailableUpdate?> CheckAsync(CancellationToken token)
    {
        using var response = await client.GetAsync($"https://api.github.com/repos/{Repository}/releases/latest", token);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
        var release = json.RootElement;
        if (release.GetProperty("draft").GetBoolean() || release.GetProperty("prerelease").GetBoolean()) return null;
        if (!Version.TryParse(release.GetProperty("tag_name").GetString()?.TrimStart('v'), out var latest))
            throw new InvalidDataException("Sürüm bilgisi okunamadı.");
        var current = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 1, 0);
        if (new Version(latest.Major, latest.Minor, Math.Max(0, latest.Build)) <= new Version(current.Major, current.Minor, Math.Max(0, current.Build))) return null;
        var arch = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "arm64" : "x64";
        var name = $"PDF-Viewer-Setup-{arch}.exe";
        Uri? installer = null;
        string? digest = null;
        foreach (var asset in release.GetProperty("assets").EnumerateArray())
        {
            if (asset.GetProperty("name").GetString() != name) continue;
            installer = new Uri(asset.GetProperty("browser_download_url").GetString()!);
            digest = asset.TryGetProperty("digest", out var value) ? value.GetString() : null;
        }
        if (installer is null || installer.Scheme != "https" || installer.Host != "github.com" ||
            !installer.AbsolutePath.StartsWith($"/{Repository}/releases/download/", StringComparison.Ordinal))
            throw new InvalidDataException("Bu mimari için güvenilir kurulum dosyası bulunamadı.");
        if (digest is null || !digest.StartsWith("sha256:") || digest.Length != 71 || !digest[7..].All(Uri.IsHexDigit))
            throw new InvalidDataException("Güncellemenin doğrulama bilgisi bulunamadı.");
        return new(latest, installer, digest[7..], name);
    }
    public async Task<string> DownloadAsync(AvailableUpdate update, CancellationToken token)
    {
        var directory = Path.Combine(Path.GetTempPath(), "PDF-Viewer", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, update.FileName);
        try
        {
            using var response = await client.GetAsync(update.Installer, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength > MaxInstallerBytes) throw new InvalidDataException("Kurulum dosyası çok büyük.");
            await using (var input = await response.Content.ReadAsStreamAsync(token))
            await using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
            {
                var buffer = new byte[81920]; long total = 0; int count;
                while ((count = await input.ReadAsync(buffer, token)) > 0)
                {
                    total += count;
                    if (total > MaxInstallerBytes) throw new InvalidDataException("Kurulum dosyası çok büyük.");
                    await output.WriteAsync(buffer.AsMemory(0, count), token);
                }
            }
            await using var file = File.OpenRead(path);
            var hash = Convert.ToHexString(await SHA256.HashDataAsync(file, token));
            if (!hash.Equals(update.Sha256, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Kurulum dosyası doğrulanamadı.");
            return path;
        }
        catch { Directory.Delete(directory, true); throw; }
    }
    public void Dispose() => client.Dispose();
}
