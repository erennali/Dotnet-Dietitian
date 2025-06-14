using Dotnet_Dietitian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dotnet_Dietitian.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<AppRole> AppRoles { get; set; }
    public DbSet<Diyetisyen> Diyetisyenler { get; set; }
    public DbSet<Hasta> Hastalar { get; set; }
    public DbSet<DiyetProgrami> DiyetProgramlari { get; set; }
    public DbSet<OdemeBilgisi> OdemeBilgileri { get; set; }
    public DbSet<Randevu> Randevular { get; set; }
    public DbSet<DiyetisyenUygunluk> DiyetisyenUygunluklar { get; set; }    public DbSet<Mesaj> Mesajlar { get; set; } // Mevcut DbSet'lere ekleyin
    public DbSet<KullaniciAyarlari> KullaniciAyarlari { get; set; }
    public DbSet<IlerlemeOlcum> IlerlemeOlcumleri { get; set; }
    public DbSet<PaymentRequest> PaymentRequests { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Diyetisyen yapılandırması
        modelBuilder.Entity<Diyetisyen>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TcKimlikNumarasi).HasMaxLength(11).IsRequired();
            entity.Property(e => e.Ad).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Soyad).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Telefon).HasMaxLength(20);
            entity.Property(e => e.Puan).HasDefaultValue(0);
            entity.Property(e => e.ToplamYorumSayisi).HasDefaultValue(0);
            
            entity.HasIndex(e => e.TcKimlikNumarasi).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Telefon).IsUnique();
        });
        
        // Hasta yapılandırması
        modelBuilder.Entity<Hasta>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TcKimlikNumarasi).HasMaxLength(11).IsRequired();
            entity.Property(e => e.Ad).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Soyad).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Telefon).HasMaxLength(20);
            
            entity.HasIndex(e => e.TcKimlikNumarasi).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Telefon).IsUnique();
            
            entity.HasOne(h => h.Diyetisyen)
                    .WithMany(d => d.Hastalar)
                    .HasForeignKey(h => h.DiyetisyenId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
            entity.HasOne(h => h.DiyetProgrami)
                    .WithMany(dp => dp.Hastalar)
                    .HasForeignKey(h => h.DiyetProgramiId)
                    .OnDelete(DeleteBehavior.SetNull);
        });
        
        // DiyetProgrami yapılandırması
        modelBuilder.Entity<DiyetProgrami>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Ad).HasMaxLength(100).IsRequired();
            
            entity.HasOne(dp => dp.OlusturanDiyetisyen)
                    .WithMany(d => d.OlusturulanProgramlar)
                    .HasForeignKey(dp => dp.OlusturanDiyetisyenId)
                    .OnDelete(DeleteBehavior.SetNull);
        });
        
        // OdemeBilgisi yapılandırması
        modelBuilder.Entity<OdemeBilgisi>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tutar).HasPrecision(10, 2).IsRequired();
            entity.Property(e => e.Tarih).IsRequired();
            entity.Property(e => e.OdemeTuru).HasMaxLength(50);
            
            entity.HasOne(o => o.Hasta)
                    .WithMany(h => h.Odemeler)
                    .HasForeignKey(o => o.HastaId)
                    .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Randevu yapılandırması
        modelBuilder.Entity<Randevu>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RandevuBaslangicTarihi).IsRequired();
            entity.Property(e => e.RandevuBitisTarihi).IsRequired();
            entity.Property(e => e.RandevuTuru).HasMaxLength(50);
            entity.Property(e => e.Durum).HasMaxLength(50).HasDefaultValue("Bekliyor");
            entity.Property(e => e.DiyetisyenOnayi).HasDefaultValue(false);
            entity.Property(e => e.HastaOnayi).HasDefaultValue(false);
            
            entity.HasOne(r => r.Hasta)
                    .WithMany(h => h.Randevular)
                    .HasForeignKey(r => r.HastaId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
            entity.HasOne(r => r.Diyetisyen)
                    .WithMany(d => d.Randevular)
                    .HasForeignKey(r => r.DiyetisyenId)
                    .OnDelete(DeleteBehavior.Cascade);
        });
        
        // DiyetisyenUygunluk yapılandırması
        modelBuilder.Entity<DiyetisyenUygunluk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Gun).IsRequired();
            entity.Property(e => e.BaslangicSaati).IsRequired();
            entity.Property(e => e.BitisSaati).IsRequired();
            entity.Property(e => e.TekrarTipi).HasMaxLength(20);
            
            entity.HasOne(du => du.Diyetisyen)
                    .WithMany(d => d.UygunlukZamanlari)
                    .HasForeignKey(du => du.DiyetisyenId)
                    .OnDelete(DeleteBehavior.Cascade);
        });
          // Mesaj yapılandırması
        modelBuilder.Entity<Mesaj>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GonderenId).IsRequired();
            entity.Property(e => e.GonderenTipi).HasMaxLength(20).IsRequired();
            entity.Property(e => e.AliciId).IsRequired();
            entity.Property(e => e.AliciTipi).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Icerik).IsRequired();
            entity.Property(e => e.GonderimZamani).IsRequired();            entity.Property(e => e.Okundu).HasDefaultValue(false);
        });
          // KullaniciAyarlari yapılandırması
        modelBuilder.Entity<KullaniciAyarlari>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.KullaniciId).IsRequired();
            entity.Property(e => e.KullaniciTipi).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SonGuncellemeTarihi).IsRequired();
            
            // Unique index to ensure one settings record per user
            entity.HasIndex(e => new { e.KullaniciId, e.KullaniciTipi }).IsUnique();
        });
        
        // PaymentRequest yapılandırması
        modelBuilder.Entity<PaymentRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HastaId).IsRequired();
            entity.Property(e => e.DiyetisyenId).IsRequired();
            entity.Property(e => e.DiyetProgramiId).IsRequired();
            entity.Property(e => e.Tutar).HasPrecision(10, 2).IsRequired();
            entity.Property(e => e.Durum).IsRequired();
            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.RedNotu).HasMaxLength(500);
            
            entity.HasOne(pr => pr.Hasta)
                    .WithMany()
                    .HasForeignKey(pr => pr.HastaId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
            entity.HasOne(pr => pr.Diyetisyen)
                    .WithMany()
                    .HasForeignKey(pr => pr.DiyetisyenId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
            entity.HasOne(pr => pr.DiyetProgrami)
                    .WithMany()
                    .HasForeignKey(pr => pr.DiyetProgramiId)
                    .OnDelete(DeleteBehavior.Restrict);
                      entity.HasOne(pr => pr.OdemeBilgisi)
                    .WithOne()
                    .HasForeignKey<PaymentRequest>(pr => pr.OdemeBilgisiId)
                    .OnDelete(DeleteBehavior.NoAction);
        });
        
        base.OnModelCreating(modelBuilder);
    }
}