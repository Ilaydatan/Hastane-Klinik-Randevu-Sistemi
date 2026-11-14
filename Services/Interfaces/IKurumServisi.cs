
using HastaneRandevu.Models.Domain;
using HastaneRandevu.Models.ViewModels;

namespace HastaneRandevu.Services.Interfaces
{
    /// <summary>
    /// Kullanıcı (Hasta, Doktor, Admin) işlemleri servis arayüzü.
    /// </summary>
    public interface IKullaniciServisi
    {
        Task<Hasta?> HastaBulAsync(string id);
        Task<Doktor?> DoktorBulAsync(string id);
        Task<Admin?> AdminBulAsync(string id);
        Task<IEnumerable<Doktor>> TumDoktorlarAsync();
        Task<IEnumerable<Doktor>> BolumeGoreDoktorlarAsync(int bolumId);
        Task<IEnumerable<Doktor>> KurumaGoreDoktorlarAsync(int kurumId);
        Task<IEnumerable<Hasta>> TumHastalarAsync();
    }

    /// <summary>
    /// Sağlık kurumu, bölüm ve hizmet işlemleri.
    /// </summary>
    public interface IKurumServisi
    {
        Task<IEnumerable<SaglikKurumu>> TumKurumlariGetirAsync();
        Task<SaglikKurumu?> KurumDetayAsync(int id);
        Task<IEnumerable<Bolum>> KurumBolumleriAsync(int kurumId);
        Task<IEnumerable<Hizmet>> BolumHizmetleriAsync(int bolumId);

        // Kurum işlemleri
        Task<SaglikKurumu> KurumEkleAsync(SaglikKurumu kurum);
        Task KurumGuncelleAsync(SaglikKurumu kurum);
        Task KurumSilAsync(int id);

        // Bölüm işlemleri
        Task<Bolum> BolumEkleAsync(Bolum bolum);

        // Hizmet işlemleri
        Task<IEnumerable<Hizmet>> TumHizmetleriGetirAsync();
        Task<Hizmet> HizmetEkleAsync(Hizmet hizmet);
        Task HizmetGuncelleAsync(Hizmet hizmet);
        Task HizmetSilAsync(int id);
    }
}
