using System.ComponentModel.DataAnnotations;

namespace HastaneRandevu.Models.Domain
{
    /// <summary>
    /// Admin (Yönetici) sınıfı — ApplicationUser'dan türer.
    /// OOP: Kalıtım (Inheritance) — ApplicationUser → Admin
    /// </summary>
    public class Admin : ApplicationUser
    {
        [StringLength(100)]
        public string? Pozisyon { get; set; } = "Sistem Yöneticisi";

        // Admin hangi kurumu yönetiyor?
        public int? YonettigiKurumId { get; set; }
        public virtual SaglikKurumu? YonettigiKurum { get; set; }

        public bool TumKurumlariYonetebilir { get; set; } = false;

        // Polimorfizm: override
        public override string RolAdi => "Yönetici";

        /// <summary>
        /// Polimorfik override — Admin'e özel bilgi döndürür.
        /// </summary>
        public override string BilgiAl()
        {
            var kurum = YonettigiKurum?.Ad ?? (TumKurumlariYonetebilir ? "Tüm Kurumlar" : "Belirtilmemiş");
            return $"Yönetici: {TamAd} | Pozisyon: {Pozisyon} | Kurum: {kurum}";
        }
    }
}
