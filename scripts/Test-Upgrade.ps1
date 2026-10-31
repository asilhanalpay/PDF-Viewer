$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$installDir = Join-Path $env:RUNNER_TEMP 'PDF Viewer Upgrade Test'
$oldSetup = Join-Path $env:RUNNER_TEMP 'PDF-Viewer-Previous.exe'
Invoke-WebRequest 'https://github.com/asilhanalpay/PDF-Viewer/releases/download/V0.0.1/PDF-Viewer-Setup-x64.exe' -OutFile $oldSetup
$old = Start-Process $oldSetup -ArgumentList '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/DIR=`"$installDir`"" -Wait -PassThru
if ($old.ExitCode -ne 0) { throw 'Önceki sürüm kurulamadı' }
$exe = Join-Path $installDir 'PdfViewer.exe'
$previousVersion = (Get-Item $exe).VersionInfo.FileVersion
& "$PSScriptRoot/SmokeTest.ps1" -ExePath $exe
$registryPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{C40E5C56-8DFB-4094-A4B9-A3BBA5BC0ED2}_is1'
$before = Get-ItemProperty $registryPath
Set-Content (Join-Path $installDir 'upgrade-marker.txt') 'preserve'
$setup = Start-Process "$root/artifacts/PDF-Viewer-Setup-x64.exe" -ArgumentList '/SILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/CLOSEAPPLICATIONS', '/UPDATE=1', "/DIR=`"$installDir`"" -PassThru
if (!$setup.WaitForExit(120000)) { throw 'Güncelleme kurulumu zaman aşımına uğradı' }
if ($setup.ExitCode -ne 0) { throw 'Güncelleme kurulamadı' }
[xml]$project = Get-Content "$root/src/PdfViewer/PdfViewer.csproj"
$version = $project.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if ((Get-Item $exe).VersionInfo.FileVersion -ne "$version.0") { throw 'Dosya sürümü yenilenmedi' }
if ($previousVersion -eq (Get-Item $exe).VersionInfo.FileVersion) { throw 'Eski sürüm kaldı' }
if (!(Test-Path (Join-Path $installDir 'upgrade-marker.txt'))) { throw 'Kurulum klasörü korunmadı' }
$after = Get-ItemProperty $registryPath
if ($before.InstallLocation -ne $after.InstallLocation -or $before.UninstallString -ne $after.UninstallString) { throw 'Ayrı kurulum kaydı oluştu' }
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
