using HastaneRandevu.Data;
using HastaneRandevu.Models.Domain;
using HastaneRandevu.Models.Enums;
using HastaneRandevu.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevu.Services
{
    /// <summary>
    /// Randevu servisinin implementasyonu.
    /// OOP: IRandevuServisi interface'ini uygular — polimorfizm ve DI örneği.
    /// </summary>
    public class RandevuServisi : IRandevuServisi
    {
        private readonly AppDbContext _db;

        public RandevuServisi(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Randevu>> TumRandevularAsync()
        {
            return await _db.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Doktor)
                .Include(r => r.Hizmet).ThenInclude(h => h!.Bolum)
                .Include(r => r.Kurum)
                .OrderByDescending(r => r.BaslangicZamani)
                .ToListAsync();
        }

        public async Task<IEnumerable<Randevu>> HastaRandevularAsync(string hastaId)
        {
            return await _db.Randevular
                .Include(r => r.Doktor)
                .Include(r => r.Hizmet).ThenInclude(h => h!.Bolum)
                .Include(r => r.Kurum)
                .Where(r => r.HastaId == hastaId)
                .OrderByDescending(r => r.BaslangicZamani)
                .ToListAsync();
        }

        public async Task<IEnumerable<Randevu>> DoktorRandevularAsync(string doktorId)
        {
            return await _db.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Hizmet)
                .Include(r => r.Kurum)
                .Where(r => r.DoktorId == doktorId)
                .OrderByDescending(r => r.BaslangicZamani)
                .ToListAsync();
        }

        public async Task<IEnumerable<Randevu>> TarihAraligiRandevularAsync(DateTime baslangic, DateTime bitis)
        {
            return await _db.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Doktor)
                .Include(r => r.Hizmet)
                .Where(r => r.BaslangicZamani >= baslangic && r.BaslangicZamani <= bitis)
                .OrderBy(r => r.BaslangicZamani)
                .ToListAsync();
        }

        public async Task<Randevu?> RandevuDetayAsync(int id)
        {
            return await _db.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Doktor)
                .Include(r => r.Hizmet).ThenInclude(h => h!.Bolum)
                .Include(r => r.Kurum)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// Yeni randevu oluşturur. Çakışma kontrolü dahildir.
        /// </summary>
        public async Task<(bool Basarili, string Mesaj, Randevu? Randevu)> RandevuOlusturAsync(Randevu randevu)
        {
            // 1. Çakışma kontrolü
            var cakismaVar = await CakismaVarMiAsync(randevu.DoktorId, randevu.BaslangicZamani, randevu.BitisZamani);
            if (cakismaVar)
                return (false, "Seçilen zaman diliminde doktorun başka bir randevusu bulunmaktadır.", null);

            // 2. Doktorun o saatte uygunluğunu kontrol et
            var doktor = await _db.Doktorlar
                .Include(d => d.UygunlukZamanDilimleri)
                .FirstOrDefaultAsync(d => d.Id == randevu.DoktorId);

            if (doktor == null)
                return (false, "Doktor bulunamadı.", null);

            if (!doktor.ZamanDilimindeUygunMu(randevu.BaslangicZamani, randevu.BitisZamani))
                return (false, "Doktor seçilen saatte çalışmamaktadır. Lütfen uygun bir zaman dilimi seçin.", null);

            // 3. Geçmiş tarih kontrolü
            if (randevu.BaslangicZamani < DateTime.Now)
                return (false, "Geçmiş bir tarih için randevu oluşturulamaz.", null);

            // 4. Hizmet ücretini al
            var hizmet = await _db.Hizmetler.FindAsync(randevu.HizmetId);
            if (hizmet != null)
                randevu.UcretMiktari = hizmet.Ucret;

            randevu.Durum = AppointmentStatus.Bekliyor;
            randevu.OlusturmaTarihi = DateTime.Now;

            await _db.Randevular.AddAsync(randevu);
            await _db.SaveChangesAsync();

            return (true, "Randevunuz başarıyla oluşturuldu. Doktor onayı bekleniyor.", randevu);
        }

        public async Task<(bool Basarili, string Mesaj)> RandevuOnaylaAsync(int randevuId, string doktorId)
        {
            var randevu = await _db.Randevular.FindAsync(randevuId);
            if (randevu == null)
                return (false, "Randevu bulunamadı.");

            if (randevu.DoktorId != doktorId)
                return (false, "Bu randevuyu onaylama yetkiniz yok.");

            if (randevu.Durum != AppointmentStatus.Bekliyor)
                return (false, $"Bu randevu zaten '{randevu.DurumAdi}' durumunda.");

            randevu.Durum = AppointmentStatus.Onaylandi;
            randevu.GuncellenmeTarihi = DateTime.Now;
            await _db.SaveChangesAsync();

            return (true, "Randevu başarıyla onaylandı.");
        }

        public async Task<(bool Basarili, string Mesaj)> RandevuReddetAsync(int randevuId, string doktorId, string neden)
        {
            var randevu = await _db.Randevular.FindAsync(randevuId);
            if (randevu == null)
                return (false, "Randevu bulunamadı.");

            if (randevu.DoktorId != doktorId)
                return (false, "Bu randevuyu reddetme yetkiniz yok.");

            randevu.Durum = AppointmentStatus.Reddedildi;
            randevu.ReddetmeNedeni = neden;
            randevu.GuncellenmeTarihi = DateTime.Now;
            await _db.SaveChangesAsync();

            return (true, "Randevu reddedildi.");
        }

        public async Task<(bool Basarili, string Mesaj)> RandevuIptalEtAsync(int randevuId, string kullaniciId)
        {
            var randevu = await _db.Randevular.FindAsync(randevuId);
            if (randevu == null)
                return (false, "Randevu bulunamadı.");

            if (randevu.HastaId != kullaniciId && randevu.DoktorId != kullaniciId)
            {
                // Admin ise izin ver — admin kontrolü caller'da yapılır
                var admin = await _db.Adminler.FindAsync(kullaniciId);
                if (admin == null)
                    return (false, "Bu randevuyu iptal etme yetkiniz yok.");
            }

            if (!randevu.IptalEdilebilirMi)
                return (false, $"'{randevu.DurumAdi}' durumundaki randevu iptal edilemez.");

            randevu.Durum = AppointmentStatus.Iptal;
            randevu.GuncellenmeTarihi = DateTime.Now;
            await _db.SaveChangesAsync();

            return (true, "Randevu iptal edildi.");
        }

        public async Task<(bool Basarili, string Mesaj)> RandevuTamamlaAsync(int randevuId)
        {
            var randevu = await _db.Randevular.FindAsync(randevuId);
            if (randevu == null)
                return (false, "Randevu bulunamadı.");

            randevu.Durum = AppointmentStatus.Tamamlandi;
            randevu.GuncellenmeTarihi = DateTime.Now;
            await _db.SaveChangesAsync();

            return (true, "Randevu tamamlandı olarak işaretlendi.");
        }

        /// <summary>
        /// Çakışma kontrolü — doktorun belirtilen zaman aralığında başka onaylı/bekleyen randevusu var mı?
        /// </summary>
        public async Task<bool> CakismaVarMiAsync(string doktorId, DateTime baslangic, DateTime bitis, int? haricRandevuId = null)
        {
            var sorgu = _db.Randevular.Where(r =>
                r.DoktorId == doktorId &&
                r.Durum != AppointmentStatus.Iptal &&
                r.Durum != AppointmentStatus.Reddedildi &&
                r.BaslangicZamani < bitis &&
                r.BitisZamani > baslangic);

            if (haricRandevuId.HasValue)
                sorgu = sorgu.Where(r => r.Id != haricRandevuId.Value);

            return await sorgu.AnyAsync();
        }

        /// <summary>
        /// Doktorun belirtilen tarihteki uygun randevu saatlerini hesaplar.
        /// </summary>
        public async Task<IEnumerable<DateTime>> UygunSaatleriGetirAsync(string doktorId, DateTime tarih, int hizmetSureDakika)
        {
            var gun = (HaftaninGunu)(int)tarih.DayOfWeek;

            var zamanDilimleri = await _db.UygunlukZamanDilimleri
                .Where(z => z.DoktorId == doktorId && z.Gun == gun && z.AktifMi)
                .ToListAsync();

            var mevcutRandevular = await _db.Randevular
                .Where(r => r.DoktorId == doktorId
                         && r.BaslangicZamani.Date == tarih.Date
                         && r.Durum != AppointmentStatus.Iptal
                         && r.Durum != AppointmentStatus.Reddedildi)
                .ToListAsync();

            var uygunSaatler = new List<DateTime>();

            foreach (var dilim in zamanDilimleri)
            {
                var suankiZaman = tarih.Date.Add(dilim.BaslangicSaati.ToTimeSpan());
                var dilimBitis = tarih.Date.Add(dilim.BitisSaati.ToTimeSpan());

                while (suankiZaman.AddMinutes(hizmetSureDakika) <= dilimBitis)
                {
                    var slotBitis = suankiZaman.AddMinutes(hizmetSureDakika);

                    // Bu slotta çakışma var mı?
                    var cakismaVar = mevcutRandevular.Any(r =>
                        r.BaslangicZamani < slotBitis && r.BitisZamani > suankiZaman);

                    // Geçmişte mi?
                    var gecmisteMi = suankiZaman < DateTime.Now;

                    if (!cakismaVar && !gecmisteMi)
                        uygunSaatler.Add(suankiZaman);

                    suankiZaman = suankiZaman.AddMinutes(hizmetSureDakika);
                }
            }

            return uygunSaatler;
        }
    }
}
