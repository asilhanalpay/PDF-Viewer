# Uygulama ikonu

`icon-source-original.png`, kullanıcının verdiği ilk görseldir. `icon-source.png`, aynı siyah-beyaz kimlik korunarak belge ve PDF yazısı büyütülmüş ana çizimdir. `icon-source-small.png`, küçük boyutlarda okunaklılık için daha kalın ve sade hazırlanmış uyumlu çizimdir. Yeni çizimler mevcut ikon referans alınarak görüntü düzenleme aracıyla hazırlanmıştır.

`src/PdfViewer/Assets/PdfViewer.ico` dosyası **14 gerçek boyut** içerir: 16, 20, 24, 28, 32, 40, 48, 56, 64, 72, 96, 128, 192 ve 256 piksel. 16–96 piksel karelerinde küçük boyut çizimi, 128–256 pikselde ana çizim kullanılır. Dış köşeler şeffaftır. Windows'un görev çubuğu alanı değiştirilmez; içindeki belge ve yazı daha fazla alan kaplar.

Kurulum için `installer/assets` içinde farklı DPI/font ölçülerine uygun 58–232 piksel başlık logoları ve 164:314 oranını koruyan 820×1570 piksel yan panel bulunur. Yan panelde logo oranı değiştirilmeden yerleştirilir. Kaynaklar: [küçük logo](https://jrsoftware.org/ishelp/topic_setup_wizardsmallimagefile.htm), [yan panel](https://jrsoftware.org/ishelp/topic_setup_wizardimagefile.htm).

Yalnızca varlıkları yeniden üretmek için Pillow kurulu geliştirme ortamında `python scripts/build-icon.py` çalıştırılır. Pillow uygulamanın çalışma bağımlılığı değildir. İkon çizimini değiştirmek için önce kaynak PNG'ler güncellenir; betik yalnızca Windows boyutlarına ve kurulum düzenine aktarır.

`ApplicationIdentity.AppId`, Inno Setup `AppUserModelID` ve kurulum `AppId` sürümler arasında korunur. Mevcut kurulumun üzerine güncelleme davranışı değiştirilmemiştir. Windows eski sabitlenmiş ikonu önbellekte tutarsa eski sabitlemeyi kaldırıp yeni Başlat kısayolunu yeniden sabitleyin.
