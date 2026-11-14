namespace HastaneRandevu.Models.Enums
{
    /// <summary>
    /// Sistemdeki kullanıcı rollerini temsil eder.
    /// </summary>
    public enum UserRole
    {
        Hasta = 0,
        Doktor = 1,
        Admin = 2
    }

    /// <summary>
    /// Sağlık kurumu türünü temsil eder.
    /// </summary>
    public enum KurumTuru
    {
        Hastane = 0,
        Klinik = 1
    }

    /// <summary>
    /// Haftanın günleri (uygunluk tanımlamak için).
    /// </summary>
    public enum HaftaninGunu
    {
        Pazartesi = 1,
        Sali = 2,
        Carsamba = 3,
        Persembe = 4,
        Cuma = 5,
        Cumartesi = 6,
        Pazar = 0
    }
}
