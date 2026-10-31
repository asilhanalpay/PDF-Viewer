# Doğrulama durumu

## Tamamlanan kontroller

- Linux üzerinde .NET 10 SDK ile **16 test geçti**. Kaynak sınırları, gezinme, yeni sürüm algılama, güvenilir indirme adresi, özet zorunluluğu, ön sürüm atlama ve hatalı indirme reddi test edildi.
- GitHub Actions **41197706131**: Windows üzerinde **x64 ve ARM64 derlemesi ve Inno Setup EXE üretimi başarılı**.
- Aynı çalıştırmada **x64 uygulaması hem taşınabilir klasörden hem kurulumdan sonra açıldı**; pencere oluşumu ve erken kapanmama doğrulandı.
- NuGet taramasında o anda bildirilen savunmasız paket bulunmadı. Bu sonuç işletim sistemi PDF motorunun güvenliğine garanti vermez.
- 1, 100 ve 1000 sayfalık karma yönlü PDF, boş PDF ve bozuk PDF üreten betik çalıştırıldı. `pdfinfo` 1000 sayfalık belgenin yapısını okuyabildi; bu bir Windows görüntüleme testi değildir.

Açılış hatasının nedeni yayımlama çıktısındaki eksik ana arayüz kaynaklarıydı. Kaynak dosyaları yayımlama çıktısına alınır, ana kaynak dizini `resources.pri` olarak üretilir ve paketleme öncesi varlığı denetlenir.

## Henüz doğrulanmayanlar

CI Windows Server 2022 ortamındadır; gerçek Windows 10 ve Windows 11 cihazlarının yerine geçmez. ARM64 cihazda çalışma, kapsamlı PDF görüntüleme, DPI, erişilebilirlik, bellek ve performans testleri bekliyor. İlk sürüm kod imzasızdır. Windows CI üzerinde V0.0.1 → V0.0.2 yerinde yükseltmesi doğrulandı. Güncelleme metaverisi ve dosya doğrulaması birim testleri sahte HTTP yanıtları kullanır; bildirime tıklayarak tüm ağ akışının kullanıcı testi ayrıca yapılmalıdır.

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

## Sürüm numaralandırması

V0.0.1 ile yeni numaralandırma başlatıldı. Güncelleme ayrıştırıcısı hem `V` hem `v` önekini kabul eder; bu uyumluluk testi de geçti. Eski v0.1.0 kurulumu için ilk geçiş elle yapılmalıdır.

V0.0.1 sürümü için **41198484239** çalıştırmasında 15 test, her iki mimarinin paket üretimi ve x64 kurulum/açılış testi geçti; sürüm yayımlandı.

## V0.0.2

**41200028055** çalıştırmasında 16 test, x64/ARM64 paket üretimi ve x64 yerinde güncelleme testi geçti. Gerçek V0.0.1 kuruldu; V0.0.2 aynı klasöre uygulandı. Dosya sürümü 0.0.2.0 oldu, kaldırma kaydı ve klasördeki deneme dosyası korundu, uygulama kendiliğinden yeniden açıldı. ARM64 üzerinde yerinde güncelleme çalıştırılmadı. İlk çalıştırma 41199698597, testin kurulumun açtığı uygulamayı da beklemesi nedeniyle iptal edildi; test düzeltildikten sonra yeniden doğrulandı.
