# Geliştirme rehberi

C# / .NET 10 / WinUI 3 ile çevrim dışı Windows PDF görüntüleyici temel sürümü.
**Durum:** İlk sürümün kaynak kodu hazırlanmıştır. Windows arayüzü ve dağıtım paketi Windows üzerinde henüz doğrulanmamıştır; üretime hazır sürüm değildir.

## Temel sürüm

Yerel PDF açma, sanallaştırılmış dikey sayfa listesi, önceki/sonraki sayfa, numarayla gitme (Enter), yakınlaştırma, genişliğe sığdırma ve sistem/açık/koyu tema. Kullanıcı arayüzündeki uygulama adı PDF Viewer'dır. PDF düzenleme, dönüştürme, üyelik veya ağ servisi yoktur.

Görüntüler yalnızca gerçekleşen ListView öğelerinde tutulur; öğe geri dönüştürüldüğünde bırakılır. PDF sayfaları tek bir işlem kuyruğunda, gerektiğinde rasterleştirilir. Disk önbelleği yoktur. Sayfa görselleri en fazla 2560 piksel kenar uzunluğundadır; yüksek yakınlaştırmada keskinlik bu sınırla sınırlıdır. Sayfa boy oranı ilk render sırasında öğrenilir; karma boyutlu belgelerde ilk kaydırma sırasında konum değişebilir. Metin seçimi ve ekran okuyucuyla PDF metni okuma henüz yoktur.

## Windows'ta derleme

- Windows 10 **2004 / build 19041** veya üzeri; Windows 11. Güvenlik güncellemeleri alınan bir Windows sürümü gereklidir (Windows 10 için uygun ESU/LTSC desteğini değerlendirin).
- Visual Studio 2026 veya uyumlu MSBuild, .NET 10 SDK, WinUI uygulama geliştirme araçları ve Windows SDK 10.0.19041 veya üzeri.
- x64 veya ARM64. x86 bu sürümde hedeflenmez.

```powershell
dotnet restore src/PdfViewer/PdfViewer.csproj
dotnet build src/PdfViewer/PdfViewer.csproj -c Debug -p:Platform=x64 -r win-x64
# Üretilen bin/Debug altındaki PDF Viewer uygulamasını çalıştırın.
dotnet test tests/PdfViewer.Core.Tests -c Release
./scripts/Publish.ps1 -Runtime win-x64
```

Son komut `artifacts/PDF-Viewer-win-x64.zip` taşınabilir paketini oluşturur. ZIP içeriğini birlikte çıkartın ve `PdfViewer.exe` çalıştırın; tek EXE dağıtımı değildir. .NET ve Windows App SDK self-contained paketlenir. Windows bileşeni olan PDF motoru pakete kopyalanmaz. İmzalama sertifikası projede yoktur. MSIX, kurulum deneyimi ve dosya ilişkilendirme sonraki sürüme bırakılmıştır. Dağıtmadan önce bağımlılık taramasını inceleyin, Windows test matrisini tamamlayın ve kurumsal dağıtım için imzalayın. GitHub üzerinde ilk Windows derlemesi başarısız olmuştur; hataların giderilmesi ve yeniden doğrulama beklenmektedir.

## Yapı

- `src/PdfViewer`: WinUI görünümleri, MVVM durumu ve PDF oturumu.
- `src/PdfViewer.Core`: Windows'tan bağımsız boyut, gezinme ve kaynak sınırları.
- `tests`: kaynak sınırlarının birim testleri.
- `docs`: motor seçimi, güvenlik sınırları ve Windows test planı.
- `scripts`: test belgesi üretimi ve Windows paketleme.

## Aşamalar

1. Ortam ve altyapı seçimi tamamlandı.
2. Temel görüntüleyici kaynakları oluşturuldu; Windows derleme ve kullanıcı testi bekliyor.
3. Temel sürümün Windows 10/11 üzerinde kararlılığı doğrulanacak.
4. Sonra sekmeler, metin arama, yazdırma, son dosyalar, kısayollar, sürükle-bırak ve dosya ilişkilendirme ayrı değişiklikler halinde eklenecek. Windows.Data.Pdf metin çıkarma sunmaz; arama için motor seçimi yeniden değerlendirilecek.

Bu ortamın doğrulama sonuçları için `docs/VALIDATION.md` dosyasına bakın.
