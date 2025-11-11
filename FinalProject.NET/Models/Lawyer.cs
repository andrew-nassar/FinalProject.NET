using FinalProject.NET.Models;

public class Lawyer : Person
{

    // ID photos (store file paths or URLs)
    public ICollection<DocumentVerification> Documents { get; set; }
    // الربط بالـ Location
    public string OfficeLocationId { get; set; }      // FK
    public Location OfficeLocation { get; set; }  // الآن الموقع ككلاس منفصل
    public string PhoneNumber { get; set; }
    public string About { get; set; }
    // روابط للتخصصات
    public ICollection<LawyerSpecialization> LawyerSpecializations { get; set; }
    public Lawyer()
    {
        Role = FinalProject.NET.DBcontext.Role.Lawyer; // 👈 تعيين الدور الافتراضي
    }
    public int YearsOfExperience { get; set; }
}

