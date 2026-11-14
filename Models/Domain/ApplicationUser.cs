using System.ComponentModel.DataAnnotations;
using HastaneRandevu.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace HastaneRandevu.Models.Domain
{
    /// <summary>
    /// Sistemdeki tüm kullanıcıların temel sınıfı.
    /// Person'dan türer, IdentityUser ile entegre çalışır.
    /// OOP: Kalıtım (Inheritance) örneği — Person → ApplicationUser
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // Person özelliklerini burada taşıyoruz (Identity ile uyumluluk için)
        private string _ad = string.Empty;
        private string _soyad = string.Empty;

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50)]
        public string Ad
        {
            get => _ad;
            set => _ad = value?.Trim() ?? string.Empty;
        }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50)]
        public string Soyad
        {
            get => _soyad;
            set => _soyad = value?.Trim() ?? string.Empty;
        }

        [Phone]
        [StringLength(20)]
        public string Telefon { get; set; } = string.Empty;

        public UserRole KullaniciRolu { get; set; }
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public bool AktifMi { get; set; } = true;

        // Polimorfik virtual property — alt sınıflar override edebilir
        public virtual string TamAd => $"{Ad} {Soyad}";
        public virtual string RolAdi => KullaniciRolu.ToString();

        /// <summary>
        /// Polimorfik metot — alt sınıflar kendi bilgisini döndürür.
        /// </summary>
        public virtual string BilgiAl()
        {
            return $"{RolAdi}: {TamAd}";
        }

        public override string ToString() => TamAd;
    }
}
