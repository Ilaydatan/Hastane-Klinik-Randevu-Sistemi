using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HastaneRandevu.Models.Domain
{
    /// <summary>
    /// Sağlık kurumundaki bölümü temsil eder (ör: Kardiyoloji, Ortopedi).
    /// </summary>
    public class Bolum
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Bölüm adı zorunludur.")]
        [StringLength(100)]
        public string Ad { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Aciklama { get; set; }

        [StringLength(10)]
        public string? Renk { get; set; } = "#6366f1";  // UI için renk kodu

        public bool AktifMi { get; set; } = true;

        // Foreign Key
        public int KurumId { get; set; }

        // Navigation Properties
        public virtual SaglikKurumu? Kurum { get; set; }
        public virtual ICollection<Doktor> Doktorlar { get; set; } = new List<Doktor>();
        public virtual ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();

        public override string ToString() => Ad;
    }
}
