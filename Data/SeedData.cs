using HastaneRandevu.Models.Domain;
using HastaneRandevu.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace HastaneRandevu.Data
{
    /// <summary>
    /// Sisteme gerçekçi demo verileri yükler.
    /// 2 Hastane/Klinik, 3 Bölüm, 5 Doktor, örnek hastalar ve randevular içerir.
    /// </summary>
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var db = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Veritabanı oluştur
            await db.Database.EnsureCreatedAsync();

            // Zaten veri varsa tekrar ekleme
            if (db.SaglikKurumlari.Any()) return;

            // ═══════════════════════════════════════════
            // 1. ROLLER
            // ═══════════════════════════════════════════
            var roller = new[] { "Admin", "Doktor", "Hasta" };
            foreach (var rol in roller)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                    await roleManager.CreateAsync(new IdentityRole(rol));
            }

            // ═══════════════════════════════════════════
            // 2. SAĞLIK KURUMLARI
            // ═══════════════════════════════════════════
            var sehirHastanesi = new Hastane
            {
                Ad = "Şehir Eğitim ve Araştırma Hastanesi",
                Adres = "Adapazarı Mah. Hastane Cad. No:1, Sakarya",
                Telefon = "0264 275 00 00",
                Email = "info@sehirhastanesi.gov.tr",
                WebSitesi = "www.sehirhastanesi.gov.tr",
                CalismaBaslangicSaati = "08:00",
                CalismaBitisSaati = "18:00",
                HaftasonuAcikMi = false,
                YatakKapasitesi = 450,
                AcilServisMevcut = true,
                YogunBakimMevcut = true,
                AktifMi = true
            };

            var saglikKlinigi = new Klinik
            {
                Ad = "Özel Sağlık Kliniği",
                Adres = "Mithatpaşa Mah. Klinik Sk. No:7, Sakarya",
                Telefon = "0264 300 11 22",
                Email = "info@saglikkinigi.com",
                CalismaBaslangicSaati = "09:00",
                CalismaBitisSaati = "19:00",
                HaftasonuAcikMi = true,
                UzmanlikAlani = "Genel Pratisyenlik ve Aile Hekimliği",
                OdaSayisi = 8,
                AktifMi = true
            };

            await db.SaglikKurumlari.AddRangeAsync(sehirHastanesi, saglikKlinigi);
            await db.SaveChangesAsync();

            // ═══════════════════════════════════════════
            // 3. BÖLÜMLER
            // ═══════════════════════════════════════════
            var kardiyoloji = new Bolum
            {
                Ad = "Kardiyoloji",
                Aciklama = "Kalp ve damar hastalıkları",
                Renk = "#ef4444",
                KurumId = sehirHastanesi.Id
            };
            var ortopedi = new Bolum
            {
                Ad = "Ortopedi ve Travmatoloji",
                Aciklama = "Kemik, eklem ve kas hastalıkları",
                Renk = "#3b82f6",
                KurumId = sehirHastanesi.Id
            };
            var noroloji = new Bolum
            {
                Ad = "Nöroloji",
                Aciklama = "Sinir sistemi hastalıkları",
                Renk = "#8b5cf6",
                KurumId = sehirHastanesi.Id
            };
            var genelPratisyen = new Bolum
            {
                Ad = "Genel Pratisyenlik",
                Aciklama = "Genel muayene ve aile hekimliği",
                Renk = "#10b981",
                KurumId = saglikKlinigi.Id
            };

            await db.Bolumler.AddRangeAsync(kardiyoloji, ortopedi, noroloji, genelPratisyen);
            await db.SaveChangesAsync();

            // ═══════════════════════════════════════════
            // 4. HİZMETLER
            // ═══════════════════════════════════════════
            var hizmetler = new List<Hizmet>
            {
                new() { Ad = "Kardiyoloji Muayene", BolumId = kardiyoloji.Id, SureDakika = 30, Ucret = 500 },
                new() { Ad = "EKG (Elektrokardiyografi)", BolumId = kardiyoloji.Id, SureDakika = 15, Ucret = 150 },
                new() { Ad = "Eforlu EKG", BolumId = kardiyoloji.Id, SureDakika = 45, Ucret = 400 },
                new() { Ad = "Ortopedi Muayene", BolumId = ortopedi.Id, SureDakika = 30, Ucret = 450 },
                new() { Ad = "Alçı Uygulaması", BolumId = ortopedi.Id, SureDakika = 60, Ucret = 300 },
                new() { Ad = "Nöroloji Muayene", BolumId = noroloji.Id, SureDakika = 30, Ucret = 480 },
                new() { Ad = "EEG (Beyin Elektriği)", BolumId = noroloji.Id, SureDakika = 60, Ucret = 350 },
                new() { Ad = "Genel Muayene", BolumId = genelPratisyen.Id, SureDakika = 20, Ucret = 250 },
                new() { Ad = "Aşı Uygulaması", BolumId = genelPratisyen.Id, SureDakika = 15, Ucret = 100 },
                new() { Ad = "Reçete Yenileme", BolumId = genelPratisyen.Id, SureDakika = 10, Ucret = 150 },
            };
            await db.Hizmetler.AddRangeAsync(hizmetler);
            await db.SaveChangesAsync();

            // ═══════════════════════════════════════════
            // 5. ADMİN KULLANICISI
            // ═══════════════════════════════════════════
            var admin = new Admin
            {
                Ad = "Sistem",
                Soyad = "Yöneticisi",
                Email = "admin@hastane.com",
                UserName = "admin@hastane.com",
                Telefon = "05001234567",
                Pozisyon = "Baş Sistem Yöneticisi",
                TumKurumlariYonetebilir = true,
                KullaniciRolu = UserRole.Admin,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");

            // ═══════════════════════════════════════════
            // 6. DOKTORLAR
            // ═══════════════════════════════════════════
            var doktor1 = new Doktor
            {
                Ad = "Ahmet",
                Soyad = "Yılmaz",
                Email = "ahmet.yilmaz@hastane.com",
                UserName = "ahmet.yilmaz@hastane.com",
                Unvan = "Prof. Dr.",
                UzmanlikAlani = "Kardiyoloji",
                DeneyimYili = 20,
                MuayeneUcreti = 500,
                BolumId = kardiyoloji.Id,
                KurumId = sehirHastanesi.Id,
                Biyografi = "İstanbul Tıp Fakültesi mezunu. 20 yıllık kardiyoloji deneyimi.",
                Telefon = "05311234567",
                KullaniciRolu = UserRole.Doktor,
                EmailConfirmed = true
            };

            var doktor2 = new Doktor
            {
                Ad = "Fatma",
                Soyad = "Demir",
                Email = "fatma.demir@hastane.com",
                UserName = "fatma.demir@hastane.com",
                Unvan = "Doç. Dr.",
                UzmanlikAlani = "Ortopedi ve Travmatoloji",
                DeneyimYili = 15,
                MuayeneUcreti = 450,
                BolumId = ortopedi.Id,
                KurumId = sehirHastanesi.Id,
                Biyografi = "Hacettepe Üniversitesi mezunu. Spor yaralanmaları uzmanı.",
                Telefon = "05321234567",
                KullaniciRolu = UserRole.Doktor,
                EmailConfirmed = true
            };

            var doktor3 = new Doktor
            {
                Ad = "Mehmet",
                Soyad = "Kaya",
                Email = "mehmet.kaya@hastane.com",
                UserName = "mehmet.kaya@hastane.com",
                Unvan = "Dr.",
                UzmanlikAlani = "Nöroloji",
                DeneyimYili = 10,
                MuayeneUcreti = 480,
                BolumId = noroloji.Id,
                KurumId = sehirHastanesi.Id,
                Biyografi = "Ankara Üniversitesi mezunu. Baş ağrısı ve epilepsi uzmanı.",
                Telefon = "05331234567",
                KullaniciRolu = UserRole.Doktor,
                EmailConfirmed = true
            };

            var doktor4 = new Doktor
            {
                Ad = "Ayşe",
                Soyad = "Çelik",
                Email = "ayse.celik@klinik.com",
                UserName = "ayse.celik@klinik.com",
                Unvan = "Dr.",
                UzmanlikAlani = "Aile Hekimliği",
                DeneyimYili = 8,
                MuayeneUcreti = 250,
                BolumId = genelPratisyen.Id,
                KurumId = saglikKlinigi.Id,
                Biyografi = "Sakarya Üniversitesi mezunu. Koruyucu hekimlik odaklı.",
                Telefon = "05341234567",
                KullaniciRolu = UserRole.Doktor,
                EmailConfirmed = true
            };

            var doktor5 = new Doktor
            {
                Ad = "Ali",
                Soyad = "Öztürk",
                Email = "ali.ozturk@klinik.com",
                UserName = "ali.ozturk@klinik.com",
                Unvan = "Dr.",
                UzmanlikAlani = "Genel Pratisyenlik",
                DeneyimYili = 5,
                MuayeneUcreti = 250,
                BolumId = genelPratisyen.Id,
                KurumId = saglikKlinigi.Id,
                Telefon = "05351234567",
                KullaniciRolu = UserRole.Doktor,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(doktor1, "Doktor123!");
            await userManager.CreateAsync(doktor2, "Doktor123!");
            await userManager.CreateAsync(doktor3, "Doktor123!");
            await userManager.CreateAsync(doktor4, "Doktor123!");
            await userManager.CreateAsync(doktor5, "Doktor123!");
            await userManager.AddToRoleAsync(doktor1, "Doktor");
            await userManager.AddToRoleAsync(doktor2, "Doktor");
            await userManager.AddToRoleAsync(doktor3, "Doktor");
            await userManager.AddToRoleAsync(doktor4, "Doktor");
            await userManager.AddToRoleAsync(doktor5, "Doktor");

            // ═══════════════════════════════════════════
            // 7. DOKTOR HİZMETLERİ
            // ═══════════════════════════════════════════
            await db.DoktorHizmetler.AddRangeAsync(
                new DoktorHizmet { DoktorId = doktor1.Id, HizmetId = hizmetler[0].Id },
                new DoktorHizmet { DoktorId = doktor1.Id, HizmetId = hizmetler[1].Id },
                new DoktorHizmet { DoktorId = doktor1.Id, HizmetId = hizmetler[2].Id },
                new DoktorHizmet { DoktorId = doktor2.Id, HizmetId = hizmetler[3].Id },
                new DoktorHizmet { DoktorId = doktor2.Id, HizmetId = hizmetler[4].Id },
                new DoktorHizmet { DoktorId = doktor3.Id, HizmetId = hizmetler[5].Id },
                new DoktorHizmet { DoktorId = doktor3.Id, HizmetId = hizmetler[6].Id },
                new DoktorHizmet { DoktorId = doktor4.Id, HizmetId = hizmetler[7].Id },
                new DoktorHizmet { DoktorId = doktor4.Id, HizmetId = hizmetler[8].Id },
                new DoktorHizmet { DoktorId = doktor4.Id, HizmetId = hizmetler[9].Id },
                new DoktorHizmet { DoktorId = doktor5.Id, HizmetId = hizmetler[7].Id },
                new DoktorHizmet { DoktorId = doktor5.Id, HizmetId = hizmetler[9].Id }
            );
            await db.SaveChangesAsync();

            // ═══════════════════════════════════════════
            // 8. UYGUNLUK ZAMAN DİLİMLERİ
            // ═══════════════════════════════════════════
            var zamanDilimleri = new List<UygunlukZamanDilimi>();
            var haftaliciGunler = new[] {
                HaftaninGunu.Pazartesi, HaftaninGunu.Sali,
                HaftaninGunu.Carsamba, HaftaninGunu.Persembe, HaftaninGunu.Cuma
            };

            foreach (var gun in haftaliciGunler)
            {
                zamanDilimleri.Add(new UygunlukZamanDilimi { DoktorId = doktor1.Id, Gun = gun, BaslangicSaati = new TimeOnly(9, 0), BitisSaati = new TimeOnly(12, 0) });
                zamanDilimleri.Add(new UygunlukZamanDilimi { DoktorId = doktor1.Id, Gun = gun, BaslangicSaati = new TimeOnly(13, 0), BitisSaati = new TimeOnly(17, 0) });
                zamanDilimleri.Add(new UygunlukZamanDilimi { DoktorId = doktor2.Id, Gun = gun, BaslangicSaati = new TimeOnly(8, 0), BitisSaati = new TimeOnly(12, 0) });
                zamanDilimleri.Add(new UygunlukZamanDilimi { DoktorId = doktor3.Id, Gun = gun, BaslangicSaati = new TimeOnly(10, 0), BitisSaati = new TimeOnly(16, 0) });
                zamanDilimleri.Add(new UygunlukZamanDilimi { DoktorId = doktor4.Id, Gun = gun, BaslangicSaati = new TimeOnly(9, 0), BitisSaati = new TimeOnly(18, 0) });
                zamanDilimleri.Add(new UygunlukZamanDilimi { DoktorId = doktor5.Id, Gun = gun, BaslangicSaati = new TimeOnly(9, 0), BitisSaati = new TimeOnly(18, 0) });
            }
            // Doktor 4 ve 5 Cumartesi de çalışıyor (klinik)
            zamanDilimleri.Add(new UygunlukZamanDilimi { DoktorId = doktor4.Id, Gun = HaftaninGunu.Cumartesi, BaslangicSaati = new TimeOnly(9, 0), BitisSaati = new TimeOnly(13, 0) });
            zamanDilimleri.Add(new UygunlukZamanDilimi { DoktorId = doktor5.Id, Gun = HaftaninGunu.Cumartesi, BaslangicSaati = new TimeOnly(9, 0), BitisSaati = new TimeOnly(13, 0) });

            await db.UygunlukZamanDilimleri.AddRangeAsync(zamanDilimleri);
            await db.SaveChangesAsync();

            // ═══════════════════════════════════════════
            // 9. ÖRNEK HASTALAR
            // ═══════════════════════════════════════════
            var hasta1 = new Hasta
            {
                Ad = "Zeynep",
                Soyad = "Arslan",
                Email = "zeynep.arslan@email.com",
                UserName = "zeynep.arslan@email.com",
                Telefon = "05421234567",
                TcKimlikNo = "12345678901",
                DogumTarihi = new DateTime(1990, 5, 15),
                KanGrubu = "A+",
                KullaniciRolu = UserRole.Hasta,
                EmailConfirmed = true
            };

            var hasta2 = new Hasta
            {
                Ad = "Mustafa",
                Soyad = "Şahin",
                Email = "mustafa.sahin@email.com",
                UserName = "mustafa.sahin@email.com",
                Telefon = "05431234567",
                TcKimlikNo = "98765432109",
                DogumTarihi = new DateTime(1975, 8, 22),
                KanGrubu = "B+",
                KronikHastaliklar = "Hipertansiyon",
                KullaniciRolu = UserRole.Hasta,
                EmailConfirmed = true
            };

            var hasta3 = new Hasta
            {
                Ad = "Demo",
                Soyad = "Hasta",
                Email = "hasta@hastane.com",
                UserName = "hasta@hastane.com",
                Telefon = "05001112233",
                KullaniciRolu = UserRole.Hasta,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(hasta1, "Hasta123!");
            await userManager.CreateAsync(hasta2, "Hasta123!");
            await userManager.CreateAsync(hasta3, "Hasta123!");
            await userManager.AddToRoleAsync(hasta1, "Hasta");
            await userManager.AddToRoleAsync(hasta2, "Hasta");
            await userManager.AddToRoleAsync(hasta3, "Hasta");

            // ═══════════════════════════════════════════
            // 10. ÖRNEK RANDEVULAR
            // ═══════════════════════════════════════════
            var bugun = DateTime.Today;
            var randevular = new List<Randevu>
            {
                new Randevu
                {
                    HastaId = hasta1.Id, DoktorId = doktor1.Id, HizmetId = hizmetler[0].Id,
                    KurumId = sehirHastanesi.Id,
                    BaslangicZamani = bugun.AddDays(2).AddHours(10),
                    BitisZamani = bugun.AddDays(2).AddHours(10).AddMinutes(30),
                    Durum = AppointmentStatus.Onaylandi,
                    UcretMiktari = 500,
                    HastaNotlari = "Nefes darlığı şikayetim var."
                },
                new Randevu
                {
                    HastaId = hasta2.Id, DoktorId = doktor2.Id, HizmetId = hizmetler[3].Id,
                    KurumId = sehirHastanesi.Id,
                    BaslangicZamani = bugun.AddDays(1).AddHours(9),
                    BitisZamani = bugun.AddDays(1).AddHours(9).AddMinutes(30),
                    Durum = AppointmentStatus.Bekliyor,
                    UcretMiktari = 450,
                    HastaNotlari = "Diz ağrısı mevcut."
                },
                new Randevu
                {
                    HastaId = hasta3.Id, DoktorId = doktor4.Id, HizmetId = hizmetler[7].Id,
                    KurumId = saglikKlinigi.Id,
                    BaslangicZamani = bugun.AddDays(3).AddHours(14),
                    BitisZamani = bugun.AddDays(3).AddHours(14).AddMinutes(20),
                    Durum = AppointmentStatus.Bekliyor,
                    UcretMiktari = 250
                },
                new Randevu
                {
                    HastaId = hasta1.Id, DoktorId = doktor3.Id, HizmetId = hizmetler[5].Id,
                    KurumId = sehirHastanesi.Id,
                    BaslangicZamani = bugun.AddDays(-5).AddHours(11),
                    BitisZamani = bugun.AddDays(-5).AddHours(11).AddMinutes(30),
                    Durum = AppointmentStatus.Tamamlandi,
                    UcretMiktari = 480,
                    DoktorNotlari = "Migren teşhisi konuldu. İlaç başlandı."
                }
            };

            await db.Randevular.AddRangeAsync(randevular);
            await db.SaveChangesAsync();

            Console.WriteLine("✅ Demo veriler başarıyla yüklendi!");
            Console.WriteLine("   Admin:  admin@hastane.com / Admin123!");
            Console.WriteLine("   Doktor: ahmet.yilmaz@hastane.com / Doktor123!");
            Console.WriteLine("   Hasta:  hasta@hastane.com / Hasta123!");
        }
    }
}
