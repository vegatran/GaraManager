# 🚀 EF Core Migrations Guide

## 🎯 Tại Sao Nên Dùng EF Core Migrations Thay Vì SQL Scripts?

### ✅ **Ưu Điểm EF Core Migrations:**

1. **Tự động tạo từ Entities** - Không cần viết SQL thủ công
2. **Version control** - Track changes theo thời gian
3. **Rollback dễ dàng** - Có thể quay lại version cũ
4. **Cross-database** - Tự động generate SQL cho MySQL/SQL Server/PostgreSQL
5. **Type-safe** - Compiler check lỗi ngay
6. **Team collaboration** - Merge migrations dễ dàng hơn

### ❌ **Nhược Điểm SQL Scripts:**

1. ❌ Phải viết SQL thủ công
2. ❌ Dễ sai syntax
3. ❌ Khó maintain
4. ❌ Không track changes
5. ❌ Không rollback được
6. ❌ Phải viết lại cho mỗi database engine

---

## 🛠️ HƯỚNG DẪN SỬ DỤNG EF CORE MIGRATIONS

### **1. Cài Đặt Tools**

```bash
# Cài EF Core Tools (global)
dotnet tool install --global dotnet-ef

# Hoặc update nếu đã có
dotnet tool update --global dotnet-ef

# Verify
dotnet ef --version
```

### **2. Kiểm Tra DbContext**

File: `src/GarageManagementSystem.Infrastructure/Data/GarageDbContext.cs` đã có đầy đủ DbSets chưa?

<function_calls>
<invoke name="read_file">
<parameter name="target_file">src/GarageManagementSystem.Infrastructure/Data/GarageDbContext.cs
