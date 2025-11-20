using FinalProject.NET.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace FinalProject.NET.DBcontext
{
    public class AppDbContext : IdentityDbContext<Person, IdentityRole<Guid>, Guid>
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
                .HasOne(d => d.Lawyer)
                .WithMany(l => l.Documents)
                .HasForeignKey(d => d.LawyerId)
                .OnDelete(DeleteBehavior.Cascade);


            // ✅ منع تكرار نوع الوثيقة لنفس الشخص
            builder.Entity<DocumentVerification>()
                .HasIndex(d => new { d.LawyerId, d.DocumentType })
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

            builder.Entity<Lawyer>()
                .HasIndex(l => l.UID)
                .IsUnique();

            builder.Entity<DocumentVerification>()
                .HasOne(d => d.ReviewedBy)
                .WithMany()
                .HasForeignKey(d => d.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict);

            // DocumentVerification enums
            builder.Entity<DocumentVerification>()
                .Property(d => d.DocumentType)
                .HasConversion<string>();

            builder.Entity<DocumentVerification>()
                .Property(d => d.Status)
                .HasConversion<string>();

            // Person enums
            builder.Entity<Person>()
                .Property(p => p.Role)
                .HasConversion<string>();

            builder.Entity<Person>()
                .Property(p => p.AccountStatus)
                .HasConversion<string>();


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
                .Property(d => d.FileUrl)
                .HasMaxLength(300)
                .IsRequired();

            builder.Entity<DocumentVerification>()
                .Property(d => d.Notes)
                .HasMaxLength(500);
            // ✅ Seed Specializations from SpecializationType enum with deterministic GUIDs
            var specializations = Enum.GetValues(typeof(SpecializationType))
                .Cast<SpecializationType>()
                .Select(s => new Specialization
                {
                    Id = Guid.ParseExact($"{(int)s:D32}", "N"), // deterministic GUID
                    Name = s.ToString()
                })
            .ToList();

            builder.Entity<Specialization>().HasData(specializations);
        }



    }
}
