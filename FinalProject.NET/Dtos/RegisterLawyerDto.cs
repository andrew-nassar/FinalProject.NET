using System.ComponentModel.DataAnnotations;

namespace FinalProject.NET.Dtos
{
    public class RegisterLawyerDto : RegisterCommonDto
    {
        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "About section is required.")]
        [StringLength(1000, ErrorMessage = "About section cannot exceed 1000 characters.")]
        public string About { get; set; }

        [Required(ErrorMessage = "Years of experience is required.")]
        [Range(0, 100, ErrorMessage = "Years of experience must be between 0 and 100.")]
        public int YearsOfExperience { get; set; }

        [Required(ErrorMessage = "At least one specialization must be selected.")]
        public List<Guid> Specializations { get; set; } = new();

        [Required(ErrorMessage = "ID front photo is required.")]
        public IFormFile IdFront { get; set; }

        [Required(ErrorMessage = "ID back photo is required.")]
        public IFormFile IdBack { get; set; }

        [Required(ErrorMessage = "Selfie with ID is required.")]
        public IFormFile SelfieWithId { get; set; }

        [Required(ErrorMessage = "License photo is required.")]
        public IFormFile LicensePhoto { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        public LocationDto Location { get; set; }
    }
}
