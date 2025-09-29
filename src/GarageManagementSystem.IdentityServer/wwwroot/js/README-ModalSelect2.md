# Modal Select2 Management Pattern

## Tổng quan
Pattern này giúp quản lý Select2 trong modal một cách tự động, tránh conflict giữa các modal và trang chính.

## Cách sử dụng

### 1. Thêm class `modal-with-select2` vào modal
```html
<div class="modal fade modal-with-select2" id="myModal" tabindex="-1">
    <!-- Modal content với Select2 -->
    <select class="form-control select2" id="mySelect" name="MySelect" multiple>
        <option value="option1">Option 1</option>
        <option value="option2">Option 2</option>
    </select>
</div>
```

### 2. Select2 sẽ được tự động quản lý
- **Khi modal show**: Select2 được khởi tạo với dropdownParent
- **Khi modal hide**: Select2 được destroy để tránh conflict

### 3. Set selected values cho Edit modal
```html
<!-- Trong partial view -->
<select class="form-control select2" id="editRoles" name="Roles" multiple 
        data-selected-values='@Html.Raw(Json.Serialize(Model.Roles))'>
    <option value="Admin">Admin</option>
    <option value="User">User</option>
</select>
```

```javascript
// Trong JavaScript
$('#editModal').on('shown.bs.modal', function () {
    setTimeout(function() {
        var roles = $('#editRoles').data('selected-values');
        if (roles) {
            $('#editRoles').val(roles).trigger('change');
        }
    }, 100);
});
```

### 4. Custom options cho Select2
```javascript
// Nếu cần custom options
ModalSelect2.initWithCustomOptions('myModal', {
    '.select2': {
        placeholder: 'Custom placeholder...',
        allowClear: false
    },
    '.select2-multiple': {
        placeholder: 'Select multiple...',
        closeOnSelect: false
    }
});
```

**Lưu ý:** Không sử dụng theme Bootstrap4 để giữ style thuần của Select2.

## API Reference

### ModalSelect2.init(modalId, options)
Khởi tạo Select2 cho modal với options tùy chỉnh.

### ModalSelect2.setSelectedValues(modalId, fieldId, values)
Set selected values cho Select2 trong modal.

### ModalSelect2.initWithCustomOptions(modalId, selectors)
Khởi tạo nhiều Select2 với options khác nhau.

### ModalSelect2.reset(modalId)
Reset form và destroy Select2 trong modal.

## Ví dụ hoàn chỉnh

### HTML
```html
<!-- Create Modal -->
<div class="modal fade modal-with-select2" id="createModal">
    <select class="form-control select2" id="createRoles" name="Roles" multiple>
        <option value="Admin">Admin</option>
        <option value="User">User</option>
    </select>
</div>

<!-- Edit Modal -->
<div class="modal fade modal-with-select2" id="editModal">
    <select class="form-control select2" id="editRoles" name="Roles" multiple 
            data-selected-values='@Html.Raw(Json.Serialize(Model.Roles))'>
        <option value="Admin">Admin</option>
        <option value="User">User</option>
    </select>
</div>
```

### JavaScript
```javascript
$(document).ready(function() {
    // Select2 cho trang chính (không trong modal)
    $('.select2:not(.modal .select2)').select2({
        width: '100%'
    });

    // Edit modal - set selected values
    $('#editModal').on('shown.bs.modal', function () {
        setTimeout(function() {
            var roles = $('#editRoles').data('selected-values');
            if (roles) {
                $('#editRoles').val(roles).trigger('change');
            }
        }, 100);
    });
});
```

## Lưu ý
- Luôn sử dụng class `modal-with-select2` cho modal có Select2
- Không khởi tạo Select2 thủ công trong modal
- Sử dụng `data-selected-values` để set selected values
- Delay 100ms khi set values để đảm bảo Select2 đã được khởi tạo
