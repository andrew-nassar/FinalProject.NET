using System.Data;
using FinalProject.NET.DBcontext;
using Microsoft.AspNetCore.Identity;

namespace FinalProject.NET.Infrastructure.Data.Entities
{
    public class Person : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Role Role { get; set; }
        // statuses
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
