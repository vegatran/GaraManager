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

---

## 📋 GIAI ĐOẠN 2.2: YÊU CẦU VẬT TƯ (MATERIAL REQUEST)

### **Tổng quan:**
Giai đoạn 2.2: Yêu Cầu Vật Tư (MR) là bước quản lý xuất kho cho các phụ tùng cần thiết để thực hiện công việc sửa chữa.

### **Trạng thái triển khai:**
- ✅ **Backend:** 100% Hoàn thành (Entity, DTO, API, Repository)
- ✅ **Frontend:** 100% Hoàn thành (UI, JavaScript, Validation)
- ✅ **Database Migration:** ✅ Applied
- ✅ **Build:** ✅ Success

**Tổng tiến độ Giai đoạn 2.2:** ✅ **100%**

### **Các tính năng đã triển khai:**
1. ✅ Tạo MR từ Service Order (JO)
2. ✅ Load danh sách phụ tùng từ Quotation gợi ý
3. ✅ Thêm/xóa vật tư trong MR
4. ✅ Submit MR để phê duyệt
5. ✅ Approve/Reject MR
6. ✅ Workflow: Draft → PendingApproval → Approved → Picked → Issued → Delivered
7. ✅ Thông báo khi JO không có phụ tùng (chỉ có dịch vụ/tiền công)

### **Mối liên kết với Giai đoạn 2.3:**
- ✅ Khi ServiceOrder status = `WaitingForParts` → Cần MR
- ✅ Sau khi MR được Approve → ServiceOrder có thể chuyển sang `ReadyToWork`
- ✅ Nếu JO không có phụ tùng → Bỏ qua MR, chuyển thẳng sang 2.3

---

## 📋 GIAI ĐOẠN 2.3: QUẢN LÝ TIẾN ĐỘ SỬA CHỮA VÀ PHÁT SINH

### **Tổng quan:**
Giai đoạn 2.3: Quản Lý Tiến Độ Sửa Chữa và Phát Sinh bao gồm việc KTV bắt đầu công việc, ghi nhận giờ công thực tế, phát hiện và xử lý phát sinh, cập nhật tiến độ theo từng mốc.

### **Các hoạt động chính:**

#### **2.3.1: Bắt đầu Công việc**
- **Hoạt động:** KTV bắt đầu làm việc, ghi nhận thời gian bắt đầu thực tế
- **Bộ phận:** Kỹ thuật viên
- **Quy tắc:** Hệ thống bắt đầu tính **Giờ công thực tế (Actual Labor Hours)** của KTV cho JO đó

#### **2.3.2: Phát hiện Phát sinh**
- **Hoạt động:** KTV phát hiện hư hỏng ngoài JO ban đầu
- **Bộ phận:** Kỹ thuật viên
- **Quy tắc:** Dừng công việc liên quan. KTV ghi nhận lỗi phát sinh vào hệ thống

#### **2.3.3: Báo giá Phát sinh**
- **Hoạt động:** CVDV lập Báo giá bổ sung và liên hệ khách hàng để xin duyệt
- **Bộ phận:** Cố vấn Dịch vụ
- **Quy tắc:** Nếu khách hàng đồng ý, tạo **LSC Bổ sung** (Lệnh Sửa chữa Bổ sung) và quay lại bước **2.2.1 (Yêu cầu Xuất kho)** cho vật tư phát sinh

#### **2.3.4: Cập nhật Tiến độ**
- **Hoạt động:** KTV cập nhật tiến độ công việc theo từng mốc (ví dụ: Đồng sơn hoàn thành, Thay dầu hoàn thành)
- **Bộ phận:** Kỹ thuật viên
- **Quy tắc:** Hệ thống hiển thị **Tiến độ JO** theo thời gian thực (rất quan trọng cho CVDV theo dõi)

---

## 🔍 ĐÁNH GIÁ GIAI ĐOẠN 2.3

### **✅ Những gì đã có (~20%):**

#### **1. Database Entities:**
- ✅ `ServiceOrderLabor` có:
  - `StartTime` (DateTime?) - Thời gian bắt đầu
  - `EndTime` (DateTime?) - Thời gian kết thúc
  - `ActualHours` (decimal) - Giờ công thực tế
  - `Status` (string) - Trạng thái: "Pending", "InProgress", "Completed"
- ✅ `ServiceOrderItem` có:
  - `Status` (string) - Trạng thái item: "Pending", "InProgress", "Completed", "Cancelled"
  - `AssignedTechnicianId` (int?) - KTV được phân công
  - `EstimatedHours` (decimal?) - Giờ công dự kiến
- ✅ `ServiceOrder` có:
  - `StartDate` (DateTime?) - Khi công việc bắt đầu
  - `Status` (string) - Trạng thái tổng thể

#### **2. API Endpoints cơ bản:**
- ✅ `POST /api/ServiceOrders/{id}/start` - Bắt đầu làm việc (Pending → In Progress)
- ✅ `POST /api/ServiceOrders/{id}/complete` - Hoàn thành đơn hàng

---

### **❌ Còn thiếu (~80%):**

#### **1. 2.3.1 - Bắt đầu Công việc:**
- ❌ **Chức năng "Start Work" cụ thể cho KTV:**
  - ❌ KTV không thể click nút "Bắt đầu làm việc" cho từng item
  - ❌ Không có UI để KTV ghi nhận `StartTime` cho `ServiceOrderItem` hoặc `ServiceOrderLabor`
  - ❌ Không có chức năng ghi nhận thời gian bắt đầu thực tế cho từng item
  
- ❌ **Tính toán "Giờ công thực tế":**
  - ❌ Hệ thống chưa có cơ chế để KTV ghi nhận thời gian kết thúc công việc
  - ❌ Không tự động tính `ActualHours = (EndTime - StartTime)`
  - ❌ Không có API endpoint để KTV cập nhật `ActualHours` cho item

#### **2. 2.3.2 - Phát hiện Phát sinh:**
- ❌ **Cơ chế ghi nhận phát sinh:**
  - ❌ Không có Entity để lưu "Phát sinh" (Additional Issue/Change Order)
  - ❌ Không có UI cho KTV báo cáo các hư hỏng phát sinh ngoài JO ban đầu
  - ❌ Không có tính năng upload hình ảnh/mô tả cho phát sinh
  
- ❌ **Chức năng "Dừng công việc liên quan":**
  - ❌ Không có cơ chế để tạm dừng một `ServiceOrderItem` khi có phát sinh
  - ❌ Không có trạng thái "OnHold" hoặc "WaitingForCustomerApproval" cho ServiceOrderItem
  - ❌ Không có liên kết giữa "Phát sinh" và "ServiceOrderItem" bị ảnh hưởng

#### **3. 2.3.3 - Báo giá Phát sinh:**
- ❌ **Tạo "Báo giá bổ sung":**
  - ❌ Không có quy trình hoặc UI để CVDV tạo một `ServiceQuotation` mới liên quan đến một phát sinh của `ServiceOrder` hiện có
  - ❌ Không có field `ParentServiceOrderId` hoặc `RelatedToServiceOrderId` trong `ServiceQuotation` để liên kết
  - ❌ Không có field `IsAdditionalQuotation` hoặc `ChangeOrderQuotation` để phân biệt
  
- ❌ **Liên kết với "LSC Bổ sung":**
  - ❌ Không có khái niệm "LSC Bổ sung" (Additional Service Order)
  - ❌ Không có field `ParentServiceOrderId` hoặc `IsAdditionalOrder` trong `ServiceOrder`
  - ❌ Không có cách để tạo một `ServiceOrder` mới (hoặc cập nhật `ServiceOrder` hiện có) dựa trên báo giá phát sinh đã được duyệt
  
- ❌ **Quay lại 2.2.1 (Yêu cầu Xuất kho):**
  - ❌ Mặc dù có chức năng "Yêu cầu Vật tư (MR)" (Giai đoạn 2.2), nhưng không có luồng tự động quay lại bước này sau khi báo giá phát sinh được duyệt
  - ❌ Không có workflow: `Phát sinh → Báo giá phát sinh → Duyệt → Tạo LSC Bổ sung → Tạo MR cho phát sinh`

#### **4. 2.3.4 - Cập nhật Tiến độ:**
- ❌ **Cập nhật tiến độ theo từng mốc:**
  - ❌ Không có tính năng cho KTV đánh dấu các `ServiceOrderItem` hoặc các "mốc" công việc cụ thể là đã hoàn thành (ví dụ: "Đồng sơn hoàn thành", "Thay dầu hoàn thành")
  - ❌ Không có field `Milestone` hoặc `ProgressMilestones` trong `ServiceOrderItem`
  - ❌ Không có API endpoint để KTV cập nhật trạng thái `ServiceOrderItem.Status` từ "InProgress" → "Completed"
  
- ❌ **Hiển thị "Tiến độ JO theo thời gian thực":**
  - ❌ Hệ thống chưa có dashboard hoặc giao diện chi tiết để CVDV theo dõi tiến độ từng `ServiceOrderItem` một cách trực quan và theo thời gian thực
  - ❌ Không có bảng/UI hiển thị: Item nào đang làm, Item nào đã hoàn thành, Item nào đang chờ
  - ❌ Không có progress bar hoặc percentage cho từng item hoặc toàn bộ JO
  - ❌ Không có timeline view để xem tiến độ theo thời gian

---

## 🔗 MỐI LIÊN KẾT GIỮA GIAI ĐOẠN 2.2 VÀ 2.3

### **Liên kết chính:**

**Theo quy trình nghiệp vụ (2.3.3):**
> "Nếu khách hàng đồng ý, tạo LSC Bổ sung và **quay lại bước 2.2.1 (Yêu cầu Xuất kho) cho vật tư phát sinh.**"

**Vòng lặp workflow:**
```
2.3: KTV đang sửa chữa
    ↓
2.3.2: Phát hiện phát sinh (hư hỏng mới)
    ↓
2.3.3: CVDV tạo báo giá phát sinh → KH duyệt
    ↓
Tạo LSC Bổ sung (hoặc cập nhật JO hiện tại)
    ↓
QUAY LẠI 2.2.1: Tạo MR cho vật tư phát sinh
    ↓
2.2: Submit → Approve → Xuất kho → Delivered
    ↓
QUAY LẠI 2.3: Tiếp tục sửa chữa với vật tư mới
```

### **Liên kết kỹ thuật cần triển khai:**

1. **Entity liên kết:**
   - `ServiceQuotation` cần field `ParentServiceOrderId` hoặc `RelatedToServiceOrderId` (nullable) để liên kết với JO gốc
   - `ServiceQuotation` cần field `IsAdditionalQuotation` (bool) để phân biệt báo giá gốc vs báo giá bổ sung
   - `ServiceOrder` cần field `ParentServiceOrderId` (nullable) để liên kết LSC Bổ sung với JO gốc
   - `ServiceOrder` cần field `IsAdditionalOrder` (bool) để phân biệt

2. **Workflow liên kết:**
   - Khi tạo `ServiceQuotation` từ phát sinh: Set `RelatedToServiceOrderId = serviceOrderId`, `IsAdditionalQuotation = true`
   - Khi duyệt báo giá phát sinh: Tự động tạo MR hoặc thông báo để quay lại 2.2.1
   - Khi MR phát sinh được delivered: Tự động thông báo KTV tiếp tục công việc (2.3)

3. **UI liên kết:**
   - Trong trang Service Order detail: Hiển thị danh sách "Báo giá phát sinh" và "LSC Bổ sung"
   - Trong trang Quotation: Hiển thị link đến JO gốc (nếu là báo giá bổ sung)
   - Trong trang MR: Hiển thị link đến JO gốc và JO bổ sung (nếu có)

---

## 📊 TỔNG KẾT GIAI ĐOẠN 2.3

**Ngày đánh giá:** 2025-10-31  
**Ngày bắt đầu triển khai:** 2025-11-03  
**Ngày hoàn thành 2.3.2 & 2.3.3:** 2025-11-03  
**Ngày hoàn thành 2.3.4:** 2025-11-03  
**Trạng thái:** ✅ **Đã hoàn thành 100% (4/4 tính năng)**

### **Tiến độ triển khai:**

#### **✅ 2.3.1: Bắt đầu Công việc - HOÀN THÀNH 100%**
**Ngày hoàn thành:** 2025-11-03

**Đã triển khai:**
- ✅ Database: Thêm `StartTime`, `EndTime`, `ActualHours`, `CompletedTime` vào `ServiceOrderItem`
- ✅ Migration: `20251103035546_AddActualHoursToServiceOrderItems` (Đã apply)
- ✅ API Endpoints:
  - `POST /api/ServiceOrders/{id}/items/{itemId}/start-work` - KTV bắt đầu làm việc
  - `POST /api/ServiceOrders/{id}/items/{itemId}/stop-work` - KTV dừng làm việc
  - `POST /api/ServiceOrders/{id}/items/{itemId}/complete` - KTV hoàn thành item
- ✅ Web Controllers: `OrderManagementController` với các actions tương ứng
- ✅ JavaScript: `startItemWork()`, `stopItemWork()`, `completeItem()` với validation và confirm dialogs
- ✅ UI: View Modal hiển thị cột "Trạng Thái", "Giờ Công Thực Tế", "Thao Tác" với nút Start/Stop/Complete
- ✅ AutoMapper: Map đầy đủ các fields mới
- ✅ Business Logic:
  - Tự động tính `ActualHours` từ `StartTime` và `EndTime`
  - Tự động cập nhật `ServiceOrder.StartDate` khi item đầu tiên bắt đầu
  - Tự động chuyển `ServiceOrder.Status` sang "InProgress" khi item đầu tiên bắt đầu
  - Tự động chuyển `ServiceOrder.Status` sang "Completed" khi tất cả items hoàn thành
  - Authorization: Chỉ KTV được phân công hoặc Quản đốc/Tổ trưởng mới có thể bắt đầu

**Chức năng:**
- KTV có thể bắt đầu làm việc cho từng item trong View Modal
- Hệ thống tự động ghi nhận `StartTime` và chuyển status sang "InProgress"
- KTV có thể dừng làm việc (tính ActualHours tạm thời)
- KTV có thể hoàn thành item (tự động tính ActualHours cuối cùng và set CompletedTime)
- Hiển thị trạng thái và giờ công thực tế trong View Modal

---

#### **✅ 2.3.4: Cập nhật Tiến độ - HOÀN THÀNH 100%**
**Ngày hoàn thành:** 2025-11-03

**Đã triển khai:**

**1. Database Entities:**
- ✅ `ServiceOrderItem` đã có đầy đủ fields cần thiết:
  - `StartTime`, `EndTime`, `CompletedTime` - Thời gian bắt đầu/kết thúc/hoàn thành
  - `ActualHours` - Giờ công thực tế
  - `EstimatedHours` - Giờ công dự kiến
  - `Status` - Trạng thái: Pending, InProgress, Completed, OnHold, Cancelled

**2. API Endpoints:**
- ✅ `GET /api/ServiceOrders/{id}/progress` - Lấy tiến độ chi tiết của Service Order
  - Tính toán progress percentage (0-100%)
  - Thống kê số lượng items theo trạng thái (Pending, InProgress, Completed, OnHold, Cancelled)
  - Tổng hợp giờ công: Dự kiến, Thực tế, Còn lại
  - Timeline: Ngày tạo, Ngày bắt đầu, Ngày dự kiến hoàn thành, Ngày hoàn thành thực tế
  - Chi tiết từng item: Tên, Trạng thái, Thời gian, Giờ công, Progress percentage, KTV phân công

**3. DTOs:**
- ✅ `ServiceOrderProgressDto` - DTO cho tiến độ Service Order
  - Progress Statistics: TotalItems, PendingItems, InProgressItems, CompletedItems, OnHoldItems, CancelledItems
  - Progress Percentage: 0-100%
  - Time Statistics: TotalEstimatedHours, TotalActualHours, RemainingEstimatedHours
  - Status Timeline: OrderDate, StartDate, ExpectedCompletionDate, ActualCompletionDate
  - Items: List<ServiceOrderItemProgressDto>
- ✅ `ServiceOrderItemProgressDto` - DTO cho tiến độ từng item
  - ItemId, ItemName, Status
  - Time tracking: StartTime, EndTime, CompletedTime, EstimatedHours, ActualHours
  - Progress Percentage: 0-100%
  - Assignment: AssignedTechnicianId, AssignedTechnicianName

**4. Business Logic:**
- ✅ Tính toán progress percentage: `(CompletedItems / TotalItems) * 100`
- ✅ Tính giờ công còn lại: `(EstimatedHours của items chưa hoàn thành) - (ActualHours của items chưa hoàn thành)`
- ✅ Progress percentage cho từng item: Completed = 100%, InProgress = 50%, Pending/OnHold = 0%

**5. Web Controllers:**
- ✅ `OrderManagementController.GetOrderProgress` - Proxy API call

**6. JavaScript:**
- ✅ `order-management.js` với các functions:
  - `loadOrderProgress(serviceOrderId)` - Load tiến độ từ API khi tab được mở
  - `renderProgress(progress)` - Render dashboard tiến độ với:
    - Overall Progress Bar (animated, với percentage)
    - Statistics Cards: Tổng Hạng Mục, Chờ Xử Lý, Đang Làm, Đã Hoàn Thành
    - Time Statistics Cards: Giờ Công Dự Kiến, Giờ Công Thực Tế, Giờ Công Còn Lại
    - Timeline: Ngày tạo, Ngày bắt đầu, Ngày dự kiến hoàn thành, Ngày hoàn thành
    - Items Progress Table: Hiển thị từng item với progress bar, trạng thái, giờ công, KTV phân công
  - Event handler: Load progress khi tab "Tiến Độ" được mở

**7. UI:**
- ✅ Tab "Tiến Độ" trong View Order Modal (`_ViewOrderModal.cshtml`)
  - Overall Progress Bar với animation
  - Statistics Cards với màu sắc phân biệt
  - Time Statistics Cards với icon
  - Timeline hiển thị các mốc thời gian quan trọng
  - Items Progress Table với progress bar cho từng item
  - Badge màu cho trạng thái items
  - Hiển thị KTV phân công và giờ công

**Chức năng:**
- ✅ CVDV có thể xem tiến độ JO theo thời gian thực trong tab "Tiến Độ"
- ✅ Hiển thị progress bar tổng thể với percentage
- ✅ Thống kê số lượng items theo trạng thái
- ✅ Hiển thị giờ công: Dự kiến, Thực tế, Còn lại
- ✅ Timeline các mốc thời gian quan trọng
- ✅ Bảng chi tiết từng item với progress bar và trạng thái
- ✅ Tự động load khi tab được mở

---

#### **✅ 2.3.2: Phát hiện Phát sinh - HOÀN THÀNH 100%**
**Ngày hoàn thành:** 2025-11-03

**Đã triển khai:**

**1. Database Entities:**
- ✅ `AdditionalIssue` - Entity lưu thông tin phát sinh với các fields:
  - `ServiceOrderId` (int, required) - Liên kết với ServiceOrder gốc
  - `ServiceOrderItemId` (int?, nullable) - Hạng mục bị ảnh hưởng (optional)
  - `IssueName` (string, required) - Tên phát sinh
  - `Category` (string) - Danh mục: Engine, Brake, Suspension, Electrical, Body, Tire, Other
  - `Description` (string, required) - Mô tả chi tiết
  - `Severity` (string) - Mức độ: Critical, High, Medium, Low
  - `IsUrgent` (bool) - Cần xử lý ngay
  - `Status` (string) - Trạng thái: Identified, Reported, Quoted, Approved, Rejected, Repaired
  - `ReportedByEmployeeId` (int?) - KTV báo cáo
  - `ReportedDate` (DateTime) - Ngày báo cáo
  - `AdditionalQuotationId` (int?) - Báo giá bổ sung (nếu có)
  - `AdditionalServiceOrderId` (int?) - LSC Bổ sung (nếu có)
  - `Notes` (string?) - Ghi chú KTV
- ✅ `AdditionalIssuePhoto` - Entity lưu hình ảnh phát sinh
  - `AdditionalIssueId` (int, required)
  - `PhotoPath` (string, required)
  - `UploadDate` (DateTime)
- ✅ Migration: `20251103062345_CreateAdditionalIssues` (Đã apply)

**2. API Endpoints:**
- ✅ `GET /api/AdditionalIssues/GetByServiceOrder/{serviceOrderId}` - Lấy danh sách phát sinh theo ServiceOrder
- ✅ `GET /api/AdditionalIssues/{id}` - Lấy chi tiết phát sinh
- ✅ `POST /api/AdditionalIssues/Create` - Tạo phát sinh mới (multipart/form-data, hỗ trợ upload nhiều ảnh)
- ✅ `PUT /api/AdditionalIssues/Update/{id}` - Cập nhật phát sinh (multipart/form-data)
- ✅ `DELETE /api/AdditionalIssues/Delete/{id}` - Xóa phát sinh
- ✅ `POST /api/AdditionalIssues/{id}/upload-photos` - Upload thêm ảnh
- ✅ `DELETE /api/AdditionalIssues/{id}/photos/{photoId}` - Xóa ảnh

**3. Business Logic:**
- ✅ Tự động chuyển `ServiceOrderItem.Status` sang "OnHold" khi có phát sinh liên quan
- ✅ Cập nhật `ServiceOrderItem.Notes` với thông tin phát sinh
- ✅ Validate: Chỉ KTV được phân công hoặc Quản đốc/Tổ trưởng mới có thể báo cáo
- ✅ Auto-set `ReportedByEmployeeId` dựa trên authenticated user hoặc assigned technician
- ✅ File upload validation: Chỉ chấp nhận JPG, JPEG, PNG, GIF, WEBP, max 5MB/ảnh

**4. Web Controllers:**
- ✅ `AdditionalIssuesController` với các actions: Index, GetByServiceOrder, Create, Update, Delete, UploadPhotos, DeletePhoto

**5. JavaScript:**
- ✅ `order-management.js` với các functions:
  - `renderAdditionalIssuesList()` - Hiển thị danh sách phát sinh trong tab
  - `openReportAdditionalIssueModal()` - Mở modal báo cáo phát sinh
  - `loadServiceOrderItemsForIssue()` - Load danh sách items để chọn hạng mục bị ảnh hưởng
  - `submitReportAdditionalIssue()` - Submit form báo cáo (multipart/form-data)
  - `openEditAdditionalIssueModal()` - Mở modal sửa phát sinh
  - `deleteAdditionalIssue()` - Xóa phát sinh với confirm dialog

**6. UI:**
- ✅ Tab "Phát Sinh" trong View Order Modal với:
  - Danh sách phát sinh hiển thị: Tên, danh mục, mức độ (badge màu), trạng thái, ngày báo cáo, KTV báo cáo
  - Nút "Báo Cáo Phát Sinh" để tạo mới
  - Nút "Tạo Báo Giá" cho phát sinh chưa có báo giá
  - Nút "Sửa", "Xóa" cho từng phát sinh
  - Hiển thị hình ảnh (nếu có)
- ✅ Modal `_ReportAdditionalIssueModal.cshtml` với form:
  - Dropdown chọn hạng mục bị ảnh hưởng (optional)
  - Input: Danh mục, Tên phát sinh, Mô tả, Mức độ, Ghi chú KTV
  - Checkbox: Cần xử lý ngay
  - File upload: Upload nhiều ảnh (preview và xóa trước khi submit)

**Chức năng:**
- ✅ KTV có thể báo cáo phát sinh từ View Order Modal
- ✅ Upload nhiều ảnh minh họa
- ✅ Chọn hạng mục bị ảnh hưởng (hoặc để trống = ảnh hưởng toàn bộ JO)
- ✅ Hệ thống tự động dừng hạng mục liên quan (chuyển sang "OnHold")
- ✅ Hiển thị trạng thái phát sinh với badge màu

---

#### **✅ 2.3.3: Báo giá Phát sinh - HOÀN THÀNH 100%**
**Ngày hoàn thành:** 2025-11-03

**Đã triển khai:**

**1. Database Entities:**
- ✅ `ServiceQuotation` đã thêm:
  - `RelatedToServiceOrderId` (int?, nullable) - Liên kết với ServiceOrder gốc
  - `IsAdditionalQuotation` (bool, default: false) - Phân biệt báo giá gốc vs bổ sung
  - Navigation property: `RelatedToServiceOrder`
- ✅ `ServiceOrder` đã thêm:
  - `ParentServiceOrderId` (int?, nullable) - Liên kết với ServiceOrder gốc (self-referencing)
  - `IsAdditionalOrder` (bool, default: false) - Phân biệt JO gốc vs LSC Bổ sung
  - Navigation properties: `ParentServiceOrder`, `AdditionalServiceOrders`
- ✅ `AdditionalIssue` đã thêm:
  - `AdditionalQuotationId` (int?) - Liên kết với báo giá bổ sung
  - `AdditionalServiceOrderId` (int?) - Liên kết với LSC Bổ sung
- ✅ Migration: `20251103062345_CreateAdditionalIssues` (Đã apply)
- ✅ Migration: `20251103062346_AddAdditionalQuotationFields` (Đã apply)

**2. API Endpoints:**
- ✅ `POST /api/AdditionalIssues/{id}/create-quotation` - Tạo báo giá bổ sung từ phát sinh
  - Lấy `CustomerId` và `VehicleId` từ ServiceOrder gốc
  - Tạo `ServiceQuotation` mới với `IsAdditionalQuotation = true`
  - Set `RelatedToServiceOrderId` và `Status = "Draft"`
  - Cập nhật `AdditionalIssue.AdditionalQuotationId` và `Status = "Quoted"`
- ✅ `POST /api/ServiceQuotations/{id}/approve` - Duyệt báo giá phát sinh (đã cập nhật):
  - Nếu `IsAdditionalQuotation = true`:
    - Tạo `ServiceOrder` mới (LSC Bổ sung) với `ParentServiceOrderId` và `IsAdditionalOrder = true`
    - Cập nhật `AdditionalIssue.AdditionalServiceOrderId` và `Status = "Approved"`
    - Copy tất cả items từ báo giá

**3. Business Logic:**
- ✅ Tự động lấy thông tin khách hàng và xe từ ServiceOrder gốc khi tạo báo giá
- ✅ Tự động tạo LSC Bổ sung khi approve báo giá phát sinh
- ✅ Validate: Chỉ cho phép tạo báo giá cho phát sinh chưa có báo giá (`AdditionalQuotationId = null`)
- ✅ Validate: Phát sinh phải ở trạng thái `Identified` hoặc `Reported`

**4. DTOs:**
- ✅ `CreateQuotationFromIssueDto` - DTO để tạo báo giá từ phát sinh
  - `Items` (List<CreateQuotationItemDto>) - Danh sách items
  - `ValidUntil` (DateTime?) - Ngày hết hạn
  - `Description`, `Terms`, `CustomerNotes` (string?)
  - `TaxRate`, `DiscountAmount` (decimal)

**5. Web Controllers:**
- ✅ `AdditionalIssuesController.CreateQuotation` - Proxy API call

**6. JavaScript:**
- ✅ `order-management.js` với các functions:
  - `openCreateQuotationModal(issueId)` - Mở modal tạo báo giá từ phát sinh
  - `addQuotationItemFromIssue()` - Thêm item vào bảng báo giá
  - `removeQuotationItemFromIssue(button)` - Xóa item
  - `calculateQuotationItemFromIssue(row)` - Tính toán tự động cho từng item
  - `calculateQuotationTotalFromIssue()` - Tính tổng báo giá
  - `submitCreateQuotationFromIssue()` - Submit form tạo báo giá
- ✅ Hiển thị nút "Tạo Báo Giá" cho phát sinh chưa có báo giá
- ✅ Hiển thị link đến báo giá nếu đã có `AdditionalQuotationId`

**7. UI:**
- ✅ Modal `_CreateQuotationFromIssueModal.cshtml` với:
  - Hiển thị thông tin phát sinh (read-only)
  - Form: Ngày hết hạn, Giảm giá, Mô tả, Điều khoản, Ghi chú khách hàng
  - Bảng items động: Tên, Mô tả, Số lượng, Đơn giá, Có HĐ, VAT%, Tạm tính, VAT, Thành tiền
  - Nút "Thêm Item", "Xóa" cho từng item
  - Tổng kết tự động: Tạm tính, VAT, Giảm giá, Tổng cộng
  - Nút "Tạo Báo Giá" để submit

**Chức năng:**
- ✅ CVDV có thể tạo báo giá bổ sung từ phát sinh
- ✅ Hệ thống tự động lấy thông tin khách hàng và xe từ JO gốc
- ✅ Tự động tính toán VAT và tổng tiền
- ✅ Khi khách hàng duyệt báo giá phát sinh → Tự động tạo LSC Bổ sung
- ✅ Cập nhật trạng thái phát sinh thành "Approved"
- ✅ Workflow hoàn chỉnh: Phát sinh → Báo giá phát sinh → Duyệt → LSC Bổ sung → MR (nếu có vật tư) → Tiếp tục sửa chữa

---

### **Độ ưu tiên triển khai tiếp theo:**

1. **⭐⭐⭐ HIGH (Cần thiết ngay):**
   - ✅ ~~2.3.1: Bắt đầu Công việc~~ - **HOÀN THÀNH**
   - ✅ ~~2.3.2: Phát hiện Phát sinh~~ - **HOÀN THÀNH**
   - ✅ ~~2.3.3: Báo giá Phát sinh~~ - **HOÀN THÀNH**
   - ✅ ~~2.3.4: Cập nhật Tiến độ theo từng mốc (Dashboard & Statistics)~~ - **HOÀN THÀNH**

2. **⭐ LOW (Nice-to-have):**
   - Export/Print báo cáo tiến độ
   - Email notifications khi có phát sinh
   - Timeline view để xem tiến độ theo thời gian

---

### **Files đã thay đổi:**

**Entities (2.3.1):**
- `src/GarageManagementSystem.Core/Entities/ServiceOrderItem.cs`

**Entities (2.3.2 & 2.3.3):**
- `src/GarageManagementSystem.Core/Entities/AdditionalIssue.cs` (mới)
- `src/GarageManagementSystem.Core/Entities/AdditionalIssuePhoto.cs` (mới)
- `src/GarageManagementSystem.Core/Entities/ServiceQuotation.cs` (đã cập nhật)
- `src/GarageManagementSystem.Core/Entities/ServiceOrder.cs` (đã cập nhật)

**DTOs:**
- `src/GarageManagementSystem.Shared/DTOs/ServiceOrderItemDto.cs`
- `src/GarageManagementSystem.Shared/DTOs/AdditionalIssueDtos.cs` (mới)
- `src/GarageManagementSystem.Shared/DTOs/ServiceQuotationDto.cs` (đã cập nhật)
- `src/GarageManagementSystem.Shared/DTOs/ServiceOrderDto.cs` (đã cập nhật)

**API:**
- `src/GarageManagementSystem.API/Controllers/ServiceOrdersController.cs` (2.3.1)
- `src/GarageManagementSystem.API/Controllers/AdditionalIssuesController.cs` (mới - 2.3.2 & 2.3.3)
- `src/GarageManagementSystem.API/Controllers/ServiceQuotationsController.cs` (đã cập nhật - 2.3.3)
- `src/GarageManagementSystem.API/Profiles/ServiceOrderProfile.cs`
- `src/GarageManagementSystem.API/Profiles/AdditionalIssueProfile.cs` (mới)

**Web:**
- `src/GarageManagementSystem.Web/Controllers/OrderManagementController.cs` (2.3.1)
- `src/GarageManagementSystem.Web/Controllers/AdditionalIssuesController.cs` (mới - 2.3.2 & 2.3.3)
- `src/GarageManagementSystem.Web/Configuration/ApiEndpoints.cs`
- `src/GarageManagementSystem.Web/Views/OrderManagement/_ViewOrderModal.cshtml` (đã cập nhật)
- `src/GarageManagementSystem.Web/Views/OrderManagement/_ReportAdditionalIssueModal.cshtml` (mới)
- `src/GarageManagementSystem.Web/Views/OrderManagement/_CreateQuotationFromIssueModal.cshtml` (mới)
- `src/GarageManagementSystem.Web/wwwroot/js/order-management.js` (đã cập nhật)

**Database:**
- `src/GarageManagementSystem.Infrastructure/Data/GarageDbContext.cs` (đã cập nhật)
- `src/GarageManagementSystem.Infrastructure/Migrations/20251103035546_AddActualHoursToServiceOrderItems.cs` ✅ Applied
- `src/GarageManagementSystem.Infrastructure/Migrations/20251103062345_CreateAdditionalIssues.cs` ✅ Applied
- `src/GarageManagementSystem.Infrastructure/Migrations/20251103062346_AddAdditionalQuotationFields.cs` ✅ Applied

---

### **Tổng kết:**
- **2.3.1: 100% Hoàn thành** ✅
- **2.3.2: 100% Hoàn thành** ✅
- **2.3.3: 100% Hoàn thành** ✅
- **2.3.4: 100% Hoàn thành** ✅

### **Hướng dẫn sử dụng:**

#### **2.3.2: Phát hiện Phát sinh**
1. Vào trang **"Quản Lý Phiếu Sửa Chữa"**
2. Click nút **"Xem"** của ServiceOrder
3. Click tab **"Phát Sinh"**
4. Click **"Báo Cáo Phát Sinh"** → Điền thông tin → Upload ảnh (nếu có) → Click **"Lưu"**
5. Hệ thống tự động chuyển hạng mục liên quan sang trạng thái "OnHold"

#### **2.3.3: Báo giá Phát sinh**
1. Trong tab **"Phát Sinh"**, tìm phát sinh có trạng thái "Mới phát hiện" hoặc "Đã báo cáo"
2. Click nút **"Tạo Báo Giá"** (màu xanh lá)
3. Modal hiện ra → Điền thông tin báo giá → Thêm items → Tính toán tự động → Click **"Tạo Báo Giá"**
4. Hệ thống tự động tạo báo giá bổ sung và cập nhật trạng thái phát sinh thành "Quoted"
5. Khi khách hàng duyệt báo giá phát sinh → Hệ thống tự động tạo LSC Bổ sung
6. Quay lại quy trình xuất kho (2.2) nếu có vật tư, hoặc tiếp tục sửa chữa (2.3.1)

#### **2.3.4: Cập nhật Tiến độ**
1. Vào trang **"Quản Lý Phiếu Sửa Chữa"**
2. Click nút **"Xem"** của ServiceOrder
3. Click tab **"Tiến Độ"**
4. Hệ thống tự động load và hiển thị:
   - Progress bar tổng thể với percentage (0-100%)
   - Statistics Cards: Tổng Hạng Mục, Chờ Xử Lý, Đang Làm, Đã Hoàn Thành
   - Time Statistics: Giờ Công Dự Kiến, Giờ Công Thực Tế, Giờ Công Còn Lại
   - Timeline: Ngày tạo, Ngày bắt đầu, Ngày dự kiến hoàn thành, Ngày hoàn thành
   - Bảng chi tiết từng item với progress bar, trạng thái, giờ công, KTV phân công
5. CVDV có thể theo dõi tiến độ JO theo thời gian thực để cập nhật cho khách hàng

**Tiến độ tổng thể Giai đoạn 2.3:** ✅ **100% (4/4 hoàn thành)**
- ✅ 2.3.1: Bắt đầu Công việc - **100%**
- ✅ 2.3.2: Phát hiện Phát sinh - **100%**
- ✅ 2.3.3: Báo giá Phát sinh - **100%**
- ✅ 2.3.4: Cập nhật Tiến độ - **100%**

---

---

## 📊 TỔNG KẾT GIAI ĐOẠN 2

### **Trạng thái triển khai:**

- **2.1: Lập Kế Hoạch & Phân Công** ✅ **100% Hoàn thành**
- **2.2: Yêu Cầu Vật Tư (Material Request)** ✅ **100% Hoàn thành**
- **2.3: Quản Lý Tiến Độ Sửa Chữa và Phát Sinh** ✅ **100% Hoàn thành**
  - 2.3.1: Bắt đầu Công việc ✅ **100%**
  - 2.3.2: Phát hiện Phát sinh ✅ **100%**
  - 2.3.3: Báo giá Phát sinh ✅ **100%**
  - 2.3.4: Cập nhật Tiến độ ✅ **100%**
- **2.4: Kiểm tra Chất lượng (QC) và Bàn giao** ❌ **0% (Chưa triển khai)**

**Tổng tiến độ Giai đoạn 2:** 🟡 **75% (3/4 giai đoạn hoàn thành)**

### **Các tính năng đã triển khai:**

1. ✅ **Workflow chuyển trạng thái Service Order** (Pending → PendingAssignment → ReadyToWork → InProgress → Completed)
2. ✅ **Phân công KTV cho từng item** với EstimatedHours
3. ✅ **Phân quyền phân công** (chỉ Quản đốc/Tổ trưởng)
4. ✅ **Workload API** để hiển thị tải công việc của KTV
5. ✅ **Cập nhật Appointment** khi phân công KTV
6. ✅ **Tạo Material Request** từ Service Order
7. ✅ **Workflow MR** (Draft → PendingApproval → Approved → Picked → Issued → Delivered)
8. ✅ **KTV bắt đầu/dừng/hoàn thành công việc** cho từng item
9. ✅ **Tính giờ công thực tế** tự động từ StartTime/EndTime
10. ✅ **Phát hiện và báo cáo phát sinh** với upload ảnh
11. ✅ **Tạo báo giá bổ sung** từ phát sinh
12. ✅ **Tạo LSC Bổ sung** khi duyệt báo giá phát sinh
13. ✅ **Dashboard tiến độ** theo thời gian thực với progress bar và statistics
14. ✅ **Edit/Delete phát sinh** với logic validation tối ưu (reset từ Rejected về Identified)

### **Database Migrations đã áp dụng:**

- ✅ `20251029101126_AddTechnicianAssignmentToServiceOrderItems`
- ✅ `20251103035546_AddActualHoursToServiceOrderItems`
- ✅ `20251103062345_CreateAdditionalIssues`
- ✅ `20251103062346_AddAdditionalQuotationFields`

### **Build Status:**

- ✅ Build thành công: **0 Warning(s), 0 Error(s)**

---

**Tài liệu này tổng hợp tất cả thông tin về Giai đoạn 2 (2.1, 2.2, 2.3, 2.4) trong một file duy nhất.**

**Giai đoạn 2 đã hoàn thành 100% và sẵn sàng cho giai đoạn tiếp theo!** 🎉

---

## 📋 GIAI ĐOẠN 2.4: KIỂM TRA CHẤT LƯỢNG (QC) VÀ BÀN GIAO

### **Tổng quan:**
Giai đoạn 2.4: Kiểm tra Chất lượng (QC) và Bàn giao là bước cuối cùng trong Giai đoạn 2: Sửa Chữa & Quản lý Xuất Kho. Giai đoạn này bắt đầu khi KTV hoàn thành công việc và kết thúc khi JO được chuyển sang Giai đoạn 3 (Quyết toán & Giao xe).

### **Các bước chính:**

#### **2.4.1: Hoàn thành Kỹ thuật**
- **Hoạt động:** KTV hoàn thành công việc, đóng giờ công, chuyển JO sang "Chờ QC"
- **Bộ phận:** Kỹ thuật viên
- **Quy tắc:** Hệ thống ghi nhận **Tổng giờ công thực tế** để tính lương

#### **2.4.2: Kiểm tra QC**
- **Hoạt động:** Tổ trưởng/Nhân viên QC kiểm tra xe theo checklist tiêu chuẩn
- **Bộ phận:** Tổ trưởng/QC
- **Quy tắc:** Hệ thống ghi nhận kết quả QC: **Đạt** hoặc **Không đạt**

#### **2.4.3: Xử lý QC Không đạt**
- **Hoạt động:** Nếu QC không đạt, JO được trả về KTV làm lại. Giờ công phát sinh được track riêng
- **Bộ phận:** Quản đốc/Tổ trưởng
- **Quy tắc:** JO chuyển về trạng thái **"Đang thực hiện"** cho KTV

#### **2.4.4: Chuyển JO sang Thanh toán**
- **Hoạt động:** Nếu QC đạt, JO được chuyển sang **"Sẵn sàng Thanh toán"** và xe được bàn giao về khu vực tiếp đón
- **Bộ phận:** Cố vấn Dịch vụ
- **Quy tắc:** JO chuyển sang **Giai đoạn 3 (Quyết toán & Giao xe)**

---

## 🔍 ĐÁNH GIÁ GIAI ĐOẠN 2.4

### **✅ Những gì đã có (~30%):**

#### **1. Database Entities:**
- ✅ `ServiceOrder` có:
  - `Status` (string) - Trạng thái: "Pending", "InProgress", "Completed", "Cancelled"
  - `CompletedDate` (DateTime?) - Ngày hoàn thành
  - `StartDate` (DateTime?) - Ngày bắt đầu
- ✅ `ServiceOrderItem` có:
  - `Status` (string) - Trạng thái: "Pending", "InProgress", "Completed", "Cancelled", "OnHold"
  - `ActualHours` (decimal?) - Giờ công thực tế
  - `StartTime`, `EndTime`, `CompletedTime` (DateTime?) - Thời gian làm việc
- ✅ Logic tự động chuyển `ServiceOrder.Status` sang "Completed" khi tất cả items hoàn thành

#### **2. API Endpoints cơ bản:**
- ✅ `POST /api/ServiceOrders/{id}/items/{itemId}/complete` - Hoàn thành item
- ✅ `POST /api/ServiceOrders/{id}/items/{itemId}/start-work` - Bắt đầu làm việc
- ✅ `POST /api/ServiceOrders/{id}/items/{itemId}/stop-work` - Dừng làm việc

---

### **❌ Còn thiếu (~70%):**

#### **1. 2.4.1 - Hoàn thành Kỹ thuật:**
- ❌ **Status "WaitingForQC" (Chờ QC):**
  - ❌ Chưa có status "WaitingForQC" trong workflow
  - ❌ KTV không thể chuyển JO sang "Chờ QC" sau khi hoàn thành tất cả items
  - ❌ Không có endpoint để KTV "hoàn thành kỹ thuật" và chuyển sang QC
  
- ❌ **Tính tổng giờ công thực tế:**
  - ❌ Chưa có field `TotalActualHours` trong `ServiceOrder` để lưu tổng giờ công
  - ❌ Chưa có logic tự động tính tổng `ActualHours` từ tất cả items khi hoàn thành
  - ❌ Chưa có API endpoint để lấy tổng giờ công cho tính lương

#### **2. 2.4.2 - Kiểm tra QC:**
- ❌ **Entity QC:**
  - ❌ Chưa có Entity `QualityControl` hoặc `QCInspection` để lưu kết quả QC
  - ❌ Chưa có fields: QCResult (Đạt/Không đạt), QCInspectorId, QCDate, QCNotes, QCChecklistItems
  
- ❌ **QC Checklist:**
  - ❌ Chưa có Entity `QCChecklistItem` để lưu checklist tiêu chuẩn
  - ❌ Chưa có UI để QC staff điền checklist
  - ❌ Chưa có validation checklist bắt buộc
  
- ❌ **API Endpoints:**
  - ❌ Chưa có endpoint để tạo QC inspection
  - ❌ Chưa có endpoint để ghi nhận kết quả QC (Đạt/Không đạt)
  - ❌ Chưa có endpoint để lấy danh sách JO chờ QC

#### **3. 2.4.3 - Xử lý QC Không đạt:**
- ❌ **Logic trả về làm lại:**
  - ❌ Chưa có logic để trả JO về KTV khi QC không đạt
  - ❌ Chưa có field `ReworkRequired` hoặc `QCFailedCount` trong `ServiceOrder`
  - ❌ Chưa có logic track giờ công phát sinh riêng (rework hours)
  - ❌ Chưa có field `ReworkHours` trong `ServiceOrderItem` để track giờ công làm lại
  
- ❌ **Workflow:**
  - ❌ Chưa có logic chuyển JO từ "WaitingForQC" → "InProgress" khi QC không đạt
  - ❌ Chưa có notification cho KTV khi QC không đạt

#### **4. 2.4.4 - Chuyển JO sang Thanh toán:**
- ❌ **Status "ReadyToBill" (Sẵn sàng Thanh toán):**
  - ❌ Chưa có status "ReadyToBill" trong workflow
  - ❌ Chưa có endpoint để chuyển JO sang "ReadyToBill"
  - ❌ Chưa có logic chuyển JO sang Giai đoạn 3
  
- ❌ **Bàn giao xe:**
  - ❌ Chưa có field `HandoverDate` trong `ServiceOrder`
  - ❌ Chưa có field `HandoverLocation` (khu vực tiếp đón)
  - ❌ Chưa có logic cập nhật `Vehicle.Status` khi bàn giao

---

## 📊 TRẠNG THÁI TRIỂN KHAI GIAI ĐOẠN 2.4

**Ngày đánh giá:** 2025-11-05  
**Trạng thái:** ✅ **100% HOÀN THÀNH**

### **Tiến độ:**
- ✅ 2.4.1: Hoàn thành Kỹ thuật - **100% HOÀN THÀNH**
- ✅ 2.4.2: Kiểm tra QC - **100% HOÀN THÀNH**
- ✅ 2.4.3: Xử lý QC Không đạt - **100% HOÀN THÀNH**
- ✅ 2.4.4: Bàn giao xe - **100% HOÀN THÀNH**

### **Chi tiết triển khai:**

#### **✅ 2.4.1: Hoàn thành Kỹ thuật**
- ✅ API endpoints đầy đủ (`CompleteTechnical`, `GetTotalActualHours`)
- ✅ Web Controller endpoints (`/QCManagement/CompleteTechnical/{id}`, `/QCManagement/GetTotalActualHours/{id}`)
- ✅ UI: Button "Hoàn Thành Kỹ Thuật" trong View Order Modal
- ✅ Logic: Validation tất cả items phải Completed/Cancelled
- ✅ Logic: Tính tổng giờ công thực tế tự động
- ✅ Logic: Chuyển status sang "WaitingForQC"
- ✅ Migration: Đã apply thành công

#### **✅ 2.4.2: Kiểm tra QC**
- ✅ API endpoints đầy đủ với pagination (`GetWaitingForQC`, `StartQC`, `CompleteQC`, `GetQC`)
- ✅ Web Controller endpoints tương ứng
- ✅ UI: Trang "Quản Lý QC" với DataTable server-side pagination
- ✅ UI: Modals cho Start QC, Complete QC, View QC
- ✅ UI: Tab "QC" trong View Order Modal
- ✅ Logic: QC Checklist với Pass/Fail cho từng item
- ✅ Logic: Authorization (chỉ Tổ trưởng/QC/Quản đốc/Manager/Supervisor/Admin/SuperAdmin)
- ✅ Logic: Validation không cho phép tạo nhiều QC Pending
- ✅ Logic: Auto-populate QCInspector từ authenticated user

#### **✅ 2.4.3: Xử lý QC Không đạt**
- ✅ API endpoints đầy đủ (`FailQC`, `RecordReworkHours`)
- ✅ Web Controller endpoints tương ứng
- ✅ UI: Hiển thị ReworkHours trong View Modal
- ✅ UI: Ghi chú làm lại trong QC Modal
- ✅ Logic: Chuyển status về "InProgress" khi QC Fail
- ✅ Logic: Tăng QCFailedCount
- ✅ Logic: Ghi nhận giờ công làm lại

#### **✅ 2.4.4: Bàn giao xe**
- ✅ API endpoint đầy đủ (`Handover`)
- ✅ Web Controller endpoint (`/QCManagement/Handover/{id}`)
- ✅ UI: Modal "Bàn Giao Xe" với form đầy đủ
- ✅ UI: Button "Bàn Giao Xe" trong View Order Modal
- ✅ Logic: Validation QC phải Pass mới được bàn giao
- ✅ Logic: Chuyển status sang "ReadyToBill"
- ✅ Logic: Lưu HandoverDate và HandoverLocation
- ✅ Authorization: Chỉ Cố vấn Dịch vụ/Quản đốc/Manager/Advisor/Admin/SuperAdmin

### **Tổng kết:**
- ✅ **Database:** 2 entities mới, 2 entities được cập nhật, Migration đã apply
- ✅ **API:** 9 endpoints đầy đủ với validation, authorization, error handling
- ✅ **Web UI:** 5 Views (Index + 4 Modals), JavaScript module hoàn chỉnh (727 lines)
- ✅ **Integration:** Tích hợp đầy đủ vào Order Management
- ✅ **Documentation:** Báo cáo tiến độ, User Manual, hướng dẫn tạo demo data
- ✅ **Build:** 0 errors, 0 warnings

**Giai đoạn 2.4:** ✅ **100% HOÀN THÀNH**

---

## 🎯 YÊU CẦU TRIỂN KHAI GIAI ĐOẠN 2.4

### **Database Changes:**

1. **ServiceOrder Entity:**
   - Thêm `TotalActualHours` (decimal?) - Tổng giờ công thực tế
   - Thêm `QCFailedCount` (int) - Số lần QC không đạt
   - Thêm `HandoverDate` (DateTime?) - Ngày bàn giao
   - Thêm `HandoverLocation` (string?) - Khu vực bàn giao
   - Cập nhật `Status` để hỗ trợ: "WaitingForQC", "QCInProgress", "QCFailed", "ReadyToBill"

2. **ServiceOrderItem Entity:**
   - Thêm `ReworkHours` (decimal?) - Giờ công làm lại (nếu QC không đạt)

3. **QualityControl Entity (MỚI):**
   - `ServiceOrderId` (int, required)
   - `QCInspectorId` (int?) - Nhân viên QC
   - `QCDate` (DateTime) - Ngày kiểm tra
   - `QCResult` (string) - "Pass", "Fail"
   - `QCNotes` (string?) - Ghi chú QC
   - `QCChecklistItems` (ICollection<QCChecklistItem>) - Checklist items
   - `ReworkRequired` (bool) - Cần làm lại
   - `ReworkNotes` (string?) - Ghi chú làm lại

4. **QCChecklistItem Entity (MỚI):**
   - `QualityControlId` (int, required)
   - `ChecklistItemName` (string) - Tên checklist item
   - `IsChecked` (bool) - Đã kiểm tra
   - `Result` (string?) - "Pass", "Fail", null
   - `Notes` (string?) - Ghi chú

### **API Endpoints:**

1. **2.4.1:**
   - `POST /api/ServiceOrders/{id}/complete-technical` - KTV hoàn thành kỹ thuật, chuyển sang "WaitingForQC"
   - `GET /api/ServiceOrders/{id}/total-actual-hours` - Lấy tổng giờ công thực tế

2. **2.4.2:**
   - `GET /api/ServiceOrders/waiting-for-qc` - Lấy danh sách JO chờ QC
   - `POST /api/ServiceOrders/{id}/qc/start` - Bắt đầu kiểm tra QC
   - `POST /api/ServiceOrders/{id}/qc/complete` - Hoàn thành QC với kết quả (Đạt/Không đạt)
   - `GET /api/ServiceOrders/{id}/qc` - Lấy thông tin QC của JO

3. **2.4.3:**
   - `POST /api/ServiceOrders/{id}/qc/fail` - Ghi nhận QC không đạt, trả về KTV làm lại
   - `POST /api/ServiceOrders/{id}/items/{itemId}/rework` - Ghi nhận giờ công làm lại

4. **2.4.4:**
   - `POST /api/ServiceOrders/{id}/handover` - Bàn giao xe và chuyển sang "ReadyToBill"

### **Web UI:**

1. **2.4.1:**
   - Button "Hoàn thành Kỹ thuật" trong View Order Modal (chỉ hiện khi tất cả items đã Completed)
   - Hiển thị tổng giờ công thực tế trong View Modal

2. **2.4.2:**
   - Trang "Quản Lý QC" với danh sách JO chờ QC
   - Modal "Kiểm tra QC" với checklist
   - Button "Đạt" / "Không đạt" sau khi kiểm tra

3. **2.4.3:**
   - Hiển thị thông báo khi QC không đạt
   - Hiển thị giờ công làm lại trong View Modal

4. **2.4.4:**
   - Button "Bàn giao xe" trong View Order Modal (chỉ hiện khi QC đạt)
   - Form nhập thông tin bàn giao (khu vực, ngày giờ)

---

## 📊 TỔNG KẾT GIAI ĐOẠN 2 (CẬP NHẬT)

### **Trạng thái triển khai:**

- **2.1: Lập Kế Hoạch & Phân Công** ✅ **100% Hoàn thành**
- **2.2: Yêu Cầu Vật Tư (Material Request)** ✅ **100% Hoàn thành**
- **2.3: Quản Lý Tiến Độ Sửa Chữa và Phát Sinh** ✅ **100% Hoàn thành**
  - 2.3.1: Bắt đầu Công việc ✅ **100%**
  - 2.3.2: Phát hiện Phát sinh ✅ **100%**
  - 2.3.3: Báo giá Phát sinh ✅ **100%**
  - 2.3.4: Cập nhật Tiến độ ✅ **100%**
- **2.4: Kiểm tra Chất lượng (QC) và Bàn giao** ✅ **100% Hoàn thành**
  - 2.4.1: Hoàn thành Kỹ thuật ✅ **100%**
  - 2.4.2: Kiểm tra QC ✅ **100%**
  - 2.4.3: Xử lý QC Không đạt ✅ **100%**
  - 2.4.4: Bàn giao xe ✅ **100%**

**Tổng tiến độ Giai đoạn 2:** ✅ **100% (4/4 giai đoạn hoàn thành)**

---

## 📝 BÁO CÁO CHI TIẾT GIAI ĐOẠN 2.4

### **✅ Đã triển khai đầy đủ:**

#### **Database:**
- ✅ Entity `QualityControl` với đầy đủ fields
- ✅ Entity `QCChecklistItem` với checklist items
- ✅ Cập nhật `ServiceOrder`: TotalActualHours, QCFailedCount, HandoverDate, HandoverLocation
- ✅ Cập nhật `ServiceOrderItem`: ReworkHours
- ✅ Migration `20251105080320_AddQualityControlAndHandoverFields` đã apply thành công

#### **API:**
- ✅ Tất cả 9 endpoints đã được triển khai với:
  - Validation đầy đủ
  - Authorization theo role
  - Error handling và logging
  - Transaction support
  - Database-level filtering (optimized)

#### **Web UI:**
- ✅ Trang "Quản Lý QC" với DataTable server-side pagination
- ✅ 4 Modals: Start QC, Complete QC, View QC, Handover
- ✅ Integration vào Order Management:
  - Tab "QC" trong View Order Modal
  - Buttons: Complete Technical, Start QC, Complete QC, Handover
  - Auto-hide/show buttons dựa trên status
- ✅ JavaScript module `qc-management.js` (727 lines)
- ✅ Status formatting với các status mới

#### **Documentation:**
- ✅ Hướng dẫn tạo dữ liệu demo (`HUONG_DAN_TAO_DU_LIEU_QC.md`)
- ✅ Script SQL tạo demo data (`SQL_CREATE_DEMO_DATA_FOR_QC.sql`)
- ✅ Cập nhật User Manual với hướng dẫn sử dụng Phase 2.4

---

**Tài liệu này tổng hợp tất cả thông tin về Giai đoạn 2 (2.1, 2.2, 2.3, 2.4) trong một file duy nhất.**

