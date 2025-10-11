using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

class TestMySqlConnection
{
    static void Main()
    {
        string connectionString = "Server=45.251.114.134;Port=3307;Database=GaraManagement;User=usergara;Password=VanD@t123#@!;AllowPublicKeyRetrieval=true;SslMode=none;";
        
        Console.WriteLine("Testing MySQL Connection...");
        Console.WriteLine("Server: 45.251.114.134");
        Console.WriteLine("Port: 3307");
        Console.WriteLine("User: usergara");
        Console.WriteLine("Database: GaraManagement");
        Console.WriteLine(new string('-', 50));
        
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                Console.WriteLine("Opening connection...");
                connection.Open();
                
                Console.WriteLine("✓ Connection successful!");
                Console.WriteLine($"✓ Server Version: {connection.ServerVersion}");
                Console.WriteLine($"✓ Database: {connection.Database}");
                Console.WriteLine($"✓ Connection State: {connection.State}");
                
                // Test a simple query
                using (MySqlCommand cmd = new MySqlCommand("SELECT DATABASE() as CurrentDB, USER() as CurrentUser, NOW() as CurrentTime", connection))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine($"✓ Current Database: {reader["CurrentDB"]}");
                            Console.WriteLine($"✓ Current User: {reader["CurrentUser"]}");
                            Console.WriteLine($"✓ Server Time: {reader["CurrentTime"]}");
                        }
                    }
                }
                
                // Check all tables
                Console.WriteLine(new string('-', 50));
                Console.WriteLine("Checking all tables in database...");
                
                using (MySqlCommand cmd = new MySqlCommand("SHOW TABLES", connection))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        var tables = new List<string>();
                        while (reader.Read())
                        {
                            tables.Add(reader.GetString(0));
                        }
                        reader.Close();
                        
                        Console.WriteLine($"✓ Found {tables.Count} tables:");
                        
                        // Group by context
                        var identityTables = tables.Where(t => t.StartsWith("AspNet")).OrderBy(t => t).ToList();
                        var garaTables = tables.Where(t => t.StartsWith("Claims") || t.StartsWith("SoftDelete")).OrderBy(t => t).ToList();
                        var configTables = tables.Where(t => t.StartsWith("Clients") || t.StartsWith("Api") || t.StartsWith("IdentityResources")).OrderBy(t => t).ToList();
                        var persistedTables = tables.Where(t => t.StartsWith("PersistedGrants") || t.StartsWith("DeviceCodes") || t.StartsWith("Keys") || t.StartsWith("ServerSideSessions")).OrderBy(t => t).ToList();
                        var garageTables = tables.Where(t => !identityTables.Contains(t) && !garaTables.Contains(t) && !configTables.Contains(t) && !persistedTables.Contains(t) && !t.StartsWith("__")).OrderBy(t => t).ToList();
                        var migrationTables = tables.Where(t => t.StartsWith("__")).OrderBy(t => t).ToList();
                        
                        Console.WriteLine("\n🏢 IDENTITY TABLES:");
                        foreach (var table in identityTables) Console.WriteLine($"   ✓ {table}");
                        
                        Console.WriteLine("\n⚙️ GARA MANAGEMENT TABLES:");
                        foreach (var table in garaTables) Console.WriteLine($"   ✓ {table}");
                        
                        Console.WriteLine("\n🔧 CONFIGURATION TABLES:");
                        foreach (var table in configTables) Console.WriteLine($"   ✓ {table}");
                        
                        Console.WriteLine("\n🔑 PERSISTED GRANT TABLES:");
                        foreach (var table in persistedTables) Console.WriteLine($"   ✓ {table}");
                        
                        Console.WriteLine("\n🏭 GARAGE BUSINESS TABLES:");
                        foreach (var table in garageTables) Console.WriteLine($"   ✓ {table}");
                        
                        Console.WriteLine("\n📦 MIGRATION TABLES:");
                        foreach (var table in migrationTables) Console.WriteLine($"   ✓ {table}");
                        
                        Console.WriteLine($"\n📊 SUMMARY:");
                        Console.WriteLine($"   - Identity: {identityTables.Count}");
                        Console.WriteLine($"   - Gara Management: {garaTables.Count}");
                        Console.WriteLine($"   - Configuration: {configTables.Count}");
                        Console.WriteLine($"   - Persisted Grant: {persistedTables.Count}");
                        Console.WriteLine($"   - Garage Business: {garageTables.Count}");
                        Console.WriteLine($"   - Migration: {migrationTables.Count}");
                        Console.WriteLine($"   - TOTAL: {tables.Count} tables");
                    }
                }
                
                connection.Close();
                Console.WriteLine(new string('-', 50));
                Console.WriteLine("✓ Connection test completed successfully!");
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine("✗ MySQL Error occurred:");
            Console.WriteLine($"  Error Code: {ex.Number}");
            Console.WriteLine($"  Message: {ex.Message}");
            
            switch (ex.Number)
            {
                case 1045:
                    Console.WriteLine("  → Access denied. Check username and password.");
                    break;
                case 2003:
                    Console.WriteLine("  → Can't connect to MySQL server. Check if server is running and accessible.");
                    break;
                case 1049:
                    Console.WriteLine("  → Unknown database. The database doesn't exist.");
                    break;
                default:
                    Console.WriteLine($"  → MySQL Error Reference: Error {ex.Number}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            Console.WriteLine($"  Type: {ex.GetType().Name}");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
