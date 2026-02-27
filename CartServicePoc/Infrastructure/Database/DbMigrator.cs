using System.Reflection;
using DbUp;

namespace CartServicePoc.Infrastructure.Database;

public class DbMigrator
{
    public static void MigrateDatabase(string connectionString)
    {
        EnsureDatabase.For.SqlDatabase(connectionString);

        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .WithTransaction()
            .LogToConsole()
            .Build();

        if (upgrader.IsUpgradeRequired())
        {
            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
                throw new Exception("Database migration failed", result.Error);
        }
    }
}