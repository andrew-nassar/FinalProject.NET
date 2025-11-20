using Microsoft.EntityFrameworkCore;

namespace FinalProject.NET.Models
{

    [Index(nameof(Name), IsUnique = true)]
    public class Specialization
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public ICollection<LawyerSpecialization> LawyerSpecializations { get; set; }
    }

    public class LawyerSpecialization
    {
        public Guid LawyerId { get; set; }
        public Lawyer Lawyer { get; set; }

        public Guid SpecializationId { get; set; }
        public Specialization Specialization { get; set; }
    }

}
