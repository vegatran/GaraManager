# GIAI ĐOẠN 2: CẢI TIẾN TƯƠNG LAI

## 📋 MỤC LỤC

1. [High Priority Improvements](#high-priority-improvements)
2. [Medium Priority Improvements](#medium-priority-improvements)
3. [Low Priority Improvements](#low-priority-improvements)

---

## 🔴 HIGH PRIORITY IMPROVEMENTS

### **1. Validation & Error Handling nâng cao**

**Mô tả:** Cải thiện validation và error messages để tăng UX và giảm lỗi người dùng

**Cần implement:**
- ✅ Validate khi hủy Service Order có items đang "InProgress" → Warning rõ ràng
- ✅ Validate khi xóa Additional Issue có items đang "OnHold" → Đã có
- ✅ Validate EstimatedHours dựa trên ServiceType (nếu có historical data)
- ✅ Better error messages với context cụ thể (ví dụ: "Không thể hủy JO vì có 3 items đang làm việc")

**Độ ưu tiên:** ⭐⭐⭐ (High)
**Thời gian ước tính:** 1-2 ngày

---

### **2. Performance Optimization**

**Mô tả:** Tối ưu hiệu suất cho các tính năng Phase 2

**Cần implement:**
- ✅ Cache workload data cho KTV (5 phút) để giảm số lượng API calls
- ✅ Lazy load progress data (chỉ load khi tab được mở) → Đã có
- ✅ Optimize progress query với projection thay vì load full entities
- ✅ Index database cho các queries thường dùng (AssignedTechnicianId, Status, StartTime)

**Độ ưu tiên:** ⭐⭐⭐ (High)
**Thời gian ước tính:** 1 ngày

---

### **3. Tính tổng EstimatedHours trong View Order Modal**

**Mô tả:** Hiển thị tổng giờ công dự kiến và so sánh với thực tế

**Cần implement:**
- ✅ Tính tổng EstimatedHours từ tất cả items trong View Modal
- ✅ Hiển thị tổng EstimatedHours vs ActualHours
- ✅ Progress indicator dựa trên giờ công (EstimatedHours / ActualHours)
- ✅ Warning nếu ActualHours > EstimatedHours * 1.5 (vượt quá 50%)

**Độ ưu tiên:** ⭐⭐⭐ (High)
**Thời gian ước tính:** 0.5 ngày

**Files cần sửa:**
- `src/GarageManagementSystem.Web/Views/OrderManagement/_ViewOrderModal.cshtml`
- `src/GarageManagementSystem.Web/wwwroot/js/order-management.js`

---

## 🟡 MEDIUM PRIORITY IMPROVEMENTS

### **4. Export/Print Reports**

**Mô tả:** Xuất báo cáo tiến độ và phiếu phân công

**Cần implement:**
- ✅ Export tiến độ Service Order ra PDF/Excel
- ✅ Print phiếu phân công cho KTV
- ✅ Export danh sách phát sinh ra Excel
- ✅ Print báo giá phát sinh

**Độ ưu tiên:** ⭐⭐ (Medium)
**Thời gian ước tính:** 2-3 ngày

**Công nghệ đề xuất:**
- PDF: iTextSharp hoặc QuestPDF
- Excel: EPPlus hoặc ClosedXML

---

### **5. Email Notifications**

**Mô tả:** Gửi email thông báo cho các sự kiện quan trọng

**Cần implement:**
- ✅ Email khi có phát sinh mới (gửi cho CVDV)
- ✅ Email khi báo giá phát sinh được duyệt/từ chối (gửi cho KTV)
- ✅ Email khi Service Order hoàn thành (gửi cho khách hàng)
- ✅ Email khi QC không đạt (gửi cho KTV và Quản đốc)

**Độ ưu tiên:** ⭐⭐ (Medium)
**Thời gian ước tính:** 2 ngày

**Công nghệ đề xuất:**
- SendGrid hoặc SMTP
- Background job để gửi email async

---

### **6. Timeline View**

**Mô tả:** Hiển thị timeline các mốc thời gian quan trọng

**Cần implement:**
- ✅ Timeline hiển thị các mốc thời gian quan trọng của Service Order
- ✅ Visual timeline cho Service Order progress
- ✅ History timeline cho phát sinh (khi tạo, khi duyệt, khi từ chối)

**Độ ưu tiên:** ⭐⭐ (Medium)
**Thời gian ước tính:** 2 ngày

**Công nghệ đề xuất:**
- Timeline.js hoặc custom CSS/JS
- Hiển thị trong tab "Tiến Độ" hoặc tab riêng "Lịch Sử"

---

## 🟢 LOW PRIORITY IMPROVEMENTS

### **7. Kiểm tra xung đột thời gian khi phân công KTV**

**Mô tả:** Validate không xung đột lịch khi phân công KTV

**Cần implement:**
- ✅ Check xung đột dựa trên `ScheduledDate`, `EstimatedHours`, và Appointments hiện tại
- ✅ Hiển thị warning nếu có xung đột
- ✅ Cho phép override nếu cần

**Độ ưu tiên:** ⭐⭐ (Low)
**Thời gian ước tính:** 1-2 ngày

---

### **8. Gợi ý KTV phù hợp**

**Mô tả:** Gợi ý KTV phù hợp với hạng mục dựa trên chuyên môn

**Cần implement:**
- ✅ Hiển thị chuyên môn/skills của KTV trong dropdown
- ✅ Gợi ý KTV dựa trên `Service.Category`, `Service.ServiceType`
- ✅ Highlight KTV phù hợp nhất trong dropdown

**Độ ưu tiên:** ⭐⭐ (Low)
**Thời gian ước tính:** 1-2 ngày

**Cần bổ sung:**
- Entity `EmployeeSkill` hoặc field `Skills` trong Employee
- Mapping giữa Service.Category và Employee.Skills

---

### **9. Validation EstimatedHours nâng cao**

**Mô tả:** Validation dựa trên loại service và lịch sử

**Cần implement:**
- ✅ Validation theo ServiceType/ServiceCategory
- ✅ So sánh với historical data (trung bình EstimatedHours của cùng Service trong 3 tháng gần nhất)
- ✅ Warning nếu chênh lệch > 50% so với trung bình

**Độ ưu tiên:** ⭐ (Very Low)
**Thời gian ước tính:** 1-2 ngày

---

### **10. Export/Print phiếu phân công**

**Mô tả:** In phiếu phân công cho KTV

**Cần implement:**
- ✅ Template print phân công (PDF/HTML)
- ✅ Export Excel: Danh sách phân công theo KTV
- ✅ View schedule theo KTV

**Độ ưu tiên:** ⭐ (Very Low)
**Thời gian ước tính:** 1-2 ngày

---

## 📊 TỔNG KẾT

### **Ưu tiên triển khai:**

1. **🔴 HIGH (Nên làm trước khi production):**
   - Validation & Error Handling nâng cao
   - Performance Optimization
   - Tính tổng EstimatedHours trong View

2. **🟡 MEDIUM (Có thể làm sau khi production ổn định):**
   - Export/Print Reports
   - Email Notifications
   - Timeline View

3. **🟢 LOW (Optional, làm khi có thời gian):**
   - Kiểm tra xung đột thời gian
   - Gợi ý KTV phù hợp
   - Validation EstimatedHours nâng cao
   - Export/Print phiếu phân công

---

**Ghi chú:** Tất cả các cải tiến này là **optional** và không ảnh hưởng đến tính năng core của Giai đoạn 2. Hệ thống đã sẵn sàng cho production với các tính năng hiện tại.

**Ngày tạo:** 2025-11-03
**Trạng thái:** 📝 Đã note, chờ triển khai khi có thời gian

