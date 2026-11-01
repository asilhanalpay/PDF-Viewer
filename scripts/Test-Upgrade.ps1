param([string]$PreviousTag = '')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$installDir = Join-Path $env:RUNNER_TEMP 'Özel Klasör/PDF Viewer Upgrade Test'
$installDir = [IO.Path]::GetFullPath($installDir)
[xml]$project = Get-Content "$root/src/PdfViewer/PdfViewer.csproj"
$version = $project.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
$targetVersion = [version]$version
$oldSetup = Join-Path $env:RUNNER_TEMP 'PDF-Viewer-Previous.exe'
if (!$PreviousTag) {
    # Use the newest stable predecessor, including when this test runs on the already-published version.
    $releases = Invoke-RestMethod 'https://api.github.com/repos/asilhanalpay/PDF-Viewer/releases?per_page=100'
    $candidates = foreach ($release in $releases) {
        $parsed = $null
        if (!$release.draft -and !$release.prerelease -and [version]::TryParse(($release.tag_name -replace '^[vV]', ''), [ref]$parsed) -and $parsed -lt $targetVersion) {
            [pscustomobject]@{ Tag = $release.tag_name; Version = $parsed }
        }
    }
    $PreviousTag = ($candidates | Sort-Object Version -Descending | Select-Object -First 1).Tag
}
if (!$PreviousTag) { throw 'Yükseltme testi için önceki kararlı sürüm bulunamadı' }
$encodedTag = [Uri]::EscapeDataString($PreviousTag)
Invoke-WebRequest "https://github.com/asilhanalpay/PDF-Viewer/releases/download/$encodedTag/PDF-Viewer-Setup-x64.exe" -OutFile $oldSetup
$old = Start-Process $oldSetup -ArgumentList '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/DIR=`"$installDir`"" -Wait -PassThru
if ($old.ExitCode -ne 0) { throw 'Önceki sürüm kurulamadı' }
$exe = Join-Path $installDir 'PdfViewer.exe'
$previousVersion = (Get-Item $exe).VersionInfo.FileVersion
& "$PSScriptRoot/SmokeTest.ps1" -ExePath $exe
if ((Get-Item $exe).VersionInfo.FileVersion -ne $previousVersion) { throw 'Açılışta kullanıcı onayı olmadan sürüm değişti' }
$registryPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{C40E5C56-8DFB-4094-A4B9-A3BBA5BC0ED2}_is1'
$before = Get-ItemProperty $registryPath
Set-Content (Join-Path $installDir 'upgrade-marker.txt') 'preserve'
# Exercise the application's actual launcher, rather than duplicating its command-line arguments.
$launcherSource = "using System.IO;" + [IO.File]::ReadAllText("$root/src/PdfViewer/Services/UpdateInstaller.cs")
Add-Type -TypeDefinition $launcherSource
$installerPath = [IO.Path]::GetFullPath("$root/artifacts/PDF-Viewer-Setup-x64.exe")
$startInfo = [PdfViewer.Services.UpdateInstaller]::CreateStartInfo($installerPath, $installDir)
$setup = [Diagnostics.Process]::Start($startInfo)
if (!$setup.WaitForExit(120000)) { throw 'Güncelleme kurulumu zaman aşımına uğradı' }
if ($setup.ExitCode -ne 0) { throw 'Güncelleme kurulamadı' }
if ((Get-Item $exe).VersionInfo.FileVersion -ne "$version.0") { throw 'Dosya sürümü yenilenmedi' }
if ($previousVersion -eq (Get-Item $exe).VersionInfo.FileVersion) { throw 'Eski sürüm kaldı' }
if (!(Test-Path (Join-Path $installDir 'upgrade-marker.txt'))) { throw 'Kurulum klasörü korunmadı' }
$after = Get-ItemProperty $registryPath
if ($before.InstallLocation -ne $after.InstallLocation -or $before.UninstallString -ne $after.UninstallString) { throw 'Ayrı kurulum kaydı oluştu' }
if ([IO.Path]::TrimEndingDirectorySeparator($after.InstallLocation) -ne $installDir) { throw 'Uygulama başka klasöre kuruldu' }
Start-Sleep -Seconds 10
$running = Get-Process PdfViewer -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe }
if (!$running -or $running.MainWindowHandle -eq 0) { throw 'Güncelleme sonrası uygulama yeniden açılmadı' }
& "$PSScriptRoot/Assert-Icon.ps1" -ExePath $exe -ProcessId $running.Id
$shortcut = Join-Path ([Environment]::GetFolderPath('Programs')) 'PDF Viewer.lnk'
if (!(Test-Path $shortcut)) { throw 'Başlat menüsü kısayolu eksik' }
$shell = New-Object -ComObject Shell.Application
$item = $shell.NameSpace((Split-Path $shortcut -Parent)).ParseName((Split-Path $shortcut -Leaf))
if ($item.ExtendedProperty('System.AppUserModel.ID') -ne 'PDFViewer.Desktop') { throw 'Kısayol ve görev çubuğu uygulama kimliği eşleşmiyor' }
$running | Stop-Process -Force
Write-Output "Yerinde yükseltme başarılı: $previousVersion -> $version.0; aynı klasör ve kaldırma kaydı; otomatik yeniden açılış."
