using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HastaneRandevu.Models.Enums;

namespace HastaneRandevu.Models.Domain
{
    /// <summary>
    /// Hasta sınıfı — ApplicationUser'dan türer.
    /// OOP: Kalıtım (Inheritance) — ApplicationUser → Hasta
    /// </summary>
    public class Hasta : ApplicationUser
    {
        [StringLength(11)]
        public string? TcKimlikNo { get; set; }

        public DateTime? DogumTarihi { get; set; }

        [StringLength(10)]
        public string? KanGrubu { get; set; }

        [StringLength(500)]
        public string? Adres { get; set; }

        [StringLength(1000)]
        public string? AlerjiBilgisi { get; set; }

        [StringLength(1000)]
        public string? KronikHastaliklar { get; set; }

        // Navigation property
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        // Polimorfizm: override — kendi rol adını döndürür
        public override string RolAdi => "Hasta";

        /// <summary>
        /// Polimorfik override — Hasta'ya özel bilgi döndürür.
        /// </summary>
        public override string BilgiAl()
        {
            var yas = DogumTarihi.HasValue
                ? $" | Yaş: {DateTime.Now.Year - DogumTarihi.Value.Year}"
                : string.Empty;
            return $"Hasta: {TamAd}{yas} | TC: {TcKimlikNo ?? "Belirtilmemiş"}";
        }

        public int AktifRandevuSayisi()
        {
            return Randevular.Count(r => r.Durum == AppointmentStatus.Bekliyor
                                     || r.Durum == AppointmentStatus.Onaylandi);
        }
    }
}
