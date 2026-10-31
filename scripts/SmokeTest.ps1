param([Parameter(Mandatory)][string]$ExePath)
$ErrorActionPreference = 'Stop'
$app = Start-Process $ExePath -PassThru
try {
    Start-Sleep -Seconds 8
    $app.Refresh()
    if ($app.HasExited) {
        Get-Content "$env:LOCALAPPDATA/PDF Viewer/startup-error.log" -ErrorAction SilentlyContinue
        Get-Content "$env:LOCALAPPDATA/PDF Viewer/resource-error.log" -ErrorAction SilentlyContinue
        throw "Uygulama erken kapandı: $($app.ExitCode)"
    }
    if ($app.MainWindowHandle -eq 0) { throw 'Uygulama penceresi oluşmadı' }
} finally {
    if (!$app.HasExited) { Stop-Process -Id $app.Id -Force }
}
