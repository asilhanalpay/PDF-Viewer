#ifndef AppVersion
  #define AppVersion "0.0.6"
#endif
#ifndef AppArch
  #define AppArch "x64"
#endif
[Setup]
AppId={{C40E5C56-8DFB-4094-A4B9-A3BBA5BC0ED2}
MinVersion=10.0.19041
AppName=PDF Viewer
AppVersion={#AppVersion}
DefaultDirName={localappdata}\Programs\PDF Viewer
DefaultGroupName=PDF Viewer
UsePreviousAppDir=yes
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
OutputDir=..\artifacts
OutputBaseFilename=PDF-Viewer-Setup-{#AppArch}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
WizardImageFile=assets\BrandPanel.png
WizardSmallImageFile=assets\Header58.png,assets\Header71.png,assets\Header77.png,assets\Header85.png,assets\Header97.png,assets\Header103.png,assets\Header112.png,assets\Header116.png,assets\Header124.png,assets\Header129.png,assets\Header143.png,assets\Header147.png,assets\Header159.png,assets\Header194.png,assets\Header232.png
WizardImageStretch=yes
SetupIconFile=..\src\PdfViewer\Assets\PdfViewer.ico
UninstallDisplayIcon={app}\PdfViewer.exe
CloseApplications=yes
RestartApplications=no
#if AppArch == "arm64"
ArchitecturesAllowed=arm64
ArchitecturesInstallIn64BitMode=arm64
#else
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
#endif
[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
[Files]
Source: "..\artifacts\PDF-Viewer-win-{#AppArch}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{autoprograms}\PDF Viewer"; Filename: "{app}\PdfViewer.exe"; IconFilename: "{app}\PdfViewer.exe"; AppUserModelID: "PDFViewer.Desktop"
[Run]
Filename: "{app}\PdfViewer.exe"; Description: "PDF Viewer uygulamasını başlat"; Flags: nowait postinstall skipifsilent

Filename: "{app}\PdfViewer.exe"; Flags: nowait; Check: IsUpdate

[Code]
function IsUpdate: Boolean;
begin
  Result := ExpandConstant('{param:UPDATE|0}') = '1';
end;
