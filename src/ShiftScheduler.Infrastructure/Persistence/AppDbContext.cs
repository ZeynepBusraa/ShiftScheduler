using Microsoft.EntityFrameworkCore;
using ShiftScheduler.Domain.Entities;
using System.Reflection;

namespace ShiftScheduler.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftRequest> ShiftRequests => Set<ShiftRequest>();
    public DbSet<Department> Departments => Set<Department>();
    
    // BAŞHEKİM ONAY SİSTEMİ İÇİN YENİ EKLENEN TABLO:
    public DbSet<ShiftList> ShiftLists => Set<ShiftList>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Nöbet Listesi ve Hazırlayan Asistan İlişkisi (Silinme çakışmalarını önlemek için Restrict yapıyoruz)
        modelBuilder.Entity<ShiftList>()
            .HasOne(sl => sl.PreparedByUser)
            .WithMany(u => u.PreparedShiftLists)
            .HasForeignKey(sl => sl.PreparedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Nöbet ve Nöbet Listesi İlişkisi
        modelBuilder.Entity<Shift>()
            .HasOne(s => s.ShiftList)
            .WithMany(sl => sl.Shifts)
            .HasForeignKey(s => s.ShiftListId)
            .OnDelete(DeleteBehavior.Cascade);

        // YENİ EKLENEN KISIM: Nöbet Değişim Taleplerindeki çoklu silme (Cascade) çakışmasını engelliyoruz
        modelBuilder.Entity<ShiftRequest>()
            .HasOne(sr => sr.Requester)
            .WithMany()
            .HasForeignKey(sr => sr.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShiftRequest>()
            .HasOne(sr => sr.TargetDoctor)
            .WithMany()
            .HasForeignKey(sr => sr.TargetDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // İlgili assembly'deki tüm IEntityTypeConfiguration implementasyonlarını otomatik olarak uygular
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "Göğüs Cerrahisi" }
        );

    }
}