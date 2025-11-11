namespace FinalProject.NET.Models
{
    public class User : Person
    {
        // ID photos (store file paths or URLs)
        public ICollection<DocumentVerification> Documents { get; set; }
        public User()
        {
            Role = DBcontext.Role.User; // 👈 تعيين الدور الافتراضي
        }
    }
}
