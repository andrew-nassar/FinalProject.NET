using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using FinalProject.NET.DBcontext;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.NET.Infrastructure.Data.Entities
{
    [Index(nameof(LawyerId), nameof(DocumentType), IsUnique = true)]
    public class DocumentVerification
    {
        public Guid Id { get; set; }

        public DocumentType DocumentType { get; set; }
        public string FileUrl { get; set; }
        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
        public string? Notes { get; set; }

        public Guid LawyerId { get; set; }
        [ForeignKey("LawyerId")]
        public Lawyer Lawyer { get; set; }

        public Guid? ReviewedById { get; set; }
        public Person? ReviewedBy { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

    }
}


