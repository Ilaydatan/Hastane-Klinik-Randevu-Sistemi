namespace HastaneRandevu.Models.Enums
{
    /// <summary>
    /// Randevunun mevcut durumunu temsil eder.
    /// </summary>
    public enum AppointmentStatus
    {
        Bekliyor = 0,       // Hasta randevu oluşturdu, doktor onayı bekleniyor
        Onaylandi = 1,      // Doktor tarafından onaylandı
        Reddedildi = 2,     // Doktor tarafından reddedildi
        Iptal = 3,          // Hasta veya yönetici tarafından iptal edildi
        Tamamlandi = 4      // Randevu gerçekleşti
    }
}
