# 📊 MIGRATIONS CONSOLIDATION

## 🎯 Mục Đích

Tổng hợp tất cả migrations của GarageDbContext thành **1 file duy nhất** để:
- ✅ **Đơn giản hóa**: Không cần quản lý nhiều migration files
- ✅ **Idempotent**: Có thể chạy nhiều lần an toàn
- ✅ **Tự động**: Tự động check và apply migrations chưa có
- ✅ **Đầy đủ**: Bao gồm tất cả migrations từ InitialCreate đến AddVATFieldsToPartAndQuotationItem

## 📁 Files

### **CONSOLIDATED_DATABASE_SCHEMA.sql** ⭐ MAIN
- **Kích thước**: 317 KB
- **Migrations**: 15 migrations (từ InitialCreate đến AddVATFieldsToPartAndQuotationItem)
- **Tính năng**: Idempotent, tự động check migration history
- **Sử dụng**: Thay thế tất cả migration files riêng lẻ

### **Individual Migration Files** (Legacy)
```
src/GarageManagementSystem.Infrastructure/Migrations/
├── 20251012073417_InitialCreate.cs
├── 20251014023327_AddMileageToVehicle.cs
├── 20251014052214_AddPrintTemplateTable.cs
├── 20251014061829_AddPrintTemplatesTable.cs
├── 20251014103414_AddHasInvoiceToQuotationItem.cs
├── 20251015012805_AddCustomerReceptionAndWorkflowTracking.cs
├── 20251015100112_ConvertReceptionStatusToEnum.cs
├── 20251015122942_AddPricingModelsToServiceAndQuotationItem.cs
├── 20251015123726_AddPricingModelSupport.cs
├── 20251016004206_UpdateServicePricingFields.cs
├── 20251016021540_IncreasePricingBreakdownLength.cs
├── 20251016031914_AddItemCategoryToQuotationItem.cs
├── 20251019153822_AddQuotationAttachmentAndInsurancePricing.cs
├── 20251020074835_AddCorporateFieldsToQuotation.cs
└── 20251022102808_AddVATFieldsToPartAndQuotationItem.cs
```

## 🚀 Cách Sử Dụng

### **Setup Database Mới**
```sql
-- MySQL Workbench
source D:/Source/GaraManager/docs/CONSOLIDATED_DATABASE_SCHEMA.sql
```

### **Update Database Hiện Tại**
```sql
-- MySQL Workbench (idempotent)
source D:/Source/GaraManager/docs/CONSOLIDATED_DATABASE_SCHEMA.sql
```

### **Re-generate File**
```bash
# Terminal
cd src/GarageManagementSystem.Infrastructure
dotnet ef migrations script 0 --context GarageDbContext \
  -p . -s ../GarageManagementSystem.API \
  -o ../../docs/CONSOLIDATED_DATABASE_SCHEMA.sql \
  --idempotent
```

## 🔧 Cách Hoạt Động

### **Idempotent Logic**
```sql
-- Mỗi migration được wrap trong procedure
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` 
                  WHERE `MigrationId` = '20251012073417_InitialCreate') THEN
        -- Migration logic here
        INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
        VALUES ('20251012073417_InitialCreate', '8.0.11');
    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;
```

### **Migration History Tracking**
- ✅ Tự động check `__EFMigrationsHistory` table
- ✅ Chỉ apply migrations chưa có
- ✅ Update migration history sau khi apply
- ✅ An toàn chạy nhiều lần

## 📊 So Sánh

| Aspect | Individual Files | Consolidated File |
|--------|------------------|-------------------|
| **Số lượng files** | 15+ files | 1 file |
| **Quản lý** | Phức tạp | Đơn giản |
| **Idempotent** | ❌ | ✅ |
| **Tự động check** | ❌ | ✅ |
| **Kích thước** | Phân tán | 317 KB |
| **Maintenance** | Khó | Dễ |

## ⚠️ Lưu Ý

1. **Backup dữ liệu** trước khi chạy script
2. **File tổng hợp** được khuyến nghị sử dụng cho mọi trường hợp
3. **Individual files** vẫn tồn tại nhưng không cần thiết
4. **Re-generate** file khi có migration mới
5. **EF Core Tools** cần thiết để generate script

## 🎯 Kết Luận

**CONSOLIDATED_DATABASE_SCHEMA.sql** là giải pháp tối ưu để quản lý database schema:
- ✅ **Single Source of Truth**
- ✅ **Idempotent và an toàn**
- ✅ **Dễ sử dụng và maintain**
- ✅ **Thay thế hoàn toàn individual migration files**

---

**📄 Tài liệu này được cập nhật thường xuyên. Vui lòng kiểm tra phiên bản mới nhất.**
