-- ✅ Hướng dẫn: Làm thế nào để có danh sách JO chờ QC

/*
WORKFLOW ĐỂ CÓ JO CHỜ QC:

1. Tạo Service Order từ Quotation đã Approved
   - Vào "Báo Giá" → Duyệt một báo giá
   - Vào "Phiếu Sửa Chữa" → Tạo mới từ báo giá đã duyệt

2. Phân công KTV và giờ công
   - Xem chi tiết Service Order → Click "Phân công KTV"
   - Chọn KTV và giờ công cho từng item

3. KTV bắt đầu làm việc
   - Xem chi tiết Service Order → Tab "Chi Tiết Dịch Vụ"
   - Click "Bắt đầu" cho từng item

4. KTV hoàn thành từng item
   - Click "Hoàn thành" cho từng item sau khi làm xong

5. Hoàn thành kỹ thuật
   - Khi TẤT CẢ items đã Completed hoặc Cancelled
   - Button "Hoàn Thành Kỹ Thuật" sẽ xuất hiện trong View Order Modal
   - Click button này → Service Order chuyển sang "WaitingForQC"

6. Kiểm tra QC Management
   - Vào "Kiểm Tra QC" → Sẽ thấy JO chờ QC trong danh sách
*/

-- ✅ SCRIPT KIỂM TRA VÀ TẠO DỮ LIỆU DEMO:

-- Bước 1: Kiểm tra Service Orders hiện tại
SELECT 
    so.Id,
    so.OrderNumber,
    so.Status,
    COUNT(soi.Id) as TotalItems,
    SUM(CASE WHEN soi.Status = 'Completed' THEN 1 ELSE 0 END) as CompletedItems,
    SUM(CASE WHEN soi.Status = 'Cancelled' THEN 1 ELSE 0 END) as CancelledItems,
    SUM(CASE WHEN soi.Status NOT IN ('Completed', 'Cancelled') THEN 1 ELSE 0 END) as IncompleteItems
FROM ServiceOrders so
LEFT JOIN ServiceOrderItems soi ON soi.ServiceOrderId = so.Id AND soi.IsDeleted = 0
WHERE so.IsDeleted = 0
GROUP BY so.Id, so.OrderNumber, so.Status
ORDER BY so.Id DESC;

-- Bước 2: Nếu có Service Order với tất cả items Completed/Cancelled
-- Cập nhật status sang WaitingForQC (thay {ServiceOrderId} bằng ID thực tế)

-- UPDATE ServiceOrders 
-- SET Status = 'WaitingForQC',
--     CompletedDate = GETDATE(),
--     TotalActualHours = (
--         SELECT ISNULL(SUM(ActualHours), 0) 
--         FROM ServiceOrderItems 
--         WHERE ServiceOrderId = {ServiceOrderId} 
--           AND IsDeleted = 0
--           AND ActualHours IS NOT NULL
--     )
-- WHERE Id = {ServiceOrderId} 
--   AND IsDeleted = 0
--   AND Status IN ('Completed', 'InProgress');

-- Bước 3: Hoặc cập nhật tất cả items của một Service Order sang Completed
-- (Thay {ServiceOrderId} bằng ID thực tế)

-- UPDATE ServiceOrderItems 
-- SET Status = 'Completed',
--     CompletedTime = GETDATE(),
--     ActualHours = ISNULL(EstimatedHours, 0),
--     EndTime = GETDATE()
-- WHERE ServiceOrderId = {ServiceOrderId} 
--   AND IsDeleted = 0
--   AND Status != 'Cancelled';

-- Sau đó chạy lại Bước 2

-- Bước 4: Kiểm tra lại danh sách JO chờ QC
SELECT 
    so.Id,
    so.OrderNumber,
    c.Name as CustomerName,
    v.LicensePlate as VehiclePlate,
    so.CompletedDate,
    so.TotalActualHours,
    so.QCFailedCount,
    so.Status
FROM ServiceOrders so
LEFT JOIN Customers c ON c.Id = so.CustomerId
LEFT JOIN Vehicles v ON v.Id = so.VehicleId
WHERE so.IsDeleted = 0 
  AND so.Status = 'WaitingForQC'
ORDER BY so.CompletedDate DESC, so.OrderDate DESC;

