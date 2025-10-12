# HƯỚNG DẪN DATA DEMO - HỆ THỐNG QUẢN LÝ GARAGE Ô TÔ

## 📋 MỤC LỤC
1. [Giới thiệu Data Demo](#giới-thiệu-data-demo)
2. [Cài đặt Data Demo](#cài-đặt-data-demo)
3. [Quy trình Demo hoàn chỉnh](#quy-trình-demo-hoàn-chỉnh)
4. [Các tình huống Demo](#các-tình-huống-demo)
5. [Kết quả mong đợi](#kết-quả-mong-đợi)

---

## 🎯 GIỚI THIỆU DATA DEMO

### **Mục đích**
Data Demo được thiết kế để:
- ✅ **Làm quen với hệ thống**: Hiểu cách sử dụng các tính năng
- ✅ **Test đầy đủ quy trình**: Từ khách hàng đến thanh toán
- ✅ **Demo cho khách hàng**: Thuyết trình tính năng
- ✅ **Training nhân viên**: Hướng dẫn sử dụng

### **Dữ liệu Demo bao gồm**
- 👥 **10 khách hàng** với đầy đủ thông tin
- 🚗 **15 xe** các hãng khác nhau
- 🔧 **200+ phụ tùng** phân loại theo nhóm
- ⚙️ **50+ dịch vụ** sửa chữa
- 👷 **20+ công lao động** chi tiết
- 💼 **25 đơn hàng** hoàn chỉnh
- 💰 **Giao dịch tài chính** đầy đủ
- 📦 **Giao dịch kho** nhập xuất

---

## 🚀 CÀI ĐẶT DATA DEMO

### **Bước 1: Backup dữ liệu hiện tại**
```sql
-- Backup database hiện tại
mysqldump -u usergara -p GaraManagement > backup_before_demo.sql
```

### **Bước 2: Reset database**
```sql
-- Xóa tất cả dữ liệu (cẩn thận!)
TRUNCATE TABLE PaymentTransactions;
TRUNCATE TABLE ServiceOrderLabors;
TRUNCATE TABLE ServiceOrderParts;
TRUNCATE TABLE ServiceOrderItems;
TRUNCATE TABLE ServiceOrders;
TRUNCATE TABLE StockTransactions;
TRUNCATE TABLE FinancialTransactions;
TRUNCATE TABLE Parts;
TRUNCATE TABLE Services;
TRUNCATE TABLE Vehicles;
TRUNCATE TABLE Customers;
-- ... (các bảng khác)
```

### **Bước 3: Import Data Demo**
```sql
-- Import data demo
source demo_data.sql
```

### **Bước 4: Verify Data**
```sql
-- Kiểm tra dữ liệu đã import
SELECT COUNT(*) as CustomerCount FROM Customers;
SELECT COUNT(*) as VehicleCount FROM Vehicles;
SELECT COUNT(*) as PartCount FROM Parts;
SELECT COUNT(*) as ServiceCount FROM Services;
SELECT COUNT(*) as OrderCount FROM ServiceOrders;
```

---

## 🔄 QUY TRÌNH DEMO HOÀN CHỈNH

### **Scenario 1: Khách hàng mới - Xe Mercedes C-Class**

#### **Bước 1: Đăng ký khách hàng**
```
┌─────────────────────────────────────────────────────────┐
│              THÔNG TIN KHÁCH HÀNG DEMO                  │
├─────────────────────────────────────────────────────────┤
│ Họ tên: Nguyễn Minh Tuấn                               │
│ SĐT: 0901234567                                        │
│ Email: tuan.nguyen@email.com                           │
│ Địa chỉ: 123 Nguyễn Huệ, Quận 1, TP.HCM               │
│ Loại: Cá nhân                                          │
│ Ghi chú: Khách VIP, thường xuyên sửa chữa             │
└─────────────────────────────────────────────────────────┘
```

#### **Bước 2: Đăng ký xe**
```
┌─────────────────────────────────────────────────────────┐
│              THÔNG TIN XE DEMO                          │
├─────────────────────────────────────────────────────────┤
│ Biển số: 30A-12345                                     │
│ Hãng xe: Mercedes-Benz                                  │
│ Model: C-Class (W205)                                   │
│ Năm sản xuất: 2020                                      │
│ Màu sắc: Đen                                            │
│ Số khung: WDD2050461A123456                            │
│ Bảo hiểm: Bảo Việt - BV123456789                       │
└─────────────────────────────────────────────────────────┘
```

#### **Bước 3: Tạo đơn hàng sửa chữa**
```
┌─────────────────────────────────────────────────────────┐
│              ĐƠN HÀNG DEMO                              │
├─────────────────────────────────────────────────────────┤
│ Số đơn: SO-2024-001                                    │
│ Khách hàng: Nguyễn Minh Tuấn                           │
│ Xe: 30A-12345 - Mercedes C-Class 2020                   │
│ Ngày: 15/01/2024                                       │
│ Tình trạng: Đèn pha bị mờ, cần thay mới                │
│                                                         │
│ DỊCH VỤ:                                              │
│ • Thay đèn pha Mercedes C-Class W205                    │
│                                                         │
│ PHỤ TÙNG:                                             │
│ • Đèn pha Mercedes C-Class W205 (OEM)                   │
│ • Bóng đèn H7 55W                                       │
│                                                         │
│ CÔNG LAO ĐỘNG:                                        │
│ • Công tháo đèn pha (1.5 giờ)                          │
│ • Công lắp đèn pha (1.0 giờ)                           │
│ • Công điều chỉnh góc chiếu sáng (0.5 giờ)             │
│                                                         │
│ TỔNG CỘNG: 2,850,000 VNĐ                               │
└─────────────────────────────────────────────────────────┘
```

#### **Bước 4: Nhập kho phụ tùng**
```
┌─────────────────────────────────────────────────────────┐
│              GIAO DỊCH NHẬP KHO DEMO                   │
├─────────────────────────────────────────────────────────┤
│ Số giao dịch: STK-2024-001                             │
│ Loại: Nhập kho                                         │
│ Nhà cung cấp: Mercedes-Benz Vietnam                    │
│ Số hóa đơn: MB-2024-001                                │
│ Ngày nhập: 10/01/2024                                  │
│                                                         │
│ CHI TIẾT NHẬP KHO:                                    │
│ • Đèn pha Mercedes C-Class W205: 5 cái x 2,500,000     │
│ • Bóng đèn H7 55W: 20 cái x 150,000                    │
│ • Bộ lọc gió động cơ: 10 cái x 200,000                 │
│ • Dầu động cơ 5W-30: 50 lít x 120,000                  │
│                                                         │
│ TỔNG NHẬP: 15,800,000 VNĐ                              │
└─────────────────────────────────────────────────────────┘
```

#### **Bước 5: Giao dịch tài chính**
```
┌─────────────────────────────────────────────────────────┐
│              GIAO DỊCH TÀI CHÍNH DEMO                  │
├─────────────────────────────────────────────────────────┤
│ THU NHẬP:                                              │
│ • Doanh thu dịch vụ: 2,850,000 VNĐ                     │
│ • Thu từ khách hàng: 2,850,000 VNĐ                     │
│                                                         │
│ CHI PHÍ:                                               │
│ • Chi phí phụ tùng: 2,650,000 VNĐ                      │
│ • Chi phí công lao động: 150,000 VNĐ                   │
│ • Chi phí vận chuyển: 50,000 VNĐ                       │
│                                                         │
│ LỢI NHUẬN: 0 VNĐ (Break-even)                          │
└─────────────────────────────────────────────────────────┘
```

### **Scenario 2: Bảo dưỡng định kỳ - Xe Toyota Camry**

#### **Bước 1: Kiểm tra xe**
```
┌─────────────────────────────────────────────────────────┐
│              KIỂM TRA XE DEMO                           │
├─────────────────────────────────────────────────────────┤
│ Khách hàng: Lê Thị Hương                               │
│ Xe: 30B-67890 - Toyota Camry XV70 2019                 │
│ Km hiện tại: 45,000 km                                 │
│ Km bảo dưỡng cuối: 40,000 km                           │
│                                                         │
│ KIỂM TRA PHÁT HIỆN:                                   │
│ • Dầu động cơ cần thay (5,000 km)                      │
│ • Lọc gió động cơ bẩn                                  │
│ • Lọc gió điều hòa cần thay                            │
│ • Phanh trước cần kiểm tra                             │
│ • Lốp xe còn tốt (60% độ mòn)                          │
└─────────────────────────────────────────────────────────┘
```

#### **Bước 2: Tạo báo giá bảo dưỡng**
```
┌─────────────────────────────────────────────────────────┐
│              BÁO GIÁ BẢO DƯỠNG DEMO                    │
├─────────────────────────────────────────────────────────┤
│ Số báo giá: BG-2024-002                                │
│ Khách hàng: Lê Thị Hương                               │
│ Xe: 30B-67890 - Toyota Camry XV70                      │
│ Ngày: 16/01/2024                                       │
│                                                         │
│ DỊCH VỤ BẢO DƯỠNG:                                    │
│ • Thay dầu động cơ: 200,000 VNĐ                        │
│ • Thay lọc dầu: 50,000 VNĐ                             │
│ • Thay lọc gió động cơ: 150,000 VNĐ                    │
│ • Thay lọc gió điều hòa: 100,000 VNĐ                   │
│ • Kiểm tra phanh: 100,000 VNĐ                          │
│                                                         │
│ PHỤ TÙNG:                                             │
│ • Dầu động cơ 5W-30 (4 lít): 480,000 VNĐ               │
│ • Lọc dầu Toyota: 80,000 VNĐ                           │
│ • Lọc gió động cơ: 120,000 VNĐ                         │
│ • Lọc gió điều hòa: 80,000 VNĐ                         │
│                                                         │
│ TỔNG CỘNG: 1,360,000 VNĐ                               │
└─────────────────────────────────────────────────────────┘
```

#### **Bước 3: Thực hiện bảo dưỡng**
```
┌─────────────────────────────────────────────────────────┐
│              THỰC HIỆN BẢO DƯỠNG DEMO                  │
├─────────────────────────────────────────────────────────┤
│ Thợ thực hiện: Nguyễn Văn Minh                         │
│ Thời gian bắt đầu: 08:00 - 16/01/2024                 │
│ Thời gian hoàn thành: 10:30 - 16/01/2024               │
│                                                         │
│ CÔNG VIỆC THỰC HIỆN:                                  │
│ • Thay dầu động cơ (1.0 giờ)                           │
│ • Thay lọc dầu (0.5 giờ)                               │
│ • Thay lọc gió động cơ (0.5 giờ)                       │
│ • Thay lọc gió điều hòa (0.5 giờ)                      │
│ • Kiểm tra phanh (1.0 giờ)                             │
│                                                         │
│ TỔNG THỜI GIAN: 3.5 giờ                                │
│ TỔNG TIỀN CÔNG: 175,000 VNĐ                            │
└─────────────────────────────────────────────────────────┘
```

---

## 📊 CÁC TÌNH HUỐNG DEMO

### **Tình huống 1: Quản lý tồn kho**
```
┌─────────────────────────────────────────────────────────┐
│              CẢNH BÁO TỒN KHO DEMO                     │
├─────────────────────────────────────────────────────────┤
│ PHỤ TÙNG TỒN KHO THẤP:                                │
│ • Dầu phanh DOT4: 2 lít (Cảnh báo: < 5 lít)           │
│ • Bugi đánh lửa: 5 bộ (Cảnh báo: < 10 bộ)             │
│ • Lọc nhiên liệu: 3 cái (Cảnh báo: < 5 cái)           │
│                                                         │
│ HÀNH ĐỘNG:                                             │
│ • Tạo đơn đặt hàng tự động                             │
│ • Gửi email nhắc nhở nhà cung cấp                      │
│ • Cập nhật lịch nhập hàng                              │
└─────────────────────────────────────────────────────────┘
```

### **Tình huống 2: Báo cáo doanh thu**
```
┌─────────────────────────────────────────────────────────┐
│              BÁO CÁO DOANH THU DEMO                    │
├─────────────────────────────────────────────────────────┤
│ BÁO CÁO THÁNG 01/2024:                                 │
│                                                         │
│ DOANH THU:                                             │
│ • Dịch vụ sửa chữa: 45,500,000 VNĐ                     │
│ • Bán phụ tùng: 23,200,000 VNĐ                         │
│ • Công lao động: 12,800,000 VNĐ                        │
│ • Tổng doanh thu: 81,500,000 VNĐ                       │
│                                                         │
│ CHI PHÍ:                                               │
│ • Chi phí phụ tùng: 52,300,000 VNĐ                     │
│ • Chi phí nhân công: 15,600,000 VNĐ                    │
│ • Chi phí vận chuyển: 2,400,000 VNĐ                    │
│ • Tổng chi phí: 70,300,000 VNĐ                         │
│                                                         │
│ LỢI NHUẬN: 11,200,000 VNĐ (13.7%)                     │
└─────────────────────────────────────────────────────────┘
```

### **Tình huống 3: Quản lý công nợ**
```
┌─────────────────────────────────────────────────────────┐
│              QUẢN LÝ CÔNG NỢ DEMO                      │
├─────────────────────────────────────────────────────────┤
│ KHÁCH HÀNG CÓ CÔNG NỢ:                                │
│                                                         │
│ • Trần Văn Đức - 30C-11111: 3,500,000 VNĐ             │
│   Đơn hàng: SO-2024-015 (Sơn toàn thân)               │
│   Ngày tạo: 10/01/2024                                │
│   Hạn thanh toán: 25/01/2024                          │
│                                                         │
│ • Phạm Thị Lan - 30D-22222: 2,200,000 VNĐ             │
│   Đơn hàng: SO-2024-018 (Thay hộp số)                 │
│   Ngày tạo: 12/01/2024                                │
│   Hạn thanh toán: 27/01/2024                          │
│                                                         │
│ TỔNG CÔNG NỢ: 5,700,000 VNĐ                           │
│                                                         │
│ HÀNH ĐỘNG:                                             │
│ • Gửi email nhắc nợ                                    │
│ • Gọi điện thoại nhắc nhở                              │
│ • Cập nhật trạng thái theo dõi                         │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 KẾT QUẢ MONG ĐỢI

### **Sau khi hoàn thành Demo, người dùng sẽ:**

#### **✅ Hiểu được quy trình hoàn chỉnh:**
1. **Đăng ký khách hàng** → **Đăng ký xe** → **Tạo đơn hàng**
2. **Quản lý phụ tùng** → **Nhập kho** → **Xuất kho**
3. **Thực hiện dịch vụ** → **Tính công lao động** → **Thanh toán**
4. **Tạo báo cáo** → **Quản lý tài chính** → **Theo dõi công nợ**

#### **✅ Thành thạo các tính năng:**
- Tìm kiếm và lọc dữ liệu
- Tạo và chỉnh sửa thông tin
- Quản lý tồn kho và cảnh báo
- Tạo báo cáo và xuất dữ liệu
- Quản lý quyền truy cập

#### **✅ Đánh giá được hiệu quả:**
- Tiết kiệm thời gian quản lý
- Giảm thiểu sai sót
- Tăng tính minh bạch
- Cải thiện dịch vụ khách hàng

### **Dữ liệu Demo được thiết kế để:**
- **Thực tế**: Phản ánh đúng tình huống thực tế
- **Đầy đủ**: Bao phủ tất cả tính năng hệ thống
- **Dễ hiểu**: Logic rõ ràng, dễ theo dõi
- **Mở rộng**: Có thể thêm dữ liệu mới

---

## 📝 GHI CHÚ QUAN TRỌNG

### **⚠️ Lưu ý khi sử dụng Data Demo:**
1. **Backup dữ liệu** trước khi import demo
2. **Test trên môi trường** development trước
3. **Không sử dụng** trên production
4. **Xóa dữ liệu demo** sau khi hoàn thành test

### **🔄 Reset Data Demo:**
```sql
-- Script reset data demo
source reset_demo_data.sql
```

### **📞 Hỗ trợ:**
- **Technical Support**: support@garamanager.com
- **Documentation**: docs.garamanager.com
- **Training**: training@garamanager.com

---

*Data Demo được cập nhật thường xuyên theo phiên bản hệ thống. Phiên bản hiện tại: v2.0.0*
