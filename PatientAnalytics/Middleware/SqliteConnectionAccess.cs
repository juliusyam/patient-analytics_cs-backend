using Microsoft.Data.Sqlite;
using PatientAnalytics.Services;

namespace PatientAnalytics.Middleware;

public static class SqliteConnectionAccess
{
    public static void EstablishConnection(string? connectionString)
    {
        var sqliteConnection = new SqliteConnection(connectionString);
        
        sqliteConnection.Open();

        var sqliteCommand = sqliteConnection.CreateCommand();
        
        CreateUsersTable(sqliteCommand);
        CreateUserRefreshesTable(sqliteCommand);
        CreatePatientsTable(sqliteCommand);
        CreatePatientTemperaturesTable(sqliteCommand);
        CreatePatientBloodPressuresTable(sqliteCommand);
        CreatePatientHeightsTable(sqliteCommand);
        CreatePatientWeightsTable(sqliteCommand);
    }

    public static async Task ResetDatabase(UserService userService, string? connectionString)
    {
        var sqliteConnection = new SqliteConnection(connectionString);
        
        sqliteConnection.Open();

        var sqliteCommand = sqliteConnection.CreateCommand();
        
        DropUsersTable(sqliteCommand);
        CreateUsersTable(sqliteCommand);

        var (user, password) = await userService.CreateInitialSuperAdmin();

        DropPatientWeightsTable(sqliteCommand);
        CreateUserRefreshesTable(sqliteCommand);
        
        DropPatientsTable(sqliteCommand);
        CreatePatientsTable(sqliteCommand);
        
        DropPatientTemperaturesTable(sqliteCommand);
        CreatePatientTemperaturesTable(sqliteCommand);
        
        DropPatientBloodPressuresTable(sqliteCommand);
        CreatePatientBloodPressuresTable(sqliteCommand);
        
        DropPatientHeightsTable(sqliteCommand);
        CreatePatientHeightsTable(sqliteCommand);
        
        DropPatientWeightsTable(sqliteCommand);
        CreatePatientWeightsTable(sqliteCommand);
        
        Console.WriteLine($"Database Reset Successfully. New initial SuperAdmin account is - Username: { user.Username }, Password: { password }");
    }
    
    private static void CreateUsersTable(SqliteCommand command)
    {
        command.CommandText =
            "CREATE TABLE IF NOT EXISTS Users(" +
                "Id INTEGER NOT NULL UNIQUE, " +
                "DateOfBirth DATETIME NOT NULL, " +
                "Gender TEXT NOT NULL, " +
                "Email TEXT NOT NULL UNIQUE, " +
                "Username TEXT NOT NULL UNIQUE, " +
                "FirstName TEXT, " +
                "LastName TEXT, " +
                "Address INTEGER, " +
                "PasswordHash TEXT NOT NULL, "+
                "DateCreated DATETIME NOT NULL, " +
                "DateEdited DATETIME, " +
                "Role TEXT NOT NULL, " +
                "PRIMARY KEY(Id AUTOINCREMENT)" +
            ");";

        command.ExecuteNonQuery();
    }

    private static void DropUsersTable(SqliteCommand command)
    {
        command.CommandText = "DROP TABLE IF EXISTS Users";
        command.ExecuteNonQuery();
    }
    
    private static void CreateUserRefreshesTable(SqliteCommand command)
    {
        command.CommandText =
            "CREATE TABLE IF NOT EXISTS UserRefreshes(" +
            "Id INTEGER NOT NULL UNIQUE, " +
            "UserId INTEGER NOT NULL, " +
            "RefreshTokenHash TEXT NOT NULL, " +
            "RefreshTokenExpiry DATETIME NOT NULL, " +
            "DateCreated DATETIME NOT NULL, " +
            "PRIMARY KEY(Id AUTOINCREMENT)" +
            "FOREIGN KEY(UserId) REFERENCES Users(Id)" +
            ");";

        command.ExecuteNonQuery();
    }

    private static void DropUserRefreshesTable(SqliteCommand command)
    {
        command.CommandText = "DROP TABLE IF EXISTS UserRefreshes";
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

    private static void CreatePatientTemperaturesTable(SqliteCommand command)
    {
        command.CommandText = 
            "CREATE TABLE IF NOT EXISTS PatientTemperatures(" +
            "Id INTEGER NOT NULL UNIQUE, " +
            "PatientId INTEGER NOT NULL, " +
            "DoctorId INTEGER NOT NULL, " +
            "DateCreated DATETIME NOT NULL, " +
            "TemperatureCelsius DOUBLE NOT NULL, " +
            "PRIMARY KEY(Id AUTOINCREMENT)" +
            "FOREIGN KEY(DoctorId) REFERENCES Users(Id)" +
            "FOREIGN KEY(PatientId) REFERENCES Patients(Id)" +
            ");";
        
        command.ExecuteNonQuery();
    }
    
    private static void DropPatientTemperaturesTable(SqliteCommand command)
    {
        command.CommandText = "DROP TABLE IF EXISTS PatientTemperatures";
        command.ExecuteNonQuery();
    }
    
    private static void CreatePatientBloodPressuresTable(SqliteCommand command)
    {
        command.CommandText = 
            "CREATE TABLE IF NOT EXISTS PatientBloodPressures(" +
            "Id INTEGER NOT NULL UNIQUE, " +
            "PatientId INTEGER NOT NULL, " +
            "DoctorId INTEGER NOT NULL, " +
            "DateCreated DATETIME NOT NULL, " +
            "BloodPressureSystolic DOUBLE NOT NULL, " +
            "BloodPressureDiastolic DOUBLE NOT NULL, " +
            "PRIMARY KEY(Id AUTOINCREMENT)" +
            "FOREIGN KEY(DoctorId) REFERENCES Users(Id)" +
            "FOREIGN KEY(PatientId) REFERENCES Patients(Id)" +
            ");";
        
        command.ExecuteNonQuery();
    }
    
    private static void DropPatientBloodPressuresTable(SqliteCommand command)
    {
        command.CommandText = "DROP TABLE IF EXISTS PatientBloodPressures";
        command.ExecuteNonQuery();
    }
    
    private static void CreatePatientHeightsTable(SqliteCommand command)
    {
        command.CommandText = 
            "CREATE TABLE IF NOT EXISTS PatientHeights(" +
            "Id INTEGER NOT NULL UNIQUE, " +
            "PatientId INTEGER NOT NULL, " +
            "DoctorId INTEGER NOT NULL, " +
            "DateCreated DATETIME NOT NULL, " +
            "HeightCm DOUBLE NOT NULL, " +
            "PRIMARY KEY(Id AUTOINCREMENT)" +
            "FOREIGN KEY(DoctorId) REFERENCES Users(Id)" +
            "FOREIGN KEY(PatientId) REFERENCES Patients(Id)" +
            ");";
        
        command.ExecuteNonQuery();
    }
    
    private static void DropPatientHeightsTable(SqliteCommand command)
    {
        command.CommandText = "DROP TABLE IF EXISTS PatientHeights";
        command.ExecuteNonQuery();
    }
    
    private static void CreatePatientWeightsTable(SqliteCommand command)
    {
        command.CommandText = 
            "CREATE TABLE IF NOT EXISTS PatientWeights(" +
            "Id INTEGER NOT NULL UNIQUE, " +
            "PatientId INTEGER NOT NULL, " +
            "DoctorId INTEGER NOT NULL, " +
            "DateCreated DATETIME NOT NULL, " +
            "WeightKg DOUBLE NOT NULL, " +
            "PRIMARY KEY(Id AUTOINCREMENT)" +
            "FOREIGN KEY(DoctorId) REFERENCES Users(Id)" +
            "FOREIGN KEY(PatientId) REFERENCES Patients(Id)" +
            ");";
        
        command.ExecuteNonQuery();
    }
    
    private static void DropPatientWeightsTable(SqliteCommand command)
    {
        command.CommandText = "DROP TABLE IF EXISTS PatientWeights";
        command.ExecuteNonQuery();
    }
}