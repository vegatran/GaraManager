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
                
                // Thêm dữ liệu demo cho phiếu nhập hàng
                var sql = @"
                INSERT INTO StockTransactions (
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
                ) VALUES 
                ('STK-2025-001', 1, '2025-01-20 10:00:00', 1, 50, 350000, 450000, 17500000, 22500000, 1, 'PN-2025-001', true, 'Nhập dầu nhớt từ nhà cung cấp', 1, 100, 50, false, NOW(), 'DemoData'),
                ('STK-2025-002', 1, '2025-01-20 10:00:00', 2, 30, 100000, 120000, 3000000, 3600000, 1, 'PN-2025-001', true, 'Nhập lọc dầu từ nhà cung cấp', 1, 80, 50, false, NOW(), 'DemoData'),
                ('STK-2025-003', 1, '2025-01-19 14:30:00', 3, 20, 600000, 800000, 12000000, 16000000, 2, 'PN-2025-002', true, 'Nhập má phanh từ nhà cung cấp', 2, 70, 50, false, NOW(), 'DemoData'),
                ('STK-2025-004', 1, '2025-01-19 14:30:00', 4, 15, 800000, 1000000, 12000000, 15000000, 2, 'PN-2025-002', true, 'Nhập bugi từ nhà cung cấp', 2, 65, 50, false, NOW(), 'DemoData');
                
                UPDATE Parts SET QuantityInStock = 100 WHERE Id = 1;
                UPDATE Parts SET QuantityInStock = 80 WHERE Id = 2; 
                UPDATE Parts SET QuantityInStock = 70 WHERE Id = 3;
                UPDATE Parts SET QuantityInStock = 65 WHERE Id = 4;
                ";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    int rowsAffected = command.ExecuteNonQuery();
                    Console.WriteLine($"Successfully inserted {rowsAffected} rows");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
