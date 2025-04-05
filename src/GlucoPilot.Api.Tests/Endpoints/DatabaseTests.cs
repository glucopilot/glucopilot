using GlucoPilot.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlucoPilot.Api.Tests.Endpoints
{
    internal class DatabaseTests
    {
        protected SqliteConnection _connection;
        protected GlucoPilotDbContext _dbContext;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<GlucoPilotDbContext>()
                .UseSqlite(_connection)
                .Options;
            _dbContext = new GlucoPilotDbContext(options);
            _dbContext.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            _connection?.Dispose();
            _dbContext?.Dispose();
        }
    }
}
