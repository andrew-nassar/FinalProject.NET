using System.Data;
using FinalProject.NET.DBcontext;
using Microsoft.AspNetCore.Identity;

namespace FinalProject.NET.Models
{
    public class Person : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Role Role { get; set; }
        // statuses
        public bool IsDeleted { get; set; } = false;
        public AccountStatus AccountStatus { get; set; } = AccountStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
