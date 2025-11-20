namespace FinalProject.NET.Models
{
    public class User : Person
    {
        public User()
        {
            Role = DBcontext.Role.User; // 👈 تعيين الدور الافتراضي
        }
    }
}
