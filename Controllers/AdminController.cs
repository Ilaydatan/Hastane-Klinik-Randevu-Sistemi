using HastaneRandevu.Data;
using HastaneRandevu.Models.Domain;
using HastaneRandevu.Models.Enums;
using HastaneRandevu.Models.ViewModels;
using HastaneRandevu.Services;
using HastaneRandevu.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevu.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRandevuServisi _randevuServisi;
        private readonly IKurumServisi _kurumServisi;
        private readonly IKullaniciServisi _kullaniciServisi;
        private readonly AppDbContext _db;

        public AdminController(UserManager<ApplicationUser> userManager,
                               IRandevuServisi randevuServisi,
                               IKurumServisi kurumServisi,
                               IKullaniciServisi kullaniciServisi,
                               AppDbContext db)
        {
            _userManager = userManager;
            _randevuServisi = randevuServisi;
            _kurumServisi = kurumServisi;
            _kullaniciServisi = kullaniciServisi;
            _db = db;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var admin = await _db.Adminler
                .Include(a => a.YonettigiKurum)
                .FirstOrDefaultAsync(a => a.Id == user!.Id);

            var bugun = DateTime.Today;
            var tumRandevular = await _randevuServisi.TumRandevularAsync();
            var kurumlar = await _kurumServisi.TumKurumlariGetirAsync();

            var model = new AdminDashboardViewModel
            {
                Admin = admin!,
                ToplamHasta = await _db.Hastalar.CountAsync(h => h.AktifMi),
                ToplamDoktor = await _db.Doktorlar.CountAsync(d => d.AktifMi),
                ToplamKurum = await _db.SaglikKurumlari.CountAsync(k => k.AktifMi),
                BugunkuRandevu = tumRandevular.Count(r => r.BaslangicZamani.Date == bugun),
                BelikyorRandevu = tumRandevular.Count(r => r.Durum == AppointmentStatus.Bekliyor),
                ToplamRandevu = tumRandevular.Count(),
                SonRandevular = tumRandevular.Take(10),
                Kurumlar = kurumlar
            };

            return View(model);
        }

        // ════ KURUM YÖNETİMİ ════

        public async Task<IActionResult> Kurumlar()
        {
            var kurumlar = await _kurumServisi.TumKurumlariGetirAsync();
            return View(kurumlar);
        }

        [HttpGet]
        public IActionResult KurumEkle()
        {
            ViewBag.KurumTurleri = Enum.GetValues<KurumTuru>()
                .Select(k => new SelectListItem(k == KurumTuru.Hastane ? "Hastane" : "Klinik", ((int)k).ToString()));
            return View(new SaglikKurumuEkleViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KurumEkle(SaglikKurumuEkleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.KurumTurleri = Enum.GetValues<KurumTuru>()
                    .Select(k => new SelectListItem(k.ToString(), ((int)k).ToString()));
                return View(model);
            }

            // Factory Pattern: Kurum türüne göre nesne oluştur
            var kurum = KullaniciFactory.KurumOlustur(model.KurumTuru, model.Ad, model.Adres);
            kurum.Telefon = model.Telefon;
            kurum.Email = model.Email;
            kurum.CalismaBaslangicSaati = model.CalismaBaslangicSaati;
            kurum.CalismaBitisSaati = model.CalismaBitisSaati;
            kurum.HaftasonuAcikMi = model.HaftasonuAcikMi;

            if (kurum is Hastane hastane)
            {
                hastane.YatakKapasitesi = model.YatakKapasitesi ?? 0;
                hastane.AcilServisMevcut = model.AcilServisMevcut;
            }
            else if (kurum is Klinik klinik)
            {
                klinik.UzmanlikAlani = model.UzmanlikAlani;
            }

            await _kurumServisi.KurumEkleAsync(kurum);
            TempData["Basari"] = $"{kurum.KurumTuruAdi} başarıyla eklendi.";
            return RedirectToAction(nameof(Kurumlar));
        }


        // ════ HİZMET YÖNETİMİ ════

        // Bütün hizmetleri listele
        public async Task<IActionResult> Hizmetler()
        {
            var hizmetler = await _kurumServisi.TumHizmetleriGetirAsync();
            return View(hizmetler);
        }

        // Yeni hizmet ekleme sayfası
        [HttpGet]
        public async Task<IActionResult> HizmetEkle(int? bolumId)
        {
            var bolumler = await _db.Bolumler
                .Where(b => b.AktifMi)
                .OrderBy(b => b.Ad)
                .ToListAsync();

            ViewBag.Bolumler = new SelectList(
                bolumler, "Id", "Ad", bolumId
            );

            return View(new Hizmet
            {
                BolumId = bolumId ?? 0,
                SureDakika = 30,
                AktifMi = true
            });
        }

        // Yeni hizmeti kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizmetEkle(Hizmet hizmet)
        {
            if (!await _db.Bolumler.AnyAsync(
                b => b.Id == hizmet.BolumId && b.AktifMi))
            {
                ModelState.AddModelError(
                    nameof(hizmet.BolumId),
                    "Lütfen geçerli bir bölüm seçin."
                );
            }

            if (!ModelState.IsValid)
            {
                var bolumler = await _db.Bolumler
                    .Where(b => b.AktifMi)
                    .OrderBy(b => b.Ad)
                    .ToListAsync();

                ViewBag.Bolumler = new SelectList(
                    bolumler, "Id", "Ad", hizmet.BolumId
                );

                return View(hizmet);
            }

            hizmet.AktifMi = true;

            await _kurumServisi.HizmetEkleAsync(hizmet);

            TempData["Basari"] =
                $"{hizmet.Ad} hizmeti başarıyla eklendi.";

            return RedirectToAction(nameof(Hizmetler));
        }

        // Hizmet düzenleme sayfası
        [HttpGet]
        public async Task<IActionResult> HizmetDuzenle(int id)
        {
            var hizmet = await _db.Hizmetler
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hizmet == null)
                return NotFound();

            var bolumler = await _db.Bolumler
                .Where(b => b.AktifMi)
                .OrderBy(b => b.Ad)
                .ToListAsync();

            ViewBag.Bolumler = new SelectList(
                bolumler, "Id", "Ad", hizmet.BolumId
            );

            return View(hizmet);
        }

        // Hizmet güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizmetDuzenle(int id, Hizmet model)
        {
            var hizmet = await _db.Hizmetler
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hizmet == null)
                return NotFound();

            if (!await _db.Bolumler.AnyAsync(
                b => b.Id == model.BolumId && b.AktifMi))
            {
                ModelState.AddModelError(
                    nameof(model.BolumId),
                    "Lütfen geçerli bir bölüm seçin."
                );
            }

            if (!ModelState.IsValid)
            {
                var bolumler = await _db.Bolumler
                    .Where(b => b.AktifMi)
                    .OrderBy(b => b.Ad)
                    .ToListAsync();

                ViewBag.Bolumler = new SelectList(
                    bolumler, "Id", "Ad", model.BolumId
                );

                return View(model);
            }

            hizmet.Ad = model.Ad;
            hizmet.Aciklama = model.Aciklama;
            hizmet.SureDakika = model.SureDakika;
            hizmet.Ucret = model.Ucret;
            hizmet.BolumId = model.BolumId;
            hizmet.AktifMi = model.AktifMi;

            await _kurumServisi.HizmetGuncelleAsync(hizmet);

            TempData["Basari"] =
                $"{hizmet.Ad} hizmeti güncellendi.";

            return RedirectToAction(nameof(Hizmetler));
        }

        // Hizmeti pasife al
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizmetSil(int id)
        {
            await _kurumServisi.HizmetSilAsync(id);

            TempData["Basari"] =
                "Hizmet başarıyla pasife alındı.";

            return RedirectToAction(nameof(Hizmetler));
        }


        // ════ BÖLÜM YÖNETİMİ ════

        [HttpGet]
        public async Task<IActionResult> BolumEkle(int kurumId)
        {
            var kurum = await _kurumServisi.KurumDetayAsync(kurumId);
            if (kurum == null) return NotFound();
            ViewBag.Kurum = kurum;
            return View(new Bolum { KurumId = kurumId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BolumEkle(Bolum bolum)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Kurum = await _kurumServisi.KurumDetayAsync(bolum.KurumId);
                return View(bolum);
            }
            await _kurumServisi.BolumEkleAsync(bolum);
            TempData["Basari"] = "Bölüm başarıyla eklendi.";
            return RedirectToAction(nameof(KurumDetay), new { id = bolum.KurumId });
        }

        public async Task<IActionResult> KurumDetay(int id)
        {
            var kurum = await _kurumServisi.KurumDetayAsync(id);
            if (kurum == null) return NotFound();
            return View(kurum);
        }

        // ════ DOKTOR YÖNETİMİ ════

        public async Task<IActionResult> Doktorlar()
        {
            var doktorlar = await _kullaniciServisi.TumDoktorlarAsync();
            return View(doktorlar);
        }

        [HttpGet]
        public async Task<IActionResult> DoktorEkle()
        {
            var kurumlar = await _kurumServisi.TumKurumlariGetirAsync();
            ViewBag.Kurumlar = kurumlar.Select(k => new SelectListItem(k.Ad, k.Id.ToString()));
            return View(new DoktorEkleViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoktorEkle(DoktorEkleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var kurumlar = await _kurumServisi.TumKurumlariGetirAsync();
                ViewBag.Kurumlar = kurumlar.Select(k => new SelectListItem(k.Ad, k.Id.ToString()));
                return View(model);
            }

            var doktor = new Doktor
            {
                Ad = model.Ad,
                Soyad = model.Soyad,
                Email = model.Email,
                UserName = model.Email,
                Unvan = model.Unvan,
                UzmanlikAlani = model.UzmanlikAlani,
                Telefon = model.Telefon,
                DeneyimYili = model.DeneyimYili,
                MuayeneUcreti = model.MuayeneUcreti,
                BolumId = model.BolumId,
                KurumId = model.KurumId,
                Biyografi = model.Biyografi,
                KullaniciRolu = UserRole.Doktor,
                EmailConfirmed = true
            };

            var sonuc = await _userManager.CreateAsync(doktor, model.Sifre);
            if (sonuc.Succeeded)
            {
                await _userManager.AddToRoleAsync(doktor, "Doktor");
                TempData["Basari"] = $"{doktor.TamAd} sisteme başarıyla eklendi.";
                return RedirectToAction(nameof(Doktorlar));
            }

            foreach (var hata in sonuc.Errors)
                ModelState.AddModelError(string.Empty, hata.Description);

            var kurumlarTekrar = await _kurumServisi.TumKurumlariGetirAsync();
            ViewBag.Kurumlar = kurumlarTekrar.Select(k => new SelectListItem(k.Ad, k.Id.ToString()));
            return View(model);
        }

        // AJAX: Bölümleri getir (doktor eklerken)
        [HttpGet]
        public async Task<IActionResult> BolumleriGetir(int kurumId)
        {
            var bolumler = await _kurumServisi.KurumBolumleriAsync(kurumId);
            return Json(bolumler.Select(b => new { b.Id, b.Ad }));
        }

        // ════ RANDEVU YÖNETİMİ ════

        public async Task<IActionResult> TumRandevular()
        {
            var randevular = await _randevuServisi.TumRandevularAsync();
            return View(randevular);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuIptal(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var (basarili, mesaj) = await _randevuServisi.RandevuIptalEtAsync(id, user!.Id);
            TempData[basarili ? "Basari" : "Hata"] = mesaj;
            return RedirectToAction(nameof(TumRandevular));
        }

        // ════ HASTA LİSTESİ ════

        public async Task<IActionResult> Hastalar()
        {
            var hastalar = await _kullaniciServisi.TumHastalarAsync();
            return View(hastalar);
        }
    }

    // Admin'e özel ViewModel'ler (ayrı dosyaya da taşınabilir)
    public class SaglikKurumuEkleViewModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Ad { get; set; } = string.Empty;
        public string? Adres { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public KurumTuru KurumTuru { get; set; } = KurumTuru.Hastane;
        public string CalismaBaslangicSaati { get; set; } = "08:00";
        public string CalismaBitisSaati { get; set; } = "18:00";
        public bool HaftasonuAcikMi { get; set; }
        // Hastane'ye özel
        public int? YatakKapasitesi { get; set; }
        public bool AcilServisMevcut { get; set; }
        // Klinik'e özel
        public string? UzmanlikAlani { get; set; }
    }

    public class DoktorEkleViewModel
    {
        [System.ComponentModel.DataAnnotations.Required] public string Ad { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required] public string Soyad { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required][System.ComponentModel.DataAnnotations.EmailAddress] public string Email { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required] public string Telefon { get; set; } = string.Empty;
        [System.ComponentModel.DataAnnotations.Required] public string Sifre { get; set; } = "Doktor123!";
        public string Unvan { get; set; } = "Dr.";
        [System.ComponentModel.DataAnnotations.Required] public string UzmanlikAlani { get; set; } = string.Empty;
        public int DeneyimYili { get; set; }
        public decimal MuayeneUcreti { get; set; }
        public int? KurumId { get; set; }
        public int? BolumId { get; set; }
        public string? Biyografi { get; set; }
    }
}
