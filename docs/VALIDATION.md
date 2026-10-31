# Doğrulama kaydı

## Bu ortamda yapılanlar

Linux x64, geçici olarak kurulan .NET SDK 10.0.403.

- `dotnet test tests/PdfViewer.Core.Tests`: **14 geçti, 0 başarısız**. Raster sınırları, gezinme ve geçersiz değerlerin yanı sıra yeni sürüm algılama, güvenilir indirme adresi, SHA-256 zorunluluğu, ön sürüm atlama ve hatalı dosyanın reddi kontrol edildi.
- `dotnet build src/PdfViewer -p:EnableWindowsTargeting=true -p:Platform=x64`: **başarısız**. Paket restore tamamlandı; WinUI `XamlCompiler.exe` Linux üzerinde `Exec format error` verdi. C# UI derlemesi ve XAML bağlamaları bu denemeyle doğrulanmış değildir.
- `python scripts/generate-fixtures.py`: 1, 100, 1000 sayfalık karma dikey/yatay PDF'ler, bozuk PDF ve boş PDF üretildi. `pdfinfo` 1000 sayfalık belgenin sayfa sayısını ve yapısını okuyabildi. Bu, Windows motoruyla görüntüleme testi değildir.
- NuGet güvenlik taraması (`EnableWindowsTargeting=true dotnet list src/PdfViewer/PdfViewer.csproj package --vulnerable --include-transitive`): mevcut kaynaklarda bildirilen savunmasız paket bulunmadı. Bu sonuç Windows PDF motoru için güvenlik garantisi değildir.
- XAML, proje dosyaları ve manifest XML yapıları ayrıştırıldı; bu kontrol WinUI XAML derlemesi yerine geçmez.
- Windows 10/11 UI, native PDF motoru, DPI, bellek, yazılımsal erişilebilirlik ve dağıtım testi **çalıştırılmadı**.
- Taşınabilir ZIP bu ortamda **üretilmedi**. Windows CI dosyası hazırlanmıştır; uzakta tetiklenmemiştir.

## Windows kabul matrisi — tamamı bekliyor

Her senaryoyu Windows 10 (desteklenen güncel build) ve Windows 11 üzerinde x64; ARM64 paketini ayrıca native ARM64 cihazda çalıştırın.

| Senaryo | Beklenen |
|---|---|
| 1 / 100 / 1000 sayfa | İlk sayfa açılır; hızlı dikey kaydırmada doğru sayfa ve sayaç |
| Karma yatay/dikey, taranmış, farklı fontlar | İçerik oranları doğru; yönler doğru |
| Önceki/sonraki, Enter ile 1/son/aralık dışı sayfa | Gezinme sınırlarda kalır |
| Yakınlaştırma, genişliğe sığdırma, pencere yeniden boyutlama | Görseller yeniden işlenir; UI yanıt verir |
| Boş, bozuk, parola korumalı, erişim izni olmayan, silinen PDF | Anlaşılır hata; önceki belge korunur |
| 200 MB üzeri ve 20.000 üzeri sayfa | Açma reddedilir; işlem kararlı kalır |
| Uzun kaydırma, belge değişimi, render sırasında kapatma | Eski sayfa gösterilmez; nesne kullanımı hatası oluşmaz |
| %100 / %150 / %200 DPI, iki monitör | Araçlar okunabilir; sayfalar doğru ölçekte |
| Açık/koyu/sistem, yüksek karşıtlık, klavye odağı | Kontroller erişilebilir ve okunabilir |
| Ağ devre dışıyken PDF açma | Ağ bağlantısı gerektirmez |
| ZIP'i temiz Windows VM'ye çıkarıp çalıştırma | Geliştirme aracı/.NET kurulumu olmadan başlar |

Task Manager veya Windows Performance Recorder ile ilk sayfa süresi, açma süresi ve working set kaydedilmeli. Aynı dosyada en az 10 ileri/geri kaydırma ve 20 aç/kapat döngüsünden sonra kalıcı artış incelenmeli. Sayısal performans hedefleri henüz ölçülmemiştir. Bozuk/şifreli gerçek belge ve adversarial PDF corpus testleri temel sürüm kabulünden önce gereklidir.

## Windows derleme güncellemesi

İlk Windows derlemesindeki `Grid.IsEnabled` XAML hatası giderildi. GitHub Actions çalıştırması `41196394127` üzerinde x64 ve ARM64 için Windows derlemesi ve Inno Setup EXE üretimi başarıyla tamamlandı. Kurulumlar kod imzasızdır. Bu sonuç kapsamlı PDF kullanıcı testi veya temiz Windows 10/11 cihaz doğrulaması değildir.
