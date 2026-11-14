using HastaneRandevu.Data;
using HastaneRandevu.Models.Domain;
using HastaneRandevu.Services;
using HastaneRandevu.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════
// SERVİS KAYITLARI (Dependency Injection)
// ═══════════════════════════════════════════════════════

// MVC
builder.Services.AddControllersWithViews();

// Entity Framework Core + SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=hastane_randevu.db"));

// ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Şifre gereksinimleri
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;

    // Hesap kilitleme
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    // Kullanıcı
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cookie ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Giris";
    options.LogoutPath = "/Auth/Cikis";
    options.AccessDeniedPath = "/Auth/ErisimEngellendi";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
});

// OOP Servis Kayıtları (Interface → Implementation bağlantısı)
// Polimorfizm: IRandevuServisi istendiğinde RandevuServisi verilir
builder.Services.AddScoped<IRandevuServisi, RandevuServisi>();
builder.Services.AddScoped<IKurumServisi, KurumServisi>();
builder.Services.AddScoped<IKullaniciServisi, KullaniciServisi>();

// Session desteği
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ═══════════════════════════════════════════════════════
// MIDDLEWARE PIPELINE
// ═══════════════════════════════════════════════════════

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ROUTE TANIMLAMALARI
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ═══════════════════════════════════════════════════════
// DEMO VERİLERİ YÜKLE
// ═══════════════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    try
    {
        await SeedData.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Demo veriler yüklenirken hata oluştu.");
    }
}

app.Run();
