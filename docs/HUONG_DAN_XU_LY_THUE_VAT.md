# 📋 HƯỚNG DẪN XỬ LÝ THUẾ VAT TRONG HỆ THỐNG

## 🎯 **TỔNG QUAN VẤN ĐỀ**

### **❌ Vấn đề hiện tại:**
- Mỗi phụ tùng chỉ có **1 VATRate duy nhất**
- Không phân biệt được nguồn gốc từ nhà cung cấp nào
- Xuất hàng không đúng với thuế thực tế đã nhập

### **✅ Giải pháp đã implement:**
- **VAT Override** trong QuotationItem
- **Thông tin VAT** chi tiết trong Part entity
- **Logic tính VAT** linh hoạt theo từng trường hợp

---

## 🏗️ **KIẾN TRÚC HỆ THỐNG**

### **1. 📦 Part Entity - Thông tin VAT cơ bản**
```csharp
public class Part : BaseEntity
{
    // Thông tin thuế VAT
    public decimal VATRate { get; set; } = 10; // Tỷ lệ thuế VAT mặc định (%)
    public bool IsVATApplicable { get; set; } = true; // Có áp dụng thuế VAT không
    
    // Thông tin hóa đơn
    public string InvoiceType { get; set; } = "WithInvoice"; // WithInvoice, WithoutInvoice, Internal
    public bool HasInvoice { get; set; } = true; // Có hóa đơn hay không
}
```

### **2. 📋 QuotationItem Entity - VAT Override**
```csharp
public class QuotationItem : BaseEntity
{
    public decimal VATRate { get; set; } = 0.10m; // VAT rate mặc định
    
    // ✅ VAT Override từ Part
    public decimal? OverrideVATRate { get; set; } // Ghi đè VAT từ Part
    public bool? OverrideIsVATApplicable { get; set; } // Ghi đè áp dụng VAT từ Part
    
    // ✅ Methods để tính VAT hiệu quả
    public decimal GetEffectiveVATRate()
    {
        return OverrideVATRate ?? VATRate;
    }
    
    public bool GetEffectiveIsVATApplicable()
    {
        return OverrideIsVATApplicable ?? IsVATApplicable;
    }
    
    public decimal CalculateVATAmount()
    {
        var effectiveVATRate = GetEffectiveVATRate();
        var effectiveIsVATApplicable = GetEffectiveIsVATApplicable();
        
        if (!effectiveIsVATApplicable) return 0;
        
        return SubTotal * effectiveVATRate;
    }
}
```

---

## 🔄 **QUY TRÌNH XỬ LÝ VAT**

### **Bước 1: 📥 Nhập hàng từ nhà cung cấp**
```
1. Tạo Purchase Order với VATRate tương ứng
2. Lưu thông tin VAT vào Part entity
3. Cập nhật tồn kho với thông tin VAT
```

### **Bước 2: 📤 Xuất hàng cho khách hàng**
```
1. Tạo Quotation với phụ tùng
2. Hệ thống tự động lấy VATRate từ Part (READ-ONLY)
3. ❌ KHÔNG ĐƯỢC PHÉP Override VAT (tuân thủ quy định thuế)
4. Tính toán VATAmount chính xác theo VAT đã nhập
```

### **Bước 3: 🧮 Tính toán VAT**
```
Công thức:
- SubTotal = Quantity × UnitPrice
- VATAmount = SubTotal × EffectiveVATRate (nếu IsVATApplicable = true)
- TotalAmount = SubTotal + VATAmount - DiscountAmount
```

---

## 💡 **CÁC TRƯỜNG HỢP SỬ DỤNG**

### **Trường hợp 1: 🏢 Khách hàng cá nhân**
```
- Sử dụng VATRate từ Part (READ-ONLY)
- ❌ KHÔNG ĐƯỢC PHÉP Override VAT
- Áp dụng thuế theo VAT đã nhập từ nhà cung cấp
```

### **Trường hợp 2: 🏥 Khách hàng bảo hiểm**
```
- Sử dụng VATRate từ Part (READ-ONLY)
- ❌ KHÔNG ĐƯỢC PHÉP Override VAT
- Cần có hóa đơn VAT hợp lệ theo VAT đã nhập
```

### **Trường hợp 3: 🏭 Khách hàng công ty**
```
- Sử dụng VATRate từ Part (READ-ONLY)
- ❌ KHÔNG ĐƯỢC PHÉP Override VAT
- Cần có hóa đơn VAT đầy đủ theo VAT đã nhập
```

### **Trường hợp 4: 🔧 Phụ tùng không có hóa đơn**
```
- Set IsVATApplicable = false
- VATAmount = 0
- Không áp dụng thuế VAT
```

---

## 🎛️ **CÁCH SỬ DỤNG TRONG GIAO DIỆN**

### **1. 📝 Tạo/Cập nhật phụ tùng**
```
Trong modal "Tạo Phụ Tùng" hoặc "Cập Nhật Phụ Tùng":
- Thuế VAT (%): Dropdown 0%, 8%, 10%
- Áp dụng thuế VAT: Checkbox
- Loại hóa đơn: Dropdown (Có hóa đơn VAT, Không hóa đơn, Nội bộ)
```

### **2. 📋 Tạo báo giá**
```
Trong modal "Tạo Báo Giá":
- Hệ thống tự động lấy VATRate từ Part
- Cho phép Override VAT cho từng item
- Tính toán tự động VATAmount và TotalAmount
```

### **3. 🔍 Xem chi tiết báo giá**
```
Trong modal "Chi Tiết Báo Giá":
- Hiển thị VATRate gốc từ Part
- Hiển thị VATRate hiệu quả (nếu có Override)
- Hiển thị VATAmount và TotalAmount
```

---

## 📊 **VÍ DỤ THỰC TẾ**

### **Ví dụ 1: Phụ tùng có VAT chuẩn**
```
Phụ tùng: Lọc gió động cơ
- Giá bán: 180,000 VNĐ
- VATRate: 10%
- IsVATApplicable: true

Tính toán:
- SubTotal: 1 × 180,000 = 180,000 VNĐ
- VATAmount: 180,000 × 10% = 18,000 VNĐ
- TotalAmount: 180,000 + 18,000 = 198,000 VNĐ
```

### **Ví dụ 2: Phụ tùng không có hóa đơn**
```
Phụ tùng: Phụ tùng tháo từ xe cũ
- Giá bán: 150,000 VNĐ
- VATRate: 10%
- IsVATApplicable: false

Tính toán:
- SubTotal: 1 × 150,000 = 150,000 VNĐ
- VATAmount: 0 VNĐ (không áp dụng VAT)
- TotalAmount: 150,000 + 0 = 150,000 VNĐ
```

### **Ví dụ 3: Override VAT cho bảo hiểm**
```
Phụ tùng: Lọc gió động cơ
- Giá bán: 180,000 VNĐ
- VATRate gốc: 10%
- OverrideVATRate: 8% (cho bảo hiểm)
- IsVATApplicable: true

Tính toán:
- SubTotal: 1 × 180,000 = 180,000 VNĐ
- VATAmount: 180,000 × 8% = 14,400 VNĐ
- TotalAmount: 180,000 + 14,400 = 194,400 VNĐ
```

---

## 🔧 **CẤU HÌNH HỆ THỐNG**

### **1. 📋 Các tỷ lệ VAT được hỗ trợ**
```
- 0%: Không thuế (hàng xuất khẩu, dịch vụ không chịu thuế)
- 8%: Thuế VAT giảm (một số mặt hàng thiết yếu)
- 10%: Thuế VAT chuẩn (hầu hết hàng hóa, dịch vụ)
```

### **2. 🎯 Logic mặc định**
```
- VATRate mặc định: 10%
- IsVATApplicable mặc định: true
- InvoiceType mặc định: "WithInvoice"
- HasInvoice mặc định: true
```

### **3. 🔒 Read-Only Logic (TUÂN THỦ QUY ĐỊNH THUẾ)**
```
- VATRate từ Part entity là READ-ONLY
- KHÔNG ĐƯỢC PHÉP Override VAT khi xuất hàng
- Đảm bảo tính minh bạch và tuân thủ quy định thuế
- VAT phải khớp với hóa đơn nhập hàng
```

---

## 🚀 **TÍNH NĂNG NÂNG CAO**

### **1. 🏷️ Batch Management (Tương lai)**
```
- Quản lý từng lô hàng với VAT riêng biệt
- FIFO (First In, First Out) để xuất đúng lô
- Theo dõi VAT theo nguồn gốc nhà cung cấp
```

### **2. 🎛️ Smart VAT Selection**
```
- Tự động chọn VAT phù hợp theo loại khách hàng
- Ưu tiên lô có VAT thấp hơn cho bảo hiểm
- Ưu tiên lô có VAT chuẩn cho công ty
```

### **3. 📊 Advanced Reporting**
```
- Báo cáo VAT theo nhà cung cấp
- Báo cáo VAT theo loại khách hàng
- Báo cáo VAT theo thời gian
```

---

## ⚠️ **LƯU Ý QUAN TRỌNG**

### **1. 🔒 Tuân thủ quy định thuế (NGUYÊN TẮC BẮT BUỘC)**
```
- ❌ KHÔNG ĐƯỢC PHÉP chỉnh sửa VAT khi xuất hàng
- ✅ VAT phải khớp với hóa đơn nhập hàng từ nhà cung cấp
- ✅ Đảm bảo tính minh bạch và tuân thủ quy định thuế
- ✅ Lưu trữ đầy đủ chứng từ thuế để kiểm tra
- ✅ Luôn kiểm tra quy định thuế hiện hành
```

---

## 🎯 **TỔNG HỢP IMPLEMENTATION VAT READ-ONLY**

### **📋 Các thay đổi đã thực hiện:**

#### **1. 🗄️ Database & Entities**
- ✅ **Part Entity**: Thêm `VATRate` và `IsVATApplicable`
- ✅ **QuotationItem Entity**: Thêm `OverrideVATRate` và `OverrideIsVATApplicable` (READ-ONLY)
- ✅ **PurchaseOrder Entity**: Thêm `VATRate`
- ✅ **Migration**: `20251022102808_AddVATFieldsToPartAndQuotationItem.cs`

#### **2. 🔄 DTOs & Mapping**
- ✅ **PartDto**: Thêm `VATRate` và `IsVATApplicable`
- ✅ **QuotationItemDto**: Thêm `OverrideVATRate`, `OverrideIsVATApplicable`, `PartVATRate`, `PartIsVATApplicable`
- ✅ **PurchaseOrderDto**: Thêm `VATRate`
- ✅ **AutoMapper**: Cập nhật mapping profiles

#### **3. 🎨 UI/UX Changes**
- ✅ **Create Quotation Modal**: Thêm asterisk (*) cho VAT header
- ✅ **Edit Quotation Modal**: Thêm asterisk (*) cho VAT header
- ✅ **CSS Styling**: READ-ONLY VAT input styling với tooltip
- ✅ **Parts Management**: Thêm VAT fields trong create/edit modals

#### **4. 💻 JavaScript Logic**
- ✅ **searchPartsWithVAT**: Load VAT từ Part entity
- ✅ **initializeServiceTypeahead**: Phân biệt Parts vs Services
- ✅ **VAT READ-ONLY Logic**: Disable VAT input cho phụ tùng từ kho
- ✅ **Tooltip System**: Hiển thị thông báo READ-ONLY
- ✅ **Event Handlers**: Xử lý checkbox "Có hóa đơn" với VAT logic

#### **5. 🔧 API & Controllers**
- ✅ **PartsController**: Xử lý VAT fields trong CRUD operations
- ✅ **ServiceQuotationsController**: Map VAT fields từ entities
- ✅ **QuotationManagementController**: Truyền VAT data đến client
- ✅ **PartsManagementController**: Include VAT fields trong responses

### **🎯 Kết quả đạt được:**

#### **✅ Tuân thủ nguyên tắc thuế:**
- VAT từ phụ tùng nhập kho KHÔNG ĐƯỢC PHÉP chỉnh sửa khi xuất hàng
- Đảm bảo tính minh bạch và tuân thủ quy định thuế
- Lưu trữ đầy đủ thông tin VAT từ hóa đơn nhập hàng

#### **✅ UI/UX cải thiện:**
- VAT input fields có styling READ-ONLY rõ ràng
- Tooltip hiển thị thông báo "Không được chỉnh sửa"
- Asterisk (*) trong header để chỉ ra VAT có thể READ-ONLY
- Phân biệt rõ ràng giữa Services (có thể chỉnh sửa VAT) và Parts (READ-ONLY VAT)

#### **✅ Logic nghiệp vụ chính xác:**
- Tự động load VAT từ Part entity khi search phụ tùng
- Disable VAT input khi là phụ tùng từ kho
- Cho phép chỉnh sửa VAT cho Services (dịch vụ)
- Tính toán VAT chính xác theo thông tin từ Part

### **🔍 Cách hoạt động:**

1. **Khi tạo báo giá mới:**
   - Tab "Phụ Tùng": VAT tự động load từ Part entity (READ-ONLY)
   - Tab "Sửa Chữa/Paint": VAT có thể chỉnh sửa tự do

2. **Khi chỉnh sửa báo giá:**
   - Phụ tùng từ kho: VAT READ-ONLY với tooltip
   - Dịch vụ: VAT có thể chỉnh sửa bình thường

3. **Khi nhập hàng:**
   - Purchase Order có VAT rate tương ứng
   - Part entity lưu VAT information từ hóa đơn nhập

4. **Khi xuất hàng:**
   - VAT phải khớp với VAT đã nhập (tuân thủ thuế)
   - Không được phép override VAT từ phụ tùng

---

## 📋 **QUY ĐỊNH VAT Ở VIỆT NAM**

### **⚖️ Thuế suất VAT cho các hạng mục:**

```
┌─────────────────────────────────────────────────────────┐
│         QUY ĐỊNH VAT THEO LUẬT VIỆT NAM                │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ ✅ HÀNG HÓA (Chịu thuế VAT 10%):                      │
│    • Phụ tùng ô tô                     → VAT 10%       │
│    • Vật liệu sơn                      → VAT 10%       │
│    • Vật liệu sửa chữa (matit, keo...) → VAT 10%       │
│    • Dầu nhớt, mỡ bôi trơn             → VAT 10%       │
│                                                         │
│ ✅ DỊCH VỤ SỬA CHỮA (Chịu thuế VAT 10%):              │
│    • Dịch vụ thay phụ tùng             → VAT 10%       │
│    • Dịch vụ sửa chữa                  → VAT 10%       │
│    • Dịch vụ bảo dưỡng                 → VAT 10%       │
│    • Dịch vụ sơn                       → VAT 10%       │
│    • Dịch vụ kiểm tra, chẩn đoán       → VAT 10%       │
│                                                         │
│ ⚠️ CÔNG LAO ĐỘNG (Tùy cách hạch toán):                │
│                                                         │
│ CÁCH 1 - GỘP VÀO GIÁ DỊCH VỤ (Phổ biến):              │
│    • Dịch vụ đã bao gồm công           → VAT 10%       │
│    • Giá dịch vụ = Vật liệu + Công                     │
│    • Ví dụ: "Thay đèn pha" = PT + Công → VAT 10%       │
│                                                         │
│ CÁCH 2 - TÁCH RIÊNG CÔNG (Ít dùng):                   │
│    • Công lao động tách riêng          → KHÔNG VAT     │
│    • Chỉ ghi giá công, không tính VAT                  │
│    • Lưu ý: Phải ghi rõ "Công lao động"               │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

---

## 🧾 **MẪU HÓA ĐƠN ĐÚNG QUY ĐỊNH**

### **CÁCH 1: Gộp công vào giá dịch vụ (KHUYẾN NGHỊ)**

```
┌─────────────────────────────────────────────────────────────────┐
│              HÓA ĐƠN GIÁ TRỊ GIA TĂNG                           │
├─────────────────────────────────────────────────────────────────┤
│ Số: 0001250                Ký hiệu: AA/24E                     │
│ Ngày 18 tháng 01 năm 2024                                      │
│                                                                 │
│ Đơn vị bán: GARAGE Ô TÔ ABC | MST: 0123456789                 │
│ Đơn vị mua: CÔNG TY BẢO HIỂM BẢO VIỆT | MST: 0100107929       │
│                                                                 │
│ Claim: CLM-2024-005 | Xe: 30A-12345 - Mercedes C-Class 2020    │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│ STT │ Tên hàng hóa, dịch vụ           │ĐVT │SL │Đơn giá│Thành tiền│
├─────┼─────────────────────────────────┼────┼───┼───────┼──────────┤
│                                                                 │
│     I. PHỤ TÙNG (VAT 10%)                                      │
│                                                                 │
│  1  │ Cản trước MB C-Class W205 OEM   │Cái │ 1 │ 2,400 │  2,400   │
│  2  │ Đèn pha phải MB C-Class W205 LED│Cái │ 1 │ 3,200 │  3,200   │
│  3  │ Gương chiếu hậu phải điện       │Cái │ 1 │ 1,100 │  1,100   │
│                                                                 │
│     II. VẬT LIỆU SƠN (VAT 10%)                                │
│                                                                 │
│  4  │ Sơn màu đen Mercedes Code 040   │Lít │ 2 │   350 │    700   │
│  5  │ Sơn lót epoxy + Sơn bóng + Phụ  │Bộ  │ 1 │ 1,200 │  1,200   │
│                                                                 │
│     III. DỊCH VỤ SỬA CHỮA (VAT 10% - BAO GỒM CÔNG)           │
│                                                                 │
│  6  │ Thay cản trước (bao gồm công    │Lần │ 1 │   375 │    375   │
│     │ tháo lắp 1.5h x 50k)            │    │   │       │          │
│                                                                 │
│  7  │ Thay đèn pha (bao gồm công tháo │Lần │ 1 │   275 │    275   │
│     │ lắp + điều chỉnh 1.5h x 50k)    │    │   │       │          │
│                                                                 │
│  8  │ Thay gương (bao gồm công tháo   │Lần │ 1 │   140 │    140   │
│     │ lắp 0.8h x 50k)                 │    │   │       │          │
│                                                                 │
│  9  │ Sửa chữa thân xe - Đập nắn capô │Lần │ 1 │   560 │    560   │
│     │ (bao gồm công 3h x 60k + VL)    │    │   │       │          │
│                                                                 │
│ 10  │ Sơn 3 chi tiết (Cản, Capô, Cửa) │Lần │ 1 │ 3,580 │  3,580   │
│     │ (bao gồm công sơn 10.5h x 80k)  │    │   │       │          │
│                                                                 │
├─────────────────────────────────────────────────────────────────┤
│ Cộng tiền hàng (chịu thuế VAT):                  13,530 (nghìn)│
│ Thuế suất GTGT: 10%                               1,353 (nghìn)│
├─────────────────────────────────────────────────────────────────┤
│ TỔNG TIỀN THANH TOÁN:                            14,883 (nghìn)│
│                                                                 │
│ Số tiền bằng chữ:                                              │
│ Mười bốn triệu tám trăm tám mươi ba nghìn đồng chẵn           │
│                                                                 │
│ Ghi chú:                                                       │
│ • Claim CLM-2024-005 - Công ty BH Bảo Việt                    │
│ • Giá dịch vụ đã bao gồm công lao động                        │
│ • Tất cả phụ tùng có HĐ VAT đầu vào                           │
└─────────────────────────────────────────────────────────────────┘

💡 CÁCH NÀY: Công đã gộp vào giá dịch vụ → Tính VAT 10% trên tổng
```

---

## 🎯 **SO SÁNH 2 CÁCH HẠCH TOÁN**

### **CÁCH 1: Gộp công vào dịch vụ (KHUYẾN NGHỊ)**

**Ưu điểm:**
- ✅ Đơn giản, dễ hiểu
- ✅ Khách hàng thấy giá trọn gói
- ✅ Không cần tách công riêng
- ✅ Phù hợp với garage nhỏ/vừa

**Nhược điểm:**
- ⚠️ Không thấy rõ chi phí công
- ⚠️ Khó quản lý hiệu suất thợ

**Ví dụ:**
```
Dịch vụ thay đèn pha:
• Phụ tùng: 3,200,000 VNĐ
• Công tháo lắp: 75,000 VNĐ (1.5h x 50k)
• Giá dịch vụ: 200,000 VNĐ (đã bao gồm công)
─────────────────────────
TỔNG: 3,475,000 VNĐ
VAT 10%: 347,500 VNĐ
THÀNH TIỀN: 3,822,500 VNĐ
```

---

## 📋 **THIẾT KẾ HỆ THỐNG QUẢN LÝ LÔ HÀNG VÀ THUẾ VAT**

### **🎯 VẤN ĐỀ CẦN GIẢI QUYẾT**

**Tình huống thực tế:**
- **Nhà cung cấp A**: Bán phụ tùng X với VAT 10%
- **Nhà cung cấp B**: Bán phụ tùng X với VAT 8%
- **Khi xuất**: Phải xuất đúng lô với VAT tương ứng

**Vấn đề hiện tại:**
- Part entity chỉ có 1 VATRate duy nhất
- Không phân biệt được nguồn gốc từ nhà cung cấp nào
- Xuất hàng không đúng với thuế thực tế

### **💡 GIẢI PHÁP ĐÃ IMPLEMENT**

#### **PHƯƠNG PHÁP: 🎛️ VAT Override trong QuotationItem**

```csharp
public class QuotationItem : BaseEntity
{
    // ... existing fields ...
    
    public decimal? OverrideVATRate { get; set; } // ✅ THÊM: Ghi đè VAT
    public bool? OverrideIsVATApplicable { get; set; } // ✅ THÊM: Ghi đè áp dụng VAT
    
    // Logic tính VAT
    public decimal GetEffectiveVATRate()
    {
        return OverrideVATRate ?? Part.VATRate;
    }
    
    public bool GetEffectiveIsVATApplicable()
    {
        return OverrideIsVATApplicable ?? Part.IsVATApplicable;
    }
}
```

#### **UI cho phép override VAT**
```html
<!-- Trong Quotation Item -->
<div class="col-md-3">
    <div class="form-group">
        <label>Thuế VAT (%)</label>
        <select class="form-control form-control-sm vat-rate-override">
            <option value="">Dùng mặc định</option>
            <option value="0">Không VAT (0%)</option>
            <option value="8">VAT 8%</option>
            <option value="10">VAT 10%</option>
        </select>
    </div>
</div>
```

### **📊 VÍ DỤ THỰC TẾ**

**Tình huống:**
- **Phụ tùng**: Lọc gió động cơ
- **Nhà cung cấp A**: 100 cái, VAT 10%, ngày 01/01/2024
- **Nhà cung cấp B**: 50 cái, VAT 8%, ngày 15/01/2024
- **Xuất**: 120 cái cho khách hàng cá nhân

**Logic FIFO:**
1. **Lô A**: Xuất 100 cái với VAT 10%
2. **Lô B**: Xuất 20 cái với VAT 8%
3. **Tổng**: 120 cái với VAT hỗn hợp

**Kết quả:**
- **Tạm tính**: 120 × 180,000 = 21,600,000 VNĐ
- **VAT lô A**: 100 × 180,000 × 10% = 1,800,000 VNĐ
- **VAT lô B**: 20 × 180,000 × 8% = 288,000 VNĐ
- **Tổng VAT**: 2,088,000 VNĐ
- **Tổng cộng**: 23,688,000 VNĐ

---

## 📝 **KHUYẾN NGHỊ CHO HỆ THỐNG**

### **Thiết lập mặc định:**

```javascript
// Cấu hình VAT trong hệ thống
const VAT_CONFIG = {
  parts: {
    taxable: true,
    taxRate: 10,
    description: "Phụ tùng ô tô"
  },
  materials: {
    taxable: true,
    taxRate: 10,
    description: "Vật liệu sơn, sửa chữa"
  },
  services: {
    taxable: true,
    taxRate: 10,
    description: "Dịch vụ sửa chữa (đã gồm công)",
    includesLabor: true  // ⭐ Quan trọng
  },
  labor: {
    taxable: false,  // ⚠️ Nếu tách riêng
    taxRate: 0,
    description: "Công lao động thuần túy",
    note: "Chỉ dùng khi TÁCH RIÊNG công"
  }
};
```

### **Logic xuất hóa đơn:**

```javascript
// Pseudo code
function generateInvoice(serviceOrder) {
  let taxableAmount = 0;
  let nonTaxableAmount = 0;
  let items = [];
  
  // 1. Phụ tùng (VAT 10%)
  serviceOrder.parts.forEach(part => {
    items.push({
      name: part.name,
      quantity: part.quantity,
      unitPrice: part.sellPrice,
      totalPrice: part.quantity * part.sellPrice,
      taxRate: 10,
      taxAmount: (part.quantity * part.sellPrice) * 0.1,
      category: "Part"
    });
    taxableAmount += part.quantity * part.sellPrice;
  });
  
  // 2. Dịch vụ (VAT 10% - ĐÃ GỒM CÔNG)
  serviceOrder.services.forEach(service => {
    // Tính tổng giá dịch vụ + công
    let serviceTotal = service.price;
    
    // Cộng thêm công (nếu có công riêng trong ServiceOrderLabors)
    let laborTotal = serviceOrder.labors
      .filter(l => l.serviceId === service.id)
      .reduce((sum, l) => sum + l.totalCost, 0);
    
    let totalServicePrice = serviceTotal + laborTotal;
    
    items.push({
      name: service.name + " (bao gồm công lao động)",
      quantity: service.quantity,
      unitPrice: totalServicePrice,
      totalPrice: totalServicePrice,
      taxRate: 10,
      taxAmount: totalServicePrice * 0.1,
      category: "Service"
    });
    taxableAmount += totalServicePrice;
  });
  
  // Tính VAT
  let vatAmount = taxableAmount * 0.1;
  let totalAmount = taxableAmount + vatAmount + nonTaxableAmount;
  
  return {
    items: items,
    subTotal: taxableAmount,
    vatAmount: vatAmount,
    laborAmount: 0,  // Đã gộp vào dịch vụ
    totalAmount: totalAmount
  };
}
```

---

## 📋 **CHECKLIST XUẤT HÓA ĐƠN**

### **Trước khi xuất:**

```
☑️ 1. Xác định cách hạch toán:
   (•) CÁCH 1: Gộp công vào dịch vụ (Khuyến nghị)
       → Tất cả chịu VAT 10%
   
   ( ) CÁCH 2: Tách riêng công
       → Hàng & DV: VAT 10%
       → Công: KHÔNG VAT

☑️ 2. Kiểm tra hóa đơn đầu vào:
   • Phụ tùng có HĐ đầu vào?
   • Vật liệu có HĐ đầu vào?
   • Ghi rõ số HĐ để đối chiếu

☑️ 3. Tính toán chính xác:
   • Tổng chịu thuế
   • VAT 10%
   • Tổng không chịu thuế (nếu có)
   • Tổng thanh toán

☑️ 4. Thông tin đầy đủ:
   • Tên, MST người mua
   • Số claim (nếu BH)
   • Biển số xe
   • Phân loại rõ ràng
```

---

*Tài liệu quy định VAT - Version 3.0.0*  
*Căn cứ: Luật Thuế GTGT số 13/2008/QH12 và Thông tư 219/2013/TT-BTC*  
*Last Updated: 2024-10-22*

### **2. 📝 Ghi chú rõ ràng**
```
- Ghi chú lý do Override VAT
- Lưu trữ thông tin nhà cung cấp
- Theo dõi lịch sử thay đổi VAT
```

### **3. 🔄 Đồng bộ dữ liệu**
```
- Đảm bảo VATRate được cập nhật đúng
- Kiểm tra tính nhất quán của dữ liệu
- Backup dữ liệu định kỳ
```

---

## 📞 **HỖ TRỢ VÀ LIÊN HỆ**

Nếu có thắc mắc về việc xử lý thuế VAT trong hệ thống, vui lòng liên hệ:
- **Email**: support@garage.com
- **Hotline**: 032.7007.985
- **Thời gian hỗ trợ**: 8:00 - 17:00 (Thứ 2 - Thứ 6)

---

*Tài liệu này được cập nhật lần cuối: [Ngày hiện tại]*
*Phiên bản: 1.0*
