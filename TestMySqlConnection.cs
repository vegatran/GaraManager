using System;
using MySql.Data.MySqlClient;

class TestMySqlConnection
{
    static void Main()
    {
        string connectionString = "Server=45.251.114.134;Port=3306;Database=GaraManagement;User=usergara;Password=VanD@t123#@!;AllowPublicKeyRetrieval=true;SslMode=none;";
        
        Console.WriteLine("Testing MySQL Connection...");
        Console.WriteLine("Server: 45.251.114.134");
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
                
                // Show databases
                Console.WriteLine(new string('-', 50));
                Console.WriteLine("Available Databases:");
                using (MySqlCommand cmd = new MySqlCommand("SHOW DATABASES", connection))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"  - {reader.GetString(0)}");
                        }
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
                    Console.WriteLine($"  → See: https://dev.mysql.com/doc/mysql-errors/8.0/en/server-error-reference.html#error_er_{ex.Number}");
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

