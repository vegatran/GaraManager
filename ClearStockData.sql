-- =====================================================
-- SCRIPT CLEAR DỮ LIỆU TỒN KHO ĐÃ IMPORT
-- =====================================================

USE garamanagement;

-- Tắt safe update mode
SET SQL_SAFE_UPDATES = 0;

-- Xóa Stock Transactions trước (vì có foreign key)
DELETE FROM StockTransactions 
WHERE TransactionNumber IN (
    'ST001', 'ST002', 'ST003', 'ST004', 'ST005', 'ST006',
    'ST007', 'ST008', 'ST009', 'ST010', 'ST011', 'ST012',
    'ST013', 'ST014', 'ST015', 'ST016'
);

-- Xóa Parts
DELETE FROM Parts 
WHERE PartNumber IN (
    'ACQUY_70', 'BAQGN', 'BDZA', 'DBT10W40', 'MABOSCH', 
    'LOTP205', 'SONXE2K', 'DAUPHANH', 'BUGINGK', 
    'DAYCUROA', 'LOCGIO', 'DAUHOPSO'
);

-- Bật lại safe update mode
SET SQL_SAFE_UPDATES = 1;

-- Kiểm tra kết quả
SELECT 'CLEAR COMPLETED' as Status;
