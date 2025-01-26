using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FrizerskiSalon_VSITE.Models;

namespace FrizerskiSalon_VSITE.Data
{
    public class ApplicationDbContext : IdentityDbContext<User> // Koristimo svoj User model koji nasljeđuje IdentityUser
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet za modele
        public DbSet<Service> Services { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        // Podesiti preciznost za decimalna polja
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);  // Pozivanje osnovne konfiguracije

            // Konfiguracija za decimal (Price u Service)
            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)"); // Preciznost i skala za cijenu

            // Konfiguracija za stranog ključa (Reservation -> User)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Ako brišete korisnika, brišu se i povezane rezervacije
        }
    }
}
