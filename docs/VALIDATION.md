# Doğrulama durumu — V0.0.4

[Windows doğrulaması ve yayımlama](https://github.com/asilhanalpay/PDF-Viewer/actions/runs/41248797962)

## Başarılı kontroller

- **33 birim testi:** kaynak ve gezinme sınırları, sayfa ölçüleri, yakınlaştırma yönü, güncelleme doğrulaması ve mevcut kurulumun hedeflenmesi.
- **11 gerçek Windows PDF testi:** 1/100/1000 sayfalık belgelerin ilk ve son sayfaları; portre, yatay ve döndürülmüş sayfalar; bozuk/boş/parola korumalı dosyalar; dosya boyutu ve sayfa dizini sınırları; arka arkaya iptal, yeniden görüntüleme ve oturum kapatma.
- Sayfa görüntüleri PNG olarak çözüldü; ölçüleri ve boş olmayan açık/koyu piksel içeriği denetlendi. Test belgeleri sentetiktir; tüm gerçek PDF türlerini temsil etmez.
- **x64 ve ARM64:** WinUI derlemesi ve EXE kurulum paketi üretimi başarılı.
- **Windows x64:** uygulama penceresi açıldı. Gerçek V0.0.1 kurulumu aynı klasörde V0.0.4'e yükseltildi; dosya sürümü, kaldırma kaydı, mevcut klasörün korunması ve otomatik yeniden açılış doğrulandı.
- NuGet taramasında mevcut kaynakların bildirdiği savunmasız uygulama paketi bulunmadı.

İlk Windows test çalıştırmasında test dosyalarının karışık yol ayraçları WinRT tarafından reddedildi. Yollar normalleştirildi; dosyaya erişim doğrulaması negatif PDF testlerinin dışına alındı. Başarısız çalıştırma başarı olarak sayılmadı.

## Düzeltilen sorunlar

Sayfa geometrisi artık ilk görüntülemede değişmez. Yeniden boyutlandırma olayları birleştirilir, okuma konumu korunmaya çalışılır ve görüntü yenilenirken eski görüntü tutulur. Ölçü/DPI değişmiyorsa hazır sayfa görüntüsü tekrar kullanılır. Native görüntüleme bitmeden sayfa ve akış kaynakları kapatılmaz. Sayfa numarası Enter anında doğrudan okunur; yakınlaştırma sınırlarında ters yönlü sıçrama önlenir.

## Henüz tamamlanmayan kontroller

CI ortamı **Windows Server 2022 x64** kullanır. Fiziksel Windows 10/11 ve ARM64 cihazlarında elle kullanım, farklı monitör/DPI, ekran okuyucu, uzun süreli bellek ve performans ölçümleri yapılmadı. Gerçek kullanıcı belgeleri, taranmış/görsel ağırlıklı PDF'ler ve geniş bozuk belge koleksiyonu ayrıca denenmelidir. Araç çubuğunun bütün etkileşimleri ve kaydırma/yeniden boyutlandırmanın görsel davranışı elle kontrol edilmeyi bekliyor. Bu testler her PDF ve cihazda hatasız çalışmayı garanti etmez.

## Tekrar çalıştırma

```powershell
dotnet test tests/PdfViewer.Core.Tests -c Release
python scripts/generate-fixtures.py
$env:PDF_VIEWER_FIXTURES = (Resolve-Path artifacts/fixtures).Path
dotnet test tests/PdfViewer.Windows.Tests -c Release
./scripts/Publish.ps1 -Runtime win-x64
```

Windows PDF testleri Windows'ta çalıştırılmalıdır. Paketleme betiği de bu testleri, uygulama açılış kontrolünü ve bağımlılık taramasını çalıştırır. Kurulumlar henüz kod imzasızdır.
