namespace FinalProject.NET.Models
{
    public class LawyerInfo
    {
        public Guid Id { get; set; }       // Primary Key = LawyerId بالظبط
        // Navigation Property
        public Lawyer Lawyer { get; set; }
        public string PhoneNumber { get; set; }
        public string About { get; set; }
        public int YearsOfExperience { get; set; }
        public string NationalId { get; set; }

        public string? Personal_Photo_Url { get; set; }
    }
}
