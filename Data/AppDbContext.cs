using HastaneRandevu.Models.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HastaneRandevu.Data
{
    /// <summary>
    /// Entity Framework Core veritabanı bağlamı.
    /// TPH (Table-Per-Hierarchy) stratejisi ile kalıtım hiyerarşisini yönetir.
    /// </summary>
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet'ler — her tablo
        public DbSet<SaglikKurumu> SaglikKurumlari { get; set; }
        public DbSet<Hastane> Hastaneler { get; set; }
        public DbSet<Klinik> Klinikler { get; set; }
        public DbSet<Bolum> Bolumler { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<DoktorHizmet> DoktorHizmetler { get; set; }
        public DbSet<Hasta> Hastalar { get; set; }
        public DbSet<Doktor> Doktorlar { get; set; }
        public DbSet<Admin> Adminler { get; set; }
        public DbSet<UygunlukZamanDilimi> UygunlukZamanDilimleri { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // TPH: SaglikKurumu hiyerarşisi — discriminator kolonu
            builder.Entity<SaglikKurumu>()
                .HasDiscriminator<string>("KurumTipi")
                .HasValue<Hastane>("Hastane")
                .HasValue<Klinik>("Klinik");

            // ApplicationUser TPH hiyerarşisi
            builder.Entity<ApplicationUser>()
                .HasDiscriminator<string>("KullaniciTipi")
                .HasValue<ApplicationUser>("Base")
                .HasValue<Hasta>("Hasta")
                .HasValue<Doktor>("Doktor")
                .HasValue<Admin>("Admin");

            // Randevu → Hasta (Restrict delete to avoid cascade conflict)
            builder.Entity<Randevu>()
                .HasOne(r => r.Hasta)
                .WithMany(h => h.Randevular)
                .HasForeignKey(r => r.HastaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Randevu → Doktor
            builder.Entity<Randevu>()
                .HasOne(r => r.Doktor)
                .WithMany(d => d.Randevular)
                .HasForeignKey(r => r.DoktorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Randevu → SaglikKurumu
            builder.Entity<Randevu>()
                .HasOne(r => r.Kurum)
                .WithMany()
                .HasForeignKey(r => r.KurumId)
                .OnDelete(DeleteBehavior.Restrict);

            // DoktorHizmet — composite primary key
            builder.Entity<DoktorHizmet>()
                .HasKey(dh => new { dh.DoktorId, dh.HizmetId });

            builder.Entity<DoktorHizmet>()
                .HasOne(dh => dh.Doktor)
                .WithMany(d => d.DoktorHizmetler)
                .HasForeignKey(dh => dh.DoktorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DoktorHizmet>()
                .HasOne(dh => dh.Hizmet)
                .WithMany(h => h.DoktorHizmetler)
                .HasForeignKey(dh => dh.HizmetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Doktor → Bolum
            builder.Entity<Doktor>()
                .HasOne(d => d.Bolum)
                .WithMany(b => b.Doktorlar)
                .HasForeignKey(d => d.BolumId)
                .OnDelete(DeleteBehavior.SetNull);

            // Doktor → Kurum
            builder.Entity<Doktor>()
                .HasOne(d => d.Kurum)
                .WithMany(k => k.Doktorlar)
                .HasForeignKey(d => d.KurumId)
                .OnDelete(DeleteBehavior.SetNull);

            // Bolum → Kurum
            builder.Entity<Bolum>()
                .HasOne(b => b.Kurum)
                .WithMany(k => k.Bolumler)
                .HasForeignKey(b => b.KurumId)
                .OnDelete(DeleteBehavior.Cascade);

            // UygunlukZamanDilimi → Doktor
            builder.Entity<UygunlukZamanDilimi>()
                .HasOne(u => u.Doktor)
                .WithMany(d => d.UygunlukZamanDilimleri)
                .HasForeignKey(u => u.DoktorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Admin → Kurum
            builder.Entity<Admin>()
                .HasOne(a => a.YonettigiKurum)
                .WithMany()
                .HasForeignKey(a => a.YonettigiKurumId)
                .OnDelete(DeleteBehavior.SetNull);

            // Decimal precision
            builder.Entity<Hizmet>()
                .Property(h => h.Ucret)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Randevu>()
                .Property(r => r.UcretMiktari)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Doktor>()
                .Property(d => d.MuayeneUcreti)
                .HasColumnType("decimal(18,2)");

            // Tablolara Türkçe isim ver
            builder.Entity<SaglikKurumu>().ToTable("SaglikKurumlari");
            builder.Entity<Bolum>().ToTable("Bolumler");
            builder.Entity<Hizmet>().ToTable("Hizmetler");
            builder.Entity<DoktorHizmet>().ToTable("DoktorHizmetler");
            builder.Entity<UygunlukZamanDilimi>().ToTable("UygunlukZamanDilimleri");
            builder.Entity<Randevu>().ToTable("Randevular");
        }
    }
}
