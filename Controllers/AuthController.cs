using HastaneRandevu.Models.Domain;
using HastaneRandevu.Models.Enums;
using HastaneRandevu.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HastaneRandevu.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Giris(string? donusUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["DonusUrl"] = donusUrl;
            return View(new GirisViewModel { DonusUrl = donusUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Giris(GirisViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var kullanici = await _userManager.FindByEmailAsync(model.Email);
            if (kullanici == null || !kullanici.AktifMi)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
                return View(model);
            }

            var sonuc = await _signInManager.PasswordSignInAsync(
                kullanici, model.Sifre, model.BeniHatirla, lockoutOnFailure: true);

            if (sonuc.Succeeded)
            {
                // Role göre yönlendir
                return kullanici.KullaniciRolu switch
                {
                    UserRole.Admin => RedirectToAction("Dashboard", "Admin"),
                    UserRole.Doktor => RedirectToAction("Dashboard", "Doktor"),
                    _ => RedirectToAction("Dashboard", "Hasta")
                };
            }

            if (sonuc.IsLockedOut)
                ModelState.AddModelError(string.Empty, "Hesabınız kilitlendi. Lütfen daha sonra tekrar deneyin.");
            else
                ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");

            return View(model);
        }

        [HttpGet]
        public IActionResult Kayit()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View(new KayitViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kayit(KayitViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var mevcutKullanici = await _userManager.FindByEmailAsync(model.Email);
            if (mevcutKullanici != null)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
                return View(model);
            }

            // Factory Pattern kullanımı
            var hasta = new Hasta
            {
                Ad = model.Ad,
                Soyad = model.Soyad,
                Email = model.Email,
                UserName = model.Email,
                Telefon = model.Telefon,
                TcKimlikNo = model.TcKimlikNo,
                DogumTarihi = model.DogumTarihi,
                KullaniciRolu = UserRole.Hasta,
                EmailConfirmed = true
            };

            var sonuc = await _userManager.CreateAsync(hasta, model.Sifre);
            if (sonuc.Succeeded)
            {
                await _userManager.AddToRoleAsync(hasta, "Hasta");
                await _signInManager.SignInAsync(hasta, isPersistent: false);
                TempData["Basari"] = "Hesabınız başarıyla oluşturuldu. Hoş geldiniz!";
                return RedirectToAction("Dashboard", "Hasta");
            }

            foreach (var hata in sonuc.Errors)
                ModelState.AddModelError(string.Empty, hata.Description);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cikis()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Giris");
        }

        public IActionResult ErisimEngellendi() => View();
    }
}
