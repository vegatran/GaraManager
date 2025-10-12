# HƯỚNG DẪN RESET VÀ TẠO LẠI DATABASE TỪ ĐẦU

## ⚠️ CẢNH BÁO
**Các lệnh này sẽ XÓA TẤT CẢ DỮ LIỆU trong database!**  
Chỉ thực hiện khi bạn muốn reset hoàn toàn database.

---

## 📋 CÁC BƯỚC THỰC HIỆN

### Bước 1: DROP tất cả tables hiện tại

```sql
-- Chạy trên MySQL Workbench
source D:/Source/GaraManager/docs/DROP_ALL_TABLES.sql
```

### Bước 2: Tạo lại tất cả tables từ migration mới

```sql
-- Chạy trên MySQL Workbench
source D:/Source/GaraManager/docs/CREATE_ALL_TABLES_FRESH.sql
```

### Bước 3: Load demo data (optional)

```sql
-- Chạy trên MySQL Workbench
source D:/Source/GaraManager/docs/DEMO_DATA_ONLY.sql
```

---

## 📁 FILES QUAN TRỌNG

### 1. **DROP_ALL_TABLES.sql**
- Xóa tất cả tables
- Tắt foreign key checks để xóa theo thứ tự bất kỳ

### 2. **CREATE_ALL_TABLES_FRESH.sql** ⭐
- Tạo TẤT CẢ tables mới theo entities mới nhất
- Được generate từ migration `InitialCreate`
- Bao gồm tất cả columns, indexes, foreign keys

### 3. **DEMO_DATA_ONLY.sql**
- Load demo data cho Phase 1 & 2
- Sử dụng variables để tránh hardcode IDs
- Có verification queries

---

## 🔄 ALTERNATIVE: Sử dụng EF Core Migrations

Nếu muốn dùng .NET tool thay vì SQL script:

```bash
# Bước 1: Xóa database
dotnet ef database drop --context GarageDbContext -p src/GarageManagementSystem.Infrastructure -s src/GarageManagementSystem.API --force

# Bước 2: Tạo lại database
dotnet ef database update --context GarageDbContext -p src/GarageManagementSystem.Infrastructure -s src/GarageManagementSystem.API
```

---

## ✅ TRẠNG THÁI MIGRATIONS

### Migrations Cũ (ĐÃ XÓA):
- ❌ InitialMySQL
- ❌ UpdateEntitiesForPhase1API
- ❌ AddAuditLogAndPhase4Updates
- ❌ AddStatusToServiceOrderItemsAndParts
- ❌ AddAllMissingTables
- ❌ AddMissingColumnsToTables

### Migration Mới:
- ✅ **InitialCreate** - Tạo TẤT CẢ tables từ đầu với entities mới nhất

---

## 🎯 LỢI ÍCH

1. ✅ **Clean Start**: Database hoàn toàn đồng bộ với entities
2. ✅ **No Conflicts**: Không còn vấn đề column mismatch
3. ✅ **Proper Relationships**: Tất cả foreign keys được thiết lập đúng
4. ✅ **Latest Schema**: Schema mới nhất từ entities

---

## 📊 KẾT QUẢ

Sau khi hoàn thành, bạn sẽ có:
- Database sạch với schema mới nhất
- Tất cả tables, columns, indexes, foreign keys đúng như entities
- Demo data đầy đủ cho testing (nếu chạy Bước 3)

---

## 💡 LƯU Ý

1. **Backup dữ liệu quan trọng** trước khi chạy DROP_ALL_TABLES.sql
2. Nếu gặp lỗi foreign key, đảm bảo `SET FOREIGN_KEY_CHECKS = 0` đã được set
3. Script `CREATE_ALL_TABLES_FRESH.sql` cũng tạo bảng `__EFMigrationsHistory` để track migrations
4. Sau khi tạo tables, có thể chạy `DEMO_DATA_ONLY.sql` để có dữ liệu test

---

**Date Created**: 12/10/2025  
**Migration Version**: InitialCreate

