-- =====================================================
-- COMPLETE DEMO DATA - ALL TABLES
-- Based on CREATE_DATABASE_FROM_DBCONTEXT.sql schema
-- Garage Management System - Phase 1 & 2
-- Date: 12/10/2025
-- =====================================================
-- PREREQUISITES:
-- 1. Database must exist: GaraManagement
-- 2. All tables must be created (run CREATE_DATABASE_FROM_DBCONTEXT.sql first)
-- =====================================================

USE GaraManagement;
SET FOREIGN_KEY_CHECKS = 0;
SET SQL_SAFE_UPDATES = 0;

-- =====================================================
-- CLEANUP EXISTING DEMO DATA
-- =====================================================

DELETE FROM InvoiceItems WHERE CreatedBy = 'DemoData';
DELETE FROM Payments WHERE CreatedBy = 'DemoData';
DELETE FROM Invoices WHERE CreatedBy = 'DemoData';
DELETE FROM InsuranceClaimDocuments WHERE CreatedBy = 'DemoData';
DELETE FROM InsuranceClaims WHERE CreatedBy = 'DemoData';
DELETE FROM PartBatchUsages WHERE CreatedBy = 'DemoData';
DELETE FROM ServiceOrderParts WHERE CreatedBy = 'DemoData';
DELETE FROM ServiceOrderLabors WHERE CreatedBy = 'DemoData';
DELETE FROM ServiceOrderItems WHERE CreatedBy = 'DemoData';
DELETE FROM StockTransactions WHERE CreatedBy = 'DemoData';
DELETE FROM InsuranceInvoiceItems WHERE CreatedBy = 'DemoData';
DELETE FROM InsuranceInvoices WHERE CreatedBy = 'DemoData';
DELETE FROM PaymentTransactions WHERE CreatedBy = 'DemoData';
DELETE FROM PartInventoryBatches WHERE CreatedBy = 'DemoData';
DELETE FROM Appointments WHERE CreatedBy = 'DemoData';
DELETE FROM ServiceOrders WHERE CreatedBy = 'DemoData';
DELETE FROM QuotationItems WHERE CreatedBy = 'DemoData';
DELETE FROM ServiceQuotations WHERE CreatedBy = 'DemoData';
DELETE FROM InspectionPhotos WHERE CreatedBy = 'DemoData';
DELETE FROM InspectionIssues WHERE CreatedBy = 'DemoData';
DELETE FROM VehicleInspections WHERE CreatedBy = 'DemoData';
DELETE FROM PurchaseOrderItems WHERE CreatedBy = 'DemoData';
DELETE FROM PurchaseOrders WHERE CreatedBy = 'DemoData';
DELETE FROM FinancialTransactionAttachments WHERE CreatedBy = 'DemoData';
DELETE FROM FinancialTransactions WHERE CreatedBy = 'DemoData';
DELETE FROM VehicleInsurances WHERE CreatedBy = 'DemoData';
DELETE FROM VehicleModels WHERE CreatedBy = 'DemoData';
DELETE FROM Vehicles WHERE CreatedBy = 'DemoData';
DELETE FROM EngineSpecifications WHERE CreatedBy = 'DemoData';
DELETE FROM PartSuppliers WHERE CreatedBy = 'DemoData';
DELETE FROM Parts WHERE CreatedBy = 'DemoData';
DELETE FROM LaborItems WHERE CreatedBy = 'DemoData';
DELETE FROM Services WHERE CreatedBy = 'DemoData';
DELETE FROM Employees WHERE CreatedBy = 'DemoData';
DELETE FROM Customers WHERE CreatedBy = 'DemoData';
DELETE FROM VehicleBrands WHERE CreatedBy = 'DemoData';
DELETE FROM Suppliers WHERE CreatedBy = 'DemoData';
DELETE FROM PartGroups WHERE CreatedBy = 'DemoData';
DELETE FROM ServiceTypes WHERE CreatedBy = 'DemoData';
DELETE FROM LaborCategories WHERE CreatedBy = 'DemoData';
DELETE FROM Positions WHERE CreatedBy = 'DemoData';
DELETE FROM Departments WHERE CreatedBy = 'DemoData';
DELETE FROM AuditLogs WHERE CreatedBy = 'DemoData';

-- =====================================================
-- BASE TABLES
-- =====================================================

-- 1. DEPARTMENTS
INSERT INTO Departments (Name, Description, IsActive, IsDeleted, CreatedAt, CreatedBy) VALUES
('Kỹ Thuật', 'Phòng kỹ thuật sửa chữa', 1, 0, NOW(), 'DemoData'),
('Dịch Vụ Khách Hàng', 'Phòng chăm sóc khách hàng', 1, 0, NOW(), 'DemoData'),
('Kho', 'Phòng quản lý kho', 1, 0, NOW(), 'DemoData'),
('Kế Toán', 'Phòng kế toán tài chính', 1, 0, NOW(), 'DemoData');

-- 2. POSITIONS
INSERT INTO Positions (Name, Description, IsActive, IsDeleted, CreatedAt, CreatedBy) VALUES
('Thợ Chính', 'Thợ chính sửa chữa', 1, 0, NOW(), 'DemoData'),
('Thợ Phụ', 'Thợ phụ hỗ trợ', 1, 0, NOW(), 'DemoData'),
('Tư Vấn Dịch Vụ', 'Nhân viên tư vấn', 1, 0, NOW(), 'DemoData'),
('Thủ Kho', 'Nhân viên kho', 1, 0, NOW(), 'DemoData'),
('Kế Toán Viên', 'Nhân viên kế toán', 1, 0, NOW(), 'DemoData'),
('Quản Lý', 'Quản lý chi nhánh', 1, 0, NOW(), 'DemoData');

-- 3. SERVICE TYPES (with EstimatedDuration)
INSERT INTO ServiceTypes (TypeName, TypeCode, Description, Category, EstimatedDuration, IsActive, IsDeleted, CreatedAt, CreatedBy) VALUES
('Bảo Dưỡng', 'BD', 'Dịch vụ bảo dưỡng định kỳ', 'Maintenance', 60, 1, 0, NOW(), 'DemoData'),
('Sửa Chữa', 'SC', 'Dịch vụ sửa chữa', 'Repair', 120, 1, 0, NOW(), 'DemoData'),
('Kiểm Định', 'KD', 'Dịch vụ kiểm định xe', 'Inspection', 90, 1, 0, NOW(), 'DemoData'),
('Sơn', 'SON', 'Dịch vụ sơn xe', 'Painting', 480, 1, 0, NOW(), 'DemoData');

-- 4. LABOR CATEGORIES
INSERT INTO LaborCategories (CategoryCode, CategoryName, Description, BaseRate, StandardRate, IsActive, IsDeleted, CreatedAt, CreatedBy) VALUES
('LC001', 'Sửa chữa cơ khí', 'Công sửa chữa cơ khí', 150000, 150000, 1, 0, NOW(), 'DemoData'),
('LC002', 'Bảo dưỡng', 'Công bảo dưỡng', 100000, 100000, 1, 0, NOW(), 'DemoData'),
('LC003', 'Kiểm định', 'Công kiểm định xe', 80000, 80000, 1, 0, NOW(), 'DemoData'),
('LC004', 'Sơn', 'Công sơn xe', 200000, 200000, 1, 0, NOW(), 'DemoData');

-- 5. PART GROUPS
INSERT INTO PartGroups (GroupCode, GroupName, Category, SubCategory, Description, `Function`, Unit, IsActive, IsDeleted, CreatedAt, CreatedBy) VALUES
('PG001', 'Dầu nhớt', 'Dầu', 'Dầu động cơ', 'Nhóm dầu nhớt', 'Lubrication', 'Lít', 1, 0, NOW(), 'DemoData'),
('PG002', 'Lọc', 'Lọc', 'Lọc dầu, gió', 'Nhóm lọc', 'Filtration', 'Cái', 1, 0, NOW(), 'DemoData'),
('PG003', 'Phanh', 'Phanh', 'Má phanh, đĩa phanh', 'Nhóm phanh', 'Braking', 'Bộ', 1, 0, NOW(), 'DemoData'),
('PG004', 'Lốp xe', 'Lốp', 'Lốp ô tô', 'Nhóm lốp xe', 'Traction', 'Cái', 1, 0, NOW(), 'DemoData');

-- 6. VEHICLE BRANDS (with LogoUrl and Website)
INSERT INTO VehicleBrands (BrandName, BrandCode, Country, Description, LogoUrl, Website, IsActive, IsDeleted, CreatedAt, CreatedBy) VALUES
('Toyota', 'TOY', 'Japan', 'Toyota Motor Corporation', '/images/brands/toyota.png', 'https://toyota.com', 1, 0, NOW(), 'DemoData'),
('Honda', 'HON', 'Japan', 'Honda Motor Co., Ltd.', '/images/brands/honda.png', 'https://honda.com', 1, 0, NOW(), 'DemoData'),
('Ford', 'FORD', 'USA', 'Ford Motor Company', '/images/brands/ford.png', 'https://ford.com', 1, 0, NOW(), 'DemoData'),
('Mazda', 'MAZ', 'Japan', 'Mazda Motor Corporation', '/images/brands/mazda.png', 'https://mazda.com', 1, 0, NOW(), 'DemoData');

-- 7. SUPPLIERS
INSERT INTO Suppliers (SupplierCode, SupplierName, Phone, Email, Address, City, Country, Website, ContactPerson, ContactPhone, TaxCode, BankAccount, BankName, PaymentTerms, DeliveryTerms, Notes, IsOEMSupplier, IsActive, LastOrderDate, TotalOrderValue, Rating, IsDeleted, CreatedAt, CreatedBy) VALUES
('SUP001', 'Phụ Tùng Chính Hãng', '0287777777', 'contact@parts.com', '111 Nguyễn Văn Linh', 'TP.HCM', 'Vietnam', 'https://parts.com', 'Nguyễn Văn A', '0901234567', '0123456789', '1234567890', 'Vietcombank', '30 ngày', 'FOB', 'Nhà cung cấp uy tín', 0, 1, NULL, NULL, 4.5, 0, NOW(), 'DemoData'),
('SUP002', 'OEM Parts Vietnam', '0287777778', 'oem@parts.vn', '222 Lê Văn Việt', 'TP.HCM', 'Vietnam', 'https://oemparts.vn', 'Trần Thị B', '0907654321', '9876543210', '0987654321', 'ACB', '60 ngày', 'CIF', 'Nhà cung cấp OEM', 1, 1, NULL, NULL, 5.0, 0, NOW(), 'DemoData');

-- 8. CUSTOMERS
INSERT INTO Customers (Name, Phone, AlternativePhone, Email, Address, TaxCode, ContactPersonName, DateOfBirth, Gender, CreatedDate, UpdatedDate, IsActive, IsDeleted, CreatedAt, CreatedBy) VALUES
('Công ty TNHH ABC', '0281234567', '0281234568', 'abc@company.com', '100 Nguyễn Huệ, Q1, TP.HCM', '0123456789', 'Nguyễn Văn Giám Đốc', NULL, NULL, NOW(), NULL, 1, 0, NOW(), 'DemoData'),
('Trần Thị Bình', '0912345678', NULL, 'binh@email.com', '200 Lê Lợi, Q1, TP.HCM', NULL, NULL, '1985-05-15', 'Nữ', NOW(), NULL, 1, 0, NOW(), 'DemoData'),
('Nguyễn Văn Cường', '0913456789', NULL, 'cuong@email.com', '300 Trần Hưng Đạo, Hà Nội', NULL, NULL, '1990-10-20', 'Nam', NOW(), NULL, 1, 0, NOW(), 'DemoData'),
('Lê Thị Dung', '0914567890', NULL, 'dung@email.com', '400 Hùng Vương, Đà Nẵng', NULL, NULL, '1988-03-15', 'Nữ', NOW(), NULL, 1, 0, NOW(), 'DemoData');

-- =====================================================
-- GET IDs FROM BASE TABLES
-- =====================================================

SET @dept1 = (SELECT Id FROM Departments WHERE Name = 'Kỹ Thuật' AND CreatedBy = 'DemoData' LIMIT 1);
SET @dept2 = (SELECT Id FROM Departments WHERE Name = 'Dịch Vụ Khách Hàng' AND CreatedBy = 'DemoData' LIMIT 1);
SET @pos1 = (SELECT Id FROM Positions WHERE Name = 'Thợ Chính' AND CreatedBy = 'DemoData' LIMIT 1);
SET @pos2 = (SELECT Id FROM Positions WHERE Name = 'Thợ Phụ' AND CreatedBy = 'DemoData' LIMIT 1);
SET @pos3 = (SELECT Id FROM Positions WHERE Name = 'Tư Vấn Dịch Vụ' AND CreatedBy = 'DemoData' LIMIT 1);
SET @st1 = (SELECT Id FROM ServiceTypes WHERE TypeCode = 'BD' AND CreatedBy = 'DemoData' LIMIT 1);
SET @st2 = (SELECT Id FROM ServiceTypes WHERE TypeCode = 'SC' AND CreatedBy = 'DemoData' LIMIT 1);
SET @st3 = (SELECT Id FROM ServiceTypes WHERE TypeCode = 'KD' AND CreatedBy = 'DemoData' LIMIT 1);
SET @lc1 = (SELECT Id FROM LaborCategories WHERE CategoryCode = 'LC001' AND CreatedBy = 'DemoData' LIMIT 1);
SET @lc2 = (SELECT Id FROM LaborCategories WHERE CategoryCode = 'LC002' AND CreatedBy = 'DemoData' LIMIT 1);
SET @pg1 = (SELECT Id FROM PartGroups WHERE GroupCode = 'PG001' AND CreatedBy = 'DemoData' LIMIT 1);
SET @pg2 = (SELECT Id FROM PartGroups WHERE GroupCode = 'PG002' AND CreatedBy = 'DemoData' LIMIT 1);
SET @pg3 = (SELECT Id FROM PartGroups WHERE GroupCode = 'PG003' AND CreatedBy = 'DemoData' LIMIT 1);
SET @cust1 = (SELECT Id FROM Customers WHERE Phone = '0281234567' AND CreatedBy = 'DemoData' LIMIT 1);
SET @cust2 = (SELECT Id FROM Customers WHERE Phone = '0912345678' AND CreatedBy = 'DemoData' LIMIT 1);
SET @cust3 = (SELECT Id FROM Customers WHERE Phone = '0913456789' AND CreatedBy = 'DemoData' LIMIT 1);
SET @supp1 = (SELECT Id FROM Suppliers WHERE SupplierCode = 'SUP001' AND CreatedBy = 'DemoData' LIMIT 1);

-- =====================================================
-- LEVEL 1 DEPENDENT TABLES
-- =====================================================

-- 9. EMPLOYEES
INSERT INTO Employees (Name, Email, Phone, Address, DepartmentId, PositionId, `Position`, Department, HireDate, Salary, Status, Skills, IsDeleted, CreatedAt, CreatedBy) VALUES
('Nguyễn Văn An', 'nva@garage.com', '0901234567', '123 Lý Thường Kiệt', @dept1, @pos1, 'Thợ Chính', 'Kỹ Thuật', '2023-01-15', 15000000, 'Active', 'Sửa chữa động cơ, phanh', 0, NOW(), 'DemoData'),
('Trần Văn Bình', 'tvb@garage.com', '0902345678', '456 Điện Biên Phủ', @dept1, @pos2, 'Thợ Phụ', 'Kỹ Thuật', '2023-03-20', 10000000, 'Active', 'Bảo dưỡng', 0, NOW(), 'DemoData'),
('Lê Thị Cẩm', 'ltc@garage.com', '0903456789', '789 Hai Bà Trưng', @dept2, @pos3, 'Tư Vấn Dịch Vụ', 'Dịch Vụ Khách Hàng', '2023-06-01', 12000000, 'Active', 'Tư vấn khách hàng', 0, NOW(), 'DemoData');

SET @emp1 = (SELECT Id FROM Employees WHERE Email = 'nva@garage.com' AND CreatedBy = 'DemoData' LIMIT 1);
SET @emp2 = (SELECT Id FROM Employees WHERE Email = 'tvb@garage.com' AND CreatedBy = 'DemoData' LIMIT 1);
SET @emp3 = (SELECT Id FROM Employees WHERE Email = 'ltc@garage.com' AND CreatedBy = 'DemoData' LIMIT 1);

-- 10. SERVICES
INSERT INTO Services (Name, Description, Price, Duration, Category, ServiceTypeId, LaborType, SkillLevel, LaborHours, LaborRate, TotalLaborCost, RequiredTools, RequiredSkills, WorkInstructions, IsActive, IsDeleted, CreatedAt, CreatedBy) VALUES
('Thay dầu động cơ', 'Dịch vụ thay dầu và lọc dầu động cơ', 500000, 60, 'Bảo dưỡng', @st1, 'Maintenance', 'Basic', 1, 100000, 100000, NULL, 'Bảo dưỡng cơ bản', NULL, 1, 0, NOW(), 'DemoData'),
('Kiểm tra tổng quát', 'Kiểm tra toàn bộ hệ thống xe', 300000, 90, 'Kiểm định', @st3, 'Inspection', 'Basic', 2, 80000, 160000, NULL, 'Kiểm định xe', NULL, 1, 0, NOW(), 'DemoData'),
('Sửa phanh', 'Thay má phanh và kiểm tra hệ thống phanh', 1500000, 180, 'Sửa chữa', @st2, 'Repair', 'Intermediate', 3, 150000, 450000, NULL, 'Sửa chữa phanh', NULL, 1, 0, NOW(), 'DemoData'),
('Bảo dưỡng định kỳ 10.000km', 'Bảo dưỡng theo chu kỳ', 800000, 120, 'Bảo dưỡng', @st1, 'Maintenance', 'Intermediate', 2, 100000, 200000, NULL, 'Bảo dưỡng định kỳ', NULL, 1, 0, NOW(), 'DemoData');

SET @svc1 = (SELECT Id FROM Services WHERE Name = 'Thay dầu động cơ' AND CreatedBy = 'DemoData' LIMIT 1);
SET @svc3 = (SELECT Id FROM Services WHERE Name = 'Sửa phanh' AND CreatedBy = 'DemoData' LIMIT 1);

-- 11. PARTS
INSERT INTO Parts (PartNumber, PartName, Description, Category, Brand, CostPrice, AverageCostPrice, SellPrice, QuantityInStock, MinimumStock, ReorderLevel, Unit, CompatibleVehicles, Location, SourceType, InvoiceType, HasInvoice, CanUseForCompany, CanUseForInsurance, CanUseForIndividual, `Condition`, SourceReference, PartGroupId, OEMNumber, AftermarketNumber, Manufacturer, Dimensions, Weight, Material, Color, WarrantyMonths, WarrantyConditions, IsOEM, IsActive, IsDeleted, CreatedAt, CreatedBy) VALUES
('P001', 'Dầu nhớt 5W30', 'Dầu nhớt tổng hợp cao cấp', 'Dầu nhớt', 'Castrol', 350000, 350000, 450000, 100, 10, 15, 'Lít', 'Toyota, Honda', 'K1-A1', 'Purchased', 'WithInvoice', 1, 1, 1, 1, 'New', NULL, @pg1, 'OEM-5W30', NULL, 'Castrol', '4L', 4.5, NULL, NULL, 12, 'Không bảo hành nếu pha loãng', 0, 1, 0, NOW(), 'DemoData'),
('P002', 'Lọc dầu', 'Lọc dầu động cơ chính hãng', 'Lọc', 'Denso', 100000, 100000, 150000, 100, 20, 30, 'Cái', 'Toyota, Honda, Ford', 'K1-A2', 'Purchased', 'WithInvoice', 1, 1, 1, 1, 'New', NULL, @pg2, 'OEM-FILTER', NULL, 'Denso', '10x5cm', 0.3, 'Metal', NULL, 6, NULL, 0, 1, 0, NOW(), 'DemoData'),
('P003', 'Má phanh trước', 'Má phanh đĩa trước', 'Phanh', 'Brembo', 600000, 600000, 800000, 50, 5, 10, 'Bộ', 'Toyota Camry', 'K1-B1', 'Purchased', 'WithInvoice', 1, 1, 1, 1, 'New', NULL, @pg3, 'OEM-BRAKE', NULL, 'Brembo', '15x10cm', 2.0, 'Ceramic', NULL, 6, 'Không bảo hành khi va chạm', 0, 1, 0, NOW(), 'DemoData'),
('P004', 'Lọc gió', 'Lọc gió động cơ', 'Lọc', 'Mann', 80000, 80000, 120000, 80, 15, 25, 'Cái', 'Honda City', 'K1-A3', 'Purchased', 'WithInvoice', 1, 1, 1, 1, 'New', NULL, @pg2, NULL, 'AF-001', 'Mann', '20x15cm', 0.5, 'Paper', NULL, 6, NULL, 0, 1, 0, NOW(), 'DemoData');

SET @part1 = (SELECT Id FROM Parts WHERE PartNumber = 'P001' AND CreatedBy = 'DemoData' LIMIT 1);
SET @part2 = (SELECT Id FROM Parts WHERE PartNumber = 'P002' AND CreatedBy = 'DemoData' LIMIT 1);
SET @part3 = (SELECT Id FROM Parts WHERE PartNumber = 'P003' AND CreatedBy = 'DemoData' LIMIT 1);

-- 12. LABOR ITEMS
INSERT INTO LaborItems (LaborCategoryId, CategoryId, PartGroupId, LaborCode, ItemName, LaborName, PartName, Description, StandardHours, LaborRate, TotalLaborCost, SkillLevel, RequiredTools, WorkSteps, Difficulty, IsActive, IsDeleted, CreatedAt, CreatedBy) VALUES
(@lc1, @lc1, @pg3, 'L001', 'Công thay má phanh', 'Công thay má phanh', NULL, 'Tháo lắp và thay má phanh', 2.5, 150000, 375000, 'Intermediate', 'Cờ lê, kích nâng', 'Nâng xe, tháo bánh, thay má phanh, lắp lại', 'Medium', 1, 0, NOW(), 'DemoData'),
(@lc2, @lc2, @pg1, 'L002', 'Công thay dầu', 'Công thay dầu', NULL, 'Thay dầu động cơ và lọc dầu', 1.0, 100000, 100000, 'Basic', 'Chảo hứng dầu, cờ lê lọc', 'Hứng dầu cũ, thay lọc, đổ dầu mới', 'Easy', 1, 0, NOW(), 'DemoData');

SET @labor1 = (SELECT Id FROM LaborItems WHERE LaborCode = 'L001' AND CreatedBy = 'DemoData' LIMIT 1);
SET @labor2 = (SELECT Id FROM LaborItems WHERE LaborCode = 'L002' AND CreatedBy = 'DemoData' LIMIT 1);

-- 13. VEHICLES
INSERT INTO Vehicles (LicensePlate, Brand, Model, Year, Color, VIN, EngineNumber, CustomerId, OwnershipType, VehicleType, UsageType, InsuranceCompany, PolicyNumber, CoverageType, InsuranceStartDate, InsuranceEndDate, InsurancePremium, HasInsurance, IsInsuranceActive, ClaimNumber, AdjusterName, AdjusterPhone, ClaimDate, ClaimSettlementDate, ClaimAmount, ClaimStatus, CompanyName, TaxCode, ContactPerson, ContactPhone, Department, CostCenter, IsDeleted, CreatedAt, CreatedBy) VALUES
('51A-12345', 'Toyota', 'Camry', '2020', 'Đen', 'VIN123456789', 'ENG001', @cust1, 'Company', 'Company', 'Commercial', 'Bảo Việt', 'BH001', 'Full', '2024-01-01', '2024-12-31', 12000000, 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 'Công ty TNHH ABC', '0123456789', 'Nguyễn Văn Giám Đốc', '0281234567', 'Sales', 'CC001', 0, NOW(), 'DemoData'),
('59B-67890', 'Honda', 'City', '2019', 'Trắng', 'VIN234567890', 'ENG002', @cust2, 'Personal', 'Personal', 'Private', 'Bảo Minh', 'BH002', 'Third Party', '2024-03-01', '2025-02-28', 5000000, 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NOW(), 'DemoData'),
('43C-11111', 'Ford', 'Ranger', '2021', 'Xanh', 'VIN345678901', 'ENG003', @cust3, 'Personal', 'Personal', 'Private', 'PTI', 'BH003', 'Full', '2024-06-01', '2025-05-31', 8000000, 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NOW(), 'DemoData');

SET @veh1 = (SELECT Id FROM Vehicles WHERE LicensePlate = '51A-12345' AND CreatedBy = 'DemoData' LIMIT 1);
SET @veh2 = (SELECT Id FROM Vehicles WHERE LicensePlate = '59B-67890' AND CreatedBy = 'DemoData' LIMIT 1);
SET @veh3 = (SELECT Id FROM Vehicles WHERE LicensePlate = '43C-11111' AND CreatedBy = 'DemoData' LIMIT 1);

-- =====================================================
-- WORKFLOW 1: Inspection → Quotation → Appointment → Order → Invoice → Payment
-- Customer: Trần Thị Bình | Vehicle: 59B-67890 | Service: Sửa phanh
-- =====================================================

-- Step 1: Vehicle Inspection
INSERT INTO VehicleInspections (InspectionNumber, CustomerId, VehicleId, InspectionDate, InspectionType, Mileage, CurrentMileage, InspectorName, Status, GeneralCondition, ExteriorCondition, InteriorCondition, EngineCondition, BrakeCondition, SuspensionCondition, TireCondition, ElectricalCondition, CoolingCondition, ExhaustCondition, BatteryCondition, LightsCondition, CustomerComplaints, Recommendations, TechnicianNotes, Findings, Notes, CustomerName, CustomerPhone, VehiclePlate, VehicleMake, VehicleModel, VehicleYear, IsDeleted, CreatedAt, CreatedBy) VALUES
('INS-001', @cust2, @veh2, NOW(), 'General', 45000, 45000, 'Nguyễn Văn An', 'Completed', 'Tốt', 'Bình thường', 'Sạch sẽ', 'Tốt', 'Cần thay', 'Tốt', 'Tốt', 'Tốt', 'Tốt', 'Tốt', 'Tốt', 'Tốt', 'Phanh kêu khi dừng', 'Nên thay má phanh trước', 'Đã kiểm tra toàn bộ hệ thống', 'Má phanh mòn 70%, cần thay', 'Kiểm tra định kỳ', 'Trần Thị Bình', '0912345678', '59B-67890', 'Honda', 'City', '2019', 0, NOW(), 'DemoData');

SET @insp1 = LAST_INSERT_ID();

-- Step 2: Inspection Issues
INSERT INTO InspectionIssues (VehicleInspectionId, Category, IssueName, Description, Severity, RequiresImmediateAction, EstimatedCost, TechnicianNotes, SuggestedServiceId, Status, IsDeleted, CreatedAt, CreatedBy) VALUES
(@insp1, 'Brake', 'Má phanh mòn', 'Má phanh mòn 70%, cần thay ngay', 'High', 1, 800000, 'Cần thay ngay để đảm bảo an toàn', NULL, 'Identified', 0, NOW(), 'DemoData'),
(@insp1, 'Tire', 'Lốp mòn', 'Lốp trước phải mòn, có thể dùng thêm 5000km', 'Medium', 0, 1500000, 'Theo dõi và thay khi cần', NULL, 'Identified', 0, NOW(), 'DemoData');

-- Step 3: Inspection Photos
INSERT INTO InspectionPhotos (VehicleInspectionId, InspectionIssueId, FilePath, FileName, Category, Description, DisplayOrder, IsDeleted, CreatedAt, CreatedBy) VALUES
(@insp1, NULL, '/uploads/inspections/20251012-001-brake.jpg', '20251012-001-brake.jpg', 'Brake', 'Hình ảnh má phanh mòn', 1, 0, NOW(), 'DemoData'),
(@insp1, NULL, '/uploads/inspections/20251012-001-overall.jpg', '20251012-001-overall.jpg', 'Exterior', 'Hình tổng quan xe', 2, 0, NOW(), 'DemoData');

-- Step 4: Service Quotation
INSERT INTO ServiceQuotations (QuotationNumber, CustomerId, VehicleId, VehicleInspectionId, PreparedById, QuotationDate, ExpiryDate, ValidUntil, Status, SubTotal, TaxAmount, VATAmount, TaxRate, DiscountAmount, DiscountPercent, TotalAmount, QuotationType, IsTaxExempt, Notes, CustomerName, CustomerPhone, VehiclePlate, VehicleMake, VehicleModel, ServiceOrderId, IsDeleted, CreatedAt, CreatedBy, ApprovedDate) VALUES
('QT-001', @cust2, @veh2, @insp1, @emp1, NOW(), DATE_ADD(NOW(), INTERVAL 14 DAY), DATE_ADD(NOW(), INTERVAL 14 DAY), 'Approved', 2300000, 230000, 230000, 10, 0, 0, 2530000, 'Personal', 0, 'Báo giá sửa phanh', 'Trần Thị Bình', '0912345678', '59B-67890', 'Honda', 'City', NULL, 0, NOW(), 'DemoData', NOW());

SET @quot1 = LAST_INSERT_ID();

-- Step 5: Quotation Items
INSERT INTO QuotationItems (ServiceQuotationId, QuotationId, ServiceId, PartId, ItemType, ItemName, Description, Quantity, UnitPrice, SubTotal, DiscountAmount, DiscountPercent, VATRate, VATAmount, TotalPrice, TotalAmount, FinalPrice, IsOptional, IsApproved, Notes, DisplayOrder, IsDeleted, CreatedAt, CreatedBy) VALUES
(@quot1, @quot1, @svc3, NULL, 'Service', 'Sửa phanh', 'Dịch vụ sửa phanh', 1, 1500000, 1500000, 0, 0, 10, 150000, 1650000, 1650000, 1650000, 0, 1, NULL, 1, 0, NOW(), 'DemoData'),
(@quot1, @quot1, NULL, @part3, 'Part', 'Má phanh trước', 'Má phanh trước', 1, 800000, 800000, 0, 0, 10, 80000, 880000, 880000, 880000, 0, 1, NULL, 2, 0, NOW(), 'DemoData');

-- Step 6: Appointment
INSERT INTO Appointments (AppointmentNumber, CustomerId, VehicleId, ScheduledDateTime, EstimatedDuration, AppointmentType, ServiceRequested, CustomerNotes, Status, AssignedToId, VehicleInspectionId, ReminderSent, ReminderSentDate, CancellationReason, ServiceOrderId, IsDeleted, CreatedAt, CreatedBy, ConfirmedDate, ArrivalTime, ActualStartTime, ActualEndTime) VALUES
('APT-001', @cust2, @veh2, DATE_ADD(NOW(), INTERVAL 2 DAY), 180, 'Service', 'Sửa phanh', 'Khách hàng muốn sửa sớm', 'Completed', @emp1, @insp1, 1, NOW(), NULL, NULL, 0, NOW(), 'DemoData', NOW(), DATE_ADD(NOW(), INTERVAL 2 DAY), DATE_ADD(NOW(), INTERVAL 2 DAY), DATE_ADD(DATE_ADD(NOW(), INTERVAL 2 DAY), INTERVAL 180 MINUTE));

SET @appt1 = LAST_INSERT_ID();

-- Step 7: Service Order
INSERT INTO ServiceOrders (OrderNumber, CustomerId, VehicleId, VehicleInspectionId, ServiceQuotationId, OrderDate, ScheduledDate, StartDate, CompletedDate, Status, SubTotal, DiscountAmount, VATAmount, TotalAmount, FinalAmount, PaymentStatus, ServiceTotal, PartsTotal, AmountPaid, AmountRemaining, Notes, EstimatedAmount, ActualAmount, IsDeleted, CreatedAt, CreatedBy) VALUES
('SO-001', @cust2, @veh2, @insp1, @quot1, NOW(), DATE_ADD(NOW(), INTERVAL 2 DAY), DATE_ADD(NOW(), INTERVAL 2 DAY), DATE_ADD(NOW(), INTERVAL 2 DAY), 'Completed', 2300000, 0, 230000, 2530000, 2530000, 'Paid', 1500000, 800000, 2530000, 0, 'Đã hoàn thành sửa phanh', 2530000, 2530000, 0, NOW(), 'DemoData');

SET @so1 = LAST_INSERT_ID();

-- Update relationships
UPDATE ServiceQuotations SET ServiceOrderId = @so1 WHERE Id = @quot1;
UPDATE Appointments SET ServiceOrderId = @so1 WHERE Id = @appt1;

-- Step 8: Service Order Items
INSERT INTO ServiceOrderItems (ServiceOrderId, ServiceId, ServiceName, Description, Quantity, UnitPrice, TotalPrice, Discount, FinalPrice, Status, Notes, IsDeleted, CreatedAt, CreatedBy) VALUES
(@so1, @svc3, 'Sửa phanh', 'Dịch vụ sửa phanh toàn diện', 1, 1500000, 1500000, 0, 1500000, 'Completed', 'Đã hoàn thành', 0, NOW(), 'DemoData');

-- Step 9: Service Order Parts
INSERT INTO ServiceOrderParts (ServiceOrderId, PartId, ServiceOrderItemId, PartName, Quantity, UnitCost, UnitPrice, TotalPrice, Status, IsWarranty, WarrantyUntil, ReturnDate, Notes, IsDeleted, CreatedAt, CreatedBy) VALUES
(@so1, @part3, NULL, 'Má phanh trước', 1, 600000, 800000, 800000, 'Used', 1, DATE_ADD(NOW(), INTERVAL 6 MONTH), NULL, 'Bảo hành 6 tháng', 0, NOW(), 'DemoData');

-- Step 10: Service Order Labors
INSERT INTO ServiceOrderLabors (ServiceOrderId, LaborItemId, EmployeeId, ActualHours, LaborRate, TotalLaborCost, Status, Notes, StartTime, EndTime, CompletedTime, IsDeleted, CreatedAt, CreatedBy) VALUES
(@so1, @labor1, @emp1, 2.5, 150000, 375000, 'Completed', 'Hoàn thành tốt', DATE_ADD(NOW(), INTERVAL 2 DAY), DATE_ADD(DATE_ADD(NOW(), INTERVAL 2 DAY), INTERVAL 150 MINUTE), DATE_ADD(DATE_ADD(NOW(), INTERVAL 2 DAY), INTERVAL 150 MINUTE), 0, NOW(), 'DemoData');

-- Step 11: Stock Transaction
INSERT INTO StockTransactions (TransactionNumber, TransactionType, TransactionDate, PartId, Quantity, UnitCost, UnitPrice, TotalCost, TotalAmount, RelatedEntity, RelatedEntityId, ProcessedById, StockAfter, QuantityBefore, QuantityAfter, HasInvoice, IsDeleted, CreatedAt, CreatedBy) VALUES
('ST-001', 2, NOW(), @part3, -1, 600000, 800000, 600000, 800000, 'ServiceOrder', @so1, @emp1, 49, 50, 49, 1, 0, NOW(), 'DemoData');

-- Update part quantity
UPDATE Parts SET QuantityInStock = 49 WHERE Id = @part3;

-- Step 12: Invoice
INSERT INTO Invoices (InvoiceNumber, InvoiceSymbol, InvoiceType, InvoiceDate, DueDate, ServiceOrderId, CustomerId, VehicleId, SubTotal, VATRate, VATAmount, DiscountAmount, TotalAmount, FinalAmount, Currency, Status, PaymentStatus, PaymentMethod, PaidAmount, RemainingAmount, IsApproved, IsDigitallySigned, Notes, SellerName, SellerAddress, SellerTaxCode, SellerPhone, BuyerName, BuyerAddress, BuyerTaxCode, CustomerName, CustomerPhone, CustomerAddress, VehiclePlate, VehicleMake, VehicleModel, VehicleYear, IsDeleted, CreatedAt, CreatedBy, PaidDate) VALUES
('INV-001', 'AA/24E', 'VAT', NOW(), DATE_ADD(NOW(), INTERVAL 7 DAY), @so1, @cust2, @veh2, 2300000, 10, 230000, 0, 2530000, 2530000, 'VND', 'Paid', 'Paid', 'Cash', 2530000, 0, 1, 0, 'Hóa đơn sửa phanh', 'Garage Demo Co', '123 Nguyễn Trãi, Q1, TP.HCM', '0987654321', '02812345678', 'Trần Thị Bình', '200 Lê Lợi, Q1, TP.HCM', NULL, 'Trần Thị Bình', '0912345678', '200 Lê Lợi, Q1, TP.HCM', '59B-67890', 'Honda', 'City', '2019', 0, NOW(), 'DemoData', NOW());

SET @inv1 = LAST_INSERT_ID();

-- Step 13: Invoice Items
INSERT INTO InvoiceItems (InvoiceId, LineNumber, ItemType, ItemName, Description, Unit, Quantity, UnitPrice, TotalPrice, SubTotal, TaxRate, VATRate, TaxAmount, VATAmount, AmountIncludingTax, TotalAmount, ServiceId, PartId, ServiceName, PartName, HasInputInvoice, DisplayOrder, IsDeleted, CreatedAt, CreatedBy) VALUES
(@inv1, 1, 'Service', 'Sửa phanh', 'Dịch vụ sửa phanh', 'Lần', 1, 1500000, 1500000, 1500000, 10, 10, 150000, 150000, 1650000, 1650000, @svc3, NULL, 'Sửa phanh', NULL, 1, 1, 0, NOW(), 'DemoData'),
(@inv1, 2, 'Part', 'Má phanh trước', 'Má phanh trước', 'Bộ', 1, 800000, 800000, 800000, 10, 10, 80000, 80000, 880000, 880000, NULL, @part3, NULL, 'Má phanh trước', 1, 2, 0, NOW(), 'DemoData');

-- Step 14: Payment
INSERT INTO Payments (PaymentDate, CustomerId, CustomerName, CustomerPhone, InvoiceId, InvoiceNumber, Amount, PaymentMethod, ReferenceNumber, Notes, Status, IsDeleted, CreatedAt, CreatedBy) VALUES
(NOW(), @cust2, 'Trần Thị Bình', '0912345678', @inv1, 'INV-001', 2530000, 'Cash', 'PAY-001', 'Thanh toán tiền mặt', 'Completed', 0, NOW(), 'DemoData');

-- Step 15: Financial Transaction
INSERT INTO FinancialTransactions (TransactionNumber, TransactionType, Category, SubCategory, Amount, Currency, TransactionDate, PaymentMethod, ReferenceNumber, Description, RelatedEntity, RelatedEntityId, EmployeeId, ApprovedBy, ApprovedDate, Notes, IsApproved, IsReconciled, Status, IsDeleted, CreatedAt, CreatedBy) VALUES
('FIN-001', 'Income', 'Service Revenue', 'Brake Repair', 2530000, 'VND', NOW(), 'Cash', 'INV-001', 'Doanh thu từ hóa đơn INV-001', 'Invoice', @inv1, @emp3, 'Manager', NOW(), NULL, 1, 1, 'Completed', 0, NOW(), 'DemoData');

-- Step 16: Vehicle Insurance
INSERT INTO VehicleInsurances (VehicleId, PolicyNumber, InsuranceCompany, CoverageType, StartDate, EndDate, PremiumAmount, Currency, PaymentMethod, IsActive, IsRenewed, Notes, IsDeleted, CreatedAt, CreatedBy) VALUES
(@veh2, 'BH002', 'Bảo Minh', 'Third Party', '2024-03-01', '2025-02-28', 5000000, 'VND', 'BankTransfer', 1, 0, 'Bảo hiểm bên thứ 3', 0, NOW(), 'DemoData');

-- =====================================================
-- WORKFLOW 2: Oil Change Service
-- Customer: Nguyễn Văn Cường | Vehicle: 43C-11111 | Service: Thay dầu
-- =====================================================

-- Step 1: Vehicle Inspection
INSERT INTO VehicleInspections (InspectionNumber, CustomerId, VehicleId, InspectionDate, InspectionType, Mileage, CurrentMileage, InspectorName, Status, GeneralCondition, ExteriorCondition, InteriorCondition, EngineCondition, BrakeCondition, SuspensionCondition, TireCondition, ElectricalCondition, CoolingCondition, ExhaustCondition, BatteryCondition, LightsCondition, CustomerComplaints, Recommendations, TechnicianNotes, Findings, Notes, CustomerName, CustomerPhone, VehiclePlate, VehicleMake, VehicleModel, VehicleYear, IsDeleted, CreatedAt, CreatedBy) VALUES
('INS-002', @cust3, @veh3, NOW(), 'Pre-service', 30000, 30000, 'Trần Văn Bình', 'Completed', 'Rất tốt', 'Tốt', 'Sạch', 'Tốt', 'Tốt', 'Tốt', 'Tốt', 'Tốt', 'Tốt', 'Tốt', 'Tốt', 'Tốt', 'Muốn bảo dưỡng định kỳ', 'Thay dầu và kiểm tra tổng quát', 'Xe còn mới, tình trạng tốt', 'Không có vấn đề', 'Bảo dưỡng 30.000km', 'Nguyễn Văn Cường', '0913456789', '43C-11111', 'Ford', 'Ranger', '2021', 0, NOW(), 'DemoData');

SET @insp2 = LAST_INSERT_ID();

-- Step 2: Quotation
INSERT INTO ServiceQuotations (QuotationNumber, CustomerId, VehicleId, VehicleInspectionId, PreparedById, QuotationDate, ExpiryDate, ValidUntil, Status, SubTotal, TaxAmount, VATAmount, TaxRate, DiscountAmount, DiscountPercent, TotalAmount, QuotationType, IsTaxExempt, Notes, CustomerName, CustomerPhone, VehiclePlate, VehicleMake, VehicleModel, ServiceOrderId, IsDeleted, CreatedAt, CreatedBy, ApprovedDate) VALUES
('QT-002', @cust3, @veh3, @insp2, @emp2, NOW(), DATE_ADD(NOW(), INTERVAL 14 DAY), DATE_ADD(NOW(), INTERVAL 14 DAY), 'Approved', 2400000, 228000, 228000, 10, 120000, 5, 2508000, 'Personal', 0, 'Báo giá thay dầu', 'Nguyễn Văn Cường', '0913456789', '43C-11111', 'Ford', 'Ranger', NULL, 0, NOW(), 'DemoData', NOW());

SET @quot2 = LAST_INSERT_ID();

-- Step 3: Quotation Items
INSERT INTO QuotationItems (ServiceQuotationId, QuotationId, ServiceId, PartId, ItemType, ItemName, Description, Quantity, UnitPrice, SubTotal, DiscountAmount, DiscountPercent, VATRate, VATAmount, TotalPrice, TotalAmount, FinalPrice, IsOptional, IsApproved, Notes, DisplayOrder, IsDeleted, CreatedAt, CreatedBy) VALUES
(@quot2, @quot2, @svc1, NULL, 'Service', 'Thay dầu động cơ', 'Thay dầu động cơ', 1, 500000, 500000, 0, 0, 10, 50000, 550000, 550000, 550000, 0, 1, NULL, 1, 0, NOW(), 'DemoData'),
(@quot2, @quot2, NULL, @part1, 'Part', 'Dầu nhớt 5W30', 'Dầu nhớt 4 lít', 4, 450000, 1800000, 0, 0, 10, 180000, 1980000, 1980000, 1980000, 0, 1, NULL, 2, 0, NOW(), 'DemoData'),
(@quot2, @quot2, NULL, @part2, 'Part', 'Lọc dầu', 'Lọc dầu động cơ', 1, 150000, 150000, 30000, 20, 10, 12000, 132000, 132000, 132000, 0, 1, 'Giảm giá 20%', 3, 0, NOW(), 'DemoData');

-- Step 4: Appointment
INSERT INTO Appointments (AppointmentNumber, CustomerId, VehicleId, ScheduledDateTime, EstimatedDuration, AppointmentType, ServiceRequested, CustomerNotes, Status, AssignedToId, VehicleInspectionId, ReminderSent, ReminderSentDate, CancellationReason, ServiceOrderId, IsDeleted, CreatedAt, CreatedBy, ConfirmedDate, ArrivalTime, ActualStartTime, ActualEndTime) VALUES
('APT-002', @cust3, @veh3, DATE_ADD(NOW(), INTERVAL 3 DAY), 120, 'Maintenance', 'Bảo dưỡng', 'Đặt lịch trước', 'Completed', @emp2, @insp2, 1, NOW(), NULL, NULL, 0, NOW(), 'DemoData', NOW(), DATE_ADD(NOW(), INTERVAL 3 DAY), DATE_ADD(NOW(), INTERVAL 3 DAY), DATE_ADD(DATE_ADD(NOW(), INTERVAL 3 DAY), INTERVAL 120 MINUTE));

SET @appt2 = LAST_INSERT_ID();

-- Step 5: Service Order
INSERT INTO ServiceOrders (OrderNumber, CustomerId, VehicleId, VehicleInspectionId, ServiceQuotationId, OrderDate, ScheduledDate, StartDate, CompletedDate, Status, SubTotal, DiscountAmount, VATAmount, TotalAmount, FinalAmount, PaymentStatus, ServiceTotal, PartsTotal, AmountPaid, AmountRemaining, Notes, EstimatedAmount, ActualAmount, IsDeleted, CreatedAt, CreatedBy) VALUES
('SO-002', @cust3, @veh3, @insp2, @quot2, NOW(), DATE_ADD(NOW(), INTERVAL 3 DAY), DATE_ADD(NOW(), INTERVAL 3 DAY), DATE_ADD(NOW(), INTERVAL 3 DAY), 'Completed', 2400000, 120000, 228000, 2508000, 2508000, 'Paid', 500000, 1920000, 2508000, 0, 'Đã hoàn thành thay dầu', 2508000, 2508000, 0, NOW(), 'DemoData');

SET @so2 = LAST_INSERT_ID();

-- Update relationships
UPDATE ServiceQuotations SET ServiceOrderId = @so2 WHERE Id = @quot2;
UPDATE Appointments SET ServiceOrderId = @so2 WHERE Id = @appt2;

-- Step 6: Service Order Items/Parts/Labors
INSERT INTO ServiceOrderItems (ServiceOrderId, ServiceId, ServiceName, Description, Quantity, UnitPrice, TotalPrice, Discount, FinalPrice, Status, Notes, IsDeleted, CreatedAt, CreatedBy) VALUES
(@so2, @svc1, 'Thay dầu động cơ', 'Thay dầu và lọc dầu', 1, 500000, 500000, 0, 500000, 'Completed', 'Đã hoàn thành', 0, NOW(), 'DemoData');

INSERT INTO ServiceOrderParts (ServiceOrderId, PartId, ServiceOrderItemId, PartName, Quantity, UnitCost, UnitPrice, TotalPrice, Status, IsWarranty, WarrantyUntil, ReturnDate, Notes, IsDeleted, CreatedAt, CreatedBy) VALUES
(@so2, @part1, NULL, 'Dầu nhớt 5W30', 4, 350000, 450000, 1800000, 'Used', 1, DATE_ADD(NOW(), INTERVAL 12 MONTH), NULL, 'Bảo hành 12 tháng', 0, NOW(), 'DemoData'),
(@so2, @part2, NULL, 'Lọc dầu', 1, 100000, 120000, 120000, 'Used', 1, DATE_ADD(NOW(), INTERVAL 6 MONTH), NULL, 'Giảm giá', 0, NOW(), 'DemoData');

INSERT INTO ServiceOrderLabors (ServiceOrderId, LaborItemId, EmployeeId, ActualHours, LaborRate, TotalLaborCost, Status, Notes, StartTime, EndTime, CompletedTime, IsDeleted, CreatedAt, CreatedBy) VALUES
(@so2, @labor2, @emp2, 1.0, 100000, 100000, 'Completed', 'Hoàn thành', DATE_ADD(NOW(), INTERVAL 3 DAY), DATE_ADD(DATE_ADD(NOW(), INTERVAL 3 DAY), INTERVAL 60 MINUTE), DATE_ADD(DATE_ADD(NOW(), INTERVAL 3 DAY), INTERVAL 60 MINUTE), 0, NOW(), 'DemoData');

-- Step 7: Stock Transactions
INSERT INTO StockTransactions (TransactionNumber, TransactionType, TransactionDate, PartId, Quantity, UnitCost, UnitPrice, TotalCost, TotalAmount, RelatedEntity, RelatedEntityId, ProcessedById, StockAfter, QuantityBefore, QuantityAfter, HasInvoice, Notes, IsDeleted, CreatedAt, CreatedBy) VALUES
('ST-002', 2, NOW(), @part1, -4, 350000, 450000, 1400000, 1800000, 'ServiceOrder', @so2, @emp2, 96, 100, 96, 1, 'Xuất dầu cho SO-002', 0, NOW(), 'DemoData'),
('ST-003', 2, NOW(), @part2, -1, 100000, 120000, 100000, 120000, 'ServiceOrder', @so2, @emp2, 99, 100, 99, 1, 'Xuất lọc dầu cho SO-002', 0, NOW(), 'DemoData');

UPDATE Parts SET QuantityInStock = 96 WHERE Id = @part1;
UPDATE Parts SET QuantityInStock = 99 WHERE Id = @part2;

-- Step 8: Invoices
INSERT INTO Invoices (InvoiceNumber, InvoiceSymbol, InvoiceType, InvoiceDate, DueDate, ServiceOrderId, CustomerId, VehicleId, SubTotal, VATRate, VATAmount, DiscountAmount, TotalAmount, FinalAmount, Currency, Status, PaymentStatus, PaymentMethod, PaidAmount, RemainingAmount, IsApproved, IsDigitallySigned, Notes, SellerName, SellerAddress, SellerTaxCode, SellerPhone, BuyerName, BuyerAddress, BuyerTaxCode, CustomerName, CustomerPhone, CustomerAddress, VehiclePlate, VehicleMake, VehicleModel, VehicleYear, IsDeleted, CreatedAt, CreatedBy, PaidDate) VALUES
('INV-002', 'AA/24E', 'VAT', NOW(), DATE_ADD(NOW(), INTERVAL 7 DAY), @so2, @cust3, @veh3, 2400000, 10, 228000, 120000, 2508000, 2508000, 'VND', 'Paid', 'Paid', 'BankTransfer', 2508000, 0, 1, 0, 'Hóa đơn thay dầu', 'Garage Demo Co', '123 Nguyễn Trãi, Q1, TP.HCM', '0987654321', '02812345678', 'Nguyễn Văn Cường', '300 Trần Hưng Đạo, Hà Nội', NULL, 'Nguyễn Văn Cường', '0913456789', '300 Trần Hưng Đạo, Hà Nội', '43C-11111', 'Ford', 'Ranger', '2021', 0, NOW(), 'DemoData', NOW());

SET @inv2 = LAST_INSERT_ID();

-- Step 9: Invoice Items
INSERT INTO InvoiceItems (InvoiceId, LineNumber, ItemType, ItemName, Description, Unit, Quantity, UnitPrice, TotalPrice, SubTotal, TaxRate, VATRate, TaxAmount, VATAmount, AmountIncludingTax, TotalAmount, ServiceId, PartId, ServiceName, PartName, HasInputInvoice, DisplayOrder, IsDeleted, CreatedAt, CreatedBy) VALUES
(@inv2, 1, 'Service', 'Thay dầu động cơ', 'Thay dầu động cơ', 'Lần', 1, 500000, 500000, 500000, 10, 10, 50000, 50000, 550000, 550000, @svc1, NULL, 'Thay dầu động cơ', NULL, 1, 1, 0, NOW(), 'DemoData'),
(@inv2, 2, 'Part', 'Dầu nhớt 5W30', 'Dầu nhớt 4 lít', 'Lít', 4, 450000, 1800000, 1800000, 10, 10, 180000, 180000, 1980000, 1980000, NULL, @part1, NULL, 'Dầu nhớt 5W30', 1, 2, 0, NOW(), 'DemoData'),
(@inv2, 3, 'Part', 'Lọc dầu', 'Lọc dầu động cơ', 'Cái', 1, 120000, 120000, 120000, 10, 10, 12000, 12000, 132000, 132000, NULL, @part2, NULL, 'Lọc dầu', 1, 3, 0, NOW(), 'DemoData');

-- Step 10: Payments
INSERT INTO Payments (PaymentDate, CustomerId, CustomerName, CustomerPhone, InvoiceId, InvoiceNumber, Amount, PaymentMethod, ReferenceNumber, Notes, Status, IsDeleted, CreatedAt, CreatedBy) VALUES
(NOW(), @cust3, 'Nguyễn Văn Cường', '0913456789', @inv2, 'INV-002', 2508000, 'BankTransfer', 'PAY-002', 'Chuyển khoản', 'Completed', 0, NOW(), 'DemoData');

-- Step 11: Financial Transactions
INSERT INTO FinancialTransactions (TransactionNumber, TransactionType, Category, SubCategory, Amount, Currency, TransactionDate, PaymentMethod, ReferenceNumber, Description, RelatedEntity, RelatedEntityId, EmployeeId, ApprovedBy, ApprovedDate, Notes, IsApproved, IsReconciled, Status, IsDeleted, CreatedAt, CreatedBy) VALUES
('FIN-002', 'Income', 'Service Revenue', 'Oil Change', 2508000, 'VND', NOW(), 'BankTransfer', 'INV-002', 'Doanh thu từ hóa đơn INV-002', 'Invoice', @inv2, @emp3, 'Manager', NOW(), NULL, 1, 1, 'Completed', 0, NOW(), 'DemoData');

-- =====================================================
-- ADDITIONAL DATA
-- =====================================================

-- Part Suppliers
INSERT INTO PartSuppliers (PartId, SupplierId, SupplierPartNumber, CostPrice, Currency, MinimumOrderQuantity, LeadTimeDays, IsPreferred, LastOrderDate, LastCostPrice, Notes, IsActive, IsDeleted, CreatedAt, CreatedBy) VALUES
(@part1, @supp1, 'SUP-P001', 350000, 'VND', 10, 7, 1, NULL, 350000, 'Nhà cung cấp chính', 1, 0, NOW(), 'DemoData'),
(@part2, @supp1, 'SUP-P002', 100000, 'VND', 20, 5, 1, NULL, 100000, 'Nhà cung cấp chính', 1, 0, NOW(), 'DemoData'),
(@part3, @supp1, 'SUP-P003', 600000, 'VND', 5, 10, 1, NULL, 600000, 'Nhà cung cấp chính', 1, 0, NOW(), 'DemoData');

-- Purchase Order  
INSERT INTO PurchaseOrders (OrderNumber, SupplierId, OrderDate, ExpectedDeliveryDate, ActualDeliveryDate, Status, SubTotal, TaxAmount, ShippingCost, TotalAmount, EmployeeId, IsApproved, Notes, IsDeleted, CreatedAt, CreatedBy) VALUES
('PO-001', @supp1, NOW(), DATE_ADD(NOW(), INTERVAL 7 DAY), NULL, 'Pending', 5000000, 500000, 0, 5500000, @emp1, 0, 'Đơn đặt hàng phụ tùng tháng 10', 0, NOW(), 'DemoData');

SET @po1 = LAST_INSERT_ID();

-- Purchase Order Items
INSERT INTO PurchaseOrderItems (PurchaseOrderId, PartId, PartName, QuantityOrdered, QuantityReceived, UnitPrice, TotalPrice, IsReceived, Notes, IsDeleted, CreatedAt, CreatedBy) VALUES
(@po1, @part1, 'Dầu nhớt 5W30', 20, 0, 350000, 7000000, 0, 'Đặt hàng tháng 10', 0, NOW(), 'DemoData'),
(@po1, @part2, 'Lọc dầu', 30, 0, 100000, 3000000, 0, 'Đặt hàng tháng 10', 0, NOW(), 'DemoData'),
(@po1, @part3, 'Má phanh trước', 10, 0, 600000, 6000000, 0, 'Đặt hàng tháng 10', 0, NOW(), 'DemoData');

-- Audit Logs
INSERT INTO AuditLogs (EntityName, EntityId, `Action`, UserId, UserName, `Timestamp`, IpAddress, UserAgent, Details, Severity, IsDeleted, CreatedAt, CreatedBy) VALUES
('ServiceOrder', @so1, 'Create', '1', 'nva@garage.com', NOW(), '192.168.1.100', 'Mozilla/5.0', 'Tạo lệnh sửa chữa SO-001', 'Info', 0, NOW(), 'DemoData'),
('Invoice', @inv1, 'Create', '1', 'nva@garage.com', NOW(), '192.168.1.100', 'Mozilla/5.0', 'Tạo hóa đơn INV-001', 'Info', 0, NOW(), 'DemoData'),
('Payment', @inv1, 'Create', '3', 'ltc@garage.com', NOW(), '192.168.1.101', 'Mozilla/5.0', 'Nhận thanh toán INV-001', 'Info', 0, NOW(), 'DemoData'),
('ServiceOrder', @so2, 'Create', '2', 'tvb@garage.com', NOW(), '192.168.1.100', 'Mozilla/5.0', 'Tạo lệnh sửa chữa SO-002', 'Info', 0, NOW(), 'DemoData'),
('Invoice', @inv2, 'Create', '2', 'tvb@garage.com', NOW(), '192.168.1.100', 'Mozilla/5.0', 'Tạo hóa đơn INV-002', 'Info', 0, NOW(), 'DemoData'),
('Payment', @inv2, 'Create', '3', 'ltc@garage.com', NOW(), '192.168.1.101', 'Mozilla/5.0', 'Nhận thanh toán INV-002', 'Info', 0, NOW(), 'DemoData');

SET FOREIGN_KEY_CHECKS = 1;
SET SQL_SAFE_UPDATES = 1;

-- =====================================================
-- VERIFICATION
-- =====================================================

SELECT '╔════════════════════════════════════════╗' as '';
SELECT '║   DEMO DATA LOADED SUCCESSFULLY!       ║' as '';
SELECT '╚════════════════════════════════════════╝' as '';

SELECT 
    'Table' as TableName, 
    'Records' as Count
UNION ALL SELECT 'Departments', CAST(COUNT(*) as CHAR) FROM Departments WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'Positions', CAST(COUNT(*) as CHAR) FROM Positions WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'ServiceTypes', CAST(COUNT(*) as CHAR) FROM ServiceTypes WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'LaborCategories', CAST(COUNT(*) as CHAR) FROM LaborCategories WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'PartGroups', CAST(COUNT(*) as CHAR) FROM PartGroups WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'VehicleBrands', CAST(COUNT(*) as CHAR) FROM VehicleBrands WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'Suppliers', CAST(COUNT(*) as CHAR) FROM Suppliers WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'Customers', CAST(COUNT(*) as CHAR) FROM Customers WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'Employees', CAST(COUNT(*) as CHAR) FROM Employees WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'Services', CAST(COUNT(*) as CHAR) FROM Services WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'Parts', CAST(COUNT(*) as CHAR) FROM Parts WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'LaborItems', CAST(COUNT(*) as CHAR) FROM LaborItems WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'Vehicles', CAST(COUNT(*) as CHAR) FROM Vehicles WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'VehicleInspections', CAST(COUNT(*) as CHAR) FROM VehicleInspections WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'InspectionIssues', CAST(COUNT(*) as CHAR) FROM InspectionIssues WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'InspectionPhotos', CAST(COUNT(*) as CHAR) FROM InspectionPhotos WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'ServiceQuotations', CAST(COUNT(*) as CHAR) FROM ServiceQuotations WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'QuotationItems', CAST(COUNT(*) as CHAR) FROM QuotationItems WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'Appointments', CAST(COUNT(*) as CHAR) FROM Appointments WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'ServiceOrders', CAST(COUNT(*) as CHAR) FROM ServiceOrders WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'ServiceOrderItems', CAST(COUNT(*) as CHAR) FROM ServiceOrderItems WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'ServiceOrderParts', CAST(COUNT(*) as CHAR) FROM ServiceOrderParts WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'ServiceOrderLabors', CAST(COUNT(*) as CHAR) FROM ServiceOrderLabors WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'StockTransactions', CAST(COUNT(*) as CHAR) FROM StockTransactions WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'Invoices', CAST(COUNT(*) as CHAR) FROM Invoices WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'InvoiceItems', CAST(COUNT(*) as CHAR) FROM InvoiceItems WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'Payments', CAST(COUNT(*) as CHAR) FROM Payments WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'FinancialTransactions', CAST(COUNT(*) as CHAR) FROM FinancialTransactions WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'VehicleInsurances', CAST(COUNT(*) as CHAR) FROM VehicleInsurances WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'PartSuppliers', CAST(COUNT(*) as CHAR) FROM PartSuppliers WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'PurchaseOrders', CAST(COUNT(*) as CHAR) FROM PurchaseOrders WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'PurchaseOrderItems', CAST(COUNT(*) as CHAR) FROM PurchaseOrderItems WHERE CreatedBy = 'DemoData'
UNION ALL SELECT 'AuditLogs', CAST(COUNT(*) as CHAR) FROM AuditLogs WHERE CreatedBy = 'DemoData';

SELECT '╔════════════════════════════════════════╗' as '';
SELECT '║         WORKFLOW SUMMARY               ║' as '';
SELECT '╚════════════════════════════════════════╝' as '';

SELECT 
    'Workflow' as Summary,
    COUNT(DISTINCT so.Id) as ServiceOrders,
    COUNT(DISTINCT i.Id) as Invoices,
    COUNT(DISTINCT p.Id) as Payments,
    CONCAT(FORMAT(COALESCE(SUM(p.Amount), 0), 0), ' VNĐ') as TotalRevenue
FROM ServiceOrders so
LEFT JOIN Invoices i ON i.ServiceOrderId = so.Id
LEFT JOIN Payments p ON p.InvoiceId = i.Id
WHERE so.CreatedBy = 'DemoData';

SELECT 
    so.OrderNumber as 'Lệnh SC',
    c.Name as 'Khách hàng',
    v.LicensePlate as 'Biển số',
    so.Status as 'Trạng thái',
    i.InvoiceNumber as 'Hóa đơn',
    CONCAT(FORMAT(p.Amount, 0), ' VNĐ') as 'Thanh toán'
FROM ServiceOrders so
LEFT JOIN Customers c ON c.Id = so.CustomerId
LEFT JOIN Vehicles v ON v.Id = so.VehicleId
LEFT JOIN Invoices i ON i.ServiceOrderId = so.Id
LEFT JOIN Payments p ON p.InvoiceId = i.Id
WHERE so.CreatedBy = 'DemoData'
ORDER BY so.Id;

SELECT '✅ DEMO DATA READY FOR TESTING!' as Status;

