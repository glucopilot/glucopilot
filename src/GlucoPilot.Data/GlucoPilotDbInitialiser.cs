using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace GlucoPilot.Data
{
    public sealed class GlucoPilotDbInitialiser
    {
        private readonly IDatabaseFacade _db;

        public GlucoPilotDbInitialiser(IDatabaseFacade db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task InitialiseDbAsync(CancellationToken cancellationToken)
        {
            var pendingMigrations = (await _db.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToArray();
            if (pendingMigrations.Length > 0)
            {
                await _db.MigrateAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}