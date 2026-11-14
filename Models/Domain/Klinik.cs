using HastaneRandevu.Models.Enums;

namespace HastaneRandevu.Models.Domain
{
    /// <summary>
    /// Klinik sınıfı — SaglikKurumu'ndan türer.
    /// OOP: Kalıtım (Inheritance) — SaglikKurumu → Klinik
    /// </summary>
    public class Klinik : SaglikKurumu
    {
        public string? UzmanlikAlani { get; set; }  // ör: Diş, Göz, Cilt
        public bool RandevuZorunluMu { get; set; } = true;
        public int OdaSayisi { get; set; }

        // Soyut özellik implementasyonu
        public override KurumTuru KurumTuru => KurumTuru.Klinik;
        public override string KurumTuruAdi => "Klinik";

        /// <summary>
        /// Polimorfik override — Klinik'e özel bilgi ekler.
        /// </summary>
        public override string BilgiAl()
        {
            var temel = base.BilgiAl();
            var uzmanlik = UzmanlikAlani != null ? $" | Uzmanlık: {UzmanlikAlani}" : string.Empty;
            return $"{temel}{uzmanlik}";
        }
    }
}
