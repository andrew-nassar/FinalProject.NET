using FinalProject.NET.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.NET.DBcontext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<DocumentVerification> DocumentVerifications { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Lawyer> Lawyers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<LawyerSpecialization> LawyerSpecializations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 🔗 DocumentVerification ↔ Person
            builder.Entity<DocumentVerification>()
                .HasOne(d => d.Person)
                .WithMany(p => p.Documents)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ منع تكرار نوع الوثيقة لنفس الشخص
            builder.Entity<DocumentVerification>()
                .HasIndex(d => new { d.PersonId, d.DocumentType })
                .IsUnique();

            // 🔗 Lawyer ↔ Location
            builder.Entity<Lawyer>()
                .HasOne(l => l.OfficeLocation)
                .WithMany(loc => loc.Lawyers)
                .HasForeignKey(l => l.OfficeLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔗 LawyerSpecialization ↔ Lawyer & Specialization
            builder.Entity<LawyerSpecialization>()
                .HasKey(ls => new { ls.LawyerId, ls.SpecializationId });

            builder.Entity<LawyerSpecialization>()
                .HasOne(ls => ls.Lawyer)
                .WithMany(l => l.LawyerSpecializations)
                .HasForeignKey(ls => ls.LawyerId);

            builder.Entity<LawyerSpecialization>()
                .HasOne(ls => ls.Specialization)
                .WithMany(s => s.LawyerSpecializations)
                .HasForeignKey(ls => ls.SpecializationId);

            // 🧾 خصائص نصية: تحديد الطول
            builder.Entity<Person>()
                .Property(p => p.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Entity<Person>()
                .Property(p => p.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Entity<DocumentVerification>()
                .Property(d => d.FilePath)
                .HasMaxLength(300)
                .IsRequired();

            builder.Entity<DocumentVerification>()
                .Property(d => d.Notes)
                .HasMaxLength(500);
        }



    }
}
