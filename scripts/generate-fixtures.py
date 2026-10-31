"""Deterministic synthetic PDFs. No external dependency; run before Windows manual tests."""
from pathlib import Path
out = Path(__file__).resolve().parents[1] / 'artifacts' / 'fixtures'
out.mkdir(parents=True, exist_ok=True)
def create(count):
    objects = [b'<< /Type /Catalog /Pages 2 0 R >>',
               ('<< /Type /Pages /Count %d /Kids [%s] >>' % (count, ' '.join(f'{3+i*2} 0 R' for i in range(count)))).encode()]
    font_id = 3 + count * 2
    for i in range(count):
        width, height = (842, 595) if i % 2 else (595, 842)
        objects.append(f'<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {width} {height}] /Resources << /Font << /F1 {font_id} 0 R >> >> /Contents {4+i*2} 0 R >>'.encode())
        content = f'BT /F1 24 Tf 48 500 Td (PDF Viewer - Page {i+1}) Tj ET'.encode()
        objects.append(f'<< /Length {len(content)} >>\nstream\n'.encode() + content + b'\nendstream')
    objects.append(b'<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>')
    result = bytearray(b'%PDF-1.4\n'); offsets = [0]
    for i, obj in enumerate(objects, 1):
        offsets.append(len(result)); result.extend(f'{i} 0 obj\n'.encode()+obj+b'\nendobj\n')
    xref = len(result)
    result.extend(f'xref\n0 {len(offsets)}\n0000000000 65535 f \n'.encode())
    for offset in offsets[1:]: result.extend(f'{offset:010d} 00000 n \n'.encode())
    result.extend(f'trailer\n<< /Size {len(offsets)} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n'.encode())
    (out/f'{count}-pages.pdf').write_bytes(result)
for count in (1, 100, 1000): create(count)
(out/'broken.pdf').write_bytes(b'%PDF-1.7\ninvalid object')
(out/'empty.pdf').write_bytes(b'')
print(out)
