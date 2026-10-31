# Doğrulama durumu

## Tamamlanan kontroller

- Linux üzerinde .NET 10 SDK ile **14 test geçti**. Kaynak sınırları, gezinme, yeni sürüm algılama, güvenilir indirme adresi, özet zorunluluğu, ön sürüm atlama ve hatalı indirme reddi test edildi.
- GitHub Actions **41197706131**: Windows üzerinde **x64 ve ARM64 derlemesi ve Inno Setup EXE üretimi başarılı**.
- Aynı çalıştırmada **x64 uygulaması hem taşınabilir klasörden hem kurulumdan sonra açıldı**; pencere oluşumu ve erken kapanmama doğrulandı.
- NuGet taramasında o anda bildirilen savunmasız paket bulunmadı. Bu sonuç işletim sistemi PDF motorunun güvenliğine garanti vermez.
- 1, 100 ve 1000 sayfalık karma yönlü PDF, boş PDF ve bozuk PDF üreten betik çalıştırıldı. `pdfinfo` 1000 sayfalık belgenin yapısını okuyabildi; bu bir Windows görüntüleme testi değildir.

Açılış hatasının nedeni yayımlama çıktısındaki eksik ana arayüz kaynaklarıydı. Kaynak dosyaları yayımlama çıktısına alınır, ana kaynak dizini `resources.pri` olarak üretilir ve paketleme öncesi varlığı denetlenir.

## Henüz doğrulanmayanlar

CI Windows Server 2022 ortamındadır; gerçek Windows 10 ve Windows 11 cihazlarının yerine geçmez. ARM64 cihazda çalışma, kapsamlı PDF görüntüleme, DPI, erişilebilirlik, bellek ve performans testleri bekliyor. İlk sürüm kod imzasızdır. İki gerçek sürüm arasındaki yükseltme, ikinci sürümün kabul testinde doğrulanmalıdır; mevcut güncelleme testleri sahte HTTP yanıtları kullanır.

## Windows kabul planı

| Senaryo | Beklenen |
|---|---|
| 1 / 100 / 1000 sayfa ve karma yön | Doğru içerik, oranlar ve sayaç |
| Önceki/sonraki ve Enter ile sayfaya gitme | Gezinme sınırlar içinde |
| Yakınlaştırma, sığdırma ve pencere boyutu | Arayüz yanıt verir; doğru ölçekte görüntü |
| Boş, bozuk, şifreli veya erişilemeyen PDF | Anlaşılır hata; önceki belge korunur |
| 200 MB üzeri / 20.000 sayfa üzeri | Dosya açma reddedilir |
| Hızlı kaydırma, belge değiştirme, kapatma | Eski görsel veya nesne kullanımı hatası oluşmaz |
| %100 / %150 / %200 DPI, iki monitör, temalar | Okunabilir ve doğru ölçekte arayüz |
| Ağ yokken belge açma | Belge görüntüleme çalışır |
| Temiz Windows 10/11 cihazda EXE kurulumu | Ek .NET kurulumu istemeden açılır |
| İkinci sürümün uygulama içinden kurulması | Onay, doğrulama, sürüm yükseltme ve yeniden açılış başarılı |

İlk sayfa süresi ve bellek kullanımı henüz ölçülmedi. En az 20 belge aç/kapat döngüsü, uzun kaydırma ve gerçek bozuk/şifreli belge testleri yapılmalıdır.
