-- =====================================================
-- SCRIPT IMPORT DỮ LIỆU TỒN KHO TỪ BÁO CÁO
-- =====================================================

USE garamanagement;

-- Tắt safe update mode để có thể UPDATE/DELETE không có WHERE
SET SQL_SAFE_UPDATES = 0;

-- =====================================================
-- 1. THÊM PARTS MỚI VÀO BẢNG Parts
-- =====================================================

-- Thêm các phụ tùng mới (chỉ thêm những chưa có)
INSERT INTO Parts (
    PartNumber, PartName, Description, 
    CostPrice, SellPrice, QuantityInStock, 
    Unit, Category, Brand, 
    IsActive, CreatedAt, UpdatedAt, IsDeleted,
    SourceType, InvoiceType, HasInvoice, 
    CanUseForCompany, CanUseForInsurance, CanUseForIndividual,
    `Condition`, WarrantyMonths, IsOEM
) VALUES 
-- Ắc quy
('ACQUY_70', 'BÌNH ẮC QUY GS N70', 'Ắc quy GS N70 cho xe hạng trung', 
 1194340, 1300000, 23, 
 'CÁI', 'Ắc quy', 'GS', 
 1, NOW(), NOW(), 0,
 'Purchase', 'Purchase', 1, 1, 1, 1, 'New', 12, 0),

-- Bạc đạn
('BAQGN', 'Bạc đạn ZA.60TB039B09', 'Bạc đạn chất lượng cao', 
 285000, 350000, 95, 
 'CÁI', 'Bạc đạn', 'ZA', 
 1, NOW(), NOW(), 0,
 'Purchase', 'Purchase', 1, 1, 1, 1, 'New', 24, 0),

-- Đĩa phanh
('BDZA', 'Đĩa phanh BOSCH', 'Đĩa phanh BOSCH chất lượng cao', 
 650000, 750000, 20, 
 'CÁI', 'Phanh', 'BOSCH', 
 1, NOW(), NOW(), 0,
 'Purchase', 'Purchase', 1, 1, 1, 1, 'New', 12, 0),

-- Dầu động cơ
('DBT10W40', 'Dầu bôi trơn ENEOS AL SNCF 10W40 4L *6', 'Dầu động cơ ENEOS 10W40', 
 272727, 300000, 24625, 
 'Chai', 'Dầu nhớt', 'ENEOS', 
 1, NOW(), NOW(), 0,
 'Purchase', 'Purchase', 1, 1, 1, 1, 'New', 12, 0),

-- Má phanh
('MABOSCH', 'Má phanh BOSCH', 'Má phanh BOSCH chất lượng cao', 
 350000, 400000, 25, 
 'Bộ', 'Phanh', 'BOSCH', 
 1, NOW(), NOW(), 0,
 'Purchase', 'Purchase', 1, 1, 1, 1, 'New', 12, 0),

-- Lốp xe
('LOTP205', 'Lốp Michelin 205/55R16', 'Lốp xe Michelin 205/55R16', 
 1000000, 1200000, 20, 
 'Chiếc', 'Lốp xe', 'Michelin', 
 1, NOW(), NOW(), 0,
 'Purchase', 'Purchase', 1, 1, 1, 1, 'New', 12, 0),

-- Sơn xe
('SONXE2K', 'Sơn xe 2K', 'Sơn xe 2K chất lượng cao', 
 1000000, 1200000, 10, 
 'Bộ', 'Sơn', 'Generic', 
 1, NOW(), NOW(), 0,
 'Purchase', 'Purchase', 1, 1, 1, 1, 'New', 6, 0),

-- Dầu phanh
('DAUPHANH', 'Dầu phanh DOT4', 'Dầu phanh DOT4 chất lượng cao', 
 150000, 180000, 50, 
 'Chai', 'Dầu phanh', 'Generic', 
 1, NOW(), NOW(), 0,
 'Purchase', 'Purchase', 1, 1, 1, 1, 'New', 12, 0),

-- Bugi
('BUGINGK', 'Bugi NGK', 'Bugi NGK chất lượng cao', 
 250000, 300000, 100, 
 'CÁI', 'Bugi', 'NGK', 
 1, NOW(), NOW(), 0,
 'Purchase', 'Purchase', 1, 1, 1, 1, 'New', 12, 0),

-- Dây curoa
('DAYCUROA', 'Dây curoa động cơ', 'Dây curoa động cơ chất lượng cao', 
 200000, 250000, 30, 
 'CÁI', 'Dây curoa', 'Generic', 
 1, NOW(), NOW(), 0,
 'Purchase', 'Purchase', 1, 1, 1, 1, 'New', 12, 0),

-- Lọc gió
('LOCGIO', 'Lọc gió động cơ', 'Lọc gió động cơ chất lượng cao', 
 180000, 220000, 40, 
 'CÁI', 'Lọc', 'Generic', 
 1, NOW(), NOW(), 0,
 'Purchase', 'Purchase', 1, 1, 1, 1, 'New', 6, 0),

-- Dầu hộp số
('DAUHOPSO', 'Dầu hộp số tự động', 'Dầu hộp số tự động chất lượng cao', 
 300000, 350000, 25, 
 'Lít', 'Dầu hộp số', 'Generic', 
 1, NOW(), NOW(), 0,
 'Purchase', 'Purchase', 1, 1, 1, 1, 'New', 12, 0);

-- =====================================================
-- 2. TẠO STOCK TRANSACTIONS CHO TỒN ĐẦU KỲ
-- =====================================================

-- Tạo các giao dịch tồn đầu kỳ
INSERT INTO StockTransactions (
    TransactionNumber, PartId, TransactionType, 
    Quantity, QuantityBefore, QuantityAfter, StockAfter,
    UnitCost, UnitPrice, TotalCost, TotalAmount, TransactionDate,
    Notes, CreatedAt, UpdatedAt, IsDeleted
) VALUES 
-- Ắc quy N70 - Tồn đầu kỳ: 10 cái
('ST001', (SELECT Id FROM Parts WHERE PartNumber = 'ACQUY_70'), 4, -- TonDauKy = 4
 10, 0, 10, 10,
 1194340, 1300000, 11943400, 13000000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Ắc quy GS N70', NOW(), NOW(), 0),

-- Bạc đạn - Tồn đầu kỳ: 95 cái  
('ST002', (SELECT Id FROM Parts WHERE PartNumber = 'BAQGN'), 'TonDauKy',
 95, 0, 95, 285000, 27075000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Bạc đạn ZA', NOW(), NOW(), 0),

-- Đĩa phanh - Tồn đầu kỳ: 20 cái
('ST003', (SELECT Id FROM Parts WHERE PartNumber = 'BDZA'), 'TonDauKy',
 20, 0, 20, 650000, 13000000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Đĩa phanh BOSCH', NOW(), NOW(), 0),

-- Dầu động cơ - Tồn đầu kỳ: 0 chai (nhập 30, xuất 5.275, còn 24.625)
('ST004', (SELECT Id FROM Parts WHERE PartNumber = 'DBT10W40'), 'TonDauKy',
 0, 0, 0, 272727, 0, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Dầu ENEOS', NOW(), NOW(), 0),

-- Má phanh - Tồn đầu kỳ: 25 bộ
('ST005', (SELECT Id FROM Parts WHERE PartNumber = 'MABOSCH'), 'TonDauKy',
 25, 0, 25, 350000, 8750000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Má phanh BOSCH', NOW(), NOW(), 0),

-- Lốp xe - Tồn đầu kỳ: 20 chiếc
('ST006', (SELECT Id FROM Parts WHERE PartNumber = 'LOTP205'), 'TonDauKy',
 20, 0, 20, 1000000, 20000000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Lốp Michelin', NOW(), NOW(), 0),

-- Sơn xe - Tồn đầu kỳ: 10 bộ
('ST007', (SELECT Id FROM Parts WHERE PartNumber = 'SONXE2K'), 'TonDauKy',
 10, 0, 10, 1000000, 10000000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Sơn xe 2K', NOW(), NOW(), 0),

-- Dầu phanh - Tồn đầu kỳ: 50 chai
('ST008', (SELECT Id FROM Parts WHERE PartNumber = 'DAUPHANH'), 'TonDauKy',
 50, 0, 50, 150000, 7500000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Dầu phanh DOT4', NOW(), NOW(), 0),

-- Bugi - Tồn đầu kỳ: 100 cái
('ST009', (SELECT Id FROM Parts WHERE PartNumber = 'BUGINGK'), 'TonDauKy',
 100, 0, 100, 250000, 25000000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Bugi NGK', NOW(), NOW(), 0),

-- Dây curoa - Tồn đầu kỳ: 30 cái
('ST010', (SELECT Id FROM Parts WHERE PartNumber = 'DAYCUROA'), 'TonDauKy',
 30, 0, 30, 200000, 6000000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Dây curoa động cơ', NOW(), NOW(), 0),

-- Lọc gió - Tồn đầu kỳ: 40 cái
('ST011', (SELECT Id FROM Parts WHERE PartNumber = 'LOCGIO'), 'TonDauKy',
 40, 0, 40, 180000, 7200000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Lọc gió động cơ', NOW(), NOW(), 0),

-- Dầu hộp số - Tồn đầu kỳ: 25 lít
('ST012', (SELECT Id FROM Parts WHERE PartNumber = 'DAUHOPSO'), 'TonDauKy',
 25, 0, 25, 300000, 7500000, '2025-01-01 08:00:00',
 'Tồn đầu kỳ - Dầu hộp số tự động', NOW(), NOW(), 0);

-- =====================================================
-- 3. TẠO STOCK TRANSACTIONS CHO NHẬP TRONG KỲ
-- =====================================================

-- Nhập trong kỳ cho Ắc quy N70: 22 cái
INSERT INTO StockTransactions (
    TransactionNumber, PartId, TransactionType, 
    Quantity, QuantityBefore, QuantityAfter,
    UnitPrice, TotalAmount, TransactionDate,
    Notes, CreatedAt, UpdatedAt, IsDeleted
) VALUES 
('ST013', (SELECT Id FROM Parts WHERE PartNumber = 'ACQUY_70'), 'NhapKho',
 22, 10, 32, 1194340, 26275480, '2025-01-15 10:00:00',
 'Nhập kho - Ắc quy GS N70', NOW(), NOW(), 0);

-- Nhập trong kỳ cho Dầu động cơ: 30 chai
INSERT INTO StockTransactions (
    TransactionNumber, PartId, TransactionType, 
    Quantity, QuantityBefore, QuantityAfter,
    UnitPrice, TotalAmount, TransactionDate,
    Notes, CreatedAt, UpdatedAt, IsDeleted
) VALUES 
('ST014', (SELECT Id FROM Parts WHERE PartNumber = 'DBT10W40'), 'NhapKho',
 30, 0, 30, 272727, 8181810, '2025-01-20 14:00:00',
 'Nhập kho - Dầu ENEOS', NOW(), NOW(), 0);

-- =====================================================
-- 4. TẠO STOCK TRANSACTIONS CHO XUẤT TRONG KỲ
-- =====================================================

-- Xuất trong kỳ cho Ắc quy N70: 9 cái
INSERT INTO StockTransactions (
    TransactionNumber, PartId, TransactionType, 
    Quantity, QuantityBefore, QuantityAfter,
    UnitPrice, TotalAmount, TransactionDate,
    Notes, CreatedAt, UpdatedAt, IsDeleted
) VALUES 
('ST015', (SELECT Id FROM Parts WHERE PartNumber = 'ACQUY_70'), 'XuatKho',
 9, 32, 23, 1194340, 10749060, '2025-01-25 16:00:00',
 'Xuất kho - Ắc quy GS N70', NOW(), NOW(), 0);

-- Xuất trong kỳ cho Dầu động cơ: 5.275 chai
INSERT INTO StockTransactions (
    TransactionNumber, PartId, TransactionType, 
    Quantity, QuantityBefore, QuantityAfter,
    UnitPrice, TotalAmount, TransactionDate,
    Notes, CreatedAt, UpdatedAt, IsDeleted
) VALUES 
('ST016', (SELECT Id FROM Parts WHERE PartNumber = 'DBT10W40'), 'XuatKho',
 5.275, 30, 24.625, 272727, 1465908, '2025-01-28 11:00:00',
 'Xuất kho - Dầu ENEOS', NOW(), NOW(), 0);

-- =====================================================
-- 5. CẬP NHẬT LẠI QUANTITYIN_STOCK CHO TẤT CẢ PARTS
-- =====================================================

UPDATE Parts SET QuantityInStock = 23 WHERE PartNumber = 'ACQUY_70';
UPDATE Parts SET QuantityInStock = 95 WHERE PartNumber = 'BAQGN';
UPDATE Parts SET QuantityInStock = 20 WHERE PartNumber = 'BDZA';
UPDATE Parts SET QuantityInStock = 24.625 WHERE PartNumber = 'DBT10W40';
UPDATE Parts SET QuantityInStock = 25 WHERE PartNumber = 'MABOSCH';
UPDATE Parts SET QuantityInStock = 20 WHERE PartNumber = 'LOTP205';
UPDATE Parts SET QuantityInStock = 10 WHERE PartNumber = 'SONXE2K';
UPDATE Parts SET QuantityInStock = 50 WHERE PartNumber = 'DAUPHANH';
UPDATE Parts SET QuantityInStock = 100 WHERE PartNumber = 'BUGINGK';
UPDATE Parts SET QuantityInStock = 30 WHERE PartNumber = 'DAYCUROA';
UPDATE Parts SET QuantityInStock = 40 WHERE PartNumber = 'LOCGIO';
UPDATE Parts SET QuantityInStock = 25 WHERE PartNumber = 'DAUHOPSO';

-- Bật lại safe update mode
SET SQL_SAFE_UPDATES = 1;

-- =====================================================
-- 6. KIỂM TRA KẾT QUẢ
-- =====================================================

SELECT 
    p.PartNumber as 'Mã Hiệu',
    p.PartName as 'Tên VTTH',
    p.Unit as 'Đơn Vị',
    p.QuantityInStock as 'Tồn Cuối',
    p.SellPrice as 'Đơn Giá VNĐ'
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
