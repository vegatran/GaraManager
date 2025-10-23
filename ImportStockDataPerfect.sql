-- =====================================================
-- SCRIPT IMPORT DỮ LIỆU TỒN KHO (PERFECT - 100% CHÍNH XÁC)
-- =====================================================

USE garamanagement;

-- Tắt safe update mode
SET SQL_SAFE_UPDATES = 0;

-- =====================================================
-- 1. THÊM PARTS MỚI VÀO BẢNG Parts (100% CHÍNH XÁC)
-- =====================================================

INSERT INTO Parts (
    PartNumber, PartName, Description, 
    CostPrice, AverageCostPrice, SellPrice, QuantityInStock, 
    MinimumStock, Unit, Category, Brand, 
    IsActive, CreatedAt, UpdatedAt, IsDeleted,
    SourceType, InvoiceType, HasInvoice, 
    CanUseForCompany, CanUseForInsurance, CanUseForIndividual,
    `Condition`, WarrantyMonths, IsOEM
) VALUES 
-- Ắc quy
('ACQUY_70', 'BÌNH ẮC QUY GS N70', 'Ắc quy GS N70 cho xe hạng trung', 
 1194340, 1194340, 1300000, 23, 
 5, 'CÁI', 'Ắc quy', 'GS',
 1, NOW(), NOW(), 0,
 'Purchased', 'WithInvoice', 1, 1, 1, 1,
 'New', 12, 0),

-- Bạc đạn
('BAQGN', 'Bạc đạn ZA.60TB039B09', 'Bạc đạn chất lượng cao', 
 285000, 285000, 350000, 95, 
 10, 'CÁI', 'Bạc đạn', 'ZA',
 1, NOW(), NOW(), 0,
 'Purchased', 'WithInvoice', 1, 1, 1, 1,
 'New', 24, 0),

-- Đĩa phanh
('BDZA', 'Đĩa phanh BOSCH', 'Đĩa phanh BOSCH chất lượng cao', 
 650000, 650000, 750000, 20, 
 5, 'CÁI', 'Phanh', 'BOSCH',
 1, NOW(), NOW(), 0,
 'Purchased', 'WithInvoice', 1, 1, 1, 1,
 'New', 12, 0),

-- Dầu động cơ
('DBT10W40', 'Dầu bôi trơn ENEOS AL SNCF 10W40 4L *6', 'Dầu động cơ ENEOS 10W40', 
 272727, 272727, 300000, 24, 
 10, 'Chai', 'Dầu nhớt', 'ENEOS',
 1, NOW(), NOW(), 0,
 'Purchased', 'WithInvoice', 1, 1, 1, 1,
 'New', 12, 0),

-- Má phanh
('MABOSCH', 'Má phanh BOSCH', 'Má phanh BOSCH chất lượng cao', 
 350000, 350000, 400000, 25, 
 5, 'Bộ', 'Phanh', 'BOSCH',
 1, NOW(), NOW(), 0,
 'Purchased', 'WithInvoice', 1, 1, 1, 1,
 'New', 12, 0),

-- Lốp xe
('LOTP205', 'Lốp Michelin 205/55R16', 'Lốp xe Michelin 205/55R16', 
 1000000, 1000000, 1200000, 20, 
 5, 'Chiếc', 'Lốp xe', 'Michelin',
 1, NOW(), NOW(), 0,
 'Purchased', 'WithInvoice', 1, 1, 1, 1,
 'New', 12, 0),

-- Sơn xe
('SONXE2K', 'Sơn xe 2K', 'Sơn xe 2K chất lượng cao', 
 1000000, 1000000, 1200000, 10, 
 2, 'Bộ', 'Sơn', 'Generic',
 1, NOW(), NOW(), 0,
 'Purchased', 'WithInvoice', 1, 1, 1, 1,
 'New', 6, 0),

-- Dầu phanh
('DAUPHANH', 'Dầu phanh DOT4', 'Dầu phanh DOT4 chất lượng cao', 
 150000, 150000, 180000, 50, 
 10, 'Chai', 'Dầu phanh', 'Generic',
 1, NOW(), NOW(), 0,
 'Purchased', 'WithInvoice', 1, 1, 1, 1,
 'New', 12, 0),

-- Bugi
('BUGINGK', 'Bugi NGK', 'Bugi NGK chất lượng cao', 
 250000, 250000, 300000, 100, 
 20, 'CÁI', 'Bugi', 'NGK',
 1, NOW(), NOW(), 0,
 'Purchased', 'WithInvoice', 1, 1, 1, 1,
 'New', 12, 0),

-- Dây curoa
('DAYCUROA', 'Dây curoa động cơ', 'Dây curoa động cơ chất lượng cao', 
 200000, 200000, 250000, 30, 
 10, 'CÁI', 'Dây curoa', 'Generic',
 1, NOW(), NOW(), 0,
 'Purchased', 'WithInvoice', 1, 1, 1, 1,
 'New', 12, 0),

-- Lọc gió
('LOCGIO', 'Lọc gió động cơ', 'Lọc gió động cơ chất lượng cao', 
 180000, 180000, 220000, 40, 
 10, 'CÁI', 'Lọc', 'Generic',
 1, NOW(), NOW(), 0,
 'Purchased', 'WithInvoice', 1, 1, 1, 1,
 'New', 6, 0),

-- Dầu hộp số
('DAUHOPSO', 'Dầu hộp số tự động', 'Dầu hộp số tự động chất lượng cao', 
 300000, 300000, 350000, 25, 
 5, 'Lít', 'Dầu hộp số', 'Generic',
 1, NOW(), NOW(), 0,
 'Purchased', 'WithInvoice', 1, 1, 1, 1,
 'New', 12, 0);

-- =====================================================
-- 2. TẠO STOCK TRANSACTIONS CHO TỒN ĐẦU KỲ
-- =====================================================

INSERT INTO StockTransactions (
    TransactionNumber, PartId, TransactionType, 
    Quantity, QuantityBefore, QuantityAfter, StockAfter,
    UnitCost, UnitPrice, TotalCost, TotalAmount, TransactionDate,
    Notes, CreatedAt, UpdatedAt, IsDeleted, HasInvoice
) VALUES 
-- Ắc quy N70 - Tồn đầu kỳ: 10 cái
 ('ST001', (SELECT Id FROM Parts WHERE PartNumber = 'ACQUY_70'), 4,
  10, 0, 10, 10,
  1194340, 1300000, 11943400, 13000000, '2025-01-01 08:00:00',
  'Tồn đầu kỳ - Ắc quy GS N70', NOW(), NOW(), 0, 1),

-- Bạc đạn - Tồn đầu kỳ: 95 cái  
('ST002', (SELECT Id FROM Parts WHERE PartNumber = 'BAQGN'), 4,
 95, 0, 95, 95,
 285000, 350000, 27075000, 33250000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Bạc đạn ZA', NOW(), NOW(), 0, 1),

-- Đĩa phanh - Tồn đầu kỳ: 20 cái
('ST003', (SELECT Id FROM Parts WHERE PartNumber = 'BDZA'), 4,
 20, 0, 20, 20,
 650000, 750000, 13000000, 15000000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Đĩa phanh BOSCH', NOW(), NOW(), 0, 1),

-- Dầu động cơ - Tồn đầu kỳ: 0 chai
('ST004', (SELECT Id FROM Parts WHERE PartNumber = 'DBT10W40'), 4,
 0, 0, 0, 0,
 272727, 300000, 0, 0, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Dầu ENEOS', NOW(), NOW(), 0, 1),

-- Má phanh - Tồn đầu kỳ: 25 bộ
('ST005', (SELECT Id FROM Parts WHERE PartNumber = 'MABOSCH'), 4,
 25, 0, 25, 25,
 350000, 400000, 8750000, 10000000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Má phanh BOSCH', NOW(), NOW(), 0, 1),

-- Lốp xe - Tồn đầu kỳ: 20 chiếc
('ST006', (SELECT Id FROM Parts WHERE PartNumber = 'LOTP205'), 4,
 20, 0, 20, 20,
 1000000, 1200000, 20000000, 24000000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Lốp Michelin', NOW(), NOW(), 0, 1),

-- Sơn xe - Tồn đầu kỳ: 10 bộ
('ST007', (SELECT Id FROM Parts WHERE PartNumber = 'SONXE2K'), 4,
 10, 0, 10, 10,
 1000000, 1200000, 10000000, 12000000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Sơn xe 2K', NOW(), NOW(), 0, 1),

-- Dầu phanh - Tồn đầu kỳ: 50 chai
('ST008', (SELECT Id FROM Parts WHERE PartNumber = 'DAUPHANH'), 4,
 50, 0, 50, 50,
 150000, 180000, 7500000, 9000000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Dầu phanh DOT4', NOW(), NOW(), 0, 1),

-- Bugi - Tồn đầu kỳ: 100 cái
('ST009', (SELECT Id FROM Parts WHERE PartNumber = 'BUGINGK'), 4,
 100, 0, 100, 100,
 250000, 300000, 25000000, 30000000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Bugi NGK', NOW(), NOW(), 0, 1),

-- Dây curoa - Tồn đầu kỳ: 30 cái
('ST010', (SELECT Id FROM Parts WHERE PartNumber = 'DAYCUROA'), 4,
 30, 0, 30, 30,
 200000, 250000, 6000000, 7500000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Dây curoa động cơ', NOW(), NOW(), 0, 1),

-- Lọc gió - Tồn đầu kỳ: 40 cái
('ST011', (SELECT Id FROM Parts WHERE PartNumber = 'LOCGIO'), 4,
 40, 0, 40, 40,
 180000, 220000, 7200000, 8800000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Lọc gió động cơ', NOW(), NOW(), 0, 1),

-- Dầu hộp số - Tồn đầu kỳ: 25 lít
('ST012', (SELECT Id FROM Parts WHERE PartNumber = 'DAUHOPSO'), 4,
 25, 0, 25, 25,
 300000, 350000, 7500000, 8750000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Dầu hộp số tự động', NOW(), NOW(), 0, 1);

-- =====================================================
-- 3. TẠO STOCK TRANSACTIONS CHO NHẬP TRONG KỲ
-- =====================================================

-- Nhập trong kỳ cho Ắc quy N70: 22 cái
INSERT INTO StockTransactions (
    TransactionNumber, PartId, TransactionType, 
    Quantity, QuantityBefore, QuantityAfter, StockAfter,
    UnitCost, UnitPrice, TotalCost, TotalAmount, TransactionDate,
    Notes, CreatedAt, UpdatedAt, IsDeleted, HasInvoice
) VALUES 
('ST013', (SELECT Id FROM Parts WHERE PartNumber = 'ACQUY_70'), 1,
 22, 10, 32, 32,
 1194340, 1300000, 26275480, 28600000, '2025-01-15 10:00:00',
 'Nhập kho - Ắc quy GS N70', NOW(), NOW(), 0, 1);

-- Nhập trong kỳ cho Dầu động cơ: 30 chai
INSERT INTO StockTransactions (
    TransactionNumber, PartId, TransactionType, 
    Quantity, QuantityBefore, QuantityAfter, StockAfter,
    UnitCost, UnitPrice, TotalCost, TotalAmount, TransactionDate,
    Notes, CreatedAt, UpdatedAt, IsDeleted, HasInvoice
) VALUES 
('ST014', (SELECT Id FROM Parts WHERE PartNumber = 'DBT10W40'), 1,
 30, 0, 30, 30,
 272727, 300000, 8181810, 9000000, '2025-01-20 14:00:00',
 'Nhập kho - Dầu ENEOS', NOW(), NOW(), 0, 1);

-- =====================================================
-- 4. TẠO STOCK TRANSACTIONS CHO XUẤT TRONG KỲ
-- =====================================================

-- Xuất trong kỳ cho Ắc quy N70: 9 cái
INSERT INTO StockTransactions (
    TransactionNumber, PartId, TransactionType, 
    Quantity, QuantityBefore, QuantityAfter, StockAfter,
    UnitCost, UnitPrice, TotalCost, TotalAmount, TransactionDate,
    Notes, CreatedAt, UpdatedAt, IsDeleted, HasInvoice
) VALUES 
('ST015', (SELECT Id FROM Parts WHERE PartNumber = 'ACQUY_70'), 2,
 9, 32, 23, 23,
 1194340, 1300000, 10749060, 11700000, '2025-01-25 16:00:00',
 'Xuất kho - Ắc quy GS N70', NOW(), NOW(), 0, 1);

-- Xuất trong kỳ cho Dầu động cơ: 6 chai
INSERT INTO StockTransactions (
    TransactionNumber, PartId, TransactionType, 
    Quantity, QuantityBefore, QuantityAfter, StockAfter,
    UnitCost, UnitPrice, TotalCost, TotalAmount, TransactionDate,
    Notes, CreatedAt, UpdatedAt, IsDeleted, HasInvoice
) VALUES 
('ST016', (SELECT Id FROM Parts WHERE PartNumber = 'DBT10W40'), 2,
 6, 30, 24, 24,
 272727, 300000, 1636362, 1800000, '2025-01-28 11:00:00',
 'Xuất kho - Dầu ENEOS', NOW(), NOW(), 0, 1);

-- Bật lại safe update mode
SET SQL_SAFE_UPDATES = 1;

-- =====================================================
-- 5. KIỂM TRA KẾT QUẢ
-- =====================================================

SELECT 
    p.PartNumber as 'Mã Hiệu',
    p.PartName as 'Tên VTTH',
    p.Unit as 'Đơn Vị',
    p.QuantityInStock as 'Tồn Cuối',
    p.MinimumStock as 'Tồn Tối Thiểu',
    p.SellPrice as 'Đơn Giá VNĐ',
    (p.QuantityInStock * p.SellPrice) as 'Tổng Giá Trị VNĐ'
FROM Parts p 
WHERE p.PartNumber IN (
    'ACQUY_70', 'BAQGN', 'BDZA', 'DBT10W40', 'MABOSCH', 
    'LOTP205', 'SONXE2K', 'DAUPHANH', 'BUGINGK', 
    'DAYCUROA', 'LOCGIO', 'DAUHOPSO'
)
ORDER BY p.PartNumber;

SELECT 
    'TỔNG CỘNG' as 'Mã Hiệu',
    COUNT(*) as 'Số Loại VTTH',
    SUM(p.QuantityInStock) as 'Tổng Số Lượng',
    SUM(p.QuantityInStock * p.SellPrice) as 'Tổng Giá Trị VNĐ'
FROM Parts p 
WHERE p.PartNumber IN (
    'ACQUY_70', 'BAQGN', 'BDZA', 'DBT10W40', 'MABOSCH', 
    'LOTP205', 'SONXE2K', 'DAUPHANH', 'BUGINGK', 
    'DAYCUROA', 'LOCGIO', 'DAUHOPSO'
);

COMMIT;
