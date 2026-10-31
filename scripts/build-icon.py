"""Convert the supplied artwork to Windows ICO sizes; requires Pillow, only for regeneration."""
from pathlib import Path
from PIL import Image

root = Path(__file__).resolve().parents[1]
source = root / "assets/icon-source.png"
target = root / "src/PdfViewer/Assets/PdfViewer.ico"
sizes = (16, 20, 24, 32, 40, 48, 64, 96, 128, 256)
with Image.open(source) as image:
    if image.width != image.height:
        raise ValueError("The icon source must be square; no automatic cropping is applied.")
    image.convert("RGBA").save(target, format="ICO", sizes=[(size, size) for size in sizes])
with Image.open(target) as icon:
    assert icon.ico.sizes() == {(size, size) for size in sizes}
print(target)
