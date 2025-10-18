# 🎯 PRICING MODELS IMPLEMENTATION

## 📋 **TỔNG QUAN**

Hệ thống đã được implement **3 mô hình tính giá** khác nhau để phù hợp với từng loại dịch vụ trong ngành sửa chữa ô tô:

### **🔧 1. COMBINED MODEL (Gộp chung)**
- **Áp dụng:** Sửa chữa, Thay thế phụ tùng
- **Cách tính:** Giá dịch vụ đã bao gồm vật liệu + công lao động
- **VAT:** 10% trên tổng giá
- **Ví dụ:** "Sửa phanh" = 1,500,000 VNĐ (đã bao công)

### **🎨 2. SEPARATED MODEL (Tách riêng)**
- **Áp dụng:** Sơn xe, Dịch vụ có vật liệu riêng biệt
- **Cách tính:** Vật liệu + Công lao động riêng biệt
- **VAT:** 10% trên tổng (vật liệu + công)
- **Ví dụ:** "Sơn xe" = 1,200,000 VNĐ (sơn) + 800,000 VNĐ (công) = 2,000,000 VNĐ

### **⚙️ 3. LABOR_ONLY MODEL (Chỉ công)**
- **Áp dụng:** Công lao động thuần túy (không bán vật liệu)
- **Cách tính:** Chỉ tính công lao động
- **VAT:** 0% (theo Thông tư 219/2013/TT-BTC)
- **Ví dụ:** "Đập nắn thân xe" = 300,000 VNĐ (chỉ công)

---

## 🏗️ **KIẾN TRÚC HỆ THỐNG**

### **📊 Database Schema Updates**

#### **Service Entity (Updated)**
```sql
ALTER TABLE Services ADD COLUMN PricingModel VARCHAR(20) DEFAULT 'Combined';
ALTER TABLE Services ADD COLUMN MaterialCost DECIMAL(15,2) DEFAULT 0;
ALTER TABLE Services ADD COLUMN IsVATApplicable BOOLEAN DEFAULT TRUE;
ALTER TABLE Services ADD COLUMN VATRate INT DEFAULT 10;
ALTER TABLE Services ADD COLUMN PricingNotes VARCHAR(100);
```

#### **QuotationItem Entity (Updated)**
```sql
ALTER TABLE QuotationItems ADD COLUMN PricingModel VARCHAR(20) DEFAULT 'Combined';
ALTER TABLE QuotationItems ADD COLUMN MaterialCost DECIMAL(15,2) DEFAULT 0;
ALTER TABLE QuotationItems ADD COLUMN LaborCost DECIMAL(15,2) DEFAULT 0;
ALTER TABLE QuotationItems ADD COLUMN IsVATApplicable BOOLEAN DEFAULT TRUE;
ALTER TABLE QuotationItems ADD COLUMN PricingBreakdown TEXT;
```

### **🔧 Core Services**

#### **PricingService.cs**
```csharp
public static PricingResult CalculateServicePrice(Service service, int quantity = 1)
{
    switch (service.PricingModel)
    {
        case "Combined":    // Gộp chung
        case "Separated":   // Tách riêng  
        case "LaborOnly":   // Chỉ công
    }
}
```

---

## 📈 **CÁCH SỬ DỤNG**

### **🔍 API Endpoints**

#### **Test Pricing Calculation**
```http
GET /api/ServiceQuotations/pricing-demo/{serviceId}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "serviceId": 1,
    "serviceName": "Sơn xe",
    "pricingModel": "Separated",
    "unitPrice": 2000000,
    "materialCost": 1200000,
    "laborCost": 800000,
    "vatRate": 10,
    "vatAmount": 200000,
    "totalAmount": 2200000,
    "isVATApplicable": true,
    "description": "Tách riêng vật liệu và công lao động"
  }
}
```

### **💻 Frontend Integration**

Khi tạo báo giá, hệ thống sẽ:
1. **Tự động detect** pricing model từ Service
2. **Tính toán giá** theo model tương ứng
3. **Apply VAT** theo quy định
4. **Lưu breakdown** chi tiết vào database

---

## 🎯 **VÍ DỤ THỰC TẾ**

### **📋 Demo Data**

```sql
-- Sửa chữa phanh (Combined)
INSERT INTO Services (Name, PricingModel, Price, MaterialCost, LaborRate, IsVATApplicable, VATRate) 
VALUES ('Sửa chữa phanh', 'Combined', 1500000, 0, 0, 1, 10);

-- Sơn xe (Separated)  
INSERT INTO Services (Name, PricingModel, Price, MaterialCost, LaborRate, LaborHours, IsVATApplicable, VATRate)
VALUES ('Sơn xe', 'Separated', 2000000, 1200000, 200000, 4, 1, 10);

-- Đập nắn (LaborOnly)
INSERT INTO Services (Name, PricingModel, Price, MaterialCost, LaborRate, LaborHours, IsVATApplicable, VATRate)
VALUES ('Đập nắn thân xe', 'LaborOnly', 300000, 0, 150000, 2, 0, 0);
```

### **💰 Kết Quả Tính Giá**

| Dịch Vụ | Model | Vật Liệu | Công | VAT | Tổng |
|---------|-------|----------|------|-----|------|
| Sửa phanh | Combined | 1,500,000 | 0 | 150,000 | 1,650,000 |
| Sơn xe | Separated | 1,200,000 | 800,000 | 200,000 | 2,200,000 |
| Đập nắn | LaborOnly | 0 | 300,000 | 0 | 300,000 |

---

## ✅ **BENEFITS**

### **🎯 Cho Khách Hàng**
- **Minh bạch:** Biết rõ vật liệu và công
- **Linh hoạt:** Có thể chọn chỉ công hoặc trọn gói
- **Tuân thủ:** Đúng quy định VAT

### **🏢 Cho Gara**
- **Linh hoạt:** Nhiều cách tính giá
- **Tuân thủ:** Đúng luật thuế
- **Chuyên nghiệp:** Hệ thống tính giá chuẩn
- **Tự động:** Không cần tính thủ công

### **💼 Cho Kế Toán**
- **Rõ ràng:** Tách biệt vật liệu và công
- **Tuân thủ:** Đúng quy định VAT
- **Audit:** Dễ kiểm tra và đối chiếu

---

## 🚀 **NEXT STEPS**

1. **✅ COMPLETED:** Core pricing models implementation
2. **✅ COMPLETED:** Database schema updates  
3. **✅ COMPLETED:** API endpoints for testing
4. **🔄 PENDING:** Frontend UI updates
5. **🔄 PENDING:** Migration và demo data
6. **🔄 PENDING:** Testing và validation

---

## 📞 **SUPPORT**

Nếu có vấn đề về pricing models, vui lòng:
1. Kiểm tra `PricingService.cs` logic
2. Test API endpoint `/pricing-demo/{serviceId}`
3. Review database migration
4. Check VAT compliance theo Thông tư 219/2013/TT-BTC
