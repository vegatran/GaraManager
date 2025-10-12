using System;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

string connectionString = "Server=45.251.114.134;Port=3307;Database=GaraManagement;User=usergara;Password=VanD@t123#@!;AllowPublicKeyRetrieval=true;SslMode=none;AllowUserVariables=True;";
string sqlFilePath = @"D:\Source\GaraManager\docs\DEMO_DATA_COMPLETE.sql";

Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine("RUNNING DEMO DATA SCRIPT WITH ERROR DETECTION");
Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine($"File: {sqlFilePath}");
Console.WriteLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine();

try
{
    string sqlContent = File.ReadAllText(sqlFilePath);
    
    // Remove comments and split into statements
    sqlContent = Regex.Replace(sqlContent, @"--.*$", "", RegexOptions.Multiline);
    sqlContent = Regex.Replace(sqlContent, @"/\*.*?\*/", "", RegexOptions.Singleline);
    
    var statements = sqlContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
    
    int totalStatements = 0;
    int successCount = 0;
    int errorCount = 0;
    string? firstError = null;
    int firstErrorLine = 0;
    
    using (var connection = new MySqlConnection(connectionString))
    {
        connection.Open();
        Console.WriteLine("✓ Connected to database successfully");
        Console.WriteLine();
        
        int lineNumber = 0;
        foreach (var statement in statements)
        {
            lineNumber++;
            var trimmedStatement = statement.Trim();
            
            if (string.IsNullOrWhiteSpace(trimmedStatement))
                continue;
            
            if (trimmedStatement.StartsWith("USE ", StringComparison.OrdinalIgnoreCase) ||
                trimmedStatement.StartsWith("SET ", StringComparison.OrdinalIgnoreCase) ||
                trimmedStatement.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (var cmd = new MySqlCommand(trimmedStatement, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch { }
                continue;
            }
            
            if (trimmedStatement.StartsWith("INSERT ", StringComparison.OrdinalIgnoreCase) ||
                trimmedStatement.StartsWith("UPDATE ", StringComparison.OrdinalIgnoreCase) ||
                trimmedStatement.StartsWith("DELETE ", StringComparison.OrdinalIgnoreCase))
            {
                totalStatements++;
                
                try
                {
                    using (var cmd = new MySqlCommand(trimmedStatement, connection))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }
                    successCount++;
                    
                    if (totalStatements % 10 == 0)
                    {
                        Console.Write($"\rProcessed: {totalStatements} statements ({successCount} success, {errorCount} errors)");
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    
                    if (firstError == null)
                    {
                        firstError = ex.Message;
                        firstErrorLine = lineNumber;
                        
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("❌ FIRST ERROR DETECTED:");
                        Console.WriteLine("=".PadRight(80, '='));
                        Console.ResetColor();
                        Console.WriteLine($"Statement #{lineNumber}");
                        Console.WriteLine($"Error: {ex.Message}");
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Statement:");
                        Console.WriteLine(trimmedStatement.Length > 500 
                            ? trimmedStatement.Substring(0, 500) + "..." 
                            : trimmedStatement);
                        Console.ResetColor();
                        Console.WriteLine();
                        Console.WriteLine("=".PadRight(80, '='));
                        Console.WriteLine("Script execution stopped at first error.");
                        Console.WriteLine();
                        
                        // Extract the missing field name if possible
                        var fieldMatch = Regex.Match(ex.Message, @"Field '(\w+)' doesn't have a default value");
                        if (fieldMatch.Success)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"💡 SOLUTION: Add column '{fieldMatch.Groups[1].Value}' to the INSERT statement");
                            Console.ResetColor();
                        }
                        
                        var columnMatch = Regex.Match(ex.Message, @"Unknown column '(\w+)' in 'field list'");
                        if (columnMatch.Success)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"💡 SOLUTION: Remove or fix column '{columnMatch.Groups[1].Value}' in the INSERT statement");
                            Console.ResetColor();
                        }
                        
                        var countMatch = Regex.Match(ex.Message, @"Column count doesn't match value count");
                        if (countMatch.Success)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"💡 SOLUTION: Number of columns doesn't match number of values");
                            Console.ResetColor();
                        }
                        
                        break;
                    }
                }
            }
        }
        
        Console.WriteLine();
        Console.WriteLine();
        
        if (errorCount == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ ALL STATEMENTS EXECUTED SUCCESSFULLY!");
            Console.ResetColor();
        }
        
        Console.WriteLine();
        Console.WriteLine("SUMMARY:");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine($"Total Statements: {totalStatements}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Success: {successCount}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Errors: {errorCount}");
        Console.ResetColor();
        Console.WriteLine("=".PadRight(80, '='));
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"FATAL ERROR: {ex.Message}");
    Console.ResetColor();
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
