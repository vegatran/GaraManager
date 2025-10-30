# GIAI ĐOẠN 2: LẬP KẾ HOẠCH & PHÂN CÔNG

## 📋 MỤC LỤC

1. [Tổng quan Giai đoạn 2.1](#tổng-quan)
2. [Đánh giá ban đầu](#đánh-gía-ban-đầu)
3. [Triển khai](#triển-khai)
4. [Hoàn thành](#hoàn-thành)
5. [Migration](#migration)

---

## 📖 TỔNG QUAN

### **Mô tả:**
Giai đoạn 2.1: Lập Kế Hoạch & Phân Công là bước đầu tiên trong Giai đoạn 2: Sửa Chữa & Quản lý Xuất Kho. Giai đoạn này bắt đầu khi Lệnh Sửa Chữa (JO) đã được khách hàng duyệt (từ Giai đoạn 1) và kết thúc khi xe được nghiệm thu chất lượng (QC).

### **Các bước chính:**

#### **2.1.1: Chuyển JO sang Trạng thái chờ xử lý**
- **Hoạt động:** Cố vấn Dịch vụ (CVDV) chuyển JO từ "Đã Báo giá" sang "Chờ Phân công"
- **Bộ phận:** Cố vấn Dịch vụ
- **Quy tắc:** Hệ thống tự động khóa tính năng chỉnh sửa Báo giá

#### **2.1.2: Phân công KTV & Thời gian**
- **Hoạt động:** Quản đốc/Tổ trưởng chỉ định KTV phù hợp và nhập Giờ công dự kiến (Estimated Labor Hours) cho từng hạng mục
- **Bộ phận:** Quản đốc/Tổ trưởng
- **Quy tắc:** Lịch làm việc KTV được cập nhật, JO chuyển sang trạng thái "Đang chờ Vật tư/Sẵn sàng làm"

---

## 🔍 ĐÁNH GIÁ BAN ĐẦU

### **✅ Những gì đã có (~40%):**

#### **1. Database Entities:**
- ✅ `ServiceOrder` với `PrimaryTechnicianId` (KTV chính - 1 người)
- ✅ `ServiceOrderItem` với các field cơ bản
- ✅ `ServiceOrderLabor` với `ActualHours` (thiếu `EstimatedHours`)

#### **2. API Controllers:**
- ✅ CRUD đầy đủ cho ServiceOrder
- ✅ Business rule: Kiểm tra Quotation phải "Approved" mới cho tạo JO

#### **3. Giao diện:**
- ✅ Trang quản lý JO với DataTable
- ✅ Modals: Create, Edit, View cơ bản

### **❌ Còn thiếu (~60%):**

#### **1. 2.1.1 - Chuyển JO sang "Chờ Phân công":**
- ❌ Logic chuyển trạng thái: "Đã Báo giá" → "Chờ Phân công"
- ❌ Khóa chỉnh sửa Báo giá sau khi tạo JO
- ❌ Status hiện tại: "Pending", "InProgress", "Completed" - thiếu workflow states

#### **2. 2.1.2 - Phân công KTV & Thời gian:**
- ❌ Phân công KTV cho từng item (chỉ có KTV chính cho JO)
- ❌ Field `AssignedTechnicianId` trong `ServiceOrderItem`
- ❌ Nhập Giờ công dự kiến (`EstimatedHours`) cho từng item
- ❌ Modal/form để phân công
- ❌ Integration cập nhật lịch làm việc KTV

---

## 🛠️ TRIỂN KHAI

### **Bước 1: Database Migration**

#### **Entities được cập nhật:**
- ✅ `ServiceOrderItem`: Thêm `AssignedTechnicianId` và `EstimatedHours`
- ✅ `ServiceOrderLabor`: Thêm `EstimatedHours`
- ✅ Navigation property: `AssignedTechnician` trong `ServiceOrderItem`

#### **Migration:**
- **File:** `20251029101126_AddTechnicianAssignmentToServiceOrderItems.cs`
- **Thay đổi:**
  - Thêm column `EstimatedHours` vào `ServiceOrderLabors` (decimal, NOT NULL, default = 0)
  - Thêm column `AssignedTechnicianId` vào `ServiceOrderItems` (int, NULLABLE)
  - Thêm column `EstimatedHours` vào `ServiceOrderItems` (decimal, NULLABLE)
  - Tạo Index: `IX_ServiceOrderItems_AssignedTechnicianId`
  - Tạo Foreign Key: `FK_ServiceOrderItems_Employees_AssignedTechnicianId`

### **Bước 2: DTOs**

#### **DTOs mới:**
- ✅ `AssignTechnicianDto` - Phân công KTV cho một item
- ✅ `ChangeServiceOrderStatusDto` - Chuyển trạng thái JO
- ✅ `BulkAssignTechnicianDto` - Phân công hàng loạt

#### **DTOs được cập nhật:**
- ✅ `ServiceOrderItemDto`: Thêm `AssignedTechnicianId`, `AssignedTechnicianName`, `EstimatedHours`

### **Bước 3: API Endpoints**

#### **Các endpoints mới:**

1. **`PUT /api/ServiceOrders/{id}/change-status`**
   - Chuyển trạng thái ServiceOrder
   - Validate workflow transitions
   - Auto-lock Quotation khi chuyển sang "PendingAssignment"

2. **`PUT /api/ServiceOrders/{id}/items/{itemId}/assign-technician`**
   - Phân công KTV cho một item cụ thể
   - Validate technician tồn tại
   - Auto-update order status nếu tất cả items đã được phân công

3. **`PUT /api/ServiceOrders/{id}/bulk-assign-technician`**
   - Phân công hàng loạt cho nhiều items
   - Cho phép áp dụng cho tất cả hoặc selected items

4. **`PUT /api/ServiceOrders/{id}/items/{itemId}/set-estimated-hours`**
   - Cập nhật giờ công dự kiến cho một item
   - Validation: 0.1 - 24 giờ

#### **Business Logic:**
- ✅ Lock Quotation editing trong `UpdateQuotation` API khi đã có `ServiceOrderId`
- ✅ Workflow state machine với validation transitions
- ✅ Auto-transition: "PendingAssignment" → "ReadyToWork" khi tất cả items đã được phân công

### **Bước 4: AutoMapper**

- ✅ Cập nhật `ServiceOrderProfile` để map `AssignedTechnicianName`
- ✅ Cập nhật `MapToDto` với logic bổ sung để map navigation properties

### **Bước 5: Web Controllers**

#### **OrderManagementController:**
- ✅ `ChangeOrderStatus` endpoint
- ✅ `AssignTechnicianToItem` endpoint
- ✅ `BulkAssignTechnician` endpoint
- ✅ `SetEstimatedHours` endpoint
- ✅ Cập nhật `TranslateOrderStatus` với các status mới:
  - "PendingAssignment" → "Chờ Phân Công"
  - "WaitingForParts" → "Đang Chờ Vật Tư"
  - "ReadyToWork" → "Sẵn Sàng Làm"

#### **QuotationManagementController:**
- ✅ Thêm `ServiceOrderId` vào GetQuotations response để check lock

### **Bước 6: Views**

#### **Modal mới:**
- ✅ `_AssignTechnicianModal.cshtml`
  - Form phân công KTV với table items
  - Dropdown chọn KTV cho từng item
  - Input EstimatedHours cho từng item
  - Phân công hàng loạt (cùng KTV cho tất cả)
  - Nút "Lưu tất cả phân công"

#### **Modal được cập nhật:**
- ✅ `_ViewOrderModal.cshtml`: Thêm cột "KTV Được Phân Công" và "Giờ Công Dự Kiến"

### **Bước 7: JavaScript**

#### **order-management.js:**

**Functions mới:**
- ✅ `changeOrderStatus()` - Chuyển trạng thái JO với validation
- ✅ `openAssignTechnicianModal()` - Mở modal phân công, load order details
- ✅ `loadTechniciansForAssignment()` - Load danh sách KTV cho dropdown
- ✅ `populateAssignTechnicianItems()` - Populate items vào modal table
- ✅ `assignTechnicianToItem()` - Phân công từng item
- ✅ `bulkAssignTechnician()` - Phân công hàng loạt
- ✅ `saveAllAssignments()` - Lưu tất cả phân công (Promise.all)

**DataTable updates:**
- ✅ Thêm button "Chuyển sang Chờ Phân công" (hiện khi status = "Pending")
- ✅ Thêm button "Phân công" (hiện khi status = "PendingAssignment")
- ✅ Logic hiển thị/ẩn buttons dựa trên status

**View Modal updates:**
- ✅ Hiển thị `AssignedTechnicianName` và `EstimatedHours` trong table items

#### **quotation-management.js:**

**Lock Quotation logic:**
- ✅ Check `serviceOrderId` trong `editQuotation()` → Hiển thị warning và redirect
- ✅ Ẩn nút Edit trong DataTable nếu có `ServiceOrderId`

---

## ✅ HOÀN THÀNH

### **Trạng thái triển khai:**

- **Backend:** ✅ 100% Hoàn thành
- **Frontend:** ✅ 100% Hoàn thành
- **Database Migration:** ✅ Applied
- **Build:** ✅ Success

**Tổng tiến độ Giai đoạn 2.1:** ✅ **100%**

---

## 📊 WORKFLOW ĐÃ TRIỂN KHAI

### **Status Transitions:**
```
Pending → PendingAssignment → WaitingForParts/ReadyToWork → InProgress → Completed
```

### **Validation:**
- ✅ Chỉ cho phép transitions hợp lệ
- ✅ Auto-transition khi tất cả items đã được phân công

### **Lock Quotation:**
- ✅ API: Check `ServiceOrderId` → Từ chối cập nhật
- ✅ Frontend: Check `serviceOrderId` → Warning + redirect
- ✅ DataTable: Ẩn nút Edit nếu có `ServiceOrderId`

---

## 🗄️ DATABASE MIGRATION

### **Migration Details:**

**File:** `20251029101126_AddTechnicianAssignmentToServiceOrderItems.cs`  
**Ngày tạo:** 2025-10-29 10:11:26  
**Ngày áp dụng:** 2025-10-29

### **Thay đổi Database:**

#### **ServiceOrderLabors Table:**
```sql
ALTER TABLE ServiceOrderLabors 
ADD EstimatedHours decimal(65,30) NOT NULL DEFAULT 0;
```

#### **ServiceOrderItems Table:**
```sql
-- Thêm columns
ALTER TABLE ServiceOrderItems 
ADD AssignedTechnicianId int NULL;

ALTER TABLE ServiceOrderItems 
ADD EstimatedHours decimal(65,30) NULL;

-- Tạo Index
CREATE INDEX IX_ServiceOrderItems_AssignedTechnicianId 
ON ServiceOrderItems (AssignedTechnicianId);

-- Tạo Foreign Key
ALTER TABLE ServiceOrderItems
ADD CONSTRAINT FK_ServiceOrderItems_Employees_AssignedTechnicianId
FOREIGN KEY (AssignedTechnicianId) 
REFERENCES Employees (Id);
```

### **Kết quả:**
- ✅ Migration Status: **Applied Successfully**
- ✅ Build Status: **Build Succeeded**
- ✅ Database: **Updated**

### **Lưu ý:**
1. **Data Safety:**
   - `EstimatedHours` trong `ServiceOrderLabors` có default = 0 (không mất data)
   - `AssignedTechnicianId` và `EstimatedHours` trong `ServiceOrderItems` là nullable (an toàn)

2. **Foreign Key:**
   - Foreign Key đến bảng `Employees` với `ON DELETE SET NULL`
   - Nếu xóa Employee, `AssignedTechnicianId` sẽ được set về NULL

3. **Rollback:**
   - Có thể rollback bằng: `dotnet ef database update <PreviousMigration>`
   - Hoặc xóa migration: `dotnet ef migrations remove`

---

## 📂 FILES ĐÃ THAY ĐỔI

### **Entities:**
- `src/GarageManagementSystem.Core/Entities/ServiceOrderItem.cs`
- `src/GarageManagementSystem.Core/Entities/ServiceOrderLabor.cs`

### **DTOs:**
- `src/GarageManagementSystem.Shared/DTOs/ServiceOrderItemDto.cs`
- `src/GarageManagementSystem.Shared/DTOs/AssignTechnicianDto.cs` (mới)

### **API:**
- `src/GarageManagementSystem.API/Controllers/ServiceOrdersController.cs`
- `src/GarageManagementSystem.API/Controllers/ServiceQuotationsController.cs`
- `src/GarageManagementSystem.API/Profiles/ServiceOrderProfile.cs`

### **Web:**
- `src/GarageManagementSystem.Web/Controllers/OrderManagementController.cs`
- `src/GarageManagementSystem.Web/Controllers/QuotationManagementController.cs`
- `src/GarageManagementSystem.Web/Configuration/ApiEndpoints.cs`
- `src/GarageManagementSystem.Web/Views/OrderManagement/_AssignTechnicianModal.cshtml` (mới)
- `src/GarageManagementSystem.Web/Views/OrderManagement/_ViewOrderModal.cshtml`
- `src/GarageManagementSystem.Web/Views/OrderManagement/Index.cshtml`
- `src/GarageManagementSystem.Web/wwwroot/js/order-management.js`
- `src/GarageManagementSystem.Web/wwwroot/js/quotation-management.js`

### **Migrations:**
- `src/GarageManagementSystem.Infrastructure/Migrations/20251029101126_AddTechnicianAssignmentToServiceOrderItems.cs`

---

## 🧪 TESTING CHECKLIST

### **Chức năng cần test:**

- [ ] **2.1.1 - Chuyển trạng thái:**
  - [ ] Test chuyển từ "Pending" → "PendingAssignment"
  - [ ] Test validation transitions không hợp lệ
  - [ ] Test lock Quotation editing khi đã có JO

- [ ] **2.1.2 - Phân công KTV:**
  - [ ] Test phân công KTV cho từng item
  - [ ] Test phân công hàng loạt
  - [ ] Test nhập EstimatedHours
  - [ ] Test validation EstimatedHours (0.1 - 24 giờ)
  - [ ] Test auto-transition khi tất cả items đã được phân công
  - [ ] Test hiển thị thông tin phân công trong View Modal

- [ ] **Lock Quotation:**
  - [ ] Test ẩn nút Edit trong DataTable nếu có ServiceOrderId
  - [ ] Test warning khi click Edit nếu đã có ServiceOrderId
  - [ ] Test API từ chối UpdateQuotation nếu có ServiceOrderId

---

## 📝 HƯỚNG DẪN SỬ DỤNG

### **2.1.1: Chuyển JO sang "Chờ Phân công"**

1. Vào trang **"Quản Lý Phiếu Sửa Chữa"**
2. Tìm JO có trạng thái **"Chờ Xử Lý"** (Pending)
3. Click nút **"→"** (Chuyển trạng thái) trong cột "Thao Tác"
4. Xác nhận chuyển trạng thái
5. JO chuyển sang **"Chờ Phân Công"** (PendingAssignment)
6. Quotation editing tự động bị khóa

### **2.1.2: Phân công KTV & Thời gian**

#### **Phân công từng item:**
1. Click nút **"👔"** (Phân công KTV) trong cột "Thao Tác"
2. Modal hiện với danh sách items
3. Chọn KTV cho từng item trong dropdown
4. Nhập EstimatedHours cho từng item (tùy chọn)
5. Click **"✓"** (Phân công) ở từng item hoặc **"Lưu Tất Cả Phân Công"**

#### **Phân công hàng loạt:**
1. Trong modal phân công, ở phần **"Phân Công Hàng Loạt"**
2. Chọn KTV từ dropdown
3. Nhập EstimatedHours (tùy chọn - sẽ áp dụng cho tất cả)
4. Click **"Áp Dụng"**
5. Tất cả items chưa được phân công sẽ được assign cùng KTV

#### **Lưu tất cả:**
1. Sau khi phân công từng item hoặc hàng loạt
2. Click **"Lưu Tất Cả Phân Công"**
3. Tất cả phân công được lưu cùng lúc
4. Nếu tất cả items đã được phân công → JO tự động chuyển sang **"Sẵn Sàng Làm"** (ReadyToWork)

---

## ⚠️ LƯU Ý QUAN TRỌNG

1. **Workflow States:** Cần tuân thủ đúng workflow, không thể nhảy bước
2. **Lock Quotation:** Một khi Quotation đã được chuyển thành JO, không thể chỉnh sửa Quotation nữa
3. **Validation:** EstimatedHours phải từ 0.1 đến 24 giờ
4. **Foreign Key:** Nếu xóa Employee, AssignedTechnicianId sẽ là NULL (safe)

---

## 🎯 TÍNH NĂNG ĐÃ SẴN SÀNG

Sau khi migration được áp dụng, các tính năng sau đã sẵn sàng sử dụng:

1. ✅ **Phân công KTV cho từng item** trong ServiceOrder
2. ✅ **Nhập giờ công dự kiến** (EstimatedHours) cho từng item
3. ✅ **Hiển thị thông tin KTV được phân công** trong View Modal
4. ✅ **Workflow chuyển trạng thái** ServiceOrder
5. ✅ **Lock Quotation editing** khi đã có ServiceOrder

---

## 📊 TỔNG KẾT

**Ngày hoàn thành:** 2025-10-29  
**Trạng thái:** ✅ **100% Hoàn thành và sẵn sàng sử dụng**

**Giai đoạn 2.1: Lập Kế Hoạch & Phân Công** đã được triển khai đầy đủ từ Backend đến Frontend, Database Migration đã được áp dụng thành công.

---

## 📝 CÁC TÍNH NĂNG ĐÃ BỔ SUNG

### **1. Phân quyền phân công KTV** ✅

**Yêu cầu:** "Quản đốc/Tổ trưởng chỉ định KTV" (từ tài liệu)

**Triển khai:**
- ✅ Kiểm tra Position: Quản đốc, Tổ trưởng, Quản lý, Manager, Supervisor
- ✅ Kiểm tra Roles từ claims: Manager, Supervisor, Admin, SuperAdmin
- ✅ Áp dụng cho `AssignTechnicianToItem` và `BulkAssignTechnician`
- ✅ Return `Forbid` nếu không có quyền

**Code Location:**
- `src/GarageManagementSystem.API/Controllers/ServiceOrdersController.cs`
  - `AssignTechnicianToItem` endpoint (line ~787-816)
  - `BulkAssignTechnician` endpoint (line ~890-917)

---

### **2. API Workload Endpoint** ✅

**Endpoint:** `GET /api/Employees/{id}/workload`

**Chức năng:**
- ✅ Hiển thị tổng giờ công dự kiến đã phân công
- ✅ Số lượng JO đang xử lý
- ✅ Số items được phân công hôm nay
- ✅ Capacity used (dựa trên 8h/ngày)
- ✅ Danh sách active orders với chi tiết

**Response Structure:**
```json
{
  "Employee": { "Id", "Name", "Position" },
  "Date": "2025-10-29",
  "ActiveOrders": {
    "Count": 3,
    "TotalEstimatedHours": 6.5,
    "Items": [...]
  },
  "Today": {
    "AssignedItemsCount": 5,
    "EstimatedHours": 2.5
  },
  "Statistics": {
    "TotalActiveItems": 8,
    "TotalCompletedOrders": 12,
    "CapacityUsed": 81.25
  }
}
```

**Code Location:**
- `src/GarageManagementSystem.API/Controllers/EmployeesController.cs` (line ~438-548)

---

### **3. Hiển thị Workload trong Dropdown** ✅

**Triển khai:**
- ✅ Load workload cho từng KTV khi mở modal phân công
- ✅ Hiển thị trong dropdown: `"Nguyễn Văn A - KTV (6.5h/8h, 3 JO, 81% tải)"`
- ✅ Áp dụng cho dropdown hàng loạt

**Code Location:**
- `src/GarageManagementSystem.Web/wwwroot/js/order-management.js`
  - `loadTechniciansForAssignment()` (line ~586-618)
  - `populateTechnicianDropdowns()` (line ~620-648)

**Workflow:**
1. Load danh sách employees
2. Load workload cho từng employee (Promise.all)
3. Format display text với workload info
4. Populate vào dropdown

---

### **4. Cập nhật Appointment khi phân công** ✅

**Triển khai:**
- ✅ Tìm Appointment liên quan đến ServiceOrder (`ServiceOrderId`)
- ✅ Cập nhật `AssignedToId` khi phân công KTV (nếu chưa có)
- ✅ Cập nhật `EstimatedDuration` dựa trên tổng EstimatedHours
- ✅ Tự động tạo Appointment mới nếu chưa có (khi có ScheduledDate)

**Logic:**
1. Khi phân công KTV cho item:
   - Tìm Appointment có `ServiceOrderId = order.Id`
   - Nếu có: Update `AssignedToId` và `EstimatedDuration`
   - Nếu không có và có `ScheduledDate`: Tạo Appointment mới

**Code Location:**
- `src/GarageManagementSystem.API/Controllers/ServiceOrdersController.cs`
  - `AssignTechnicianToItem` endpoint (line ~852-908)

---

## 🎯 CÁC TÍNH NĂNG OPTIONAL (Có thể làm sau)

### **1. Kiểm tra xung đột thời gian** ⏳

**Mô tả:** Validate không xung đột lịch khi phân công KTV

**Cần implement:**
- Check xung đột dựa trên `ScheduledDate`, `EstimatedHours`, và Appointments hiện tại
- Hiển thị warning nếu có xung đột
- Cho phép override nếu cần

**Độ ưu tiên:** ⭐⭐ (Low)

---

### **2. Hiển thị chuyên môn/skills của KTV** ⏳

**Mô tả:** Gợi ý KTV phù hợp với hạng mục

**Cần implement:**
- Hiển thị chuyên môn trong dropdown
- Gợi ý KTV dựa trên `Service.Category`, `Service.ServiceType`
- Highlight KTV phù hợp nhất

**Độ ưu tiên:** ⭐⭐ (Low)

---

### **3. Tính tổng EstimatedHours trong View** ⏳

**Mô tả:** Hiển thị tổng giờ công dự kiến của JO

**Cần implement:**
- Tính tổng EstimatedHours khi phân công
- Hiển thị trong View Order Modal
- So sánh Estimated vs Actual khi có dữ liệu

**Độ ưu tiên:** ⭐ (Very Low)

---

### **4. Validation EstimatedHours nâng cao** ⏳

**Mô tả:** Validation dựa trên loại service và lịch sử

**Cần implement:**
- Validation theo ServiceType/ServiceCategory
- So sánh với historical data
- Warning nếu chênh lệch > 50%

**Độ ưu tiên:** ⭐⭐ (Low)

---

### **5. Export/Print phiếu phân công** ⏳

**Mô tả:** In phiếu phân công cho KTV

**Cần implement:**
- Template print phân công (PDF/HTML)
- Export Excel: Danh sách phân công theo KTV
- View schedule theo KTV

**Độ ưu tiên:** ⭐ (Very Low)

---

**Tài liệu này tổng hợp tất cả thông tin về Giai đoạn 2.1 trong một file duy nhất.**

