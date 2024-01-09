using Microsoft.Data.Sqlite;

namespace PracticeApplication.Middleware;

public class SqliteConnectionAccess
{
    public SqliteConnectionAccess()
    {
        var sqliteConnection = new SqliteConnection("Data Source=PracticeApplication.db");
        
        sqliteConnection.Open();

        var sqliteCommand = sqliteConnection.CreateCommand();

        CreateUsersTable(sqliteCommand);
        CreatePatientsTable(sqliteCommand);
    }

    private static void CreateUsersTable(SqliteCommand command)
    {
        command.CommandText =
            "CREATE TABLE IF NOT EXISTS Users(Id INTEGER NOT NULL UNIQUE, DateOfBirth TEXT NOT NULL, Gender TEXT NOT NULL, Email TEXT NOT NULL UNIQUE, Address INTEGER, DateCreated TEXT NOT NULL, DateEdited TEXT, PRIMARY KEY(Id AUTOINCREMENT));";

        command.ExecuteNonQuery();
    }

    private static void CreatePatientsTable(SqliteCommand command)
    {
        command.CommandText = 
            "CREATE TABLE IF NOT EXISTS Patients(Id INTEGER NOT NULL UNIQUE, DoctorId INTEGER NOT NULL, DateOfBirth TEXT NOT NULL, Gender TEXT NOT NULL, Email TEXT NOT NULL UNIQUE, Address INTEGER, DateCreated TEXT NOT NULL, DateEdited TEXT, PRIMARY KEY(Id AUTOINCREMENT));";
        
        command.ExecuteNonQuery();
    }
}