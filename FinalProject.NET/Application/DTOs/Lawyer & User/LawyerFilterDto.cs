namespace FinalProject.NET.Dtos
{
    public class LawyerFilterDto
    {
        public List<Guid>? SpecializationIds { get; set; }
        public string? Country { get; set; }
        public string? Government { get; set; }
        public string? City { get; set; }
    }

}
