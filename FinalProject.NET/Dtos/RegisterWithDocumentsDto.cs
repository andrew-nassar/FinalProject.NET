using FinalProject.NET.DBcontext;

namespace FinalProject.NET.Dtos
{
    public class RegisterWithDocumentsDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }

        public List<DocumentUploadDto> Documents { get; set; }
        public List<SpecializationType> Specializations { get; set; } // للمحامي فقط
    }
    public class DocumentUploadDto
    {
        public DocumentType DocumentType { get; set; }
        public string FilePath { get; set; }
    }

}
