using FinalProject.NET.DBcontext;

namespace FinalProject.NET.Dtos
{
    public class LawyerStatusUpdateDto
    {
        public Guid LawyerId { get; set; }
        public AccountStatus Status { get; set; }  // Active / Inactive
        public string? UID { get; set; }           // Optional, فقط Admin أو Activator يمكن إضافته
    }
}
