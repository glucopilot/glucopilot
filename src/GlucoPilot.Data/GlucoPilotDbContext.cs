using Microsoft.EntityFrameworkCore;

namespace GlucoPilot.Data
{
    public class GlucoPilotDbContext : DbContext
    {
        public GlucoPilotDbContext(DbContextOptions<GlucoPilotDbContext> options) : base(options)
        {
        }
    }
}
