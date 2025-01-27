using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FrizerskiSalon_VSITE.Models;

namespace FrizerskiSalon_VSITE.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Service> Services { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracija za decimalnu preciznost
            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            // Konfiguracija za vezu između rezervacija i korisnika
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Konfiguracija za vezu između rezervacija i usluga
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Service)
                .WithMany()
                .HasForeignKey(r => r.ServiceId)
                .OnDelete(DeleteBehavior.Restrict); // Ovdje možete odabrati ponašanje brisanja
        }
    }
}
