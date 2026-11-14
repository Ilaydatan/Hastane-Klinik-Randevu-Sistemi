using HastaneRandevu.Models.Domain;
using HastaneRandevu.Models.Enums;

namespace HastaneRandevu.Models.ViewModels
{
    // Hasta dashboard
    public class HastaDashboardViewModel
    {
        public Hasta Hasta { get; set; } = null!;
        public IEnumerable<Randevu> YaklasanRandevular { get; set; } = Enumerable.Empty<Randevu>();
        public IEnumerable<Randevu> GecmisRandevular { get; set; } = Enumerable.Empty<Randevu>();
        public int ToplamRandevuSayisi { get; set; }
        public int BelikyorSayisi { get; set; }
        public int OnaylananSayisi { get; set; }
    }

    // Doktor dashboard
    public class DoktorDashboardViewModel
    {
        public Doktor Doktor { get; set; } = null!;
        public IEnumerable<Randevu> BugunRandevular { get; set; } = Enumerable.Empty<Randevu>();
        public IEnumerable<Randevu> BekleyenRandevular { get; set; } = Enumerable.Empty<Randevu>();
        public IEnumerable<UygunlukZamanDilimi> ZamanDilimleri { get; set; } = Enumerable.Empty<UygunlukZamanDilimi>();
        public int BugunToplamRandevu { get; set; }
        public int BekleyenSayisi { get; set; }
        public int BuHaftaRandevu { get; set; }
    }

    // Admin dashboard
    public class AdminDashboardViewModel
    {
        public Admin Admin { get; set; } = null!;
        public int ToplamHasta { get; set; }
        public int ToplamDoktor { get; set; }
        public int ToplamKurum { get; set; }
        public int BugunkuRandevu { get; set; }
        public int BelikyorRandevu { get; set; }
        public int ToplamRandevu { get; set; }
        public IEnumerable<Randevu> SonRandevular { get; set; } = Enumerable.Empty<Randevu>();
        public IEnumerable<SaglikKurumu> Kurumlar { get; set; } = Enumerable.Empty<SaglikKurumu>();
    }

    // Admin - Kurum yönetimi
    public class KurumYonetimViewModel
    {
        public IEnumerable<SaglikKurumu> Kurumlar { get; set; } = Enumerable.Empty<SaglikKurumu>();
        public KurumTuru YeniKurumTuru { get; set; } = KurumTuru.Hastane;
        public SaglikKurumu? DuzenlenenKurum { get; set; }
    }

    // Admin - Doktor yönetimi
    public class DoktorYonetimViewModel
    {
        public IEnumerable<Doktor> Doktorlar { get; set; } = Enumerable.Empty<Doktor>();
        public IEnumerable<SaglikKurumu> Kurumlar { get; set; } = Enumerable.Empty<SaglikKurumu>();
        public IEnumerable<Bolum> Bolumler { get; set; } = Enumerable.Empty<Bolum>();
    }
}
