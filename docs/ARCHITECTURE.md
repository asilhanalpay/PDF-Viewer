# Motor seçimi ve güvenlik

| Seçenek | WinUI 3 | Lisans / dağıtım | Güvenlik ve kapsam | Karar |
|---|---|---|---|---|
| Windows.Data.Pdf | Windows SDK WinRT API; sayfalar BitmapImage ile gösterilir | Windows sistem API'si; ayrı PDF motoru redistribüsyonu yok. Windows ve SDK koşulları geçerli | OS güvenlik güncellemeleriyle servis edilir. Raster görüntüleme; metin çıkarma yok | Temel sürüm |
| PDFium | Yerel DLL ve C# sarmalayıcı gerekir | BSD tarzı ana lisans ve üçüncü taraf bildirimleri; mimariye özel DLL | Aktif upstream; seçilen wrapper'ın güncelleme temposu ayrıca incelenmeli. Native sürüm güncellemeleri uygulamaya aittir | Arama aşamasında aday |
| MuPDF | Native entegrasyon gerekir | AGPL veya ticari lisans; kapalı kaynak dağıtımda hukuki değerlendirme gerekir | Aktif upstream; uygulama native güncellemeleri dağıtmalıdır | Bu sürüme alınmadı |

Karşılaştırma ürün seçimi içindir; tüm alternatiflerin sürüm bazında güvenlik denetimi yapılmış olduğu anlamına gelmez. Microsoft.WindowsAppSDK 1.8.260921001 NuGet sürümü sabitlendi; yayın öncesi destek takvimi, güvenlik bildirimleri ve lisans dosyaları yeniden kontrol edilmelidir. .NET 10 LTS kullanılır.

Kaynaklar:
- https://learn.microsoft.com/en-us/uwp/api/windows.data.pdf.pdfdocument
- https://learn.microsoft.com/en-us/uwp/api/windows.data.pdf.pdfpage.rendertostreamasync
- https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/deploy-unpackaged-apps
- https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/servicing
- https://pdfium.googlesource.com/pdfium/+/refs/heads/main/LICENSE
- https://mupdf.com/licensing

## İşlem sınırları

Uygulama güncelleme denetimi ve indirmesi için GitHub’a bağlanır; telemetri SDK’sı içermez, PDF içi bağlantıları açmaz, gömülü dosya/form/JavaScript işlemez. Yalnızca Windows PDF raster API'si çağrılır. Motorun kendisinde oluşabilecek bir güvenlik açığına karşı uygulama düzeyinde garanti verilmez: motor bu sürümde ayrı bir sandbox sürecinde değildir. Yerel Windows dosya seçicisi kullanıcı tarafından seçilen bulut sağlayıcılarına/UNC yollarına erişebilir; uygulama bu sistem davranışını engellemez.

200 MB ve 20.000 sayfa sınırı, 2560 piksel raster kenar sınırı ve tek eşzamanlı native render kullanılır. Bunlar açılmış PDF'nin karmaşıklığını veya motorun dahili belleğini kesin sınırlandırmaz. Şifreli belgeler parola istemeden anlaşılır genel hata ile reddedilir. Açma başarısız olduğunda önceki belge korunur. Render hatası ilgili sayfada gösterilir. Render iptalleri ve oturum kapatma aynı kilit altında düzenlenir.

Kalıcı belge önbelleği ve son dosyalar listesi tutulmaz. Dosya adı sadece uygulama belleğinde gösterilir. Uygulama kaynak PDF'ye yazmaz. Çökme kurtarma, sert zaman aşımı ve işlem izolasyonu gelecekte ayrıca değerlendirilmelidir.
