namespace FinalProject.NET.Models
{
    public class Location
    {
        public Guid Id { get; set; } // ID تلقائي
        public string Country { get; set; }       // الدولة
        public string Government { get; set; }   // المحافظة
        public string City { get; set; }         // المدينة
        public string Street { get; set; }       // الشارع

        // Navigation property: كل المحامين اللي في المكان ده
        public ICollection<Lawyer> Lawyers { get; set; }
    }

}
