# 📋 HƯỚNG DẪN TẠO PHIẾU NHẬP HÀNG
## Garage Management System

---

## 🎯 TỔNG QUAN

Phiếu nhập hàng (Purchase Order) là tài liệu quan trọng trong quy trình quản lý kho, cho phép bạn:
- Tạo đơn đặt hàng với nhà cung cấp
- Quản lý chi tiết từng mặt hàng cần nhập
- Theo dõi trạng thái đơn hàng
- Tự động cập nhật tồn kho khi nhận hàng

---

## 📋 YÊU CẦU TRƯỚC KHI BẮT ĐẦU

### ✅ Kiểm tra dữ liệu cần thiết:
1. **Nhà cung cấp** đã được tạo trong hệ thống
2. **Phụ tùng** cần nhập đã có trong danh mục
3. **Thông tin giá** của phụ tùng từ nhà cung cấp
4. **Số lượng** cần đặt hàng

---

## 🚀 HƯỚNG DẪN STEP BY STEP

### **BƯỚC 1: Truy cập trang Quản lý Phiếu Nhập**

1. Đăng nhập vào hệ thống Garage Management System
2. Trong **Sidebar Menu**, click vào **"Quản Lý Kho"**
3. Chọn **"Phiếu Nhập Hàng"** từ menu con
4. Hệ thống sẽ hiển thị danh sách các phiếu nhập hiện có

### **BƯỚC 2: Tạo phiếu nhập mới**

1. Click vào nút **"Tạo Phiếu Nhập Mới"** (màu xanh, góc trên bên phải)
2. Modal **"Tạo Phiếu Nhập Mới"** sẽ xuất hiện

### **BƯỚC 3: Điền thông tin cơ bản**

#### **3.1 Thông tin nhà cung cấp:**
- **Nhà Cung Cấp**: Chọn từ dropdown (bắt buộc)
- **Ngày Đặt Hàng**: Tự động set ngày hiện tại
- **Ghi Chú**: Nhập ghi chú nếu cần

#### **3.2 Thông tin phiếu nhập:**
- **Số Phiếu**: Tự động tạo (format: PO-YYYYMMDD-XXX)
- **Trạng Thái**: Mặc định là "Đang Chờ"
- **Thuế VAT**: Chọn tỷ lệ thuế (0%, 8%, 10%) - mặc định là 8%

### **BƯỚC 4: Thêm chi tiết phụ tùng**

#### **4.1 Thêm phụ tùng vào phiếu:**
1. Click vào nút **"Thêm Phụ Tùng"**
2. Trong modal **"Chọn Phụ Tùng"**:
   - **Tìm kiếm phụ tùng**: Gõ tên hoặc mã phụ tùng
   - **Chọn từ danh sách**: Click vào phụ tùng cần thiết
   - **Giá tự động**: Hệ thống sẽ tự động điền giá từ nhà cung cấp

#### **4.2 Điều chỉnh thông tin phụ tùng:**
- **Số Lượng**: Nhập số lượng cần đặt
- **Giá Đơn Vị**: Kiểm tra và điều chỉnh nếu cần
- **Thành Tiền**: Tự động tính toán (Số lượng × Giá đơn vị)
- **Ghi Chú**: Thêm ghi chú cho từng phụ tùng

#### **4.3 Thêm nhiều phụ tùng:**
- Lặp lại **Bước 4.1** và **4.2** cho từng phụ tùng khác
- Danh sách phụ tùng sẽ hiển thị trong bảng

### **BƯỚC 5: Kiểm tra và tính toán tổng**

#### **5.1 Xem lại danh sách:**
- Kiểm tra tất cả phụ tùng đã thêm
- Xác nhận số lượng và giá cả
- **Tạm Tính**: Tổng tiền hàng (chưa VAT)
- **Thuế VAT**: Tiền thuế theo tỷ lệ đã chọn
- **Tổng Cộng**: Tổng tiền cuối cùng (bao gồm VAT)

#### **5.2 Điều chỉnh nếu cần:**
- **Sửa**: Click vào icon ✏️ để chỉnh sửa
- **Xóa**: Click vào icon 🗑️ để xóa phụ tùng
- **Thêm mới**: Tiếp tục thêm phụ tùng khác

### **BƯỚC 6: Lưu phiếu nhập**

1. Click nút **"Lưu Phiếu Nhập"**
2. Hệ thống sẽ:
   - Validate dữ liệu
   - Tạo phiếu nhập với trạng thái "Đang Chờ"
   - Hiển thị thông báo thành công
3. Modal sẽ đóng và quay về danh sách

### **BƯỚC 7: Theo dõi trạng thái**

#### **7.1 Các trạng thái phiếu nhập:**
- **🟡 Đang Chờ**: Phiếu mới tạo, chưa gửi cho nhà cung cấp
- **🔵 Đã Gửi**: Đã gửi cho nhà cung cấp
- **🟢 Đã Nhận**: Đã nhận hàng từ nhà cung cấp
- **🔴 Đã Hủy**: Phiếu bị hủy

#### **7.2 Thao tác theo trạng thái:**
- **Đang Chờ**: Có thể chỉnh sửa, gửi, hoặc hủy
- **Đã Gửi**: Có thể nhận hàng hoặc hủy
- **Đã Nhận**: Chỉ có thể xem chi tiết

---

## 🔧 CÁC THAO TÁC BỔ SUNG

### **📤 Gửi phiếu cho nhà cung cấp:**
1. Trong danh sách, click **"Gửi"** (nút màu xanh)
2. Xác nhận gửi phiếu
3. Trạng thái chuyển thành "Đã Gửi"

### **📥 Nhận hàng:**
1. Khi nhận hàng từ nhà cung cấp, click **"Nhận Hàng"**
2. Modal **"Xác Nhận Nhận Hàng"** xuất hiện
3. Kiểm tra số lượng thực tế nhận được
4. Điều chỉnh nếu có chênh lệch
5. Click **"Xác Nhận Nhận Hàng"**
6. **Hệ thống tự động thực hiện:**

   #### ✅ **1. Stock Transaction (Phiếu Nhập Kho)**
   - Tạo **phiếu nhập kho** cho MỖI phụ tùng
   - Transaction Number: `STK-YYYYMMDD-XXXX`
   - Type: `NhapKho`
   - Tự động cập nhật `QuantityInStock` trong Parts table
   - Lưu thông tin: Supplier, RelatedEntity, Notes
   
   #### ✅ **2. Financial Transaction (Phiếu Chi)**
   - Tạo **phiếu chi** cho kế toán
   - Transaction Number: `FIN-YYYYMMDD-XXXX`
   - Type: `Expense`
   - Category: `Parts Purchase`
   - SubCategory: `Purchase Order`
   - Amount: Tổng tiền PO (bao gồm VAT)
   - Status: `Pending` (chờ thanh toán thực tế)
   - Reference: Số phiếu nhập hàng
   
   #### ✅ **3. PartSupplier Update**
   - Cập nhật `LastOrderDate`: Ngày mới nhất đặt hàng
   - Cập nhật `LastCostPrice`: Giá mới nhất của phụ tùng
   
   #### ✅ **4. Status Update**
   - Chuyển trạng thái PO thành "Đã Nhận"
   - Hiển thị ngày nhận hàng thực tế

### **✏️ Chỉnh sửa phiếu:**
1. Click **"Chỉnh Sửa"** (icon ✏️)
2. Modal chỉnh sửa sẽ mở
3. Thực hiện các thay đổi cần thiết
4. Click **"Cập Nhật"** để lưu

### **👁️ Xem chi tiết:**
1. Click **"Xem Chi Tiết"** (icon 👁️)
2. Modal chi tiết hiển thị đầy đủ thông tin
3. Có thể in phiếu nhập từ đây

---

## 💰 CÁCH TÍNH THUẾ VAT

### **📋 Công thức tính:**
```
Tạm Tính = Σ(Số Lượng × Đơn Giá)
Thuế VAT = Tạm Tính × (Tỷ Lệ VAT / 100)
Tổng Cộng = Tạm Tính + Thuế VAT
```

### **📊 Ví dụ cụ thể:**
- **Phụ tùng A**: 2 cái × 100,000 VNĐ = 200,000 VNĐ
- **Phụ tùng B**: 1 cái × 500,000 VNĐ = 500,000 VNĐ
- **Tạm Tính**: 200,000 + 500,000 = 700,000 VNĐ
- **Thuế VAT (8%)**: 700,000 × 8% = 56,000 VNĐ
- **Tổng Cộng**: 700,000 + 56,000 = 756,000 VNĐ

### **⚙️ Các tỷ lệ VAT phổ biến:**
- **0%**: Không thuế (hàng xuất khẩu, dịch vụ không chịu thuế)
- **8%**: Thuế VAT giảm (một số mặt hàng thiết yếu)
- **10%**: Thuế VAT chuẩn (hầu hết hàng hóa, dịch vụ)

---

## 📊 TÍNH NĂNG NÂNG CAO

### **🔍 Tìm kiếm và lọc:**
- **Tìm kiếm**: Gõ từ khóa vào ô search
- **Lọc theo trạng thái**: Chọn trạng thái từ dropdown
- **Lọc theo nhà cung cấp**: Chọn nhà cung cấp cụ thể
- **Lọc theo ngày**: Chọn khoảng thời gian

### **📈 Báo cáo:**
- **Tổng quan**: Số lượng phiếu theo trạng thái
- **Chi tiết**: Danh sách đầy đủ với phân trang
- **Xuất Excel**: Export dữ liệu ra file Excel

---

## ⚠️ LƯU Ý QUAN TRỌNG

### **🚨 Trước khi tạo phiếu:**
- ✅ Kiểm tra nhà cung cấp có tồn tại
- ✅ Xác nhận giá cả với nhà cung cấp
- ✅ Tính toán số lượng cần thiết
- ✅ Kiểm tra ngân sách có đủ
- ✅ Xác nhận tỷ lệ thuế VAT với nhà cung cấp

### **🚨 Khi nhận hàng:**
- ✅ Kiểm tra số lượng thực tế
- ✅ Kiểm tra chất lượng hàng hóa
- ✅ Xác nhận giá cả cuối cùng
- ✅ Cập nhật thông tin kho

### **🚨 Bảo mật:**
- 🔒 Chỉ nhân viên có quyền mới được tạo phiếu
- 🔒 Không được xóa phiếu đã gửi
- 🔒 Ghi chú rõ ràng mọi thay đổi

---

## 🆘 XỬ LÝ SỰ CỐ THƯỜNG GẶP

### **❌ Không tìm thấy phụ tùng:**
- **Nguyên nhân**: Phụ tùng chưa được tạo trong hệ thống
- **Giải pháp**: Tạo phụ tùng mới trước khi tạo phiếu nhập

### **❌ Không có giá tự động:**
- **Nguyên nhân**: Chưa có giá từ nhà cung cấp
- **Giải pháp**: Nhập giá thủ công hoặc liên hệ nhà cung cấp

### **❌ Không thể nhận hàng:**
- **Nguyên nhân**: Phiếu chưa ở trạng thái "Đã Gửi"
- **Giải pháp**: Gửi phiếu cho nhà cung cấp trước

### **❌ Lỗi cập nhật tồn kho:**
- **Nguyên nhân**: Dữ liệu không nhất quán
- **Giải pháp**: Liên hệ quản trị viên hệ thống

---

## 📞 HỖ TRỢ

Nếu gặp vấn đề trong quá trình sử dụng:
- 📧 Email: support@garage.com
- 📞 Hotline: 1900-xxxx
- 💬 Chat: Trong hệ thống
- 📋 Ticket: Tạo ticket hỗ trợ

---

## 📝 CHANGELOG

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2024-01-15 | Tạo tài liệu hướng dẫn ban đầu |
| 1.1 | 2024-01-20 | Thêm phần xử lý sự cố |
| 1.2 | 2024-01-25 | Cập nhật tính năng nhận hàng |
| 1.3 | 2024-01-30 | **Thêm tính năng tính thuế VAT** - Hỗ trợ tỷ lệ 0%, 8%, 10% |

---

---

## 📊 **QUY TRÌNH NHẬP KHO HOÀN CHỈNH**

### **🎯 Tổng Quan**

Hệ thống quản lý nhập kho đã được thiết kế với quy trình hoàn chỉnh từ tạo phiếu nhập đến xác nhận nhập kho thực tế.

### **📊 Các Trang Quản Lý**

#### **1. Nhập Xuất Kho** (`/StockManagement`)
- **Mục đích**: Tạo phiếu nhập hàng mới
- **Chức năng**: 
  - Tạo đơn nhập hàng với nhiều phụ tùng
  - Sử dụng Typeahead để tìm kiếm phụ tùng
  - Quản lý theo đơn hàng (không phải từng item riêng lẻ)
  - Lưu trữ thông tin nhà cung cấp, số phiếu, ghi chú

#### **2. Phiếu Nhập Hàng** (`/PurchaseOrder`)
- **Mục đích**: Quản lý danh sách phiếu nhập đã tạo
- **Chức năng**:
  - Xem tất cả phiếu nhập
  - Xem chi tiết từng phiếu nhập
  - In phiếu nhập
  - Xác nhận nhập kho thực tế

### **🔄 Quy Trình Nhập Kho Chi Tiết**

#### **Bước 1: Tạo Phiếu Nhập Hàng**
1. Truy cập **Nhập Xuất Kho** (`/StockManagement`)
2. Click **"Tạo Giao Dịch Mới"**
3. Chọn loại giao dịch: **"Nhập kho"**
4. Điền thông tin:
   - Nhà cung cấp
   - Số phiếu nhập
   - Ngày nhập
   - Ghi chú
5. Thêm phụ tùng:
   - Sử dụng Typeahead để tìm kiếm phụ tùng
   - Nhập số lượng, đơn giá
   - Click **"Thêm Phụ Tùng"** để thêm nhiều item
6. Click **"Lưu Giao Dịch"**

#### **Bước 2: Quản Lý Phiếu Nhập**
1. Truy cập **Phiếu Nhập Hàng** (`/PurchaseOrder`)
2. Xem danh sách tất cả phiếu nhập:
   - Số phiếu
   - Ngày tạo
   - Nhà cung cấp
   - Số loại phụ tùng
   - Tổng tiền
   - Trạng thái

#### **Bước 3: Xem Chi Tiết Phiếu Nhập**
1. Click **"Xem chi tiết"** (👁️) trên phiếu nhập
2. Modal hiển thị:
   - Thông tin phiếu nhập
   - Chi tiết từng phụ tùng
   - Tổng tiền
3. Có thể:
   - **In phiếu nhập**
   - **Xác nhận nhập kho**

#### **Bước 4: In Phiếu Nhập**
1. Click **"In phiếu"** (🖨️) hoặc từ modal chi tiết
2. Trang in hiển thị:
   - Header công ty
   - Thông tin phiếu nhập
   - Bảng chi tiết phụ tùng
   - Chữ ký các bên liên quan
   - Ngày in
3. Sử dụng nút **"In Phiếu Nhập Hàng"** để in

#### **Bước 5: Xác Nhận Nhập Kho**
1. Click **"Xác nhận nhập kho"** (✅)
2. Modal **"Xác Nhận Nhập Kho"** hiển thị:
   - Bảng phụ tùng với số lượng đặt
   - Ô nhập số lượng thực nhập
   - Ô ghi chú cho từng item
   - Ghi chú tổng quan
3. Kiểm tra hàng hóa thực tế
4. Nhập số lượng thực nhập (có thể khác số lượng đặt)
5. Thêm ghi chú nếu cần
6. Click **"Xác Nhận Nhập Kho"**

### **🎨 Tính Năng UI/UX**

#### **Typeahead Tìm Kiếm Phụ Tùng**
- Gõ tên phụ tùng để tìm kiếm
- Hiển thị kết quả theo thời gian thực
- Hỗ trợ tìm kiếm theo tên, mã phụ tùng, thương hiệu

#### **Quản Lý Đơn Hàng**
- Một đơn nhập có thể chứa nhiều phụ tùng
- Tự động tính tổng tiền
- Lưu trữ theo `ReferenceNumber` (số phiếu)

#### **DataTable với Search**
- Tìm kiếm theo số phiếu, nhà cung cấp
- Sắp xếp theo ngày tạo
- Phân trang tự động

#### **Responsive Design**
- ✅ **Desktop**: Table hiển thị đầy đủ columns
- ✅ **Tablet (≤768px)**: Table có thể scroll ngang, buttons nhỏ hơn
- ✅ **Mobile (≤576px)**: Buttons xếp dọc, dễ bấm
- ✅ Modal responsive với overflow fix
- ✅ Typeahead dropdown CSS optimized cho mobile
- ✅ Print-friendly

### **🔧 Công Nghệ Sử Dụng**

#### **Backend**
- **Controller**: `PurchaseOrderController`
- **API Endpoints**: `/PurchaseOrder/*`
- **Data Access**: `StockTransactionDto` với related entities

#### **Frontend**
- **JavaScript**: `purchase-order-management.js`
- **DataTables**: Hiển thị danh sách
- **Bootstrap Modals**: Chi tiết và xác nhận
- **SweetAlert2**: Thông báo đẹp
- **Print CSS**: Tối ưu cho in ấn

#### **Database**
- **Table**: `StockTransactions`
- **Grouping**: Theo `ReferenceNumber`
- **Relations**: `Parts`, `Suppliers`, `Employees`

### **📈 Lợi Ích**

#### **Cho Quản Lý**
- Theo dõi được tất cả phiếu nhập
- Kiểm soát quy trình nhập kho
- Báo cáo tổng hợp dễ dàng

#### **Cho Nhân Viên**
- Giao diện thân thiện
- Tìm kiếm phụ tùng nhanh chóng
- Quy trình rõ ràng, dễ thực hiện

#### **Cho Hệ Thống**
- Dữ liệu nhất quán
- Audit trail đầy đủ
- Scalable architecture

### **🚀 Cách Sử Dụng**

1. **Tạo phiếu nhập**: `/StockManagement` → Tạo Giao Dịch Mới
2. **Quản lý phiếu nhập**: `/PurchaseOrder` → Xem danh sách
3. **In phiếu nhập**: Click nút In từ danh sách hoặc chi tiết
4. **Xác nhận nhập kho**: Click nút Xác nhận → Điền số lượng thực tế

### **📝 Ghi Chú**

- Tất cả phiếu nhập đều có trạng thái **"Chờ nhập"** ban đầu
- Có thể in phiếu nhập trước khi xác nhận nhập kho
- Số lượng thực nhập có thể khác số lượng đặt (do hư hỏng, thiếu hàng...)
- Hệ thống tự động cập nhật tồn kho sau khi xác nhận nhập

---

**✅ Quy trình nhập kho đã hoàn thiện và sẵn sàng sử dụng!**

**📄 Tài liệu này được cập nhật thường xuyên. Vui lòng kiểm tra phiên bản mới nhất.**
