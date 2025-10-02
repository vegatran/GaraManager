# Dynamic Claims Guide - Hướng dẫn sử dụng Dynamic Claims

## Tổng quan

Hệ thống Dynamic Claims cho phép bạn cấu hình claims một cách linh hoạt thông qua database mà không cần hardcode values. Claims sẽ được tự động lấy từ các nguồn dữ liệu khác nhau dựa trên cấu hình.

## Cách hoạt động

### 1. Claims Mặc định của IdentityServer
- **Giữ nguyên**: Tất cả claims mặc định của IdentityServer4 (sub, name, email, role, etc.)
- **Không override**: CustomProfileService chỉ **bổ sung** thêm custom claims

### 2. Custom Claims từ ClaimsManagement
- **Nguồn**: Bảng `Claims` trong database
- **Điều kiện**: Chỉ lấy claims có `IsActive = true`
- **Cache**: 15 phút với sliding expiration 5 phút

### 3. Dynamic Value Resolution
Claims sẽ được resolve theo thứ tự ưu tiên:

1. **CustomValueSource** (nếu có) → Dynamic lookup từ database
2. **DefaultValue** (nếu có) → Static value
3. **Fallback** → Claim name

## Cấu hình Claims trong Database

### Cấu trúc bảng Claims
```sql
CREATE TABLE Claims (
    Id int PRIMARY KEY,
    Name nvarchar(200) NOT NULL,           -- Tên claim (key)
    DisplayName nvarchar(200),             -- Tên hiển thị
    Description nvarchar(500),             -- Mô tả
    IsStandard bit DEFAULT 0,              -- Standard OIDC claim
    IsActive bit DEFAULT 1,                -- Active status
    Category nvarchar(100),                -- Phân loại
    CustomValueSource nvarchar(200),       -- Format: "Table:Column"
    DefaultValue nvarchar(500),            -- Giá trị mặc định
    CreatedAt datetime2 DEFAULT GETDATE(),
    UpdatedAt datetime2
);
```

### Ví dụ cấu hình Claims

#### 1. Dynamic Claims từ Database
```sql
INSERT INTO Claims (Name, DisplayName, CustomValueSource, IsActive, Category)
VALUES 
('user_department', 'Department', 'users:department', 1, 'Profile'),
('user_level', 'Level', 'roles:level', 1, 'Profile'),
('user_location', 'Location', 'users:city', 1, 'Profile');
```

#### 2. Static Claims với Default Value
```sql
INSERT INTO Claims (Name, DisplayName, DefaultValue, IsActive, Category)
VALUES 
('organization', 'Organization', 'Garage Management System', 1, 'System'),
('environment', 'Environment', 'development', 1, 'System');
```

#### 3. System Dynamic Claims
```sql
INSERT INTO Claims (Name, DisplayName, CustomValueSource, IsActive, Category)
VALUES 
('timestamp', 'Current Time', 'system:timestamp', 1, 'System'),
('session_id', 'Session ID', 'system:session_id', 1, 'System'),
('business_hours', 'Business Hours', 'business:business_hours', 1, 'Business');
```

## Format CustomValueSource

### Cú pháp: `Table:Column`

#### 1. Users Table
```
users:department     → SELECT Department FROM Users WHERE Id = {userId}
users:city          → SELECT City FROM Users WHERE Id = {userId}
users:phone         → SELECT PhoneNumber FROM Users WHERE Id = {userId}
```

#### 2. Roles Table
```
roles:level         → SELECT Level FROM Roles WHERE Id IN (SELECT RoleId FROM UserRoles WHERE UserId = {userId})
roles:permissions   → SELECT Permissions FROM Roles WHERE Id IN (...)
```

#### 3. System Values
```
system:timestamp    → DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
system:session_id   → Guid.NewGuid().ToString()
system:version      → "1.0.0"
system:environment  → Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
```

#### 4. Business Logic
```
business:business_hours → (DateTime.Now.Hour >= 9 && DateTime.Now.Hour <= 17).ToString()
business:weekend        → (DateTime.Now.DayOfWeek == Saturday || Sunday).ToString()
business:quarter        → ((DateTime.Now.Month - 1) / 3 + 1).ToString()
```

## Ví dụ sử dụng

### 1. Tạo Claim động từ bảng Users
```sql
INSERT INTO Claims (Name, DisplayName, CustomValueSource, IsActive, Category)
VALUES ('user_department', 'User Department', 'users:department', 1, 'Profile');
```

### 2. Tạo Claim với giá trị mặc định
```sql
INSERT INTO Claims (Name, DisplayName, DefaultValue, IsActive, Category)
VALUES ('app_name', 'Application Name', 'GaraManager', 1, 'System');
```

### 3. Tạo Claim business logic
```sql
INSERT INTO Claims (Name, DisplayName, CustomValueSource, IsActive, Category)
VALUES ('is_weekend', 'Is Weekend', 'business:weekend', 1, 'Business');
```

## Claims được trả về

### Claims mặc định (IdentityServer4)
- `sub` → User ID
- `name` → UserName hoặc Email
- `email` → Email
- `email_verified` → EmailConfirmed
- `given_name` → FirstName
- `family_name` → LastName
- `phone_number` → PhoneNumber
- `phone_number_verified` → PhoneNumberConfirmed
- `role` → User roles

### Custom Claims (từ OptimizedClaimsService)
- **User-specific claims** → Từ database với CustomValueSource
- **Business logic claims** → Tính toán dựa trên thời gian, business rules
- **Role-based claims** → Dựa trên user roles

### Ví dụ output
```json
{
  "sub": "user123",
  "name": "john.doe@example.com",
  "email": "john.doe@example.com",
  "role": "Admin",
  "role": "Manager",
  
  // Custom claims
  "user_department": "IT Department",
  "user_level": "Senior",
  "organization": "Garage Management System",
  "timestamp": "2024-01-15 14:30:25",
  "business_hours": "true",
  "is_weekend": "false",
  "security_level": "high",
  "can_manage_users": "true"
}
```

## Cache Management

### Cache Keys
- `user_claims_{userId}` → User-specific claims (15 phút)

### Cache Invalidation
```csharp
// Invalidate specific user cache
_optimizedClaimsService.InvalidateUserClaimsCache(userId);

// Invalidate all claims cache
_optimizedClaimsService.InvalidateAllClaimsCache();
```

## Best Practices

### 1. Performance
- Sử dụng `CustomValueSource` cho data thay đổi ít
- Sử dụng `DefaultValue` cho static data
- Cache được tự động quản lý

### 2. Security
- Chỉ expose claims cần thiết
- Validate `CustomValueSource` format
- Sanitize output values

### 3. Maintenance
- Document `CustomValueSource` format
- Test claims với different user roles
- Monitor cache performance

## Troubleshooting

### 1. Claim không xuất hiện
- Kiểm tra `IsActive = true`
- Kiểm tra `CustomValueSource` format
- Kiểm tra cache status

### 2. Giá trị claim sai
- Kiểm tra database data
- Kiểm tra `CustomValueSource` lookup
- Kiểm tra `DefaultValue`

### 3. Performance issues
- Kiểm tra cache hit rate
- Optimize database queries
- Consider reducing cache time
