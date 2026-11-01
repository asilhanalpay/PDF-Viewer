param([Parameter(Mandatory)][string]$ExePath, [int]$ProcessId = 0, [string]$ReferenceIconPath = '')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
if (!('PdfViewer.NativeIconCheck' -as [type])) {
    Add-Type @'
using System;
using System.Runtime.InteropServices;
namespace PdfViewer {
    public static class NativeIconCheck {
        [DllImport("user32.dll", CharSet=CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);
    }
}
'@
}
$iconPath = if ($ReferenceIconPath) { $ReferenceIconPath } else { Join-Path (Split-Path $ExePath -Parent) 'Assets/PdfViewer.ico' }
if (!(Test-Path $iconPath)) { throw 'Pencere ikonu kurulumda eksik' }
$expectedIcon = [System.Drawing.Icon]::new($iconPath, 32, 32)
$embeddedIcon = [System.Drawing.Icon]::ExtractAssociatedIcon($ExePath)
try {
    if (!$embeddedIcon) { throw 'EXE içinde uygulama ikonu bulunamadı' }
    $expected = $expectedIcon.ToBitmap()
    $actual = $embeddedIcon.ToBitmap()
    try {
        if ($expected.Size -ne $actual.Size) { throw 'EXE ikonunun boyutu beklenenden farklı' }
        for ($x=0; $x -lt $expected.Width; $x++) {
            for ($y=0; $y -lt $expected.Height; $y++) {
                if ($expected.GetPixel($x,$y).ToArgb() -ne $actual.GetPixel($x,$y).ToArgb()) {
                    throw 'EXE ikonu verilen görselle eşleşmiyor'
                }
            }
        }
    } finally { $expected.Dispose(); $actual.Dispose() }
} finally { $expectedIcon.Dispose(); if ($embeddedIcon) { $embeddedIcon.Dispose() } }
if ($ProcessId -ne 0) {
    $app = Get-Process -Id $ProcessId
    foreach ($kind in 0,1) {
        $handle = [PdfViewer.NativeIconCheck]::SendMessage($app.MainWindowHandle, 0x007F, [IntPtr]$kind, [IntPtr]::Zero)
        if ($handle -eq [IntPtr]::Zero) { throw 'Pencere/görev çubuğu için ikon atanmamış' }
    }
}
Write-Output "EXE ikonu kaynak görselle eşleşiyor: $ExePath"
if ($ProcessId -ne 0) { Write-Output 'Pencere/görev çubuğu ikonları atanmış.' }
