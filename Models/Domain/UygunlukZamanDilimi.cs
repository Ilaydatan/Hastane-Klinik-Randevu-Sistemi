using System.ComponentModel.DataAnnotations;
using HastaneRandevu.Models.Enums;

namespace HastaneRandevu.Models.Domain
{
    /// <summary>
    /// Doktorun belirli bir gün ve saatte uygunluk zaman dilimini temsil eder.
    /// Hastalar yalnızca bu tanımlı zaman dilimlerine randevu alabilir.
    /// </summary>
    public class UygunlukZamanDilimi
    {
        public int Id { get; set; }

        [Required]
        public HaftaninGunu Gun { get; set; }

        [Required]
        public TimeOnly BaslangicSaati { get; set; }

        [Required]
        public TimeOnly BitisSaati { get; set; }

        public bool AktifMi { get; set; } = true;

        // Aynı zaman dilimindeki maksimum randevu sayısı
        [Range(1, 100)]
        public int MaksimumRandevuSayisi { get; set; } = 10;

        // Foreign Key
        public string DoktorId { get; set; } = string.Empty;

        // Navigation Property
        public virtual Doktor? Doktor { get; set; }

        // Computed properties
        public string GunAdi => Gun.ToString();

        public string SaatAraligi =>
            $"{BaslangicSaati:HH\\:mm} - {BitisSaati:HH\\:mm}";

        public int SureDakika =>
            (int)(BitisSaati.ToTimeSpan() - BaslangicSaati.ToTimeSpan()).TotalMinutes;

        /// <summary>
        /// Verilen tarih bu zaman dilimine denk gelip gelmediğini kontrol eder.
        /// </summary>
        public bool TariheUygunMu(DateTime tarih)
        {
            return (HaftaninGunu)(int)tarih.DayOfWeek == Gun && AktifMi;
        }

        public override string ToString() =>
            $"{GunAdi}: {SaatAraligi}";
    }
}
