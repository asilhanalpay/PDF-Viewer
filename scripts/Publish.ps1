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
Copy-Item "$root/README.md","$root/THIRD-PARTY-NOTICES.md" $output
Compress-Archive -Path "$output/*" -DestinationPath "$root/artifacts/PDF-Viewer-$Runtime.zip" -Force
