using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HastaneRandevu.Models.Domain
{
    /// <summary>
    /// Bir bölümde sunulan sağlık hizmetini temsil eder.
    /// (ör: Genel Muayene, EKG, Röntgen, Kan Tahlili)
    /// </summary>
    public class Hizmet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(150)]
        public string Ad { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Aciklama { get; set; }

        /// <summary>Hizmetin süresi (dakika cinsinden)</summary>
        [Range(5, 480, ErrorMessage = "Süre 5-480 dakika arasında olmalıdır.")]
        public int SureDakika { get; set; } = 30;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000)]
        public decimal Ucret { get; set; }

        public bool AktifMi { get; set; } = true;

        // Foreign Key
        public int BolumId { get; set; }

        // Navigation Properties
        public virtual Bolum? Bolum { get; set; }
        public virtual ICollection<DoktorHizmet> DoktorHizmetler { get; set; } = new List<DoktorHizmet>();
        public virtual ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        public string UcretFormatli => $"{Ucret:N2} ₺";
        public string SureFormatli => SureDakika >= 60
            ? $"{SureDakika / 60} sa {SureDakika % 60} dk"
            : $"{SureDakika} dk";

        public override string ToString() => $"{Ad} ({SureFormatli}) - {UcretFormatli}";
    }

    /// <summary>
    /// Doktor ↔ Hizmet çoka-çok ilişki tablosu.
    /// </summary>
    public class DoktorHizmet
    {
        public string  DoktorId { get; set; } = string.Empty;
        public virtual Doktor? Doktor { get; set; }

        public int HizmetId { get; set; }
        public virtual Hizmet? Hizmet { get; set; }

        public bool AktifMi { get; set; } = true;
    }
}
