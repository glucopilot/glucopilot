using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace GlucoPilot.Data
{
    public interface IDatabaseFacade
    {
        Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken);
        Task MigrateAsync(CancellationToken cancellationToken);
    }
}
