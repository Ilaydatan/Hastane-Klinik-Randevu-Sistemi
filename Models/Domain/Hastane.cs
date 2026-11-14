using HastaneRandevu.Models.Enums;

namespace HastaneRandevu.Models.Domain
{
    /// <summary>
    /// Hastane sınıfı — SaglikKurumu'ndan türer.
    /// OOP: Kalıtım (Inheritance) — SaglikKurumu → Hastane
    /// </summary>
    public class Hastane : SaglikKurumu
    {
        public int YatakKapasitesi { get; set; }
        public bool AcilServisMevcut { get; set; } = true;
        public bool YogunBakimMevcut { get; set; } = false;
        public int BolumSayisi => Bolumler?.Count ?? 0;

        // Soyut özellik implementasyonu (abstract → concrete)
        public override KurumTuru KurumTuru => KurumTuru.Hastane;
        public override string KurumTuruAdi => "Hastane";

        /// <summary>
        /// Polimorfik override — Hastane'ye özel bilgi ekler.
        /// </summary>
        public override string BilgiAl()
        {
            var temel = base.BilgiAl();
            var acil = AcilServisMevcut ? "✓ Acil Servis" : "✗ Acil Servis";
            return $"{temel} | Yatak: {YatakKapasitesi} | {acil}";
        }
    }
}
