using System.ComponentModel.DataAnnotations;
using HastaneRandevu.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HastaneRandevu.Models.ViewModels
{
    public class RandevuOlusturViewModel
    {
        // Seçim adımları
        public int? SeciliKurumId { get; set; }
        public int? SeciliBolumId { get; set; }
        public string? SeciliDoktorId { get; set; }
        public int? SeciliHizmetId { get; set; }

        [Required(ErrorMessage = "Randevu tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Randevu Tarihi")]
        public DateTime? SeciliTarih { get; set; }

        [Required(ErrorMessage = "Randevu saati zorunludur.")]
        [Display(Name = "Randevu Saati")]
        public DateTime? SeciliSaat { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notlarınız (İsteğe Bağlı)")]
        public string? HastaNotlari { get; set; }

        // Dropdown listeleri
        public List<SelectListItem> Kurumlar { get; set; } = new();
        public List<SelectListItem> Bolumler { get; set; } = new();
        public List<SelectListItem> Doktorlar { get; set; } = new();
        public List<SelectListItem> Hizmetler { get; set; } = new();
        public List<DateTime> UygunSaatler { get; set; } = new();

        // Seçilen nesnelerin detayları (özet gösterimi için)
        public SaglikKurumu? SeciliKurum { get; set; }
        public Bolum? SeciliBolum { get; set; }
        public Doktor? SeciliDoktor { get; set; }
        public Hizmet? SeciliHizmet { get; set; }
    }

    public class RandevuDetayViewModel
    {
        public Randevu Randevu { get; set; } = null!;
        public bool OnaylamaYetkisiVar { get; set; }
        public bool IptalYetkisiVar { get; set; }
    }

    public class RandevuReddetViewModel
    {
        public int RandevuId { get; set; }

        [Required(ErrorMessage = "Red nedeni zorunludur.")]
        [StringLength(500)]
        [Display(Name = "Red Nedeni")]
        public string Neden { get; set; } = string.Empty;
    }

    public class DoktorZamanDilimiViewModel
    {
        public string DoktorId { get; set; } = string.Empty;
        public List<UygunlukZamanDilimi> MevcutDilimler { get; set; } = new();
        public YeniZamanDilimiViewModel YeniDilim { get; set; } = new();
    }

    public class YeniZamanDilimiViewModel
    {
        [Required]
        [Display(Name = "Gün")]
        public HastaneRandevu.Models.Enums.HaftaninGunu Gun { get; set; }

        [Required]
        [Display(Name = "Başlangıç Saati")]
        public TimeOnly BaslangicSaati { get; set; } = new TimeOnly(9, 0);

        [Required]
        [Display(Name = "Bitiş Saati")]
        public TimeOnly BitisSaati { get; set; } = new TimeOnly(17, 0);
    }
}
