using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HastaneRandevu.Models.Enums;

namespace HastaneRandevu.Models.Domain
{
    /// <summary>
    /// Randevu sınıfı — sistemin çekirdeği.
    /// Hasta, Doktor, Hizmet ve zaman bilgisini bir araya getirir.
    /// </summary>
    public class Randevu
    {
        public int Id { get; set; }

        [Required]
        public DateTime BaslangicZamani { get; set; }

        [Required]
        public DateTime BitisZamani { get; set; }

        public AppointmentStatus Durum { get; set; } = AppointmentStatus.Bekliyor;

        [StringLength(1000)]
        public string? HastaNotlari { get; set; }

        [StringLength(1000)]
        public string? DoktorNotlari { get; set; }

        [StringLength(500)]
        public string? ReddetmeNedeni { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UcretMiktari { get; set; }

        public bool OdenmeDurumu { get; set; } = false;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public DateTime? GuncellenmeTarihi { get; set; }

        // Foreign Keys
        [Required]
        public string HastaId { get; set; } = string.Empty;

        [Required]
        public string DoktorId { get; set; } = string.Empty;

        [Required]
        public int HizmetId { get; set; }

        public int KurumId { get; set; }

        // Navigation Properties
        public virtual Hasta? Hasta { get; set; }
        public virtual Doktor? Doktor { get; set; }
        public virtual Hizmet? Hizmet { get; set; }
        public virtual SaglikKurumu? Kurum { get; set; }

        // Computed properties
        public string DurumAdi => Durum switch
        {
            AppointmentStatus.Bekliyor   => "Bekliyor",
            AppointmentStatus.Onaylandi  => "Onaylandı",
            AppointmentStatus.Reddedildi => "Reddedildi",
            AppointmentStatus.Iptal      => "İptal Edildi",
            AppointmentStatus.Tamamlandi => "Tamamlandı",
            _ => "Bilinmiyor"
        };

        public string DurumCssClass => Durum switch
        {
            AppointmentStatus.Bekliyor   => "badge-warning",
            AppointmentStatus.Onaylandi  => "badge-success",
            AppointmentStatus.Reddedildi => "badge-danger",
            AppointmentStatus.Iptal      => "badge-secondary",
            AppointmentStatus.Tamamlandi => "badge-info",
            _ => "badge-secondary"
        };

        public int SureDakika => (int)(BitisZamani - BaslangicZamani).TotalMinutes;

        public bool IptalEdilebilirMi =>
            Durum == AppointmentStatus.Bekliyor ||
            Durum == AppointmentStatus.Onaylandi;

        public bool GecmisteMi => BitisZamani < DateTime.Now;

        public string UcretFormatli => $"{UcretMiktari:N2} ₺";

        public override string ToString() =>
            $"Randevu #{Id} | {Hasta?.TamAd} → {Doktor?.TamAd} | {BaslangicZamani:dd.MM.yyyy HH:mm} | {DurumAdi}";
    }
}
