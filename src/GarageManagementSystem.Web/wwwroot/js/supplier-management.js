/**
 * Supplier Management Module
 * 
 * Handles all supplier-related operations
 * CRUD operations for suppliers
 */

window.SupplierManagement = {
    // DataTable instance
    supplierTable: null,

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
            { data: 'name', title: 'Tên Nhà Cung Cấp' },
            { data: 'contactPerson', title: 'Người Liên Hệ' },
            { data: 'phone', title: 'Số Điện Thoại' },
            { data: 'email', title: 'Email' },
            { data: 'address', title: 'Địa Chỉ' },
            { 
                data: 'isActive', 
                title: 'Trạng Thái',
                render: DataTablesUtility.renderStatus
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

        this.supplierTable = DataTablesUtility.initAjaxTable('#suppliersTable', '/SupplierManagement/GetSuppliers', columns, {
            order: [[0, 'desc']],
            pageLength: 25,
            dom: 'rtip'
        });
    },

    // Bind events
    bindEvents: function() {
        var self = this;

        // Search functionality
        $('#searchInput').on('keyup', function() {
            self.supplierTable.search(this.value).draw();
        });

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
        var formData = {
            SupplierCode: $('#supplierCode').val(),
            SupplierName: $('#supplierName').val(),
            ContactPerson: $('#contactPerson').val(),
            Phone: $('#phone').val(),
            Email: $('#email').val(),
            Address: $('#address').val(),
            IsActive: $('#isActive').val() === 'true'
        };

        $.ajax({
            url: '/SupplierManagement/CreateSupplier',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
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
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra khi tạo nhà cung cấp: ' + error,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        });
    },

    // View supplier details
    viewSupplier: function(id) {
        $.ajax({
            url: '/SupplierManagement/GetSupplier/' + id,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var supplier = response.data;
                    Swal.fire({
                        title: 'Thông Tin Nhà Cung Cấp',
                        html: `
                            <div class="text-left">
                                <p><strong>Mã NCC:</strong> ${supplier.supplierCode || 'N/A'}</p>
                                <p><strong>Tên:</strong> ${supplier.name || 'N/A'}</p>
                                <p><strong>Người liên hệ:</strong> ${supplier.contactPerson || 'N/A'}</p>
                                <p><strong>Số điện thoại:</strong> ${supplier.phone || 'N/A'}</p>
                                <p><strong>Email:</strong> ${supplier.email || 'N/A'}</p>
                                <p><strong>Địa chỉ:</strong> ${supplier.address || 'N/A'}</p>
                                <p><strong>Trạng thái:</strong> ${supplier.isActive ? 'Hoạt động' : 'Không hoạt động'}</p>
                            </div>
                        `,
                        icon: 'info',
                        confirmButtonText: 'Đóng'
                    });
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
        $.ajax({
            url: '/SupplierManagement/GetSupplier/' + id,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var supplier = response.data;
                    
                    $('#editSupplierId').val(supplier.id);
                    $('#editSupplierCode').val(supplier.supplierCode);
                    $('#editSupplierName').val(supplier.name);
                    $('#editContactPerson').val(supplier.contactPerson);
                    $('#editPhone').val(supplier.phone);
                    $('#editEmail').val(supplier.email);
                    $('#editAddress').val(supplier.address);
                    $('#editIsActive').val(supplier.isActive.toString());
                    
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

    // Update supplier
    updateSupplier: function() {
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
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra khi cập nhật nhà cung cấp: ' + error,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
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
