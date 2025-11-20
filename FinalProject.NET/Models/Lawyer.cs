using FinalProject.NET.Models;

public class Lawyer : Person
{

    public Guid OfficeLocationId { get; set; }      
    public Location OfficeLocation { get; set; }  
    public string PhoneNumber { get; set; }
    public string About { get; set; }
    public ICollection<DocumentVerification> Documents { get; set; }
    
    public string UID { get; set; }
    public ICollection<LawyerSpecialization> LawyerSpecializations { get; set; }
    public Lawyer()
    {
        Role = FinalProject.NET.DBcontext.Role.Lawyer; 
    }
    public int YearsOfExperience { get; set; }

}

