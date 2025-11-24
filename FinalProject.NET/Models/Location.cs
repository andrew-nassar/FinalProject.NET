using Microsoft.EntityFrameworkCore;

namespace FinalProject.NET.Models
{
    [Index(nameof(Country), nameof(Government), nameof(City))]
    public class Location
    {
        public Guid Id { get; set; }
        public string Country { get; set; }
        public string Government { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public ICollection<Lawyer> Lawyers { get; set; } = new List<Lawyer>();

    }


}
