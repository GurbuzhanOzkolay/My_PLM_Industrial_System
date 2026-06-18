Industrial PLM and Data Analysis System
Bu proje, bir endüstriyel işletmenin Ürün Yaşam Döngüsü Yönetimi (PLM) ve Üretim Verimliliği Analizi (OEE) süreçlerini dijitalleştirmek amacıyla geliştirilmiş kapsamlı bir kurumsal yönetim prototipidir.

📝 Staj Hakkında
Bu çalışma, [Ostim Teknik Üniversitesi] [Yazılım Mühendisliği] öğrencisi olarak 2. Sınıf Yaz Stajım döneminde (Sampa Otomotivde) gerçekleştirilmiştir. Staj sürecinde endüstriyel veritabanı yönetimi, hiyerarşik ürün ağacı (BOM) mimarisi ve çok katmanlı backend geliştirme süreçlerinde profesyonel deneyim kazanılmıştır.

🛠 Kullanılan Teknolojiler
Backend: C#, ASP.NET Core Web API, Entity Framework Core

Veritabanı: MS SQL Server (İlişkisel Veri Modelleme)

Veri Analitiği: Python (Pandas) - OEE (Toplam Ekipman Etkinliği) Hesaplamaları

Mimari: Recursive (Öz Yinelemeli) Tree Structure, Mikro-servis tabanlı veri analitiği

Versiyon Kontrol: Git & GitHub

🚀 Projenin Temel Özellikleri
Dinamik Ürün Ağacı (BOM): Sonsuz kırılımlı montaj yapısını destekleyen Self-Referencing veritabanı tasarımı.

Otomatik Maliyet Hesaplama: Alt parçalarda yapılan fiyat değişikliklerinin, üst montaj (Parent) maliyetlerine recursive (öz yinelemeli) olarak otomatik yansıtılması.

OEE Analitiği: Fabrika üretim loglarının Python ile işlenerek Kullanılabilirlik, Performans ve Kalite metriklerinin hesaplanması.

API Entegrasyonu: Karmaşık hiyerarşik verilerin JSON formatında frontend'e servis edilmesi.

🏗 Mimari Yapı
Proje, iş mantığı (C#) ve veri analitiği (Python) katmanlarını ortak bir veritabanı havuzunda buluşturan hibrit bir mimari ile kurgulanmıştır.

Örnek Hiyerarşik Veri Modeli:

Arka Aks (Ana Montaj)

Salıncak Gövdesi

Fren Kampanası

Fren Balataları

Fren Silindiri

📊 Özet
Bu proje ile endüstriyel bir işletmedeki ham verilerin anlamlı bir hiyerarşiye dönüştürülmesi ve üretim verimliliğinin matematiksel modellerle (OEE) izlenmesi hedeflenmiştir.

Geliştirici: [Gürbüzhan Özkolay]
LınkedIn[www.linkedin.com/in/gürbüzhan-özkolay-8964a0335]
