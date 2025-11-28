using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.NET.Models
{
    public class LawyerInfo
    {

        [Key, ForeignKey("Lawyer")]
        public Guid Id { get; set; }       // Primary Key = LawyerId بالظبط
        // Navigation Property
        public Lawyer Lawyer { get; set; }
        public string PhoneNumber { get; set; }
        public string About { get; set; }
        public int YearsOfExperience { get; set; }

        public string? Personal_Photo_Url { get; set; }
    }
}
