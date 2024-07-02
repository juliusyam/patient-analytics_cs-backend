using Microsoft.Data.Sqlite;
using PatientAnalytics.Services;

namespace PatientAnalytics.Middleware;

public static class SqliteConnectionAccess
{
    public static void EstablishConnection(string? connectionString)
    {
        var sqliteConnection = new SqliteConnection(connectionString);
        
        sqliteConnection.Open();
    }
}