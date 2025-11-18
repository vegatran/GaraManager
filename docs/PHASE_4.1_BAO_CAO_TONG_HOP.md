# 📊 BÁO CÁO TỔNG HỢP PHASE 4.1 - HỆ THỐNG QUẢN LÝ GARAGE Ô TÔ

**Ngày cập nhật:** 2025-01-XX  
**Phiên bản:** Phase 4.1  
**Trạng thái tổng thể:** 🟢 **92.5% hoàn thành** (Sprint 1: 95%, Sprint 2: 90%)

---

## 📋 MỤC LỤC

1. [Tổng Quan Phase 4.1](#tổng-quan-phase-41)
2. [Sprint 1: Chuẩn Hóa Danh Mục Phụ Tùng & Quản Lý Vị Trí Kho](#sprint-1-chuẩn-hóa-danh-mục-phụ-tùng--quản-lý-vị-trí-kho)
   - [2.1. Tiến Độ Chi Tiết](#21-tiến-độ-chi-tiết)
   - [2.2. Bug Fixes](#22-bug-fixes)
   - [2.3. Còn Thiếu](#23-còn-thiếu)
3. [Sprint 2: Cảnh Báo Tồn Kho & Kiểm Kê Định Kỳ](#sprint-2-cảnh-báo-tồn-kho--kiểm-kê-định-kỳ)
   - [3.1. Tiến Độ Chi Tiết](#31-tiến-độ-chi-tiết)
   - [3.2. Test Manual](#32-test-manual)
   - [3.3. Còn Thiếu](#33-còn-thiếu)
4. [Files Đã Tạo/Sửa](#files-đã-tạosửa)
5. [Kế Hoạch Tiếp Theo](#kế-hoạch-tiếp-theo)
6. [Tài Liệu Tham Khảo](#tài-liệu-tham-khảo)

---

## 🎯 TỔNG QUAN PHASE 4.1

### **Mục Tiêu**
Phase 4.1 tập trung vào việc **chuẩn hóa quản lý phụ tùng** và **nâng cao khả năng quản lý kho**, bao gồm:

1. **Sprint 1:** Chuẩn hóa danh mục phụ tùng & Quản lý vị trí kho
2. **Sprint 2:** Cảnh báo tồn kho & Kiểm kê định kỳ

### **Trạng Thái Tổng Thể**
- **Sprint 1:** ✅ **95% hoàn thành**
- **Sprint 2:** 🟢 **90% hoàn thành**
- **Tổng thể:** 🟢 **92.5% hoàn thành**

### **Tính Năng Chính Đã Hoàn Thành**
- ✅ Quản lý SKU, Barcode cho phụ tùng
- ✅ Quản lý đơn vị quy đổi (PartUnits)
- ✅ Quản lý vị trí kho (Warehouse/Zone/Bin)
- ✅ Quản lý Warehouse (Kho) riêng biệt
- ✅ Cảnh báo tồn kho thấp/hết hàng
- ✅ Kiểm kê định kỳ (Inventory Checks)
- ✅ Điều chỉnh tồn kho (Inventory Adjustments)

---

## 📦 SPRINT 1: CHUẨN HÓA DANH MỤC PHỤ TÙNG & QUẢN LÝ VỊ TRÍ KHO

**Trạng thái:** ✅ **95% hoàn thành**

### **2.1. TIẾN ĐỘ CHI TIẾT**

#### **2.1.1. Database Schema & Migration** ✅ **100%**

**Entities đã tạo:**
- ✅ **Part.Sku** (string, max 100, unique index)
- ✅ **Part.Barcode** (string, max 150, unique index)
- ✅ **Part.DefaultUnit** (string, max 20) - Thay thế `Part.Unit` (obsolete)
- ✅ **PartUnit** entity:
  - `UnitName` (string, max 50)
  - `ConversionRate` (decimal 18,4) - Hệ số quy đổi so với DefaultUnit
  - `Barcode` (string, max 150) - Mã vạch riêng cho đơn vị
  - `IsDefault` (bool) - Đánh dấu đơn vị mặc định
  - Unique index: `(PartId, UnitName)`
- ✅ **Warehouse** entity:
  - `Code` (string, max 50, unique)
  - `Name` (string, max 150)
  - `Address`, `ManagerName`, `PhoneNumber`
  - `IsDefault`, `IsActive`
- ✅ **WarehouseZone** entity:
  - `WarehouseId` (FK)
  - `Code` (string, max 50)
  - `Name` (string, max 150)
  - `DisplayOrder`, `IsActive`
  - Unique index: `(WarehouseId, Code)`
- ✅ **WarehouseBin** entity:
  - `WarehouseId` (FK)
  - `WarehouseZoneId` (FK, nullable)
  - `Code` (string, max 50)
  - `Name` (string, max 150)
  - `Capacity` (decimal, nullable)
  - `IsDefault`, `IsActive`
  - Unique index: `(WarehouseId, Code)`
- ✅ **PartInventoryBatch** relationships:
  - `WarehouseId`, `WarehouseZoneId`, `WarehouseBinId` (FK, nullable, SetNull on delete)

**Migration:**
- ✅ Migration: `20251111062333_20251111041000_AddWarehouseAndSku.cs`
- ✅ Database schema đã được tạo thành công
- ⚠️ **Chưa có seed data** (sẽ làm sau khi hoàn thành Phase 4.1)

---

#### **2.1.2. Backend API & Services** ✅ **100%**

**PartsController:**
- ✅ **GET /api/parts**: Lấy danh sách parts với SKU, Barcode, DefaultUnit, PartUnits
- ✅ **GET /api/parts/{id}**: Lấy part details với PartUnits
- ✅ **POST /api/parts**: Tạo part mới với SKU, Barcode, DefaultUnit, PartUnits
- ✅ **PUT /api/parts/{id}**: Cập nhật part với SKU, Barcode, DefaultUnit, PartUnits
- ✅ **Validation**:
  - Unique SKU (nếu có)
  - Unique Barcode (nếu có)
  - Đảm bảo DefaultUnit có trong PartUnits với IsDefault=true
  - Chỉ 1 PartUnit có IsDefault=true tại một thời điểm
- ✅ **Logic đồng bộ DefaultUnit ↔ PartUnits**:
  - Nếu DefaultUnit được set → Tự động thêm vào PartUnits nếu chưa có
  - Nếu PartUnit.IsDefault=true → Tự động cập nhật DefaultUnit
  - Đảm bảo consistency 2 chiều

**WarehousesController:**
- ✅ **GET /api/warehouses**: Lấy danh sách warehouses với Zones, Bins
- ✅ **GET /api/warehouses/{id}**: Lấy warehouse details
- ✅ **POST /api/warehouses**: Tạo warehouse mới
- ✅ **PUT /api/warehouses/{id}**: Cập nhật warehouse
- ✅ **DELETE /api/warehouses/{id}**: Xóa warehouse (soft delete)
- ✅ **Zones & Bins endpoints**: CRUD cho Zones và Bins

**DTOs & AutoMapper:**
- ✅ **PartDto**: Thêm `Sku`, `Barcode`, `DefaultUnit`, `List<PartUnitDto> Units`
- ✅ **PartUnitDto**: `UnitName`, `ConversionRate`, `Barcode`, `IsDefault`
- ✅ **WarehouseDto**: Nested `Zones[]`, `Bins[]`
- ✅ **WarehouseZoneDto**: Nested `Bins[]`
- ✅ **WarehouseBinDto**: Đầy đủ các trường
- ✅ **AutoMapper Profiles**: Mapping đầy đủ cho tất cả entities

**Repositories & Services:**
- ✅ **PartRepository**: `GetWithDetailsAsync()` - Include PartUnits
- ✅ **WarehouseRepository**: CRUD operations
- ✅ **ExcelImportService**: Cập nhật để sử dụng `DefaultUnit` và tạo `PartUnit`
- ✅ **StockTransactionsController**: Cập nhật để sử dụng `DefaultUnit` và `PartUnit`

---

#### **2.1.3. Frontend UI** ✅ **100%**

**Parts Management UI:**
- ✅ **Create Part Modal**:
  - Input fields: SKU, Barcode, DefaultUnit
  - Warehouse location dropdowns: Kho → Khu vực → Kệ/Ngăn
  - Auto-fill location field khi chọn kho/khu/ngăn
  - Tab "Đơn Vị Quy Đổi" với form inline (không dùng SweetAlert)
- ✅ **Edit Part Modal**:
  - Tương tự Create Modal
  - Load existing PartUnits vào bảng
  - Load warehouse location từ part data
- ✅ **View Part Modal**:
  - Hiển thị SKU, Barcode, DefaultUnit
  - Hiển thị danh sách PartUnits với conversion rates
  - Hiển thị warehouse location
- ✅ **Parts List Table**:
  - Column: SKU (hiển thị "-" nếu không có)
  - Column: DefaultUnit (hiển thị "-" nếu không có)
  - DataTables với server-side pagination

**Warehouse Management UI:**
- ✅ **WarehouseManagementController**: Proxy actions cho API
- ✅ **WarehouseManagement Views**:
  - Index.cshtml: Danh sách warehouses với DataTables
  - _CreateWarehouseModal.cshtml: Modal tạo warehouse với tabs (Basic Info, Zones, Bins)
  - _EditWarehouseModal.cshtml: Modal sửa warehouse với tabs
  - _ViewWarehouseModal.cshtml: Modal xem chi tiết warehouse
- ✅ **warehouse-management.js**: Logic CRUD đầy đủ
- ✅ **Sidebar Menu**: Đã thêm menu "Kho" (Warehouse) vào sidebar

**Đơn Vị Quy Đổi UI:**
- ✅ **Form inline** (không dùng SweetAlert)
- ✅ **Bảng hiển thị** với đầy đủ logic CRUD
- ✅ **Validation** đầy đủ

**Warehouse Location UI:**
- ✅ **Dropdown hierarchy**: Kho → Khu vực → Kệ/Ngăn
- ✅ **Auto-fill location**: Tự động điền "Ghi chú vị trí"
- ✅ **Dynamic loading**: Zones và Bins load theo Warehouse được chọn

---

#### **2.1.4. Testing & Build** ✅ **100%**

- ✅ **Build**: `dotnet build` thành công, không có lỗi
- ✅ **Linter**: Không có lỗi
- ✅ **Migration**: Đã apply thành công vào database
- ⚠️ **Seed data**: Chưa có (sẽ làm sau khi hoàn thành Phase 4.1)

---

### **2.2. BUG FIXES**

#### **✅ BUG 1: Validation SKU/Barcode Unique ở API Level**

**Vấn đề:**
- Chỉ có unique index ở database level
- Không có validation ở API level trước khi save
- User chỉ biết lỗi khi database throw exception (khó hiểu)

**Fix:**
- ✅ **Thêm validation SKU unique** trong `CreatePart()` và `UpdatePart()`
- ✅ **Thêm validation Barcode unique**
- ✅ **Normalize SKU và Barcode**: Trim whitespace, set null thay vì empty string
- ✅ **Handle database unique constraint violation**: Catch `DbUpdateException` với "Duplicate entry"

**Files đã sửa:**
- `src/GarageManagementSystem.API/Controllers/PartsController.cs`

---

#### **✅ BUG 2: Logic Đồng Bộ DefaultUnit ↔ PartUnits**

**Vấn đề:**
- Logic đồng bộ DefaultUnit ↔ PartUnits có thể không đúng
- Có thể có nhiều PartUnits có IsDefault=true cùng lúc
- Nếu DefaultUnit không có trong PartUnits, không tự động tạo mới

**Fix:**
- ✅ **Đảm bảo chỉ 1 PartUnit có IsDefault=true**
- ✅ **Tự động tạo PartUnit nếu DefaultUnit không có trong PartUnits**
- ✅ **Đồng bộ DefaultUnit với UnitName**

**Files đã sửa:**
- `src/GarageManagementSystem.API/Controllers/PartsController.cs` - Method `EnsureDefaultUnit()`

---

#### **✅ BUG 3: Xử Lý Unique Index với NULL Values**

**Vấn đề:**
- Unique index cho SKU và Barcode không handle NULL values đúng cách
- Empty string ("") có thể bị unique constraint

**Fix:**
- ✅ **Normalize SKU và Barcode**: Trim whitespace, set null thay vì empty string
- ✅ **Validation chỉ check nếu không null/empty**

**Files đã sửa:**
- `src/GarageManagementSystem.API/Controllers/PartsController.cs` - Methods `CreatePart()`, `UpdatePart()`

---

#### **⚠️ BUG 4: Warehouse Location - PartInventoryBatch Relationship**

**Kết luận:**
- ✅ **Đây KHÔNG PHẢI là bug** - Đây là design đúng:
  - Part.Location chỉ là ghi chú vị trí dự kiến
  - PartInventoryBatch chứa vị trí thực tế khi nhập kho
  - PartInventoryBatch được tạo khi nhập kho, không phải khi tạo part

---

### **2.3. CÒN THIẾU (5%)**

1. **❌ Seed Data Warehouse** (Ưu tiên: Thấp)
   - Database chưa có warehouse mặc định
   - User phải tự tạo warehouse từ UI (đã có UI)
   - **Ghi chú:** User đã nói sẽ làm seed data sau khi hoàn thành Phase 4.1

2. **❌ Testing** (Ưu tiên: Trung bình)
   - Chưa có unit tests cho các services mới
   - Chưa có integration tests cho API endpoints
   - Chưa có E2E tests cho UI workflows
   - **Ghi chú:** Có thể test manual trước, unit tests làm sau

3. **❌ Documentation** (Ưu tiên: Thấp)
   - Chưa có user manual cho warehouse management
   - Chưa có API documentation chi tiết cho warehouse endpoints
   - Chưa có hướng dẫn sử dụng warehouse location trong Parts Management
   - **Ghi chú:** Có thể làm sau khi release

---

## 🚨 SPRINT 2: CẢNH BÁO TỒN KHO & KIỂM KÊ ĐỊNH KỲ

**Trạng thái:** 🟢 **90% hoàn thành**

### **3.1. TIẾN ĐỘ CHI TIẾT**

#### **3.1.1. Minimum Stock Levels & Alerts** 🟢 **75%**

**Database Schema:** ✅ **100%**
- `Part.MinimumStock` và `Part.ReorderLevel` đã có trong schema
- `InventoryAlert` entity đã có đầy đủ

**Backend API:** ✅ **80%**
- ✅ GET `/api/inventory-alerts/low-stock` - Lấy danh sách parts dưới mức tồn kho tối thiểu
- ✅ GET `/api/inventory-alerts/out-of-stock` - Lấy danh sách parts hết hàng
- ✅ GET `/api/inventory-alerts/overstock` - Lấy danh sách parts tồn kho cao
- ✅ GET `/api/inventory-alerts/reorder-suggestions` - Gợi ý đặt hàng lại
- ✅ GET `/api/inventory-alerts/expiring-soon` - Cảnh báo hết hạn sớm
- ✅ GET `/api/inventory-alerts/GetAlertsCount` - Lấy tổng số alerts

**Frontend UI:** ✅ **100%**
- ✅ Dashboard Widget hiển thị low stock alerts
- ✅ InventoryAlerts Page với DataTables
- ✅ Parts Management UI (MinimumStock & ReorderLevel fields trong Create/Edit Part Modal)
- ✅ Badge count trên menu (auto-update mỗi 30 giây)
- ✅ Filter theo alert type và severity
- ✅ Mark as resolved / Mark all as resolved

**Còn Thiếu:**
- ⚠️ Real-time notifications (ưu tiên thấp)
- ⚠️ Background job (ưu tiên thấp)
- ⚠️ Testing (ưu tiên trung bình)
- ⚠️ Documentation (ưu tiên thấp)

---

#### **3.1.2. Periodic Inventory Checks** 🟢 **98%**

**Database Entities:** ✅ **100%**
- `InventoryCheck` entity với đầy đủ fields
- `InventoryCheckItem` entity
- Migration: `20251113071933_AddInventoryCheckEntities`

**Backend API:** ✅ **100%**
- ✅ POST `/api/inventory-checks` - Tạo phiếu kiểm kê mới
- ✅ GET `/api/inventory-checks` - Lấy danh sách (với filters)
- ✅ GET `/api/inventory-checks/{id}` - Lấy chi tiết
- ✅ PUT `/api/inventory-checks/{id}` - Cập nhật
- ✅ DELETE `/api/inventory-checks/{id}` - Xóa (soft delete)
- ✅ POST `/api/inventory-checks/{id}/complete` - Hoàn thành kiểm kê
- ✅ POST `/api/inventory-checks/{id}/items` - Thêm item
- ✅ PUT `/api/inventory-checks/{id}/items/{itemId}` - Cập nhật item
- ✅ DELETE `/api/inventory-checks/{id}/items/{itemId}` - Xóa item
- ✅ Auto-generate Code (IK-YYYY-NNN format)
- ✅ Auto-calculation: SystemQuantity, DiscrepancyQuantity, IsDiscrepancy
- ✅ Status workflow: Draft → InProgress → Completed

**Frontend UI:** ✅ **98%**
- ✅ Inventory Checks Management Page với DataTables
- ✅ Create/Edit/View Modals
- ✅ Items Management với typeahead search
- ✅ Export/Print Features: **100%** ✅
  - ✅ Export danh sách phiếu kiểm kê ra Excel (với filters)
  - ✅ Export chi tiết phiếu kiểm kê ra Excel (với items và statistics)
  - ✅ Print phiếu kiểm kê (print-friendly format)
- ✅ Integration với Inventory Adjustment: **100%** ✅
  - ✅ Button "Tạo Điều Chỉnh" trong Inventory Check View Modal
  - ✅ Tự động tạo adjustment từ check items có discrepancy

**Còn Thiếu:**
- ⏳ Advanced Features (bulk operations, duplicate, history) - ưu tiên thấp
- ⏳ Testing - ưu tiên trung bình
- ⏳ Documentation - ưu tiên thấp

---

#### **3.1.3. Discrepancy Handling (Inventory Adjustment)** 🟢 **100%** ✅

**Database Entities:** ✅ **100%**
- `InventoryAdjustment` entity
- `InventoryAdjustmentItem` entity
- One-to-one relationship với `InventoryCheckItem`

**Backend API:** ✅ **100%**
- ✅ GET `/api/inventory-adjustments` - Lấy danh sách (với filters)
- ✅ GET `/api/inventory-adjustments/{id}` - Lấy chi tiết
- ✅ POST `/api/inventory-adjustments` - Tạo thủ công
- ✅ POST `/api/inventory-adjustments/from-check/{checkId}` - Tạo từ Inventory Check
- ✅ PUT `/api/inventory-adjustments/{id}/approve` - Duyệt
- ✅ PUT `/api/inventory-adjustments/{id}/reject` - Từ chối
- ✅ DELETE `/api/inventory-adjustments/{id}` - Xóa (soft delete)
- ✅ Auto-generate AdjustmentNumber (ADJ-YYYY-NNN format)
- ✅ Tự động tạo StockTransaction khi approve
- ✅ Tự động cập nhật Part.QuantityInStock khi approve
- ✅ Validation đầy đủ (negative stock, part deleted, duplicate approval)

**Frontend UI:** ✅ **100%**
- ✅ Inventory Adjustments Management Page
- ✅ View Modal với items table
- ✅ Create From Check Modal
- ✅ Create Manual Modal
- ✅ Item Modal cho Create Manual
- ✅ Approve/Reject workflow
- ✅ JavaScript module (`inventory-adjustments.js`)
- ✅ Sidebar menu integration

**Integration:** ✅ **100%**
- ✅ Tích hợp với Inventory Checks
- ✅ Button "Tạo Điều Chỉnh" trong Inventory Check View Modal

---

### **3.2. TEST MANUAL**

**Test Cases chính:**
1. ✅ **Parts Management UI** - MinimumStock & ReorderLevel
2. ✅ **Validation Logic** - Client-side và Server-side
3. ✅ **Visual Indicators** - Hiển thị cảnh báo trong View Part Modal
4. ✅ **Badge Count** - Hiển thị số lượng alerts trên menu
5. ✅ **GetAlertsCount Endpoint** - API endpoint để lấy tổng số alerts
6. ✅ **Inventory Checks** - Tạo, thêm items, hoàn thành
7. ✅ **Inventory Adjustments** - Tạo từ check, tạo thủ công, duyệt/từ chối

**Chi tiết test cases:** Xem phần [Test Manual](#test-manual-chi-tiết) bên dưới

---

### **3.3. CÒN THIẾU (10%)**

#### **1. Advanced Features cho Inventory Checks** ⏳
**Ưu tiên:** ⭐⭐ (Thấp)  
**Thời gian ước tính:** 3.5-4.5 ngày

- ⏳ Bulk Operations (1 ngày)
- ⏳ Duplicate Check (0.5 ngày)
- ⏳ History/Audit Trail (1-2 ngày)
- ⏳ Comments/Notes Timeline (1 ngày)

#### **2. Testing** ⏳
**Ưu tiên:** ⭐⭐⭐ (Trung bình)  
**Thời gian ước tính:** 5.5-8 ngày

- ⏳ Unit Tests (2-3 ngày)
- ⏳ Integration Tests (2-3 ngày)
- ⏳ E2E Tests (1-2 ngày)
- ⏳ Manual Testing Checklist (0.5-1 ngày)

#### **3. Real-time Features** ⏳
**Ưu tiên:** ⭐ (Rất thấp)  
**Thời gian ước tính:** 3-5 ngày

- ⏳ Real-time Notifications (2-3 ngày)
- ⏳ Background Job (1-2 ngày)

#### **4. Documentation** ⏳
**Ưu tiên:** ⭐⭐ (Thấp)  
**Thời gian ước tính:** 2-3 ngày

- ⏳ User Manual (1 ngày) - ✅ **Đã hoàn thành**
- ⏳ API Documentation (0.5-1 ngày)
- ⏳ Technical Documentation (0.5-1 ngày)

---

## 📝 FILES ĐÃ TẠO/SỬA

### **Backend:**

**Sprint 1:**
- ✅ `src/GarageManagementSystem.Core/Entities/PartUnit.cs`
- ✅ `src/GarageManagementSystem.Core/Entities/Warehouse.cs`
- ✅ `src/GarageManagementSystem.Core/Entities/WarehouseZone.cs`
- ✅ `src/GarageManagementSystem.Core/Entities/WarehouseBin.cs`
- ✅ `src/GarageManagementSystem.API/Controllers/PartsController.cs` (updated)
- ✅ `src/GarageManagementSystem.API/Controllers/WarehousesController.cs`
- ✅ `src/GarageManagementSystem.Shared/DTOs/PartDto.cs` (updated)
- ✅ `src/GarageManagementSystem.Shared/DTOs/PartUnitDto.cs`
- ✅ `src/GarageManagementSystem.Shared/DTOs/WarehouseDto.cs`
- ✅ `src/GarageManagementSystem.Infrastructure/Data/GarageDbContext.cs` (updated)
- ✅ `src/GarageManagementSystem.Infrastructure/Repositories/UnitOfWork.cs` (updated)

**Sprint 2:**
- ✅ `src/GarageManagementSystem.Core/Entities/InventoryAdjustment.cs`
- ✅ `src/GarageManagementSystem.Core/Entities/InventoryAdjustmentItem.cs`
- ✅ `src/GarageManagementSystem.API/Controllers/InventoryAdjustmentsController.cs`
- ✅ `src/GarageManagementSystem.API/Controllers/InventoryAlertsController.cs` (updated)
- ✅ `src/GarageManagementSystem.API/Controllers/InventoryChecksController.cs` (updated - Export Excel)
- ✅ `src/GarageManagementSystem.Shared/DTOs/InventoryAdjustmentDto.cs`
- ✅ `src/GarageManagementSystem.Core/Entities/InventoryCheckItem.cs` (updated - InventoryAdjustmentItemId)
- ✅ `src/GarageManagementSystem.Infrastructure/Data/GarageDbContext.cs` (updated)

### **Frontend:**

**Sprint 1:**
- ✅ `src/GarageManagementSystem.Web/Controllers/WarehouseManagementController.cs`
- ✅ `src/GarageManagementSystem.Web/Views/WarehouseManagement/Index.cshtml`
- ✅ `src/GarageManagementSystem.Web/Views/WarehouseManagement/_CreateWarehouseModal.cshtml`
- ✅ `src/GarageManagementSystem.Web/Views/WarehouseManagement/_EditWarehouseModal.cshtml`
- ✅ `src/GarageManagementSystem.Web/Views/WarehouseManagement/_ViewWarehouseModal.cshtml`
- ✅ `src/GarageManagementSystem.Web/wwwroot/js/warehouse-management.js`
- ✅ `src/GarageManagementSystem.Web/Views/PartsManagement/Index.cshtml` (updated)
- ✅ `src/GarageManagementSystem.Web/wwwroot/js/parts-management.js` (updated)
- ✅ `src/GarageManagementSystem.Web/Views/Shared/_SidebarMenu.cshtml` (updated)

**Sprint 2:**
- ✅ `src/GarageManagementSystem.Web/Controllers/InventoryAdjustmentsController.cs`
- ✅ `src/GarageManagementSystem.Web/Controllers/InventoryAlertsController.cs` (updated)
- ✅ `src/GarageManagementSystem.Web/Views/InventoryAdjustments/Index.cshtml`
- ✅ `src/GarageManagementSystem.Web/Views/InventoryAdjustments/_ViewModal.cshtml`
- ✅ `src/GarageManagementSystem.Web/Views/InventoryAdjustments/_CreateFromCheckModal.cshtml`
- ✅ `src/GarageManagementSystem.Web/Views/InventoryAdjustments/_CreateModal.cshtml`
- ✅ `src/GarageManagementSystem.Web/Views/InventoryAdjustments/_ItemModal.cshtml`
- ✅ `src/GarageManagementSystem.Web/wwwroot/js/inventory-adjustments.js`
- ✅ `src/GarageManagementSystem.Web/Views/InventoryChecks/Index.cshtml` (updated - Export Excel button)
- ✅ `src/GarageManagementSystem.Web/Views/InventoryChecks/_ViewModal.cshtml` (updated - Export Excel, Print, Tạo Điều Chỉnh)
- ✅ `src/GarageManagementSystem.Web/wwwroot/js/inventory-checks.js` (updated - Export/Print functions)
- ✅ `src/GarageManagementSystem.Web/Configuration/ApiEndpoints.cs` (updated)
- ✅ `src/GarageManagementSystem.Web/Views/Shared/_SidebarMenu.cshtml` (updated)

### **Migrations:**
- ✅ `20251111062333_20251111041000_AddWarehouseAndSku.cs`
- ✅ `20251113071933_AddInventoryCheckEntities.cs`
- ✅ Migration cho InventoryAdjustment entities

---

## 🎯 KẾ HOẠCH TIẾP THEO

### **Phase 1: Documentation** ✅ **100% HOÀN THÀNH**
1. ✅ User Manual (1 ngày) - **Đã hoàn thành**
2. ✅ API Documentation (0.5-1 ngày) - **Đã hoàn thành**
3. ✅ Technical Documentation (0.5-1 ngày) - **Đã hoàn thành**

### **Phase 2: Testing (Delay - Ưu tiên trung bình - 5.5-8 ngày)**
1. ⏳ Manual Testing Checklist (0.5-1 ngày)
2. ⏳ Unit Tests (2-3 ngày)
3. ⏳ Integration Tests (2-3 ngày)
4. ⏳ E2E Tests (1-2 ngày) - Optional

### **Phase 3: Advanced Features (Ưu tiên thấp - 3.5-4.5 ngày)**
1. ⏳ Bulk Operations (1 ngày)
2. ⏳ Duplicate Check (0.5 ngày)
3. ⏳ History/Audit Trail (1-2 ngày)
4. ⏳ Comments/Notes Timeline (1 ngày)

### **Phase 4: Seed Data (Delay - Ưu tiên thấp)**
1. ⏳ Seed data warehouse mặc định
2. ⏳ Seed data demo cho testing

### **Phase 5: Real-time Features (Ưu tiên rất thấp - 3-5 ngày)**
1. ⏳ Real-time Notifications (2-3 ngày)
2. ⏳ Background Job (1-2 ngày)

---

## 📊 TỔNG KẾT

### **Đã Hoàn Thành:**
- ✅ **Sprint 1:** 95% - Chức năng chính đã hoạt động đầy đủ
- ✅ **Sprint 2:** 90% - Tất cả tính năng chính đã hoạt động
- ✅ **Inventory Adjustment system** hoàn chỉnh với approval workflow
- ✅ **Export/Print features** cho Inventory Checks đã hoàn thành
- ✅ **User Manual** đã hoàn thành
- ✅ **API Documentation** đã hoàn thành
- ✅ **Technical Documentation** đã hoàn thành

### **Còn Thiếu:**
- ⚠️ **Testing** - Cần testing để đảm bảo chất lượng (delay)
- ⚠️ **Seed data** - Cần seed data warehouse để test và demo (delay)
- ⚠️ **Advanced Features** - Có thể làm sau khi hệ thống đã được sử dụng trong production

### **Trạng Thái Tổng Thể:**
**🟢 92.5% hoàn thành** - Gần hoàn thành, chỉ còn documentation và một số tính năng nâng cao

---

## 📖 HƯỚNG DẪN SỬ DỤNG (USER MANUAL)

### **Sprint 1: Chuẩn Hóa Danh Mục Phụ Tùng & Quản Lý Vị Trí Kho**

#### **1. Quản Lý SKU, Barcode và Đơn Vị**

**Thêm SKU và Barcode cho Phụ Tùng:**
1. Truy cập menu **"Quản Lý Kho" → "Phụ Tùng"**
2. Click nút **"Thêm Phụ Tùng Mới"** hoặc chọn phụ tùng cần chỉnh sửa
3. Trong form tạo/sửa phụ tùng, điền:
   - **SKU (Stock Keeping Unit)**: Mã định danh duy nhất cho phụ tùng trong hệ thống
   - **Barcode**: Mã vạch để quét khi xuất/nhập kho
   - **Đơn vị mặc định**: Đơn vị chính của phụ tùng (VD: "Cái", "Lít", "Kg")
4. Lưu ý: SKU và Barcode là **tùy chọn** nhưng nếu nhập thì phải **duy nhất** trong hệ thống

**Quản Lý Đơn Vị Quy Đổi:**
1. Trong form tạo/sửa phụ tùng, chuyển sang tab **"Đơn Vị Quy Đổi"**
2. Thêm đơn vị mới: Nhập thông tin (Đơn vị, Hệ số quy đổi, Mã vạch) và click **"Thêm/Cập nhật"**
3. Sửa đơn vị: Click **"Sửa"** trên dòng đơn vị cần sửa, chỉnh sửa và click **"Thêm/Cập nhật"**
4. Xóa đơn vị: Click **"Xóa"** trên dòng đơn vị cần xóa
5. Đặt làm mặc định: Click **"Đặt làm mặc định"** trên dòng đơn vị

**Lưu ý:**
- **Hệ số quy đổi**: Số lượng đơn vị này bằng bao nhiêu đơn vị mặc định (VD: 1 Thùng = 10 Cái)
- **Mặc định**: Chỉ có **1 đơn vị** được đặt làm mặc định tại một thời điểm
- **Đồng bộ**: Khi đặt đơn vị làm mặc định, trường "Đơn vị mặc định" ở tab "Thông tin cơ bản" sẽ tự động cập nhật

#### **2. Quản Lý Vị Trí Kho**

**Gán Vị Trí Kho cho Phụ Tùng:**
1. Trong form tạo/sửa phụ tùng, tìm phần **"Vị Trí Kho"**
2. Chọn vị trí theo thứ tự: **Kho lưu trữ** → **Khu vực** → **Kệ/Ngăn**
3. **"Ghi chú vị trí"** sẽ tự động điền khi bạn chọn kho/khu/kệ, nhưng bạn có thể chỉnh sửa thủ công
4. Bạn có thể chỉ chọn **Kho** mà không cần chọn **Khu vực** hoặc **Kệ/Ngăn**

#### **3. Quản Lý Warehouse (Kho)**

**Truy Cập Quản Lý Warehouse:**
1. Click menu **"Quản Lý Kho" → "Kho"**
2. Bạn sẽ thấy danh sách các kho hiện có

**Tạo Warehouse Mới:**
1. Click nút **"Thêm Kho Mới"**
2. Điền thông tin trong modal (tab "Thông Tin Cơ Bản"):
   - Mã kho, Tên kho, Địa chỉ, Người quản lý, Số điện thoại
   - ☑ Kho mặc định, ☑ Hoạt động
3. (Tùy chọn) Chuyển sang tab **"Khu Vực"** để thêm khu vực
4. (Tùy chọn) Chuyển sang tab **"Kệ"** để thêm kệ/ngăn
5. Click **"Lưu"** để lưu warehouse và tất cả khu vực/kệ đã thêm

**Lưu ý:**
- **Mã kho/khu/kệ** phải **duy nhất** trong hệ thống
- Bạn có thể thêm **khu vực** và **kệ** sau khi tạo warehouse (sửa warehouse)
- **Kệ** có thể thuộc về một **khu vực** hoặc trực tiếp thuộc **kho** (không chọn khu vực)

### **Sprint 2: Cảnh Báo Tồn Kho & Kiểm Kê Định Kỳ**

#### **1. Cảnh Báo Tồn Kho (Inventory Alerts)**

**Thiết Lập Mức Tồn Kho Tối Thiểu:**
1. Truy cập **"Quản Lý Kho" → "Phụ Tùng"**
2. Chọn phụ tùng cần thiết lập, click **"Sửa"**
3. Tìm phần **"Quản Lý Tồn Kho"**:
   - **Mức tồn kho tối thiểu**: Số lượng tối thiểu cần duy trì
   - **Mức đặt hàng lại**: Số lượng nên đặt hàng khi tồn kho thấp
4. Click **"Lưu"**

**Xem Cảnh Báo Tồn Kho:**
1. Truy cập **"Quản Lý Kho" → "Cảnh Báo Tồn Kho"**
2. Bạn sẽ thấy danh sách các cảnh báo:
   - **🔴 Cao**: Tồn kho ≤ 50% mức tối thiểu
   - **🟡 Trung bình**: Tồn kho ≤ mức tối thiểu nhưng > 50%
   - **⚫ Hết hàng**: Tồn kho = 0
3. (Tùy chọn) Filter theo: Loại cảnh báo, Mức độ, Kho
4. (Tùy chọn) Click **"Đánh dấu đã xử lý"** để đánh dấu cảnh báo đã được xử lý

**Badge Cảnh Báo trên Menu:**
- Hệ thống tự động hiển thị **badge số lượng cảnh báo** trên menu **"Cảnh Báo Tồn Kho"**
- Badge sẽ tự động cập nhật mỗi **30 giây**

#### **2. Kiểm Kê Định Kỳ (Inventory Checks)**

**Tạo Phiếu Kiểm Kê Mới:**
1. Truy cập **"Quản Lý Kho" → "Kiểm Kê Định Kỳ"**
2. Click nút **"Tạo Phiếu Kiểm Kê Mới"**
3. Điền thông tin: Mã phiếu (tự động), Tên phiếu, Mô tả, Ngày kiểm kê, Kho, Khu vực, Kệ/Ngăn, Ghi chú
4. Click **"Lưu"** để tạo phiếu kiểm kê ở trạng thái **"Draft"** (Nháp)

**Thêm Items vào Phiếu Kiểm Kê:**
1. Sau khi tạo phiếu kiểm kê, click **"Xem"** để mở chi tiết
2. Click nút **"Bắt Đầu Kiểm Kê"** để chuyển trạng thái sang **"InProgress"** (Đang kiểm kê)
3. Click nút **"Thêm Item"** để thêm phụ tùng cần kiểm kê:
   - Tìm phụ tùng (Typeahead search)
   - **Số lượng hệ thống**: Tự động lấy từ `Part.QuantityInStock`
   - **Số lượng thực tế**: Bạn nhập số lượng đếm được thực tế
   - **Chênh lệch**: Tự động tính = Số lượng thực tế - Số lượng hệ thống
4. Click **"Lưu"** để thêm item vào phiếu kiểm kê
5. Lặp lại để thêm các phụ tùng khác

**Hoàn Thành Kiểm Kê:**
1. Sau khi thêm đủ items, click nút **"Hoàn Thành Kiểm Kê"**
2. Phiếu kiểm kê sẽ chuyển sang trạng thái **"Completed"** (Đã hoàn thành)
3. Hệ thống sẽ hiển thị **thống kê**: Tổng số items, Số items có chênh lệch, Tổng số lượng thiếu/thừa
4. (Tùy chọn) Nếu có chênh lệch, click nút **"Tạo Điều Chỉnh"** để tạo phiếu điều chỉnh tồn kho

**Export/Print Phiếu Kiểm Kê:**
- **Export Excel**: Trong danh sách phiếu kiểm kê, click **"Xuất Excel"** (có thể filter trước)
- **Print**: Trong chi tiết phiếu kiểm kê, click **"In"**

#### **3. Điều Chỉnh Tồn Kho (Inventory Adjustments)**

**Tạo Điều Chỉnh Từ Kiểm Kê:**
1. Sau khi hoàn thành kiểm kê có chênh lệch, trong chi tiết phiếu kiểm kê, click nút **"Tạo Điều Chỉnh"**
2. Điền thông tin: Lý do điều chỉnh, Ghi chú
3. Click **"Tạo Điều Chỉnh"**
4. Hệ thống sẽ tự động:
   - Tạo phiếu điều chỉnh với mã **ADJ-YYYY-NNN** (tự động)
   - Tạo các items điều chỉnh từ các items kiểm kê có chênh lệch
   - Liên kết items điều chỉnh với items kiểm kê (one-to-one)
5. Phiếu điều chỉnh sẽ ở trạng thái **"Pending"** (Chờ duyệt)

**Tạo Điều Chỉnh Thủ Công:**
1. Truy cập **"Quản Lý Kho" → "Điều Chỉnh Tồn Kho"**
2. Click nút **"Tạo Điều Chỉnh Mới"**
3. Điền thông tin: Ngày điều chỉnh, Kho, Khu vực, Kệ/Ngăn, Lý do, Ghi chú
4. Click **"Thêm Item"** để thêm phụ tùng cần điều chỉnh:
   - Tìm phụ tùng (Typeahead search)
   - **Số lượng trước**: Tự động lấy từ `Part.QuantityInStock`
   - **Số lượng thay đổi**: Có thể **âm** (giảm) hoặc **dương** (tăng)
   - **Số lượng sau**: Tự động tính = Số lượng trước + Số lượng thay đổi
5. Click **"Lưu"** để thêm item vào danh sách
6. Lặp lại để thêm các items khác
7. Click **"Tạo Điều Chỉnh"** để lưu phiếu điều chỉnh

**Duyệt/Từ Chối Điều Chỉnh:**
1. Truy cập **"Quản Lý Kho" → "Điều Chỉnh Tồn Kho"**
2. Click **"Xem"** trên phiếu điều chỉnh cần duyệt
3. Xem chi tiết phiếu điều chỉnh
4. Click **"Duyệt"** hoặc **"Từ Chối"**

**Nếu Duyệt:**
- Hệ thống sẽ:
  1. Cập nhật `Part.QuantityInStock` cho từng item
  2. Tạo `StockTransaction` để ghi nhận thay đổi
  3. Cập nhật trạng thái phiếu điều chỉnh thành **"Approved"**
  4. Ghi nhận người duyệt và thời gian duyệt

**Nếu Từ Chối:**
- Hệ thống sẽ yêu cầu nhập **Lý do từ chối**
- Phiếu điều chỉnh sẽ chuyển sang trạng thái **"Rejected"**

**Lưu ý:**
- Chỉ **Admin/Manager** mới có quyền duyệt/từ chối
- Sau khi duyệt, **không thể** xóa phiếu điều chỉnh
- Sau khi duyệt, hệ thống sẽ tự động cập nhật tồn kho và tạo giao dịch kho

### **Troubleshooting**

**Vấn đề: Không thấy dropdown Warehouse khi tạo phụ tùng**
- **Nguyên nhân:** Chưa có warehouse nào trong hệ thống
- **Giải pháp:** Truy cập **"Quản Lý Kho" → "Kho"** và tạo warehouse mới

**Vấn đề: Không thể đặt đơn vị làm mặc định**
- **Nguyên nhân:** Đơn vị chưa được thêm vào danh sách "Đơn Vị Quy Đổi"
- **Giải pháp:** Trong form phụ tùng, chuyển sang tab **"Đơn Vị Quy Đổi"** và thêm đơn vị vào danh sách

**Vấn đề: Cảnh báo tồn kho không hiển thị**
- **Nguyên nhân:** Chưa thiết lập "Mức tồn kho tối thiểu" cho phụ tùng
- **Giải pháp:** Mở phụ tùng cần thiết lập, nhập **"Mức tồn kho tối thiểu"** và **"Mức đặt hàng lại"**, sau đó lưu lại

**Vấn đề: Không thể hoàn thành kiểm kê**
- **Nguyên nhân:** Chưa thêm items vào phiếu kiểm kê
- **Giải pháp:** Click **"Bắt Đầu Kiểm Kê"** để chuyển sang trạng thái "InProgress", sau đó thêm ít nhất 1 item vào phiếu kiểm kê

**Vấn đề: Không thể duyệt điều chỉnh**
- **Nguyên nhân:** Không có quyền Admin/Manager hoặc Số lượng sau điều chỉnh < 0
- **Giải pháp:** Kiểm tra quyền của tài khoản và kiểm tra lại số lượng thay đổi trong items (đảm bảo số lượng sau ≥ 0)

---

## 🧪 TEST MANUAL CHI TIẾT

### **1. Test Parts Management UI - MinimumStock & ReorderLevel**

#### **Test Case 1.1: Tạo Part Mới với MinimumStock và ReorderLevel**
- **Input:** MinimumStock = 10, ReorderLevel = 20
- **Expected:** Part được tạo thành công, MinimumStock = 10, ReorderLevel = 20 trong database

#### **Test Case 1.2: Validation - ReorderLevel < MinimumStock**
- **Input:** MinimumStock = 10, ReorderLevel = 5
- **Expected:** Client-side validation hiển thị lỗi, ReorderLevel tự động được điều chỉnh thành 10

#### **Test Case 1.3: Validation - MinimumStock < 0**
- **Input:** MinimumStock = -5
- **Expected:** Client-side validation hiển thị lỗi, MinimumStock tự động được điều chỉnh thành 0

### **2. Test Visual Indicators - View Part Modal**

#### **Test Case 2.1: Part hết hàng (QuantityInStock = 0)**
- **Setup:** Tạo part với QuantityInStock = 0, MinimumStock = 10
- **Expected:** Hiển thị badge màu đỏ "Hết hàng", số lượng tồn kho có màu đỏ

#### **Test Case 2.2: Part cảnh báo (QuantityInStock <= MinimumStock, nhưng > 0)**
- **Setup:** Tạo part với QuantityInStock = 5, MinimumStock = 10
- **Expected:** Hiển thị badge màu vàng "Cảnh báo", số lượng tồn kho có màu vàng

#### **Test Case 2.3: Part bình thường (QuantityInStock > MinimumStock)**
- **Setup:** Tạo part với QuantityInStock = 20, MinimumStock = 10
- **Expected:** Không có badge cảnh báo, số lượng tồn kho có màu xanh

### **3. Test Badge Count trên Menu**

#### **Test Case 3.1: Badge hiển thị khi có alerts**
- **Setup:** Tạo các parts với QuantityInStock <= MinimumStock, đợi 30 giây
- **Expected:** Badge màu đỏ hiển thị số lượng alerts trên menu "Cảnh Báo Tồn Kho"

#### **Test Case 3.2: Badge ẩn khi không có alerts**
- **Setup:** Đảm bảo tất cả parts có QuantityInStock > MinimumStock, đợi 30 giây
- **Expected:** Badge ẩn (không hiển thị) trên menu "Cảnh Báo Tồn Kho"

### **4. Test Inventory Checks**

#### **Test Case 4.1: Tạo phiếu kiểm kê mới**
- **Steps:**
  1. Truy cập "Quản Lý Kho" → "Kiểm Kê Định Kỳ"
  2. Click "Tạo Phiếu Kiểm Kê Mới"
  3. Điền thông tin và click "Lưu"
- **Expected:** Phiếu kiểm kê được tạo với mã tự động (IK-YYYY-NNN), trạng thái "Draft"

#### **Test Case 4.2: Thêm items vào phiếu kiểm kê**
- **Steps:**
  1. Mở phiếu kiểm kê, click "Bắt Đầu Kiểm Kê"
  2. Click "Thêm Item", chọn phụ tùng, nhập số lượng thực tế
  3. Click "Lưu"
- **Expected:** Item được thêm vào phiếu kiểm kê, chênh lệch được tự động tính

#### **Test Case 4.3: Hoàn thành kiểm kê**
- **Steps:**
  1. Sau khi thêm items, click "Hoàn Thành Kiểm Kê"
  2. Xác nhận hoàn thành
- **Expected:** Phiếu kiểm kê chuyển sang trạng thái "Completed", hiển thị thống kê

### **5. Test Inventory Adjustments**

#### **Test Case 5.1: Tạo điều chỉnh từ kiểm kê**
- **Steps:**
  1. Hoàn thành kiểm kê có chênh lệch
  2. Click "Tạo Điều Chỉnh"
  3. Điền lý do và ghi chú, click "Tạo Điều Chỉnh"
- **Expected:** Phiếu điều chỉnh được tạo với mã tự động (ADJ-YYYY-NNN), trạng thái "Pending", items được tự động tạo từ items kiểm kê có chênh lệch

#### **Test Case 5.2: Tạo điều chỉnh thủ công**
- **Steps:**
  1. Truy cập "Quản Lý Kho" → "Điều Chỉnh Tồn Kho"
  2. Click "Tạo Điều Chỉnh Mới"
  3. Điền thông tin, thêm items, click "Tạo Điều Chỉnh"
- **Expected:** Phiếu điều chỉnh được tạo với mã tự động, trạng thái "Pending"

#### **Test Case 5.3: Duyệt điều chỉnh**
- **Steps:**
  1. Mở phiếu điều chỉnh, click "Duyệt"
  2. Xác nhận duyệt
- **Expected:** Phiếu điều chỉnh chuyển sang trạng thái "Approved", `Part.QuantityInStock` được cập nhật, `StockTransaction` được tạo

#### **Test Case 5.4: Từ chối điều chỉnh**
- **Steps:**
  1. Mở phiếu điều chỉnh, click "Từ Chối"
  2. Nhập lý do từ chối, xác nhận
- **Expected:** Phiếu điều chỉnh chuyển sang trạng thái "Rejected", lý do từ chối được lưu

---

## 🏗️ GIẢI THÍCH LOGIC: WAREHOUSE → ZONE → BIN

### **Cấu Trúc Database**

**1. Warehouse (Kho)**
- Là cấp cao nhất
- Có thể có nhiều Zones (khu vực)
- Có thể có nhiều Bins (kệ/ngăn) trực thuộc (không qua Zone)

**2. WarehouseZone (Khu vực)**
- Thuộc về một Warehouse (bắt buộc)
- Có thể có nhiều Bins (kệ/ngăn) thuộc về Zone đó

**3. WarehouseBin (Kệ/Ngăn)**
- **Thuộc về một Warehouse (bắt buộc)** - `WarehouseId` (required)
- **Có thể thuộc về một Zone (tùy chọn)** - `WarehouseZoneId` (nullable)
- Nghĩa là:
  - **Bin có thể thuộc trực tiếp Warehouse** (`WarehouseZoneId = null`)
  - **Bin có thể thuộc Zone** (`WarehouseZoneId != null`)

### **Logic Hiện Tại**

**Khi chọn Warehouse:**
1. Load Zones từ `warehouse.zones` → Enable dropdown Zone
2. Load Bins trực thuộc Warehouse (`warehouse.bins` - bins có `WarehouseZoneId = null`) → Enable dropdown Bin
3. Nếu Warehouse không có Zones → Disable dropdown Zone
4. Nếu Warehouse không có Bins trực thuộc → Disable dropdown Bin (nhưng vẫn có thể có bins trong zones)

**Khi chọn Zone:**
1. **Nếu Zone có bins** (`zone.bins`):
   - Load bins từ Zone đó → Enable dropdown Bin
   - **CHỈ hiển thị bins thuộc Zone đó** (không hiển thị bins trực thuộc Warehouse)
2. **Nếu Zone không có bins** (`zone.bins = null hoặc empty`):
   - Load bins trực thuộc Warehouse (`warehouse.bins`) → Enable dropdown Bin
   - **CHỈ hiển thị bins trực thuộc Warehouse** (bins có `WarehouseZoneId = null`)
3. **Nếu cả Zone và Warehouse đều không có bins**: Disable dropdown Bin

### **Vấn Đề Hiện Tại**

**Vấn đề 1: Logic không rõ ràng**
- Khi chọn Zone, nếu Zone có bins, chỉ hiển thị bins trong Zone
- **NHƯNG** nếu Zone không có bins, mới hiển thị bins trực thuộc Warehouse
- Điều này có thể gây confusion: **"Tại sao chọn Zone rồi mà không chọn được Bin?"**

**Vấn đề 2: Zone được chọn nhưng không có bins**
- Khi Zone được chọn nhưng Zone đó không có bins
- Logic hiện tại sẽ fallback sang `warehouse.bins`
- **Vấn đề:** User không hiểu tại sao không chọn được Bin khi đã chọn Zone

### **Giải Pháp Đề Xuất (Option 1 - Đề xuất)**

**Khi chọn Zone:**
- **Luôn hiển thị cả bins trong Zone VÀ bins trực thuộc Warehouse:**
  - Load bins trong Zone (`zone.bins`) nếu có
  - **VÀ** load bins trực thuộc Warehouse (`warehouse.bins` - bins có `WarehouseZoneId = null`) nếu có
  - Enable dropdown Bin (nếu có ít nhất một bin)
- **Nếu Zone không có bins VÀ Warehouse cũng không có bins:**
  - Disable dropdown Bin
  - Hiển thị message: "Chưa có kệ/ngăn nào trong khu vực này. Vui lòng tạo kệ/ngăn trước."

**Ưu điểm:**
- User luôn có thể chọn Bin, không bị disable
- Linh hoạt: có thể chọn bin trong Zone hoặc bin trực thuộc Warehouse
- Không gây confusion

**Nhược điểm:**
- Có thể hiển thị nhiều bins (cả trong Zone và Warehouse)

### **Ví Dụ Cụ Thể**

**Ví dụ 1: Warehouse có Zones và Bins**
- **Warehouse:** "Kho A"
  - **Zones:** Zone 1: "Khu vực 1" (có 2 bins: Bin 1, Bin 2)
  - **Bins trực thuộc Warehouse:** Bin 3, Bin 4

**Khi chọn Warehouse "Kho A":**
- Dropdown Zone: Hiển thị "Khu vực 1" → Enable
- Dropdown Bin: Hiển thị "Bin 3", "Bin 4" → Enable

**Khi chọn Zone "Khu vực 1":**
- Dropdown Bin: Hiển thị "Bin 1", "Bin 2", "Bin 3", "Bin 4" → Enable
- (Bins trong Zone + Bins trực thuộc Warehouse)

---

## 📡 API DOCUMENTATION

### **Base URL**
```
https://your-domain.com/api
```

### **Authentication**
Tất cả API endpoints yêu cầu authentication thông qua JWT Bearer Token:
```
Authorization: Bearer {token}
```

### **Response Format**
Tất cả API responses sử dụng format chuẩn:
```json
{
  "success": true,
  "data": { ... },
  "message": "Success message",
  "errors": []
}
```

---

### **1. PARTS MANAGEMENT API**

#### **1.1. GET /api/parts**
Lấy danh sách phụ tùng với pagination và filters.

**Query Parameters:**
- `pageNumber` (int, optional): Số trang (mặc định: 1)
- `pageSize` (int, optional): Số items mỗi trang (mặc định: 10)
- `searchTerm` (string, optional): Tìm kiếm theo tên, mã, mô tả
- `category` (string, optional): Lọc theo danh mục

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "partNumber": "PT-001",
      "partName": "Lốp xe Michelin",
      "sku": "LOP-MIC-001",
      "barcode": "1234567890123",
      "defaultUnit": "Cái",
      "quantityInStock": 50,
      "minimumStock": 10,
      "reorderLevel": 20,
      "units": [
        {
          "unitName": "Cái",
          "conversionRate": 1.0,
          "isDefault": true
        },
        {
          "unitName": "Thùng",
          "conversionRate": 10.0,
          "isDefault": false
        }
      ]
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10
}
```

#### **1.2. GET /api/parts/{id}**
Lấy chi tiết phụ tùng theo ID.

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "partNumber": "PT-001",
    "partName": "Lốp xe Michelin",
    "sku": "LOP-MIC-001",
    "barcode": "1234567890123",
    "defaultUnit": "Cái",
    "quantityInStock": 50,
    "minimumStock": 10,
    "reorderLevel": 20,
    "units": [ ... ],
    "warehouseId": 1,
    "warehouseName": "Kho Chính",
    "warehouseZoneId": 1,
    "warehouseZoneName": "Khu A",
    "warehouseBinId": 1,
    "warehouseBinName": "Kệ 1",
    "location": "Kho Chính - Khu A - Kệ 1"
  }
}
```

#### **1.3. POST /api/parts**
Tạo phụ tùng mới.

**Request Body:**
```json
{
  "partNumber": "PT-002",
  "partName": "Dầu nhớt",
  "sku": "DAU-001",
  "barcode": "9876543210987",
  "defaultUnit": "Lít",
  "minimumStock": 20,
  "reorderLevel": 40,
  "warehouseId": 1,
  "warehouseZoneId": 1,
  "warehouseBinId": 1,
  "units": [
    {
      "unitName": "Lít",
      "conversionRate": 1.0,
      "isDefault": true
    },
    {
      "unitName": "Thùng",
      "conversionRate": 20.0,
      "isDefault": false
    }
  ]
}
```

**Validation:**
- SKU phải unique (nếu có)
- Barcode phải unique (nếu có)
- DefaultUnit phải có trong units với IsDefault=true

#### **1.4. PUT /api/parts/{id}**
Cập nhật phụ tùng.

**Request Body:** Tương tự POST /api/parts

#### **1.5. DELETE /api/parts/{id}**
Xóa phụ tùng (soft delete).

---

### **2. WAREHOUSES MANAGEMENT API**

#### **2.1. GET /api/warehouses**
Lấy danh sách tất cả warehouses với zones và bins.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "code": "WH-001",
      "name": "Kho Chính",
      "address": "123 Đường ABC",
      "managerName": "Nguyễn Văn A",
      "phoneNumber": "0123456789",
      "isDefault": true,
      "isActive": true,
      "zones": [
        {
          "id": 1,
          "code": "ZONE-001",
          "name": "Khu A",
          "displayOrder": 1,
          "bins": [
            {
              "id": 1,
              "code": "BIN-001",
              "name": "Kệ 1",
              "capacity": 100
            }
          ]
        }
      ],
      "bins": [
        {
          "id": 2,
          "code": "BIN-002",
          "name": "Kệ 2",
          "warehouseZoneId": null
        }
      ]
    }
  ]
}
```

#### **2.2. GET /api/warehouses/{id}**
Lấy chi tiết warehouse theo ID.

#### **2.3. POST /api/warehouses**
Tạo warehouse mới.

**Request Body:**
```json
{
  "code": "WH-002",
  "name": "Kho Phụ",
  "address": "456 Đường XYZ",
  "managerName": "Trần Văn B",
  "phoneNumber": "0987654321",
  "isDefault": false,
  "isActive": true
}
```

#### **2.4. PUT /api/warehouses/{id}**
Cập nhật warehouse.

#### **2.5. DELETE /api/warehouses/{id}**
Xóa warehouse (soft delete).

#### **2.6. POST /api/warehouses/{warehouseId}/zones**
Tạo zone mới trong warehouse.

**Request Body:**
```json
{
  "code": "ZONE-002",
  "name": "Khu B",
  "displayOrder": 2,
  "isActive": true
}
```

#### **2.7. PUT /api/warehouses/{warehouseId}/zones/{zoneId}**
Cập nhật zone.

#### **2.8. DELETE /api/warehouses/{warehouseId}/zones/{zoneId}**
Xóa zone (soft delete).

#### **2.9. POST /api/warehouses/{warehouseId}/bins**
Tạo bin mới trong warehouse.

**Request Body:**
```json
{
  "code": "BIN-003",
  "name": "Kệ 3",
  "warehouseZoneId": 1,
  "capacity": 150,
  "isDefault": false,
  "isActive": true
}
```

#### **2.10. PUT /api/warehouses/{warehouseId}/bins/{binId}**
Cập nhật bin.

#### **2.11. DELETE /api/warehouses/{warehouseId}/bins/{binId}**
Xóa bin (soft delete).

---

### **3. INVENTORY CHECKS API**

#### **3.1. GET /api/inventory-checks**
Lấy danh sách phiếu kiểm kê với filters.

**Query Parameters:**
- `warehouseId` (int, optional): Lọc theo warehouse
- `warehouseZoneId` (int, optional): Lọc theo zone
- `warehouseBinId` (int, optional): Lọc theo bin
- `status` (string, optional): Lọc theo trạng thái (Draft, InProgress, Completed)
- `startDate` (DateTime, optional): Ngày bắt đầu
- `endDate` (DateTime, optional): Ngày kết thúc

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "code": "IK-2025-001",
      "name": "Kiểm kê tháng 1/2025",
      "checkDate": "2025-01-15T00:00:00",
      "status": "Completed",
      "warehouseName": "Kho Chính",
      "totalItems": 10,
      "discrepancyItems": 2,
      "totalDiscrepancy": -5
    }
  ]
}
```

#### **3.2. GET /api/inventory-checks/{id}**
Lấy chi tiết phiếu kiểm kê với items.

#### **3.3. POST /api/inventory-checks**
Tạo phiếu kiểm kê mới.

**Request Body:**
```json
{
  "name": "Kiểm kê tháng 1/2025",
  "description": "Kiểm kê định kỳ tháng 1",
  "checkDate": "2025-01-15",
  "warehouseId": 1,
  "warehouseZoneId": 1,
  "warehouseBinId": 1,
  "notes": "Ghi chú"
}
```

**Response:** Code tự động generate (IK-YYYY-NNN format)

#### **3.4. PUT /api/inventory-checks/{id}**
Cập nhật phiếu kiểm kê.

#### **3.5. DELETE /api/inventory-checks/{id}**
Xóa phiếu kiểm kê (soft delete, chỉ khi status != Completed).

#### **3.6. POST /api/inventory-checks/{id}/complete**
Hoàn thành phiếu kiểm kê.

**Request Body:**
```json
{
  "notes": "Ghi chú hoàn thành"
}
```

#### **3.7. POST /api/inventory-checks/{id}/items**
Thêm item vào phiếu kiểm kê.

**Request Body:**
```json
{
  "partId": 1,
  "actualQuantity": 48,
  "notes": "Thiếu 2 lốp"
}
```

**Response:** SystemQuantity tự động lấy từ Part.QuantityInStock, DiscrepancyQuantity tự động tính.

#### **3.8. PUT /api/inventory-checks/{id}/items/{itemId}**
Cập nhật item trong phiếu kiểm kê.

#### **3.9. DELETE /api/inventory-checks/{id}/items/{itemId}**
Xóa item khỏi phiếu kiểm kê.

#### **3.10. GET /api/inventory-checks/export/excel**
Export danh sách phiếu kiểm kê ra Excel.

**Query Parameters:** Tương tự GET /api/inventory-checks

#### **3.11. GET /api/inventory-checks/{id}/export/excel**
Export chi tiết phiếu kiểm kê ra Excel.

---

### **4. INVENTORY ADJUSTMENTS API**

#### **4.1. GET /api/inventory-adjustments**
Lấy danh sách phiếu điều chỉnh với filters.

**Query Parameters:**
- `warehouseId` (int, optional): Lọc theo warehouse
- `status` (string, optional): Lọc theo trạng thái (Pending, Approved, Rejected)
- `startDate` (DateTime, optional): Ngày bắt đầu
- `endDate` (DateTime, optional): Ngày kết thúc

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "adjustmentNumber": "ADJ-2025-001",
      "adjustmentDate": "2025-01-15T00:00:00",
      "status": "Approved",
      "warehouseName": "Kho Chính",
      "reason": "Chênh lệch sau kiểm kê",
      "approvedByEmployeeName": "Nguyễn Văn A",
      "approvedAt": "2025-01-15T10:00:00"
    }
  ]
}
```

#### **4.2. GET /api/inventory-adjustments/{id}**
Lấy chi tiết phiếu điều chỉnh với items.

#### **4.3. POST /api/inventory-adjustments**
Tạo phiếu điều chỉnh thủ công.

**Request Body:**
```json
{
  "adjustmentDate": "2025-01-15",
  "warehouseId": 1,
  "warehouseZoneId": 1,
  "warehouseBinId": 1,
  "reason": "Hàng hỏng, cần điều chỉnh",
  "notes": "Ghi chú",
  "items": [
    {
      "partId": 1,
      "quantityChange": -2,
      "notes": "Hỏng 2 lốp"
    }
  ]
}
```

**Response:** AdjustmentNumber tự động generate (ADJ-YYYY-NNN format)

#### **4.4. POST /api/inventory-adjustments/from-check/{checkId}**
Tạo phiếu điều chỉnh từ phiếu kiểm kê.

**Request Body:**
```json
{
  "reason": "Chênh lệch sau kiểm kê định kỳ",
  "notes": "Điều chỉnh theo kết quả kiểm kê tháng 1/2025"
}
```

**Response:** Tự động tạo items từ các items kiểm kê có discrepancy (IsDiscrepancy = true)

#### **4.5. PUT /api/inventory-adjustments/{id}/approve**
Duyệt phiếu điều chỉnh.

**Request Body:**
```json
{
  "notes": "Ghi chú duyệt"
}
```

**Logic:**
1. Cập nhật `Part.QuantityInStock` cho từng item
2. Tạo `StockTransaction` để ghi nhận thay đổi
3. Cập nhật trạng thái thành "Approved"
4. Ghi nhận người duyệt và thời gian duyệt

#### **4.6. PUT /api/inventory-adjustments/{id}/reject**
Từ chối phiếu điều chỉnh.

**Request Body:**
```json
{
  "rejectionReason": "Lý do từ chối"
}
```

#### **4.7. DELETE /api/inventory-adjustments/{id}**
Xóa phiếu điều chỉnh (soft delete, chỉ khi status != Approved).

---

### **5. INVENTORY ALERTS API**

#### **5.1. GET /api/inventory-alerts/low-stock**
Lấy danh sách parts dưới mức tồn kho tối thiểu.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "partName": "Lốp xe",
      "partNumber": "PT-001",
      "currentStock": 5,
      "minStock": 10,
      "deficit": 5,
      "reorderQuantity": 15,
      "estimatedCost": 1500000,
      "alertLevel": "High",
      "location": "Kho Chính - Khu A - Kệ 1"
    }
  ],
  "count": 1
}
```

#### **5.2. GET /api/inventory-alerts/out-of-stock**
Lấy danh sách parts hết hàng (QuantityInStock = 0).

#### **5.3. GET /api/inventory-alerts/overstock**
Lấy danh sách parts tồn kho cao (QuantityInStock > ReorderLevel * 3).

#### **5.4. GET /api/inventory-alerts/reorder-suggestions**
Gợi ý đặt hàng lại dựa trên mức tồn kho và lịch sử sử dụng.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "partName": "Lốp xe",
      "partNumber": "PT-001",
      "currentStock": 5,
      "minStock": 10,
      "maxStock": 20,
      "avgDailyUsage": 0.5,
      "usage30Days": 15,
      "suggestedOrderQuantity": 15,
      "estimatedCost": 1500000,
      "priority": "High",
      "leadTime": "7 days"
    }
  ],
  "count": 1,
  "totalEstimatedCost": 1500000
}
```

#### **5.5. GET /api/inventory-alerts/expiring-soon**
Cảnh báo parts sắp hết hạn.

**Query Parameters:**
- `daysAhead` (int, optional): Số ngày trước khi hết hạn (mặc định: 30)

#### **5.6. GET /api/inventory-alerts/GetAlertsCount**
Lấy tổng số alerts (low stock + out of stock).

**Response:**
```json
{
  "success": true,
  "count": 5,
  "lowStock": 3,
  "outOfStock": 2
}
```

#### **5.7. GET /api/inventory-alerts/export-excel**
Export alerts ra Excel.

**Query Parameters:**
- `alertType` (string, optional): Loại alert (LowStock, OutOfStock, null = tất cả)

---

### **6. ERROR CODES**

**400 Bad Request:**
- Validation errors
- Invalid request body
- Missing required fields

**401 Unauthorized:**
- Missing or invalid JWT token
- Token expired

**403 Forbidden:**
- Insufficient permissions
- Role-based access denied

**404 Not Found:**
- Resource not found
- Invalid ID

**409 Conflict:**
- Duplicate SKU/Barcode
- Duplicate Code (Warehouse/Zone/Bin)
- Business rule violation

**500 Internal Server Error:**
- Server errors
- Database errors
- Unexpected exceptions

---

## 🔧 TECHNICAL DOCUMENTATION

### **1. DATABASE SCHEMA**

#### **1.1. Part Entity**
```sql
CREATE TABLE Parts (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    PartNumber VARCHAR(50) NOT NULL,
    PartName NVARCHAR(200) NOT NULL,
    Sku VARCHAR(100) UNIQUE,              -- ✅ Phase 4.1
    Barcode VARCHAR(150) UNIQUE,          -- ✅ Phase 4.1
    DefaultUnit VARCHAR(20),              -- ✅ Phase 4.1 (thay thế Unit)
    QuantityInStock INT NOT NULL DEFAULT 0,
    MinimumStock INT NOT NULL DEFAULT 0,  -- ✅ Phase 4.1
    ReorderLevel INT,                     -- ✅ Phase 4.1
    Location NVARCHAR(500),               -- Ghi chú vị trí (text)
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    CreatedBy INT,
    UpdatedBy INT
);

CREATE INDEX IX_Parts_Sku ON Parts(Sku) WHERE Sku IS NOT NULL;
CREATE INDEX IX_Parts_Barcode ON Parts(Barcode) WHERE Barcode IS NOT NULL;
```

#### **1.2. PartUnit Entity**
```sql
CREATE TABLE PartUnits (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    PartId INT NOT NULL,
    UnitName VARCHAR(50) NOT NULL,
    ConversionRate DECIMAL(18,4) NOT NULL DEFAULT 1.0,
    Barcode VARCHAR(150),
    IsDefault BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    
    FOREIGN KEY (PartId) REFERENCES Parts(Id) ON DELETE CASCADE,
    UNIQUE INDEX IX_PartUnits_PartId_UnitName (PartId, UnitName)
);
```

#### **1.3. Warehouse Entity**
```sql
CREATE TABLE Warehouses (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Code VARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(150) NOT NULL,
    Address NVARCHAR(500),
    ManagerName NVARCHAR(100),
    PhoneNumber VARCHAR(20),
    IsDefault BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    CreatedBy INT,
    UpdatedBy INT
);
```

#### **1.4. WarehouseZone Entity**
```sql
CREATE TABLE WarehouseZones (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    WarehouseId INT NOT NULL,
    Code VARCHAR(50) NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    
    FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id) ON DELETE CASCADE,
    UNIQUE INDEX IX_WarehouseZones_WarehouseId_Code (WarehouseId, Code)
);
```

#### **1.5. WarehouseBin Entity**
```sql
CREATE TABLE WarehouseBins (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    WarehouseId INT NOT NULL,
    WarehouseZoneId INT NULL,             -- Có thể null (bin trực thuộc warehouse)
    Code VARCHAR(50) NOT NULL,
    Name NVARCHAR(150) NOT NULL,
    Capacity DECIMAL(18,2),
    IsDefault BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    
    FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id) ON DELETE CASCADE,
    FOREIGN KEY (WarehouseZoneId) REFERENCES WarehouseZones(Id) ON DELETE SET NULL,
    UNIQUE INDEX IX_WarehouseBins_WarehouseId_Code (WarehouseId, Code)
);
```

#### **1.6. InventoryCheck Entity**
```sql
CREATE TABLE InventoryChecks (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Code VARCHAR(50) NOT NULL UNIQUE,     -- IK-YYYY-NNN format
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    CheckDate DATETIME NOT NULL,
    WarehouseId INT,
    WarehouseZoneId INT,
    WarehouseBinId INT,
    Status VARCHAR(50) NOT NULL DEFAULT 'Draft',  -- Draft, InProgress, Completed
    StartedByEmployeeId INT,
    StartedAt DATETIME,
    CompletedByEmployeeId INT,
    CompletedAt DATETIME,
    Notes NVARCHAR(1000),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    
    FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id) ON DELETE SET NULL,
    FOREIGN KEY (WarehouseZoneId) REFERENCES WarehouseZones(Id) ON DELETE SET NULL,
    FOREIGN KEY (WarehouseBinId) REFERENCES WarehouseBins(Id) ON DELETE SET NULL,
    FOREIGN KEY (StartedByEmployeeId) REFERENCES Employees(Id) ON DELETE SET NULL,
    FOREIGN KEY (CompletedByEmployeeId) REFERENCES Employees(Id) ON DELETE SET NULL
);
```

#### **1.7. InventoryCheckItem Entity**
```sql
CREATE TABLE InventoryCheckItems (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    InventoryCheckId INT NOT NULL,
    PartId INT NOT NULL,
    SystemQuantity INT NOT NULL,          -- Tự động lấy từ Part.QuantityInStock
    ActualQuantity INT NOT NULL,          -- Số lượng thực tế đếm được
    DiscrepancyQuantity INT NOT NULL,     -- = ActualQuantity - SystemQuantity
    IsDiscrepancy BIT NOT NULL DEFAULT 0, -- = DiscrepancyQuantity != 0
    IsAdjusted BIT NOT NULL DEFAULT 0,    -- Đã tạo adjustment chưa
    InventoryAdjustmentItemId INT NULL,   -- Link đến InventoryAdjustmentItem
    Notes NVARCHAR(500),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    
    FOREIGN KEY (InventoryCheckId) REFERENCES InventoryChecks(Id) ON DELETE CASCADE,
    FOREIGN KEY (PartId) REFERENCES Parts(Id) ON DELETE RESTRICT,
    FOREIGN KEY (InventoryAdjustmentItemId) REFERENCES InventoryAdjustmentItems(Id) ON DELETE SET NULL
);
```

#### **1.8. InventoryAdjustment Entity**
```sql
CREATE TABLE InventoryAdjustments (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    AdjustmentNumber VARCHAR(50) NOT NULL UNIQUE,  -- ADJ-YYYY-NNN format
    InventoryCheckId INT NULL,                     -- Link đến InventoryCheck (nếu có)
    WarehouseId INT,
    WarehouseZoneId INT,
    WarehouseBinId INT,
    AdjustmentDate DATETIME NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected
    Reason NVARCHAR(1000),
    ApprovedByEmployeeId INT,
    ApprovedAt DATETIME,
    RejectionReason NVARCHAR(1000),
    Notes NVARCHAR(500),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    
    FOREIGN KEY (InventoryCheckId) REFERENCES InventoryChecks(Id) ON DELETE SET NULL,
    FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id) ON DELETE SET NULL,
    FOREIGN KEY (WarehouseZoneId) REFERENCES WarehouseZones(Id) ON DELETE SET NULL,
    FOREIGN KEY (WarehouseBinId) REFERENCES WarehouseBins(Id) ON DELETE SET NULL,
    FOREIGN KEY (ApprovedByEmployeeId) REFERENCES Employees(Id) ON DELETE SET NULL
);
```

#### **1.9. InventoryAdjustmentItem Entity**
```sql
CREATE TABLE InventoryAdjustmentItems (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    InventoryAdjustmentId INT NOT NULL,
    PartId INT NOT NULL,
    InventoryCheckItemId INT NULL,        -- Link đến InventoryCheckItem (one-to-one)
    QuantityChange INT NOT NULL,          -- Có thể âm (giảm) hoặc dương (tăng)
    SystemQuantityBefore INT NOT NULL,    -- Số lượng trước điều chỉnh
    SystemQuantityAfter INT NOT NULL,     -- = SystemQuantityBefore + QuantityChange
    Notes NVARCHAR(500),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME,
    
    FOREIGN KEY (InventoryAdjustmentId) REFERENCES InventoryAdjustments(Id) ON DELETE CASCADE,
    FOREIGN KEY (PartId) REFERENCES Parts(Id) ON DELETE RESTRICT,
    FOREIGN KEY (InventoryCheckItemId) REFERENCES InventoryCheckItems(Id) ON DELETE SET NULL,
    UNIQUE INDEX IX_InventoryAdjustmentItems_InventoryCheckItemId (InventoryCheckItemId) WHERE InventoryCheckItemId IS NOT NULL
);
```

---

### **2. BUSINESS LOGIC**

#### **2.1. Part DefaultUnit ↔ PartUnits Synchronization**

**Quy tắc:**
1. `Part.DefaultUnit` phải có trong `Part.PartUnits` với `IsDefault=true`
2. Chỉ có 1 `PartUnit` có `IsDefault=true` tại một thời điểm
3. Khi set `DefaultUnit` → Tự động thêm vào `PartUnits` nếu chưa có (ConversionRate=1, IsDefault=true)
4. Khi set `PartUnit.IsDefault=true` → Tự động cập nhật `Part.DefaultUnit`

**Implementation:**
```csharp
private void EnsureDefaultUnit(Part part)
{
    if (!string.IsNullOrWhiteSpace(part.DefaultUnit))
    {
        // Set tất cả units về IsDefault=false
        foreach (var unit in part.PartUnits)
        {
            unit.IsDefault = false;
        }
        
        // Tìm matching unit
        var matchingUnit = part.PartUnits.FirstOrDefault(u => 
            u.UnitName.Equals(part.DefaultUnit, StringComparison.OrdinalIgnoreCase));
        
        if (matchingUnit != null)
        {
            matchingUnit.IsDefault = true;
            part.DefaultUnit = matchingUnit.UnitName; // Case-sensitive từ database
        }
        else
        {
            // Tạo mới PartUnit
            var newUnit = new PartUnit
            {
                UnitName = part.DefaultUnit.Trim(),
                ConversionRate = 1,
                IsDefault = true,
                Part = part
            };
            part.PartUnits.Add(newUnit);
            part.DefaultUnit = newUnit.UnitName;
        }
    }
}
```

#### **2.2. Inventory Check Code Generation**

**Format:** `IK-YYYY-NNN`
- `IK`: Prefix cố định
- `YYYY`: Năm (4 chữ số)
- `NNN`: Số thứ tự (3 chữ số, bắt đầu từ 001)

**Logic:**
```csharp
private async Task<string> GenerateCheckCodeAsync()
{
    var year = DateTime.Now.Year;
    var prefix = $"IK-{year}-";
    
    var lastCheck = await _context.InventoryChecks
        .Where(ic => ic.Code.StartsWith(prefix))
        .OrderByDescending(ic => ic.Code)
        .FirstOrDefaultAsync();
    
    int nextNumber = 1;
    if (lastCheck != null)
    {
        var lastNumberStr = lastCheck.Code.Substring(prefix.Length);
        if (int.TryParse(lastNumberStr, out int lastNumber))
        {
            nextNumber = lastNumber + 1;
        }
    }
    
    return $"{prefix}{nextNumber:D3}";
}
```

#### **2.3. Inventory Adjustment Code Generation**

**Format:** `ADJ-YYYY-NNN`
- Tương tự Inventory Check Code Generation

#### **2.4. Inventory Adjustment Approval Workflow**

**Khi approve:**
1. Validate: Đảm bảo tất cả items có `SystemQuantityAfter >= 0`
2. Begin transaction
3. Cập nhật `Part.QuantityInStock` cho từng item:
   ```csharp
   part.QuantityInStock = adjustmentItem.SystemQuantityAfter;
   ```
4. Tạo `StockTransaction` cho từng item:
   ```csharp
   var transaction = new StockTransaction
   {
       TransactionNumber = await GenerateTransactionNumberAsync(),
       PartId = adjustmentItem.PartId,
       TransactionType = adjustmentItem.QuantityChange > 0 ? StockTransactionType.In : StockTransactionType.Out,
       Quantity = Math.Abs(adjustmentItem.QuantityChange),
       RelatedEntity = "InventoryAdjustment",
       RelatedEntityId = adjustment.Id,
       TransactionDate = DateTime.Now,
       Notes = adjustmentItem.Notes
   };
   ```
5. Cập nhật trạng thái adjustment thành "Approved"
6. Ghi nhận người duyệt và thời gian duyệt
7. Commit transaction

#### **2.5. Inventory Check Discrepancy Calculation**

**Khi thêm/cập nhật item:**
```csharp
checkItem.SystemQuantity = part.QuantityInStock; // Tự động lấy
checkItem.DiscrepancyQuantity = checkItem.ActualQuantity - checkItem.SystemQuantity;
checkItem.IsDiscrepancy = checkItem.DiscrepancyQuantity != 0;
```

---

### **3. ARCHITECTURE OVERVIEW**

#### **3.1. Project Structure**
```
GarageManagementSystem/
├── Core/                          # Domain layer
│   ├── Entities/                  # Domain entities
│   ├── Interfaces/                # Repository interfaces
│   ├── Enums/                     # Enumerations
│   └── Extensions/                # Extension methods
├── Infrastructure/                # Data access layer
│   ├── Data/                      # DbContext, Migrations
│   └── Repositories/              # Repository implementations
├── Shared/                        # Shared DTOs and models
│   ├── DTOs/                      # Data Transfer Objects
│   └── Models/                    # Response models
├── API/                           # API layer
│   └── Controllers/               # API controllers
└── Web/                           # Web UI layer
    ├── Controllers/               # MVC controllers
    ├── Views/                     # Razor views
    └── wwwroot/                   # Static files (JS, CSS)
```

#### **3.2. Design Patterns**

**Repository Pattern:**
- `IGenericRepository<T>`: Generic repository interface
- `GenericRepository<T>`: Generic repository implementation
- `IUnitOfWork`: Unit of Work pattern để quản lý transactions

**DTO Pattern:**
- Tách biệt domain entities và data transfer objects
- AutoMapper để map giữa entities và DTOs

**Dependency Injection:**
- Sử dụng built-in DI container của ASP.NET Core
- Register services trong `Program.cs` hoặc `Startup.cs`

#### **3.3. Data Flow**

**API Request Flow:**
```
Client Request
    ↓
API Controller
    ↓
DTO Validation
    ↓
UnitOfWork (Repository)
    ↓
DbContext
    ↓
Database
```

**Response Flow:**
```
Database
    ↓
Entity
    ↓
AutoMapper (Entity → DTO)
    ↓
API Response (ApiResponse<T>)
    ↓
Client
```

---

### **4. CODE STRUCTURE**

#### **4.1. Controllers**

**Base Controller Pattern:**
- Tất cả controllers kế thừa từ `ControllerBase`
- Sử dụng `[Authorize(Policy = "ApiScope")]` cho authentication
- Standard response format: `ApiResponse<T>` hoặc `PagedResponse<T>`

**Error Handling:**
```csharp
try
{
    // Business logic
    return Ok(ApiResponse<T>.SuccessResult(data));
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error message");
    return StatusCode(500, ApiResponse<T>.ErrorResult("Error message", ex.Message));
}
```

#### **4.2. Repositories**

**Generic Repository:**
```csharp
public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
```

**Unit of Work:**
```csharp
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Part> Parts { get; }
    IGenericRepository<Warehouse> Warehouses { get; }
    IGenericRepository<InventoryCheck> InventoryChecks { get; }
    IGenericRepository<InventoryAdjustment> InventoryAdjustments { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

#### **4.3. DTOs**

**Standard DTO Structure:**
```csharp
public class PartDto : BaseDto
{
    public string PartNumber { get; set; }
    public string PartName { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public string? DefaultUnit { get; set; }
    public int QuantityInStock { get; set; }
    public int MinimumStock { get; set; }
    public int? ReorderLevel { get; set; }
    public List<PartUnitDto> Units { get; set; } = new();
}
```

---

### **5. VALIDATION RULES**

#### **5.1. Part Validation**
- SKU: Unique (nếu có), max 100 characters
- Barcode: Unique (nếu có), max 150 characters
- DefaultUnit: Phải có trong PartUnits với IsDefault=true
- MinimumStock: >= 0
- ReorderLevel: >= MinimumStock (nếu có)

#### **5.2. Warehouse Validation**
- Code: Unique, max 50 characters
- Name: Required, max 150 characters

#### **5.3. WarehouseZone Validation**
- Code: Unique trong Warehouse, max 50 characters
- WarehouseId: Required

#### **5.4. WarehouseBin Validation**
- Code: Unique trong Warehouse, max 50 characters
- WarehouseId: Required
- WarehouseZoneId: Optional (nullable)

#### **5.5. Inventory Adjustment Validation**
- SystemQuantityAfter: >= 0 (không được âm)
- Status: Chỉ có thể xóa khi status != "Approved"
- Part: Không được deleted

---

### **6. PERFORMANCE OPTIMIZATION**

#### **6.1. Database Queries**
- Sử dụng `AsNoTracking()` cho read-only queries
- Sử dụng `Include()` và `ThenInclude()` để eager load related entities
- Sử dụng pagination cho large datasets
- Index trên các columns thường xuyên query (Sku, Barcode, Code)

#### **6.2. Caching**
- Cache warehouse data (ít thay đổi)
- Cache parts list với search filters

#### **6.3. Async/Await**
- Tất cả database operations sử dụng async/await
- Parallel processing cho independent operations

---

## 📚 TÀI LIỆU THAM KHẢO

**Lưu ý:** Tất cả các file báo cáo chi tiết đã được tổng hợp vào file này. Các file cũ đã được xóa để tránh trùng lặp.

---

---

## 📋 CÁC PHẦN CÒN THIẾU (Từ kiểm tra cuối cùng)

### **1. Purchase Order Receive - PartInventoryBatch** ⚠️

**Vấn đề:**
- Khi nhận hàng từ Purchase Order, hệ thống **CHƯA tạo PartInventoryBatch**
- PartInventoryBatch cần thiết để:
  - Track hàng có/không hóa đơn (HasInvoice, InvoiceNumber, InvoiceDate)
  - Quản lý lô hàng (BatchNumber, ExpiryDate)
  - Phân biệt hàng dùng cho công ty/bảo hiểm/cá nhân (CanUseForCompany, CanUseForInsurance, CanUseForIndividual)

**Hiện tại:**
- ✅ Đã tăng tồn kho đúng (`Part.QuantityInStock += item.QuantityOrdered`)
- ✅ Đã tạo StockTransaction
- ✅ Đã tạo FinancialTransaction (Expense) với Status = "Pending"
- ❌ **CHƯA tạo PartInventoryBatch**

**Ưu tiên:** ⭐⭐⭐ (Cao) - Cần thiết cho quản lý lô hàng và hóa đơn

---

### **2. Material Request Issue - Financial Transaction (Income)** ⚠️

**Vấn đề:**
- Khi xuất hàng từ Material Request, hệ thống **CHƯA tạo Financial Transaction (Income)**
- Material Request Issue chỉ tính COGS, chưa tạo phiếu thu

**Hiện tại:**
- ✅ Đã giảm tồn kho đúng
- ✅ Đã tạo StockTransaction
- ✅ Đã tính COGS cho ServiceOrder
- ❌ **CHƯA tạo FinancialTransaction (Income)**

**Lưu ý:** 
- Có thể tạo Financial Transaction khi Service Order được complete (thay vì khi Issue MR)
- Hoặc tạo khi có Payment Transaction

**Ưu tiên:** ⭐⭐ (Trung bình) - Tùy thuộc vào quy trình kế toán

---

### **3. Service Order Complete - Invoice & Financial Transaction** ⚠️

**Vấn đề:**
- Khi Service Order được complete, cần kiểm tra xem có tự động tạo Invoice và Financial Transaction (Income) không

**Cần kiểm tra:**
- ✅ Có endpoint `POST /api/service-orders/{id}/complete` không?
- ✅ Có tự động tạo Invoice khi complete không?
- ✅ Có tự động tạo Financial Transaction (Income) khi complete không?

**Hiện tại:**
- Có `InvoiceController.CreateFromServiceOrder()` để tạo Invoice từ Service Order
- Nhưng cần kiểm tra xem có tự động gọi khi complete không

**Ưu tiên:** ⭐⭐⭐ (Cao) - Cần thiết cho quy trình kế toán

---

### **4. Purchase Order - Invoice Validation** ⚠️

**Vấn đề:**
- Khi nhận hàng, cần validate số lượng nhập so với hóa đơn (nếu có)
- Cần input InvoiceNumber, InvoiceDate từ user khi receive

**Hiện tại:**
- PurchaseOrder entity không có InvoiceNumber, InvoiceDate
- Có thể lưu trong PartInventoryBatch (sau khi bổ sung)

**Cần bổ sung:**
- Thêm input InvoiceNumber, InvoiceDate trong ReceiveOrder endpoint
- Validate số lượng nhập so với hóa đơn (nếu có)

**Ưu tiên:** ⭐⭐ (Trung bình) - Tùy thuộc vào yêu cầu nghiệp vụ

---

**Ngày cập nhật:** 2025-01-XX  
**Trạng thái:** 🟢 **92.5% hoàn thành** (Sprint 1: 95%, Sprint 2: 90%)  
**File này là file tổng hợp duy nhất cho Phase 4.1**

