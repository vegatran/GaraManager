# API Implementation Guide

## Tổng Quan

Tài liệu này mô tả chi tiết các API đã được implement cho hệ thống Garage Management System, bao gồm các quy trình nghiệp vụ chính và cách sử dụng.

## 📋 Table of Contents

1. [Configuration System](#1-configuration-system)
2. [Inspection API](#2-inspection-api)
3. [Quotation API](#3-quotation-api)
4. [Service Order API](#4-service-order-api)
5. [Insurance Claim API](#5-insurance-claim-api)
6. [Invoice API](#6-invoice-api)
7. [Payment API](#7-payment-api)
8. [Business Workflows](#8-business-workflows)

---

## 1. Configuration System

### Mục đích
Cho phép quản trị viên cấu hình linh hoạt các thông số hệ thống, đặc biệt là **VAT rates** mà không cần thay đổi code.

### Endpoints

#### GET `/api/Configuration`
Lấy danh sách tất cả configurations (có thể filter theo category và active status)

**Query Parameters:**
- `category` (optional): Filter by category
- `activeOnly` (optional): true/false

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "configKey": "VAT.Parts.Rate",
      "configValue": "0.10",
      "dataType": "Decimal",
      "category": "VAT",
      "description": "VAT rate for parts (default 10%)",
      "isActive": true
    }
  ]
}
```

#### GET `/api/Configuration/{key}`
Lấy configuration theo key

#### POST `/api/Configuration`
Tạo configuration mới

**Request Body:**
```json
{
  "configKey": "VAT.Parts.Rate",
  "configValue": "0.08",
  "dataType": "Decimal",
  "category": "VAT",
  "description": "VAT rate for parts"
}
```

#### PUT `/api/Configuration/{id}`
Cập nhật configuration

#### DELETE `/api/Configuration/{id}`
Xóa configuration

### Các Configuration Keys Quan Trọng

#### VAT Configuration
- `VAT.Parts.Rate`: Thuế VAT cho phụ tùng (default: 0.10 = 10%)
- `VAT.Services.Rate`: Thuế VAT cho dịch vụ (default: 0.10 = 10%)
- `VAT.Enabled`: Bật/tắt tính VAT (default: true)

#### Invoice Configuration
- `Invoice.NumberPrefix`: Tiền tố số hóa đơn (default: "INV")
- `Invoice.DueDays`: Số ngày đến hạn thanh toán (default: 30)
- `Invoice.Footer.Text`: Text footer của hóa đơn
- `Invoice.Logo.Path`: Đường dẫn logo công ty

#### Payment Configuration
- `Payment.Methods.Enabled`: Các phương thức thanh toán được phép
- `Payment.RequireApproval`: Yêu cầu phê duyệt thanh toán (default: false)
- `Payment.MinimumAmount`: Số tiền thanh toán tối thiểu

---

## 2. Inspection API

### Mục đích
Quản lý quy trình kiểm tra xe trước khi sửa chữa.

### Workflow
```
Create Inspection (Draft) → Complete Inspection → Create Quotation from Inspection
```

### Endpoints

#### GET `/api/Inspection`
Lấy danh sách inspections

**Query Parameters:**
- `status`: Draft, Completed
- `customerId`: Filter by customer
- `fromDate`, `toDate`: Date range

#### GET `/api/Inspection/{id}`
Lấy chi tiết inspection

#### POST `/api/Inspection`
Tạo inspection mới

**Request Body:**
```json
{
  "customerId": 1,
  "vehicleId": 1,
  "inspectionDate": "2025-10-11T10:00:00",
  "mileage": 50000,
  "inspectorId": 1,
  "findings": "Brake pads worn, oil leak detected",
  "recommendations": "Replace brake pads, fix oil seal",
  "notes": "Urgent repair needed"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Tạo phiếu kiểm tra thành công",
  "data": {
    "id": 1,
    "inspectionNumber": "INS-202510-0001"
  }
}
```

#### PUT `/api/Inspection/{id}`
Cập nhật inspection (chỉ Draft)

#### POST `/api/Inspection/{id}/complete`
Hoàn thành inspection (Draft → Completed)

#### DELETE `/api/Inspection/{id}`
Xóa inspection (chỉ Draft, chưa có quotation)

---

## 3. Quotation API

### Mục đích
Tạo báo giá cho khách hàng, tự động tính VAT theo config.

### Workflow
```
Create from Inspection OR Create Manual → Add Items → Send Quotation → Convert to Service Order
```

### Endpoints

#### GET `/api/Quotation`
Lấy danh sách quotations

#### GET `/api/Quotation/{id}`
Lấy chi tiết quotation với danh sách items

#### POST `/api/Quotation/from-inspection/{inspectionId}`
Tạo quotation từ inspection

**Response:**
```json
{
  "success": true,
  "message": "Tạo báo giá thành công",
  "data": {
    "id": 1,
    "quotationNumber": "QT-202510-0001"
  }
}
```

#### POST `/api/Quotation`
Tạo quotation thủ công

**Request Body:**
```json
{
  "customerId": 1,
  "vehicleId": 1,
  "expiryDate": "2025-11-10T00:00:00",
  "notes": "Special discount applied"
}
```

#### POST `/api/Quotation/{id}/items`
Thêm item vào quotation (tự động tính VAT theo config)

**Request Body:**
```json
{
  "itemType": "Part",
  "partId": 1,
  "partName": "Brake Pad Set",
  "quantity": 1,
  "unitPrice": 500000,
  "description": "Front brake pads"
}
```

hoặc

```json
{
  "itemType": "Service",
  "serviceId": 1,
  "serviceName": "Oil Change",
  "quantity": 1,
  "unitPrice": 300000
}
```

**VAT Calculation:**
- System tự động lấy VAT rate từ config (`VAT.Parts.Rate` hoặc `VAT.Services.Rate`)
- Tính: SubTotal = Quantity × UnitPrice
- VATAmount = SubTotal × VATRate
- TotalAmount = SubTotal + VATAmount

#### DELETE `/api/Quotation/{id}/items/{itemId}`
Xóa item khỏi quotation

#### POST `/api/Quotation/{id}/send`
Gửi quotation cho khách hàng (Draft → Sent)

#### POST `/api/Quotation/{id}/convert-to-order`
Chuyển quotation thành Service Order (copy tất cả items)

---

## 4. Service Order API

### Mục đích
Quản lý đơn hàng sửa chữa, theo dõi tiến độ công việc.

### Workflow
```
Create Order → Add Items/Parts → Start Work → Complete → Create Invoice
```

### Endpoints

#### GET `/api/ServiceOrder`
Lấy danh sách service orders

**Query Parameters:**
- `status`: Pending, In Progress, Completed, Cancelled
- `customerId`, `fromDate`, `toDate`

#### GET `/api/ServiceOrder/{id}`
Lấy chi tiết order với service items và parts

#### POST `/api/ServiceOrder`
Tạo service order mới

**Request Body:**
```json
{
  "customerId": 1,
  "vehicleId": 1,
  "quotationId": 1,
  "insuranceClaimId": null,
  "description": "Regular maintenance"
}
```

#### POST `/api/ServiceOrder/{id}/items`
Thêm service item

**Request Body:**
```json
{
  "serviceId": 1,
  "serviceName": "Oil Change",
  "quantity": 1,
  "unitPrice": 300000
}
```

#### POST `/api/ServiceOrder/{id}/parts`
Thêm part

**Request Body:**
```json
{
  "partId": 1,
  "partName": "Engine Oil 5W-30",
  "quantity": 4,
  "unitPrice": 150000
}
```

#### DELETE `/api/ServiceOrder/{id}/items/{itemId}`
Xóa service item

#### DELETE `/api/ServiceOrder/{id}/parts/{partId}`
Xóa part

#### POST `/api/ServiceOrder/{id}/start`
Bắt đầu làm việc (Pending → In Progress)

#### POST `/api/ServiceOrder/{id}/complete`
Hoàn thành đơn hàng (In Progress → Completed)

**Request Body:**
```json
{
  "actualAmount": 1500000
}
```

#### POST `/api/ServiceOrder/{id}/cancel`
Hủy đơn hàng

**Request Body:**
```json
{
  "reason": "Customer cancelled"
}
```

---

## 5. Insurance Claim API

### Mục đích
Quản lý hồ sơ bồi thường bảo hiểm, upload documents, tạo service order từ claim.

### Workflow
```
Create Claim → Upload Documents → Approve/Reject → Create Service Order → Settle
```

### Endpoints

#### GET `/api/InsuranceClaim`
Lấy danh sách insurance claims

#### GET `/api/InsuranceClaim/{id}`
Lấy chi tiết claim với documents

#### POST `/api/InsuranceClaim`
Tạo claim mới

**Request Body:**
```json
{
  "customerId": 1,
  "vehicleId": 1,
  "insuranceCompany": "Bảo Việt",
  "policyNumber": "BV-2024-001",
  "policyHolderName": "Nguyễn Văn A",
  "accidentDate": "2025-10-01T00:00:00",
  "accidentLocation": "Xa lộ Hà Nội, Tp.HCM",
  "accidentDescription": "Collision with motorcycle",
  "damageDescription": "Front bumper damaged, headlight broken",
  "estimatedAmount": 5000000,
  "notes": "Police report attached"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Tạo hồ sơ bồi thường thành công",
  "data": {
    "id": 1,
    "claimNumber": "CLM-202510-0001",
    "status": "Pending"
  }
}
```

#### PUT `/api/InsuranceClaim/{id}`
Cập nhật claim (chỉ Pending)

#### POST `/api/InsuranceClaim/{id}/approve`
Duyệt/Từ chối claim (Requires: Admin, Manager)

**Request Body:**
```json
{
  "approve": true,
  "approvedAmount": 4500000,
  "notes": "Approved with adjusted amount"
}
```

#### POST `/api/InsuranceClaim/{id}/settle`
Thanh toán bồi thường (Requires: Admin, Manager)

**Request Body:**
```json
{
  "settlementAmount": 4500000,
  "invoiceId": 1,
  "notes": "Settled via bank transfer"
}
```

#### POST `/api/InsuranceClaim/{id}/create-service-order`
Tạo Service Order từ approved claim

#### POST `/api/InsuranceClaim/{id}/documents`
Upload document (multipart/form-data)

**Form Data:**
- `File`: File to upload
- `DocumentType`: Photo, Police Report, Estimate, Invoice, Other

---

## 6. Invoice API

### Mục đích
Tạo hóa đơn từ Service Order, tự động tính VAT theo config.

### Workflow
```
Create Invoice from Service Order → Auto Calculate VAT → Send to Customer → Payment
```

### Endpoints

#### GET `/api/Invoice`
Lấy danh sách invoices

#### GET `/api/Invoice/{id}`
Lấy chi tiết invoice với items

#### POST `/api/Invoice/from-service-order/{serviceOrderId}`
Tạo invoice từ service order (tự động tính VAT)

**Response:**
```json
{
  "success": true,
  "message": "Tạo hóa đơn thành công",
  "data": {
    "id": 1,
    "invoiceNumber": "INV-202510-0001",
    "subTotal": 1000000,
    "vatAmount": 100000,
    "totalAmount": 1100000
  }
}
```

**VAT Calculation Logic:**
```csharp
// Get VAT rates from config
var partsVatRate = await _configService.GetDecimalConfigAsync("VAT.Parts.Rate", 0.10m);
var servicesVatRate = await _configService.GetDecimalConfigAsync("VAT.Services.Rate", 0.10m);

// Calculate for each item type
foreach (var item in serviceItems) {
    var vatRate = (item.ItemType == "Part") ? partsVatRate : servicesVatRate;
    item.VATAmount = item.SubTotal * vatRate;
    item.TotalAmount = item.SubTotal + item.VATAmount;
}
```

#### PUT `/api/Invoice/{id}/status`
Cập nhật status của invoice

#### DELETE `/api/Invoice/{id}`
Xóa invoice (chỉ Pending, chưa có payment)

---

## 7. Payment API

### Mục đích
Quản lý thanh toán đa phương thức (tiền mặt, chuyển khoản, thẻ, e-wallet).

### Workflow
```
Create Payment from Invoice → Select Payment Method → Complete → Update Invoice Status
```

### Endpoints

#### GET `/api/Payment`
Lấy danh sách payments

**Query Parameters:**
- `paymentMethod`: Cash, Bank Transfer, Credit Card, E-Wallet, QR Code
- `customerId`, `fromDate`, `toDate`

#### GET `/api/Payment/{id}`
Lấy chi tiết payment

#### POST `/api/Payment/from-invoice/{invoiceId}`
Tạo payment từ invoice (hỗ trợ thanh toán từng phần)

**Request Body:**
```json
{
  "amount": 1100000,
  "paymentMethod": "Bank Transfer",
  "referenceNumber": "TXN-20251011-001",
  "notes": "Paid via ACB bank"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "data": {
    "id": 1,
    "amount": 1100000,
    "invoiceStatus": "Paid"
  }
}
```

**Partial Payment Support:**
- Nếu `amount < invoice.TotalAmount`: Invoice status = "Partially Paid"
- Nếu tổng payments >= invoice.TotalAmount: Invoice status = "Paid"

#### POST `/api/Payment`
Tạo payment thủ công (không có invoice)

#### POST `/api/Payment/{id}/cancel`
Hủy payment (Requires: Admin, Manager)

**Request Body:**
```json
{
  "reason": "Duplicate payment"
}
```

#### GET `/api/Payment/statistics/by-method`
Thống kê payments theo phương thức

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "paymentMethod": "Bank Transfer",
      "count": 15,
      "totalAmount": 25000000
    },
    {
      "paymentMethod": "Cash",
      "count": 10,
      "totalAmount": 8000000
    }
  ]
}
```

#### GET `/api/Payment/statistics/by-date`
Thống kê payments theo ngày

#### GET `/api/Payment/methods`
Lấy danh sách payment methods available

**Response:**
```json
{
  "success": true,
  "data": [
    { "value": "Cash", "label": "Tiền mặt" },
    { "value": "Bank Transfer", "label": "Chuyển khoản" },
    { "value": "Credit Card", "label": "Thẻ tín dụng" },
    { "value": "E-Wallet", "label": "Ví điện tử" },
    { "value": "QR Code", "label": "QR Code" }
  ]
}
```

---

## 8. Business Workflows

### Workflow 1: Regular Service (Không có Inspection)
```
1. Customer calls → Create Appointment
2. Customer arrives → Create Service Order directly
3. Add Services and Parts → Start Work
4. Complete Service Order → Create Invoice (auto VAT)
5. Customer pays → Create Payment
```

### Workflow 2: Service with Inspection & Quotation
```
1. Customer arrives → Create Inspection
2. Inspector examines vehicle → Complete Inspection
3. Create Quotation from Inspection
4. Add recommended Services/Parts → Send Quotation
5. Customer accepts → Convert Quotation to Service Order
6. Start Work → Complete → Create Invoice → Payment
```

### Workflow 3: Insurance Claim
```
1. Customer reports accident → Create Insurance Claim
2. Upload documents (photos, police report)
3. Manager reviews → Approve Claim
4. Create Service Order from Claim
5. Perform repairs → Create Invoice
6. Settle Claim → Create Payment
```

### Workflow 4: Quotation Only (Customer Request)
```
1. Customer requests estimate → Create Quotation manually
2. Add Services/Parts → Send Quotation
3. Customer may or may not proceed
4. If accepted → Convert to Service Order
```

---

## 9. Key Features

### ✅ Flexible VAT System
- **Configurable rates** via `SystemConfiguration`
- Separate rates for Parts and Services
- Can enable/disable VAT globally
- **No code changes required** to adjust VAT rates

### ✅ Auto Number Generation
- Inspection: `INS-YYYYMM-XXXX`
- Quotation: `QT-YYYYMM-XXXX`
- Service Order: `SO-YYYYMM-XXXX`
- Insurance Claim: `CLM-YYYYMM-XXXX`
- Invoice: `INV-YYYYMM-XXXX` (configurable prefix)

### ✅ Status Tracking
- **Inspection**: Draft → Completed
- **Quotation**: Draft → Sent → Accepted/Rejected → Converted
- **Service Order**: Pending → In Progress → Completed/Cancelled
- **Insurance Claim**: Pending → Approved/Rejected → Settled
- **Invoice**: Pending → Partially Paid → Paid
- **Payment**: Completed/Cancelled

### ✅ Document Management
- Upload documents for Insurance Claims
- Support multiple document types
- Track file metadata (size, uploader, timestamp)

### ✅ Partial Payment Support
- Allow multiple payments for one invoice
- Auto-update invoice status based on total paid
- Track payment history

### ✅ Role-Based Authorization
- Some endpoints require Admin/Manager roles
- Configured via `[Authorize(Roles = "Admin,Manager")]`

---

## 10. Configuration Management

### Thay đổi VAT Rate

**Option 1: Via API**
```http
PUT /api/Configuration/1
Content-Type: application/json

{
  "configValue": "0.08"  // Change from 10% to 8%
}
```

**Option 2: Via Database**
```sql
UPDATE SystemConfiguration 
SET ConfigValue = '0.08', UpdatedAt = NOW()
WHERE ConfigKey = 'VAT.Parts.Rate';
```

**Lưu ý:**
- ConfigurationService có **cache 5 phút**, nên thay đổi sẽ có hiệu lực sau tối đa 5 phút
- Hoặc restart API để clear cache ngay lập tức

---

## 11. Error Handling

Tất cả API đều trả về consistent error format:

**Success Response:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... }
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Error description"
}
```

**HTTP Status Codes:**
- `200 OK`: Success
- `400 Bad Request`: Validation errors, business rule violations
- `401 Unauthorized`: Not authenticated
- `403 Forbidden`: Not authorized (role-based)
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server errors

---

## 12. Testing Recommendations

### Unit Tests
- Test VAT calculation logic with different rates
- Test number generation with edge cases
- Test status transitions

### Integration Tests
- Test complete workflows end-to-end
- Test with different configurations
- Test authorization rules

### Performance Tests
- Test with large datasets
- Test configuration cache effectiveness
- Test concurrent operations

---

## 13. Next Steps

### Phase 1 (Completed) ✅
- Configuration System
- Inspection, Quotation, Service Order APIs
- Insurance Claim, Invoice, Payment APIs

### Phase 2 (Recommended)
- Email notifications (quotation sent, invoice created)
- SMS notifications
- Print/PDF generation for Invoices and Quotations
- Customer portal for viewing quotations/invoices
- Mobile app APIs

### Phase 3 (Advanced)
- Inventory management integration
- Auto reorder for low-stock parts
- Employee commission tracking
- Advanced reporting and analytics
- Integration with accounting systems

---

## 14. Support & Maintenance

### Logging
- All controllers use `ILogger` for error logging
- Check logs for debugging issues

### Database Migrations
- Run `Migration_Add_Configuration_And_Documents.sql` to create new tables
- Seed default configurations automatically

### Cache Management
- Configuration cache: 5 minutes
- Clear cache: Restart API or wait for expiration

---

**Document Version:** 1.0  
**Last Updated:** 2025-10-11  
**Author:** Garage Management System Team

