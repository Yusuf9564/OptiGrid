# 🌐 OptiGrid — Version-LTS (Official Release)

OptiGrid, kararlı ve uzun vadeli destek sunan **Version-LTS** aşamasıyla birlikte nihai formuna kavuşmuştur. Önceki sürümlerdeki aktif geliştirme ve test süreçleri tamamlanmış; platform maksimum kararlılık, performans ve genişletilmiş simülasyon mekanikleriyle kararlı bir üretim ortamına hazır hale getirilmiştir.

---

## 🌟 LTS Sürümü ile Gelen Yenilikler ve İyileştirmeler

* **Gelişmiş Kamera Sistemleri & Araç Takibi:** Simülasyonun izlenebilirliğini artırmak adına yeni dinamik kamera açıları entegre edilmiştir. **Araç Takip Kamerası (Vehicle Tracking Camera)** mekanizması sayesinde, grid üzerindeki spesifik lojistik araçlarına anlık olarak odaklanılarak ($target\ follow$) sinematik ve teknik açılardan izleme yapılabilmektedir.
* **Atık Yönetimi & Kentsel Mobilya Üretim Zinciri:** Simülasyona akıllı şehir dinamikleri eklenmiştir. Araçların grid üzerindeki **çöp toplama (waste collection)** rotaları optimize edilmiş ve toplanan bu atık girdilerine bağlı olarak dinamik bir **kentsel mobilya üretimi (urban furniture production)** döngüsü kurulmuştur. Lojistik veri, doğrudan üretim ekonomisine bağlanmıştır.
* **Merkezi Duraklatma (ESC) Menüsü:** Simülasyon akışını anlık olarak kesintiye uğratmadan yönetebilmek için gelişmiş bir **ESC Duraklatma Menüsü** tasarlanmıştır. Bu menü üzerinden simülasyon durum kontrolü, konfigürasyon ayarları ve ana menüye güvenli geçişler kararlı hale getirilmiştir.
* **Eksiksiz Optimizasyon & Hata Arındırma:** Önceki sürümlerde gözlemlenen tüm teknik hatalar, bellek sızıntıları (memory leak) ve anlık FPS düşüşleri tamamen giderilmiştir. Simülasyon motoru en stabil haliyle çalışmaktadır.
* **Gelişmiş Karbon Emisyon Analizi:** Araçların katettiği lineer mesafeye ek olarak, grid mimarisi üzerindeki dönüş açıları ($Angular\ Displacement$) baz alınarak dinamik yakıt tüketimi ve $CO_2$ salınımı mikro düzeyde hatasız hesaplanmaktadır.
* **Geometrik Alan Verimlilik Analizi:** Üçgen, kare ve altıgen lojistik sistemlerin alan kullanım efektifliği ($Area\ Efficiency$) karşılaştırmalı istatistiksel algoritmalarla analiz paneline entegre edilmiştir.
* **Merkezi Analytics Dashboard:** Tüm geometrik modellerin gerçek zamanlı verileri (Rota uzunluğu, dolambaçlılık indeksi, karbon ayak izi) tek bir merkezi ekranda gelişmiş grafiklerle kıyaslanabilir hale getirilmiştir.

---

## 🎮 Kullanım Bilgileri & Kurulum

* **Kurulum:** Paylaşılan `Version-LTS.rar` dosyasını bilgisayarınızda bir klasöre çıkartıp `OptiGrid.exe` dosyasını doğrudan çalıştırmanız yeterlidir. Ek bir kurulum veya kütüphane gerektirmez.
* **Navigasyon:** Simülasyon sırasında dilediğiniz an `ESC` tuşuna basarak yeni duraklatma menüsüne erişebilir, kamera modları arasında geçiş yapabilir veya ana menüye dönüş yapabilirsiniz.

> 📌 **LTS Notu:** Bu sürüm, projenin testleri tamamlanmış, tüm kararsızlıkları temizlenmiş ve dökümante edilmiş en güncel, kararlı (Stable) nihai versiyonudur.
