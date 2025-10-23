# 🗑️ HƯỚNG DẪN XÓA MIGRATIONS DƯ THỪA

## ⚠️ **CẢNH BÁO QUAN TRỌNG**

**KHÔNG BAO GIỜ XÓA MIGRATIONS** nếu:
- ❌ Database production đang sử dụng migrations này
- ❌ Team members khác đang làm việc với migrations
- ❌ Chưa backup database và migration history
- ❌ Chưa test trên development environment

## ✅ **ĐIỀU KIỆN AN TOÀN ĐỂ XÓA**

### **1. Database Development Only**
- ✅ Chỉ có development database
- ✅ Không có production data quan trọng
- ✅ Có thể reset database hoàn toàn

### **2. Team Agreement**
- ✅ Tất cả team members đồng ý
- ✅ Không có ai đang làm việc với migrations
- ✅ Đã sync code với team

### **3. Backup Complete**
- ✅ Backup database hiện tại
- ✅ Backup migration files (để rollback nếu cần)
- ✅ Backup `__EFMigrationsHistory` table

## 🚀 **QUY TRÌNH XÓA AN TOÀN**

### **Bước 1: Backup Everything**
```bash
# Backup database
mysqldump -u root -p GarageManagementDB > backup_before_migration_cleanup.sql

# Backup migration files
cp -r src/GarageManagementSystem.Infrastructure/Migrations/ migrations_backup/

# Backup migration history
mysqldump -u root -p GarageManagementDB __EFMigrationsHistory > migration_history_backup.sql
```

### **Bước 2: Test Consolidated Script**
```sql
-- Test trên database copy
CREATE DATABASE GarageManagementDB_Test;
USE GarageManagementDB_Test;
source D:/Source/GaraManager/docs/CONSOLIDATED_DATABASE_SCHEMA.sql;
```

### **Bước 3: Reset Development Database**
```sql
-- Reset hoàn toàn development database
USE GarageManagementDB;
source D:/Source/GaraManager/docs/DROP_ALL_TABLES.sql;
source D:/Source/GaraManager/docs/CONSOLIDATED_DATABASE_SCHEMA.sql;
source D:/Source/GaraManager/docs/DEMO_DATA_COMPLETE.sql;
```

### **Bước 4: Xóa Migration Files**
```bash
# Xóa tất cả migration files (giữ lại ModelSnapshot)
cd src/GarageManagementSystem.Infrastructure/Migrations/

# Xóa các file migration cũ
rm 20251012073417_InitialCreate.cs
rm 20251012073417_InitialCreate.Designer.cs
rm 20251014023327_AddMileageToVehicle.cs
rm 20251014023327_AddMileageToVehicle.Designer.cs
rm 20251014052214_AddPrintTemplateTable.cs
rm 20251014052214_AddPrintTemplateTable.Designer.cs
rm 20251014061829_AddPrintTemplatesTable.cs
rm 20251014061829_AddPrintTemplatesTable.Designer.cs
rm 20251014103414_AddHasInvoiceToQuotationItem.cs
rm 20251014103414_AddHasInvoiceToQuotationItem.Designer.cs
rm 20251015012805_AddCustomerReceptionAndWorkflowTracking.cs
rm 20251015012805_AddCustomerReceptionAndWorkflowTracking.Designer.cs
rm 20251015100112_ConvertReceptionStatusToEnum.cs
rm 20251015100112_ConvertReceptionStatusToEnum.Designer.cs
rm 20251015122942_AddPricingModelsToServiceAndQuotationItem.cs
rm 20251015122942_AddPricingModelsToServiceAndQuotationItem.Designer.cs
rm 20251015123726_AddPricingModelSupport.cs
rm 20251015123726_AddPricingModelSupport.Designer.cs
rm 20251016004206_UpdateServicePricingFields.cs
rm 20251016004206_UpdateServicePricingFields.Designer.cs
rm 20251016021540_IncreasePricingBreakdownLength.cs
rm 20251016021540_IncreasePricingBreakdownLength.Designer.cs
rm 20251016031914_AddItemCategoryToQuotationItem.cs
rm 20251016031914_AddItemCategoryToQuotationItem.Designer.cs
rm 20251019153822_AddQuotationAttachmentAndInsurancePricing.cs
rm 20251019153822_AddQuotationAttachmentAndInsurancePricing.Designer.cs
rm 20251020074835_AddCorporateFieldsToQuotation.cs
rm 20251020074835_AddCorporateFieldsToQuotation.Designer.cs
rm 20251022102808_AddVATFieldsToPartAndQuotationItem.cs
rm 20251022102808_AddVATFieldsToPartAndQuotationItem.Designer.cs

# GIỮ LẠI: GarageDbContextModelSnapshot.cs
```

### **Bước 5: Tạo Migration Mới (Optional)**
```bash
# Nếu muốn tạo migration mới từ đầu
cd src/GarageManagementSystem.Infrastructure
dotnet ef migrations add InitialCreate --context GarageDbContext -p . -s ../GarageManagementSystem.API
```

### **Bước 6: Test Application**
```bash
# Test build
dotnet build GaraManager.sln

# Test run
cd src/GarageManagementSystem.API
dotnet run
```

## 🔄 **ROLLBACK NẾU CÓ VẤN ĐỀ**

### **Khôi Phục Migration Files**
```bash
# Restore từ backup
cp -r migrations_backup/* src/GarageManagementSystem.Infrastructure/Migrations/
```

### **Khôi Phục Database**
```bash
# Restore database
mysql -u root -p GarageManagementDB < backup_before_migration_cleanup.sql
```

## 📊 **SAU KHI XÓA**

### **Files Còn Lại**
```
src/GarageManagementSystem.Infrastructure/Migrations/
└── GarageDbContextModelSnapshot.cs  (GIỮ LẠI)
```

### **Files Mới**
```
docs/
├── CONSOLIDATED_DATABASE_SCHEMA.sql  (MAIN)
├── CREATE_DATABASE_FROM_DBCONTEXT.sql (BACKUP)
└── DROP_ALL_TABLES.sql               (RESET)
```

## ⚠️ **LƯU Ý QUAN TRỌNG**

### **1. EF Core Behavior**
- ✅ EF Core sẽ hoạt động bình thường với `CONSOLIDATED_DATABASE_SCHEMA.sql`
- ✅ `GarageDbContextModelSnapshot.cs` vẫn cần thiết cho EF Core
- ✅ Có thể tạo migration mới bất cứ lúc nào

### **2. Team Development**
- ⚠️ **Tất cả team members** phải sync code sau khi xóa
- ⚠️ **Không ai được** tạo migration mới cho đến khi sync
- ⚠️ **Database reset** có thể cần thiết cho tất cả members

### **3. Production Deployment**
- ❌ **KHÔNG BAO GIỜ** xóa migrations trên production
- ❌ **KHÔNG BAO GIỜ** deploy code đã xóa migrations
- ✅ Chỉ sử dụng `CONSOLIDATED_DATABASE_SCHEMA.sql` cho setup mới

## 🎯 **KHUYẾN NGHỊ**

### **Option 1: Xóa Hoàn Toàn (Khuyến nghị cho Development)**
- ✅ Xóa tất cả migration files cũ
- ✅ Sử dụng `CONSOLIDATED_DATABASE_SCHEMA.sql` làm single source of truth
- ✅ Tạo migration mới khi cần thiết

### **Option 2: Giữ Lại (An toàn hơn)**
- ✅ Giữ migration files trong thư mục riêng
- ✅ Sử dụng `CONSOLIDATED_DATABASE_SCHEMA.sql` cho deployment
- ✅ Có thể rollback nếu cần

### **Option 3: Archive (Compromise)**
- ✅ Move migration files vào thư mục `Migrations_Archive/`
- ✅ Giữ `CONSOLIDATED_DATABASE_SCHEMA.sql` làm main
- ✅ Có thể reference lại nếu cần

## 🚨 **EMERGENCY RECOVERY**

Nếu có vấn đề nghiêm trọng:

```bash
# 1. Stop tất cả services
# 2. Restore từ backup
mysql -u root -p GarageManagementDB < backup_before_migration_cleanup.sql

# 3. Restore migration files
cp -r migrations_backup/* src/GarageManagementSystem.Infrastructure/Migrations/

# 4. Rebuild và test
dotnet build GaraManager.sln
dotnet run
```

---

**⚠️ QUAN TRỌNG: Chỉ thực hiện trên development environment và khi đã backup đầy đủ!**
