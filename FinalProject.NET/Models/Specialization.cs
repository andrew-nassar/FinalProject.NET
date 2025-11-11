namespace FinalProject.NET.Models
{
    public class Specialization
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public ICollection<LawyerSpecialization> LawyerSpecializations { get; set; }
    }

    public class LawyerSpecialization
    {
        public string LawyerId { get; set; }
        public Lawyer Lawyer { get; set; }

        public string SpecializationId { get; set; }
        public Specialization Specialization { get; set; }
    }

}
