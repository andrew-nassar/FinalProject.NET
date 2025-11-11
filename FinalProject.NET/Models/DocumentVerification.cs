using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using FinalProject.NET.DBcontext;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.NET.Models
{
    [Index(nameof(PersonId), nameof(DocumentType), IsUnique = true)]

    public class DocumentVerification
    {
        public string Id { get; set; }

        // نوع الوثيقة: بطاقة أمامية، خلفية، سيلفي، كارنيه...
        public DocumentType DocumentType { get; set; }

        // مسار الصورة أو رابطها
        public string FilePath { get; set; }

        // حالة التحقق
        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

        // ملاحظات من المشرف (لو تم الرفض أو فيه مشكلة)
        public string Notes { get; set; }

        // الربط بالمستخدم أو المحامي
        public string PersonId { get; set; }  // لو دمجت Person مع IdentityUser
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        public string ReviewedById { get; set; }
        public Person ReviewedBy { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    }
}


