-- =====================================================
-- DROP ALL TABLES FROM GARAGEDBCONTEXT
-- Garage Management System
-- Date: 12/10/2025
-- WARNING: THIS WILL DELETE ALL DATA!
-- =====================================================
-- This script drops all tables in reverse dependency order
-- Based on tables in CREATE_DATABASE_FROM_DBCONTEXT.sql
-- =====================================================

USE GaraManagement;
SET FOREIGN_KEY_CHECKS = 0;

-- =====================================================
-- DROP TABLES IN REVERSE DEPENDENCY ORDER
-- =====================================================

-- Level 4: Most dependent tables
DROP TABLE IF EXISTS `Payments`;
DROP TABLE IF EXISTS `InvoiceItems`;
DROP TABLE IF EXISTS `InsuranceClaimDocuments`;
DROP TABLE IF EXISTS `PartBatchUsages`;
DROP TABLE IF EXISTS `ServiceOrderParts`;
DROP TABLE IF EXISTS `InsuranceInvoiceItems`;
DROP TABLE IF EXISTS `StockTransactions`;
DROP TABLE IF EXISTS `ServiceOrderLabors`;
DROP TABLE IF EXISTS `ServiceOrderItems`;
DROP TABLE IF EXISTS `PaymentTransactions`;

-- Level 3: Third level dependencies
DROP TABLE IF EXISTS `Invoices`;
DROP TABLE IF EXISTS `InsuranceClaims`;
DROP TABLE IF EXISTS `PartInventoryBatches`;
DROP TABLE IF EXISTS `InsuranceInvoices`;
DROP TABLE IF EXISTS `Appointments`;
DROP TABLE IF EXISTS `ServiceOrders`;
DROP TABLE IF EXISTS `QuotationItems`;
DROP TABLE IF EXISTS `InspectionPhotos`;
DROP TABLE IF EXISTS `PartGroupCompatibilities`;

-- Level 2: Second level dependencies
DROP TABLE IF EXISTS `ServiceQuotations`;
DROP TABLE IF EXISTS `InspectionIssues`;
DROP TABLE IF EXISTS `VehicleInspections`;
DROP TABLE IF EXISTS `PurchaseOrderItems`;
DROP TABLE IF EXISTS `FinancialTransactionAttachments`;
DROP TABLE IF EXISTS `EngineSpecifications`;

-- Level 1: First level dependencies
DROP TABLE IF EXISTS `PurchaseOrders`;
DROP TABLE IF EXISTS `FinancialTransactions`;
DROP TABLE IF EXISTS `PartSuppliers`;
DROP TABLE IF EXISTS `VehicleInsurances`;
DROP TABLE IF EXISTS `VehicleModels`;
DROP TABLE IF EXISTS `Services`;
DROP TABLE IF EXISTS `Employees`;
DROP TABLE IF EXISTS `Parts`;
DROP TABLE IF EXISTS `LaborItems`;
DROP TABLE IF EXISTS `Vehicles`;

-- Base tables (no dependencies)
DROP TABLE IF EXISTS `VehicleBrands`;
DROP TABLE IF EXISTS `SystemConfigurations`;
DROP TABLE IF EXISTS `Suppliers`;
DROP TABLE IF EXISTS `ServiceTypes`;
DROP TABLE IF EXISTS `Positions`;
DROP TABLE IF EXISTS `PartGroups`;
DROP TABLE IF EXISTS `LaborCategories`;
DROP TABLE IF EXISTS `Departments`;
DROP TABLE IF EXISTS `Customers`;
DROP TABLE IF EXISTS `AuditLogs`;

-- Migration history
DROP TABLE IF EXISTS `__EFMigrationsHistory`;

SET FOREIGN_KEY_CHECKS = 1;

-- =====================================================
-- VERIFICATION
-- =====================================================

SELECT '╔════════════════════════════════════════╗' as '';
SELECT '║   ALL TABLES DROPPED SUCCESSFULLY!     ║' as '';
SELECT '╚════════════════════════════════════════╝' as '';

SELECT 
    COUNT(*) as RemainingTables
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'GaraManagement' 
AND TABLE_NAME NOT IN ('__EFMigrationsHistory');

SELECT '⚠️  DATABASE IS NOW EMPTY - Ready for CREATE_DATABASE_FROM_DBCONTEXT.sql' as Status;

