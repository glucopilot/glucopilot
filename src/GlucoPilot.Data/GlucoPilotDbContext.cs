using GlucoPilot.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Data
{
    public class GlucoPilotDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }

        public GlucoPilotDbContext(DbContextOptions<GlucoPilotDbContext> options) : base(options)
        {
        }
    }
}
