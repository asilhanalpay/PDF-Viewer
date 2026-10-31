# Kurulum ve güncelleme

Inno Setup 6 ile tek bir kurulum EXE'si üretilir. Uygulama mevcut kullanıcıya kurulur; yönetici yetkisi istemez. Aynı AppId ve kurulum yolu sonraki sürümlerde korunur. Uygulama, Windows'un yüklü uygulamalar listesinden kaldırılabilir.

## Uygulama içinden

**Güncelle** düğmesi GitHub'ın son kararlı sürümünü denetler. Yeni sürüm varsa kullanıcıya sorulur, uygun mimarinin EXE'si HTTPS ile indirilir ve GitHub varlık bilgisindeki SHA-256 özeti doğrulanır. Doğrulama bilgisi yoksa veya eşleşmezse kurulum başlatılmaz. Kullanıcı onayından sonra uygulama kapanır ve görünür kurulum sihirbazı açılır. Başarısız indirmeler silinir. Başarılı indirmeler geçici klasörde kalabilir; Windows geçici dosya temizliğiyle silinebilir.

Sürekli ağ bağlantısı, hesap girişi veya arka planda otomatik kurulum yoktur. Yalnızca bu düğme GitHub'a bağlanır. SHA-256 aktarım bütünlüğünü denetler; bağımsız kod imzası yerine geçmez. Kaynak deponun herkese açık olması gerekir.

## Yeni sürüm yayımlama

1. `src/PdfViewer/PdfViewer.csproj` içindeki `Version` değerini artırın (örneğin `0.0.2`).
2. `docs/SURUM-NOTLARI.md` içeriğini güncelleyin; Windows kabul testlerini tamamlayın.
3. Değişiklikleri gönderip sürümle aynı etiketi oluşturun: `git tag V0.0.2` ve `git push origin V0.0.2`.
4. Windows iş akışı test, derleme ve kurulum üretimini yapar; hepsi başarılıysa EXE'leri GitHub Releases üzerinde yayımlar. Uygulama artık bu sürümü bulur.

Kod imzalama sertifikası henüz sağlanmadığı için kurulumlar imzasızdır. Otomatik imzalama ve sertifika sırları ileride eklenmelidir. x64 kurulum ve pencere açılış testi CI'da; ARM64 çalışma testi fiziksel ARM64 cihazda yapılmalıdır. İki sürüm arasında gerçek yükseltme ilk sürümle tek başına doğrulanamaz; ikinci sürüm kabul testine dahildir.

Sürüm sırası **V0.0.1 → V0.0.2 → V0.0.3** şeklindedir. Eski `v0.1.0` kurulumu kullanılıyorsa numaralandırma değişimi için V0.0.1 kurulum dosyası bir kez elle çalıştırılmalıdır. Sonraki sürümler uygulama içinden denetlenebilir.
