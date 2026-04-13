using System.ComponentModel.DataAnnotations;
using FinalProject.NET.DBcontext;

namespace FinalProject.NET.Dtos
{
    public class UpdateDocumentsDto
    {
        [Required(ErrorMessage = "Please select one of the following statuses: Pending, Approved, or Rejected.")]
        [EnumDataType(typeof(VerificationStatus), ErrorMessage = "Invalid status. Choose Pending = 0 , Approved = 1 , or Rejected = 2.")]
        public VerificationStatus Status { get; set; }   // nullable helps show validation message
    }
}
