# MedRandevu — Hastane / Klinik Randevu Sistemi

Nesneye Dayalı Programlama dersi proje ödevi (Seçenek A) için geliştirilmiş, rol tabanlı bir
hastane/klinik randevu yönetim sistemidir. ASP.NET Core MVC ile yazılmıştır.

**Hazırlayanlar:** İlaydanur Tan - İmhan Acar

## Özellikler

### Hasta
- Kayıt olma ve giriş yapma
- Kurum → bölüm → doktor → hizmet → tarih → saat sırasıyla randevu alma
- Sadece doktorun uygun olduğu saatler listelenir
- Randevularını görüntüleme ve iptal etme

### Doktor
- Haftalık çalışma saatlerini (uygunluk zaman dilimleri) tanımlama ve silme
- Gelen randevuları onaylama veya gerekçe yazarak reddetme
- Onaylanan randevuyu tamamlandı olarak işaretleme

### Yönetici (Admin)
- Hastane ve klinik tanımlama (çalışma saatleri, hafta sonu durumu, kuruma özel alanlar)
- Kuruma bölüm ekleme
- Bölümlere hizmet ekleme (süre ve ücret ile)
- Doktor ekleme (uzmanlık alanı, bölüm, muayene ücreti)
- Hasta, doktor ve tüm randevuların listelenmesi; randevu iptali

### İş kuralları
- Randevu oluştururken **çakışma kontrolü** yapılır.
- Hastalar yalnızca doktorun tanımladığı **uygunluk zaman dilimlerine** randevu alabilir.
- Randevu **onay mekanizmasına** sahiptir: Bekliyor → Onaylandı / Reddedildi → Tamamlandı, ayrıca İptal.
- Randevuda işlem, süre, ücret, doktor ve tarih/saat bilgisi saklanır.

## Teknolojiler

- C# / ASP.NET Core MVC (.NET 10)
- Entity Framework Core + SQLite
- ASP.NET Core Identity (rol tabanlı yetkilendirme)
- Razor View'lar, Bootstrap 5, Bootstrap Icons

## Kurulum ve çalıştırma

Gereksinim: [.NET SDK](https://dotnet.microsoft.com/download) (proje `net10.0` hedefler).

```bash
git clone <depo-adresi>
cd <depo-klasoru>/HastaneRandevu
dotnet run
```

Uygulama açılırken veritabanı (`hastane_randevu.db`) ve demo veriler otomatik oluşturulur.
Konsolda yazan adresi (örneğin `http://localhost:5024`) tarayıcıda aç.

Veritabanını sıfırlamak için uygulamayı durdurup `hastane_randevu.db` dosyasını silmen yeterlidir.

## Demo hesapları

| Rol | E-posta | Şifre |
|---|---|---|
| Yönetici | admin@hastane.com | Admin123! |
| Doktor | ahmet.yilmaz@hastane.com | Doktor123! |
| Hasta | hasta@hastane.com | Hasta123! |

## Proje yapısı

```
HastaneRandevu/
├── Controllers/        Auth, Hasta, Doktor, Admin, Home controller'ları
├── Data/               AppDbContext, IRepository / Repository, SeedData
├── Models/
│   ├── Domain/         Person, ApplicationUser, Hasta, Doktor, Admin,
│   │                   SaglikKurumu, Hastane, Klinik, Bolum, Hizmet,
│   │                   UygunlukZamanDilimi, Randevu
│   ├── Enums/          UserRole, AppointmentStatus, KurumTuru, HaftaninGunu
│   └── ViewModels/     Form ve panel modelleri
├── Services/           Servis arayüzleri ve implementasyonları
├── Views/              Razor sayfaları
└── wwwroot/            CSS ve statik dosyalar
```

## OOP ilkeleri ve tasarım desenleri

| İlke / Desen | Projede nerede |
|---|---|
| Kalıtım | `Person` → `ApplicationUser` → `Hasta`, `Doktor`, `Admin` |
| Kalıtım ve çok biçimlilik | `SaglikKurumu` → `Hastane`, `Klinik`; kuruma özel alanlar ve `KurumTuruAdi` türe göre değişir |
| Soyutlama / arayüz | `IRandevuServisi`, `IKurumServisi`, `IKullaniciServisi` ve implementasyonları |
| Bağımlılık enjeksiyonu | Servislerin `Program.cs` içinde arayüz → sınıf olarak kaydedilmesi |
| Factory deseni | `KullaniciFactory` ile kurum türüne göre nesne oluşturma |
| Repository deseni | `Data/IRepository` ve `Data/Repository` |
| Kapsülleme | Randevu kuralları controller'da değil servis katmanında tutulur |

## Lisans

Bu proje ders ödevi olarak hazırlanmıştır.