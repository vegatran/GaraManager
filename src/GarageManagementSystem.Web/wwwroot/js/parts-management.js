// Parts Management Module
window.PartsManagement = {
    partTable: null,

    init: function() {
        this.initDataTable();
        this.bindEvents();
    },

    initDataTable: function() {
        var self = this;
        
        // Sử dụng DataTablesUtility với style chung
        var columns = [
            { data: 'id', title: 'ID', width: '5%' },
            { data: 'partNumber', title: 'Mã Phụ Tùng', width: '10%' },
            { data: 'name', title: 'Tên Phụ Tùng', width: '15%' },
            { data: 'category', title: 'Danh Mục', width: '10%' },
            { data: 'brand', title: 'Thương Hiệu', width: '10%' },
            { 
                data: 'price', 
                title: 'Giá Bán', 
                width: '10%',
                render: DataTablesUtility.renderCurrency
            },
            { data: 'stockQuantity', title: 'Tồn Kho', width: '8%' },
            { data: 'minStockLevel', title: 'Tồn TT', width: '8%' },
            { data: 'unit', title: 'Đơn Vị', width: '7%' },
            { 
                data: 'status', 
                title: 'Trạng Thái', 
                width: '8%',
                render: DataTablesUtility.renderStatus
            },
            {
                data: null,
                title: 'Thao Tác',
                width: '9%',
                orderable: false,
                render: function(data, type, row) {
                    return `
                        <button class="btn btn-info btn-sm view-part" data-id="${row.id}" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-warning btn-sm edit-part" data-id="${row.id}" title="Sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn btn-danger btn-sm delete-part" data-id="${row.id}" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    `;
                }
            }
        ];
        
        this.partTable = DataTablesUtility.initAjaxTable('#partTable', '/PartsManagement/GetParts', columns, {
            order: [[0, 'desc']],
            pageLength: 25,
            dom: 'rtip'  // Chỉ hiển thị table, paging, info, processing (không có search box và length menu)
        });
    },

    bindEvents: function() {
        var self = this;

        // Search functionality
        $('#searchInput').on('keyup', function() {
            self.partTable.search(this.value).draw();
        });

        // Create part
        $('#createPartForm').on('submit', function(e) {
            e.preventDefault();
            self.createPart();
        });

        // Edit part
        $('#editPartForm').on('submit', function(e) {
            e.preventDefault();
            self.updatePart();
        });

        // View part
        $(document).on('click', '.view-part', function() {
            var partId = $(this).data('id');
            self.viewPart(partId);
        });

        // Edit part button
        $(document).on('click', '.edit-part', function() {
            var partId = $(this).data('id');
            self.loadPartForEdit(partId);
        });

        // Delete part
        $(document).on('click', '.delete-part', function() {
            var partId = $(this).data('id');
            self.deletePart(partId);
        });

        // Search
        $('#searchInput').on('keyup', function() {
            self.partTable.search(this.value).draw();
        });
    },

    createPart: function() {
        var formData = {
            PartNumber: $('#createPartNumber').val(),
            PartName: $('#createPartName').val(),
            Description: $('#createDescription').val(),
            Category: $('#createCategory').val(),
            Brand: $('#createBrand').val(),
            CostPrice: parseFloat($('#createCostPrice').val()) || 0,
            SellPrice: parseFloat($('#createSellPrice').val()) || 0,
            QuantityInStock: parseInt($('#createQuantityInStock').val()) || 0,
            MinimumStock: parseInt($('#createMinimumStock').val()) || 0,
            ReorderLevel: parseInt($('#createReorderLevel').val()) || null,
            Unit: $('#createUnit').val(),
            Location: $('#createLocation').val(),
            CompatibleVehicles: $('#createCompatibleVehicles').val(),
            IsActive: $('#createIsActive').is(':checked')
        };

        // Validate required fields
        if (!formData.PartNumber || !formData.PartName || formData.SellPrice <= 0) {
            GarageApp.showError('Vui lòng điền đầy đủ thông tin bắt buộc');
            return;
        }

        $.ajax({
            url: '/PartsManagement/CreatePart',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess('Thêm phụ tùng thành công');
                    $('#createPartModal').modal('hide');
                    $('#createPartForm')[0].reset();
                    window.PartsManagement.partTable.ajax.reload();
                } else {
                     GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Không thể thêm phụ tùng');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi thêm phụ tùng');
                }
            }
        });
    },

    loadPartForEdit: function(partId) {
        $.ajax({
            url: '/PartsManagement/GetPart/' + partId,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    window.PartsManagement.populateEditModal(response.data);
                    $('#editPartModal').modal('show');
                } else {
                    GarageApp.showError('Không thể tải thông tin phụ tùng');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin phụ tùng');
                }
            }
        });
    },

    populateEditModal: function(part) {
        $('#editId').val(part.id);
        $('#editPartNumber').val(part.partNumber);
        $('#editPartName').val(part.partName);
        $('#editDescription').val(part.description);
        $('#editCategory').val(part.category);
        $('#editBrand').val(part.brand);
        $('#editCostPrice').val(part.costPrice);
        $('#editSellPrice').val(part.sellPrice);
        $('#editQuantityInStock').val(part.quantityInStock);
        $('#editMinimumStock').val(part.minimumStock);
        $('#editReorderLevel').val(part.reorderLevel);
        $('#editUnit').val(part.unit);
        $('#editLocation').val(part.location);
        $('#editCompatibleVehicles').val(part.compatibleVehicles);
        $('#editIsActive').prop('checked', part.isActive);
    },

    updatePart: function() {
        var partId = $('#editId').val();
        var formData = {
            Id: parseInt(partId),
            PartNumber: $('#editPartNumber').val(),
            PartName: $('#editPartName').val(),
            Description: $('#editDescription').val(),
            Category: $('#editCategory').val(),
            Brand: $('#editBrand').val(),
            CostPrice: parseFloat($('#editCostPrice').val()) || 0,
            SellPrice: parseFloat($('#editSellPrice').val()) || 0,
            QuantityInStock: parseInt($('#editQuantityInStock').val()) || 0,
            MinimumStock: parseInt($('#editMinimumStock').val()) || 0,
            ReorderLevel: parseInt($('#editReorderLevel').val()) || null,
            Unit: $('#editUnit').val(),
            Location: $('#editLocation').val(),
            CompatibleVehicles: $('#editCompatibleVehicles').val(),
            IsActive: $('#editIsActive').is(':checked')
        };

        $.ajax({
            url: '/PartsManagement/UpdatePart/' + partId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess('Cập nhật phụ tùng thành công');
                    $('#editPartModal').modal('hide');
                    window.PartsManagement.partTable.ajax.reload();
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Không thể cập nhật phụ tùng');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi cập nhật phụ tùng');
                }
            }
        });
    },

    viewPart: function(partId) {
        $.ajax({
            url: '/PartsManagement/GetPart/' + partId,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    window.PartsManagement.populateViewModal(response.data);
                    $('#viewPartModal').modal('show');
                } else {
                    GarageApp.showError('Không thể tải thông tin phụ tùng');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin phụ tùng');
                }
            }
        });
    },

    populateViewModal: function(part) {
        $('#viewPartNumber').text(part.partNumber);
        $('#viewPartName').text(part.partName);
        $('#viewDescription').text(part.description || 'N/A');
        $('#viewCategory').text(part.category || 'N/A');
        $('#viewBrand').text(part.brand || 'N/A');
        $('#viewCostPrice').text(part.costPrice.toLocaleString() + ' VNĐ');
        $('#viewSellPrice').text(part.sellPrice.toLocaleString() + ' VNĐ');
        $('#viewQuantityInStock').text(part.quantityInStock);
        $('#viewMinimumStock').text(part.minimumStock);
        $('#viewReorderLevel').text(part.reorderLevel || 'N/A');
        $('#viewUnit').text(part.unit || 'N/A');
        $('#viewLocation').text(part.location || 'N/A');
        $('#viewCompatibleVehicles').text(part.compatibleVehicles || 'N/A');
        $('#viewIsActive').text(part.isActive ? 'Đang hoạt động' : 'Ngừng hoạt động');
        $('#viewCreatedAt').text(new Date(part.createdAt).toLocaleString('vi-VN'));
    },

    deletePart: function(partId) {
        Swal.fire({
            title: 'Xác nhận xóa?',
            text: 'Bạn có chắc chắn muốn xóa phụ tùng này?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/PartsManagement/DeletePart/' + partId,
                    type: 'DELETE',
                    success: function(response) {
                        if (response.success) {
                            GarageApp.showSuccess('Xóa phụ tùng thành công');
                            window.PartsManagement.partTable.ajax.reload();
                        } else {
                            GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Không thể xóa phụ tùng');
                        }
                    },
                    error: function(xhr) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi xóa phụ tùng');
                        }
                    }
                });
            }
        });
    }
};

// Initialize when document is ready
$(document).ready(function() {
    if ($('#partTable').length) {
        PartsManagement.init();
    }
});

