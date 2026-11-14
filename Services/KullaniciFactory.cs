using HastaneRandevu.Models.Domain;
using HastaneRandevu.Models.Enums;

namespace HastaneRandevu.Services
{
    /// <summary>
    /// Kullanıcı oluşturmak için Factory deseni.
    /// OOP: Factory Pattern — nesne oluşturma sorumluluğunu merkezi bir noktada toplar.
    /// </summary>
    public static class KullaniciFactory
    {
        /// <summary>
        /// Role göre doğru kullanıcı nesnesini oluşturur.
        /// Polimorfizm: Aynı çağrı farklı türde nesneler döndürür.
        /// </summary>
        public static ApplicationUser Olustur(UserRole rol, string ad, string soyad, string email, string telefon)
        {
            return rol switch
            {
                UserRole.Hasta => new Hasta
                {
                    Ad = ad,
                    Soyad = soyad,
                    Email = email,
                    UserName = email,
                    Telefon = telefon,
                    KullaniciRolu = UserRole.Hasta,
                    OlusturmaTarihi = DateTime.Now
                },
                UserRole.Doktor => new Doktor
                {
                    Ad = ad,
                    Soyad = soyad,
                    Email = email,
                    UserName = email,
                    Telefon = telefon,
                    KullaniciRolu = UserRole.Doktor,
                    OlusturmaTarihi = DateTime.Now
                },
                UserRole.Admin => new Admin
                {
                    Ad = ad,
                    Soyad = soyad,
                    Email = email,
                    UserName = email,
                    Telefon = telefon,
                    KullaniciRolu = UserRole.Admin,
                    OlusturmaTarihi = DateTime.Now
                },
                _ => throw new ArgumentException($"Bilinmeyen kullanıcı rolü: {rol}")
            };
        }

        /// <summary>
        /// Hastane veya Klinik nesnesi oluşturur.
        /// OOP: Factory Pattern — SaglikKurumu hiyerarşisi için.
        /// </summary>
        public static SaglikKurumu KurumOlustur(KurumTuru tur, string ad, string? adres = null)
        {
            return tur switch
            {
                KurumTuru.Hastane => new Hastane
                {
                    Ad = ad,
                    Adres = adres,
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now
                },
                KurumTuru.Klinik => new Klinik
                {
                    Ad = ad,
                    Adres = adres,
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now
                },
                _ => throw new ArgumentException($"Bilinmeyen kurum türü: {tur}")
            };
        }
    }
}
