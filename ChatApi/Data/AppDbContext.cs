using Microsoft.EntityFrameworkCore;

namespace ChatApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        public DbSet<Message> Messages { get; set; }
    }
}
