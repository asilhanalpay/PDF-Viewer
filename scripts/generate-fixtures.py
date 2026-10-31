"""Deterministic synthetic PDFs. No external dependency; run before Windows manual tests."""
from pathlib import Path
import hashlib, struct
out = Path(__file__).resolve().parents[1] / 'artifacts' / 'fixtures'
out.mkdir(parents=True, exist_ok=True)
def rc4(key, data):
    state = list(range(256)); j = 0
    for i in range(256):
        j = (j + state[i] + key[i % len(key)]) % 256
        state[i], state[j] = state[j], state[i]
    i = j = 0; result = bytearray()
    for value in data:
        i = (i + 1) % 256; j = (j + state[i]) % 256
        state[i], state[j] = state[j], state[i]
        result.append(value ^ state[(state[i] + state[j]) % 256])
    return bytes(result)
def create(count, encrypted=False, rotated=False):
    padding = bytes.fromhex('28bf4e5e4e758a4164004e56fffa01082e2e00b6d0683e802f0ca9fe6453697a')
    identifier = hashlib.md5(b'PDF Viewer test fixture').digest()
    owner = rc4(hashlib.md5((b'owner'+padding)[:32]).digest()[:5], (b'secret'+padding)[:32])
    file_key = hashlib.md5((b'secret'+padding)[:32] + owner + struct.pack('<i', -4) + identifier).digest()[:5]
    user = rc4(file_key, padding)
    objects = [b'<< /Type /Catalog /Pages 2 0 R >>',
               ('<< /Type /Pages /Count %d /Kids [%s] >>' % (count, ' '.join(f'{3+i*2} 0 R' for i in range(count)))).encode()]
    font_id = 3 + count * 2
    for i in range(count):
        width, height = (842, 595) if i % 2 else (595, 842)
        rotation = 90 if rotated and i == 2 else 0
        objects.append(f'<< /Type /Page /Parent 2 0 R /Rotate {rotation} /MediaBox [0 0 {width} {height}] /Resources << /Font << /F1 {font_id} 0 R >> >> /Contents {4+i*2} 0 R >>'.encode())
        content = f'BT /F1 24 Tf 48 500 Td (PDF Viewer - Page {i+1}) Tj ET'.encode()
        if encrypted:
            object_key = hashlib.md5(file_key + struct.pack('<I', 4+i*2)[:3] + b'\0\0').digest()[:10]
            content = rc4(object_key, content)
        objects.append(f'<< /Length {len(content)} >>\nstream\n'.encode() + content + b'\nendstream')
    objects.append(b'<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>')
    if encrypted:
        objects.append(f'<< /Filter /Standard /V 1 /R 2 /Length 40 /O <{owner.hex()}> /U <{user.hex()}> /P -4 >>'.encode())
    result = bytearray(b'%PDF-1.4\n'); offsets = [0]
    for i, obj in enumerate(objects, 1):
        offsets.append(len(result)); result.extend(f'{i} 0 obj\n'.encode()+obj+b'\nendobj\n')
    xref = len(result)
    result.extend(f'xref\n0 {len(offsets)}\n0000000000 65535 f \n'.encode())
    for offset in offsets[1:]: result.extend(f'{offset:010d} 00000 n \n'.encode())
    encryption = f' /Encrypt {len(objects)} 0 R /ID [<{identifier.hex()}> <{identifier.hex()}>]' if encrypted else ''
    result.extend(f'trailer\n<< /Size {len(offsets)} /Root 1 0 R{encryption} >>\nstartxref\n{xref}\n%%EOF\n'.encode())
    name = 'encrypted.pdf' if encrypted else 'rotated.pdf' if rotated else f'{count}-pages.pdf'
    (out/name).write_bytes(result)
for count in (1, 100, 1000): create(count)
create(1, encrypted=True)
create(3, rotated=True)
(out/'broken.pdf').write_bytes(b'%PDF-1.7\ninvalid object')
(out/'empty.pdf').write_bytes(b'')
print(out)
