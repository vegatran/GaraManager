CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `AuditLogs` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `EntityName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `EntityId` int NULL,
        `Action` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `UserId` varchar(100) CHARACTER SET utf8mb4 NULL,
        `UserName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Timestamp` datetime(6) NOT NULL,
        `IpAddress` varchar(50) CHARACTER SET utf8mb4 NULL,
        `UserAgent` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Details` longtext CHARACTER SET utf8mb4 NULL,
        `Severity` varchar(50) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_AuditLogs` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `Customers` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Phone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `AlternativePhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Email` varchar(200) CHARACTER SET utf8mb4 NULL,
        `Address` varchar(500) CHARACTER SET utf8mb4 NULL,
        `TaxCode` varchar(50) CHARACTER SET utf8mb4 NULL,
        `ContactPersonName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `DateOfBirth` datetime(6) NULL,
        `Gender` varchar(20) CHARACTER SET utf8mb4 NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Customers` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `Departments` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(200) CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Departments` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `LaborCategories` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `CategoryName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `CategoryCode` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `BaseRate` decimal(65,30) NOT NULL,
        `StandardRate` decimal(65,30) NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_LaborCategories` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `PartGroups` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `GroupName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `GroupCode` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Category` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `SubCategory` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `Function` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Unit` varchar(50) CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PartGroups` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `Positions` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(200) CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Positions` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `ServiceTypes` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `TypeName` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `TypeCode` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Category` varchar(50) CHARACTER SET utf8mb4 NULL,
        `EstimatedDuration` int NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ServiceTypes` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `Suppliers` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `SupplierName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `SupplierCode` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ContactPerson` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Phone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `ContactPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Email` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Address` varchar(200) CHARACTER SET utf8mb4 NULL,
        `City` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Country` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Website` varchar(100) CHARACTER SET utf8mb4 NULL,
        `TaxCode` varchar(50) CHARACTER SET utf8mb4 NULL,
        `BankAccount` varchar(100) CHARACTER SET utf8mb4 NULL,
        `BankName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `PaymentTerms` varchar(50) CHARACTER SET utf8mb4 NULL,
        `DeliveryTerms` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `IsOEMSupplier` tinyint(1) NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `LastOrderDate` datetime(6) NULL,
        `TotalOrderValue` decimal(65,30) NULL,
        `Rating` decimal(65,30) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Suppliers` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `SystemConfigurations` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ConfigKey` longtext CHARACTER SET utf8mb4 NOT NULL,
        `ConfigValue` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Description` longtext CHARACTER SET utf8mb4 NULL,
        `DataType` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Category` longtext CHARACTER SET utf8mb4 NOT NULL,
        `IsEditable` tinyint(1) NOT NULL,
        `IsVisible` tinyint(1) NOT NULL,
        `DisplayOrder` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_SystemConfigurations` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `VehicleBrands` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `BrandName` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `BrandCode` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Country` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `LogoUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Website` varchar(200) CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_VehicleBrands` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `Vehicles` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `LicensePlate` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Brand` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Model` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Year` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Color` varchar(50) CHARACTER SET utf8mb4 NULL,
        `VIN` varchar(17) CHARACTER SET utf8mb4 NULL,
        `EngineNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `CustomerId` int NOT NULL,
        `OwnershipType` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `VehicleType` varchar(20) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'Personal',
        `UsageType` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `InsuranceCompany` varchar(100) CHARACTER SET utf8mb4 NULL,
        `PolicyNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `CoverageType` varchar(50) CHARACTER SET utf8mb4 NULL,
        `InsuranceStartDate` datetime(6) NULL,
        `InsuranceEndDate` datetime(6) NULL,
        `InsurancePremium` decimal(65,30) NULL,
        `HasInsurance` tinyint(1) NOT NULL,
        `IsInsuranceActive` tinyint(1) NOT NULL,
        `ClaimNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `AdjusterName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `AdjusterPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `ClaimDate` datetime(6) NULL,
        `ClaimSettlementDate` datetime(6) NULL,
        `ClaimAmount` decimal(65,30) NULL,
        `ClaimStatus` varchar(20) CHARACTER SET utf8mb4 NULL,
        `CompanyName` varchar(200) CHARACTER SET utf8mb4 NULL,
        `TaxCode` varchar(20) CHARACTER SET utf8mb4 NULL,
        `ContactPerson` varchar(100) CHARACTER SET utf8mb4 NULL,
        `ContactPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Department` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CostCenter` varchar(50) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Vehicles` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Vehicles_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `LaborItems` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `LaborCategoryId` int NOT NULL,
        `CategoryId` int NULL,
        `PartGroupId` int NULL,
        `LaborCode` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ItemName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `LaborName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `PartName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `StandardHours` decimal(65,30) NOT NULL,
        `LaborRate` decimal(65,30) NOT NULL,
        `TotalLaborCost` decimal(65,30) NOT NULL,
        `SkillLevel` varchar(100) CHARACTER SET utf8mb4 NULL,
        `RequiredTools` varchar(100) CHARACTER SET utf8mb4 NULL,
        `WorkSteps` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Difficulty` varchar(100) CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_LaborItems` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_LaborItems_LaborCategories_LaborCategoryId` FOREIGN KEY (`LaborCategoryId`) REFERENCES `LaborCategories` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_LaborItems_PartGroups_PartGroupId` FOREIGN KEY (`PartGroupId`) REFERENCES `PartGroups` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `Parts` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `PartNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `PartName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `Category` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Brand` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CostPrice` decimal(18,2) NOT NULL,
        `AverageCostPrice` decimal(65,30) NOT NULL,
        `SellPrice` decimal(18,2) NOT NULL,
        `QuantityInStock` int NOT NULL,
        `MinimumStock` int NOT NULL,
        `ReorderLevel` int NULL,
        `Unit` varchar(20) CHARACTER SET utf8mb4 NULL,
        `CompatibleVehicles` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Location` varchar(100) CHARACTER SET utf8mb4 NULL,
        `SourceType` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `InvoiceType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `HasInvoice` tinyint(1) NOT NULL,
        `CanUseForCompany` tinyint(1) NOT NULL,
        `CanUseForInsurance` tinyint(1) NOT NULL,
        `CanUseForIndividual` tinyint(1) NOT NULL,
        `Condition` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `SourceReference` varchar(100) CHARACTER SET utf8mb4 NULL,
        `PartGroupId` int NULL,
        `OEMNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `AftermarketNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Manufacturer` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Dimensions` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Weight` decimal(65,30) NULL,
        `Material` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Color` varchar(50) CHARACTER SET utf8mb4 NULL,
        `WarrantyMonths` int NOT NULL,
        `WarrantyConditions` varchar(100) CHARACTER SET utf8mb4 NULL,
        `IsOEM` tinyint(1) NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Parts` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Parts_PartGroups_PartGroupId` FOREIGN KEY (`PartGroupId`) REFERENCES `PartGroups` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `Employees` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Phone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Email` varchar(200) CHARACTER SET utf8mb4 NULL,
        `Address` varchar(500) CHARACTER SET utf8mb4 NULL,
        `PositionId` int NULL,
        `DepartmentId` int NULL,
        `Position` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Department` varchar(50) CHARACTER SET utf8mb4 NULL,
        `HireDate` datetime(6) NULL,
        `Salary` decimal(18,2) NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Skills` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Employees` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Employees_Departments_DepartmentId` FOREIGN KEY (`DepartmentId`) REFERENCES `Departments` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_Employees_Positions_PositionId` FOREIGN KEY (`PositionId`) REFERENCES `Positions` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `Services` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `Price` decimal(18,2) NOT NULL,
        `Duration` int NOT NULL,
        `Category` varchar(50) CHARACTER SET utf8mb4 NULL,
        `ServiceTypeId` int NULL,
        `LaborType` varchar(50) CHARACTER SET utf8mb4 NULL,
        `SkillLevel` varchar(100) CHARACTER SET utf8mb4 NULL,
        `LaborHours` int NOT NULL,
        `LaborRate` decimal(65,30) NOT NULL,
        `TotalLaborCost` decimal(65,30) NOT NULL,
        `RequiredTools` varchar(100) CHARACTER SET utf8mb4 NULL,
        `RequiredSkills` varchar(100) CHARACTER SET utf8mb4 NULL,
        `WorkInstructions` varchar(500) CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Services` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Services_ServiceTypes_ServiceTypeId` FOREIGN KEY (`ServiceTypeId`) REFERENCES `ServiceTypes` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `VehicleModels` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `BrandId` int NOT NULL,
        `ModelName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `ModelCode` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Generation` varchar(20) CHARACTER SET utf8mb4 NULL,
        `StartYear` varchar(10) CHARACTER SET utf8mb4 NULL,
        `EndYear` varchar(10) CHARACTER SET utf8mb4 NULL,
        `VehicleType` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Segment` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_VehicleModels` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_VehicleModels_VehicleBrands_BrandId` FOREIGN KEY (`BrandId`) REFERENCES `VehicleBrands` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `VehicleInsurances` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `VehicleId` int NOT NULL,
        `PolicyNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `InsuranceCompany` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `CoverageType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `StartDate` datetime(6) NOT NULL,
        `EndDate` datetime(6) NOT NULL,
        `PremiumAmount` decimal(65,30) NOT NULL,
        `Currency` varchar(3) CHARACTER SET utf8mb4 NOT NULL,
        `PaymentMethod` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `AgentName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `AgentPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `AgentEmail` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CoverageDetails` varchar(500) CHARACTER SET utf8mb4 NULL,
        `DeductibleAmount` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Exclusions` varchar(500) CHARACTER SET utf8mb4 NULL,
        `EmergencyContact` varchar(100) CHARACTER SET utf8mb4 NULL,
        `EmergencyPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        `IsRenewed` tinyint(1) NOT NULL,
        `RenewalDate` datetime(6) NULL,
        `PreviousPolicyId` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_VehicleInsurances` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_VehicleInsurances_VehicleInsurances_PreviousPolicyId` FOREIGN KEY (`PreviousPolicyId`) REFERENCES `VehicleInsurances` (`Id`),
        CONSTRAINT `FK_VehicleInsurances_Vehicles_VehicleId` FOREIGN KEY (`VehicleId`) REFERENCES `Vehicles` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `PartSuppliers` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `PartId` int NOT NULL,
        `SupplierId` int NOT NULL,
        `SupplierPartNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `CostPrice` decimal(65,30) NOT NULL,
        `Currency` varchar(10) CHARACTER SET utf8mb4 NOT NULL,
        `MinimumOrderQuantity` int NOT NULL,
        `LeadTimeDays` int NOT NULL,
        `Packaging` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `IsPreferred` tinyint(1) NOT NULL,
        `LastOrderDate` datetime(6) NULL,
        `LastCostPrice` decimal(65,30) NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PartSuppliers` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PartSuppliers_Parts_PartId` FOREIGN KEY (`PartId`) REFERENCES `Parts` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PartSuppliers_Suppliers_SupplierId` FOREIGN KEY (`SupplierId`) REFERENCES `Suppliers` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `FinancialTransactions` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `TransactionNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `TransactionType` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Category` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `SubCategory` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Amount` decimal(65,30) NOT NULL,
        `Currency` varchar(3) CHARACTER SET utf8mb4 NOT NULL,
        `TransactionDate` datetime(6) NOT NULL,
        `PaymentMethod` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ReferenceNumber` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `RelatedEntity` varchar(100) CHARACTER SET utf8mb4 NULL,
        `RelatedEntityId` int NULL,
        `EmployeeId` int NULL,
        `ApprovedBy` varchar(100) CHARACTER SET utf8mb4 NULL,
        `ApprovedDate` datetime(6) NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `IsApproved` tinyint(1) NOT NULL,
        `IsReconciled` tinyint(1) NOT NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_FinancialTransactions` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_FinancialTransactions_Employees_EmployeeId` FOREIGN KEY (`EmployeeId`) REFERENCES `Employees` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `PurchaseOrders` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `OrderNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `SupplierId` int NOT NULL,
        `OrderDate` datetime(6) NOT NULL,
        `ExpectedDeliveryDate` datetime(6) NULL,
        `ActualDeliveryDate` datetime(6) NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `SupplierOrderNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `ContactPerson` varchar(100) CHARACTER SET utf8mb4 NULL,
        `ContactPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `ContactEmail` varchar(100) CHARACTER SET utf8mb4 NULL,
        `DeliveryAddress` varchar(500) CHARACTER SET utf8mb4 NULL,
        `PaymentTerms` varchar(50) CHARACTER SET utf8mb4 NULL,
        `DeliveryTerms` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Currency` varchar(50) CHARACTER SET utf8mb4 NULL,
        `SubTotal` decimal(65,30) NOT NULL,
        `TaxAmount` decimal(65,30) NOT NULL,
        `ShippingCost` decimal(65,30) NOT NULL,
        `TotalAmount` decimal(65,30) NOT NULL,
        `EmployeeId` int NULL,
        `ApprovedBy` varchar(100) CHARACTER SET utf8mb4 NULL,
        `ApprovedDate` datetime(6) NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `IsApproved` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PurchaseOrders` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PurchaseOrders_Employees_EmployeeId` FOREIGN KEY (`EmployeeId`) REFERENCES `Employees` (`Id`),
        CONSTRAINT `FK_PurchaseOrders_Suppliers_SupplierId` FOREIGN KEY (`SupplierId`) REFERENCES `Suppliers` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `VehicleInspections` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `InspectionNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `VehicleId` int NOT NULL,
        `CustomerId` int NOT NULL,
        `InspectorId` int NULL,
        `InspectorName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `InspectionDate` datetime(6) NOT NULL,
        `InspectionType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `CurrentMileage` int NULL,
        `Mileage` int NULL,
        `FuelLevel` varchar(20) CHARACTER SET utf8mb4 NULL,
        `GeneralCondition` TEXT CHARACTER SET utf8mb4 NULL,
        `ExteriorCondition` TEXT CHARACTER SET utf8mb4 NULL,
        `InteriorCondition` TEXT CHARACTER SET utf8mb4 NULL,
        `EngineCondition` TEXT CHARACTER SET utf8mb4 NULL,
        `BrakeCondition` TEXT CHARACTER SET utf8mb4 NULL,
        `SuspensionCondition` TEXT CHARACTER SET utf8mb4 NULL,
        `TireCondition` TEXT CHARACTER SET utf8mb4 NULL,
        `ElectricalCondition` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `CoolingCondition` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `ExhaustCondition` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `BatteryCondition` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `LightsCondition` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `Findings` TEXT CHARACTER SET utf8mb4 NULL,
        `CustomerComplaints` TEXT CHARACTER SET utf8mb4 NULL,
        `Recommendations` TEXT CHARACTER SET utf8mb4 NULL,
        `TechnicianNotes` TEXT CHARACTER SET utf8mb4 NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `CompletedDate` datetime(6) NULL,
        `QuotationId` int NULL,
        `CustomerName` varchar(200) CHARACTER SET utf8mb4 NULL,
        `CustomerPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `VehiclePlate` varchar(20) CHARACTER SET utf8mb4 NULL,
        `VehicleMake` varchar(50) CHARACTER SET utf8mb4 NULL,
        `VehicleModel` varchar(50) CHARACTER SET utf8mb4 NULL,
        `VehicleYear` int NULL,
        `Notes` TEXT CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_VehicleInspections` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_VehicleInspections_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_VehicleInspections_Employees_InspectorId` FOREIGN KEY (`InspectorId`) REFERENCES `Employees` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_VehicleInspections_Vehicles_VehicleId` FOREIGN KEY (`VehicleId`) REFERENCES `Vehicles` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `EngineSpecifications` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ModelId` int NOT NULL,
        `EngineCode` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `EngineName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Displacement` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `FuelType` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Aspiration` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `CylinderLayout` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `CylinderCount` int NOT NULL,
        `StartYear` varchar(20) CHARACTER SET utf8mb4 NULL,
        `EndYear` varchar(20) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_EngineSpecifications` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_EngineSpecifications_VehicleModels_ModelId` FOREIGN KEY (`ModelId`) REFERENCES `VehicleModels` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `FinancialTransactionAttachments` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `FinancialTransactionId` int NOT NULL,
        `FileName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `FilePath` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `FileType` varchar(100) CHARACTER SET utf8mb4 NULL,
        `FileSize` bigint NOT NULL,
        `MimeType` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `UploadedAt` datetime(6) NOT NULL,
        `UploadedBy` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_FinancialTransactionAttachments` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_FinancialTransactionAttachments_FinancialTransactions_Financ~` FOREIGN KEY (`FinancialTransactionId`) REFERENCES `FinancialTransactions` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `PurchaseOrderItems` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `PurchaseOrderId` int NOT NULL,
        `PartId` int NOT NULL,
        `PartName` varchar(200) CHARACTER SET utf8mb4 NULL,
        `QuantityOrdered` int NOT NULL,
        `QuantityReceived` int NOT NULL,
        `UnitPrice` decimal(65,30) NOT NULL,
        `TotalPrice` decimal(65,30) NOT NULL,
        `SupplierPartNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `PartDescription` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Unit` varchar(50) CHARACTER SET utf8mb4 NULL,
        `ExpectedDeliveryDate` datetime(6) NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `IsReceived` tinyint(1) NOT NULL,
        `ReceivedDate` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PurchaseOrderItems` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PurchaseOrderItems_Parts_PartId` FOREIGN KEY (`PartId`) REFERENCES `Parts` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId` FOREIGN KEY (`PurchaseOrderId`) REFERENCES `PurchaseOrders` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `InspectionIssues` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `VehicleInspectionId` int NOT NULL,
        `Category` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `IssueName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(2000) CHARACTER SET utf8mb4 NOT NULL,
        `Severity` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `RequiresImmediateAction` tinyint(1) NOT NULL,
        `EstimatedCost` decimal(18,2) NULL,
        `TechnicianNotes` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `SuggestedServiceId` int NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_InspectionIssues` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_InspectionIssues_Services_SuggestedServiceId` FOREIGN KEY (`SuggestedServiceId`) REFERENCES `Services` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_InspectionIssues_VehicleInspections_VehicleInspectionId` FOREIGN KEY (`VehicleInspectionId`) REFERENCES `VehicleInspections` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `ServiceQuotations` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `QuotationNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `VehicleInspectionId` int NULL,
        `InspectionId` int NULL,
        `CustomerId` int NOT NULL,
        `VehicleId` int NOT NULL,
        `PreparedById` int NULL,
        `QuotationDate` datetime(6) NOT NULL,
        `ValidUntil` datetime(6) NULL,
        `ExpiryDate` datetime(6) NULL,
        `Description` TEXT CHARACTER SET utf8mb4 NULL,
        `Terms` TEXT CHARACTER SET utf8mb4 NULL,
        `QuotationType` varchar(20) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'Personal',
        `SubTotal` decimal(18,2) NOT NULL,
        `TaxAmount` decimal(18,2) NOT NULL,
        `VATAmount` decimal(65,30) NOT NULL,
        `TaxRate` decimal(5,2) NOT NULL,
        `DiscountAmount` decimal(18,2) NOT NULL,
        `DiscountPercent` decimal(65,30) NOT NULL,
        `TotalAmount` decimal(18,2) NOT NULL,
        `MaxInsuranceAmount` decimal(18,2) NULL,
        `Deductible` decimal(18,2) NULL,
        `InsuranceApprovalDate` datetime(6) NULL,
        `InsuranceApprovedAmount` decimal(18,2) NULL,
        `InsuranceApprovalNotes` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `InsuranceAdjusterContact` varchar(200) CHARACTER SET utf8mb4 NULL,
        `PONumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `PaymentTerms` varchar(20) CHARACTER SET utf8mb4 NULL DEFAULT 'Cash',
        `IsTaxExempt` tinyint(1) NOT NULL,
        `CompanyApprovalDate` datetime(6) NULL,
        `CompanyApprovedBy` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CompanyApprovalNotes` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `CompanyContactPerson` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `SentDate` datetime(6) NULL,
        `ApprovedDate` datetime(6) NULL,
        `RejectedDate` datetime(6) NULL,
        `CustomerNotes` TEXT CHARACTER SET utf8mb4 NULL,
        `RejectionReason` TEXT CHARACTER SET utf8mb4 NULL,
        `Notes` TEXT CHARACTER SET utf8mb4 NULL,
        `CustomerName` varchar(200) CHARACTER SET utf8mb4 NULL,
        `CustomerPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `CustomerEmail` varchar(100) CHARACTER SET utf8mb4 NULL,
        `VehiclePlate` varchar(20) CHARACTER SET utf8mb4 NULL,
        `VehicleMake` varchar(50) CHARACTER SET utf8mb4 NULL,
        `VehicleModel` varchar(50) CHARACTER SET utf8mb4 NULL,
        `ServiceOrderId` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ServiceQuotations` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ServiceQuotations_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_ServiceQuotations_Employees_PreparedById` FOREIGN KEY (`PreparedById`) REFERENCES `Employees` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_ServiceQuotations_VehicleInspections_VehicleInspectionId` FOREIGN KEY (`VehicleInspectionId`) REFERENCES `VehicleInspections` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_ServiceQuotations_Vehicles_VehicleId` FOREIGN KEY (`VehicleId`) REFERENCES `Vehicles` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `PartGroupCompatibilities` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `PartGroupId` int NOT NULL,
        `BrandId` int NULL,
        `ModelId` int NULL,
        `EngineSpecificationId` int NULL,
        `BodyType` varchar(50) CHARACTER SET utf8mb4 NULL,
        `DriveType` varchar(50) CHARACTER SET utf8mb4 NULL,
        `TransmissionType` varchar(50) CHARACTER SET utf8mb4 NULL,
        `FuelSystem` varchar(50) CHARACTER SET utf8mb4 NULL,
        `StartYear` varchar(10) CHARACTER SET utf8mb4 NULL,
        `EndYear` varchar(10) CHARACTER SET utf8mb4 NULL,
        `SpecialNotes` varchar(100) CHARACTER SET utf8mb4 NULL,
        `IsOEMOnly` tinyint(1) NOT NULL,
        `IsAftermarketAllowed` tinyint(1) NOT NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PartGroupCompatibilities` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PartGroupCompatibilities_EngineSpecifications_EngineSpecific~` FOREIGN KEY (`EngineSpecificationId`) REFERENCES `EngineSpecifications` (`Id`),
        CONSTRAINT `FK_PartGroupCompatibilities_PartGroups_PartGroupId` FOREIGN KEY (`PartGroupId`) REFERENCES `PartGroups` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PartGroupCompatibilities_VehicleBrands_BrandId` FOREIGN KEY (`BrandId`) REFERENCES `VehicleBrands` (`Id`),
        CONSTRAINT `FK_PartGroupCompatibilities_VehicleModels_ModelId` FOREIGN KEY (`ModelId`) REFERENCES `VehicleModels` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `InspectionPhotos` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `VehicleInspectionId` int NOT NULL,
        `InspectionIssueId` int NULL,
        `FilePath` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `FileName` varchar(200) CHARACTER SET utf8mb4 NULL,
        `Category` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `DisplayOrder` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_InspectionPhotos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_InspectionPhotos_InspectionIssues_InspectionIssueId` FOREIGN KEY (`InspectionIssueId`) REFERENCES `InspectionIssues` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_InspectionPhotos_VehicleInspections_VehicleInspectionId` FOREIGN KEY (`VehicleInspectionId`) REFERENCES `VehicleInspections` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `QuotationItems` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ServiceQuotationId` int NOT NULL,
        `QuotationId` int NOT NULL,
        `ServiceId` int NULL,
        `PartId` int NULL,
        `InspectionIssueId` int NULL,
        `ItemType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ItemName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `PartName` varchar(200) CHARACTER SET utf8mb4 NULL,
        `ServiceName` varchar(200) CHARACTER SET utf8mb4 NULL,
        `Description` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `Quantity` int NOT NULL,
        `UnitPrice` decimal(18,2) NOT NULL,
        `SubTotal` decimal(65,30) NOT NULL,
        `DiscountAmount` decimal(65,30) NOT NULL,
        `DiscountPercent` decimal(65,30) NOT NULL,
        `VATRate` decimal(65,30) NOT NULL,
        `VATAmount` decimal(65,30) NOT NULL,
        `TotalPrice` decimal(18,2) NOT NULL,
        `TotalAmount` decimal(65,30) NOT NULL,
        `FinalPrice` decimal(65,30) NOT NULL,
        `IsOptional` tinyint(1) NOT NULL,
        `IsApproved` tinyint(1) NOT NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `DisplayOrder` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_QuotationItems` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_QuotationItems_InspectionIssues_InspectionIssueId` FOREIGN KEY (`InspectionIssueId`) REFERENCES `InspectionIssues` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_QuotationItems_Parts_PartId` FOREIGN KEY (`PartId`) REFERENCES `Parts` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_QuotationItems_ServiceQuotations_ServiceQuotationId` FOREIGN KEY (`ServiceQuotationId`) REFERENCES `ServiceQuotations` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_QuotationItems_Services_ServiceId` FOREIGN KEY (`ServiceId`) REFERENCES `Services` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `ServiceOrders` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `OrderNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `CustomerId` int NOT NULL,
        `VehicleId` int NOT NULL,
        `OrderDate` datetime(6) NOT NULL,
        `ScheduledDate` datetime(6) NULL,
        `CompletedDate` datetime(6) NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Notes` TEXT CHARACTER SET utf8mb4 NULL,
        `SubTotal` decimal(65,30) NOT NULL,
        `VATAmount` decimal(65,30) NOT NULL,
        `TotalAmount` decimal(18,2) NOT NULL,
        `DiscountAmount` decimal(18,2) NOT NULL,
        `FinalAmount` decimal(18,2) NOT NULL,
        `PaymentStatus` varchar(50) CHARACTER SET utf8mb4 NULL,
        `VehicleInspectionId` int NULL,
        `ServiceQuotationId` int NULL,
        `QuotationId` int NULL,
        `InsuranceClaimId` int NULL,
        `Description` TEXT CHARACTER SET utf8mb4 NULL,
        `EstimatedAmount` decimal(65,30) NOT NULL,
        `ActualAmount` decimal(65,30) NOT NULL,
        `StartDate` datetime(6) NULL,
        `PrimaryTechnicianId` int NULL,
        `ServiceTotal` decimal(18,2) NOT NULL,
        `PartsTotal` decimal(18,2) NOT NULL,
        `AmountPaid` decimal(18,2) NOT NULL,
        `AmountRemaining` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ServiceOrders` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ServiceOrders_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_ServiceOrders_Employees_PrimaryTechnicianId` FOREIGN KEY (`PrimaryTechnicianId`) REFERENCES `Employees` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_ServiceOrders_ServiceQuotations_ServiceQuotationId` FOREIGN KEY (`ServiceQuotationId`) REFERENCES `ServiceQuotations` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_ServiceOrders_VehicleInspections_VehicleInspectionId` FOREIGN KEY (`VehicleInspectionId`) REFERENCES `VehicleInspections` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_ServiceOrders_Vehicles_VehicleId` FOREIGN KEY (`VehicleId`) REFERENCES `Vehicles` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `Appointments` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AppointmentNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `CustomerId` int NOT NULL,
        `VehicleId` int NULL,
        `ScheduledDateTime` datetime(6) NOT NULL,
        `EstimatedDuration` int NOT NULL,
        `AppointmentType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ServiceRequested` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CustomerNotes` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `ConfirmedDate` datetime(6) NULL,
        `ArrivalTime` datetime(6) NULL,
        `ActualStartTime` datetime(6) NULL,
        `ActualEndTime` datetime(6) NULL,
        `AssignedToId` int NULL,
        `ReminderSent` tinyint(1) NOT NULL,
        `ReminderSentDate` datetime(6) NULL,
        `CancellationReason` varchar(500) CHARACTER SET utf8mb4 NULL,
        `VehicleInspectionId` int NULL,
        `ServiceOrderId` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Appointments` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Appointments_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Appointments_Employees_AssignedToId` FOREIGN KEY (`AssignedToId`) REFERENCES `Employees` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_Appointments_ServiceOrders_ServiceOrderId` FOREIGN KEY (`ServiceOrderId`) REFERENCES `ServiceOrders` (`Id`),
        CONSTRAINT `FK_Appointments_VehicleInspections_VehicleInspectionId` FOREIGN KEY (`VehicleInspectionId`) REFERENCES `VehicleInspections` (`Id`),
        CONSTRAINT `FK_Appointments_Vehicles_VehicleId` FOREIGN KEY (`VehicleId`) REFERENCES `Vehicles` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `InsuranceInvoices` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ServiceOrderId` int NOT NULL,
        `InsuranceCompany` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `PolicyNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ClaimNumber` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `AccidentDate` datetime(6) NOT NULL,
        `AccidentLocation` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `LicensePlate` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `VehicleModel` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `TotalApprovedAmount` decimal(65,30) NOT NULL,
        `CustomerResponsibility` decimal(65,30) NOT NULL,
        `InsuranceResponsibility` decimal(65,30) NOT NULL,
        `VatAmount` decimal(65,30) NOT NULL,
        `FinalAmount` decimal(65,30) NOT NULL,
        `Notes` varchar(1000) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_InsuranceInvoices` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_InsuranceInvoices_ServiceOrders_ServiceOrderId` FOREIGN KEY (`ServiceOrderId`) REFERENCES `ServiceOrders` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `PartInventoryBatches` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `PartId` int NOT NULL,
        `BatchNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ReceiveDate` datetime(6) NOT NULL,
        `QuantityReceived` int NOT NULL,
        `QuantityRemaining` int NOT NULL,
        `UnitCost` decimal(65,30) NOT NULL,
        `SourceType` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `Condition` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `HasInvoice` tinyint(1) NOT NULL,
        `InvoiceNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `InvoiceDate` datetime(6) NULL,
        `SupplierName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `SupplierId` int NULL,
        `CanUseForCompany` tinyint(1) NOT NULL,
        `CanUseForInsurance` tinyint(1) NOT NULL,
        `CanUseForIndividual` tinyint(1) NOT NULL,
        `SourceReference` varchar(100) CHARACTER SET utf8mb4 NULL,
        `SourceVehicle` varchar(100) CHARACTER SET utf8mb4 NULL,
        `SourceVehicleId` int NULL,
        `SourceServiceOrderId` int NULL,
        `Location` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Shelf` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Bin` varchar(50) CHARACTER SET utf8mb4 NULL,
        `ExpiryDate` datetime(6) NULL,
        `IsExpired` tinyint(1) NOT NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `ReceivedBy` varchar(100) CHARACTER SET utf8mb4 NULL,
        `EmployeeId` int NULL,
        `IsActive` tinyint(1) NOT NULL,
        `SourceVehicleEntityId` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PartInventoryBatches` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PartInventoryBatches_Employees_EmployeeId` FOREIGN KEY (`EmployeeId`) REFERENCES `Employees` (`Id`),
        CONSTRAINT `FK_PartInventoryBatches_Parts_PartId` FOREIGN KEY (`PartId`) REFERENCES `Parts` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PartInventoryBatches_ServiceOrders_SourceServiceOrderId` FOREIGN KEY (`SourceServiceOrderId`) REFERENCES `ServiceOrders` (`Id`),
        CONSTRAINT `FK_PartInventoryBatches_Suppliers_SupplierId` FOREIGN KEY (`SupplierId`) REFERENCES `Suppliers` (`Id`),
        CONSTRAINT `FK_PartInventoryBatches_Vehicles_SourceVehicleEntityId` FOREIGN KEY (`SourceVehicleEntityId`) REFERENCES `Vehicles` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `PaymentTransactions` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ReceiptNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ServiceOrderId` int NOT NULL,
        `PaymentDate` datetime(6) NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `PaymentMethod` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `TransactionReference` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CardType` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CardLastFourDigits` varchar(4) CHARACTER SET utf8mb4 NULL,
        `ReceivedById` int NULL,
        `Notes` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `IsRefund` tinyint(1) NOT NULL,
        `RefundReason` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PaymentTransactions` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PaymentTransactions_Employees_ReceivedById` FOREIGN KEY (`ReceivedById`) REFERENCES `Employees` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_PaymentTransactions_ServiceOrders_ServiceOrderId` FOREIGN KEY (`ServiceOrderId`) REFERENCES `ServiceOrders` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `ServiceOrderItems` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ServiceOrderId` int NOT NULL,
        `ServiceId` int NOT NULL,
        `ServiceName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Quantity` int NOT NULL,
        `UnitPrice` decimal(18,2) NOT NULL,
        `Discount` decimal(65,30) NOT NULL,
        `FinalPrice` decimal(65,30) NOT NULL,
        `TotalPrice` decimal(18,2) NOT NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ServiceOrderItems` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ServiceOrderItems_ServiceOrders_ServiceOrderId` FOREIGN KEY (`ServiceOrderId`) REFERENCES `ServiceOrders` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ServiceOrderItems_Services_ServiceId` FOREIGN KEY (`ServiceId`) REFERENCES `Services` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `ServiceOrderLabors` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ServiceOrderId` int NOT NULL,
        `LaborItemId` int NOT NULL,
        `EmployeeId` int NULL,
        `ActualHours` decimal(65,30) NOT NULL,
        `LaborRate` decimal(65,30) NOT NULL,
        `TotalLaborCost` decimal(65,30) NOT NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NULL,
        `StartTime` datetime(6) NULL,
        `EndTime` datetime(6) NULL,
        `CompletedTime` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ServiceOrderLabors` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ServiceOrderLabors_Employees_EmployeeId` FOREIGN KEY (`EmployeeId`) REFERENCES `Employees` (`Id`),
        CONSTRAINT `FK_ServiceOrderLabors_LaborItems_LaborItemId` FOREIGN KEY (`LaborItemId`) REFERENCES `LaborItems` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ServiceOrderLabors_ServiceOrders_ServiceOrderId` FOREIGN KEY (`ServiceOrderId`) REFERENCES `ServiceOrders` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `StockTransactions` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `PartId` int NOT NULL,
        `TransactionNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `TransactionType` int NOT NULL,
        `Quantity` int NOT NULL,
        `UnitCost` decimal(65,30) NOT NULL,
        `UnitPrice` decimal(18,2) NOT NULL,
        `TotalCost` decimal(65,30) NOT NULL,
        `TotalAmount` decimal(18,2) NOT NULL,
        `TransactionDate` datetime(6) NOT NULL,
        `SupplierName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `SupplierId` int NULL,
        `InvoiceNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `HasInvoice` tinyint(1) NOT NULL,
        `ReferenceNumber` varchar(100) CHARACTER SET utf8mb4 NULL,
        `SourceType` varchar(30) CHARACTER SET utf8mb4 NULL,
        `SourceReference` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Condition` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `RelatedEntity` varchar(100) CHARACTER SET utf8mb4 NULL,
        `RelatedEntityId` int NULL,
        `ServiceOrderId` int NULL,
        `EmployeeId` int NULL,
        `ProcessedById` int NULL,
        `Location` varchar(100) CHARACTER SET utf8mb4 NULL,
        `StockAfter` int NOT NULL,
        `QuantityBefore` int NOT NULL,
        `QuantityAfter` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_StockTransactions` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_StockTransactions_Employees_EmployeeId` FOREIGN KEY (`EmployeeId`) REFERENCES `Employees` (`Id`),
        CONSTRAINT `FK_StockTransactions_Employees_ProcessedById` FOREIGN KEY (`ProcessedById`) REFERENCES `Employees` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_StockTransactions_Parts_PartId` FOREIGN KEY (`PartId`) REFERENCES `Parts` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_StockTransactions_ServiceOrders_ServiceOrderId` FOREIGN KEY (`ServiceOrderId`) REFERENCES `ServiceOrders` (`Id`),
        CONSTRAINT `FK_StockTransactions_Suppliers_SupplierId` FOREIGN KEY (`SupplierId`) REFERENCES `Suppliers` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `InsuranceInvoiceItems` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `InsuranceInvoiceId` int NOT NULL,
        `ItemName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `ItemType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ApprovedPrice` decimal(65,30) NOT NULL,
        `CustomerPrice` decimal(65,30) NOT NULL,
        `InsurancePrice` decimal(65,30) NOT NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_InsuranceInvoiceItems` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_InsuranceInvoiceItems_InsuranceInvoices_InsuranceInvoiceId` FOREIGN KEY (`InsuranceInvoiceId`) REFERENCES `InsuranceInvoices` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `ServiceOrderParts` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ServiceOrderId` int NOT NULL,
        `PartId` int NOT NULL,
        `ServiceOrderItemId` int NULL,
        `PartName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `Quantity` int NOT NULL,
        `UnitCost` decimal(18,2) NOT NULL,
        `UnitPrice` decimal(18,2) NOT NULL,
        `TotalPrice` decimal(18,2) NOT NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NULL,
        `IsWarranty` tinyint(1) NOT NULL,
        `WarrantyUntil` datetime(6) NULL,
        `ReturnDate` datetime(6) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_ServiceOrderParts` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ServiceOrderParts_Parts_PartId` FOREIGN KEY (`PartId`) REFERENCES `Parts` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_ServiceOrderParts_ServiceOrderItems_ServiceOrderItemId` FOREIGN KEY (`ServiceOrderItemId`) REFERENCES `ServiceOrderItems` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_ServiceOrderParts_ServiceOrders_ServiceOrderId` FOREIGN KEY (`ServiceOrderId`) REFERENCES `ServiceOrders` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `PartBatchUsages` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `PartInventoryBatchId` int NOT NULL,
        `ServiceOrderId` int NOT NULL,
        `ServiceOrderPartId` int NULL,
        `QuantityUsed` int NOT NULL,
        `UnitCost` decimal(65,30) NOT NULL,
        `UnitPrice` decimal(65,30) NOT NULL,
        `TotalCost` decimal(65,30) NOT NULL,
        `TotalPrice` decimal(65,30) NOT NULL,
        `UsageDate` datetime(6) NOT NULL,
        `CustomerName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CustomerId` int NULL,
        `CustomerType` varchar(20) CHARACTER SET utf8mb4 NULL,
        `VehiclePlate` varchar(20) CHARACTER SET utf8mb4 NULL,
        `VehicleId` int NULL,
        `RequiresInvoice` tinyint(1) NOT NULL,
        `OutgoingInvoiceNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `InvoiceDate` datetime(6) NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `EmployeeId` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PartBatchUsages` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_PartBatchUsages_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`),
        CONSTRAINT `FK_PartBatchUsages_Employees_EmployeeId` FOREIGN KEY (`EmployeeId`) REFERENCES `Employees` (`Id`),
        CONSTRAINT `FK_PartBatchUsages_PartInventoryBatches_PartInventoryBatchId` FOREIGN KEY (`PartInventoryBatchId`) REFERENCES `PartInventoryBatches` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PartBatchUsages_ServiceOrderParts_ServiceOrderPartId` FOREIGN KEY (`ServiceOrderPartId`) REFERENCES `ServiceOrderParts` (`Id`),
        CONSTRAINT `FK_PartBatchUsages_ServiceOrders_ServiceOrderId` FOREIGN KEY (`ServiceOrderId`) REFERENCES `ServiceOrders` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_PartBatchUsages_Vehicles_VehicleId` FOREIGN KEY (`VehicleId`) REFERENCES `Vehicles` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `InsuranceClaimDocuments` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `InsuranceClaimId` int NOT NULL,
        `DocumentName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `DocumentType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `FilePath` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `FileName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `FileExtension` varchar(20) CHARACTER SET utf8mb4 NULL,
        `FileSize` bigint NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `UploadDate` datetime(6) NOT NULL,
        `UploadedAt` datetime(6) NULL,
        `UploadedBy` int NULL,
        `UploadedByName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `IsRequired` tinyint(1) NOT NULL,
        `IsVerified` tinyint(1) NOT NULL,
        `VerifiedDate` datetime(6) NULL,
        `VerifiedBy` varchar(100) CHARACTER SET utf8mb4 NULL,
        `VerificationNotes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_InsuranceClaimDocuments` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `InsuranceClaims` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `VehicleInsuranceId` int NOT NULL,
        `ServiceOrderId` int NULL,
        `CustomerId` int NULL,
        `VehicleId` int NULL,
        `InvoiceId` int NULL,
        `ClaimNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ClaimDate` datetime(6) NOT NULL,
        `ClaimStatus` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `IncidentType` varchar(100) CHARACTER SET utf8mb4 NULL,
        `IncidentDescription` varchar(500) CHARACTER SET utf8mb4 NULL,
        `IncidentDate` datetime(6) NULL,
        `AccidentDate` datetime(6) NULL,
        `IncidentLocation` varchar(200) CHARACTER SET utf8mb4 NULL,
        `AccidentLocation` varchar(200) CHARACTER SET utf8mb4 NULL,
        `AccidentDescription` varchar(500) CHARACTER SET utf8mb4 NULL,
        `DamageDescription` varchar(500) CHARACTER SET utf8mb4 NULL,
        `PoliceReportNumber` varchar(100) CHARACTER SET utf8mb4 NULL,
        `AdjusterName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `AdjusterPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `AdjusterEmail` varchar(100) CHARACTER SET utf8mb4 NULL,
        `EstimatedDamage` decimal(65,30) NULL,
        `EstimatedAmount` decimal(65,30) NULL,
        `ApprovedAmount` decimal(65,30) NULL,
        `SettlementAmount` decimal(65,30) NULL,
        `ApprovedBy` varchar(100) CHARACTER SET utf8mb4 NULL,
        `PaidAmount` decimal(65,30) NULL,
        `ApprovalDate` datetime(6) NULL,
        `SettlementDate` datetime(6) NULL,
        `AdjusterNotes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CustomerNotes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `RepairShopName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `RepairShopAddress` varchar(200) CHARACTER SET utf8mb4 NULL,
        `RepairShopPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `RequiresInspection` tinyint(1) NOT NULL,
        `InspectionDate` datetime(6) NULL,
        `InspectorName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `InspectionReport` varchar(500) CHARACTER SET utf8mb4 NULL,
        `IsRepairCompleted` tinyint(1) NOT NULL,
        `RepairCompletionDate` datetime(6) NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `CustomerName` varchar(200) CHARACTER SET utf8mb4 NULL,
        `CustomerPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `CustomerEmail` varchar(100) CHARACTER SET utf8mb4 NULL,
        `VehiclePlate` varchar(20) CHARACTER SET utf8mb4 NULL,
        `VehicleMake` varchar(50) CHARACTER SET utf8mb4 NULL,
        `VehicleModel` varchar(50) CHARACTER SET utf8mb4 NULL,
        `VehicleYear` int NULL,
        `InsuranceCompany` varchar(100) CHARACTER SET utf8mb4 NULL,
        `PolicyNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `PolicyHolderName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `ServiceOrderId1` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_InsuranceClaims` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_InsuranceClaims_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`),
        CONSTRAINT `FK_InsuranceClaims_ServiceOrders_ServiceOrderId1` FOREIGN KEY (`ServiceOrderId1`) REFERENCES `ServiceOrders` (`Id`),
        CONSTRAINT `FK_InsuranceClaims_VehicleInsurances_VehicleInsuranceId` FOREIGN KEY (`VehicleInsuranceId`) REFERENCES `VehicleInsurances` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_InsuranceClaims_Vehicles_VehicleId` FOREIGN KEY (`VehicleId`) REFERENCES `Vehicles` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `Invoices` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `InvoiceNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `InvoiceSymbol` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `InvoiceDate` datetime(6) NOT NULL,
        `DueDate` datetime(6) NULL,
        `InvoiceType` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `SellerName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `SellerTaxCode` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `SellerAddress` varchar(200) CHARACTER SET utf8mb4 NULL,
        `SellerPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `BuyerName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `BuyerTaxCode` varchar(50) CHARACTER SET utf8mb4 NULL,
        `CustomerTaxCode` varchar(50) CHARACTER SET utf8mb4 NULL,
        `BuyerAddress` varchar(200) CHARACTER SET utf8mb4 NULL,
        `CustomerAddress` varchar(200) CHARACTER SET utf8mb4 NULL,
        `BuyerPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `BuyerEmail` varchar(100) CHARACTER SET utf8mb4 NULL,
        `ServiceOrderId` int NULL,
        `ServiceOrderNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `InsuranceClaimId` int NULL,
        `ClaimNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `InsuranceCompany` varchar(100) CHARACTER SET utf8mb4 NULL,
        `VehicleId` int NULL,
        `VehiclePlate` varchar(20) CHARACTER SET utf8mb4 NULL,
        `VehicleInfo` varchar(100) CHARACTER SET utf8mb4 NULL,
        `VehicleMake` varchar(50) CHARACTER SET utf8mb4 NULL,
        `VehicleModel` varchar(50) CHARACTER SET utf8mb4 NULL,
        `VehicleYear` int NULL,
        `CustomerId` int NULL,
        `CustomerName` varchar(200) CHARACTER SET utf8mb4 NULL,
        `CustomerPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `SubTotal` decimal(65,30) NOT NULL,
        `VATRate` decimal(65,30) NOT NULL,
        `VATAmount` decimal(65,30) NOT NULL,
        `TotalAmount` decimal(65,30) NOT NULL,
        `DiscountAmount` decimal(65,30) NOT NULL,
        `FinalAmount` decimal(65,30) NOT NULL,
        `Currency` varchar(3) CHARACTER SET utf8mb4 NOT NULL,
        `AmountInWords` varchar(500) CHARACTER SET utf8mb4 NULL,
        `PaymentMethod` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `BankAccount` varchar(100) CHARACTER SET utf8mb4 NULL,
        `BankName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `PaymentStatus` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `PaidAmount` decimal(65,30) NOT NULL,
        `RemainingAmount` decimal(65,30) NOT NULL,
        `IssuedDate` datetime(6) NULL,
        `PaidDate` datetime(6) NULL,
        `PaymentDate` datetime(6) NULL,
        `CancelledDate` datetime(6) NULL,
        `CancellationReason` varchar(500) CHARACTER SET utf8mb4 NULL,
        `ReplacesInvoiceId` int NULL,
        `AdjustmentForInvoiceId` int NULL,
        `Notes` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `EmployeeId` int NULL,
        `EmployeeName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `IsApproved` tinyint(1) NOT NULL,
        `ApprovedDate` datetime(6) NULL,
        `ApprovedBy` varchar(100) CHARACTER SET utf8mb4 NULL,
        `IsDigitallySigned` tinyint(1) NOT NULL,
        `DigitalSignature` varchar(500) CHARACTER SET utf8mb4 NULL,
        `SignedDate` datetime(6) NULL,
        `InsuranceClaimId1` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Invoices` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Invoices_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`),
        CONSTRAINT `FK_Invoices_Employees_EmployeeId` FOREIGN KEY (`EmployeeId`) REFERENCES `Employees` (`Id`),
        CONSTRAINT `FK_Invoices_InsuranceClaims_InsuranceClaimId1` FOREIGN KEY (`InsuranceClaimId1`) REFERENCES `InsuranceClaims` (`Id`),
        CONSTRAINT `FK_Invoices_Invoices_AdjustmentForInvoiceId` FOREIGN KEY (`AdjustmentForInvoiceId`) REFERENCES `Invoices` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Invoices_Invoices_ReplacesInvoiceId` FOREIGN KEY (`ReplacesInvoiceId`) REFERENCES `Invoices` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Invoices_ServiceOrders_ServiceOrderId` FOREIGN KEY (`ServiceOrderId`) REFERENCES `ServiceOrders` (`Id`),
        CONSTRAINT `FK_Invoices_Vehicles_VehicleId` FOREIGN KEY (`VehicleId`) REFERENCES `Vehicles` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `InvoiceItems` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `InvoiceId` int NOT NULL,
        `LineNumber` int NOT NULL,
        `ItemType` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `ItemName` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `PartName` varchar(500) CHARACTER SET utf8mb4 NULL,
        `ServiceName` varchar(500) CHARACTER SET utf8mb4 NULL,
        `ItemCode` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Description` TEXT CHARACTER SET utf8mb4 NULL,
        `Unit` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Quantity` decimal(65,30) NOT NULL,
        `UnitPrice` decimal(65,30) NOT NULL,
        `TotalPrice` decimal(65,30) NOT NULL,
        `SubTotal` decimal(65,30) NOT NULL,
        `TaxRate` decimal(65,30) NOT NULL,
        `VATRate` decimal(65,30) NOT NULL,
        `TaxAmount` decimal(65,30) NOT NULL,
        `VATAmount` decimal(65,30) NOT NULL,
        `AmountIncludingTax` decimal(65,30) NOT NULL,
        `TotalAmount` decimal(65,30) NOT NULL,
        `PartId` int NULL,
        `ServiceId` int NULL,
        `LaborItemId` int NULL,
        `ServiceOrderItemId` int NULL,
        `ServiceOrderPartId` int NULL,
        `ServiceOrderLaborId` int NULL,
        `InputInvoiceNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `InputInvoiceDate` datetime(6) NULL,
        `HasInputInvoice` tinyint(1) NOT NULL,
        `Category` varchar(50) CHARACTER SET utf8mb4 NULL,
        `SubCategory` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Notes` varchar(500) CHARACTER SET utf8mb4 NULL,
        `DisplayOrder` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_InvoiceItems` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_InvoiceItems_Invoices_InvoiceId` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_InvoiceItems_LaborItems_LaborItemId` FOREIGN KEY (`LaborItemId`) REFERENCES `LaborItems` (`Id`),
        CONSTRAINT `FK_InvoiceItems_Parts_PartId` FOREIGN KEY (`PartId`) REFERENCES `Parts` (`Id`),
        CONSTRAINT `FK_InvoiceItems_ServiceOrderItems_ServiceOrderItemId` FOREIGN KEY (`ServiceOrderItemId`) REFERENCES `ServiceOrderItems` (`Id`),
        CONSTRAINT `FK_InvoiceItems_ServiceOrderLabors_ServiceOrderLaborId` FOREIGN KEY (`ServiceOrderLaborId`) REFERENCES `ServiceOrderLabors` (`Id`),
        CONSTRAINT `FK_InvoiceItems_ServiceOrderParts_ServiceOrderPartId` FOREIGN KEY (`ServiceOrderPartId`) REFERENCES `ServiceOrderParts` (`Id`),
        CONSTRAINT `FK_InvoiceItems_Services_ServiceId` FOREIGN KEY (`ServiceId`) REFERENCES `Services` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE TABLE `Payments` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `PaymentDate` datetime(6) NOT NULL,
        `CustomerId` int NOT NULL,
        `CustomerName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `CustomerPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `InvoiceId` int NULL,
        `InvoiceNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `Amount` decimal(65,30) NOT NULL,
        `PaymentMethod` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ReferenceNumber` varchar(100) CHARACTER SET utf8mb4 NULL,
        `Status` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Notes` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Payments` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Payments_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_Payments_Invoices_InvoiceId` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices` (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    INSERT INTO `Departments` (`Id`, `CreatedAt`, `CreatedBy`, `DeletedAt`, `DeletedBy`, `Description`, `IsActive`, `IsDeleted`, `Name`, `UpdatedAt`, `UpdatedBy`)
    VALUES (1, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Bộ phận dịch vụ sửa chữa và bảo dưỡng xe', TRUE, FALSE, 'Dịch Vụ', NULL, NULL),
    (2, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Bộ phận quản lý phụ tùng và linh kiện', TRUE, FALSE, 'Phụ Tùng', NULL, NULL),
    (3, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Bộ phận hành chính và quản lý', TRUE, FALSE, 'Hành Chính', NULL, NULL),
    (4, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Bộ phận kế toán và tài chính', TRUE, FALSE, 'Kế Toán', NULL, NULL),
    (5, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Bộ phận chăm sóc và hỗ trợ khách hàng', TRUE, FALSE, 'Chăm Sóc Khách Hàng', NULL, NULL),
    (6, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Bộ phận quản lý và điều hành', TRUE, FALSE, 'Quản Lý', NULL, NULL);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    INSERT INTO `Employees` (`Id`, `Address`, `CreatedAt`, `CreatedBy`, `DeletedAt`, `DeletedBy`, `Department`, `DepartmentId`, `Email`, `HireDate`, `IsDeleted`, `Name`, `Phone`, `Position`, `PositionId`, `Salary`, `Skills`, `Status`, `UpdatedAt`, `UpdatedBy`)
    VALUES (1, NULL, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Kỹ thuật', NULL, 'nguyenvana@garage.com', TIMESTAMP '2023-10-12 14:34:13', FALSE, 'Nguyễn Văn A', '0123456789', 'Thợ sửa chữa', NULL, 8000000.0, 'Sửa chữa động cơ, Thay dầu, Kiểm tra phanh', 'Active', NULL, NULL),
    (2, NULL, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Kỹ thuật', NULL, 'tranthib@garage.com', TIMESTAMP '2024-10-12 14:34:13', FALSE, 'Trần Thị B', '0987654321', 'Thợ lốp', NULL, 7000000.0, 'Thay lốp, Cân bằng, Sửa chữa lốp', 'Active', NULL, NULL);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    INSERT INTO `Positions` (`Id`, `CreatedAt`, `CreatedBy`, `DeletedAt`, `DeletedBy`, `Description`, `IsActive`, `IsDeleted`, `Name`, `UpdatedAt`, `UpdatedBy`)
    VALUES (1, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Thực hiện sửa chữa và bảo dưỡng xe', TRUE, FALSE, 'Kỹ Thuật Viên', NULL, NULL),
    (2, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Kỹ thuật viên có kinh nghiệm cao', TRUE, FALSE, 'Kỹ Thuật Viên Cao Cấp', NULL, NULL),
    (3, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Quản lý và tư vấn phụ tùng', TRUE, FALSE, 'Chuyên Viên Phụ Tùng', NULL, NULL),
    (4, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Tư vấn và hỗ trợ khách hàng', TRUE, FALSE, 'Tư Vấn Dịch Vụ', NULL, NULL),
    (5, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Tiếp đón và hỗ trợ khách hàng', TRUE, FALSE, 'Lễ Tân', NULL, NULL),
    (6, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Xử lý công việc kế toán', TRUE, FALSE, 'Kế Toán', NULL, NULL),
    (7, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Quản lý và điều hành', TRUE, FALSE, 'Quản Lý', NULL, NULL),
    (8, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Hỗ trợ công việc quản lý', TRUE, FALSE, 'Trợ Lý', NULL, NULL),
    (9, TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Giám sát hoạt động sửa chữa', TRUE, FALSE, 'Giám Sát', NULL, NULL);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    INSERT INTO `Services` (`Id`, `Category`, `CreatedAt`, `CreatedBy`, `DeletedAt`, `DeletedBy`, `Description`, `Duration`, `IsActive`, `IsDeleted`, `LaborHours`, `LaborRate`, `LaborType`, `Name`, `Price`, `RequiredSkills`, `RequiredTools`, `ServiceTypeId`, `SkillLevel`, `TotalLaborCost`, `UpdatedAt`, `UpdatedBy`, `WorkInstructions`)
    VALUES (1, 'Bảo dưỡng', TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Thay dầu động cơ và lọc dầu', 30, TRUE, FALSE, 1, 0.0, NULL, 'Thay dầu động cơ', 200000.0, NULL, NULL, NULL, NULL, 0.0, NULL, NULL, NULL),
    (2, 'An toàn', TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Kiểm tra và bảo dưỡng hệ thống phanh', 45, TRUE, FALSE, 1, 0.0, NULL, 'Kiểm tra phanh', 150000.0, NULL, NULL, NULL, NULL, 0.0, NULL, NULL, NULL),
    (3, 'Lốp xe', TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Thay lốp xe và cân bằng', 60, TRUE, FALSE, 1, 0.0, NULL, 'Thay lốp', 300000.0, NULL, NULL, NULL, NULL, 0.0, NULL, NULL, NULL),
    (4, 'Sửa chữa', TIMESTAMP '2025-10-12 14:34:13', NULL, NULL, NULL, 'Chẩn đoán và sửa chữa động cơ', 120, TRUE, FALSE, 1, 0.0, NULL, 'Sửa chữa động cơ', 500000.0, NULL, NULL, NULL, NULL, 0.0, NULL, NULL, NULL);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Appointments_AppointmentNumber` ON `Appointments` (`AppointmentNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Appointments_AssignedToId` ON `Appointments` (`AssignedToId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Appointments_CustomerId` ON `Appointments` (`CustomerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Appointments_ServiceOrderId` ON `Appointments` (`ServiceOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Appointments_VehicleId` ON `Appointments` (`VehicleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Appointments_VehicleInspectionId` ON `Appointments` (`VehicleInspectionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Customers_Email` ON `Customers` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Customers_Phone` ON `Customers` (`Phone`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Employees_DepartmentId` ON `Employees` (`DepartmentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Employees_PositionId` ON `Employees` (`PositionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_EngineSpecifications_ModelId` ON `EngineSpecifications` (`ModelId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_FinancialTransactionAttachments_FinancialTransactionId` ON `FinancialTransactionAttachments` (`FinancialTransactionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_FinancialTransactions_EmployeeId` ON `FinancialTransactions` (`EmployeeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InspectionIssues_SuggestedServiceId` ON `InspectionIssues` (`SuggestedServiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InspectionIssues_VehicleInspectionId` ON `InspectionIssues` (`VehicleInspectionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InspectionPhotos_InspectionIssueId` ON `InspectionPhotos` (`InspectionIssueId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InspectionPhotos_VehicleInspectionId` ON `InspectionPhotos` (`VehicleInspectionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InsuranceClaimDocuments_InsuranceClaimId` ON `InsuranceClaimDocuments` (`InsuranceClaimId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InsuranceClaims_CustomerId` ON `InsuranceClaims` (`CustomerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InsuranceClaims_InvoiceId` ON `InsuranceClaims` (`InvoiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InsuranceClaims_ServiceOrderId1` ON `InsuranceClaims` (`ServiceOrderId1`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InsuranceClaims_VehicleId` ON `InsuranceClaims` (`VehicleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InsuranceClaims_VehicleInsuranceId` ON `InsuranceClaims` (`VehicleInsuranceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InsuranceInvoiceItems_InsuranceInvoiceId` ON `InsuranceInvoiceItems` (`InsuranceInvoiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InsuranceInvoices_ServiceOrderId` ON `InsuranceInvoices` (`ServiceOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InvoiceItems_InvoiceId` ON `InvoiceItems` (`InvoiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InvoiceItems_LaborItemId` ON `InvoiceItems` (`LaborItemId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InvoiceItems_PartId` ON `InvoiceItems` (`PartId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InvoiceItems_ServiceId` ON `InvoiceItems` (`ServiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InvoiceItems_ServiceOrderItemId` ON `InvoiceItems` (`ServiceOrderItemId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InvoiceItems_ServiceOrderLaborId` ON `InvoiceItems` (`ServiceOrderLaborId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_InvoiceItems_ServiceOrderPartId` ON `InvoiceItems` (`ServiceOrderPartId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Invoices_AdjustmentForInvoiceId` ON `Invoices` (`AdjustmentForInvoiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Invoices_CustomerId` ON `Invoices` (`CustomerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Invoices_EmployeeId` ON `Invoices` (`EmployeeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Invoices_InsuranceClaimId1` ON `Invoices` (`InsuranceClaimId1`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Invoices_ReplacesInvoiceId` ON `Invoices` (`ReplacesInvoiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Invoices_ServiceOrderId` ON `Invoices` (`ServiceOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Invoices_VehicleId` ON `Invoices` (`VehicleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_LaborItems_LaborCategoryId` ON `LaborItems` (`LaborCategoryId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_LaborItems_PartGroupId` ON `LaborItems` (`PartGroupId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartBatchUsages_CustomerId` ON `PartBatchUsages` (`CustomerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartBatchUsages_EmployeeId` ON `PartBatchUsages` (`EmployeeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartBatchUsages_PartInventoryBatchId` ON `PartBatchUsages` (`PartInventoryBatchId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartBatchUsages_ServiceOrderId` ON `PartBatchUsages` (`ServiceOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartBatchUsages_ServiceOrderPartId` ON `PartBatchUsages` (`ServiceOrderPartId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartBatchUsages_VehicleId` ON `PartBatchUsages` (`VehicleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartGroupCompatibilities_BrandId` ON `PartGroupCompatibilities` (`BrandId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartGroupCompatibilities_EngineSpecificationId` ON `PartGroupCompatibilities` (`EngineSpecificationId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartGroupCompatibilities_ModelId` ON `PartGroupCompatibilities` (`ModelId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartGroupCompatibilities_PartGroupId` ON `PartGroupCompatibilities` (`PartGroupId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartInventoryBatches_EmployeeId` ON `PartInventoryBatches` (`EmployeeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartInventoryBatches_PartId` ON `PartInventoryBatches` (`PartId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartInventoryBatches_SourceServiceOrderId` ON `PartInventoryBatches` (`SourceServiceOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartInventoryBatches_SourceVehicleEntityId` ON `PartInventoryBatches` (`SourceVehicleEntityId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartInventoryBatches_SupplierId` ON `PartInventoryBatches` (`SupplierId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Parts_PartGroupId` ON `Parts` (`PartGroupId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Parts_PartNumber` ON `Parts` (`PartNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartSuppliers_PartId` ON `PartSuppliers` (`PartId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PartSuppliers_SupplierId` ON `PartSuppliers` (`SupplierId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Payments_CustomerId` ON `Payments` (`CustomerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Payments_InvoiceId` ON `Payments` (`InvoiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_PaymentTransactions_ReceiptNumber` ON `PaymentTransactions` (`ReceiptNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PaymentTransactions_ReceivedById` ON `PaymentTransactions` (`ReceivedById`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PaymentTransactions_ServiceOrderId` ON `PaymentTransactions` (`ServiceOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PurchaseOrderItems_PartId` ON `PurchaseOrderItems` (`PartId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PurchaseOrderItems_PurchaseOrderId` ON `PurchaseOrderItems` (`PurchaseOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PurchaseOrders_EmployeeId` ON `PurchaseOrders` (`EmployeeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_PurchaseOrders_SupplierId` ON `PurchaseOrders` (`SupplierId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_QuotationItems_InspectionIssueId` ON `QuotationItems` (`InspectionIssueId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_QuotationItems_PartId` ON `QuotationItems` (`PartId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_QuotationItems_ServiceId` ON `QuotationItems` (`ServiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_QuotationItems_ServiceQuotationId` ON `QuotationItems` (`ServiceQuotationId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceOrderItems_ServiceId` ON `ServiceOrderItems` (`ServiceId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceOrderItems_ServiceOrderId` ON `ServiceOrderItems` (`ServiceOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceOrderLabors_EmployeeId` ON `ServiceOrderLabors` (`EmployeeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceOrderLabors_LaborItemId` ON `ServiceOrderLabors` (`LaborItemId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceOrderLabors_ServiceOrderId` ON `ServiceOrderLabors` (`ServiceOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceOrderParts_PartId` ON `ServiceOrderParts` (`PartId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceOrderParts_ServiceOrderId` ON `ServiceOrderParts` (`ServiceOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceOrderParts_ServiceOrderItemId` ON `ServiceOrderParts` (`ServiceOrderItemId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceOrders_CustomerId` ON `ServiceOrders` (`CustomerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_ServiceOrders_OrderNumber` ON `ServiceOrders` (`OrderNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceOrders_PrimaryTechnicianId` ON `ServiceOrders` (`PrimaryTechnicianId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_ServiceOrders_ServiceQuotationId` ON `ServiceOrders` (`ServiceQuotationId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceOrders_VehicleId` ON `ServiceOrders` (`VehicleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceOrders_VehicleInspectionId` ON `ServiceOrders` (`VehicleInspectionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceQuotations_CustomerId` ON `ServiceQuotations` (`CustomerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceQuotations_PreparedById` ON `ServiceQuotations` (`PreparedById`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_ServiceQuotations_QuotationNumber` ON `ServiceQuotations` (`QuotationNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_ServiceQuotations_VehicleId` ON `ServiceQuotations` (`VehicleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_ServiceQuotations_VehicleInspectionId` ON `ServiceQuotations` (`VehicleInspectionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Services_ServiceTypeId` ON `Services` (`ServiceTypeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_StockTransactions_EmployeeId` ON `StockTransactions` (`EmployeeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_StockTransactions_PartId` ON `StockTransactions` (`PartId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_StockTransactions_ProcessedById` ON `StockTransactions` (`ProcessedById`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_StockTransactions_ServiceOrderId` ON `StockTransactions` (`ServiceOrderId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_StockTransactions_SupplierId` ON `StockTransactions` (`SupplierId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_StockTransactions_TransactionNumber` ON `StockTransactions` (`TransactionNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Suppliers_SupplierCode` ON `Suppliers` (`SupplierCode`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_VehicleInspections_CustomerId` ON `VehicleInspections` (`CustomerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_VehicleInspections_InspectionNumber` ON `VehicleInspections` (`InspectionNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_VehicleInspections_InspectorId` ON `VehicleInspections` (`InspectorId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_VehicleInspections_VehicleId` ON `VehicleInspections` (`VehicleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_VehicleInsurances_PreviousPolicyId` ON `VehicleInsurances` (`PreviousPolicyId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_VehicleInsurances_VehicleId` ON `VehicleInsurances` (`VehicleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_VehicleModels_BrandId` ON `VehicleModels` (`BrandId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE INDEX `IX_Vehicles_CustomerId` ON `Vehicles` (`CustomerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Vehicles_LicensePlate` ON `Vehicles` (`LicensePlate`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Vehicles_VIN` ON `Vehicles` (`VIN`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    ALTER TABLE `InsuranceClaimDocuments` ADD CONSTRAINT `FK_InsuranceClaimDocuments_InsuranceClaims_InsuranceClaimId` FOREIGN KEY (`InsuranceClaimId`) REFERENCES `InsuranceClaims` (`Id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    ALTER TABLE `InsuranceClaims` ADD CONSTRAINT `FK_InsuranceClaims_Invoices_InvoiceId` FOREIGN KEY (`InvoiceId`) REFERENCES `Invoices` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251012073417_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251012073417_InitialCreate', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    ALTER TABLE `Vehicles` ADD `Mileage` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25', `HireDate` = TIMESTAMP '2023-10-14 09:33:25'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25', `HireDate` = TIMESTAMP '2024-10-14 09:33:25'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 09:33:25'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014023327_AddMileageToVehicle') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251014023327_AddMileageToVehicle', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13', `HireDate` = TIMESTAMP '2023-10-14 12:22:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13', `HireDate` = TIMESTAMP '2024-10-14 12:22:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 12:22:13'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014052214_AddPrintTemplateTable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251014052214_AddPrintTemplateTable', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    CREATE TABLE `PrintTemplates` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `TemplateName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `TemplateType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `HeaderHtml` varchar(4000) CHARACTER SET utf8mb4 NULL,
        `FooterHtml` varchar(2000) CHARACTER SET utf8mb4 NULL,
        `CompanyInfo` varchar(2000) CHARACTER SET utf8mb4 NULL,
        `CustomCss` varchar(4000) CHARACTER SET utf8mb4 NULL,
        `IsDefault` tinyint(1) NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `DisplayOrder` int NOT NULL,
        `LogoFileName` varchar(200) CHARACTER SET utf8mb4 NULL,
        `LogoPath` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Notes` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_PrintTemplates` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26', `HireDate` = TIMESTAMP '2023-10-14 13:18:26'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26', `HireDate` = TIMESTAMP '2024-10-14 13:18:26'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 13:18:26'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014061829_AddPrintTemplatesTable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251014061829_AddPrintTemplatesTable', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    ALTER TABLE `QuotationItems` ADD `HasInvoice` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13', `HireDate` = TIMESTAMP '2023-10-14 17:34:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13', `HireDate` = TIMESTAMP '2024-10-14 17:34:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-14 17:34:13'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251014103414_AddHasInvoiceToQuotationItem') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251014103414_AddHasInvoiceToQuotationItem', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    ALTER TABLE `VehicleInspections` ADD `CustomerReceptionId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    ALTER TABLE `ServiceQuotations` ADD `CustomerReceptionId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    ALTER TABLE `ServiceOrders` ADD `CustomerReceptionId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    CREATE TABLE `CustomerReceptions` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ReceptionNumber` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `CustomerId` int NOT NULL,
        `VehicleId` int NOT NULL,
        `ReceptionDate` datetime(6) NOT NULL,
        `CustomerRequest` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `CustomerComplaints` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `ReceptionNotes` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `AssignedTechnicianId` int NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `AssignedDate` datetime(6) NULL,
        `InspectionStartDate` datetime(6) NULL,
        `InspectionCompletedDate` datetime(6) NULL,
        `Priority` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `ServiceType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `IsInsuranceClaim` tinyint(1) NOT NULL,
        `InsuranceCompany` varchar(100) CHARACTER SET utf8mb4 NULL,
        `InsurancePolicyNumber` varchar(50) CHARACTER SET utf8mb4 NULL,
        `EmergencyContact` varchar(20) CHARACTER SET utf8mb4 NULL,
        `EmergencyContactName` varchar(200) CHARACTER SET utf8mb4 NULL,
        `CustomerName` varchar(200) CHARACTER SET utf8mb4 NULL,
        `CustomerPhone` varchar(20) CHARACTER SET utf8mb4 NULL,
        `VehiclePlate` varchar(20) CHARACTER SET utf8mb4 NULL,
        `VehicleMake` varchar(50) CHARACTER SET utf8mb4 NULL,
        `VehicleModel` varchar(50) CHARACTER SET utf8mb4 NULL,
        `VehicleYear` int NULL,
        `CreatedDate` datetime(6) NOT NULL,
        `UpdatedDate` datetime(6) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_CustomerReceptions` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_CustomerReceptions_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_CustomerReceptions_Employees_AssignedTechnicianId` FOREIGN KEY (`AssignedTechnicianId`) REFERENCES `Employees` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_CustomerReceptions_Vehicles_VehicleId` FOREIGN KEY (`VehicleId`) REFERENCES `Vehicles` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02', `HireDate` = TIMESTAMP '2023-10-15 08:28:02'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02', `HireDate` = TIMESTAMP '2024-10-15 08:28:02'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 08:28:02'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    CREATE UNIQUE INDEX `IX_VehicleInspections_CustomerReceptionId` ON `VehicleInspections` (`CustomerReceptionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    CREATE INDEX `IX_ServiceQuotations_CustomerReceptionId` ON `ServiceQuotations` (`CustomerReceptionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    CREATE INDEX `IX_ServiceOrders_CustomerReceptionId` ON `ServiceOrders` (`CustomerReceptionId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    CREATE INDEX `IX_CustomerReceptions_AssignedTechnicianId` ON `CustomerReceptions` (`AssignedTechnicianId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    CREATE INDEX `IX_CustomerReceptions_CustomerId` ON `CustomerReceptions` (`CustomerId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    CREATE UNIQUE INDEX `IX_CustomerReceptions_ReceptionNumber` ON `CustomerReceptions` (`ReceptionNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    CREATE INDEX `IX_CustomerReceptions_VehicleId` ON `CustomerReceptions` (`VehicleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    ALTER TABLE `ServiceOrders` ADD CONSTRAINT `FK_ServiceOrders_CustomerReceptions_CustomerReceptionId` FOREIGN KEY (`CustomerReceptionId`) REFERENCES `CustomerReceptions` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    ALTER TABLE `ServiceQuotations` ADD CONSTRAINT `FK_ServiceQuotations_CustomerReceptions_CustomerReceptionId` FOREIGN KEY (`CustomerReceptionId`) REFERENCES `CustomerReceptions` (`Id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    ALTER TABLE `VehicleInspections` ADD CONSTRAINT `FK_VehicleInspections_CustomerReceptions_CustomerReceptionId` FOREIGN KEY (`CustomerReceptionId`) REFERENCES `CustomerReceptions` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015012805_AddCustomerReceptionAndWorkflowTracking') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251015012805_AddCustomerReceptionAndWorkflowTracking', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    ALTER TABLE `CustomerReceptions` ADD `StatusTemp` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN


                    UPDATE CustomerReceptions 
                    SET StatusTemp = CASE 
                        WHEN Status = 'Pending' OR Status = 'Chờ Kiểm Tra' THEN 0
                        WHEN Status = 'Assigned' OR Status = 'Đã Phân Công' THEN 1
                        WHEN Status = 'InProgress' OR Status = 'Đang Kiểm Tra' THEN 2
                        WHEN Status = 'Completed' OR Status = 'Đã Hoàn Thành' THEN 3
                        WHEN Status = 'Cancelled' OR Status = 'Đã Hủy' THEN 4
                        ELSE 0
                    END
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    ALTER TABLE `CustomerReceptions` DROP COLUMN `Status`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    ALTER TABLE `CustomerReceptions` RENAME COLUMN `StatusTemp` TO `Status`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11', `HireDate` = TIMESTAMP '2023-10-15 17:01:11'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11', `HireDate` = TIMESTAMP '2024-10-15 17:01:11'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 17:01:11'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015100112_ConvertReceptionStatusToEnum') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251015100112_ConvertReceptionStatusToEnum', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    ALTER TABLE `Services` ADD `IsVATApplicable` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    ALTER TABLE `Services` ADD `MaterialCost` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    ALTER TABLE `Services` ADD `PricingModel` varchar(20) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    ALTER TABLE `Services` ADD `PricingNotes` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    ALTER TABLE `Services` ADD `VATRate` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    ALTER TABLE `QuotationItems` ADD `IsVATApplicable` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    ALTER TABLE `QuotationItems` ADD `LaborCost` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    ALTER TABLE `QuotationItems` ADD `MaterialCost` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    ALTER TABLE `QuotationItems` ADD `PricingBreakdown` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    ALTER TABLE `QuotationItems` ADD `PricingModel` varchar(20) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40', `HireDate` = TIMESTAMP '2023-10-15 19:29:40'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40', `HireDate` = TIMESTAMP '2024-10-15 19:29:40'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40', `IsVATApplicable` = TRUE, `MaterialCost` = 0.0, `PricingModel` = 'Combined', `PricingNotes` = NULL, `VATRate` = 10
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40', `IsVATApplicable` = TRUE, `MaterialCost` = 0.0, `PricingModel` = 'Combined', `PricingNotes` = NULL, `VATRate` = 10
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40', `IsVATApplicable` = TRUE, `MaterialCost` = 0.0, `PricingModel` = 'Combined', `PricingNotes` = NULL, `VATRate` = 10
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:29:40', `IsVATApplicable` = TRUE, `MaterialCost` = 0.0, `PricingModel` = 'Combined', `PricingNotes` = NULL, `VATRate` = 10
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015122942_AddPricingModelsToServiceAndQuotationItem') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251015122942_AddPricingModelsToServiceAndQuotationItem', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25', `HireDate` = TIMESTAMP '2023-10-15 19:37:25'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25', `HireDate` = TIMESTAMP '2024-10-15 19:37:25'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-15 19:37:25'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251015123726_AddPricingModelSupport') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251015123726_AddPricingModelSupport', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04', `HireDate` = TIMESTAMP '2023-10-16 07:42:04'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04', `HireDate` = TIMESTAMP '2024-10-16 07:42:04'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-16 07:42:04'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016004206_UpdateServicePricingFields') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251016004206_UpdateServicePricingFields', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    ALTER TABLE `QuotationItems` MODIFY COLUMN `PricingBreakdown` varchar(1500) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    ALTER TABLE `QuotationItems` MODIFY COLUMN `Notes` varchar(1500) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39', `HireDate` = TIMESTAMP '2023-10-16 09:15:39'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39', `HireDate` = TIMESTAMP '2024-10-16 09:15:39'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-16 09:15:39'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016021540_IncreasePricingBreakdownLength') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251016021540_IncreasePricingBreakdownLength', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    ALTER TABLE `QuotationItems` ADD `ItemCategory` varchar(50) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13', `HireDate` = TIMESTAMP '2023-10-16 10:19:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13', `HireDate` = TIMESTAMP '2024-10-16 10:19:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-16 10:19:13'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251016031914_AddItemCategoryToQuotationItem') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251016031914_AddItemCategoryToQuotationItem', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    ALTER TABLE `ServiceQuotations` ADD `InsuranceApprovedDiscountAmount` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    ALTER TABLE `ServiceQuotations` ADD `InsuranceApprovedSubTotal` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    ALTER TABLE `ServiceQuotations` ADD `InsuranceApprovedTaxAmount` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    ALTER TABLE `ServiceQuotations` ADD `InsuranceApprovedTotalAmount` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    ALTER TABLE `QuotationItems` ADD `InsuranceApprovalNotes` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    ALTER TABLE `QuotationItems` ADD `InsuranceApprovedSubTotal` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    ALTER TABLE `QuotationItems` ADD `InsuranceApprovedTotalAmount` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    ALTER TABLE `QuotationItems` ADD `InsuranceApprovedUnitPrice` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    ALTER TABLE `QuotationItems` ADD `InsuranceApprovedVATAmount` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    CREATE TABLE `QuotationAttachments` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ServiceQuotationId` int NOT NULL,
        `FileName` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `FilePath` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `FileType` varchar(100) CHARACTER SET utf8mb4 NULL,
        `FileSize` bigint NOT NULL,
        `AttachmentType` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Notes` varchar(1000) CHARACTER SET utf8mb4 NULL,
        `IsInsuranceDocument` tinyint(1) NOT NULL,
        `UploadedDate` datetime(6) NOT NULL,
        `UploadedById` int NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `CreatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `UpdatedAt` datetime(6) NULL,
        `UpdatedBy` longtext CHARACTER SET utf8mb4 NULL,
        `DeletedAt` datetime(6) NULL,
        `DeletedBy` longtext CHARACTER SET utf8mb4 NULL,
        `IsDeleted` tinyint(1) NOT NULL,
        CONSTRAINT `PK_QuotationAttachments` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_QuotationAttachments_Employees_UploadedById` FOREIGN KEY (`UploadedById`) REFERENCES `Employees` (`Id`),
        CONSTRAINT `FK_QuotationAttachments_ServiceQuotations_ServiceQuotationId` FOREIGN KEY (`ServiceQuotationId`) REFERENCES `ServiceQuotations` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18', `HireDate` = TIMESTAMP '2023-10-19 22:38:18'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18', `HireDate` = TIMESTAMP '2024-10-19 22:38:18'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-19 22:38:18'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    CREATE INDEX `IX_QuotationAttachments_ServiceQuotationId` ON `QuotationAttachments` (`ServiceQuotationId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    CREATE INDEX `IX_QuotationAttachments_UploadedById` ON `QuotationAttachments` (`UploadedById`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251019153822_AddQuotationAttachmentAndInsurancePricing') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251019153822_AddQuotationAttachmentAndInsurancePricing', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    ALTER TABLE `ServiceQuotations` ADD `ContractDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    ALTER TABLE `ServiceQuotations` ADD `ContractFilePath` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    ALTER TABLE `ServiceQuotations` ADD `ContractNumber` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    ALTER TABLE `ServiceQuotations` ADD `CorporateApprovalDate` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    ALTER TABLE `ServiceQuotations` ADD `CorporateApprovalNotes` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    ALTER TABLE `ServiceQuotations` ADD `CorporateApprovedAmount` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    ALTER TABLE `ServiceQuotations` ADD `DiscountRate` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    ALTER TABLE `QuotationItems` ADD `CorporateApprovalNotes` longtext CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    ALTER TABLE `QuotationItems` ADD `CorporateApprovedSubTotal` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    ALTER TABLE `QuotationItems` ADD `CorporateApprovedTotalAmount` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    ALTER TABLE `QuotationItems` ADD `CorporateApprovedUnitPrice` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    ALTER TABLE `QuotationItems` ADD `CorporateApprovedVATAmount` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32', `HireDate` = TIMESTAMP '2023-10-20 14:48:32'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32', `HireDate` = TIMESTAMP '2024-10-20 14:48:32'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-20 14:48:32'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251020074835_AddCorporateFieldsToQuotation') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251020074835_AddCorporateFieldsToQuotation', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    ALTER TABLE `QuotationItems` ADD `OverrideIsVATApplicable` tinyint(1) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    ALTER TABLE `QuotationItems` ADD `OverrideVATRate` decimal(65,30) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    ALTER TABLE `PurchaseOrders` ADD `VATRate` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    ALTER TABLE `Parts` ADD `IsVATApplicable` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    ALTER TABLE `Parts` ADD `VATRate` decimal(65,30) NOT NULL DEFAULT 0.0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Departments` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07', `HireDate` = TIMESTAMP '2023-10-22 17:28:07'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Employees` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07', `HireDate` = TIMESTAMP '2024-10-22 17:28:07'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 5;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 6;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 7;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 8;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Positions` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 9;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 1;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 2;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 3;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    UPDATE `Services` SET `CreatedAt` = TIMESTAMP '2025-10-22 17:28:07'
    WHERE `Id` = 4;
    SELECT ROW_COUNT();


    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251022102808_AddVATFieldsToPartAndQuotationItem') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251022102808_AddVATFieldsToPartAndQuotationItem', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

