using System.ComponentModel.DataAnnotations;

namespace GlucoPilot.Data;

/// <summary>
/// Represents the options for configuring the database connection.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Represents the keys for different database providers.
    /// </summary>
    public static class DatabaseProviderKeys
    {
        /// <summary>
        /// Represents the key for MSSQL Server database provider.
        /// </summary>
        public const string SqlServer = "mssql";
    }

    /// <summary>
    /// Database connection string.
    /// </summary>
    [Required]
    public required string ConnectionString { get; init; }

    /// <summary>
    /// Database provider - must match a key in the <see cref="DatabaseProviderKeys"/> class.
    /// </summary>
    [Required]
    public required string DbProvider { get; init; }
}