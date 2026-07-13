using Microsoft.Data.SqlClient;

namespace SubiektGtAddressManager.Services;

public static class SqlServerBrowser
{
    private static string BuildConnectionString(string server, bool useWindowsAuthentication, string? username, string? password, string database)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            TrustServerCertificate = true,
            ConnectTimeout = 10,
        };

        if (useWindowsAuthentication)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = username ?? string.Empty;
            builder.Password = password ?? string.Empty;
        }

        return builder.ConnectionString;
    }

    public static async Task<List<string>> ListDatabasesAsync(
        string server, bool useWindowsAuthentication, string? username, string? password, CancellationToken ct = default)
    {
        const string sql = "SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name";

        var connectionString = BuildConnectionString(server, useWindowsAuthentication, username, password, "master");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var result = new List<string>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }

    public static async Task<List<OperatorInfo>> ListOperatorsAsync(
        string server, bool useWindowsAuthentication, string? username, string? password, string database, CancellationToken ct = default)
    {
        const string sql = """
            SELECT uz_Identyfikator, uz_Imie, uz_Nazwisko
            FROM pd_Uzytkownik
            WHERE uz_Status = 1
            ORDER BY uz_Identyfikator
            """;

        var connectionString = BuildConnectionString(server, useWindowsAuthentication, username, password, database);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var result = new List<OperatorInfo>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(new OperatorInfo(
                (reader["uz_Identyfikator"] as string ?? string.Empty).Trim(),
                reader["uz_Imie"] as string ?? string.Empty,
                reader["uz_Nazwisko"] as string ?? string.Empty));
        }

        return result;
    }
}
