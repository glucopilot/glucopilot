using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlucoPilot.Data
{
    public sealed class DatabaseFacade : IDatabaseFacade
    {
        private readonly Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade _databaseFacade;

        public DatabaseFacade(Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade databaseFacade)
        {
            _databaseFacade = databaseFacade;
        }

        public Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken)
        {
            return _databaseFacade.GetPendingMigrationsAsync(cancellationToken);
        }

        public Task MigrateAsync(CancellationToken cancellationToken)
        {
            return _databaseFacade.MigrateAsync(cancellationToken);
        }
    }
}
