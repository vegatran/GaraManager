#!/usr/bin/env python3
"""
Generate INSERT statements for demo data based on CREATE_ALL_TABLES_FRESH.sql schema
"""

# Services table columns from schema
services_insert = """INSERT INTO `services` (
    Name, Description, Price, Duration, Category, 
    ServiceTypeId, LaborType, SkillLevel, LaborHours, LaborRate, TotalLaborCost,
    RequiredTools, RequiredSkills, WorkInstructions, IsActive, IsDeleted, CreatedAt, CreatedBy
) VALUES 
('[DEMO] Thay dầu động cơ', 'Demo service', 500000, 60, 'Bảo dưỡng', 
 NULL, 'Maintenance', 'Basic', 1, 100000, 100000,
 NULL, NULL, NULL, 1, 0, NOW(), 'DemoData'),
 
('[DEMO] Kiểm tra tổng quát', 'Demo inspection', 300000, 90, 'Kiểm định', 
 NULL, 'Inspection', 'Basic', 2, 100000, 200000,
 NULL, NULL, NULL, 1, 0, NOW(), 'DemoData'),
 
('[DEMO] Sửa phanh', 'Demo brake repair', 1500000, 180, 'Sửa chữa', 
 NULL, 'Repair', 'Intermediate', 3, 150000, 450000,
 NULL, NULL, NULL, 1, 0, NOW(), 'DemoData');"""

# Parts table columns from schema  
parts_insert = """INSERT INTO `parts` (
    PartNumber, PartName, Description, Category, Brand,
    CostPrice, AverageCostPrice, SellPrice, QuantityInStock, MinimumStock, ReorderLevel,
    Unit, CompatibleVehicles, Location, SourceType, InvoiceType, HasInvoice,
    CanUseForCompany, CanUseForInsurance, CanUseForIndividual, `Condition`,
    SourceReference, PartGroupId, OEMNumber, AftermarketNumber, Manufacturer,
    Dimensions, Weight, Material, Color, WarrantyMonths, WarrantyConditions, IsOEM,
    IsActive, IsDeleted, CreatedAt, CreatedBy
) VALUES 
('DEMO001', '[DEMO] Dầu nhớt Demo', 'Demo oil', 'Dầu nhớt', 'Demo Brand',
 350000, 350000, 450000, 100, 10, 15,
 'Lít', NULL, 'K1-DEMO', 'Purchased', 'WithInvoice', 1,
 1, 1, 1, 'New',
 NULL, NULL, NULL, NULL, NULL,
 NULL, NULL, NULL, NULL, 12, NULL, 0,
 1, 0, NOW(), 'DemoData'),
 
('DEMO002', '[DEMO] Lọc dầu Demo', 'Demo filter', 'Lọc', 'Demo Brand',
 100000, 100000, 150000, 100, 20, 30,
 'Cái', NULL, 'K1-DEMO', 'Purchased', 'WithInvoice', 1,
 1, 1, 1, 'New',
 NULL, NULL, NULL, NULL, NULL,
 NULL, NULL, NULL, NULL, 6, NULL, 0,
 1, 0, NOW(), 'DemoData'),
 
('DEMO003', '[DEMO] Má phanh Demo', 'Demo brake pad', 'Phanh', 'Demo Brand',
 600000, 600000, 800000, 50, 5, 10,
 'Bộ', NULL, 'K1-DEMO', 'Purchased', 'WithInvoice', 1,
 1, 1, 1, 'New',
 NULL, NULL, NULL, NULL, NULL,
 NULL, NULL, NULL, NULL, 6, NULL, 0,
 1, 0, NOW(), 'DemoData');"""

# Suppliers table columns from schema
suppliers_insert = """INSERT INTO `suppliers` (
    SupplierName, SupplierCode, ContactPerson, Phone, ContactPhone, Email, Address,
    City, Country, Website, TaxCode, BankAccount, BankName, PaymentTerms, DeliveryTerms,
    Notes, IsOEMSupplier, IsActive, LastOrderDate, TotalOrderValue, Rating,
    IsDeleted, CreatedAt, CreatedBy
) VALUES 
('[DEMO] Phụ tùng Demo', 'DEMO001', 'Demo Contact', '0287777777', '0901111111', 'demo@parts.com', '111 Demo Plaza',
 NULL, NULL, NULL, '1234567890', '1234567890', 'Demo Bank', NULL, NULL,
 'Demo supplier', 0, 1, NULL, NULL, 4.5,
 0, NOW(), 'DemoData');"""

print("-- Services")
print(services_insert)
print("\n-- Parts")  
print(parts_insert)
print("\n-- Suppliers")
print(suppliers_insert)

