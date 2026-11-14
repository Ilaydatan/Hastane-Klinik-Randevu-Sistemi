using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HastaneRandevu.Models.Enums;

namespace HastaneRandevu.Models.Domain
{
    /// <summary>
    /// Soyut Sağlık Kurumu sınıfı — Hastane ve Klinik için ortak temel.
    /// OOP: Soyutlama (Abstraction) ve Kalıtım (Inheritance) örneği.
    /// </summary>
    public abstract class SaglikKurumu
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kurum adı zorunludur.")]
        [StringLength(200)]
        public string Ad { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Adres { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Telefon { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? WebSitesi { get; set; }

        [StringLength(1000)]
        public string? Aciklama { get; set; }

        // Çalışma saatleri
        [StringLength(10)]
        public string CalismaBaslangicSaati { get; set; } = "08:00";

        [StringLength(10)]
        public string CalismaBitisSaati { get; set; } = "18:00";

        public bool HaftasonuAcikMi { get; set; } = false;

        public bool AktifMi { get; set; } = true;
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Bolum> Bolumler { get; set; } = new List<Bolum>();
        public virtual ICollection<Doktor> Doktorlar { get; set; } = new List<Doktor>();

        // Soyut özellikler — alt sınıflar implement etmek ZORUNDA
        public abstract KurumTuru KurumTuru { get; }
        public abstract string KurumTuruAdi { get; }

        /// <summary>
        /// Polimorfik metot — alt sınıflar override edebilir.
        /// </summary>
        public virtual string BilgiAl()
        {
            return $"{KurumTuruAdi}: {Ad} | Tel: {Telefon ?? "Belirtilmemiş"} | Çalışma: {CalismaBaslangicSaati}-{CalismaBitisSaati}";
        }

        public virtual string CalismaSaatleriOzet()
        {
            var haftasonu = HaftasonuAcikMi ? "Hafta Sonu Dahil" : "Hafta İçi";
            return $"{CalismaBaslangicSaati} - {CalismaBitisSaati} ({haftasonu})";
        }

        public override string ToString() => $"{KurumTuruAdi}: {Ad}";
    }
}
