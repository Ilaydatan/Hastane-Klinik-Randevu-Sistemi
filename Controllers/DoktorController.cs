using HastaneRandevu.Data;
using HastaneRandevu.Models.Domain;
using HastaneRandevu.Models.Enums;
using HastaneRandevu.Models.ViewModels;
using HastaneRandevu.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevu.Controllers
{
    [Authorize(Roles = "Doktor")]
    public class DoktorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRandevuServisi _randevuServisi;
        private readonly AppDbContext _db;

        public DoktorController(UserManager<ApplicationUser> userManager,
                                IRandevuServisi randevuServisi,
                                AppDbContext db)
        {
            _userManager = userManager;
            _randevuServisi = randevuServisi;
            _db = db;
        }

        private async Task<Doktor?> MevcutDoktorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            return await _db.Doktorlar
                .Include(d => d.Bolum)
                .Include(d => d.Kurum)
                .Include(d => d.UygunlukZamanDilimleri)
                .Include(d => d.DoktorHizmetler).ThenInclude(dh => dh.Hizmet)
                .FirstOrDefaultAsync(d => d.Id == user.Id);
        }

        public async Task<IActionResult> Dashboard()
        {
            var doktor = await MevcutDoktorAsync();
            if (doktor == null) return RedirectToAction("Giris", "Auth");

            var tumRandevular = await _randevuServisi.DoktorRandevularAsync(doktor.Id);
            var bugun = DateTime.Today;
            var haftaSonu = bugun.AddDays(7);

            var model = new DoktorDashboardViewModel
            {
                Doktor = doktor,
                BugunRandevular = tumRandevular
                    .Where(r => r.BaslangicZamani.Date == bugun && r.Durum != AppointmentStatus.Iptal)
                    .OrderBy(r => r.BaslangicZamani),
                BekleyenRandevular = tumRandevular
                    .Where(r => r.Durum == AppointmentStatus.Bekliyor)
                    .OrderBy(r => r.BaslangicZamani),
                ZamanDilimleri = doktor.UygunlukZamanDilimleri.Where(z => z.AktifMi),
                BugunToplamRandevu = tumRandevular.Count(r => r.BaslangicZamani.Date == bugun && r.Durum != AppointmentStatus.Iptal),
                BekleyenSayisi = tumRandevular.Count(r => r.Durum == AppointmentStatus.Bekliyor),
                BuHaftaRandevu = tumRandevular.Count(r => r.BaslangicZamani >= bugun && r.BaslangicZamani <= haftaSonu && r.Durum != AppointmentStatus.Iptal)
            };

            return View(model);
        }

        public async Task<IActionResult> Randevular()
        {
            var doktor = await MevcutDoktorAsync();
            if (doktor == null) return RedirectToAction("Giris", "Auth");

            var randevular = await _randevuServisi.DoktorRandevularAsync(doktor.Id);
            ViewBag.DoktorId = doktor.Id;
            return View(randevular);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id)
        {
            var doktor = await MevcutDoktorAsync();
            if (doktor == null) return Unauthorized();

            var (basarili, mesaj) = await _randevuServisi.RandevuOnaylaAsync(id, doktor.Id);
            TempData[basarili ? "Basari" : "Hata"] = mesaj;
            return RedirectToAction(nameof(Randevular));
        }

        [HttpGet]
        public async Task<IActionResult> Reddet(int id)
        {
            var randevu = await _db.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Hizmet)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (randevu == null) return NotFound();
            return View(new RandevuReddetViewModel { RandevuId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reddet(RandevuReddetViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var doktor = await MevcutDoktorAsync();
            if (doktor == null) return Unauthorized();

            var (basarili, mesaj) = await _randevuServisi.RandevuReddetAsync(model.RandevuId, doktor.Id, model.Neden);
            TempData[basarili ? "Basari" : "Hata"] = mesaj;
            return RedirectToAction(nameof(Randevular));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tamamla(int id)
        {
            var (basarili, mesaj) = await _randevuServisi.RandevuTamamlaAsync(id);
            TempData[basarili ? "Basari" : "Hata"] = mesaj;
            return RedirectToAction(nameof(Randevular));
        }

        [HttpGet]
        public async Task<IActionResult> Program()
        {
            var doktor = await MevcutDoktorAsync();
            if (doktor == null) return RedirectToAction("Giris", "Auth");

            var model = new DoktorZamanDilimiViewModel
            {
                DoktorId = doktor.Id,
                MevcutDilimler = doktor.UygunlukZamanDilimleri.ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ZamanDilimiEkle(DoktorZamanDilimiViewModel model)
        {
            var doktor = await MevcutDoktorAsync();
            if (doktor == null) return Unauthorized();

            var yeniDilim = new UygunlukZamanDilimi
            {
                DoktorId = doktor.Id,
                Gun = model.YeniDilim.Gun,
                BaslangicSaati = model.YeniDilim.BaslangicSaati,
                BitisSaati = model.YeniDilim.BitisSaati,
                AktifMi = true
            };

            if (yeniDilim.BaslangicSaati >= yeniDilim.BitisSaati)
            {
                TempData["Hata"] = "Başlangıç saati bitiş saatinden önce olmalıdır.";
                return RedirectToAction(nameof(Program));
            }

            await _db.UygunlukZamanDilimleri.AddAsync(yeniDilim);
            await _db.SaveChangesAsync();

            TempData["Basari"] = "Çalışma saati başarıyla eklendi.";
            return RedirectToAction(nameof(Program));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ZamanDilimiSil(int id)
        {
            var dilim = await _db.UygunlukZamanDilimleri.FindAsync(id);
            if (dilim != null)
            {
                dilim.AktifMi = false;
                await _db.SaveChangesAsync();
                TempData["Basari"] = "Çalışma saati kaldırıldı.";
            }
            return RedirectToAction(nameof(Program));
        }
    }
}
