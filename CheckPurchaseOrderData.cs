using System;
using System.Data;
using System.IO;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

class Program
{
    static void Main()
    {
        // Đọc connectionString từ appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("src/GarageManagementSystem.API/appsettings.json", optional: false)
            .Build();
            
        string connectionString = configuration.GetConnectionString("DefaultConnection");
        
        Console.WriteLine($"Using connection string: {connectionString}");
        
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected to MySQL database");
                
                // Kiểm tra dữ liệu StockTransactions hiện tại
                var sql = @"
                SELECT 
                    Id,
                    TransactionNumber,
                    TransactionType,
                    TransactionDate,
                    PartId,
                    Quantity,
                    UnitCost,
                    UnitPrice,
                    TotalCost,
                    TotalAmount,
                    SupplierId,
                    ReferenceNumber,
                    HasInvoice,
                    Notes,
                    ProcessedById,
                    StockAfter,
                    QuantityBefore,
                    IsDeleted,
                    CreatedAt,
                    CreatedBy
                FROM StockTransactions 
                WHERE TransactionType = 1 
                ORDER BY CreatedAt DESC
                LIMIT 10;
                ";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        Console.WriteLine("=== DỮ LIỆU STOCK TRANSACTIONS (NhapKho) ===");
                        Console.WriteLine($"{"ID",-5} {"TransactionNumber",-15} {"ReferenceNumber",-15} {"SupplierId",-10} {"PartId",-8} {"Quantity",-8} {"TotalAmount",-12} {"CreatedAt",-20}");
                        Console.WriteLine(new string('-', 100));
                        
                        int count = 0;
                        while (reader.Read())
                        {
                            count++;
                            Console.WriteLine($"{reader["Id"],-5} {reader["TransactionNumber"],-15} {reader["ReferenceNumber"]?.ToString() ?? "NULL",-15} {reader["SupplierId"]?.ToString() ?? "NULL",-10} {reader["PartId"],-8} {reader["Quantity"],-8} {reader["TotalAmount"],-12:C0} {reader["CreatedAt"]:yyyy-MM-dd HH:mm:ss}");
                        }
                        
                        if (count == 0)
                        {
                            Console.WriteLine("❌ KHÔNG CÓ DỮ LIỆU PHIẾU NHẬP KHO!");
                        }
                        else
                        {
                            Console.WriteLine($"\n✅ Tìm thấy {count} phiếu nhập kho");
                        }
                    }
                }
                
                // Kiểm tra tổng số StockTransactions
                var countSql = "SELECT COUNT(*) as TotalCount FROM StockTransactions WHERE TransactionType = 1";
                using (var countCommand = new MySqlCommand(countSql, connection))
                {
                    var totalCount = countCommand.ExecuteScalar();
                    Console.WriteLine($"\n📊 Tổng số phiếu nhập kho: {totalCount}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
