param([ValidateSet('win-x64','win-arm64')][string]$Runtime = 'win-x64')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$platform = if ($Runtime -eq 'win-arm64') { 'ARM64' } else { 'x64' }
$output = Join-Path $root "artifacts/PDF-Viewer-$Runtime"
dotnet test "$root/tests/PdfViewer.Core.Tests" -c Release
if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
dotnet list "$root/src/PdfViewer/PdfViewer.csproj" package --vulnerable --include-transitive
if ($LASTEXITCODE -ne 0) { throw 'Dependency audit failed' }
# Audit output must also be reviewed: this command does not fail on reported vulnerabilities.
dotnet publish "$root/src/PdfViewer/PdfViewer.csproj" -c Release -r $Runtime -p:Platform=$platform -p:PublishSingleFile=false -o $output
if ($LASTEXITCODE -ne 0) { throw 'Publish failed' }
if (!(Test-Path "$output/resources.pri")) {
    Get-ChildItem "$root/src/PdfViewer/bin" -Recurse -Include *.pri,*.xbf | Select-Object FullName
    throw 'Arayüz kaynak dizini eksik: resources.pri'
}
if ($Runtime -eq 'win-x64') { & "$root/scripts/SmokeTest.ps1" -ExePath "$output/PdfViewer.exe" }
Copy-Item "$root/README.md","$root/THIRD-PARTY-NOTICES.md" $output
Compress-Archive -Path "$output/*" -DestinationPath "$root/artifacts/PDF-Viewer-$Runtime.zip" -Force
$arch = if ($Runtime -eq 'win-arm64') { 'arm64' } else { 'x64' }
[xml]$project = Get-Content "$root/src/PdfViewer/PdfViewer.csproj"
$version = $project.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
$iscc = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
if (!(Test-Path $iscc)) { throw 'Inno Setup 6 bulunamadı' }
& $iscc "/DAppVersion=$version" "/DAppArch=$arch" "$root/installer/PdfViewer.iss"
if ($LASTEXITCODE -ne 0) { throw 'Kurulum paketi oluşturulamadı' }
