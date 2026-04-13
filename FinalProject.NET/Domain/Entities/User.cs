using FinalProject.NET.DBcontext;

namespace FinalProject.NET.Infrastructure.Data.Entities
{
    public class User : Person
    {
        public User()
        {
            Role = Role.User; // 👈 تعيين الدور الافتراضي
        }
    }
}
