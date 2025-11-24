using FinalProject.NET.Models;
using Microsoft.EntityFrameworkCore;

[Index(nameof(OfficeLocationId))]
public class Lawyer : Person
{
    public Guid OfficeLocationId { get; set; }
    public Location OfficeLocation { get; set; }

    public string PhoneNumber { get; set; }
    public string About { get; set; }
    public ICollection<DocumentVerification> Documents { get; set; } = new List<DocumentVerification>();

    public string UID { get; set; }
    public ICollection<LawyerSpecialization> LawyerSpecializations { get; set; } = new List<LawyerSpecialization>();

    public Lawyer()
    {
        Role = FinalProject.NET.DBcontext.Role.Lawyer;
    }

    public int YearsOfExperience { get; set; }
}

