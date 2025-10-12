# HƯỚNG DẪN SETUP DATABASE

## 📁 CÁC FILE QUAN TRỌNG

### 1. **`DROP_ALL_TABLES.sql`** ⚠️
Xóa TẤT CẢ tables trong database (để reset hoàn toàn).

### 2. **`CREATE_DATABASE_FROM_DBCONTEXT.sql`** ⭐
Tạo TẤT CẢ tables từ GarageDbContext.

**Đặc điểm:**
- ✅ **Idempotent**: Có thể chạy nhiều lần an toàn
- ✅ **Đầy đủ**: 45 tables từ DbContext
- ✅ **Chính xác 100%**: Match với entities trong code
- ✅ **Tự động check**: Chỉ tạo table chưa có
- ✅ **Foreign Keys**: Tất cả relationships đã đúng
- ✅ **Indexes**: Đầy đủ indexes cho performance
- ✅ **155 KB**: Full schema

### 3. **`DEMO_DATA_COMPLETE.sql`** 🎯
Load demo data đầy đủ cho testing (2 workflows hoàn chỉnh).

---

## 🚀 HƯỚNG DẪN SETUP

### ⭐ SETUP MỚI HOÀN TOÀN (Khuyến nghị)

**Trên MySQL Workbench:**

```sql
-- Bước 1: DROP tất cả tables (reset hoàn toàn)
source D:/Source/GaraManager/docs/DROP_ALL_TABLES.sql

-- Bước 2: Tạo lại tất cả tables từ DbContext
source D:/Source/GaraManager/docs/CREATE_DATABASE_FROM_DBCONTEXT.sql

-- Bước 3: Load demo data (optional)
source D:/Source/GaraManager/docs/DEMO_DATA_COMPLETE.sql
```

### 🔄 UPDATE DATABASE HIỆN TẠI

Nếu database đã có và chỉ cần update schema:

```sql
-- Chỉ cần chạy file này (idempotent)
source D:/Source/GaraManager/docs/CREATE_DATABASE_FROM_DBCONTEXT.sql
```

Script sẽ tự động:
- ✅ Check table đã tồn tại → Skip
- ✅ Table chưa có → Tạo mới
- ✅ Update migration history

### 🛠️ SỬ DỤNG EF CORE (Alternative)

```bash
# Drop database
dotnet ef database drop --context GarageDbContext \
  -p src/GarageManagementSystem.Infrastructure \
  -s src/GarageManagementSystem.API --force

# Create/Update database
dotnet ef database update --context GarageDbContext \
  -p src/GarageManagementSystem.Infrastructure \
  -s src/GarageManagementSystem.API
```

---

## 📊 TABLES ĐƯỢC TẠO

**Total: 46 tables**

### Core Tables:
- AuditLogs
- Customers
- Departments
- Positions
- Employees

### Service Management:
- Services
- ServiceTypes
- ServiceOrders
- ServiceOrderItems
- ServiceOrderParts
- ServiceOrderLabors

### Inventory:
- Parts
- PartGroups
- PartSuppliers
- PartInventoryBatches
- PartBatchUsages
- StockTransactions

### Suppliers:
- Suppliers
- PurchaseOrders
- PurchaseOrderItems

### Vehicles:
- Vehicles
- VehicleBrands
- VehicleModels
- VehicleInspections
- VehicleInsurances
- EngineSpecifications

### Workflow:
- VehicleInspections
- InspectionIssues
- InspectionPhotos
- ServiceQuotations
- QuotationItems
- Appointments

### Invoicing:
- Invoices
- InvoiceItems
- Payments
- PaymentTransactions

### Insurance:
- InsuranceClaims
- InsuranceClaimDocuments
- InsuranceInvoices
- InsuranceInvoiceItems

### Financial:
- FinancialTransactions
- FinancialTransactionAttachments

### Labor:
- LaborCategories
- LaborItems

### Others:
- SystemConfigurations
- PartGroupCompatibilities

---

## 🔄 RE-GENERATE SCRIPT

Nếu cần generate lại sau khi update entities:

```bash
dotnet ef migrations script 0 --context GarageDbContext \
  -p src/GarageManagementSystem.Infrastructure \
  -s src/GarageManagementSystem.API \
  -o docs/CREATE_DATABASE_FROM_DBCONTEXT.sql \
  --idempotent
```

---

## ⚠️ LƯU Ý

1. **Backup dữ liệu** trước khi chạy script nếu database đã có data
2. File script này chỉ **CREATE tables**, không **ALTER** tables hiện có
3. Nếu muốn **reset hoàn toàn**, cần DROP tables trước
4. Script có thể mất vài phút để chạy (46+ tables)
5. Đảm bảo user `usergara` có quyền CREATE TABLE

---

## 📝 MIGRATION VERSION

- **Migration**: InitialCreate
- **Date**: 12/10/2025
- **Source**: GarageDbContext
- **EF Core**: 8.0

---

**File này là nguồn chân lý duy nhất (Single Source of Truth) cho database schema!** ✅

