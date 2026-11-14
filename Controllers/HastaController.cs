using HastaneRandevu.Data;
using HastaneRandevu.Models.Domain;
using HastaneRandevu.Models.ViewModels;
using HastaneRandevu.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevu.Controllers
{
    [Authorize(Roles = "Hasta")]
    public class HastaController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRandevuServisi _randevuServisi;
        private readonly IKurumServisi _kurumServisi;
        private readonly IKullaniciServisi _kullaniciServisi;
        private readonly AppDbContext _db;

        public HastaController(UserManager<ApplicationUser> userManager,
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

        private async Task<Hasta?> MevcutHastaAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user as Hasta;
        }

        public async Task<IActionResult> Dashboard()
        {
            var hasta = await MevcutHastaAsync();
            if (hasta == null) return RedirectToAction("Giris", "Auth");

            var tumRandevular = await _randevuServisi.HastaRandevularAsync(hasta.Id);
            var simdi = DateTime.Now;

            var model = new HastaDashboardViewModel
            {
                Hasta = hasta,
                YaklasanRandevular = tumRandevular
                    .Where(r => r.BaslangicZamani >= simdi &&
                                r.Durum != Models.Enums.AppointmentStatus.Iptal)
                    .OrderBy(r => r.BaslangicZamani)
                    .Take(5),
                GecmisRandevular = tumRandevular
                    .Where(r => r.BaslangicZamani < simdi)
                    .OrderByDescending(r => r.BaslangicZamani)
                    .Take(5),
                ToplamRandevuSayisi = tumRandevular.Count(),
                BelikyorSayisi = tumRandevular.Count(r => r.Durum == Models.Enums.AppointmentStatus.Bekliyor),
                OnaylananSayisi = tumRandevular.Count(r => r.Durum == Models.Enums.AppointmentStatus.Onaylandi)
            };

            return View(model);
        }

        public async Task<IActionResult> RandevularBenim()
        {
            var hasta = await MevcutHastaAsync();
            if (hasta == null) return RedirectToAction("Giris", "Auth");

            var randevular = await _randevuServisi.HastaRandevularAsync(hasta.Id);
            return View(randevular);
        }

        [HttpGet]
        public async Task<IActionResult> YeniRandevu()
        {
            var kurumlar = await _kurumServisi.TumKurumlariGetirAsync();
            var model = new RandevuOlusturViewModel
            {
                Kurumlar = kurumlar.Select(k => new SelectListItem(k.Ad, k.Id.ToString())).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniRandevu(RandevuOlusturViewModel model)
        {
            var hasta = await MevcutHastaAsync();
            if (hasta == null) return RedirectToAction("Giris", "Auth");

            if (!model.SeciliKurumId.HasValue || !model.SeciliBolumId.HasValue ||
                string.IsNullOrEmpty(model.SeciliDoktorId) || !model.SeciliHizmetId.HasValue ||
                !model.SeciliSaat.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Lütfen tüm alanları doldurun.");
            }

            if (!ModelState.IsValid)
            {
                await RandevuFormunuDoldurAsync(model);
                return View(model);
            }

            var hizmet = await _db.Hizmetler.FindAsync(model.SeciliHizmetId!.Value);
            if (hizmet == null)
            {
                ModelState.AddModelError(string.Empty, "Seçilen hizmet bulunamadı.");
                await RandevuFormunuDoldurAsync(model);
                return View(model);
            }

            var randevu = new Randevu
            {
                HastaId = hasta.Id,
                DoktorId = model.SeciliDoktorId!,
                HizmetId = model.SeciliHizmetId.Value,
                KurumId = model.SeciliKurumId!.Value,
                BaslangicZamani = model.SeciliSaat!.Value,
                BitisZamani = model.SeciliSaat.Value.AddMinutes(hizmet.SureDakika),
                HastaNotlari = model.HastaNotlari
            };

            var (basarili, mesaj, _) = await _randevuServisi.RandevuOlusturAsync(randevu);

            if (basarili)
            {
                TempData["Basari"] = mesaj;
                return RedirectToAction(nameof(RandevularBenim));
            }

            ModelState.AddModelError(string.Empty, mesaj);
            await RandevuFormunuDoldurAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Iptal(int id)
        {
            var hasta = await MevcutHastaAsync();
            if (hasta == null) return RedirectToAction("Giris", "Auth");

            var (basarili, mesaj) = await _randevuServisi.RandevuIptalEtAsync(id, hasta.Id);
            TempData[basarili ? "Basari" : "Hata"] = mesaj;
            return RedirectToAction(nameof(RandevularBenim));
        }

        // AJAX: Bölümleri getir
        [HttpGet]
        public async Task<IActionResult> BolumleriGetir(int kurumId)
        {
            var bolumler = await _kurumServisi.KurumBolumleriAsync(kurumId);
            return Json(bolumler.Select(b => new { b.Id, b.Ad }));
        }

        // AJAX: Doktorları getir
        [HttpGet]
        public async Task<IActionResult> DoktorlariGetir(int bolumId)
        {
            var doktorlar = await _kullaniciServisi.BolumeGoreDoktorlarAsync(bolumId);
            return Json(doktorlar.Select(d => new { d.Id, ad = d.TamAd, d.UzmanlikAlani }));
        }

        // AJAX: Hizmetleri getir
        [HttpGet]
        public async Task<IActionResult> HizmetleriGetir(int bolumId)
        {
            var hizmetler = await _kurumServisi.BolumHizmetleriAsync(bolumId);
            return Json(hizmetler.Select(h => new { h.Id, h.Ad, h.SureDakika, h.Ucret }));
        }

        // AJAX: Uygun saatleri getir
        [HttpGet]
        public async Task<IActionResult> UygunSaatleriGetir(string doktorId, string tarih, int sureDakika)
        {
            if (!DateTime.TryParse(tarih, out var tarihObj))
                return BadRequest("Geçersiz tarih.");

            var saatler = await _randevuServisi.UygunSaatleriGetirAsync(doktorId, tarihObj, sureDakika);
            return Json(saatler.Select(s => new
            {
                deger = s.ToString("yyyy-MM-ddTHH:mm"),
                goruntule = s.ToString("HH:mm")
            }));
        }

        private async Task RandevuFormunuDoldurAsync(RandevuOlusturViewModel model)
        {
            var kurumlar = await _kurumServisi.TumKurumlariGetirAsync();
            model.Kurumlar = kurumlar.Select(k => new SelectListItem(k.Ad, k.Id.ToString())).ToList();

            if (model.SeciliKurumId.HasValue)
            {
                var bolumler = await _kurumServisi.KurumBolumleriAsync(model.SeciliKurumId.Value);
                model.Bolumler = bolumler.Select(b => new SelectListItem(b.Ad, b.Id.ToString())).ToList();
            }
            if (model.SeciliBolumId.HasValue)
            {
                var doktorlar = await _kullaniciServisi.BolumeGoreDoktorlarAsync(model.SeciliBolumId.Value);
                model.Doktorlar = doktorlar.Select(d => new SelectListItem(d.TamAd, d.Id)).ToList();
                var hizmetler = await _kurumServisi.BolumHizmetleriAsync(model.SeciliBolumId.Value);
                model.Hizmetler = hizmetler.Select(h => new SelectListItem($"{h.Ad} ({h.SureFormatli}) - {h.UcretFormatli}", h.Id.ToString())).ToList();
            }
        }
    }
}
