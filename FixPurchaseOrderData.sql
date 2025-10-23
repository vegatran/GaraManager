-- Tạo dữ liệu demo cho phiếu nhập hàng
-- Cần có ReferenceNumber và SupplierId để hiển thị đúng

-- Thêm một số phiếu nhập hàng mới
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
    QuantityAfter,
    IsDeleted,
    CreatedAt,
    CreatedBy
) VALUES 
-- Phiếu nhập hàng PN-2025-001
('STK-2025-001', 1, '2025-01-20 10:00:00', 1, 50, 350000, 450000, 17500000, 22500000, 1, 'PN-2025-001', 1, 'Nhập dầu nhớt từ nhà cung cấp', 1, 100, 50, 100, 0, NOW(), 'DemoData'),
('STK-2025-002', 1, '2025-01-20 10:00:00', 2, 30, 100000, 120000, 3000000, 3600000, 1, 'PN-2025-001', 1, 'Nhập lọc dầu từ nhà cung cấp', 1, 80, 50, 80, 0, NOW(), 'DemoData'),

-- Phiếu nhập hàng PN-2025-002  
('STK-2025-003', 1, '2025-01-19 14:30:00', 3, 20, 600000, 800000, 12000000, 16000000, 2, 'PN-2025-002', 1, 'Nhập má phanh từ nhà cung cấp', 2, 70, 50, 70, 0, NOW(), 'DemoData'),
('STK-2025-004', 1, '2025-01-19 14:30:00', 4, 15, 800000, 1000000, 12000000, 15000000, 2, 'PN-2025-002', 1, 'Nhập bugi từ nhà cung cấp', 2, 65, 50, 65, 0, NOW(), 'DemoData');

-- Cập nhật số lượng tồn kho cho các phụ tùng
UPDATE Parts SET QuantityInStock = 100 WHERE Id = 1;
UPDATE Parts SET QuantityInStock = 80 WHERE Id = 2; 
UPDATE Parts SET QuantityInStock = 70 WHERE Id = 3;
UPDATE Parts SET QuantityInStock = 65 WHERE Id = 4;
