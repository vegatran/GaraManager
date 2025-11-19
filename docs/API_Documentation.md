# API DOCUMENTATION - GARAGE MANAGEMENT SYSTEM (QUY TRÌNH MỚI)

## 📋 MỤC LỤC
1. [Overview](#overview)
2. [Authentication](#authentication)
3. [🔄 WORKFLOW APIs (QUY TRÌNH MỚI)](#-workflow-apis-quy-trình-mới)
   - [Customer Reception APIs](#customer-reception-apis)
   - [Vehicle Inspection APIs](#vehicle-inspection-apis)
   - [Service Quotation APIs](#service-quotation-apis)
   - [Service Order APIs](#service-order-apis)
4. [Vehicle Management APIs](#vehicle-management-apis)
5. [Parts & Inventory APIs](#parts--inventory-apis)
6. [Financial Management APIs](#financial-management-apis)
7. [Reporting APIs](#reporting-apis)
8. [🛒 Procurement Management APIs](#-procurement-management-apis-phase-42)
   - [Demand Analysis](#phase-421-demand-analysis)
   - [Supplier Evaluation](#phase-422-supplier-evaluation)
   - [Request Quotation](#phase-422-optional-request-quotation)
   - [PO Tracking](#phase-423-po-tracking)
   - [Performance Evaluation](#phase-424-performance-evaluation)
9. [Error Handling](#error-handling)

---

## 🌐 OVERVIEW

### **Base URL**
```
Development: https://localhost:5001/api
Production:  https://api.garagemanagement.com/api
```

### **API Version**
```
Current Version: v1
```

### **Response Format**
All API responses follow this standard format:

**Success Response:**
```json
{
  "success": true,
  "data": {},
  "message": "Operation completed successfully",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Error Response:**
```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Error description",
    "details": ["Detail 1", "Detail 2"]
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

## 🔐 AUTHENTICATION

### **Login**
```http
POST /api/auth/login
```

**Request Body:**
```json
{
  "username": "admin@garage.com",
  "password": "password123",
  "rememberMe": true
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...",
    "expiresIn": 3600,
    "tokenType": "Bearer",
    "user": {
      "id": 1,
      "username": "admin@garage.com",
      "fullName": "Administrator",
      "role": "Admin",
      "permissions": ["ALL"]
    }
  },
  "message": "Login successful"
}
```

### **Refresh Token**
```http
POST /api/auth/refresh
```

**Request Body:**
```json
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
}
```

### **Logout**
```http
POST /api/auth/logout
Authorization: Bearer {access_token}
```

---

## 🔄 WORKFLOW APIs (QUY TRÌNH MỚI)

### **🔄 CUSTOMER RECEPTION APIs**

**Base Endpoint:** `/api/CustomerReceptions`

#### **1. Get All Customer Receptions**
```http
GET /api/CustomerReceptions
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "receptionNumber": "REC-20250115-0001",
      "customerId": 1,
      "customerName": "Nguyễn Văn A",
      "vehicleId": 1,
      "vehiclePlate": "29A-12345",
      "vehicleMake": "Toyota",
      "vehicleModel": "Camry",
      "vehicleYear": 2020,
      "serviceType": "Maintenance",
      "priority": "Normal",
      "assignedTechnicianId": 1,
      "assignedTechnician": {
        "id": 1,
        "name": "Trần Văn B"
      },
      "status": "Assigned",
      "receptionDate": "2025-01-15T08:00:00Z",
      "inspectionStartDate": "2025-01-15T09:00:00Z",
      "inspectionCompletedDate": "2025-01-15T17:00:00Z",
      "customerRequest": "Kiểm tra toàn bộ xe",
      "customerComplaints": "Xe có tiếng kêu lạ",
      "receptionNotes": "Khách hàng VIP"
    }
  ],
  "message": "Lấy danh sách phiếu tiếp đón thành công"
}
```

#### **2. Create Customer Reception**
```http
POST /api/CustomerReceptions
```

**Request Body:**
```json
{
  "customerId": 1,
  "vehicleId": 1,
  "serviceType": "Maintenance",
  "priority": "Normal",
  "assignedTechnicianId": 1,
  "inspectionStartDate": "2025-01-15T09:00:00Z",
  "customerRequest": "Kiểm tra toàn bộ xe",
  "customerComplaints": "Xe có tiếng kêu lạ",
  "receptionNotes": "Khách hàng VIP"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "receptionNumber": "REC-20250115-0001",
    "status": "Assigned",
    "message": "Tạo phiếu tiếp đón thành công"
  }
}
```

#### **3. Get Available Receptions for Inspection**
```http
GET /api/CustomerReceptions/GetAvailableForInspection
```

**Response:** Danh sách phiếu tiếp đón có status "Assigned" để tạo kiểm tra xe.

#### **4. Update Reception Status**
```http
PUT /api/CustomerReceptions/{id}/UpdateStatus
```

**Request Body:**
```json
{
  "status": "InProgress",
  "notes": "Bắt đầu kiểm tra xe"
}
```

### **🔍 VEHICLE INSPECTION APIs**

**Base Endpoint:** `/api/VehicleInspections`

#### **1. Create Inspection from Reception**
```http
POST /api/VehicleInspections
```

**Request Body:**
```json
{
  "customerReceptionId": 1,
  "vehicleId": 1,
  "customerId": 1,
  "inspectorId": 1,
  "inspectionDate": "2025-01-15T09:00:00Z",
  "inspectionType": "General",
  "currentMileage": 50000,
  "fuelLevel": "Half",
  "generalCondition": "Good",
  "exteriorCondition": "Good",
  "interiorCondition": "Good",
  "engineCondition": "Good",
  "brakeCondition": "Good",
  "suspensionCondition": "Good",
  "tireCondition": "Good",
  "electricalCondition": "Good",
  "lightsCondition": "Good",
  "customerComplaints": "Xe có tiếng kêu lạ",
  "recommendations": "Thay dầu động cơ",
  "technicianNotes": "Cần kiểm tra thêm hệ thống phanh"
}
```

**Business Rules:**
- Chỉ tạo từ CustomerReception có status "Assigned"
- Tự động cập nhật CustomerReception status = "InProgress"

#### **2. Complete Inspection**
```http
PUT /api/VehicleInspections/{id}/Complete
```

**Response:** Tự động cập nhật CustomerReception status = "Inspected"

#### **3. Get Available Inspections for Quotation**
```http
GET /api/VehicleInspections/GetAvailableForQuotation
```

**Response:** Danh sách inspection có status "Completed" để tạo báo giá.

### **💰 SERVICE QUOTATION APIs**

**Base Endpoint:** `/api/ServiceQuotations`

#### **1. Create Quotation from Inspection**
```http
POST /api/ServiceQuotations
```

**Request Body:**
```json
{
  "vehicleInspectionId": 1,
  "vehicleId": 1,
  "customerId": 1,
  "description": "Báo giá sửa chữa sau kiểm tra",
  "validUntil": "2025-01-22T17:00:00Z",
  "taxRate": 0.1,
  "discountAmount": 0,
  "items": [
    {
      "serviceId": 1,
      "itemName": "Thay dầu động cơ",
      "description": "Thay dầu động cơ 5W-30",
      "quantity": 1,
      "unitPrice": 500000,
      "itemType": "Labor"
    }
  ]
}
```

**Business Rules:**
- Chỉ tạo từ VehicleInspection có status "Completed"
- Tự động cập nhật CustomerReception status = "Quoted"

#### **2. Approve Quotation**
```http
PUT /api/ServiceQuotations/{id}/Approve
```

**Response:** Tự động cập nhật CustomerReception status = "Quoted"

#### **3. Get Available Quotations for Service Order**
```http
GET /api/ServiceQuotations/GetAvailableForOrder
```

**Response:** Danh sách quotation có status "Approved" để tạo phiếu sửa chữa.

### **🛠️ SERVICE ORDER APIs (Updated)**

**Base Endpoint:** `/api/ServiceOrders`

#### **1. Create Service Order from Quotation**
```http
POST /api/ServiceOrders
```

**Request Body:**
```json
{
  "serviceQuotationId": 1,
  "vehicleId": 1,
  "customerId": 1,
  "orderDate": "2025-01-15T08:00:00Z",
  "estimatedStartDate": "2025-01-16T08:00:00Z",
  "estimatedEndDate": "2025-01-17T17:00:00Z",
  "priority": "Normal",
  "notes": "Bắt đầu sửa chữa theo báo giá",
  "status": "Pending"
}
```

**Business Rules:**
- Chỉ tạo từ ServiceQuotation có status "Approved"
- Tự động cập nhật CustomerReception status = "ServiceOrdered"

---

## 🚗 VEHICLE MANAGEMENT APIS

### **1. Get All Vehicles**
```http
GET /api/vehicles
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20)
- `search` (string): Search by license plate, brand, model
- `ownershipType` (string): Personal, Company, Lease, Rental
- `hasInsurance` (bool): Filter by insurance status
- `customerId` (int): Filter by customer

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "licensePlate": "30A-12345",
        "brand": "Mercedes-Benz",
        "model": "C-Class W205",
        "year": "2020",
        "color": "Đen",
        "vin": "WDD2050461A123456",
        "engineNumber": "M274920123456",
        "ownershipType": "Personal",
        "usageType": "Private",
        "hasInsurance": true,
        "isInsuranceActive": true,
        "insuranceCompany": "Bảo Việt",
        "insuranceEndDate": "2024-12-31T00:00:00Z",
        "customer": {
          "id": 1,
          "fullName": "Nguyễn Minh Tuấn",
          "phone": "0901234567",
          "email": "tuan.nguyen@email.com"
        },
        "createdAt": "2024-01-15T10:30:00Z"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 20,
      "totalPages": 5,
      "totalItems": 100
    }
  }
}
```

### **2. Get Vehicle by ID**
```http
GET /api/vehicles/{id}
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "licensePlate": "30A-12345",
    "brand": "Mercedes-Benz",
    "model": "C-Class W205",
    "year": "2020",
    "color": "Đen",
    "vin": "WDD2050461A123456",
    "engineNumber": "M274920123456",
    "customerId": 1,
    "ownershipType": "Personal",
    "usageType": "Private",
    "hasInsurance": true,
    "isInsuranceActive": true,
    "insuranceCompany": "Bảo Việt",
    "policyNumber": "BV123456789",
    "insuranceStartDate": "2024-01-01T00:00:00Z",
    "insuranceEndDate": "2024-12-31T00:00:00Z",
    "insurancePremium": 5000000,
    "companyName": null,
    "taxCode": null,
    "customer": {
      "id": 1,
      "firstName": "Nguyễn",
      "lastName": "Minh Tuấn",
      "phone": "0901234567",
      "email": "tuan.nguyen@email.com",
      "address": "123 Nguyễn Huệ, Q1, TP.HCM"
    },
    "serviceHistory": [
      {
        "orderNumber": "SO-2024-001",
        "orderDate": "2024-01-15T10:30:00Z",
        "status": "Completed",
        "totalAmount": 3525000
      }
    ],
    "createdAt": "2024-01-10T08:00:00Z",
    "updatedAt": "2024-01-15T15:00:00Z"
  }
}
```

### **3. Create Vehicle**
```http
POST /api/vehicles
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "licensePlate": "30A-12345",
  "brand": "Mercedes-Benz",
  "model": "C-Class W205",
  "year": "2020",
  "color": "Đen",
  "vin": "WDD2050461A123456",
  "engineNumber": "M274920123456",
  "customerId": 1,
  "ownershipType": "Personal",
  "usageType": "Private",
  "hasInsurance": true,
  "insuranceCompany": "Bảo Việt",
  "policyNumber": "BV123456789",
  "coverageType": "Comprehensive",
  "insuranceStartDate": "2024-01-01",
  "insuranceEndDate": "2024-12-31",
  "insurancePremium": 5000000
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "licensePlate": "30A-12345",
    "message": "Vehicle created successfully"
  },
  "message": "Vehicle registered successfully"
}
```

### **4. Update Vehicle**
```http
PUT /api/vehicles/{id}
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:** (Same as Create, all fields optional)

### **5. Delete Vehicle**
```http
DELETE /api/vehicles/{id}
Authorization: Bearer {access_token}
```

### **6. Get Vehicle Insurance History**
```http
GET /api/vehicles/{id}/insurances
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "vehicleId": 1,
    "licensePlate": "30A-12345",
    "insurances": [
      {
        "id": 1,
        "policyNumber": "BV2024001",
        "insuranceCompany": "Bảo Việt",
        "coverageType": "Comprehensive",
        "startDate": "2024-01-01T00:00:00Z",
        "endDate": "2024-12-31T00:00:00Z",
        "premiumAmount": 5000000,
        "isActive": true,
        "claims": [
          {
            "claimNumber": "CLM-2024-001",
            "claimStatus": "Settled",
            "approvedAmount": 4200000
          }
        ]
      }
    ]
  }
}
```

### **7. Create Insurance Claim**
```http
POST /api/vehicles/{vehicleId}/claims
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "vehicleInsuranceId": 1,
  "serviceOrderId": 1,
  "claimNumber": "CLM-2024-001",
  "incidentType": "Accident",
  "incidentDescription": "Va chạm từ phía sau",
  "incidentDate": "2024-01-14T15:30:00Z",
  "incidentLocation": "123 Nguyễn Huệ, Q1",
  "policeReportNumber": "PB-2024-001",
  "estimatedDamage": 4500000,
  "adjusterName": "Trần Văn D",
  "adjusterPhone": "0908123456"
}
```

---

## 📦 PARTS & INVENTORY APIS

### **1. Get All Parts**
```http
GET /api/parts
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `page` (int): Page number
- `pageSize` (int): Items per page
- `search` (string): Search by part number, name
- `category` (string): Filter by category
- `hasInvoice` (bool): Filter by invoice status
- `sourceType` (string): Purchased, Used, Refurbished, Salvage
- `canUseForCompany` (bool): For company vehicles
- `canUseForInsurance` (bool): For insurance claims
- `lowStock` (bool): Parts below minimum stock

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "partNumber": "HEADLIGHT001",
        "partName": "Đèn pha Mercedes C-Class W205",
        "category": "Electrical System",
        "brand": "Mercedes-Benz",
        "costPrice": 2500000,
        "sellPrice": 3200000,
        "quantityInStock": 3,
        "minimumStock": 1,
        "unit": "Cái",
        "location": "Kho D-02",
        "hasInvoice": true,
        "sourceType": "Purchased",
        "condition": "New",
        "canUseForCompany": true,
        "canUseForInsurance": true,
        "canUseForIndividual": true,
        "isOEM": true,
        "warrantyMonths": 24,
        "partGroup": {
          "id": 9,
          "groupName": "Đèn pha",
          "groupCode": "HEADLIGHT"
        }
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 20,
      "totalPages": 10,
      "totalItems": 200
    }
  }
}
```

### **2. Get Part by ID**
```http
GET /api/parts/{id}
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "partNumber": "HEADLIGHT001",
    "partName": "Đèn pha Mercedes C-Class W205",
    "description": "LED headlight for Mercedes C-Class W205",
    "category": "Electrical System",
    "brand": "Mercedes-Benz",
    "costPrice": 2500000,
    "averageCostPrice": 2500000,
    "sellPrice": 3200000,
    "quantityInStock": 3,
    "minimumStock": 1,
    "reorderLevel": 2,
    "unit": "Cái",
    "compatibleVehicles": "Mercedes C-Class W205 2014-2021",
    "location": "Kho D-02",
    "sourceType": "Purchased",
    "invoiceType": "WithInvoice",
    "hasInvoice": true,
    "canUseForCompany": true,
    "canUseForInsurance": true,
    "canUseForIndividual": true,
    "condition": "New",
    "sourceReference": "Nhập từ MB Vietnam",
    "partGroupId": 9,
    "oemNumber": "A2058200240",
    "aftermarketNumber": "OSRAM LED",
    "manufacturer": "Osram",
    "dimensions": "150x80x120mm",
    "weight": 2.1,
    "material": "Nhựa + LED",
    "color": "Trong suốt",
    "warrantyMonths": 24,
    "warrantyConditions": "2 năm hoặc 50,000km",
    "isOEM": true,
    "isActive": true,
    "inventoryBatches": [
      {
        "batchNumber": "BATCH-2024-001",
        "quantityRemaining": 3,
        "unitCost": 2500000,
        "hasInvoice": true,
        "supplierName": "MB Vietnam",
        "receiveDate": "2024-01-10T08:00:00Z"
      }
    ],
    "stockHistory": [
      {
        "transactionNumber": "STK-2024-001",
        "transactionType": "In",
        "quantity": 5,
        "transactionDate": "2024-01-10T08:00:00Z"
      }
    ]
  }
}
```

### **3. Create Part**
```http
POST /api/parts
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "partNumber": "HEADLIGHT001",
  "partName": "Đèn pha Mercedes C-Class W205",
  "description": "LED headlight",
  "category": "Electrical System",
  "brand": "Mercedes-Benz",
  "costPrice": 2500000,
  "sellPrice": 3200000,
  "minimumStock": 1,
  "unit": "Cái",
  "location": "Kho D-02",
  "sourceType": "Purchased",
  "invoiceType": "WithInvoice",
  "hasInvoice": true,
  "canUseForCompany": true,
  "canUseForInsurance": true,
  "canUseForIndividual": true,
  "condition": "New",
  "partGroupId": 9,
  "oemNumber": "A2058200240",
  "manufacturer": "Osram",
  "warrantyMonths": 24,
  "isOEM": true
}
```

### **4. Create Inventory Batch**
```http
POST /api/parts/{partId}/batches
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "batchNumber": "BATCH-2024-001",
  "quantityReceived": 5,
  "unitCost": 2500000,
  "sourceType": "Purchased",
  "condition": "New",
  "hasInvoice": true,
  "invoiceNumber": "BV20240001",
  "invoiceDate": "2024-01-10",
  "supplierName": "MB Vietnam",
  "supplierId": 1,
  "canUseForCompany": true,
  "canUseForInsurance": true,
  "canUseForIndividual": true,
  "location": "Kho D-02",
  "shelf": "D-02",
  "bin": "D-02-A",
  "notes": "Nhập hàng chính hãng"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "batchId": 1,
    "batchNumber": "BATCH-2024-001",
    "partId": 1,
    "quantityReceived": 5,
    "message": "Inventory batch created successfully"
  }
}
```

### **5. Get Low Stock Parts**
```http
GET /api/parts/low-stock
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "partNumber": "HEADLIGHT001",
        "partName": "Đèn pha Mercedes C-Class",
        "quantityInStock": 1,
        "minimumStock": 2,
        "reorderLevel": 3,
        "deficit": 1,
        "suggestedOrderQuantity": 5,
        "supplier": {
          "id": 1,
          "supplierName": "MB Vietnam",
          "leadTimeDays": 7
        }
      }
    ],
    "totalItems": 15,
    "urgentItems": 5
  }
}
```

### **6. Stock Transaction**
```http
POST /api/stock/transactions
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "partId": 1,
  "transactionType": "In",
  "quantity": 5,
  "unitCost": 2500000,
  "supplierName": "MB Vietnam",
  "invoiceNumber": "BV20240001",
  "hasInvoice": true,
  "sourceType": "Purchased",
  "condition": "New",
  "notes": "Nhập hàng định kỳ"
}
```

---

## 🛠️ SERVICE ORDER APIS

### **1. Create Service Order**
```http
POST /api/service-orders
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "customerId": 1,
  "vehicleId": 1,
  "scheduledDate": "2024-01-16T08:00:00Z",
  "notes": "Khách yêu cầu thay đèn pha",
  "services": [
    {
      "serviceId": 1,
      "quantity": 1,
      "unitPrice": 200000
    }
  ],
  "parts": [
    {
      "partId": 1,
      "partInventoryBatchId": 1,
      "quantity": 1,
      "unitCost": 2500000,
      "unitPrice": 3200000
    }
  ],
  "labors": [
    {
      "laborItemId": 1,
      "estimatedHours": 1.5,
      "laborRate": 50000
    }
  ],
  "primaryTechnicianId": 1
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "orderId": 1,
    "orderNumber": "SO-2024-001",
    "totalAmount": 3500000,
    "estimatedCompletionTime": "2024-01-16T15:00:00Z",
    "message": "Service order created successfully"
  }
}
```

### **2. Get Service Order by ID**
```http
GET /api/service-orders/{id}
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "orderNumber": "SO-2024-001",
    "orderDate": "2024-01-15T10:30:00Z",
    "scheduledDate": "2024-01-16T08:00:00Z",
    "completedDate": "2024-01-16T15:00:00Z",
    "status": "Completed",
    "customer": {
      "id": 1,
      "fullName": "Nguyễn Minh Tuấn",
      "phone": "0901234567"
    },
    "vehicle": {
      "id": 1,
      "licensePlate": "30A-12345",
      "brand": "Mercedes-Benz",
      "model": "C-Class W205",
      "ownershipType": "Personal"
    },
    "services": [
      {
        "serviceId": 1,
        "serviceName": "Thay đèn pha Mercedes C-Class",
        "quantity": 1,
        "unitPrice": 200000,
        "totalPrice": 200000
      }
    ],
    "parts": [
      {
        "partId": 1,
        "partName": "Đèn pha Mercedes C-Class W205",
        "quantity": 1,
        "unitCost": 2500000,
        "unitPrice": 3200000,
        "totalPrice": 3200000,
        "hasInvoice": true,
        "batchNumber": "BATCH-2024-001"
      }
    ],
    "labors": [
      {
        "laborItemId": 1,
        "laborName": "Công tháo đèn pha",
        "estimatedHours": 1.5,
        "actualHours": 1.5,
        "laborRate": 50000,
        "totalCost": 75000,
        "employeeName": "Nguyễn Văn Minh",
        "status": "Completed"
      }
    ],
    "serviceTotal": 200000,
    "partsTotal": 3200000,
    "laborTotal": 75000,
    "totalAmount": 3475000,
    "discountAmount": 0,
    "finalAmount": 3475000,
    "amountPaid": 3475000,
    "amountRemaining": 0,
    "paymentStatus": "Paid"
  }
}
```

### **3. Update Service Order Status**
```http
PATCH /api/service-orders/{id}/status
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "status": "InProgress",
  "notes": "Bắt đầu sửa chữa"
}
```

### **4. Complete Service Order**
```http
POST /api/service-orders/{id}/complete
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "actualLabors": [
    {
      "laborId": 1,
      "actualHours": 1.5
    }
  ],
  "completionNotes": "Hoàn thành đúng tiến độ"
}
```

### **5. Get Service Orders**
```http
GET /api/service-orders
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `page`, `pageSize`
- `status`: Pending, InProgress, Completed, Cancelled
- `customerId`
- `vehicleId`
- `fromDate`, `toDate`
- `paymentStatus`: Unpaid, Partial, Paid

---

## 💰 FINANCIAL MANAGEMENT APIS

### **1. Create Financial Transaction**
```http
POST /api/financial/transactions
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "transactionType": "Income",
  "category": "Service Revenue",
  "subCategory": "Auto Repair",
  "amount": 3525000,
  "currency": "VND",
  "paymentMethod": "Cash",
  "description": "Thu từ đơn hàng SO-2024-001",
  "relatedEntity": "ServiceOrder",
  "relatedEntityId": 1
}
```

### **2. Get Financial Report**
```http
GET /api/financial/reports
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `reportType`: Daily, Weekly, Monthly, Yearly, Custom
- `fromDate`, `toDate`
- `groupBy`: Category, PaymentMethod

**Response:**
```json
{
  "success": true,
  "data": {
    "period": "2024-01",
    "totalIncome": 81500000,
    "totalExpense": 73100000,
    "netProfit": 8400000,
    "profitMargin": 10.3,
    "incomeByCategory": [
      {
        "category": "Service Revenue",
        "amount": 45500000,
        "percentage": 55.8
      },
      {
        "category": "Parts Sales",
        "amount": 23200000,
        "percentage": 28.5
      }
    ],
    "expenseByCategory": [
      {
        "category": "Parts Purchase",
        "amount": 52300000,
        "percentage": 71.5
      },
      {
        "category": "Labor Cost",
        "amount": 15600000,
        "percentage": 21.3
      }
    ]
  }
}
```

### **3. Get Outstanding Debts**
```http
GET /api/financial/debts
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `status`: All, Overdue, DueSoon
- `customerId`

**Response:**
```json
{
  "success": true,
  "data": {
    "totalOutstanding": 8500000,
    "overdueAmount": 2500000,
    "overdueCount": 2,
    "dueSoonAmount": 6000000,
    "dueSoonCount": 3,
    "debts": [
      {
        "serviceOrderId": 10,
        "orderNumber": "SO-2024-010",
        "customer": {
          "id": 3,
          "fullName": "Trần Văn Đức",
          "phone": "0901234569"
        },
        "totalAmount": 4500000,
        "amountPaid": 2000000,
        "amountRemaining": 2500000,
        "dueDate": "2024-01-25T00:00:00Z",
        "daysOverdue": 6,
        "status": "Overdue"
      }
    ]
  }
}
```

---

## 📊 REPORTING APIS

### **1. Dashboard Summary**
```http
GET /api/reports/dashboard
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `period`: Today, Week, Month, Year

**Response:**
```json
{
  "success": true,
  "data": {
    "period": "Month",
    "summary": {
      "totalRevenue": 81500000,
      "totalOrders": 25,
      "completedOrders": 20,
      "pendingOrders": 3,
      "cancelledOrders": 2,
      "averageOrderValue": 3260000,
      "newCustomers": 5,
      "returningCustomers": 15
    },
    "trends": {
      "revenueChange": "+5.2%",
      "ordersChange": "+12.5%",
      "customersChange": "+8.3%"
    },
    "topServices": [
      {
        "serviceName": "Thay dầu động cơ",
        "count": 10,
        "revenue": 1500000
      }
    ],
    "topParts": [
      {
        "partName": "Dầu động cơ 5W-30",
        "quantitySold": 50,
        "revenue": 7500000
      }
    ],
    "lowStockAlerts": 15,
    "insuranceExpiringAlerts": 5
  }
}
```

### **2. Sales Report**
```http
GET /api/reports/sales
Authorization: Bearer {access_token}
```

### **3. Inventory Report**
```http
GET /api/reports/inventory
Authorization: Bearer {access_token}
```

### **4. Customer Report**
```http
GET /api/reports/customers
Authorization: Bearer {access_token}
```

---

## 🛒 PROCUREMENT MANAGEMENT APIs (Phase 4.2)

**Base Endpoint:** `/api/procurement`

### **Phase 4.2.1: Demand Analysis**

#### **1. Get Demand Analysis**
```http
GET /api/procurement/demand-analysis?warehouseId={id}&priority={priority}&source={source}&pageNumber={page}&pageSize={size}
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `warehouseId` (optional): Filter by warehouse ID
- `priority` (optional): Filter by priority ("High", "Medium", "Low")
- `source` (optional): Filter by source ("InventoryAlert", "ServiceOrder", "All")
- `pageNumber` (default: 1): Page number
- `pageSize` (default: 20, max: 100): Items per page

**Response:**
```json
{
  "success": true,
  "data": {
    "data": [
      {
        "partId": 1,
        "partNumber": "PT001",
        "partName": "Lọc dầu động cơ",
        "currentStock": 5,
        "minimumStock": 10,
        "suggestedQuantity": 15,
        "priority": "High",
        "source": "InventoryAlert",
        "sourceEntityId": 123,
        "requiredByDate": null,
        "estimatedCost": 1500000,
        "suggestedDate": "2025-01-15T10:30:00Z"
      }
    ],
    "totalCount": 50,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 3
  },
  "message": "Demand analysis retrieved successfully"
}
```

#### **2. Get Reorder Suggestions**
```http
GET /api/procurement/reorder-suggestions?warehouseId={id}&priority={priority}&source={source}&isProcessed={bool}&pageNumber={page}&pageSize={size}
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `warehouseId` (optional): Filter by warehouse ID
- `priority` (optional): Filter by priority
- `source` (optional): Filter by source
- `isProcessed` (optional): Filter by processed status
- `pageNumber` (default: 1)
- `pageSize` (default: 20, max: 100)

**Response:**
```json
{
  "success": true,
  "data": {
    "data": [
      {
        "id": 1,
        "partId": 1,
        "partNumber": "PT001",
        "partName": "Lọc dầu động cơ",
        "currentStock": 5,
        "minimumStock": 10,
        "suggestedQuantity": 15,
        "estimatedCost": 1500000,
        "priority": "High",
        "source": "InventoryAlert",
        "sourceEntityId": 123,
        "suggestedDate": "2025-01-15T10:30:00Z",
        "requiredByDate": null,
        "isProcessed": false,
        "purchaseOrderId": null
      }
    ],
    "totalCount": 30,
    "pageNumber": 1,
    "pageSize": 20
  }
}
```

#### **3. Bulk Create Purchase Order**
```http
POST /api/procurement/bulk-create-po
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "suggestions": [
    {
      "suggestionId": 1,
      "partId": 1,
      "quantity": 15,
      "supplierId": 5,
      "unitPrice": 100000,
      "expectedDeliveryDate": "2025-01-25T00:00:00Z"
    }
  ],
  "supplierId": 5,
  "orderDate": "2025-01-15T10:30:00Z",
  "expectedDeliveryDate": "2025-01-25T00:00:00Z",
  "notes": "Đặt hàng tháng 1/2025"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 100,
    "orderNumber": "PO-2025-00100",
    "supplierId": 5,
    "supplierName": "Nhà cung cấp ABC",
    "orderDate": "2025-01-15T10:30:00Z",
    "status": "Draft",
    "totalAmount": 2500000,
    "items": [...]
  },
  "message": "Purchase order created successfully"
}
```

### **Phase 4.2.2: Supplier Evaluation**

#### **4. Get Supplier Comparison**
```http
GET /api/procurement/supplier-comparison?partId={id}&quantity={qty}
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `partId` (required): Part ID to compare
- `quantity` (default: 1): Quantity needed

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "supplierId": 5,
      "supplierName": "Nhà cung cấp ABC",
      "unitPrice": 100000,
      "minimumOrderQuantity": 10,
      "leadTimeDays": 7,
      "totalPrice": 1000000,
      "averageRating": 4.5,
      "onTimeDeliveryRate": 95.5,
      "defectRate": 1.2,
      "overallScore": 85.0,
      "isPreferred": true
    }
  ],
  "message": "Supplier comparison retrieved successfully"
}
```

#### **5. Get Supplier Recommendation**
```http
GET /api/procurement/supplier-recommendation?partId={id}&quantity={qty}
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `partId` (required): Part ID
- `quantity` (default: 1): Quantity needed

**Response:**
```json
{
  "success": true,
  "data": {
    "partId": 1,
    "recommendedSupplier": {
      "supplierId": 5,
      "supplierName": "Nhà cung cấp ABC",
      "unitPrice": 100000,
      "calculatedScore": 88.5
    },
    "recommendationReason": "Điểm số tổng thể cao, Đánh giá tốt từ người dùng",
    "recommendedAt": "2025-01-15T10:30:00Z"
  },
  "message": "Supplier recommendation retrieved successfully"
}
```

### **Phase 4.2.3: PO Tracking**

#### **6. Get In-Transit Orders**
```http
GET /api/purchase-orders/in-transit?supplierId={id}&deliveryStatus={status}&daysUntilDelivery={days}&pageNumber={page}&pageSize={size}
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `supplierId` (optional): Filter by supplier ID
- `deliveryStatus` (optional): Filter by status ("OnTime", "AtRisk", "Delayed")
- `daysUntilDelivery` (optional): Filter by days until delivery
- `pageNumber` (default: 1)
- `pageSize` (default: 20, max: 100)

**Response:**
```json
{
  "success": true,
  "data": {
    "data": [
      {
        "id": 100,
        "orderNumber": "PO-2025-00100",
        "supplierName": "Nhà cung cấp ABC",
        "expectedDeliveryDate": "2025-01-20T00:00:00Z",
        "trackingNumber": "VN123456789",
        "deliveryStatus": "OnTime",
        "daysUntilDelivery": 5
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 20
  }
}
```

#### **7. Get Tracking Info**
```http
GET /api/purchase-orders/{id}/tracking
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "purchaseOrderId": 100,
    "orderNumber": "PO-2025-00100",
    "trackingNumber": "VN123456789",
    "expectedDeliveryDate": "2025-01-20T00:00:00Z",
    "deliveryStatus": "OnTime",
    "statusHistory": [...]
  }
}
```

#### **8. Update Tracking**
```http
PUT /api/purchase-orders/{id}/update-tracking
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "trackingNumber": "VN123456789",
  "shippingMethod": "Express",
  "expectedDeliveryDate": "2025-01-20T00:00:00Z",
  "inTransitDate": "2025-01-13T00:00:00Z",
  "deliveryNotes": "Đang vận chuyển"
}
```

#### **9. Mark as In-Transit**
```http
PUT /api/purchase-orders/{id}/mark-in-transit
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "trackingNumber": "VN123456789",
  "shippingMethod": "Express",
  "inTransitDate": "2025-01-13T00:00:00Z",
  "deliveryNotes": "Đã xuất kho"
}
```

#### **10. Get Delivery Alerts**
```http
GET /api/purchase-orders/delivery-alerts
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "atRiskCount": 5,
    "delayedCount": 2,
    "atRiskOrders": [...],
    "delayedOrders": [...]
  }
}
```

### **Phase 4.2.4: Performance Evaluation**

#### **11. Get Supplier Performance Report**
```http
GET /api/procurement/supplier-performance-report?supplierId={id}&partId={id}&startDate={date}&endDate={date}&pageNumber={page}&pageSize={size}
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `supplierId` (optional): Filter by supplier ID
- `partId` (optional): Filter by part ID
- `startDate` (optional): Start date
- `endDate` (optional): End date
- `pageNumber` (default: 1)
- `pageSize` (default: 20, max: 100)

**Response:**
```json
{
  "success": true,
  "data": {
    "data": [
      {
        "supplierId": 5,
        "supplierName": "Nhà cung cấp ABC",
        "totalOrders": 50,
        "onTimeDeliveryRate": 96.0,
        "averageLeadTimeDays": 7,
        "defectRate": 1.2,
        "overallScore": 88.5
      }
    ],
    "totalCount": 10,
    "pageNumber": 1,
    "pageSize": 20
  }
}
```

#### **12. Get Supplier Ranking**
```http
GET /api/procurement/supplier-ranking?sortBy={field}&topN={n}&worstPerformers={bool}
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `sortBy` (default: "OverallScore"): Sort field
- `topN` (optional): Limit to top N suppliers
- `worstPerformers` (default: false): Return worst performers

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "rank": 1,
      "supplierId": 5,
      "supplierName": "Nhà cung cấp ABC",
      "overallScore": 88.5,
      "onTimeDeliveryRate": 96.0,
      "defectRate": 1.2
    }
  ]
}
```

#### **13. Get Performance Alerts**
```http
GET /api/procurement/performance-alerts?severity={severity}
Authorization: Bearer {access_token}
```

**Query Parameters:**
- `severity` (optional): Filter by severity ("High", "Medium", "Low")

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "supplierId": 10,
      "supplierName": "Nhà cung cấp XYZ",
      "alertType": "LowOnTimeDelivery",
      "alertMessage": "Tỷ lệ giao hàng đúng hạn thấp: 65.5%",
      "severity": "Medium"
    }
  ]
}
```

#### **14. Calculate Performance**
```http
POST /api/procurement/calculate-performance
Authorization: Bearer {access_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "supplierId": 5,
  "partId": null,
  "startDate": "2024-07-15T00:00:00Z",
  "endDate": "2025-01-15T00:00:00Z",
  "forceRecalculate": false
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "calculatedCount": 10,
    "message": "Performance calculation completed successfully"
  }
}
```

---

### **Phase 4.2.2 Optional: Request Quotation**

#### **15. Request Quotation**

**Endpoint:** `POST /api/procurement/request-quotation`

**Description:** Gửi yêu cầu báo giá cho một hoặc nhiều nhà cung cấp về phụ tùng cụ thể

**Authorization:** Required

**Request Body:**
```json
{
  "partId": 123,
  "supplierIds": [45, 46, 47],
  "requestedQuantity": 50,
  "requestNotes": "Cần gấp cho dự án quan trọng",
  "requiredByDate": "2025-02-15T00:00:00Z"
}
```

**Request Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `partId` | integer | Yes | ID của phụ tùng cần báo giá |
| `supplierIds` | array[integer] | Yes | Danh sách ID nhà cung cấp (ít nhất 1) |
| `requestedQuantity` | integer | Yes | Số lượng yêu cầu (phải > 0) |
| `requestNotes` | string | No | Ghi chú yêu cầu |
| `requiredByDate` | datetime | No | Ngày cần hàng (ISO 8601) |

**Response:**
```json
{
  "success": true,
  "data": {
    "requestedCount": 3,
    "quotations": [
      {
        "id": 1001,
        "quotationNumber": "RQ-2025-00001",
        "supplierId": 45,
        "supplierName": "Nhà Cung Cấp A",
        "supplierCode": "NCC-A",
        "partId": 123,
        "partNumber": "PT-001",
        "partName": "Phụ Tùng 001",
        "status": "Requested",
        "requestedDate": "2025-01-15T10:30:00Z",
        "requestedQuantity": 50,
        "requestNotes": "Cần gấp cho dự án quan trọng",
        "requestedById": 10
      }
    ]
  },
  "message": "Quotation requests sent successfully"
}
```

**Status Codes:**
- `200 OK` - Request created successfully
- `400 Bad Request` - Invalid input (missing required fields, invalid IDs, duplicate request)
- `401 Unauthorized` - Not authenticated
- `404 Not Found` - Part or supplier not found
- `500 Internal Server Error` - Server error

**Error Examples:**

**Missing Required Fields:**
```json
{
  "success": false,
  "errorMessage": "At least one supplier is required"
}
```

**Invalid Quantity:**
```json
{
  "success": false,
  "errorMessage": "Requested quantity must be greater than 0"
}
```

**Duplicate Request:**
```json
{
  "success": false,
  "errorMessage": "All selected suppliers already have pending or requested quotations for this part"
}
```

---

#### **16. Get Quotations**

**Endpoint:** `GET /api/procurement/quotations`

**Description:** Lấy danh sách báo giá từ suppliers với pagination và filters

**Authorization:** Required

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `partId` | integer | No | Filter theo Part ID |
| `supplierId` | integer | No | Filter theo Supplier ID |
| `status` | string | No | Filter theo status (Requested, Pending, Accepted, Rejected, Expired) |
| `pageNumber` | integer | No | Số trang (default: 1) |
| `pageSize` | integer | No | Số records mỗi trang (default: 20, max: 100) |

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1001,
      "quotationNumber": "RQ-2025-00001",
      "supplierId": 45,
      "supplierName": "Nhà Cung Cấp A",
      "supplierCode": "NCC-A",
      "partId": 123,
      "partNumber": "PT-001",
      "partName": "Phụ Tùng 001",
      "quotationDate": "2025-01-15T10:30:00Z",
      "validUntil": "2025-02-15T00:00:00Z",
      "unitPrice": 50000.00,
      "minimumOrderQuantity": 10,
      "leadTimeDays": 7,
      "warrantyPeriod": "12 tháng",
      "warrantyTerms": "Bảo hành chính hãng",
      "status": "Pending",
      "requestedById": 10,
      "requestedByName": "Nguyễn Văn A",
      "requestedDate": "2025-01-15T10:30:00Z",
      "responseDate": "2025-01-16T14:20:00Z",
      "requestedQuantity": 50,
      "requestNotes": "Cần gấp",
      "responseNotes": "Có sẵn hàng, giao trong 7 ngày",
      "notes": null
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 45,
  "totalPages": 3,
  "message": "Quotations retrieved successfully"
}
```

**Status Codes:**
- `200 OK` - Success
- `401 Unauthorized` - Not authenticated
- `500 Internal Server Error` - Server error

---

#### **17. Get Quotation By ID**

**Endpoint:** `GET /api/procurement/quotations/{id}`

**Description:** Lấy chi tiết báo giá theo ID

**Authorization:** Required

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | integer | Yes | ID của quotation |

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1001,
    "quotationNumber": "RQ-2025-00001",
    "supplierId": 45,
    "supplierName": "Nhà Cung Cấp A",
    "supplierCode": "NCC-A",
    "partId": 123,
    "partNumber": "PT-001",
    "partName": "Phụ Tùng 001",
    "quotationDate": "2025-01-15T10:30:00Z",
    "validUntil": "2025-02-15T00:00:00Z",
    "unitPrice": 50000.00,
    "minimumOrderQuantity": 10,
    "leadTimeDays": 7,
    "warrantyPeriod": "12 tháng",
    "warrantyTerms": "Bảo hành chính hãng",
    "status": "Pending",
    "requestedById": 10,
    "requestedByName": "Nguyễn Văn A",
    "requestedDate": "2025-01-15T10:30:00Z",
    "responseDate": "2025-01-16T14:20:00Z",
    "requestedQuantity": 50,
    "requestNotes": "Cần gấp",
    "responseNotes": "Có sẵn hàng, giao trong 7 ngày",
    "notes": null
  },
  "message": "Quotation retrieved successfully"
}
```

**Status Codes:**
- `200 OK` - Success
- `401 Unauthorized` - Not authenticated
- `404 Not Found` - Quotation not found
- `500 Internal Server Error` - Server error

---

#### **18. Update Quotation**

**Endpoint:** `PUT /api/procurement/quotations/{id}`

**Description:** Cập nhật báo giá (supplier response). Chỉ cho phép update quotation có status = "Requested"

**Authorization:** Required

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | integer | Yes | ID của quotation |

**Request Body:**
```json
{
  "unitPrice": 50000.00,
  "minimumOrderQuantity": 10,
  "leadTimeDays": 7,
  "validUntil": "2025-02-15T00:00:00Z",
  "warrantyPeriod": "12 tháng",
  "warrantyTerms": "Bảo hành chính hãng",
  "responseNotes": "Có sẵn hàng, giao trong 7 ngày",
  "status": "Pending"
}
```

**Request Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `unitPrice` | decimal | Yes | Giá đơn vị (phải > 0) |
| `minimumOrderQuantity` | integer | Yes | Số lượng tối thiểu (phải > 0) |
| `leadTimeDays` | integer | No | Thời gian giao hàng (ngày) |
| `validUntil` | datetime | No | Ngày hết hạn báo giá (ISO 8601) |
| `warrantyPeriod` | string | No | Thời gian bảo hành |
| `warrantyTerms` | string | No | Điều khoản bảo hành |
| `responseNotes` | string | No | Ghi chú phản hồi |
| `status` | string | Yes | Phải là "Pending" |

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1001,
    "quotationNumber": "RQ-2025-00001",
    "status": "Pending",
    "unitPrice": 50000.00,
    "responseDate": "2025-01-16T14:20:00Z"
  },
  "message": "Quotation updated successfully"
}
```

**Status Codes:**
- `200 OK` - Update successful
- `400 Bad Request` - Invalid input or status validation failed
- `401 Unauthorized` - Not authenticated
- `404 Not Found` - Quotation not found
- `500 Internal Server Error` - Server error

**Error Examples:**

**Invalid Status:**
```json
{
  "success": false,
  "errorMessage": "Cannot update quotation with status 'Pending'. Only 'Requested' quotations can be updated."
}
```

---

#### **19. Accept Quotation**

**Endpoint:** `PUT /api/procurement/quotations/{id}/accept`

**Description:** Chấp nhận báo giá. Chỉ cho phép accept quotation có status = "Pending"

**Authorization:** Required

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | integer | Yes | ID của quotation |

**Request Body:**
```json
{
  "notes": "Chấp nhận báo giá này để tạo PO"
}
```

**Request Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `notes` | string | No | Ghi chú khi chấp nhận |

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1001,
    "quotationNumber": "RQ-2025-00001",
    "status": "Accepted",
    "notes": "Chấp nhận báo giá này để tạo PO"
  },
  "message": "Quotation accepted successfully"
}
```

**Status Codes:**
- `200 OK` - Accept successful
- `400 Bad Request` - Status validation failed
- `401 Unauthorized` - Not authenticated
- `404 Not Found` - Quotation not found
- `500 Internal Server Error` - Server error

**Error Examples:**

**Invalid Status:**
```json
{
  "success": false,
  "errorMessage": "Cannot accept quotation with status 'Requested'. Only 'Pending' quotations can be accepted."
}
```

---

#### **20. Reject Quotation**

**Endpoint:** `PUT /api/procurement/quotations/{id}/reject`

**Description:** Từ chối báo giá. Chỉ cho phép reject quotation có status = "Pending"

**Authorization:** Required

**Path Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | integer | Yes | ID của quotation |

**Request Body:**
```json
{
  "notes": "Giá quá cao so với thị trường"
}
```

**Request Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `notes` | string | No | Lý do từ chối |

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1001,
    "quotationNumber": "RQ-2025-00001",
    "status": "Rejected",
    "notes": "Giá quá cao so với thị trường"
  },
  "message": "Quotation rejected successfully"
}
```

**Status Codes:**
- `200 OK` - Reject successful
- `400 Bad Request` - Status validation failed
- `401 Unauthorized` - Not authenticated
- `404 Not Found` - Quotation not found
- `500 Internal Server Error` - Server error

**Error Examples:**

**Invalid Status:**
```json
{
  "success": false,
  "errorMessage": "Cannot reject quotation with status 'Requested'. Only 'Pending' quotations can be rejected."
}
```

---

## ⚠️ ERROR HANDLING

### **HTTP Status Codes**
- `200 OK`: Success
- `201 Created`: Resource created
- `400 Bad Request`: Invalid request
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `409 Conflict`: Conflict (duplicate data)
- `422 Unprocessable Entity`: Validation error
- `500 Internal Server Error`: Server error

### **Common Error Codes**
```
VALIDATION_ERROR        - Input validation failed
AUTHENTICATION_FAILED   - Invalid credentials
AUTHORIZATION_FAILED    - Insufficient permissions
RESOURCE_NOT_FOUND      - Requested resource not found
DUPLICATE_ENTRY         - Resource already exists
BUSINESS_RULE_VIOLATION - Business logic violation
INSUFFICIENT_STOCK      - Not enough inventory
INVALID_INVOICE         - Cannot use part without invoice for company/insurance
DATABASE_ERROR          - Database operation failed
EXTERNAL_SERVICE_ERROR  - External service call failed
```

### **Error Response Examples**

**Validation Error:**
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      "License plate is required",
      "VIN must be 17 characters"
    ]
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Business Rule Violation:**
```json
{
  "success": false,
  "error": {
    "code": "INVALID_INVOICE",
    "message": "Cannot use this part for company vehicle",
    "details": [
      "Part 'Đèn pha tháo xe cũ' does not have invoice",
      "Company vehicles require parts with VAT invoice"
    ]
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Insufficient Stock:**
```json
{
  "success": false,
  "error": {
    "code": "INSUFFICIENT_STOCK",
    "message": "Not enough stock for this operation",
    "details": [
      "Part: Đèn pha MB C-Class",
      "Requested: 5",
      "Available: 3"
    ]
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

## 📝 NOTES

### **Rate Limiting**
- 1000 requests per hour per user
- Burst limit: 100 requests per minute

### **Pagination**
- Default page size: 20
- Maximum page size: 100

### **Date Formats**
- ISO 8601: `2024-01-15T10:30:00Z`
- All dates are in UTC

### **Currency**
- All amounts in VNĐ (Vietnamese Dong)
- Decimal(15,2) format

---

*API Documentation Version: 2.0.0*
*Tài liệu API - Version 3.0.0*  
*Last Updated: 2024-10-22*

