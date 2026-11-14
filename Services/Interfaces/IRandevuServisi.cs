using HastaneRandevu.Models.Domain;
using HastaneRandevu.Models.Enums;

namespace HastaneRandevu.Services.Interfaces
{
    /// <summary>
    /// Randevu işlemleri için servis arayüzü.
    /// OOP: Interface (Soyutlama) örneği — bağımlılık enjeksiyonunu sağlar.
    /// </summary>
    public interface IRandevuServisi
    {
        Task<IEnumerable<Randevu>> TumRandevularAsync();
        Task<IEnumerable<Randevu>> HastaRandevularAsync(string hastaId);
        Task<IEnumerable<Randevu>> DoktorRandevularAsync(string doktorId);
        Task<IEnumerable<Randevu>> TarihAraligiRandevularAsync(DateTime baslangic, DateTime bitis);
        Task<Randevu?> RandevuDetayAsync(int id);
        Task<(bool Basarili, string Mesaj, Randevu? Randevu)> RandevuOlusturAsync(Randevu randevu);
        Task<(bool Basarili, string Mesaj)> RandevuOnaylaAsync(int randevuId, string doktorId);
        Task<(bool Basarili, string Mesaj)> RandevuReddetAsync(int randevuId, string doktorId, string neden);
        Task<(bool Basarili, string Mesaj)> RandevuIptalEtAsync(int randevuId, string kullaniciId);
        Task<(bool Basarili, string Mesaj)> RandevuTamamlaAsync(int randevuId);

        /// <summary>Çakışma kontrolü — iki randevu çakışıyorsa true döner.</summary>
        Task<bool> CakismaVarMiAsync(string doktorId, DateTime baslangic, DateTime bitis, int? haricRandevuId = null);

        /// <summary>Doktorun o zaman diliminde uygun slotlarını döndürür.</summary>
        Task<IEnumerable<DateTime>> UygunSaatleriGetirAsync(string doktorId, DateTime tarih, int hizmetSureDakika);
    }
}
