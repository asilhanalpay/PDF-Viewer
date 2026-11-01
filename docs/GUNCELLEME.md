# Kurulum ve güncelleme

Inno Setup 6 ile tek bir kurulum EXE'si üretilir. Uygulama mevcut kullanıcıya kurulur; yönetici yetkisi istemez. Aynı AppId ve kurulum yolu sonraki sürümlerde korunur. Uygulama, Windows'un yüklü uygulamalar listesinden kaldırılabilir.

## Uygulama içinden

Uygulama açılışta ve açık olduğu sürece dakikada bir GitHub'ın son kararlı sürümünü denetler. Yeni sürüm varsa **Yeni sürüm mevcut** bildirimi gösterilir ve tekrar eden denetim durdurulur; indirme sırasında en güncel sürüm yeniden sorgulanır. Denetim önce GitHub web adresinin yalnızca yanıt başlıklarını okur; yeni sürüm görülürse indirme bilgileri API’den alınır. Uygulama kapalıyken Windows bildirimi veya arka plan servisi çalışmaz. Çevrim dışı denetim hataları sessizce geçilir; PDF görüntüleme devam eder. **Güncelle** düğmesiyle elle de denetlenebilir.

Kullanıcı onayından sonra mimariye uygun kurulum dosyası HTTPS ile indirilir ve GitHub varlık bilgisindeki SHA-256 özeti doğrulanır. Uygulama kapanır; kurulum ilerleme ekranıyla mevcut klasördeki dosyaları yeniler ve uygulamayı yeniden açar. Aynı AppId, klasör ve kaldırma kaydı korunur; ikinci bir uygulama kurulmaz. Açık belgeler kapanır. Başarısız indirme mevcut uygulamayı değiştirmez.

Kurulumun tamamlanması için geçici dosya gerekir. Başarılı indirmeler geçici klasörde kalabilir ve Windows geçici dosya temizliğiyle silinebilir. SHA-256 bağımsız kod imzası yerine geçmez; kurulumlar henüz imzasızdır.

V0.0.1 kullananlar V0.0.2'yi mevcut **Güncelle** düğmesiyle alabilir; otomatik bildirimler V0.0.2'den itibaren çalışır.

## Yeni sürüm yayımlama

1. `src/PdfViewer/PdfViewer.csproj` içindeki `Version` değerini artırın (örneğin `0.0.3`).
2. `docs/SURUM-NOTLARI.md` içeriğini güncelleyin; Windows kabul testlerini tamamlayın.
3. Değişiklikleri gönderip sürümle aynı etiketi oluşturun: `git tag V0.0.3` ve `git push origin V0.0.3`.
4. Windows iş akışı test, derleme ve kurulum üretimini yapar; hepsi başarılıysa EXE'leri GitHub Releases üzerinde yayımlar. Uygulama artık bu sürümü bulur.

Kod imzalama sertifikası henüz sağlanmadığı için kurulumlar imzasızdır. Otomatik imzalama ve sertifika sırları ileride eklenmelidir. x64 kurulum ve pencere açılış testi CI'da; ARM64 çalışma testi fiziksel ARM64 cihazda yapılmalıdır. V0.0.1 → V0.0.2 yerinde yükseltmesi Windows CI üzerinde doğrulandı.

Sürüm sırası **V0.0.1 → V0.0.2 → V0.0.3** şeklindedir. Eski `v0.1.0` kurulumu kullanılıyorsa numaralandırma değişimi için V0.0.1 kurulum dosyası bir kez elle çalıştırılmalıdır. Sonraki sürümler uygulama içinden denetlenebilir.

## Korunacak güncelleme davranışı

- Otomatik denetim yalnızca bildirim gösterir. İndirme ve kurulum kullanıcı **Güncelle** onayı verdikten sonra başlar.
- Kurulum hedefi çalışan uygulamanın `AppContext.BaseDirectory` klasörüdür. Güncelleme sırasında kullanıcıdan yeniden klasör seçmesi istenmez; varsayılan konuma ikinci bir uygulama kurulmaz.
- `UpdateInstaller.CreateStartInfo` ile aynı kurulum klasörü `/DIR` üzerinden aktarılır. Inno Setup `AppId` ve `UsePreviousAppDir` korunur; kaldırma kaydı değişmez.
- Doğrulanan dosya mevcut kurulumun üzerine uygulanır. Uygulama kapanır ve aynı yerden yeniden açılır.
- Windows CI, son uygun kararlı sürümden yükseltmeyi özel, boşluk ve Türkçe karakter içeren bir klasörde test eder. Kurulumu başlatırken uygulamanın gerçek `UpdateInstaller` kodunu kullanır. Klasör, dosya sürümü, kaldırma kaydı ve yeniden açılış kontrolleri geçmeden sürüm yayımlanmaz.
