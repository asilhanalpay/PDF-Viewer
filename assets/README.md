# Uygulama ikonu

`icon-source.png`, kullanıcının verdiği görselin kırpılmadan ve çizimi değiştirilmeden PNG biçimine aktarılmış hâlidir. Görselin pikselleri korunur; fotoğraf üst verileri taşınmaz.

`src/PdfViewer/Assets/PdfViewer.ico`, 16, 20, 24, 32, 40, 48, 64, 96, 128 ve 256 piksel boyutlarını içerir. EXE'ye gömülür; pencere/görev çubuğu, kurulum ve Başlat kısayolu aynı ikonu kullanır.

Yalnızca ikonu yeniden üretmek için Pillow kurulu bir geliştirme ortamında `python scripts/build-icon.py` çalıştırılır. Pillow uygulamanın çalışma veya kurulum bağımlılığı değildir.

Görev çubuğu eşleştirmesi için `ApplicationIdentity.AppId` ve kurulumdaki `AppUserModelID` aynı tutulmalıdır. Windows eski sabitlenmiş ikonu önbellekte tutarsa eski sabitlemeyi kaldırıp yeni Başlat kısayolunu yeniden sabitleyin.
