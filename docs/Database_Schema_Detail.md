# SCHEMA DATABASE CHI TIẾT - HỆ THỐNG QUẢN LÝ GARAGE Ô TÔ

## 📋 MỤC LỤC
1. [Vehicle Management Module](#vehicle-management-module)
2. [Parts & Inventory Management Module](#parts--inventory-management-module)
3. [Service Management Module](#service-management-module)
4. [Service Order Module](#service-order-module)
5. [Financial Management Module](#financial-management-module)
6. [Supplier & Purchase Management Module](#supplier--purchase-management-module)
7. [Relationships Diagram](#relationships-diagram)

---

## 🚗 VEHICLE MANAGEMENT MODULE

### **1. VehicleBrands Table**
Quản lý thông tin các hãng xe.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID hãng xe | 1 |
| BrandName | VARCHAR(50) | Tên hãng xe | Mercedes-Benz |
| BrandCode | VARCHAR(20) UNIQUE | Mã hãng | MB |
| Country | VARCHAR(100) | Quốc gia | Germany |
| Description | TEXT | Mô tả | Luxury German manufacturer |
| IsActive | BOOLEAN | Còn hoạt động | TRUE |
| CreatedAt | DATETIME | Ngày tạo | 2024-01-01 |
| UpdatedAt | DATETIME | Ngày cập nhật | 2024-01-01 |

**Indexes:**
- UNIQUE: BrandCode
- INDEX: BrandName, IsActive

---

### **2. VehicleModels Table**
Quản lý các model xe của từng hãng.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID model | 1 |
| BrandId | INT (FK) | Hãng xe | 1 (Mercedes-Benz) |
| ModelName | VARCHAR(100) | Tên model | C-Class |
| ModelCode | VARCHAR(20) UNIQUE | Mã model | W205 |
| Generation | VARCHAR(20) | Thế hệ | W205 |
| StartYear | VARCHAR(10) | Năm bắt đầu | 2014 |
| EndYear | VARCHAR(10) | Năm kết thúc | 2021 |
| VehicleType | VARCHAR(50) | Loại xe | Sedan |
| Segment | VARCHAR(50) | Phân khúc | Luxury |
| Description | TEXT | Mô tả | Compact executive car |
| IsActive | BOOLEAN | Còn sản xuất | TRUE |

**Relationships:**
- BrandId → VehicleBrands(Id)

**Indexes:**
- UNIQUE: ModelCode
- INDEX: BrandId, ModelName, IsActive

---

### **3. EngineSpecifications Table**
Thông số động cơ chi tiết cho mỗi model.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID động cơ | 1 |
| ModelId | INT (FK) | Model xe | 1 (C-Class W205) |
| EngineCode | VARCHAR(50) UNIQUE | Mã động cơ | M274 |
| EngineName | VARCHAR(100) | Tên động cơ | 2.0L Turbo |
| Displacement | VARCHAR(20) | Dung tích | 1991cc |
| FuelType | VARCHAR(20) | Loại nhiên liệu | Gasoline |
| Aspiration | VARCHAR(20) | Hệ thống nạp | Turbo |
| CylinderLayout | VARCHAR(20) | Bố trí xi lanh | I4 |
| CylinderCount | INT | Số xi lanh | 4 |
| StartYear | VARCHAR(20) | Năm bắt đầu | 2014 |
| EndYear | VARCHAR(20) | Năm kết thúc | 2021 |

**Relationships:**
- ModelId → VehicleModels(Id)

**Indexes:**
- UNIQUE: EngineCode
- INDEX: ModelId, EngineCode

---

### **4. Vehicles Table**
Thông tin chi tiết từng xe.

#### **Thông tin cơ bản:**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID xe | 1 |
| LicensePlate | VARCHAR(20) UNIQUE | Biển số | 30A-12345 |
| Brand | VARCHAR(100) | Hãng xe | Mercedes-Benz |
| Model | VARCHAR(100) | Model | C-Class W205 |
| Year | VARCHAR(20) | Năm sản xuất | 2020 |
| Color | VARCHAR(50) | Màu sắc | Đen |
| VIN | VARCHAR(17) UNIQUE | Số khung | WDD2050461A123456 |
| EngineNumber | VARCHAR(50) | Số động cơ | M274920123456 |
| CustomerId | INT (FK) | Chủ xe | 1 |

#### **Phân loại xe:**
| Column | Type | Description | Values |
|--------|------|-------------|--------|
| OwnershipType | VARCHAR(20) | Loại sở hữu | Personal, Company, Lease, Rental |
| UsageType | VARCHAR(20) | Mục đích sử dụng | Private, Commercial, Taxi, Delivery, Rental |

#### **Thông tin bảo hiểm:**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| InsuranceCompany | VARCHAR(100) | Công ty BH | Bảo Việt |
| PolicyNumber | VARCHAR(50) | Số hợp đồng | BV123456789 |
| CoverageType | VARCHAR(50) | Loại BH | Comprehensive |
| InsuranceStartDate | DATETIME | Ngày bắt đầu | 2024-01-01 |
| InsuranceEndDate | DATETIME | Ngày kết thúc | 2024-12-31 |
| InsurancePremium | DECIMAL(15,2) | Phí BH | 5000000 |
| HasInsurance | BOOLEAN | Có BH | TRUE |
| IsInsuranceActive | BOOLEAN | BH còn hiệu lực | TRUE |

#### **Thông tin claim:**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| ClaimNumber | VARCHAR(50) | Số claim | CLM-2024-001 |
| AdjusterName | VARCHAR(100) | Điều chỉnh viên | Nguyễn Văn A |
| AdjusterPhone | VARCHAR(20) | SĐT điều chỉnh | 0901234567 |
| ClaimDate | DATETIME | Ngày claim | 2024-01-15 |
| ClaimSettlementDate | DATETIME | Ngày giải quyết | 2024-01-30 |
| ClaimAmount | DECIMAL(15,2) | Số tiền claim | 4500000 |
| ClaimStatus | VARCHAR(20) | Trạng thái | Approved |

#### **Thông tin công ty (nếu xe công ty):**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| CompanyName | VARCHAR(200) | Tên công ty | Công ty TNHH ABC |
| TaxCode | VARCHAR(20) | Mã số thuế | 0123456789 |
| ContactPerson | VARCHAR(100) | Người liên hệ | Trần Văn B |
| ContactPhone | VARCHAR(20) | SĐT liên hệ | 0987654321 |
| Department | VARCHAR(100) | Phòng ban | Vận hành |
| CostCenter | VARCHAR(50) | Trung tâm chi phí | VH-001 |

**Relationships:**
- CustomerId → Customers(Id)

**Indexes:**
- UNIQUE: LicensePlate, VIN
- INDEX: CustomerId, Brand+Model, OwnershipType, UsageType, HasInsurance

---

### **5. VehicleInsurances Table**
Quản lý bảo hiểm xe chi tiết (lịch sử bảo hiểm).

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID bảo hiểm | 1 |
| VehicleId | INT (FK) | Xe | 1 |
| PolicyNumber | VARCHAR(50) UNIQUE | Số hợp đồng | BV2024001 |
| InsuranceCompany | VARCHAR(100) | Công ty BH | Bảo Việt |
| CoverageType | VARCHAR(50) | Loại BH | Comprehensive |
| StartDate | DATETIME | Ngày bắt đầu | 2024-01-01 |
| EndDate | DATETIME | Ngày kết thúc | 2024-12-31 |
| PremiumAmount | DECIMAL(15,2) | Phí BH | 5000000 |
| Currency | VARCHAR(3) | Đơn vị tiền | VND |
| PaymentMethod | VARCHAR(20) | Phương thức TT | Bank Transfer |
| AgentName | VARCHAR(100) | Đại lý BH | Nguyễn Văn C |
| AgentPhone | VARCHAR(20) | SĐT đại lý | 0909123456 |
| AgentEmail | VARCHAR(100) | Email đại lý | agent@baoviet.com |
| CoverageDetails | TEXT | Chi tiết BH | Full coverage... |
| DeductibleAmount | VARCHAR(100) | Số tiền khấu trừ | 500000 |
| Exclusions | TEXT | Loại trừ | War, terrorism... |
| EmergencyContact | VARCHAR(100) | Liên hệ khẩn cấp | 1900-xxxx |
| EmergencyPhone | VARCHAR(20) | SĐT khẩn cấp | 1900123456 |
| IsActive | BOOLEAN | Còn hiệu lực | TRUE |
| IsRenewed | BOOLEAN | Đã gia hạn | FALSE |
| RenewalDate | DATETIME | Ngày gia hạn | NULL |
| PreviousPolicyId | INT (FK) | BH trước đó | NULL |

**Relationships:**
- VehicleId → Vehicles(Id)
- PreviousPolicyId → VehicleInsurances(Id) (self-reference)

**Indexes:**
- UNIQUE: PolicyNumber
- INDEX: VehicleId, InsuranceCompany, IsActive, EndDate

---

### **6. InsuranceClaims Table**
Quản lý claim bảo hiểm.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID claim | 1 |
| VehicleInsuranceId | INT (FK) | Bảo hiểm | 1 |
| ServiceOrderId | INT (FK) | Đơn sửa chữa | 1 |
| ClaimNumber | VARCHAR(50) UNIQUE | Số claim | CLM-2024-001 |
| ClaimDate | DATETIME | Ngày claim | 2024-01-15 |
| ClaimStatus | VARCHAR(20) | Trạng thái | Pending |
| IncidentType | VARCHAR(100) | Loại sự cố | Accident |
| IncidentDescription | TEXT | Mô tả sự cố | Va chạm từ phía sau |
| IncidentDate | DATETIME | Ngày sự cố | 2024-01-14 |
| IncidentLocation | VARCHAR(200) | Địa điểm | 123 Nguyễn Huệ |
| PoliceReportNumber | VARCHAR(100) | Số biên bản CA | PB-2024-001 |
| AdjusterName | VARCHAR(100) | Điều chỉnh viên | Trần Văn D |
| AdjusterPhone | VARCHAR(20) | SĐT | 0908123456 |
| AdjusterEmail | VARCHAR(100) | Email | adjuster@baoviet.com |
| EstimatedDamage | DECIMAL(15,2) | Thiệt hại ước tính | 4500000 |
| ApprovedAmount | DECIMAL(15,2) | Số tiền duyệt | 4200000 |
| PaidAmount | DECIMAL(15,2) | Đã thanh toán | 4200000 |
| ApprovalDate | DATETIME | Ngày duyệt | 2024-01-20 |
| SettlementDate | DATETIME | Ngày giải quyết | 2024-01-30 |
| RequiresInspection | BOOLEAN | Cần kiểm tra | TRUE |
| InspectionDate | DATETIME | Ngày kiểm tra | 2024-01-16 |
| InspectorName | VARCHAR(100) | Người kiểm tra | Lê Văn E |
| InspectionReport | TEXT | Báo cáo | Damage to rear bumper... |
| IsRepairCompleted | BOOLEAN | Đã sửa xong | TRUE |
| RepairCompletionDate | DATETIME | Ngày hoàn thành | 2024-01-28 |

**Relationships:**
- VehicleInsuranceId → VehicleInsurances(Id)
- ServiceOrderId → ServiceOrders(Id)

**Indexes:**
- UNIQUE: ClaimNumber
- INDEX: VehicleInsuranceId, ServiceOrderId, ClaimStatus, ClaimDate

---

### **7. InsuranceClaimDocuments Table**
Tài liệu đính kèm claim.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID tài liệu | 1 |
| InsuranceClaimId | INT (FK) | Claim | 1 |
| DocumentName | VARCHAR(200) | Tên tài liệu | Ảnh hiện trường 1 |
| DocumentType | VARCHAR(50) | Loại tài liệu | Photo |
| FilePath | VARCHAR(500) | Đường dẫn | /uploads/claims/... |
| FileName | VARCHAR(100) | Tên file | IMG_001.jpg |
| FileExtension | VARCHAR(20) | Phần mở rộng | .jpg |
| FileSize | BIGINT | Kích thước (bytes) | 2048576 |
| Description | TEXT | Mô tả | Front view of damage |
| UploadDate | DATETIME | Ngày upload | 2024-01-15 |
| UploadedBy | INT | Người upload | 1 |
| UploadedByName | VARCHAR(100) | Tên người upload | Nguyễn Văn A |
| IsRequired | BOOLEAN | Bắt buộc | TRUE |
| IsVerified | BOOLEAN | Đã xác minh | TRUE |
| VerifiedDate | DATETIME | Ngày xác minh | 2024-01-16 |
| VerifiedBy | VARCHAR(100) | Người xác minh | Trần Văn D |
| VerificationNotes | TEXT | Ghi chú xác minh | Document verified OK |

**Relationships:**
- InsuranceClaimId → InsuranceClaims(Id)

**Indexes:**
- INDEX: InsuranceClaimId, DocumentType, IsVerified

---

## 📦 PARTS & INVENTORY MANAGEMENT MODULE

### **8. PartGroups Table**
Nhóm phụ tùng để quản lý tương thích.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID nhóm | 1 |
| GroupName | VARCHAR(100) | Tên nhóm | Bộ lọc gió động cơ |
| GroupCode | VARCHAR(50) UNIQUE | Mã nhóm | AIR_FILTER_ENGINE |
| Category | VARCHAR(100) | Danh mục | Engine System |
| SubCategory | VARCHAR(100) | Danh mục phụ | Air Intake |
| Description | TEXT | Mô tả | Filter air into engine |
| Function | VARCHAR(100) | Chức năng | Lọc không khí |
| Unit | VARCHAR(50) | Đơn vị | Cái |
| IsActive | BOOLEAN | Hoạt động | TRUE |

**Indexes:**
- UNIQUE: GroupCode
- INDEX: GroupName, Category, IsActive

---

### **9. Parts Table**
Quản lý phụ tùng chi tiết.

#### **Thông tin cơ bản:**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID phụ tùng | 1 |
| PartNumber | VARCHAR(50) UNIQUE | Mã phụ tùng | HEADLIGHT001 |
| PartName | VARCHAR(200) | Tên phụ tùng | Đèn pha MB C-Class |
| Description | TEXT | Mô tả | LED headlight for... |
| Category | VARCHAR(100) | Danh mục | Electrical System |
| Brand | VARCHAR(100) | Hãng | Mercedes-Benz |
| CostPrice | DECIMAL(15,2) | Giá nhập | 2500000 |
| AverageCostPrice | DECIMAL(15,2) | Giá nhập TB | 2500000 |
| SellPrice | DECIMAL(15,2) | Giá bán | 3200000 |
| QuantityInStock | INT | Tồn kho | 5 |
| MinimumStock | INT | Tồn tối thiểu | 1 |
| ReorderLevel | INT | Mức đặt lại | 2 |
| Unit | VARCHAR(20) | Đơn vị | Cái |
| Location | VARCHAR(100) | Vị trí kho | Kho D-02 |

#### **⭐ Phân loại nguồn gốc và hóa đơn (MỚI):**
| Column | Type | Description | Values |
|--------|------|-------------|--------|
| SourceType | VARCHAR(30) | Nguồn gốc | Purchased, Used, Refurbished, Salvage |
| InvoiceType | VARCHAR(50) | Loại hóa đơn | WithInvoice, WithoutInvoice, Internal |
| HasInvoice | BOOLEAN | Có hóa đơn | TRUE/FALSE |
| CanUseForCompany | BOOLEAN | Dùng cho công ty | TRUE/FALSE |
| CanUseForInsurance | BOOLEAN | Dùng cho BH | TRUE/FALSE |
| CanUseForIndividual | BOOLEAN | Dùng cho cá nhân | TRUE |
| Condition | VARCHAR(20) | Tình trạng | New, Used, Refurbished, AsIs |
| SourceReference | VARCHAR(100) | Nguồn chi tiết | "Tháo từ xe 30A-12345" |

**💡 QUY TẮC PHÂN LOẠI:**
```
✅ Hàng MỚI CÓ HÓA ĐƠN:
   SourceType = Purchased
   InvoiceType = WithInvoice
   HasInvoice = TRUE
   CanUseForCompany = TRUE
   CanUseForInsurance = TRUE
   CanUseForIndividual = TRUE
   Condition = New

⚠️ Hàng MỚI KHÔNG HÓA ĐƠN:
   SourceType = Purchased
   InvoiceType = WithoutInvoice
   HasInvoice = FALSE
   CanUseForCompany = FALSE
   CanUseForInsurance = FALSE
   CanUseForIndividual = TRUE
   Condition = New

⚠️ Hàng THÁO TỪ XE CŨ:
   SourceType = Used
   InvoiceType = WithoutInvoice
   HasInvoice = FALSE
   CanUseForCompany = FALSE
   CanUseForInsurance = FALSE
   CanUseForIndividual = TRUE
   Condition = Used
   SourceReference = "Tháo từ xe 30A-12345"
```

#### **Thông tin kỹ thuật:**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| PartGroupId | INT (FK) | Nhóm phụ tùng | 9 (Headlight) |
| OEMNumber | VARCHAR(50) | Số PT chính hãng | A2058200240 |
| AftermarketNumber | VARCHAR(50) | Số PT thay thế | OSRAM LED |
| Manufacturer | VARCHAR(100) | Nhà sản xuất | Osram |
| Dimensions | VARCHAR(100) | Kích thước | 150x80x120mm |
| Weight | DECIMAL(10,3) | Trọng lượng (kg) | 2.100 |
| Material | VARCHAR(50) | Chất liệu | Nhựa + LED |
| Color | VARCHAR(50) | Màu sắc | Trong suốt |
| WarrantyMonths | INT | Bảo hành (tháng) | 24 |
| WarrantyConditions | VARCHAR(100) | Điều kiện BH | 2 năm hoặc 50,000km |
| IsOEM | BOOLEAN | PT chính hãng | TRUE |
| IsActive | BOOLEAN | Còn kinh doanh | TRUE |

**Relationships:**
- PartGroupId → PartGroups(Id)

**Indexes:**
- UNIQUE: PartNumber
- INDEX: PartName, Category, Brand, PartGroupId, QuantityInStock, HasInvoice, SourceType, IsActive

---

### **10. PartInventoryBatches Table (⭐ MỚI)**
Quản lý phụ tùng theo LÔ HÀNG để phân biệt hóa đơn.

**💡 LÝ DO CẦN TABLE NÀY:**
- Cùng 1 phụ tùng có thể nhập nhiều lần từ nhiều nguồn khác nhau
- Lô có hóa đơn VAT → Dùng cho công ty/BH
- Lô không hóa đơn → Chỉ dùng cho cá nhân
- Lô tháo xe cũ → Chỉ dùng cho cá nhân
- Theo dõi chính xác giá vốn từng lô (FIFO/LIFO)

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID lô hàng | 1 |
| PartId | INT (FK) | Phụ tùng | 1 |
| BatchNumber | VARCHAR(50) UNIQUE | Mã lô | BATCH-2024-001 |
| ReceiveDate | DATETIME | Ngày nhập | 2024-01-10 |
| QuantityReceived | INT | SL nhập | 5 |
| QuantityRemaining | INT | SL còn lại | 3 |
| UnitCost | DECIMAL(15,2) | Giá nhập/cái | 2500000 |

**Phân loại nguồn gốc:**
| Column | Type | Description | Values |
|--------|------|-------------|--------|
| SourceType | VARCHAR(30) | Nguồn gốc | Purchased, Used, Refurbished, Salvage |
| Condition | VARCHAR(20) | Tình trạng | New, Used, Refurbished, AsIs |

**Thông tin hóa đơn:**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| HasInvoice | BOOLEAN | Có hóa đơn | TRUE |
| InvoiceNumber | VARCHAR(50) | Số hóa đơn | BV20240001 |
| InvoiceDate | DATETIME | Ngày hóa đơn | 2024-01-10 |
| SupplierName | VARCHAR(100) | Nhà cung cấp | MB Vietnam |
| SupplierId | INT (FK) | ID NCC | 1 |

**Phân loại sử dụng:**
| Column | Type | Description | Value |
|--------|------|-------------|-------|
| CanUseForCompany | BOOLEAN | Dùng cho công ty | TRUE |
| CanUseForInsurance | BOOLEAN | Dùng cho BH | TRUE |
| CanUseForIndividual | BOOLEAN | Dùng cho cá nhân | TRUE |

**Nguồn gốc chi tiết:**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| SourceReference | VARCHAR(100) | Chi tiết nguồn | Nhập từ MB Vietnam |
| SourceVehicle | VARCHAR(100) | Biển số xe | 30A-12345 |
| SourceVehicleId | INT (FK) | ID xe | 1 |
| SourceServiceOrderId | INT (FK) | ID đơn hàng | 15 |

**Thông tin lưu trữ:**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Location | VARCHAR(100) | Vị trí | Kho D |
| Shelf | VARCHAR(50) | Kệ | D-02 |
| Bin | VARCHAR(50) | Ngăn | D-02-A |
| ExpiryDate | DATETIME | Hạn sử dụng | NULL |
| IsExpired | BOOLEAN | Hết hạn | FALSE |
| IsActive | BOOLEAN | Còn tồn | TRUE |

**Relationships:**
- PartId → Parts(Id)
- SupplierId → Suppliers(Id)
- SourceVehicleId → Vehicles(Id)
- SourceServiceOrderId → ServiceOrders(Id)

**Indexes:**
- UNIQUE: BatchNumber
- INDEX: PartId, HasInvoice, SourceType, IsActive

---

### **11. PartBatchUsages Table (⭐ MỚI)**
Theo dõi sử dụng phụ tùng từ từng lô.

**💡 LÝ DO CẦN TABLE NÀY:**
- Theo dõi phụ tùng từ lô nào được sử dụng
- Đảm bảo không dùng nhầm hàng không HĐ cho công ty/BH
- Tính giá vốn chính xác theo phương pháp FIFO/LIFO
- Báo cáo theo nguồn gốc phụ tùng

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID sử dụng | 1 |
| PartInventoryBatchId | INT (FK) | Lô hàng | 1 |
| ServiceOrderId | INT (FK) | Đơn hàng | 1 |
| ServiceOrderPartId | INT (FK) | Chi tiết PT | 1 |
| QuantityUsed | INT | SL sử dụng | 2 |
| UnitCost | DECIMAL(15,2) | Giá vốn | 2500000 |
| UnitPrice | DECIMAL(15,2) | Giá bán | 3200000 |
| TotalCost | DECIMAL(15,2) | Tổng giá vốn | 5000000 |
| TotalPrice | DECIMAL(15,2) | Tổng giá bán | 6400000 |
| UsageDate | DATETIME | Ngày sử dụng | 2024-01-16 |

**Thông tin khách hàng:**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| CustomerName | VARCHAR(100) | Tên KH | Nguyễn Minh Tuấn |
| CustomerId | INT (FK) | ID KH | 1 |
| CustomerType | VARCHAR(20) | Loại KH | Individual, Company, Insurance |

**Thông tin xe:**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| VehiclePlate | VARCHAR(20) | Biển số | 30A-12345 |
| VehicleId | INT (FK) | ID xe | 1 |

**Thông tin hóa đơn xuất:**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| RequiresInvoice | BOOLEAN | Cần HĐ | TRUE |
| OutgoingInvoiceNumber | VARCHAR(50) | Số HĐ xuất | HĐ-2024-001 |
| InvoiceDate | DATETIME | Ngày HĐ | 2024-01-16 |

**Relationships:**
- PartInventoryBatchId → PartInventoryBatches(Id)
- ServiceOrderId → ServiceOrders(Id)
- ServiceOrderPartId → ServiceOrderParts(Id)
- CustomerId → Customers(Id)
- VehicleId → Vehicles(Id)

**Indexes:**
- INDEX: PartInventoryBatchId, ServiceOrderId, CustomerId, VehicleId, UsageDate

---

### **12. StockTransactions Table**
Giao dịch kho (nhập/xuất/điều chỉnh).

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID giao dịch | 1 |
| PartId | INT (FK) | Phụ tùng | 1 |
| TransactionNumber | VARCHAR(50) UNIQUE | Mã giao dịch | STK-2024-001 |
| TransactionType | ENUM | Loại GD | In, Out, Transfer, Adjustment |
| Quantity | INT | Số lượng | 5 |
| UnitCost | DECIMAL(15,2) | Giá đơn vị | 2500000 |
| TotalCost | DECIMAL(15,2) | Tổng giá trị | 12500000 |
| TransactionDate | DATETIME | Ngày GD | 2024-01-10 |
| SupplierName | VARCHAR(100) | NCC | MB Vietnam |
| InvoiceNumber | VARCHAR(50) | Số hóa đơn | BV20240001 |
| **HasInvoice** | **BOOLEAN** | **Có HĐ** | **TRUE** |
| ReferenceNumber | VARCHAR(100) | Số tham chiếu | PO-2024-001 |
| **SourceType** | **VARCHAR(30)** | **Nguồn gốc** | **Purchased** |
| **SourceReference** | **VARCHAR(100)** | **Chi tiết nguồn** | **Nhập từ MB** |
| **Condition** | **VARCHAR(20)** | **Tình trạng** | **New** |
| ServiceOrderId | INT (FK) | Đơn hàng | NULL |
| EmployeeId | INT (FK) | Nhân viên | 1 |
| Location | VARCHAR(100) | Vị trí | Kho D-02 |
| StockAfter | INT | Tồn sau GD | 5 |

**Relationships:**
- PartId → Parts(Id)
- ServiceOrderId → ServiceOrders(Id)
- EmployeeId → Employees(Id)

**Indexes:**
- UNIQUE: TransactionNumber
- INDEX: PartId, TransactionType, TransactionDate, ServiceOrderId, HasInvoice

---

## ⚙️ SERVICE MANAGEMENT MODULE

### **13. ServiceTypes Table**
Phân loại dịch vụ.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID loại DV | 1 |
| TypeName | VARCHAR(50) | Tên loại | Thay thế |
| TypeCode | VARCHAR(20) UNIQUE | Mã loại | REPLACE |
| Description | TEXT | Mô tả | Thay thế phụ tùng mới |
| Category | VARCHAR(50) | Danh mục | Mechanical |
| IsActive | BOOLEAN | Hoạt động | TRUE |

**Indexes:**
- UNIQUE: TypeCode
- INDEX: TypeName, Category, IsActive

---

### **14. Services Table**
Danh sách dịch vụ.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID dịch vụ | 1 |
| Name | VARCHAR(200) | Tên DV | Thay đèn pha MB C-Class |
| Description | TEXT | Mô tả | Replace LED headlight |
| Price | DECIMAL(15,2) | Giá dịch vụ | 200000 |
| Duration | INT | Thời gian (phút) | 90 |
| Category | VARCHAR(50) | Danh mục | Electrical |
| ServiceTypeId | INT (FK) | Loại DV | 1 (Thay thế) |
| LaborType | VARCHAR(50) | Loại công | Tháo, Lắp |
| SkillLevel | VARCHAR(100) | Trình độ | Trung bình |
| LaborHours | INT | Số giờ công | 1.5 |
| LaborRate | DECIMAL(15,2) | Đơn giá công | 50000 |
| TotalLaborCost | DECIMAL(15,2) | Tổng tiền công | 75000 |
| RequiredTools | VARCHAR(100) | Dụng cụ | Cờ lê 10, tuốc nơ vít |
| RequiredSkills | VARCHAR(100) | Kỹ năng | Thợ điện |
| WorkInstructions | TEXT | Hướng dẫn | Tháo ốc, tháo dây... |
| IsActive | BOOLEAN | Hoạt động | TRUE |

**Relationships:**
- ServiceTypeId → ServiceTypes(Id)

**Indexes:**
- INDEX: Name, Category, ServiceTypeId, IsActive

---

### **15. LaborCategories Table**
Danh mục công lao động.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID danh mục | 1 |
| CategoryName | VARCHAR(100) | Tên danh mục | Công tháo lắp |
| CategoryCode | VARCHAR(20) UNIQUE | Mã danh mục | REMOVE_INSTALL |
| Description | TEXT | Mô tả | Tháo và lắp đặt PT |
| BaseRate | DECIMAL(15,2) | Đơn giá cơ bản | 50000 |
| IsActive | BOOLEAN | Hoạt động | TRUE |

**Indexes:**
- UNIQUE: CategoryCode
- INDEX: CategoryName, IsActive

---

### **16. LaborItems Table**
Chi tiết công lao động.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID công | 1 |
| LaborCategoryId | INT (FK) | Danh mục | 1 (Tháo lắp) |
| PartGroupId | INT (FK) | Nhóm PT | 9 (Đèn pha) |
| ItemName | VARCHAR(200) | Tên công | Công tháo đèn pha |
| PartName | VARCHAR(100) | Tên PT | Đèn pha |
| Description | TEXT | Mô tả | Remove and install |
| StandardHours | DECIMAL(8,2) | Giờ công chuẩn | 1.5 |
| LaborRate | DECIMAL(15,2) | Đơn giá | 50000 |
| TotalLaborCost | DECIMAL(15,2) | Tổng tiền công | 75000 |
| SkillLevel | VARCHAR(100) | Trình độ | Trung bình |
| RequiredTools | VARCHAR(100) | Dụng cụ | Cờ lê 10 |
| WorkSteps | TEXT | Bước thực hiện | 1. Tháo ốc... |
| Difficulty | VARCHAR(100) | Độ khó | Trung bình |
| IsActive | BOOLEAN | Hoạt động | TRUE |

**Relationships:**
- LaborCategoryId → LaborCategories(Id)
- PartGroupId → PartGroups(Id)

**Indexes:**
- INDEX: LaborCategoryId, PartGroupId, ItemName, IsActive

---

## 🛠️ SERVICE ORDER MODULE

### **17. ServiceOrders Table**
Đơn hàng sửa chữa.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID đơn hàng | 1 |
| OrderNumber | VARCHAR(50) UNIQUE | Số đơn | SO-2024-001 |
| CustomerId | INT (FK) | Khách hàng | 1 |
| VehicleId | INT (FK) | Xe | 1 |
| OrderDate | DATETIME | Ngày tạo | 2024-01-15 10:30 |
| ScheduledDate | DATETIME | Ngày hẹn | 2024-01-16 08:00 |
| CompletedDate | DATETIME | Ngày hoàn thành | 2024-01-16 15:00 |
| Status | VARCHAR(20) | Trạng thái | Completed |
| Notes | TEXT | Ghi chú | Khách yêu cầu... |

**Financial breakdown:**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| TotalAmount | DECIMAL(15,2) | Tổng tiền | 2850000 |
| DiscountAmount | DECIMAL(15,2) | Giảm giá | 0 |
| FinalAmount | DECIMAL(15,2) | Thành tiền | 2850000 |
| ServiceTotal | DECIMAL(15,2) | Tiền DV | 200000 |
| PartsTotal | DECIMAL(15,2) | Tiền PT | 2000000 |
| LaborTotal | DECIMAL(15,2) | Tiền công | 75000 |
| AmountPaid | DECIMAL(15,2) | Đã thanh toán | 2850000 |
| AmountRemaining | DECIMAL(15,2) | Còn nợ | 0 |
| PaymentStatus | VARCHAR(50) | TT thanh toán | Paid |

**References:**
| Column | Type | Description | Example |
|--------|------|-------------|---------|
| VehicleInspectionId | INT (FK) | Kiểm tra xe | NULL |
| ServiceQuotationId | INT (FK) | Báo giá | NULL |
| PrimaryTechnicianId | INT (FK) | Thợ chính | 1 |

**Relationships:**
- CustomerId → Customers(Id)
- VehicleId → Vehicles(Id)
- PrimaryTechnicianId → Employees(Id)

**Indexes:**
- UNIQUE: OrderNumber
- INDEX: CustomerId, VehicleId, Status, PaymentStatus, OrderDate

---

### **18. ServiceOrderItems Table**
Chi tiết dịch vụ trong đơn hàng.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID chi tiết | 1 |
| ServiceOrderId | INT (FK) | Đơn hàng | 1 |
| ServiceId | INT (FK) | Dịch vụ | 1 |
| Quantity | INT | Số lượng | 1 |
| UnitPrice | DECIMAL(15,2) | Đơn giá | 200000 |
| TotalPrice | DECIMAL(15,2) | Thành tiền | 200000 |
| Notes | TEXT | Ghi chú | Replace LED headlight |

**Relationships:**
- ServiceOrderId → ServiceOrders(Id)
- ServiceId → Services(Id)

**Indexes:**
- INDEX: ServiceOrderId, ServiceId

---

### **19. ServiceOrderParts Table**
Chi tiết phụ tùng sử dụng.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID chi tiết | 1 |
| ServiceOrderId | INT (FK) | Đơn hàng | 1 |
| PartId | INT (FK) | Phụ tùng | 8 |
| ServiceOrderItemId | INT (FK) | Chi tiết DV | 1 |
| Quantity | INT | Số lượng | 1 |
| UnitCost | DECIMAL(15,2) | Giá vốn | 2500000 |
| UnitPrice | DECIMAL(15,2) | Giá bán | 2000000 |
| TotalPrice | DECIMAL(15,2) | Thành tiền | 2000000 |
| Notes | TEXT | Ghi chú | LED headlight OEM |
| IsWarranty | BOOLEAN | Bảo hành | TRUE |
| WarrantyUntil | DATETIME | BH đến | 2025-01-15 |

**Relationships:**
- ServiceOrderId → ServiceOrders(Id)
- PartId → Parts(Id)
- ServiceOrderItemId → ServiceOrderItems(Id)

**Indexes:**
- INDEX: ServiceOrderId, PartId, ServiceOrderItemId

---

### **20. ServiceOrderLabors Table**
Chi tiết công lao động.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID công | 1 |
| ServiceOrderId | INT (FK) | Đơn hàng | 1 |
| LaborItemId | INT (FK) | Công việc | 1 |
| EmployeeId | INT (FK) | Thợ | 1 |
| ActualHours | DECIMAL(8,2) | Giờ thực tế | 1.5 |
| LaborRate | DECIMAL(15,2) | Đơn giá | 50000 |
| TotalLaborCost | DECIMAL(15,2) | Tiền công | 75000 |
| Notes | TEXT | Ghi chú | Completed OK |
| Status | VARCHAR(20) | Trạng thái | Completed |
| StartTime | DATETIME | Bắt đầu | 2024-01-16 08:00 |
| EndTime | DATETIME | Kết thúc | 2024-01-16 09:30 |
| CompletedTime | DATETIME | Hoàn thành | 2024-01-16 09:30 |

**Relationships:**
- ServiceOrderId → ServiceOrders(Id)
- LaborItemId → LaborItems(Id)
- EmployeeId → Employees(Id)

**Indexes:**
- INDEX: ServiceOrderId, LaborItemId, EmployeeId, Status

---

## 💰 FINANCIAL MANAGEMENT MODULE

### **21. FinancialTransactions Table**
Giao dịch tài chính.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID GD | 1 |
| TransactionNumber | VARCHAR(50) UNIQUE | Mã GD | FIN-2024-001 |
| TransactionType | ENUM | Loại GD | Income, Expense, Transfer |
| Category | VARCHAR(50) | Danh mục | Service Revenue |
| SubCategory | VARCHAR(50) | Danh mục phụ | Auto Repair |
| Amount | DECIMAL(15,2) | Số tiền | 2850000 |
| Currency | VARCHAR(3) | Tiền tệ | VND |
| TransactionDate | DATETIME | Ngày GD | 2024-01-16 15:00 |
| PaymentMethod | VARCHAR(50) | Phương thức | Cash |
| ReferenceNumber | VARCHAR(100) | Tham chiếu | SO-2024-001 |
| Description | TEXT | Mô tả | Thu từ đơn hàng... |
| RelatedEntity | VARCHAR(100) | Thực thể | ServiceOrder |
| RelatedEntityId | INT | ID thực thể | 1 |
| EmployeeId | INT (FK) | Nhân viên | 1 |
| ApprovedBy | VARCHAR(100) | Người duyệt | Manager A |
| ApprovedDate | DATETIME | Ngày duyệt | 2024-01-16 16:00 |
| Notes | TEXT | Ghi chú | Paid in full |
| IsApproved | BOOLEAN | Đã duyệt | TRUE |
| IsReconciled | BOOLEAN | Đã đối chiếu | FALSE |

**Relationships:**
- EmployeeId → Employees(Id)

**Indexes:**
- UNIQUE: TransactionNumber
- INDEX: TransactionType, Category, TransactionDate, IsApproved

---

### **22. PaymentTransactions Table**
Thanh toán từ khách hàng.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID thanh toán | 1 |
| ServiceOrderId | INT (FK) | Đơn hàng | 1 |
| TransactionNumber | VARCHAR(50) UNIQUE | Mã GD | PAY-2024-001 |
| PaymentDate | DATETIME | Ngày TT | 2024-01-16 15:00 |
| PaymentMethod | VARCHAR(50) | Phương thức | Cash |
| Amount | DECIMAL(15,2) | Số tiền | 2850000 |
| Currency | VARCHAR(3) | Tiền tệ | VND |
| ReferenceNumber | VARCHAR(100) | Tham chiếu | SO-2024-001 |
| Description | TEXT | Mô tả | Payment for repairs |
| EmployeeId | INT (FK) | Thu ngân | 1 |
| Notes | TEXT | Ghi chú | Paid in cash |

**Relationships:**
- ServiceOrderId → ServiceOrders(Id)
- EmployeeId → Employees(Id)

**Indexes:**
- UNIQUE: TransactionNumber
- INDEX: ServiceOrderId, PaymentDate, PaymentMethod

---

## 🏢 SUPPLIER & PURCHASE MANAGEMENT MODULE

### **23. Suppliers Table**
Nhà cung cấp.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID NCC | 1 |
| SupplierName | VARCHAR(100) | Tên NCC | MB Vietnam |
| SupplierCode | VARCHAR(20) UNIQUE | Mã NCC | MB-VN |
| ContactPerson | VARCHAR(100) | Người liên hệ | Nguyễn Văn A |
| Phone | VARCHAR(20) | SĐT | 0281234567 |
| Email | VARCHAR(100) | Email | sales@mbvn.com |
| Address | VARCHAR(200) | Địa chỉ | 123 Nguyễn Huệ, Q1 |
| City | VARCHAR(50) | Thành phố | TP.HCM |
| Country | VARCHAR(50) | Quốc gia | Vietnam |
| Website | VARCHAR(100) | Website | mbvietnam.com.vn |
| TaxCode | VARCHAR(50) | Mã số thuế | 0123456789 |
| BankAccount | VARCHAR(100) | TK ngân hàng | 123456789 |
| BankName | VARCHAR(100) | Ngân hàng | Vietcombank |
| PaymentTerms | VARCHAR(50) | Điều khoản TT | Net 30 |
| DeliveryTerms | VARCHAR(100) | Điều khoản giao | FOB |
| IsOEMSupplier | BOOLEAN | NCC chính hãng | TRUE |
| IsActive | BOOLEAN | Hoạt động | TRUE |
| LastOrderDate | DATETIME | Đơn cuối | 2024-01-10 |
| TotalOrderValue | DECIMAL(15,2) | Tổng giá trị đơn | 15800000 |

**Indexes:**
- UNIQUE: SupplierCode
- INDEX: SupplierName, IsActive, IsOEMSupplier

---

### **24. PurchaseOrders Table**
Đơn đặt hàng từ NCC.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID đơn | 1 |
| OrderNumber | VARCHAR(50) UNIQUE | Số đơn | PO-2024-001 |
| SupplierId | INT (FK) | NCC | 1 |
| OrderDate | DATETIME | Ngày đặt | 2024-01-05 |
| ExpectedDeliveryDate | DATETIME | Dự kiến giao | 2024-01-10 |
| ActualDeliveryDate | DATETIME | Thực tế giao | 2024-01-10 |
| Status | VARCHAR(20) | Trạng thái | Received |
| SupplierOrderNumber | VARCHAR(50) | Số đơn NCC | MB-2024-001 |
| ContactPerson | VARCHAR(100) | Người liên hệ | Sales A |
| ContactPhone | VARCHAR(20) | SĐT | 0281234567 |
| ContactEmail | VARCHAR(100) | Email | sales@mbvn.com |
| DeliveryAddress | TEXT | Địa chỉ giao | 456 Lê Lợi, Q3 |
| PaymentTerms | VARCHAR(50) | Điều khoản TT | Net 30 |
| DeliveryTerms | VARCHAR(100) | Điều khoản giao | FOB |
| Currency | VARCHAR(3) | Tiền tệ | VND |
| SubTotal | DECIMAL(15,2) | Tạm tính | 14000000 |
| TaxAmount | DECIMAL(15,2) | Thuế VAT | 1400000 |
| ShippingCost | DECIMAL(15,2) | Phí vận chuyển | 400000 |
| TotalAmount | DECIMAL(15,2) | Tổng cộng | 15800000 |
| EmployeeId | INT (FK) | Người tạo | 1 |
| ApprovedBy | VARCHAR(100) | Người duyệt | Manager A |
| ApprovedDate | DATETIME | Ngày duyệt | 2024-01-05 |
| IsApproved | BOOLEAN | Đã duyệt | TRUE |

**Relationships:**
- SupplierId → Suppliers(Id)
- EmployeeId → Employees(Id)

**Indexes:**
- UNIQUE: OrderNumber
- INDEX: SupplierId, Status, OrderDate, IsApproved

---

### **25. PurchaseOrderItems Table**
Chi tiết đơn đặt hàng.

| Column | Type | Description | Example |
|--------|------|-------------|---------|
| Id | INT (PK) | ID chi tiết | 1 |
| PurchaseOrderId | INT (FK) | Đơn hàng | 1 |
| PartId | INT (FK) | Phụ tùng | 8 |
| QuantityOrdered | INT | SL đặt | 5 |
| QuantityReceived | INT | SL nhận | 5 |
| UnitPrice | DECIMAL(15,2) | Đơn giá | 2500000 |
| TotalPrice | DECIMAL(15,2) | Thành tiền | 12500000 |
| SupplierPartNumber | VARCHAR(50) | Mã PT NCC | A2058200240 |
| PartDescription | VARCHAR(100) | Mô tả | LED Headlight |
| Unit | VARCHAR(50) | Đơn vị | Cái |
| ExpectedDeliveryDate | DATETIME | Dự kiến giao | 2024-01-10 |
| Notes | TEXT | Ghi chú | OEM parts |
| IsReceived | BOOLEAN | Đã nhận | TRUE |
| ReceivedDate | DATETIME | Ngày nhận | 2024-01-10 |

**Relationships:**
- PurchaseOrderId → PurchaseOrders(Id)
- PartId → Parts(Id)

**Indexes:**
- INDEX: PurchaseOrderId, PartId, IsReceived

---

## 📊 RELATIONSHIPS DIAGRAM

### **Module Dependencies:**

```
┌───────────────────────────────────────────────────────────────────┐
│                        VEHICLE MANAGEMENT                         │
│  VehicleBrands → VehicleModels → EngineSpecifications            │
│       ↓                                                           │
│  Vehicles ← VehicleInsurances ← InsuranceClaims                  │
│       ↓                              ↓                            │
│       └──────────────────────────────┴─── InsuranceClaimDocuments│
└───────────────────────────────────────────────────────────────────┘
                    ↓                          ↓
┌───────────────────────────────────────────────────────────────────┐
│                    PARTS & INVENTORY MANAGEMENT                   │
│  PartGroups ← PartGroupCompatibility                             │
│       ↓                                                           │
│  Parts ← PartInventoryBatches ← PartBatchUsages                  │
│       ↓            ↓                    ↓                         │
│  StockTransactions │                    │                         │
└───────────────────────────────────────────────────────────────────┘
                    ↓                          ↓
┌───────────────────────────────────────────────────────────────────┐
│                      SERVICE MANAGEMENT                           │
│  ServiceTypes → Services                                          │
│  LaborCategories → LaborItems                                     │
└───────────────────────────────────────────────────────────────────┘
                    ↓
┌───────────────────────────────────────────────────────────────────┐
│                      SERVICE ORDER MODULE                         │
│  ServiceOrders → ServiceOrderItems → ServiceOrderParts           │
│       ↓              ↓                    ↓                        │
│       └─────────→ ServiceOrderLabors                              │
└───────────────────────────────────────────────────────────────────┘
                    ↓
┌───────────────────────────────────────────────────────────────────┐
│                    FINANCIAL MANAGEMENT                           │
│  FinancialTransactions                                            │
│  PaymentTransactions                                              │
└───────────────────────────────────────────────────────────────────┘
                    ↓
┌───────────────────────────────────────────────────────────────────┐
│                SUPPLIER & PURCHASE MANAGEMENT                     │
│  Suppliers → PurchaseOrders → PurchaseOrderItems                 │
│       ↓                                                           │
│  PartSuppliers                                                    │
└───────────────────────────────────────────────────────────────────┘
```

---

## 🔑 KEY FEATURES

### **1. Phân loại xe theo mục đích sử dụng**
- **OwnershipType**: Personal, Company, Lease, Rental
- **UsageType**: Private, Commercial, Taxi, Delivery

### **2. Quản lý bảo hiểm đầy đủ**
- Lịch sử bảo hiểm từng xe
- Theo dõi claim chi tiết
- Tài liệu đính kèm claim

### **3. ⭐ Phân loại phụ tùng theo hóa đơn**
- **HasInvoice**: Có/Không hóa đơn
- **CanUseForCompany**: Dùng cho xe công ty
- **CanUseForInsurance**: Dùng cho xe bảo hiểm
- **CanUseForIndividual**: Dùng cho xe cá nhân
- **SourceType**: Purchased, Used, Refurbished, Salvage
- **Condition**: New, Used, Refurbished, AsIs

### **4. ⭐ Quản lý theo lô hàng**
- **PartInventoryBatches**: Mỗi lô có thông tin HĐ riêng
- **PartBatchUsages**: Theo dõi sử dụng từng lô
- Tính giá vốn chính xác theo FIFO/LIFO
- Đảm bảo không dùng nhầm hàng không HĐ cho công ty/BH

### **5. Quản lý công lao động chi tiết**
- Phân loại công theo danh mục
- Định mức giờ công chuẩn
- Theo dõi giờ công thực tế
- Tính toán tiền công tự động

### **6. Quản lý tài chính toàn diện**
- Thu nhập từ dịch vụ
- Chi phí nhập hàng
- Thanh toán lương
- Báo cáo lãi/lỗ

---

## 📈 TOTAL TABLES: 35

- **Vehicle Management**: 7 tables
- **Parts & Inventory**: 7 tables
- **Service Management**: 4 tables
- **Service Order**: 4 tables
- **Financial Management**: 2 tables
- **Supplier & Purchase**: 4 tables
- **Supporting**: 7 tables (Customers, Employees, Quotations, Inspections, Appointments...)

---

*Database Schema Version: 2.0.0*
*Last Updated: 2024-01-15*

