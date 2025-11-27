using FinalProject.NET.Models;
using Microsoft.EntityFrameworkCore;

[Index(nameof(OfficeLocationId)), Index(nameof(NationalId), IsUnique = true)]
public class Lawyer : Person
{
    public Guid OfficeLocationId { get; set; }
    public Location OfficeLocation { get; set; }
    public string? NationalId { get; set; }


    public ICollection<DocumentVerification> Documents { get; set; } = new List<DocumentVerification>();

    public ICollection<LawyerSpecialization> LawyerSpecializations { get; set; } = new List<LawyerSpecialization>();

    public Lawyer()
    {
        Role = FinalProject.NET.DBcontext.Role.Lawyer;
    }
    // 👇 One-to-One Navigation
    public LawyerInfo LawyerInfo { get; set; }

}

