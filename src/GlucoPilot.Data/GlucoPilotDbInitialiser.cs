using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace GlucoPilot.Data
{
    public sealed class GlucoPilotDbInitialiser
    {
        private readonly GlucoPilotDbContext _db;

        public GlucoPilotDbInitialiser(GlucoPilotDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task InitialiseDbAsync(CancellationToken cancellationToken)
        {
            var pendingMigrations = (await _db.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToArray();
            if (pendingMigrations.Any())
            {
                await _db.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}