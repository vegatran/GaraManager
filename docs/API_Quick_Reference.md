# API Quick Reference

## 🚀 Quick Start

### 1. Setup Database
```sql
-- Run migration script
mysql -u root -p GarageManagementDB < docs/Migration_Add_Configuration_And_Documents.sql
```

### 2. Start Services
```bash
# Terminal 1: IdentityServer
cd src/GarageManagementSystem.IdentityServer
dotnet run

# Terminal 2: API
cd src/GarageManagementSystem.API
dotnet run

# Terminal 3: Web App
cd src/GarageManagementSystem.Web
dotnet run
```

### 3. Access
- **IdentityServer**: https://localhost:44333
- **API**: https://localhost:7100
- **Swagger**: https://localhost:7100/swagger
- **Web App**: https://localhost:7000

---

## 📌 Common Workflows

### Create Inspection → Quotation → Order
```http
# 1. Create Inspection
POST /api/Inspection
{
  "customerId": 1,
  "vehicleId": 1,
  "mileage": 50000,
  "findings": "Brake pads worn"
}

# 2. Complete Inspection
POST /api/Inspection/1/complete

# 3. Create Quotation from Inspection
POST /api/Quotation/from-inspection/1

# 4. Add Items to Quotation
POST /api/Quotation/1/items
{
  "itemType": "Part",
  "partId": 1,
  "partName": "Brake Pad Set",
  "quantity": 1,
  "unitPrice": 500000
}

# 5. Send Quotation
POST /api/Quotation/1/send

# 6. Convert to Service Order
POST /api/Quotation/1/convert-to-order

# 7. Start Work
POST /api/ServiceOrder/1/start

# 8. Complete Order
POST /api/ServiceOrder/1/complete
{"actualAmount": 1100000}

# 9. Create Invoice (auto VAT)
POST /api/Invoice/from-service-order/1

# 10. Create Payment
POST /api/Payment/from-invoice/1
{
  "amount": 1100000,
  "paymentMethod": "Cash"
}
```

### Create Insurance Claim
```http
# 1. Create Claim
POST /api/InsuranceClaim
{
  "customerId": 1,
  "vehicleId": 1,
  "insuranceCompany": "Bảo Việt",
  "policyNumber": "BV-2024-001",
  "accidentDate": "2025-10-01",
  "accidentDescription": "Front bumper damaged",
  "estimatedAmount": 5000000
}

# 2. Upload Documents
POST /api/InsuranceClaim/1/documents
Form-Data:
  - File: [photo.jpg]
  - DocumentType: "Photo"

# 3. Approve Claim (Admin/Manager)
POST /api/InsuranceClaim/1/approve
{
  "approve": true,
  "approvedAmount": 4500000
}

# 4. Create Service Order
POST /api/InsuranceClaim/1/create-service-order

# 5. Settle Claim
POST /api/InsuranceClaim/1/settle
{
  "settlementAmount": 4500000
}
```

---

## ⚙️ Configuration Management

### Change VAT Rate
```http
# Get current VAT config
GET /api/Configuration/VAT.Parts.Rate

# Update VAT rate
PUT /api/Configuration/1
{
  "configValue": "0.08"  # Change to 8%
}
```

### Add New Configuration
```http
POST /api/Configuration
{
  "configKey": "Invoice.LateFee.Rate",
  "configValue": "0.05",
  "dataType": "Decimal",
  "category": "Invoice",
  "description": "Late payment fee rate"
}
```

---

## 📊 Common Queries

### Get Statistics
```http
# Payment by method
GET /api/Payment/statistics/by-method?fromDate=2025-01-01&toDate=2025-12-31

# Payment by date
GET /api/Payment/statistics/by-date?fromDate=2025-10-01

# Invoices by status
GET /api/Invoice?status=Pending

# Orders in progress
GET /api/ServiceOrder?status=In Progress
```

---

## 🔐 Authentication

### Get Access Token (for testing)
```http
POST https://localhost:44333/connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password
&username=admin
&password=Admin@123
&client_id=garage.web
&client_secret=secret
&scope=garage.api openid profile
```

### Use Token
```http
GET /api/Invoice
Authorization: Bearer {access_token}
```

---

## 🎯 Key Endpoints

### Inspection
- `POST /api/Inspection` - Create
- `POST /api/Inspection/{id}/complete` - Complete
- `GET /api/Inspection?status=Completed` - List

### Quotation
- `POST /api/Quotation/from-inspection/{id}` - From Inspection
- `POST /api/Quotation/{id}/items` - Add Item
- `POST /api/Quotation/{id}/send` - Send
- `POST /api/Quotation/{id}/convert-to-order` - Convert

### Service Order
- `POST /api/ServiceOrder` - Create
- `POST /api/ServiceOrder/{id}/items` - Add Service
- `POST /api/ServiceOrder/{id}/parts` - Add Part
- `POST /api/ServiceOrder/{id}/start` - Start
- `POST /api/ServiceOrder/{id}/complete` - Complete

### Insurance Claim
- `POST /api/InsuranceClaim` - Create
- `POST /api/InsuranceClaim/{id}/documents` - Upload Doc
- `POST /api/InsuranceClaim/{id}/approve` - Approve
- `POST /api/InsuranceClaim/{id}/settle` - Settle

### Invoice
- `POST /api/Invoice/from-service-order/{id}` - Create from Order
- `GET /api/Invoice/{id}` - Get Details

### Payment
- `POST /api/Payment/from-invoice/{id}` - Pay Invoice
- `GET /api/Payment/methods` - Get Methods
- `GET /api/Payment/statistics/by-method` - Statistics

### Configuration
- `GET /api/Configuration` - List All
- `GET /api/Configuration/{key}` - Get by Key
- `PUT /api/Configuration/{id}` - Update

---

## 💡 Tips & Tricks

### 1. VAT Calculation
VAT is **automatically calculated** when adding items to Quotation/Invoice based on `SystemConfiguration`:
- Parts use `VAT.Parts.Rate`
- Services use `VAT.Services.Rate`

### 2. Number Generation
All entities auto-generate numbers: `INS-202510-0001`, `QT-202510-0001`, etc.

### 3. Status Flow
```
Inspection: Draft → Completed
Quotation: Draft → Sent → Converted
Service Order: Pending → In Progress → Completed
Insurance Claim: Pending → Approved → Settled
Invoice: Pending → Partially Paid → Paid
```

### 4. Partial Payments
You can create multiple payments for one invoice. Invoice status updates automatically:
- First payment < Total: "Partially Paid"
- Total payments >= Total: "Paid"

### 5. Configuration Cache
Configurations are **cached for 5 minutes**. After updating config:
- Wait 5 minutes for auto-refresh, OR
- Restart API to clear cache immediately

---

## 🐛 Common Issues

### Issue: "Cannot find configuration"
**Solution:** Run migration script to seed default configurations

### Issue: "Quotation already has service order"
**Solution:** Each quotation can only be converted once

### Issue: "Only Draft status can be edited"
**Solution:** Most entities can only be edited in Draft status

### Issue: "Invoice already paid"
**Solution:** Cannot delete paid invoices or paid payments

---

## 📝 Response Format

### Success
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... }
}
```

### Error
```json
{
  "success": false,
  "message": "Error description"
}
```

---

## 🔗 Related Documents

- [API Implementation Guide](./API_Implementation_Guide.md) - Detailed documentation
- [Invoice VAT Rules](./Invoice_VAT_Rules.md) - VAT calculation rules
- [Database Schema](./Database_Schema_Detail.md) - Database structure
- [User Manual](./User_Manual.md) - End-user guide

---

**Last Updated:** 2025-10-11

