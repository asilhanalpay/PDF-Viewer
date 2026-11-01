"""Export optical-size ICO frames and DPI-aware installer artwork. Regeneration needs Pillow."""
from pathlib import Path
from io import BytesIO
import struct
from PIL import Image

root = Path(__file__).resolve().parents[1]
target = root / "src/PdfViewer/Assets/PdfViewer.ico"
wizard = root / "installer/assets"
wizard.mkdir(parents=True, exist_ok=True)
sizes = (16, 20, 24, 28, 32, 40, 48, 56, 64, 72, 96, 128, 192, 256)
master = Image.open(root / "assets/icon-source.png").convert("RGBA")
small = Image.open(root / "assets/icon-source-small.png").convert("RGBA")
for image in (master, small):
    if image.width != image.height:
        raise ValueError("Icon artwork must be square; no automatic cropping is applied.")

# Each small frame has dedicated optical artwork; avoid simply shrinking a detailed large icon.
payloads = []
for size in sizes:
    source = small if size <= 96 else master
    frame = source.resize((size, size), Image.Resampling.LANCZOS)
    buffer = BytesIO()
    frame.save(buffer, format="PNG")
    payloads.append(buffer.getvalue())
header = struct.pack("<HHH", 0, 1, len(sizes))
offset = 6 + 16 * len(sizes)
entries = bytearray()
for size, payload in zip(sizes, payloads):
    entries.extend(struct.pack("<BBBBHHII", size if size < 256 else 0, size if size < 256 else 0, 0, 0, 1, 32, len(payload), offset))
    offset += len(payload)
target.write_bytes(header + entries + b"".join(payloads))

# Native Inno Setup header sizes, including pre-6.6 and current DPI/font metrics.
header_sizes = (58, 71, 77, 85, 97, 103, 112, 116, 124, 129, 143, 147, 159, 194, 232)
for size in header_sizes:
    small.resize((size, size), Image.Resampling.LANCZOS).save(wizard / f"Header{size}.png")
# Keep the 164:314 wizard panel ratio. Place the unchanged icon on a transparent layout canvas.
panel = Image.new("RGBA", (820, 1570), (0, 0, 0, 0))
logo = master.resize((740, 740), Image.Resampling.LANCZOS)
panel.alpha_composite(logo, ((panel.width-logo.width)//2, (panel.height-logo.height)//2))
panel.save(wizard / "BrandPanel.png")

with Image.open(target) as icon:
    assert icon.ico.sizes() == {(size, size) for size in sizes}
    for size in sizes:
        assert icon.ico.getimage((size,size)).size == (size,size)
print(f"{target}: {len(sizes)} frames; {len(header_sizes)} installer header sizes; 820x1570 brand panel")
