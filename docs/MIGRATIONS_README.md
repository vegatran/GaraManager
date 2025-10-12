# 📊 Database Migrations Guide

## Overview

This directory contains all database migration scripts for the Garage Management System. Migrations should be run in order to ensure database schema is up-to-date.

---

## 🗂️ Migration Files

### 1. **Create_Database_Migration.sql** (Base Schema)
**Status:** ✅ Already applied (assumed)  
**Purpose:** Initial database schema with core tables  
**Run:** Only once when setting up new database

**Contains:**
- Core tables: Customer, Vehicle, Employee, Service, Part, etc.
- Basic relationships and indexes
- Initial structure

### 2. **Migration_Add_Configuration_And_Documents.sql** (Phase 1)
**Status:** 🆕 NEW - Need to run  
**Purpose:** Add SystemConfiguration and InsuranceClaimDocument tables  
**Run:** After base schema is in place

**Adds:**
- `SystemConfiguration` table (for flexible VAT rates)
- `InsuranceClaimDocument` table (for insurance claim files)
- Default system configurations (VAT, Invoice, Payment settings)

**Run Command:**
```bash
mysql -u root -p GarageManagementDB < docs/Migration_Add_Configuration_And_Documents.sql
```

### 3. **Migration_Complete_Database_Schema.sql** (Comprehensive Update)
**Status:** 🆕 NEW - Need to run  
**Purpose:** Complete database schema with ALL missing tables and columns  
**Run:** After Migration #2 or as standalone comprehensive update

**Adds:**
- New tables: Department, Position, ApplicationUser, ApplicationRole
- Missing columns in existing tables (Status, Notes, workflow fields)
- All necessary indexes for performance
- Seed data: Departments, Positions, All configurations

**Features:**
- ✅ Updates existing tables (ADD COLUMN IF NOT EXISTS)
- ✅ Creates missing tables (CREATE TABLE IF NOT EXISTS)
- ✅ Idempotent (safe to run multiple times)
- ✅ Comprehensive seed data

**Run Command:**
```bash
mysql -u root -p GarageManagementDB < docs/Migration_Complete_Database_Schema.sql
```

### 4. **Migration_Insurance_Workflow.sql** (Insurance Module)
**Status:** 🆕 NEW - Recommended for insurance features  
**Purpose:** Complete insurance claim workflow with deduction tracking  
**Run:** After base schema and complete schema updates

**Adds:**
- Insurance deduction tracking (discount, depreciation, deductible)
- `InsuranceClaimDeduction` table (chi tiết khấu trừ từng hạng mục)
- `InsuranceCompany` table (danh sách công ty bảo hiểm)
- Enhanced `InsuranceClaim` with 25+ new columns
- Views for reporting
- Stored procedures for calculations
- 8 default insurance companies

**Run Command:**
```bash
mysql -u root -p GarageManagementDB < docs/Migration_Insurance_Workflow.sql
```

### 5. **Migration_Normalize_Table_Names.sql** (Optional)
**Status:** ⚠️ OPTIONAL - Only if needed  
**Purpose:** Rename tables to match Entity names (plural → singular)  
**Run:** Only if you want to standardize naming

**Example Changes:**
- `Customers` → `Customer`
- `Vehicles` → `Vehicle`
- `ServiceOrders` → `ServiceOrder`

**⚠️ WARNING:** This may require code changes in repositories!

---

## 🚀 Recommended Migration Order

### For New Database (Fresh Install)
```bash
# 1. Create database and base schema
mysql -u root -p < docs/Create_Database_Migration.sql

# 2. Add complete schema updates
mysql -u root -p GarageManagementDB < docs/Migration_Complete_Database_Schema.sql
```

### For Existing Database (Update)
```bash
# 1. Add Configuration system
mysql -u root -p GarageManagementDB < docs/Migration_Add_Configuration_And_Documents.sql

# 2. Add remaining tables and columns
mysql -u root -p GarageManagementDB < docs/Migration_Complete_Database_Schema.sql

# 3. Add Insurance Workflow (RECOMMENDED for insurance features)
mysql -u root -p GarageManagementDB < docs/Migration_Insurance_Workflow.sql

# 4. (Optional) Normalize table names
# mysql -u root -p GarageManagementDB < docs/Migration_Normalize_Table_Names.sql
```

---

## 📋 Pre-Migration Checklist

- [ ] **Backup database** before running migrations
  ```bash
  mysqldump -u root -p GarageManagementDB > backup_$(date +%Y%m%d_%H%M%S).sql
  ```

- [ ] **Check MySQL version** (should be 8.0+)
  ```bash
  mysql --version
  ```

- [ ] **Verify database connection**
  ```bash
  mysql -u root -p -e "SHOW DATABASES;"
  ```

- [ ] **Check current schema**
  ```bash
  mysql -u root -p GarageManagementDB -e "SHOW TABLES;"
  ```

---

## 🔍 Post-Migration Verification

### 1. Check Tables Created
```sql
USE GarageManagementDB;

SELECT 
    TABLE_NAME AS 'Table',
    TABLE_ROWS AS 'Rows',
    CREATE_TIME AS 'Created'
FROM information_schema.TABLES 
WHERE TABLE_SCHEMA = 'GarageManagementDB'
ORDER BY TABLE_NAME;
```

### 2. Verify SystemConfiguration
```sql
SELECT Category, COUNT(*) AS ConfigCount
FROM SystemConfiguration
WHERE IsActive = 1
GROUP BY Category;
```

### 3. Check Key Tables
```sql
-- Should return counts for all major tables
SELECT 
    'Customer' AS TableName, COUNT(*) AS Count FROM Customer
UNION ALL
SELECT 'Vehicle', COUNT(*) FROM Vehicle
UNION ALL
SELECT 'Employee', COUNT(*) FROM Employee
UNION ALL
SELECT 'ServiceOrder', COUNT(*) FROM ServiceOrder
UNION ALL
SELECT 'Invoice', COUNT(*) FROM Invoice
UNION ALL
SELECT 'Payment', COUNT(*) FROM Payment
UNION ALL
SELECT 'Quotation', COUNT(*) FROM Quotation
UNION ALL
SELECT 'InsuranceClaim', COUNT(*) FROM InsuranceClaim
UNION ALL
SELECT 'SystemConfiguration', COUNT(*) FROM SystemConfiguration
UNION ALL
SELECT 'Department', COUNT(*) FROM Department
UNION ALL
SELECT 'Position', COUNT(*) FROM Position;
```

### 4. Verify Indexes
```sql
SELECT 
    TABLE_NAME,
    INDEX_NAME,
    COLUMN_NAME
FROM information_schema.STATISTICS
WHERE TABLE_SCHEMA = 'GarageManagementDB'
ORDER BY TABLE_NAME, INDEX_NAME;
```

---

## 🎯 What Each Migration Provides

### Migration_Complete_Database_Schema.sql Adds:

#### New Tables
1. **SystemConfiguration** - Flexible system settings
2. **InsuranceClaimDocument** - Insurance document storage
3. **Department** - Company departments
4. **Position** - Employee positions/job titles
5. **ApplicationUser** - User accounts (IdentityServer)
6. **ApplicationRole** - User roles (IdentityServer)

#### Updated Tables (New Columns)
- **Employee**: DepartmentId, PositionId, UserId
- **Invoice**: Status, PaidDate, Notes
- **Payment**: Status, ReferenceNumber, Notes
- **InsuranceClaim**: ApprovalDate, ApprovedBy, SettlementDate, SettlementAmount, InvoiceId, Notes
- **Quotation**: InspectionId, ExpiryDate, Status
- **VehicleInspection**: InspectionNumber, Status
- **ServiceOrder**: QuotationId, InsuranceClaimId, StartDate, CompletedDate, ActualAmount

#### Seed Data
- **SystemConfiguration**: 25+ default configs (VAT, Invoice, Payment, System, Notification, etc.)
- **Department**: 5 default departments (Management, Technical, CS, Warehouse, Accounting)
- **Position**: 8 default positions (Director, Manager, Technician, etc.)

#### Performance Indexes
- Status indexes on all major tables
- Date indexes for reporting
- Foreign key indexes

---

## 🛠️ Troubleshooting

### Issue: "Table already exists"
**Solution:** The script uses `CREATE TABLE IF NOT EXISTS` - it's safe to run.

### Issue: "Column already exists"
**Solution:** The script uses `ADD COLUMN IF NOT EXISTS` - it's safe to run.

### Issue: "Duplicate entry for key"
**Solution:** The script uses `ON DUPLICATE KEY UPDATE` for seed data - it's safe to run.

### Issue: "Access denied"
**Solution:** Ensure MySQL user has proper permissions:
```sql
GRANT ALL PRIVILEGES ON GarageManagementDB.* TO 'your_user'@'localhost';
FLUSH PRIVILEGES;
```

### Issue: "Unknown database"
**Solution:** Create database first:
```sql
CREATE DATABASE IF NOT EXISTS GarageManagementDB 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;
```

---

## 📝 Migration Log Template

Keep track of migrations in your project:

```
Date: 2025-10-11
Migration: Migration_Complete_Database_Schema.sql
Status: ✅ Success
Tables Added: 6
Columns Added: 20+
Seed Records: 40+
Duration: ~5 seconds
Notes: Added SystemConfiguration and updated workflow tables
```

---

## 🔐 Security Notes

1. **Backup First:** Always backup before running migrations
2. **Test Environment:** Test migrations in dev/staging first
3. **Read Scripts:** Review migration scripts before running
4. **Version Control:** Keep migrations in version control
5. **Documentation:** Document any custom changes

---

## 📞 Support

If you encounter issues:

1. Check MySQL error log: `/var/log/mysql/error.log`
2. Verify database permissions
3. Review migration script comments
4. Check [IMPLEMENTATION_SUMMARY.md](./IMPLEMENTATION_SUMMARY.md) for context

---

## 🎓 Best Practices

1. ✅ **Always backup** before migrations
2. ✅ **Run in order** (base → updates → optional)
3. ✅ **Test first** in non-production environment
4. ✅ **Verify results** after each migration
5. ✅ **Document** what was changed
6. ✅ **Keep** old backups for rollback

---

## 📊 Migration Status Tracker

| Migration | Status | Date Run | Notes |
|-----------|--------|----------|-------|
| Create_Database_Migration.sql | ✅ Done | YYYY-MM-DD | Initial schema |
| Migration_Add_Configuration_And_Documents.sql | ⬜ Pending | - | Phase 1 updates |
| Migration_Complete_Database_Schema.sql | ⬜ Pending | - | Complete schema |
| Migration_Normalize_Table_Names.sql | ⚠️ Optional | - | Only if needed |

---

**Last Updated:** 2025-10-11  
**Database Version:** 2.1.0  
**Compatible With:** MySQL 8.0+

