using System.ComponentModel.DataAnnotations;

namespace HastaneRandevu.Models.Domain
{
    /// <summary>
    /// Soyut temel sınıf — sistemdeki tüm kişilerin ortak özelliklerini tanımlar.
    /// OOP: Soyutlama (Abstraction) ve Kapsülleme (Encapsulation) örneği.
    /// </summary>
    public abstract class Person
    {
        // Kapsülleme: private backing fields, public property'ler
        private string _ad = string.Empty;
        private string _soyad = string.Empty;

        public int Id { get; set; }

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

        [Required(ErrorMessage = "Telefon alanı zorunludur.")]
        [Phone]
        [StringLength(20)]
        public string Telefon { get; set; } = string.Empty;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public bool AktifMi { get; set; } = true;

        // Polimorfizm: Alt sınıflar kendi tanımlamalarını yapar (override)
        public abstract string TamAd { get; }
        public abstract string Rol { get; }

        /// <summary>
        /// Polimorfik metot — her alt sınıf kendi bilgisini döndürür.
        /// </summary>
        public virtual string BilgiAl()
        {
            return $"{Rol}: {TamAd} | Tel: {Telefon}";
        }

        public override string ToString() => TamAd;
    }
}
