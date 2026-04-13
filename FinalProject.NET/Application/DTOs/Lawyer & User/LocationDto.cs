using System.ComponentModel.DataAnnotations;

namespace FinalProject.NET.Dtos
{
    public class LocationDto
    {
        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Government is required.")]
        [StringLength(100, ErrorMessage = "Government cannot exceed 100 characters.")]
        public string Government { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Street is required.")]
        [StringLength(200, ErrorMessage = "Street cannot exceed 200 characters.")]
        public string Street { get; set; }
    }
}
