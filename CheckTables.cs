using Microsoft.EntityFrameworkCore;
using GarageManagementSystem.Infrastructure.Data;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        var connectionString = "Server=45.251.114.134;Port=3307;Database=GaraManagement;User=usergara;Password=VanD@t123#@!;AllowPublicKeyRetrieval=true;SslMode=none;";
        
        Console.WriteLine("🔍 KIỂM TRA DATABASE TABLES");
        Console.WriteLine("==========================");
        
        try
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            Console.WriteLine("✅ Kết nối MySQL thành công!");
            
            // Lấy tất cả tables
            var tables = new List<string>();
            using var command = new MySqlCommand("SHOW TABLES", connection);
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }
            reader.Close();
            
            Console.WriteLine($"\n📊 Tổng số tables: {tables.Count}");
            Console.WriteLine("\n📋 DANH SÁCH TẤT CẢ TABLES:");
            Console.WriteLine("=============================");
            
            // Nhóm tables theo context
            var identityTables = tables.Where(t => t.StartsWith("AspNet")).OrderBy(t => t).ToList();
            var garaTables = tables.Where(t => t.StartsWith("Claims") || t.StartsWith("SoftDelete")).OrderBy(t => t).ToList();
            var configTables = tables.Where(t => t.StartsWith("Clients") || t.StartsWith("Api") || t.StartsWith("IdentityResources")).OrderBy(t => t).ToList();
            var persistedTables = tables.Where(t => t.StartsWith("PersistedGrants") || t.StartsWith("DeviceCodes") || t.StartsWith("Keys") || t.StartsWith("ServerSideSessions")).OrderBy(t => t).ToList();
            var garageTables = tables.Where(t => !identityTables.Contains(t) && !garaTables.Contains(t) && !configTables.Contains(t) && !persistedTables.Contains(t) && !t.StartsWith("__")).OrderBy(t => t).ToList();
            var migrationTables = tables.Where(t => t.StartsWith("__")).OrderBy(t => t).ToList();
            
            Console.WriteLine("\n🏢 IDENTITY TABLES (IdentityDbContext):");
            foreach (var table in identityTables)
            {
                Console.WriteLine($"   ✓ {table}");
            }
            
            Console.WriteLine("\n⚙️ GARA MANAGEMENT TABLES (GaraManagementContext):");
            foreach (var table in garaTables)
            {
                Console.WriteLine($"   ✓ {table}");
            }
            
            Console.WriteLine("\n🔧 CONFIGURATION TABLES (ConfigurationDbContext):");
            foreach (var table in configTables)
            {
                Console.WriteLine($"   ✓ {table}");
            }
            
            Console.WriteLine("\n🔑 PERSISTED GRANT TABLES (PersistedGrantDbContext):");
            foreach (var table in persistedTables)
            {
                Console.WriteLine($"   ✓ {table}");
            }
            
            Console.WriteLine("\n🏭 GARAGE BUSINESS TABLES (GarageDbContext):");
            foreach (var table in garageTables)
            {
                Console.WriteLine($"   ✓ {table}");
            }
            
            Console.WriteLine("\n📦 MIGRATION TABLES:");
            foreach (var table in migrationTables)
            {
                Console.WriteLine($"   ✓ {table}");
            }
            
            // Kiểm tra tables quan trọng
            Console.WriteLine("\n🔍 KIỂM TRA TABLES QUAN TRỌNG:");
            Console.WriteLine("===============================");
            
            var importantTables = new[]
            {
                // IdentityServer
                "AspNetUsers", "AspNetRoles", "AspNetUserRoles", "AspNetUserClaims",
                "Claims", "SoftDeleteRecords",
                "Clients", "ApiResources", "ApiScopes", "IdentityResources",
                "ClientScopes", "ClientSecrets", "PersistedGrants",
                
                // Garage Business
                "Customers", "Vehicles", "Services", "ServiceOrders", "Employees",
                "Departments", "Positions", "VehicleInspections", "Parts", "Suppliers"
            };
            
            var missingTables = new List<string>();
            foreach (var table in importantTables)
            {
                if (tables.Contains(table))
                {
                    Console.WriteLine($"   ✅ {table}");
                }
                else
                {
                    Console.WriteLine($"   ❌ {table} - THIẾU!");
                    missingTables.Add(table);
                }
            }
            
            if (missingTables.Any())
            {
                Console.WriteLine($"\n⚠️  CÓ {missingTables.Count} TABLES BỊ THIẾU:");
                foreach (var table in missingTables)
                {
                    Console.WriteLine($"   - {table}");
                }
            }
            else
            {
                Console.WriteLine("\n🎉 TẤT CẢ TABLES QUAN TRỌNG ĐÃ CÓ ĐẦY ĐỦ!");
            }
            
            // Thống kê
            Console.WriteLine("\n📈 THỐNG KÊ:");
            Console.WriteLine($"   - Identity Tables: {identityTables.Count}");
            Console.WriteLine($"   - Gara Management: {garaTables.Count}");
            Console.WriteLine($"   - Configuration: {configTables.Count}");
            Console.WriteLine($"   - Persisted Grant: {persistedTables.Count}");
            Console.WriteLine($"   - Garage Business: {garageTables.Count}");
            Console.WriteLine($"   - Migration Tables: {migrationTables.Count}");
            Console.WriteLine($"   - TỔNG CỘNG: {tables.Count} tables");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Lỗi: {ex.Message}");
        }
        
        Console.WriteLine("\nNhấn Enter để thoát...");
        Console.ReadLine();
    }
}
