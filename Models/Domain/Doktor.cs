using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HastaneRandevu.Models.Enums;

namespace HastaneRandevu.Models.Domain
{
    /// <summary>
    /// Doktor sınıfı — ApplicationUser'dan türer.
    /// OOP: Kalıtım (Inheritance) — ApplicationUser → Doktor
    /// </summary>
    public class Doktor : ApplicationUser
    {
        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(100)]
        public string UzmanlikAlani { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Unvan { get; set; } = "Dr.";

        [StringLength(100)]
        public string? DiplomaNumarasi { get; set; }

        [Range(0, 50)]
        public int DeneyimYili { get; set; }

        [StringLength(1000)]
        public string? Biyografi { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000)]
        public decimal MuayeneUcreti { get; set; }

        // Foreign Key
        public int? BolumId { get; set; }
        public int? KurumId { get; set; }

        // Navigation Properties
        public virtual Bolum? Bolum { get; set; }
        public virtual SaglikKurumu? Kurum { get; set; }
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
        public virtual ICollection<UygunlukZamanDilimi> UygunlukZamanDilimleri { get; set; } = new List<UygunlukZamanDilimi>();
        public virtual ICollection<DoktorHizmet> DoktorHizmetler { get; set; } = new List<DoktorHizmet>();

        // Polimorfizm: override
        public override string TamAd => $"{Unvan} {Ad} {Soyad}";
        public override string RolAdi => "Doktor";

        /// <summary>
        /// Polimorfik override — Doktor'a özel bilgi döndürür.
        /// </summary>
        public override string BilgiAl()
        {
            return $"{TamAd} | Uzmanlık: {UzmanlikAlani} | Kurum: {Kurum?.Ad ?? "Belirtilmemiş"}";
        }

        public int BugunRandevuSayisi()
        {
            var bugun = DateTime.Today;
            return Randevular.Count(r => r.BaslangicZamani.Date == bugun
                                     && r.Durum != AppointmentStatus.Iptal);
        }

        /// <summary>
        /// Verilen zaman diliminde doktorun uygun olup olmadığını kontrol eder.
        /// </summary>
        public bool ZamanDilimindeUygunMu(DateTime baslangic, DateTime bitis)
        {
            var gun = (HaftaninGunu)(int)baslangic.DayOfWeek;
            return UygunlukZamanDilimleri.Any(z =>
                z.Gun == gun &&
                z.BaslangicSaati <= TimeOnly.FromDateTime(baslangic) &&
                z.BitisSaati >= TimeOnly.FromDateTime(bitis) &&
                z.AktifMi);
        }
    }
}
