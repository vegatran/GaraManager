# API DOCUMENTATION - GARAGE MANAGEMENT SYSTEM

## 📋 MỤC LỤC
1. [Overview](#overview)
2. [Authentication](#authentication)
3. [Vehicle Management APIs](#vehicle-management-apis)
4. [Parts & Inventory APIs](#parts--inventory-apis)
5. [Service Order APIs](#service-order-apis)
6. [Financial Management APIs](#financial-management-apis)
7. [Reporting APIs](#reporting-apis)
8. [Error Handling](#error-handling)

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
*Last Updated: 2024-01-15*

