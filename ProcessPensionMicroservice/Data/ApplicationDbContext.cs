using ProcessPensionMicroservice.Models;
using Microsoft.EntityFrameworkCore;

namespace ProcessPensionMicroservice.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<PensionerDetail> PensionerDetails { get; set; }
    }
}
