using System.Reflection;

namespace GlucoPilot.Nutrition.Data.Migrators.MSSQL;

internal static class Functions
{
    public static string ReadSql(string scriptName)
    {
        return File.ReadAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            @$"Scripts{Path.DirectorySeparatorChar}{scriptName}.sql"));
    }
}