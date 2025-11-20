namespace FinalProject.NET.DBcontext
{
    public enum VerificationStatus
    {
        Pending,
        Approved,
        Rejected
    }
    public enum AccountStatus { Active, Pending, Disabled }
    public enum EmailStatus { Active, Pending }
    public enum Role { Admin, Activator, User, Lawyer }
    [Flags] // يسمح باختيار أكثر من تخصص باستخدام الـ bitwise OR
    public enum SpecializationType
    {
        الجنائي = 1,
        المدني = 2,
        الأسرة = 4,
        الهجرة_والأجانب = 8,
        مجلس_الدولة = 16,
        تأسيس_الشركات = 32
    }
    public enum DocumentType
    {
        IdFront,
        IdBack,
        SelfieWithId,
        LicensePhoto
    }
}
