using Microsoft.Data.Sqlite;

namespace PracticeApplication.Middleware;

public static class SqliteConnectionAccess
{
    public static void EstablishConnection(string? connectionString)
    {
        var sqliteConnection = new SqliteConnection(connectionString);
        
        sqliteConnection.Open();

        var sqliteCommand = sqliteConnection.CreateCommand();

        CreateUsersTable(sqliteCommand);
        CreatePatientsTable(sqliteCommand);
    }

    public static void ResetDatabase(string? connectionString)
    {
        var sqliteConnection = new SqliteConnection(connectionString);
        
        sqliteConnection.Open();

        var sqliteCommand = sqliteConnection.CreateCommand();
        
        DropUsersTable(sqliteCommand);
        CreateUsersTable(sqliteCommand);
        
        DropPatientsTable(sqliteCommand);
        CreatePatientsTable(sqliteCommand);
    }
    
    private static void CreateUsersTable(SqliteCommand command)
    {
        command.CommandText =
            "CREATE TABLE IF NOT EXISTS Users(" +
                "Id INTEGER NOT NULL UNIQUE, " +
                "DateOfBirth TEXT NOT NULL, " +
                "Gender TEXT NOT NULL, " +
                "Email TEXT NOT NULL UNIQUE, " +
                "Username TEXT NOT NULL UNIQUE, " +
                "FirstName TEXT, " +
                "LastName TEXT, " +
                "Address INTEGER, " +
                "PasswordHash TEXT NOT NULL, "+
                "DateCreated TEXT NOT NULL, " +
                "DateEdited TEXT, " +
                "PRIMARY KEY(Id AUTOINCREMENT)" +
            ");";

        command.ExecuteNonQuery();
    }

    private static void DropUsersTable(SqliteCommand command)
    {
        command.CommandText = "DROP TABLE IF EXISTS Users";
        command.ExecuteNonQuery();
    }

    private static void CreatePatientsTable(SqliteCommand command)
    {
        command.CommandText = 
            "CREATE TABLE IF NOT EXISTS Patients(" +
                "Id INTEGER NOT NULL UNIQUE, " +
                "DoctorId INTEGER NOT NULL, " +
                "DateOfBirth TEXT NOT NULL, " +
                "Gender TEXT NOT NULL, " +
                "Email TEXT NOT NULL UNIQUE, " +
                "FirstName TEXT, " +
                "LastName TEXT, " +
                "Address INTEGER, " +
                "DateCreated TEXT NOT NULL, " +
                "DateEdited TEXT, " +
                "PRIMARY KEY(Id AUTOINCREMENT)" +
            ");";
        
        command.ExecuteNonQuery();
    }
    
    private static void DropPatientsTable(SqliteCommand command)
    {
        command.CommandText = "DROP TABLE IF EXISTS Patients";
        command.ExecuteNonQuery();
    }
}