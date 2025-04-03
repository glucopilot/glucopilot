using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GlucoPilot.Data.Tests;

[TestFixture]
internal sealed class GlucoPilotDbContextTests
{
    private SqliteConnection _connection;
    private GlucoPilotDbContext _dbContext;
    
    [Test]
    public void Context_Can_Be_Created()
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