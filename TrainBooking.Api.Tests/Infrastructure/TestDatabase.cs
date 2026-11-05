using MySqlConnector;

namespace TrainBooking.Api.Tests.Infrastructure;

/// <summary>
/// A dedicated MySQL test database, rebuilt from the repository's <c>init.sql</c>.
/// Never points at the development database: the name must end with <c>_test</c>.
/// </summary>
/// <remarks>
/// Server credentials come from the <c>TRAINBOOKING_TEST_MYSQL</c> environment variable
/// (a connection string without a database, e.g. <c>Server=localhost;Port=3306;Uid=root;Pwd=...</c>).
/// When it is not set, the root password is read from <c>MYSQL_ROOT_PASSWORD</c> in the repository's
/// <c>.env</c> file and the docker-compose MySQL on localhost:3306 is used.
/// </remarks>
public sealed class TestDatabase
{
    public const string DatabaseName = "trainbooking_test";

    private const string ServerEnvironmentVariable = "TRAINBOOKING_TEST_MYSQL";

    // init.sql targets the development database by name; tests rewrite it to the test database.
    private const string DevelopmentDatabaseIdentifier = "`0test1`";

    private readonly string _serverConnectionString;
    private readonly string _initScript;

    public TestDatabase()
    {
        if (!DatabaseName.EndsWith("_test", StringComparison.Ordinal))
            throw new InvalidOperationException("The test database name must end with '_test'.");

        var repositoryRoot = FindRepositoryRoot();
        _serverConnectionString = ResolveServerConnectionString(repositoryRoot);

        var script = File.ReadAllText(Path.Combine(repositoryRoot, "init.sql"));
        if (!script.Contains(DevelopmentDatabaseIdentifier, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"init.sql no longer contains {DevelopmentDatabaseIdentifier}; update TestDatabase so tests cannot touch the development database.");
        }

        _initScript = script.Replace(DevelopmentDatabaseIdentifier, $"`{DatabaseName}`", StringComparison.Ordinal);

        ConnectionString = new MySqlConnectionStringBuilder(_serverConnectionString)
        {
            Database = DatabaseName
        }.ConnectionString;
    }

    /// <summary>
    /// Connection string for the application under test, pointing at the test database.
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// Drops every table and recreates schema and seed data from <c>init.sql</c>.
    /// </summary>
    public async Task ResetAsync()
    {
        var builder = new MySqlConnectionStringBuilder(_serverConnectionString)
        {
            AllowUserVariables = true
        };

        await using var connection = new MySqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = _initScript;
        command.CommandTimeout = 60;
        await command.ExecuteNonQueryAsync();

        // Pooled connections may still point at dropped tables' metadata; start clean.
        MySqlConnection.ClearAllPools();
    }

    /// <summary>
    /// Runs a raw SQL statement against the test database (test setup only).
    /// </summary>
    public async Task ExecuteAsync(string sql)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static string ResolveServerConnectionString(string repositoryRoot)
    {
        var fromEnvironment = Environment.GetEnvironmentVariable(ServerEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
            return WithoutDatabase(fromEnvironment);

        var rootPassword = ReadDotEnv(repositoryRoot).GetValueOrDefault("MYSQL_ROOT_PASSWORD");
        if (string.IsNullOrWhiteSpace(rootPassword))
        {
            throw new InvalidOperationException(
                $"Set {ServerEnvironmentVariable} or MYSQL_ROOT_PASSWORD in the repository's .env file to run the integration tests.");
        }

        return new MySqlConnectionStringBuilder
        {
            Server = "localhost",
            Port = 3306,
            UserID = "root",
            Password = rootPassword
        }.ConnectionString;
    }

    private static string WithoutDatabase(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString) { Database = string.Empty };
        return builder.ConnectionString;
    }

    private static Dictionary<string, string> ReadDotEnv(string repositoryRoot)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var path = Path.Combine(repositoryRoot, ".env");
        if (!File.Exists(path))
            return values;

        foreach (var line in File.ReadAllLines(path))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;

            var separator = trimmed.IndexOf('=');
            if (separator <= 0)
                continue;

            values[trimmed[..separator].Trim()] = trimmed[(separator + 1)..].Trim().Trim('"');
        }

        return values;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TrainBooking.sln")))
            directory = directory.Parent;

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not find the repository root (TrainBooking.sln).");
    }
}
