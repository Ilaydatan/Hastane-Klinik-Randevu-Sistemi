using HastaneRandevu.Data;
using HastaneRandevu.Models.Domain;
using HastaneRandevu.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevu.Services
{
    public class KullaniciServisi : IKullaniciServisi
    {
        private readonly AppDbContext _db;

        public KullaniciServisi(AppDbContext db) => _db = db;

        public async Task<Hasta?> HastaBulAsync(string id)
            => await _db.Hastalar.FirstOrDefaultAsync(h => h.Id == id);

        public async Task<Doktor?> DoktorBulAsync(string id)
            => await _db.Doktorlar
                .Include(d => d.Bolum)
                .Include(d => d.Kurum)
                .Include(d => d.UygunlukZamanDilimleri)
                .Include(d => d.DoktorHizmetler).ThenInclude(dh => dh.Hizmet)
                .FirstOrDefaultAsync(d => d.Id == id);

        public async Task<Admin?> AdminBulAsync(string id)
            => await _db.Adminler
                .Include(a => a.YonettigiKurum)
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<IEnumerable<Doktor>> TumDoktorlarAsync()
            => await _db.Doktorlar
                .Include(d => d.Bolum)
                .Include(d => d.Kurum)
                .Where(d => d.AktifMi)
                .OrderBy(d => d.Soyad)
                .ToListAsync();

        public async Task<IEnumerable<Doktor>> BolumeGoreDoktorlarAsync(int bolumId)
            => await _db.Doktorlar
                .Include(d => d.Bolum)
                .Include(d => d.Kurum)
                .Include(d => d.UygunlukZamanDilimleri)
                .Where(d => d.BolumId == bolumId && d.AktifMi)
                .ToListAsync();

        public async Task<IEnumerable<Doktor>> KurumaGoreDoktorlarAsync(int kurumId)
            => await _db.Doktorlar
                .Include(d => d.Bolum)
                .Include(d => d.UygunlukZamanDilimleri)
                .Where(d => d.KurumId == kurumId && d.AktifMi)
                .ToListAsync();

        public async Task<IEnumerable<Hasta>> TumHastalarAsync()
            => await _db.Hastalar
                .Where(h => h.AktifMi)
                .OrderBy(h => h.Soyad)
                .ToListAsync();
    }

    public class KurumServisi : IKurumServisi
    {
        private readonly AppDbContext _db;

        public KurumServisi(AppDbContext db) => _db = db;

        public async Task<IEnumerable<SaglikKurumu>> TumKurumlariGetirAsync()
            => await _db.SaglikKurumlari
                .Include(k => k.Bolumler)
                .Where(k => k.AktifMi)
                .OrderBy(k => k.Ad)
                .ToListAsync();

        public async Task<SaglikKurumu?> KurumDetayAsync(int id)
            => await _db.SaglikKurumlari
                .Include(k => k.Bolumler).ThenInclude(b => b.Hizmetler)
                .Include(k => k.Doktorlar)
                .FirstOrDefaultAsync(k => k.Id == id);

        public async Task<IEnumerable<Bolum>> KurumBolumleriAsync(int kurumId)
            => await _db.Bolumler
                .Where(b => b.KurumId == kurumId && b.AktifMi)
                .OrderBy(b => b.Ad)
                .ToListAsync();

        public async Task<IEnumerable<Hizmet>> BolumHizmetleriAsync(int bolumId)
            => await _db.Hizmetler
                .Where(h => h.BolumId == bolumId && h.AktifMi)
                .OrderBy(h => h.Ad)
                .ToListAsync();

        public async Task<SaglikKurumu> KurumEkleAsync(SaglikKurumu kurum)
        {
            await _db.SaglikKurumlari.AddAsync(kurum);
            await _db.SaveChangesAsync();
            return kurum;
        }

        public async Task KurumGuncelleAsync(SaglikKurumu kurum)
        {
            _db.SaglikKurumlari.Update(kurum);
            await _db.SaveChangesAsync();
        }

        public async Task KurumSilAsync(int id)
        {
            var kurum = await _db.SaglikKurumlari.FindAsync(id);
            if (kurum != null)
            {
                kurum.AktifMi = false;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<Bolum> BolumEkleAsync(Bolum bolum)
        {
            await _db.Bolumler.AddAsync(bolum);
            await _db.SaveChangesAsync();
            return bolum;
        }

        public async Task<Hizmet> HizmetEkleAsync(Hizmet hizmet)
        {
            await _db.Hizmetler.AddAsync(hizmet);
            await _db.SaveChangesAsync();
            return hizmet;
        }

        public async Task<IEnumerable<Hizmet>> TumHizmetleriGetirAsync()
        {
            return await _db.Hizmetler
                .Include(h => h.Bolum)
                .OrderBy(h => h.Ad)
                .ToListAsync();
        }

        public async Task HizmetGuncelleAsync(Hizmet hizmet)
        {
            _db.Hizmetler.Update(hizmet);
            await _db.SaveChangesAsync();
        }

        public async Task HizmetSilAsync(int id)
        {
            var hizmet = await _db.Hizmetler.FindAsync(id);

            if (hizmet != null)
            {
                hizmet.AktifMi = false;
                await _db.SaveChangesAsync();
            }
        }

    }
}
