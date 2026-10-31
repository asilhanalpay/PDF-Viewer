#ifndef AppVersion
  #define AppVersion "0.0.4"
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
Name: "{autoprograms}\PDF Viewer"; Filename: "{app}\PdfViewer.exe"
[Run]
Filename: "{app}\PdfViewer.exe"; Description: "PDF Viewer uygulamasını başlat"; Flags: nowait postinstall skipifsilent

Filename: "{app}\PdfViewer.exe"; Flags: nowait; Check: IsUpdate

[Code]
function IsUpdate: Boolean;
begin
  Result := ExpandConstant('{param:UPDATE|0}') = '1';
end;
