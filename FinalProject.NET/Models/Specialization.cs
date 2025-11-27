using Microsoft.EntityFrameworkCore;

namespace FinalProject.NET.Models
{

    [Index(nameof(Name), IsUnique = true)]
    public class Specialization
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }

        public required ICollection<LawyerSpecialization> LawyerSpecializations { get; set; }
    }

    [Index(nameof(LawyerId), nameof(SpecializationId), IsUnique = true)]
    public class LawyerSpecialization
    {
        public Guid LawyerId { get; set; }
        public required Lawyer Lawyer { get; set; }

        public Guid SpecializationId { get; set; }
        public required Specialization Specialization { get; set; }
    }

}
