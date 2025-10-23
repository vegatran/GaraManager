# HƯỚNG DẪN SETUP DATABASE

## 📁 CÁC FILE QUAN TRỌNG

### ✅ **FILES CẦN THIẾT (Chỉ còn lại 4 files)**

#### 1. **`CONSOLIDATED_DATABASE_SCHEMA.sql`** ⭐ MAIN
Tổng hợp TẤT CẢ migrations thành 1 file duy nhất.

**Đặc điểm:**
- ✅ **Idempotent**: Có thể chạy nhiều lần an toàn
- ✅ **Đầy đủ**: Tất cả migrations từ InitialCreate đến AddVATFieldsToPartAndQuotationItem
- ✅ **Tự động check**: Chỉ apply migration chưa có trong `__EFMigrationsHistory`
- ✅ **317 KB**: Full consolidated schema
- ✅ **Single file**: Thay thế tất cả migration files riêng lẻ

#### 2. **`CREATE_DATABASE_FROM_DBCONTEXT.sql`** ⭐ BACKUP
Tạo TẤT CẢ tables từ GarageDbContext (backup option).

**Đặc điểm:**
- ✅ **Idempotent**: Có thể chạy nhiều lần an toàn
- ✅ **Đầy đủ**: 46 tables từ DbContext
- ✅ **Chính xác 100%**: Match với entities trong code
- ✅ **Tự động check**: Chỉ tạo table chưa có
- ✅ **Foreign Keys**: Tất cả relationships đã đúng
- ✅ **Indexes**: Đầy đủ indexes cho performance
- ✅ **155 KB**: Full schema

#### 3. **`DROP_ALL_TABLES.sql`** ⚠️ RESET
Xóa TẤT CẢ tables trong database (để reset hoàn toàn).

#### 4. **`DEMO_DATA_COMPLETE.sql`** 🎯 DEMO
Load demo data đầy đủ cho testing (2 workflows hoàn chỉnh).

### ❌ **FILES ĐÃ XÓA (Dư thừa)**
- ~~`ClearStockData.sql`~~ - Script clear dữ liệu cũ
- ~~`FixPurchaseOrderData.sql`~~ - Script fix dữ liệu cũ  
- ~~`ImportStockData.sql`~~ - Script import cũ
- ~~`ImportStockDataCorrected.sql`~~ - Script import cũ đã sửa
- ~~`ImportStockDataFinal.sql`~~ - Script import cũ
- ~~`ImportStockDataFixed.sql`~~ - Script import cũ
- ~~`ImportStockDataMinimal.sql`~~ - Script import cũ
- ~~`ImportStockDataPerfect.sql`~~ - Script import cũ
- ~~`InsertDefaultQuotationTemplate.sql`~~ - Script insert template cũ
- ~~`InsertTemplate.sql`~~ - Script insert template cũ

---

## 🚀 HƯỚNG DẪN SETUP

### ⭐ SETUP MỚI HOÀN TOÀN (Khuyến nghị)

**Trên MySQL Workbench:**

```sql
-- Bước 1: DROP tất cả tables (reset hoàn toàn)
source D:/Source/GaraManager/docs/DROP_ALL_TABLES.sql

-- Bước 2: Tạo lại tất cả tables từ DbContext (2 options)
-- Option A: Sử dụng file tổng hợp (KHUYẾN NGHỊ)
source D:/Source/GaraManager/docs/CONSOLIDATED_DATABASE_SCHEMA.sql

-- Option B: Sử dụng file cơ bản
source D:/Source/GaraManager/docs/CREATE_DATABASE_FROM_DBCONTEXT.sql

-- Bước 3: Load demo data (optional)
source D:/Source/GaraManager/docs/DEMO_DATA_COMPLETE.sql
```

### 🔄 UPDATE DATABASE HIỆN TẠI

Nếu database đã có và chỉ cần update schema:

```sql
-- Option A: Sử dụng file tổng hợp (KHUYẾN NGHỊ)
source D:/Source/GaraManager/docs/CONSOLIDATED_DATABASE_SCHEMA.sql

-- Option B: Sử dụng file cơ bản
source D:/Source/GaraManager/docs/CREATE_DATABASE_FROM_DBCONTEXT.sql
```

Script sẽ tự động:
- ✅ Check migration đã apply → Skip
- ✅ Migration chưa có → Apply
- ✅ Update `__EFMigrationsHistory` table
- ✅ Idempotent (an toàn chạy nhiều lần)

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
# Generate file tổng hợp (KHUYẾN NGHỊ)
dotnet ef migrations script 0 --context GarageDbContext \
  -p src/GarageManagementSystem.Infrastructure \
  -s src/GarageManagementSystem.API \
  -o docs/CONSOLIDATED_DATABASE_SCHEMA.sql \
  --idempotent

# Generate file cơ bản
dotnet ef migrations script 0 --context GarageDbContext \
  -p src/GarageManagementSystem.Infrastructure \
  -s src/GarageManagementSystem.API \
  -o docs/CREATE_DATABASE_FROM_DBCONTEXT.sql \
  --idempotent
```

---

## 🗑️ **XÓA MIGRATIONS DƯ THỪA**

Sau khi có `CONSOLIDATED_DATABASE_SCHEMA.sql`, có thể xóa các migration files cũ:

### **⚠️ CẢNH BÁO**
- ❌ **KHÔNG BAO GIỜ** xóa trên production
- ❌ **KHÔNG BAO GIỜ** xóa khi team đang làm việc
- ✅ **CHỈ XÓA** trên development environment
- ✅ **BACKUP** trước khi xóa

### **🚀 Cách Xóa An Toàn**

#### **Option 1: Sử dụng Script PowerShell**
```powershell
# Test trước (không xóa thực sự)
.\scripts\cleanup-migrations.ps1 -WhatIf

# Xóa thực sự (có backup tự động)
.\scripts\cleanup-migrations.ps1

# Xóa không backup (cẩn thận!)
.\scripts\cleanup-migrations.ps1 -Backup:$false -Force
```

#### **Option 2: Xóa Thủ Công**
```bash
# Backup trước
cp -r src/GarageManagementSystem.Infrastructure/Migrations/ migrations_backup/

# Xóa từng file (giữ lại GarageDbContextModelSnapshot.cs)
rm src/GarageManagementSystem.Infrastructure/Migrations/202510*.cs
```

### **📋 Files Sẽ Bị Xóa**
- ✅ `20251012073417_InitialCreate.cs` + `.Designer.cs`
- ✅ `20251014023327_AddMileageToVehicle.cs` + `.Designer.cs`
- ✅ `20251014052214_AddPrintTemplateTable.cs` + `.Designer.cs`
- ✅ `20251014061829_AddPrintTemplatesTable.cs` + `.Designer.cs`
- ✅ `20251014103414_AddHasInvoiceToQuotationItem.cs` + `.Designer.cs`
- ✅ `20251015012805_AddCustomerReceptionAndWorkflowTracking.cs` + `.Designer.cs`
- ✅ `20251015100112_ConvertReceptionStatusToEnum.cs` + `.Designer.cs`
- ✅ `20251015122942_AddPricingModelsToServiceAndQuotationItem.cs` + `.Designer.cs`
- ✅ `20251015123726_AddPricingModelSupport.cs` + `.Designer.cs`
- ✅ `20251016004206_UpdateServicePricingFields.cs` + `.Designer.cs`
- ✅ `20251016021540_IncreasePricingBreakdownLength.cs` + `.Designer.cs`
- ✅ `20251016031914_AddItemCategoryToQuotationItem.cs` + `.Designer.cs`
- ✅ `20251019153822_AddQuotationAttachmentAndInsurancePricing.cs` + `.Designer.cs`
- ✅ `20251020074835_AddCorporateFieldsToQuotation.cs` + `.Designer.cs`
- ✅ `20251022102808_AddVATFieldsToPartAndQuotationItem.cs` + `.Designer.cs`

### **📋 Files Giữ Lại**
- ✅ `GarageDbContextModelSnapshot.cs` (CẦN THIẾT cho EF Core)

### **🔄 Rollback Nếu Cần**
```bash
# Khôi phục từ backup
cp -r migrations_backup/* src/GarageManagementSystem.Infrastructure/Migrations/
```

---

## ⚠️ LƯU Ý

1. **Backup dữ liệu** trước khi chạy script nếu database đã có data
2. **CONSOLIDATED_DATABASE_SCHEMA.sql**: Tổng hợp tất cả migrations, tự động check và apply
3. **CREATE_DATABASE_FROM_DBCONTEXT.sql**: Chỉ CREATE tables, không ALTER tables hiện có
4. Nếu muốn **reset hoàn toàn**, cần DROP tables trước
5. Script có thể mất vài phút để chạy (46+ tables)
6. Đảm bảo user `usergara` có quyền CREATE TABLE
7. **File tổng hợp** được khuyến nghị sử dụng cho mọi trường hợp

---

## 📝 MIGRATION VERSION

### **CONSOLIDATED_DATABASE_SCHEMA.sql**
- **Migrations**: InitialCreate → AddVATFieldsToPartAndQuotationItem
- **Total**: 15 migrations
- **Date**: 23/10/2025
- **Size**: 317 KB
- **Source**: GarageDbContext
- **EF Core**: 8.0

### **CREATE_DATABASE_FROM_DBCONTEXT.sql**
- **Migration**: InitialCreate only
- **Date**: 12/10/2025
- **Size**: 155 KB
- **Source**: GarageDbContext
- **EF Core**: 8.0

---

## 🔄 **RESET VÀ TẠO LẠI DATABASE TỪ ĐẦU**

### **⚠️ CẢNH BÁO**
**Các lệnh này sẽ XÓA TẤT CẢ DỮ LIỆU trong database!**  
Chỉ thực hiện khi bạn muốn reset hoàn toàn database.

### **📋 CÁC BƯỚC THỰC HIỆN**

#### **Bước 1: DROP tất cả tables hiện tại**
```sql
-- Chạy trên MySQL Workbench
source D:/Source/GaraManager/docs/DROP_ALL_TABLES.sql
```

#### **Bước 2: Tạo lại tất cả tables từ migration mới**
```sql
-- Chạy trên MySQL Workbench
source D:/Source/GaraManager/docs/CREATE_DATABASE_FROM_DBCONTEXT.sql
```

#### **Bước 3: Load demo data (optional)**
```sql
-- Chạy trên MySQL Workbench
source D:/Source/GaraManager/docs/DEMO_DATA_COMPLETE.sql
```

---

## 🚀 **EF CORE MIGRATIONS GUIDE**

### **🎯 Tại Sao Nên Dùng EF Core Migrations Thay Vì SQL Scripts?**

#### **✅ Ưu Điểm EF Core Migrations:**
1. **Tự động tạo từ Entities** - Không cần viết SQL thủ công
2. **Version control** - Track changes theo thời gian
3. **Rollback dễ dàng** - Có thể quay lại version cũ
4. **Cross-database** - Tự động generate SQL cho MySQL/SQL Server/PostgreSQL
5. **Type-safe** - Compiler check lỗi ngay
6. **Team collaboration** - Merge migrations dễ dàng hơn

#### **❌ Nhược Điểm SQL Scripts:**
1. ❌ Phải viết SQL thủ công
2. ❌ Dễ sai syntax
3. ❌ Khó maintain
4. ❌ Không track changes
5. ❌ Không rollback được
6. ❌ Phải viết lại cho mỗi database engine

### **🛠️ HƯỚNG DẪN SỬ DỤNG EF CORE MIGRATIONS**

#### **1. Cài Đặt Tools**
```bash
# Cài EF Core Tools (global)
dotnet tool install --global dotnet-ef

# Hoặc update nếu đã có
dotnet tool update --global dotnet-ef

# Verify
dotnet ef --version
```

#### **2. Tạo Migration Mới**
```bash
# Di chuyển vào thư mục Infrastructure
cd src/GarageManagementSystem.Infrastructure

# Tạo migration mới
dotnet ef migrations add MigrationName

# Ví dụ:
dotnet ef migrations add AddVATFieldsToPartAndQuotationItem
```

#### **3. Cập Nhật Database**
```bash
# Apply migration lên database
dotnet ef database update

# Hoặc apply migration cụ thể
dotnet ef database update MigrationName
```

#### **4. Rollback Migration**
```bash
# Rollback về migration trước đó
dotnet ef database update PreviousMigrationName

# Rollback về migration đầu tiên
dotnet ef database update 0
```

#### **5. Xem Migration History**
```bash
# Xem danh sách migrations đã apply
dotnet ef migrations list

# Xem SQL script của migration
dotnet ef migrations script MigrationName
```

---

## 📊 **DATABASE MIGRATIONS GUIDE**

### **🗂️ Migration Files**

#### **1. InitialCreate** (Base Schema)
**Status:** ✅ Already applied  
**Purpose:** Initial database schema with core tables  
**Run:** Only once when setting up new database

**Contains:**
- Core tables: Customer, Vehicle, Employee, Service, Part, etc.
- Basic relationships and indexes
- Initial structure

#### **2. AddVATFieldsToPartAndQuotationItem** (VAT Implementation)
**Status:** ✅ Applied  
**Purpose:** Add VAT fields to Part and QuotationItem entities  
**Run:** After base schema is in place

**Adds:**
- `VATRate` and `IsVATApplicable` to Parts table
- `OverrideVATRate` and `OverrideIsVATApplicable` to QuotationItems table
- `VATRate` to PurchaseOrders table

#### **3. Future Migrations**
**Status:** 🔄 Planned  
**Purpose:** Additional features and improvements  
**Run:** As needed for new features

### **📋 Migration Best Practices**

#### **✅ Do's:**
- Always backup database before running migrations
- Test migrations on development database first
- Use descriptive migration names
- Review generated SQL before applying
- Keep migrations small and focused

#### **❌ Don'ts:**
- Don't modify existing migrations after they've been applied to production
- Don't delete migration files from the project
- Don't run migrations directly on production without testing
- Don't ignore migration conflicts in team development

### **🔧 Troubleshooting**

#### **Common Issues:**

1. **Migration conflicts:**
   ```bash
   # Reset migrations (DANGEROUS - only for development)
   dotnet ef database drop
   dotnet ef migrations remove
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

2. **Connection string issues:**
   - Check `appsettings.json` for correct connection string
   - Ensure database server is running
   - Verify user permissions

3. **Migration not found:**
   ```bash
   # List all migrations
   dotnet ef migrations list
   
   # Check migration files exist in Migrations folder
   ls src/GarageManagementSystem.Infrastructure/Migrations/
   ```

---

## 📝 **CHANGELOG**

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2024-01-15 | Tạo tài liệu hướng dẫn ban đầu |
| 1.1 | 2024-01-20 | Thêm phần EF Core Migrations |
| 1.2 | 2024-01-25 | Cập nhật tính năng reset database |
| 1.3 | 2024-01-30 | **Thêm tính năng tính thuế VAT** - Hỗ trợ tỷ lệ 0%, 8%, 10% |
| 2.0 | 2024-10-22 | **Tổng hợp tài liệu** - Kết hợp Database Setup, Reset, EF Core Migrations |
| 2.1 | 2024-10-23 | **Tổng hợp migrations** - Tạo CONSOLIDATED_DATABASE_SCHEMA.sql (317KB, 15 migrations) |
| 2.2 | 2024-10-23 | **Hướng dẫn xóa migrations** - Script PowerShell và quy trình an toàn |
| 2.3 | 2024-10-23 | **Dọn dẹp file SQL** - Xóa 10 file SQL dư thừa, chỉ giữ lại 4 file cần thiết |

---

**CONSOLIDATED_DATABASE_SCHEMA.sql là nguồn chân lý duy nhất (Single Source of Truth) cho database schema!** ✅

**📄 Tài liệu này được cập nhật thường xuyên. Vui lòng kiểm tra phiên bản mới nhất.**

