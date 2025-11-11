using FinalProject.NET.DBcontext;

namespace FinalProject.NET.Dtos
{
    // Step1: إرسال البريد للتحقق
    public class VerifyEmailDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    // Step2: تأكيد رمز التحقق
    public class ConfirmEmailDto
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

    // Step3: رفع كل البيانات النهائية لإنشاء الحساب
    public class RegisterCompleteDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }

        public List<DocumentDto> Documents { get; set; }
        public List<string> Specializations { get; set; } // فقط للمحامي

        public string VerificationCode { get; set; } // تم التأكد من البريد
    }

    public class DocumentDto
    {
        public DocumentType DocumentType { get; set; }
        public string FilePath { get; set; }
    }

}
