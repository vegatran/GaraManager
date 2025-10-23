/**
 * Supplier Management Module
 * 
 * Handles all supplier-related operations
 * CRUD operations for suppliers
 */

window.SupplierManagement = {
    // DataTable instance
    supplierTable: null,
    currentEditData: null, // ✅ THÊM: Store data for edit modal

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        // Sử dụng DataTablesUtility với style chung
        var columns = [
            { data: 'id', title: 'ID', width: '60px' },
            { data: 'supplierCode', title: 'Mã NCC' },
            { data: 'supplierName', title: 'Tên Nhà Cung Cấp' },
            { data: 'contactPerson', title: 'Người Liên Hệ' },
            { data: 'phone', title: 'Số Điện Thoại' },
            { data: 'email', title: 'Email' },
            { data: 'address', title: 'Địa Chỉ' },
            { 
                data: 'isActive', 
                title: 'Trạng Thái',
                render: function(data, type, row) {
                    return data ? '<span class="badge badge-success">Hoạt động</span>' : '<span class="badge badge-danger">Tạm dừng</span>';
                }
            },
            { 
                data: 'createdAt', 
                title: 'Ngày Tạo',
                render: DataTablesUtility.renderDate
            },
            {
                data: null,
                title: 'Thao Tác',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    return `
                        <button class="btn btn-info btn-sm view-supplier" data-id="${row.id}">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-warning btn-sm edit-supplier" data-id="${row.id}">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn btn-danger btn-sm delete-supplier" data-id="${row.id}">
                            <i class="fas fa-trash"></i>
                        </button>
                    `;
                }
            }
        ];

        this.supplierTable = DataTablesUtility.initServerSideTable('#suppliersTable', '/SupplierManagement/GetSuppliers', columns, {
            order: [[0, 'desc']],
            pageLength: 10
        });
    },

    // Bind events
    bindEvents: function() {
        var self = this;


        // Create supplier form
        $(document).on('submit', '#createSupplierForm', function(e) {
            e.preventDefault();
            self.createSupplier();
        });

        // Edit supplier form
        $(document).on('submit', '#editSupplierForm', function(e) {
            e.preventDefault();
            self.updateSupplier();
        });

        // View supplier
        $(document).on('click', '.view-supplier', function() {
            var id = $(this).data('id');
            self.viewSupplier(id);
        });

        // Edit supplier
        $(document).on('click', '.edit-supplier', function() {
            var id = $(this).data('id');
            self.editSupplier(id);
        });

        // ✅ THÊM: Populate edit modal when shown
        $('#editSupplierModal').on('shown.bs.modal', function() {
            if (self.currentEditData) {
                self.populateEditModal(self.currentEditData);
                self.currentEditData = null; // Clear after use
            }
        });

        // ✅ THÊM: Populate view modal when shown
        $('#viewSupplierModal').on('shown.bs.modal', function() {
            if (self.currentEditData) {
                self.populateViewModal(self.currentEditData);
                self.currentEditData = null; // Clear after use
            }
        });

        // Delete supplier
        $(document).on('click', '.delete-supplier', function() {
            var id = $(this).data('id');
            self.deleteSupplier(id);
        });

        // Reset form when modal is hidden
        $('#createSupplierModal').on('hidden.bs.modal', function() {
            $('#createSupplierForm')[0].reset();
        });

        $('#editSupplierModal').on('hidden.bs.modal', function() {
            $('#editSupplierForm')[0].reset();
        });
    },

    // Create new supplier
    createSupplier: function() {
        var self = this;
        var formData = {
            SupplierCode: $('#supplierCode').val(),
            SupplierName: $('#supplierName').val(),
            ContactPerson: $('#contactPerson').val(),
            Phone: $('#phone').val(),
            Email: $('#email').val(),
            Address: $('#address').val(),
            IsActive: $('#isActive').val() === 'true'
        };

        // Debug logging
        console.log('DEBUG: CreateSupplier formData:', formData);

        $.ajax({
            url: '/SupplierManagement/CreateSupplier',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                console.log('DEBUG: CreateSupplier success response:', response);
                if (response.success) {
                    Swal.fire({
                        title: 'Thành công!',
                        text: 'Đã tạo nhà cung cấp mới thành công',
                        icon: 'success',
                        confirmButtonText: 'OK'
                    }).then(() => {
                        $('#createSupplierModal').modal('hide');
                        SupplierManagement.supplierTable.ajax.reload();
                    });
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.message || 'Có lỗi xảy ra khi tạo nhà cung cấp',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function(xhr, status, error) {
                console.log('DEBUG: CreateSupplier error response:', xhr.responseText);
                console.log('DEBUG: CreateSupplier status:', xhr.status);
                
                if (xhr.status === 400) {
                    // Handle validation errors
                    try {
                        var errorResponse = JSON.parse(xhr.responseText);
                        console.log('DEBUG: Parsed error response:', errorResponse);
                        
                        if (errorResponse.errors) {
                            console.log('DEBUG: Displaying validation errors:', errorResponse.errors);
                            self.displayValidationErrors(errorResponse.errors);
                        } else {
                            Swal.fire({
                                title: 'Lỗi!',
                                text: errorResponse.message || 'Dữ liệu không hợp lệ',
                                icon: 'error',
                                confirmButtonText: 'OK'
                            });
                        }
                    } catch (e) {
                        console.log('DEBUG: Error parsing response:', e);
                        Swal.fire({
                            title: 'Lỗi!',
                            text: 'Dữ liệu không hợp lệ',
                            icon: 'error',
                            confirmButtonText: 'OK'
                        });
                    }
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: 'Có lỗi xảy ra khi tạo nhà cung cấp: ' + error,
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            }
        });
    },

    // ✅ THÊM: Function hiển thị validation errors
    displayValidationErrors: function(errors) {
        console.log('DEBUG: displayValidationErrors called with:', errors);
        
        // Clear previous errors
        $('.is-invalid').removeClass('is-invalid');
        $('.invalid-feedback').remove();
        
        // Display field-specific errors
        for (var field in errors) {
            console.log('DEBUG: Processing field:', field, 'with errors:', errors[field]);
            
            var fieldElement = $(`#${field.toLowerCase()}`);
            console.log('DEBUG: Found field element:', fieldElement.length, fieldElement);
            
            if (fieldElement.length === 0) {
                // Try alternative field names for create modal
                if (field.toLowerCase() === 'suppliercode') {
                    fieldElement = $('#supplierCode');
                } else if (field.toLowerCase() === 'suppliername') {
                    fieldElement = $('#supplierName');
                } else if (field.toLowerCase() === 'contactperson') {
                    fieldElement = $('#contactPerson');
                } else if (field.toLowerCase() === 'phone') {
                    fieldElement = $('#phone');
                } else if (field.toLowerCase() === 'email') {
                    fieldElement = $('#email');
                } else if (field.toLowerCase() === 'address') {
                    fieldElement = $('#address');
                }
                // Try alternative field names for edit modal
                if (fieldElement.length === 0) {
                    if (field.toLowerCase() === 'suppliercode') {
                        fieldElement = $('#editSupplierCode');
                    } else if (field.toLowerCase() === 'suppliername') {
                        fieldElement = $('#editSupplierName');
                    } else if (field.toLowerCase() === 'contactperson') {
                        fieldElement = $('#editContactPerson');
                    } else if (field.toLowerCase() === 'phone') {
                        fieldElement = $('#editPhone');
                    } else if (field.toLowerCase() === 'email') {
                        fieldElement = $('#editEmail');
                    } else if (field.toLowerCase() === 'address') {
                        fieldElement = $('#editAddress');
                    }
                }
            }
            
            console.log('DEBUG: Final field element:', fieldElement.length, fieldElement);
            
            if (fieldElement.length > 0) {
                fieldElement.addClass('is-invalid');
                fieldElement.after(`<div class="invalid-feedback">${errors[field].join(', ')}</div>`);
                console.log('DEBUG: Added validation error to field:', field);
            } else {
                console.log('DEBUG: Could not find field element for:', field);
            }
        }
        
        // Show general error message with specific field errors
        var errorMessages = [];
        for (var field in errors) {
            errorMessages.push(field + ': ' + errors[field].join(', '));
        }
        
        console.log('DEBUG: Showing SweetAlert with errors:', errorMessages);
        
        Swal.fire({
            title: 'Lỗi Validation!',
            html: '<div style="text-align: left;"><strong>Vui lòng kiểm tra lại các trường sau:</strong><br><br>' + 
                  errorMessages.map(msg => '• ' + msg).join('<br>') + '</div>',
            icon: 'error',
            confirmButtonText: 'OK'
        });
    },

    // View supplier details
    viewSupplier: function(id) {
        var self = this;
        $.ajax({
            url: '/SupplierManagement/GetSupplier/' + id,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    // Store data and show modal
                    self.currentEditData = response.data;
                    $('#viewSupplierModal').modal('show');
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.error || 'Không thể tải thông tin nhà cung cấp',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function(xhr, status, error) {
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra khi tải thông tin nhà cung cấp: ' + error,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        });
    },

    // Edit supplier
    editSupplier: function(id) {
        var self = this;
        $.ajax({
            url: '/SupplierManagement/GetSupplier/' + id,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    // Store data and show modal
                    self.currentEditData = response.data;
                    $('#editSupplierModal').modal('show');
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.error || 'Không thể tải thông tin nhà cung cấp',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function(xhr, status, error) {
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra khi tải thông tin nhà cung cấp: ' + error,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        });
    },

    // ✅ THÊM: Function populate edit modal
    populateEditModal: function(supplier) {
        $('#editSupplierId').val(supplier.id);
        $('#editSupplierCode').val(supplier.supplierCode);
        $('#editSupplierName').val(supplier.supplierName);
        $('#editContactPerson').val(supplier.contactPerson);
        $('#editPhone').val(supplier.phone);
        $('#editEmail').val(supplier.email);
        $('#editAddress').val(supplier.address);
        $('#editIsActive').val(supplier.isActive.toString());
    },

    // ✅ THÊM: Function populate view modal
    populateViewModal: function(supplier) {
        $('#viewSupplierCode').text(supplier.supplierCode || 'N/A');
        $('#viewSupplierName').text(supplier.supplierName || 'N/A');
        $('#viewContactPerson').text(supplier.contactPerson || 'N/A');
        $('#viewPhone').text(supplier.phone || 'N/A');
        $('#viewEmail').text(supplier.email || 'N/A');
        $('#viewAddress').text(supplier.address || 'N/A');
        $('#viewIsActive').text(supplier.isActive ? 'Hoạt động' : 'Không hoạt động');
    },

    // Update supplier
    updateSupplier: function() {
        var self = this;
        var formData = {
            Id: parseInt($('#editSupplierId').val()),
            SupplierCode: $('#editSupplierCode').val(),
            SupplierName: $('#editSupplierName').val(),
            ContactPerson: $('#editContactPerson').val(),
            Phone: $('#editPhone').val(),
            Email: $('#editEmail').val(),
            Address: $('#editAddress').val(),
            IsActive: $('#editIsActive').val() === 'true'
        };

        $.ajax({
            url: '/SupplierManagement/UpdateSupplier/' + formData.Id,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    Swal.fire({
                        title: 'Thành công!',
                        text: 'Đã cập nhật thông tin nhà cung cấp thành công',
                        icon: 'success',
                        confirmButtonText: 'OK'
                    }).then(() => {
                        $('#editSupplierModal').modal('hide');
                        SupplierManagement.supplierTable.ajax.reload();
                    });
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.message || 'Có lỗi xảy ra khi cập nhật nhà cung cấp',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function(xhr, status, error) {
                if (xhr.status === 400) {
                    // Handle validation errors
                    try {
                        var errorResponse = JSON.parse(xhr.responseText);
                        if (errorResponse.errors) {
                            self.displayValidationErrors(errorResponse.errors);
                        } else {
                            Swal.fire({
                                title: 'Lỗi!',
                                text: errorResponse.message || 'Dữ liệu không hợp lệ',
                                icon: 'error',
                                confirmButtonText: 'OK'
                            });
                        }
                    } catch (e) {
                        Swal.fire({
                            title: 'Lỗi!',
                            text: 'Dữ liệu không hợp lệ',
                            icon: 'error',
                            confirmButtonText: 'OK'
                        });
                    }
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: 'Có lỗi xảy ra khi cập nhật nhà cung cấp: ' + error,
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            }
        });
    },

    // Delete supplier
    deleteSupplier: function(id) {
        Swal.fire({
            title: 'Xác nhận xóa',
            text: 'Bạn có chắc chắn muốn xóa nhà cung cấp này?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/SupplierManagement/DeleteSupplier/' + id,
                    type: 'DELETE',
                    success: function(response) {
                        if (response.success) {
                            Swal.fire({
                                title: 'Đã xóa!',
                                text: 'Nhà cung cấp đã được xóa thành công',
                                icon: 'success',
                                confirmButtonText: 'OK'
                            }).then(() => {
                                SupplierManagement.supplierTable.ajax.reload();
                            });
                        } else {
                            Swal.fire({
                                title: 'Lỗi!',
                                text: response.message || 'Có lỗi xảy ra khi xóa nhà cung cấp',
                                icon: 'error',
                                confirmButtonText: 'OK'
                            });
                        }
                    },
                    error: function(xhr, status, error) {
                        Swal.fire({
                            title: 'Lỗi!',
                            text: 'Có lỗi xảy ra khi xóa nhà cung cấp: ' + error,
                            icon: 'error',
                            confirmButtonText: 'OK'
                        });
                    }
                });
            }
        });
    }
};

$(document).ready(function() {
    SupplierManagement.init();
});
