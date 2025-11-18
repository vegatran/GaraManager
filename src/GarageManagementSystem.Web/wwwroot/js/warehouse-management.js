/**
 * Warehouse Management Module
 * 
 * Quản lý kho (Warehouse, Zone, Bin) với đầy đủ các thao tác CRUD
 */

window.WarehouseManagement = {
    warehouseTable: null,
    currentWarehouse: null,
    originalZones: [], // Lưu zones ban đầu khi load edit modal
    originalBins: [], // Lưu bins ban đầu khi load edit modal
    zoneData: {
        create: [],
        edit: []
    },
    binData: {
        create: [],
        edit: []
    },

    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.resetZoneForm('create');
        this.resetZoneForm('edit');
        this.resetBinForm('create');
        this.resetBinForm('edit');
    },

    initDataTable: function() {
        var self = this;
        
        // Sử dụng client-side DataTable vì warehouse ít dữ liệu
        var columns = [
            { data: 'id', title: 'ID', width: '5%' },
            { data: 'code', title: 'Mã Kho', width: '10%' },
            { data: 'name', title: 'Tên Kho', width: '15%' },
            { data: 'address', title: 'Địa Chỉ', width: '20%', render: function(data) { return data || '<span class="text-muted">-</span>'; } },
            { data: 'managerName', title: 'Người Quản Lý', width: '12%', render: function(data) { return data || '<span class="text-muted">-</span>'; } },
            { data: 'phoneNumber', title: 'Số Điện Thoại', width: '10%', render: function(data) { return data || '<span class="text-muted">-</span>'; } },
            { 
                data: 'zones', 
                title: 'Khu Vực', 
                width: '8%',
                render: function(data, type, row) {
                    return data && data.length > 0 ? '<span class="badge badge-info">' + data.length + '</span>' : '<span class="text-muted">0</span>';
                }
            },
            { 
                data: 'bins', 
                title: 'Kệ/Ngăn', 
                width: '8%',
                render: function(data, type, row) {
                    var totalBins = (data && data.length > 0 ? data.length : 0);
                    var zoneBins = 0;
                    if (row.zones && row.zones.length > 0) {
                        row.zones.forEach(function(zone) {
                            if (zone.bins && zone.bins.length > 0) {
                                zoneBins += zone.bins.length;
                            }
                        });
                    }
                    var total = totalBins + zoneBins;
                    return total > 0 ? '<span class="badge badge-info">' + total + '</span>' : '<span class="text-muted">0</span>';
                }
            },
            { 
                data: 'isDefault', 
                title: 'Mặc Định', 
                width: '7%',
                render: function(data, type, row) {
                    return data ? '<span class="badge badge-primary">Mặc định</span>' : '<span class="text-muted">-</span>';
                }
            },
            { 
                data: 'isActive', 
                title: 'Trạng Thái', 
                width: '8%',
                render: function(data, type, row) {
                    return data ? '<span class="badge badge-success">Hoạt động</span>' : '<span class="badge badge-danger">Tạm dừng</span>';
                }
            },
            {
                data: null,
                title: 'Thao Tác',
                width: '12%',
                orderable: false,
                render: function(data, type, row) {
                    return `
                        <button class="btn btn-info btn-sm view-warehouse" data-id="${row.id}" title="Xem chi tiết" data-title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-warning btn-sm edit-warehouse" data-id="${row.id}" title="Sửa" data-title="Chỉnh sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn btn-danger btn-sm delete-warehouse" data-id="${row.id}" title="Xóa" data-title="Xóa kho">
                            <i class="fas fa-trash"></i>
                        </button>
                    `;
                }
            }
        ];
        
        // Khởi tạo DataTable với AJAX (client-side)
        // API trả về List<WarehouseDto> chứ không phải PagedResponse
        var config = {
            processing: true,
            serverSide: false,
            responsive: true,
            autoWidth: false,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
            order: [[0, 'desc']],
            columns: columns,
            ajax: {
                url: '/WarehouseManagement/GetWarehouses',
                type: 'GET',
                dataSrc: function(json) {
                    // API trả về List<WarehouseDto> trực tiếp
                    if (Array.isArray(json)) {
                        return json;
                    }
                    // Nếu có wrapper, trả về data
                    if (json && json.data && Array.isArray(json.data)) {
                        return json.data;
                    }
                    return [];
                },
                error: function(xhr, status, error) {
                    if (window.AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                        AuthHandler.handleUnauthorized(xhr, true);
                    } else if (window.GarageApp) {
                        GarageApp.showError('Lỗi khi tải dữ liệu kho');
                    }
                }
            },
            language: DataTablesUtility.defaultConfig.language,
            dom: "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
                 "<'row'<'col-sm-12'tr>>" +
                 "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>"
        };
        
        this.warehouseTable = $('#warehouseTable').DataTable(config);
    },

    bindEvents: function() {
        var self = this;

        // Create warehouse
        $('#createWarehouseForm').on('submit', function(e) {
            e.preventDefault();
            self.createWarehouse();
        });

        // Edit warehouse
        $('#editWarehouseForm').on('submit', function(e) {
            e.preventDefault();
            self.updateWarehouse();
        });

        // View warehouse
        $(document).on('click', '.view-warehouse', function() {
            var warehouseId = $(this).data('id');
            self.viewWarehouse(warehouseId);
        });

        // Edit warehouse button
        $(document).on('click', '.edit-warehouse', function() {
            var warehouseId = $(this).data('id');
            self.loadWarehouseForEdit(warehouseId);
        });

        // Delete warehouse
        $(document).on('click', '.delete-warehouse', function() {
            var warehouseId = $(this).data('id');
            self.deleteWarehouse(warehouseId);
        });

        // ✅ THÊM: Reset button state khi modal mở (trước khi hiển thị)
        $('#createWarehouseModal').on('show.bs.modal', function() {
            // Reset button về trạng thái ban đầu từ HTML gốc
            var submitButton = $('#createWarehouseForm button[type="submit"]');
            // Nếu button đang ở trạng thái loading, restore ngay
            if (submitButton.prop('disabled')) {
                submitButton.prop('disabled', false);
                // Restore HTML gốc từ data hoặc từ HTML mặc định
                var originalText = submitButton.data('original-text');
                if (originalText) {
                    submitButton.html(originalText);
                    submitButton.removeData('original-text');
                } else {
                    // Nếu không có original-text, restore từ HTML mặc định trong view
                    submitButton.html('<i class="fas fa-save"></i> Lưu');
                }
            }
        });

        // Reset forms khi modal đóng
        $('#createWarehouseModal').on('hidden.bs.modal', function() {
            $('#createWarehouseForm')[0].reset();
            self.zoneData.create = [];
            self.binData.create = [];
            self.renderZonesTable('create');
            self.renderBinsTable('create');
            self.resetZoneForm('create');
            self.resetBinForm('create');
            // ✅ THÊM: Restore button state khi modal đóng - force reset về HTML gốc
            var submitButton = $('#createWarehouseForm button[type="submit"]');
            submitButton.prop('disabled', false);
            submitButton.removeData('original-text');
            submitButton.html('<i class="fas fa-save"></i> Lưu');
        });

        // ✅ THÊM: Reset button state khi modal mở (trước khi hiển thị)
        $('#editWarehouseModal').on('show.bs.modal', function() {
            // Reset button về trạng thái ban đầu từ HTML gốc
            var submitButton = $('#editWarehouseForm button[type="submit"]');
            // Nếu button đang ở trạng thái loading, restore ngay
            if (submitButton.prop('disabled')) {
                submitButton.prop('disabled', false);
                // Restore HTML gốc từ data hoặc từ HTML mặc định
                var originalText = submitButton.data('original-text');
                if (originalText) {
                    submitButton.html(originalText);
                    submitButton.removeData('original-text');
                } else {
                    // Nếu không có original-text, restore từ HTML mặc định trong view
                    submitButton.html('<i class="fas fa-save"></i> Cập Nhật');
                }
            }
        });

        $('#editWarehouseModal').on('hidden.bs.modal', function() {
            self.zoneData.edit = [];
            self.binData.edit = [];
            self.renderZonesTable('edit');
            self.renderBinsTable('edit');
            self.resetZoneForm('edit');
            self.resetBinForm('edit');
            self.currentWarehouse = null;
            // ✅ THÊM: Restore button state khi modal đóng
            var submitButton = $('#editWarehouseForm button[type="submit"]');
            // Force reset button về HTML gốc
            submitButton.prop('disabled', false);
            submitButton.removeData('original-text');
            submitButton.html('<i class="fas fa-save"></i> Cập Nhật');
        });

        // Zone management - Create
        $('#createAddZoneBtn').on('click', function() {
            self.resetZoneForm('create');
            $('#createZoneFormModal').modal('show');
        });

        $('#createZoneForm').on('submit', function(e) {
            e.preventDefault();
            self.saveZoneFromForm('create');
        });

        // Zone management - Edit
        $('#editAddZoneBtn').on('click', function() {
            self.resetZoneForm('edit');
            $('#editZoneFormTitle').text('Thêm Khu Vực');
            $('#editZoneFormModal').modal('show');
        });

        $('#editZoneForm').on('submit', function(e) {
            e.preventDefault();
            self.saveZoneFromForm('edit');
        });

        // Bin management - Create
        $('#createAddBinBtn').on('click', function() {
            self.resetBinForm('create');
            self.populateZoneSelectForBin('create');
            $('#createBinFormModal').modal('show');
        });

        $('#createBinForm').on('submit', function(e) {
            e.preventDefault();
            self.saveBinFromForm('create');
        });

        // Bin management - Edit
        $('#editAddBinBtn').on('click', function() {
            self.resetBinForm('edit');
            self.populateZoneSelectForBin('edit');
            $('#editBinFormTitle').text('Thêm Kệ/Ngăn');
            $('#editBinFormModal').modal('show');
        });

        $('#editBinForm').on('submit', function(e) {
            e.preventDefault();
            self.saveBinFromForm('edit');
        });

        // Zone edit/delete buttons
        $(document).on('click', '.btn-zone-edit', function() {
            var prefix = $(this).data('prefix');
            var index = parseInt($(this).data('index'), 10);
            self.startEditZone(prefix, index);
        });

        $(document).on('click', '.btn-zone-delete', function() {
            var prefix = $(this).data('prefix');
            var index = parseInt($(this).data('index'), 10);
            self.removeZone(prefix, index);
        });

        // Bin edit/delete buttons
        $(document).on('click', '.btn-bin-edit', function() {
            var prefix = $(this).data('prefix');
            var index = parseInt($(this).data('index'), 10);
            self.startEditBin(prefix, index);
        });

        $(document).on('click', '.btn-bin-delete', function() {
            var prefix = $(this).data('prefix');
            var index = parseInt($(this).data('index'), 10);
            self.removeBin(prefix, index);
        });
    },

    // ==================== Warehouse CRUD ====================

    // ✅ THÊM: Helper functions để quản lý loading state cho buttons
    setButtonLoading: function(button, isLoading, loadingText) {
        loadingText = loadingText || 'Đang xử lý...';
        if (isLoading) {
            // ✅ SỬA: Chỉ lưu original-text nếu chưa có (tránh overwrite khi gọi nhiều lần)
            if (!button.data('original-text')) {
                button.data('original-text', button.html());
            }
            button.prop('disabled', true);
            button.html('<i class="fas fa-spinner fa-spin"></i> ' + loadingText);
        } else {
            button.prop('disabled', false);
            var originalText = button.data('original-text');
            if (originalText) {
                button.html(originalText);
                button.removeData('original-text');
            } else {
                // ✅ THÊM: Fallback nếu không có original-text - restore từ button type
                // Kiểm tra button có trong form nào và restore HTML mặc định
                var form = button.closest('form');
                if (form.attr('id') === 'editWarehouseForm') {
                    button.html('<i class="fas fa-save"></i> Cập Nhật');
                } else if (form.attr('id') === 'createWarehouseForm') {
                    button.html('<i class="fas fa-save"></i> Lưu');
                }
            }
        }
    },

    createWarehouse: function() {
        var self = this;
        
        var formData = {
            Code: $('#createCode').val().trim(),
            Name: $('#createName').val().trim(),
            Description: $('#createDescription').val().trim() || null,
            Address: $('#createAddress').val().trim() || null,
            ManagerName: $('#createManagerName').val().trim() || null,
            PhoneNumber: $('#createPhoneNumber').val().trim() || null,
            IsDefault: $('#createIsDefault').is(':checked'),
            IsActive: $('#createIsActive').is(':checked')
        };

        // Validate required fields
        if (!formData.Code || !formData.Name) {
            GarageApp.showError('Vui lòng điền đầy đủ thông tin bắt buộc (Mã kho và Tên kho)');
            return;
        }

        // ✅ THÊM: Disable submit button và show loading
        var submitButton = $('#createWarehouseForm button[type="submit"]');
        self.setButtonLoading(submitButton, true, 'Đang tạo kho...');

        $.ajax({
            url: '/WarehouseManagement/CreateWarehouse',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    // Tạo zones và bins sau khi tạo warehouse thành công
                    var warehouseId = response.data ? response.data.id : null;
                    if (!warehouseId) {
                        self.setButtonLoading(submitButton, false);
                        GarageApp.showError('Không thể lấy ID kho sau khi tạo');
                        return;
                    }
                    // ✅ THÊM: Show loading khi đang lưu zones và bins
                    self.setButtonLoading(submitButton, true, 'Đang lưu khu vực và kệ/ngăn...');
                    self.saveZonesAndBins('create', warehouseId, function() {
                        self.setButtonLoading(submitButton, false);
                        GarageApp.showSuccess('Tạo kho thành công');
                        $('#createWarehouseModal').modal('hide');
                        self.warehouseTable.ajax.reload();
                    });
                } else {
                    self.setButtonLoading(submitButton, false);
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Không thể tạo kho');
                }
            },
            error: function(xhr) {
                self.setButtonLoading(submitButton, false);
                if (window.AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else if (xhr.status === 400) {
                    try {
                        var errorResponse = JSON.parse(xhr.responseText);
                        GarageApp.showError(errorResponse.message || 'Dữ liệu không hợp lệ');
                    } catch (e) {
                        GarageApp.showError('Dữ liệu không hợp lệ');
                    }
                } else {
                    GarageApp.showError('Lỗi khi tạo kho');
                }
            }
        });
    },

    loadWarehouseForEdit: function(warehouseId) {
        var self = this;
        
        // ✅ THÊM: Show loading khi đang tải dữ liệu
        Swal.fire({
            title: 'Đang tải...',
            text: 'Đang tải thông tin kho',
            allowOutsideClick: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
        
        $.ajax({
            url: '/WarehouseManagement/GetWarehouse/' + warehouseId,
            type: 'GET',
            success: function(response) {
                Swal.close();
                // Controller trả về WarehouseDto trực tiếp (không wrapped)
                if (response && response.id) {
                    self.currentWarehouse = response;
                    self.populateEditModal(response);
                    // ✅ THÊM: Reset button state trước khi show modal để đảm bảo button không ở trạng thái loading
                    var submitButton = $('#editWarehouseForm button[type="submit"]');
                    submitButton.prop('disabled', false);
                    submitButton.removeData('original-text');
                    submitButton.html('<i class="fas fa-save"></i> Cập Nhật');
                    $('#editWarehouseModal').modal('show');
                } else {
                    GarageApp.showError('Không thể tải thông tin kho');
                }
            },
            error: function(xhr) {
                Swal.close();
                if (window.AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin kho');
                }
            }
        });
    },

    populateEditModal: function(warehouse) {
        var self = this;
        $('#editId').val(warehouse.id);
        $('#editCode').val(warehouse.code);
        $('#editName').val(warehouse.name);
        $('#editDescription').val(warehouse.description || '');
        $('#editAddress').val(warehouse.address || '');
        $('#editManagerName').val(warehouse.managerName || '');
        $('#editPhoneNumber').val(warehouse.phoneNumber || '');
        $('#editIsDefault').prop('checked', warehouse.isDefault || false);
        $('#editIsActive').prop('checked', warehouse.isActive !== false);

        // Load zones và bins - lưu bản gốc để so sánh khi save
        var zones = warehouse.zones ? warehouse.zones.map(function(zone) {
            return {
                id: zone.id,
                code: zone.code,
                name: zone.name,
                description: zone.description || '',
                displayOrder: zone.displayOrder || 0,
                isActive: zone.isActive !== false
            };
        }) : [];

        this.zoneData.edit = zones.map(function(z) { return Object.assign({}, z); }); // Clone để không ảnh hưởng original
        this.originalZones = zones.map(function(z) { return Object.assign({}, z); }); // Lưu bản gốc

        this.binData.edit = [];
        this.originalBins = [];
        
        // Add bins directly under warehouse
        if (warehouse.bins && warehouse.bins.length > 0) {
            warehouse.bins.forEach(function(bin) {
                var binData = {
                    id: bin.id,
                    warehouseZoneId: null,
                    code: bin.code,
                    name: bin.name,
                    description: bin.description || '',
                    capacity: bin.capacity || null,
                    isDefault: bin.isDefault || false,
                    isActive: bin.isActive !== false
                };
                self.binData.edit.push(binData);
                self.originalBins.push(Object.assign({}, binData)); // Lưu bản gốc
            });
        }
        
        // Add bins under zones
        if (warehouse.zones && warehouse.zones.length > 0) {
            warehouse.zones.forEach(function(zone) {
                if (zone.bins && zone.bins.length > 0) {
                    zone.bins.forEach(function(bin) {
                        var binData = {
                            id: bin.id,
                            warehouseZoneId: zone.id,
                            code: bin.code,
                            name: bin.name,
                            description: bin.description || '',
                            capacity: bin.capacity || null,
                            isDefault: bin.isDefault || false,
                            isActive: bin.isActive !== false
                        };
                        self.binData.edit.push(binData);
                        self.originalBins.push(Object.assign({}, binData)); // Lưu bản gốc
                    });
                }
            });
        }

        this.renderZonesTable('edit');
        this.renderBinsTable('edit');
        this.populateZoneSelectForBin('edit');
    },

    updateWarehouse: function() {
        var self = this;
        var warehouseId = $('#editId').val();
        
        var formData = {
            Id: parseInt(warehouseId, 10),
            Code: $('#editCode').val().trim(),
            Name: $('#editName').val().trim(),
            Description: $('#editDescription').val().trim() || null,
            Address: $('#editAddress').val().trim() || null,
            ManagerName: $('#editManagerName').val().trim() || null,
            PhoneNumber: $('#editPhoneNumber').val().trim() || null,
            IsDefault: $('#editIsDefault').is(':checked'),
            IsActive: $('#editIsActive').is(':checked')
        };

        // Validate required fields
        if (!formData.Code || !formData.Name) {
            GarageApp.showError('Vui lòng điền đầy đủ thông tin bắt buộc (Mã kho và Tên kho)');
            return;
        }

        // ✅ THÊM: Disable submit button và show loading
        var submitButton = $('#editWarehouseForm button[type="submit"]');
        self.setButtonLoading(submitButton, true, 'Đang cập nhật kho...');

        $.ajax({
            url: '/WarehouseManagement/UpdateWarehouse/' + warehouseId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    // Cập nhật zones và bins sau khi cập nhật warehouse thành công
                    self.setButtonLoading(submitButton, true, 'Đang lưu khu vực và kệ/ngăn...');
                    self.saveZonesAndBins('edit', warehouseId, function() {
                        self.setButtonLoading(submitButton, false);
                        GarageApp.showSuccess('Cập nhật kho thành công');
                        $('#editWarehouseModal').modal('hide');
                        self.warehouseTable.ajax.reload();
                    });
                } else {
                    self.setButtonLoading(submitButton, false);
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Không thể cập nhật kho');
                }
            },
            error: function(xhr) {
                self.setButtonLoading(submitButton, false);
                if (window.AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else if (xhr.status === 400) {
                    try {
                        var errorResponse = JSON.parse(xhr.responseText);
                        GarageApp.showError(errorResponse.message || 'Dữ liệu không hợp lệ');
                    } catch (e) {
                        GarageApp.showError('Dữ liệu không hợp lệ');
                    }
                } else {
                    GarageApp.showError('Lỗi khi cập nhật kho');
                }
            }
        });
    },

    deleteWarehouse: function(warehouseId) {
        var self = this;
        
        GarageApp.confirm('Bạn có chắc chắn muốn xóa kho này?', 'Xác nhận xóa', function() {
            // ✅ THÊM: Show loading khi đang xóa
            Swal.fire({
                title: 'Đang xử lý...',
                text: 'Đang xóa kho',
                allowOutsideClick: false,
                showConfirmButton: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            $.ajax({
                url: '/WarehouseManagement/DeleteWarehouse/' + warehouseId,
                type: 'DELETE',
                success: function(response) {
                    Swal.close();
                    if (response.success) {
                        GarageApp.showSuccess('Xóa kho thành công');
                        self.warehouseTable.ajax.reload();
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Không thể xóa kho');
                    }
                },
                error: function(xhr) {
                    Swal.close();
                    if (window.AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                        AuthHandler.handleUnauthorized(xhr, true);
                    } else if (xhr.status === 400) {
                        try {
                            var errorResponse = JSON.parse(xhr.responseText);
                            GarageApp.showError(errorResponse.message || 'Không thể xóa kho');
                        } catch (e) {
                            GarageApp.showError('Không thể xóa kho');
                        }
                    } else {
                        GarageApp.showError('Lỗi khi xóa kho');
                    }
                }
            });
        });
    },

    viewWarehouse: function(warehouseId) {
        var self = this;
        
        // ✅ THÊM: Show loading khi đang tải dữ liệu
        Swal.fire({
            title: 'Đang tải...',
            text: 'Đang tải thông tin kho',
            allowOutsideClick: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
        
        $.ajax({
            url: '/WarehouseManagement/GetWarehouse/' + warehouseId,
            type: 'GET',
            success: function(response) {
                Swal.close();
                    // Controller trả về WarehouseDto trực tiếp (không wrapped)
                if (response && response.id) {
                    self.populateViewModal(response);
                    $('#viewWarehouseModal').modal('show');
                } else {
                    GarageApp.showError('Không thể tải thông tin kho');
                }
            },
            error: function(xhr) {
                Swal.close();
                if (window.AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin kho');
                }
            }
        });
    },

    populateViewModal: function(warehouse) {
        $('#viewCode').text(warehouse.code || '-');
        $('#viewName').text(warehouse.name || '-');
        $('#viewDescription').text(warehouse.description || '-');
        $('#viewAddress').text(warehouse.address || '-');
        $('#viewManagerName').text(warehouse.managerName || '-');
        $('#viewPhoneNumber').text(warehouse.phoneNumber || '-');
        $('#viewIsDefault').html(warehouse.isDefault ? '<span class="badge badge-primary">Mặc định</span>' : '<span class="text-muted">-</span>');
        $('#viewIsActive').html(warehouse.isActive ? '<span class="badge badge-success">Hoạt động</span>' : '<span class="badge badge-danger">Tạm dừng</span>');
        
        if (warehouse.createdAt) {
            $('#viewCreatedAt').text(GarageApp.formatDate(warehouse.createdAt, 'DD/MM/YYYY HH:mm'));
        } else {
            $('#viewCreatedAt').text('-');
        }
        
        if (warehouse.updatedAt) {
            $('#viewUpdatedAt').text(GarageApp.formatDate(warehouse.updatedAt, 'DD/MM/YYYY HH:mm'));
        } else {
            $('#viewUpdatedAt').text('-');
        }

        // Render zones
        var zonesHtml = '';
        // ✅ SỬA: Kiểm tra zones một cách rõ ràng hơn
        if (warehouse.zones && Array.isArray(warehouse.zones) && warehouse.zones.length > 0) {
            $('#viewZonesCount').text(warehouse.zones.length);
            warehouse.zones.forEach(function(zone) {
                var binsCount = (zone.bins && Array.isArray(zone.bins)) ? zone.bins.length : 0;
                zonesHtml += `
                    <tr>
                        <td>${zone.code || '-'}</td>
                        <td>${zone.name || '-'}</td>
                        <td>${zone.description || '-'}</td>
                        <td>${zone.displayOrder || 0}</td>
                        <td><span class="badge badge-info">${binsCount}</span></td>
                        <td>${zone.isActive ? '<span class="badge badge-success">Hoạt động</span>' : '<span class="badge badge-danger">Tạm dừng</span>'}</td>
                    </tr>
                `;
            });
        } else {
            $('#viewZonesCount').text('0');
            zonesHtml = '<tr class="text-muted"><td colspan="6" class="text-center">Chưa có khu vực nào.</td></tr>';
        }
        $('#viewZonesTableBody').html(zonesHtml);

        // Render bins
        var binsHtml = '';
        var allBins = [];
        
        // Add bins directly under warehouse
        if (warehouse.bins && warehouse.bins.length > 0) {
            warehouse.bins.forEach(function(bin) {
                allBins.push({
                    bin: bin,
                    zoneName: '-'
                });
            });
        }
        
        // Add bins under zones
        if (warehouse.zones && warehouse.zones.length > 0) {
            warehouse.zones.forEach(function(zone) {
                if (zone.bins && zone.bins.length > 0) {
                    zone.bins.forEach(function(bin) {
                        allBins.push({
                            bin: bin,
                            zoneName: zone.name
                        });
                    });
                }
            });
        }

        $('#viewBinsCount').text(allBins.length);
        
        if (allBins.length > 0) {
            allBins.forEach(function(item) {
                var bin = item.bin;
                var defaultBadge = bin.isDefault ? '<span class="badge badge-primary">Mặc định</span>' : '<span class="text-muted">-</span>';
                var capacity = bin.capacity ? (GarageApp.formatNumber ? GarageApp.formatNumber(bin.capacity) : bin.capacity) : '-';
                binsHtml += `
                    <tr>
                        <td>${bin.code}</td>
                        <td>${bin.name}</td>
                        <td>${item.zoneName}</td>
                        <td>${bin.description || '-'}</td>
                        <td>${capacity}</td>
                        <td>${defaultBadge}</td>
                        <td>${bin.isActive ? '<span class="badge badge-success">Hoạt động</span>' : '<span class="badge badge-danger">Tạm dừng</span>'}</td>
                    </tr>
                `;
            });
        } else {
            binsHtml = '<tr class="text-muted"><td colspan="7" class="text-center">Chưa có kệ/ngăn nào.</td></tr>';
        }
        $('#viewBinsTableBody').html(binsHtml);
    },

    // ==================== Zone Management ====================

    getZoneFormSelectors: function(prefix) {
        return {
            code: $('#' + prefix + 'ZoneCode'),
            name: $('#' + prefix + 'ZoneName'),
            description: $('#' + prefix + 'ZoneDescription'),
            displayOrder: $('#' + prefix + 'ZoneDisplayOrder'),
            isActive: $('#' + prefix + 'ZoneIsActive'),
            id: $('#' + prefix + 'ZoneId'),
            index: $('#' + prefix + 'ZoneIndex')
        };
    },

    resetZoneForm: function(prefix) {
        var selectors = this.getZoneFormSelectors(prefix);
        selectors.code.val('');
        selectors.name.val('');
        selectors.description.val('');
        selectors.displayOrder.val('0');
        selectors.isActive.prop('checked', true);
        selectors.id.val('');
        selectors.index.val('-1');
    },

    saveZoneFromForm: function(prefix) {
        var self = this;
        var selectors = this.getZoneFormSelectors(prefix);
        
        var zoneData = {
            code: selectors.code.val().trim(),
            name: selectors.name.val().trim(),
            description: selectors.description.val().trim() || '',
            displayOrder: parseInt(selectors.displayOrder.val(), 10) || 0,
            isActive: selectors.isActive.is(':checked')
        };

        // Validate required fields
        if (!zoneData.code || !zoneData.name) {
            GarageApp.showError('Vui lòng điền đầy đủ thông tin bắt buộc (Mã khu vực và Tên khu vực)');
            return;
        }

        // Check duplicate code
        var index = parseInt(selectors.index.val(), 10);
        var existingIndex = this.zoneData[prefix].findIndex(function(z, i) {
            return i !== index && z.code.toLowerCase() === zoneData.code.toLowerCase();
        });

        if (existingIndex !== -1) {
            GarageApp.showError('Mã khu vực đã tồn tại');
            return;
        }

        if (index >= 0) {
            // Update existing zone
            this.zoneData[prefix][index] = zoneData;
        } else {
            // Add new zone
            this.zoneData[prefix].push(zoneData);
        }

        this.renderZonesTable(prefix);
        this.populateZoneSelectForBin(prefix);
        $('#' + prefix + 'ZoneFormModal').modal('hide');
        this.resetZoneForm(prefix);
    },

    startEditZone: function(prefix, index) {
        var self = this;
        var zone = this.zoneData[prefix][index];
        if (!zone) return;

        var selectors = this.getZoneFormSelectors(prefix);
        selectors.code.val(zone.code);
        selectors.name.val(zone.name);
        selectors.description.val(zone.description || '');
        selectors.displayOrder.val(zone.displayOrder || 0);
        selectors.isActive.prop('checked', zone.isActive !== false);
        selectors.id.val(zone.id || '');
        selectors.index.val(index);

        if (prefix === 'edit') {
            $('#editZoneFormTitle').text('Sửa Khu Vực');
        }

        $('#' + prefix + 'ZoneFormModal').modal('show');
    },

    removeZone: function(prefix, index) {
        var self = this;
        var zone = this.zoneData[prefix][index];
        if (!zone) return;

        GarageApp.confirm('Bạn có chắc chắn muốn xóa khu vực này?', 'Xác nhận xóa', function() {
            self.zoneData[prefix].splice(index, 1);
            self.renderZonesTable(prefix);
            self.populateZoneSelectForBin(prefix);
        });
    },

    renderZonesTable: function(prefix) {
        var zones = this.zoneData[prefix];
        var tableBody = $('#' + prefix + 'ZonesTableBody');

        if (!zones || zones.length === 0) {
            tableBody.html('<tr class="text-muted"><td colspan="6" class="text-center">Chưa có khu vực nào.</td></tr>');
            return;
        }

        var rows = zones.map(function(zone, index) {
            return `
                <tr>
                    <td>${zone.code}</td>
                    <td>${zone.name}</td>
                    <td>${zone.description || '<span class="text-muted">-</span>'}</td>
                    <td>${zone.displayOrder || 0}</td>
                    <td>${zone.isActive ? '<span class="badge badge-success">Hoạt động</span>' : '<span class="badge badge-danger">Tạm dừng</span>'}</td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-outline-secondary btn-zone-edit" data-prefix="${prefix}" data-index="${index}" title="Chỉnh sửa" data-title="Chỉnh sửa khu vực">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-outline-danger btn-zone-delete" data-prefix="${prefix}" data-index="${index}" title="Xóa" data-title="Xóa khu vực">
                            <i class="fas fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
        }).join('');

        tableBody.html(rows);
    },

    // ==================== Bin Management ====================

    getBinFormSelectors: function(prefix) {
        return {
            code: $('#' + prefix + 'BinCode'),
            name: $('#' + prefix + 'BinName'),
            warehouseZoneId: $('#' + prefix + 'BinWarehouseZoneId'),
            description: $('#' + prefix + 'BinDescription'),
            capacity: $('#' + prefix + 'BinCapacity'),
            isDefault: $('#' + prefix + 'BinIsDefault'),
            isActive: $('#' + prefix + 'BinIsActive'),
            id: $('#' + prefix + 'BinId'),
            index: $('#' + prefix + 'BinIndex')
        };
    },

    resetBinForm: function(prefix) {
        var selectors = this.getBinFormSelectors(prefix);
        selectors.code.val('');
        selectors.name.val('');
        selectors.warehouseZoneId.val('');
        selectors.description.val('');
        selectors.capacity.val('');
        selectors.isDefault.prop('checked', false);
        selectors.isActive.prop('checked', true);
        selectors.id.val('');
        selectors.index.val('-1');
    },

    populateZoneSelectForBin: function(prefix) {
        var self = this;
        var select = this.getBinFormSelectors(prefix).warehouseZoneId;
        var zones = this.zoneData[prefix] || [];
        
        // Clear existing options except the first one
        select.find('option:not(:first)').remove();
        
        // Add zone options
        zones.forEach(function(zone) {
            select.append($('<option>', {
                value: zone.id || 'temp-' + zones.indexOf(zone),
                text: zone.name + ' (' + zone.code + ')'
            }));
        });
    },

    saveBinFromForm: function(prefix) {
        var self = this;
        var selectors = this.getBinFormSelectors(prefix);
        
        var binData = {
            warehouseZoneId: selectors.warehouseZoneId.val() || null,
            code: selectors.code.val().trim(),
            name: selectors.name.val().trim(),
            description: selectors.description.val().trim() || '',
            capacity: selectors.capacity.val() ? parseFloat(selectors.capacity.val()) : null,
            isDefault: selectors.isDefault.is(':checked'),
            isActive: selectors.isActive.is(':checked')
        };

        // Validate required fields
        if (!binData.code || !binData.name) {
            GarageApp.showError('Vui lòng điền đầy đủ thông tin bắt buộc (Mã kệ/ngăn và Tên kệ/ngăn)');
            return;
        }

        // Check duplicate code
        var index = parseInt(selectors.index.val(), 10);
        var existingIndex = this.binData[prefix].findIndex(function(b, i) {
            return i !== index && b.code.toLowerCase() === binData.code.toLowerCase();
        });

        if (existingIndex !== -1) {
            GarageApp.showError('Mã kệ/ngăn đã tồn tại');
            return;
        }

        // Convert warehouseZoneId if it's a temp ID
        if (binData.warehouseZoneId && binData.warehouseZoneId.toString().startsWith('temp-')) {
            var tempIndex = parseInt(binData.warehouseZoneId.toString().replace('temp-', ''), 10);
            var zone = this.zoneData[prefix][tempIndex];
            if (zone && zone.id) {
                binData.warehouseZoneId = zone.id;
            } else {
                // Zone chưa có ID (sẽ tạo mới), lưu temp index để sau xử lý
                binData._tempZoneIndex = tempIndex;
            }
        }

        if (index >= 0) {
            // Update existing bin
            this.binData[prefix][index] = binData;
        } else {
            // Add new bin
            this.binData[prefix].push(binData);
        }

        this.renderBinsTable(prefix);
        $('#' + prefix + 'BinFormModal').modal('hide');
        this.resetBinForm(prefix);
    },

    startEditBin: function(prefix, index) {
        var self = this;
        var bin = this.binData[prefix][index];
        if (!bin) return;

        var selectors = this.getBinFormSelectors(prefix);
        selectors.code.val(bin.code);
        selectors.name.val(bin.name);
        selectors.description.val(bin.description || '');
        selectors.capacity.val(bin.capacity || '');
        selectors.isDefault.prop('checked', bin.isDefault || false);
        selectors.isActive.prop('checked', bin.isActive !== false);
        selectors.id.val(bin.id || '');
        selectors.index.val(index);

        // Populate zone select and set value
        this.populateZoneSelectForBin(prefix);
        if (bin.warehouseZoneId) {
            selectors.warehouseZoneId.val(bin.warehouseZoneId);
        } else {
            selectors.warehouseZoneId.val('');
        }

        if (prefix === 'edit') {
            $('#editBinFormTitle').text('Sửa Kệ/Ngăn');
        }

        $('#' + prefix + 'BinFormModal').modal('show');
    },

    removeBin: function(prefix, index) {
        var self = this;
        var bin = this.binData[prefix][index];
        if (!bin) return;

        GarageApp.confirm('Bạn có chắc chắn muốn xóa kệ/ngăn này?', 'Xác nhận xóa', function() {
            self.binData[prefix].splice(index, 1);
            self.renderBinsTable(prefix);
        });
    },

    renderBinsTable: function(prefix) {
        var self = this;
        var bins = this.binData[prefix];
        var zones = this.zoneData[prefix] || [];
        var tableBody = $('#' + prefix + 'BinsTableBody');

        if (!bins || bins.length === 0) {
            tableBody.html('<tr class="text-muted"><td colspan="8" class="text-center">Chưa có kệ/ngăn nào.</td></tr>');
            return;
        }

        var rows = bins.map(function(bin, index) {
            // Find zone name
            var zoneName = '-';
            if (bin.warehouseZoneId) {
                var zone = zones.find(function(z) {
                    return z.id === bin.warehouseZoneId || (z.id === undefined && zones.indexOf(z) === bin._tempZoneIndex);
                });
                if (zone) {
                    zoneName = zone.name;
                }
            }

            var defaultBadge = bin.isDefault ? '<span class="badge badge-primary">Mặc định</span>' : '<span class="text-muted">-</span>';
            var capacity = bin.capacity ? (GarageApp.formatNumber ? GarageApp.formatNumber(bin.capacity) : bin.capacity) : '-';
            
            return `
                <tr>
                    <td>${bin.code}</td>
                    <td>${bin.name}</td>
                    <td>${zoneName}</td>
                    <td>${bin.description || '<span class="text-muted">-</span>'}</td>
                    <td>${capacity}</td>
                    <td class="text-center">${defaultBadge}</td>
                    <td>${bin.isActive ? '<span class="badge badge-success">Hoạt động</span>' : '<span class="badge badge-danger">Tạm dừng</span>'}</td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-outline-secondary btn-bin-edit" data-prefix="${prefix}" data-index="${index}" title="Chỉnh sửa" data-title="Chỉnh sửa kệ/ngăn">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-outline-danger btn-bin-delete" data-prefix="${prefix}" data-index="${index}" title="Xóa" data-title="Xóa kệ/ngăn">
                            <i class="fas fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
        }).join('');

        tableBody.html(rows);
    },

    // ==================== Save Zones and Bins ====================

    saveZonesAndBins: function(prefix, warehouseId, callback) {
        var self = this;
        var zones = this.zoneData[prefix] || [];
        var bins = this.binData[prefix] || [];

        // ✅ THÊM: Tính tổng số operations để hiển thị progress
        var totalOperations = zones.length + bins.length;
        if (prefix === 'edit') {
            // Tính số zones/bins cần xóa
            var zonesToDelete = 0;
            var binsToDelete = 0;
            this.originalZones.forEach(function(originalZone) {
                var exists = zones.some(function(newZone) {
                    return newZone.id === originalZone.id;
                });
                if (!exists && originalZone.id) {
                    zonesToDelete++;
                }
            });
            this.originalBins.forEach(function(originalBin) {
                var exists = bins.some(function(newBin) {
                    return newBin.id === originalBin.id;
                });
                if (!exists && originalBin.id) {
                    binsToDelete++;
                }
            });
            totalOperations += zonesToDelete + binsToDelete;
        }

        // ✅ THÊM: Show loading với progress nếu có nhiều operations
        if (totalOperations > 0) {
            // Loading đã được show bởi button, không cần show thêm
            // Chỉ update text nếu cần
        }

        // Nếu edit mode, xóa zones/bins cũ trước
        if (prefix === 'edit') {
            self.deleteRemovedZonesAndBins(warehouseId, zones, bins, function() {
                // Sau khi xóa xong, lưu zones và bins mới
                self.saveZonesAndBinsInternal(prefix, warehouseId, zones, bins, callback);
            });
        } else {
            // Create mode: chỉ cần lưu zones và bins mới
            self.saveZonesAndBinsInternal(prefix, warehouseId, zones, bins, callback);
        }
    },

    saveZonesAndBinsInternal: function(prefix, warehouseId, zones, bins, callback) {
        var self = this;

        // Nếu không có zones và bins, gọi callback ngay
        if (zones.length === 0 && bins.length === 0) {
            if (callback) callback();
            return;
        }

        // Lưu zones trước (tuần tự để có ID)
        self.saveZonesSequentially(prefix, warehouseId, zones, function() {
            // Sau khi zones được lưu, lưu bins
            self.saveBinsSequentially(prefix, warehouseId, bins, zones, function() {
                if (callback) callback();
            });
        });
    },

    deleteRemovedZonesAndBins: function(warehouseId, newZones, newBins, callback) {
        var self = this;
        var zonesToDelete = [];
        var binsToDelete = [];

        // Tìm zones cũ không còn trong danh sách mới
        this.originalZones.forEach(function(originalZone) {
            var exists = newZones.some(function(newZone) {
                return newZone.id === originalZone.id;
            });
            if (!exists && originalZone.id) {
                zonesToDelete.push(originalZone);
            }
        });

        // Tìm bins cũ không còn trong danh sách mới
        this.originalBins.forEach(function(originalBin) {
            var exists = newBins.some(function(newBin) {
                return newBin.id === originalBin.id;
            });
            if (!exists && originalBin.id) {
                binsToDelete.push(originalBin);
            }
        });

        var totalDelete = zonesToDelete.length + binsToDelete.length;
        if (totalDelete === 0) {
            if (callback) callback();
            return;
        }

        var completedDeletes = 0;
        var checkComplete = function() {
            completedDeletes++;
            if (completedDeletes >= totalDelete) {
                if (callback) callback();
            }
        };

        // Xóa zones
        zonesToDelete.forEach(function(zone) {
            $.ajax({
                url: '/WarehouseManagement/DeleteZone/' + warehouseId + '/' + zone.id,
                type: 'DELETE',
                success: function(response) {
                    if (response.success) {
                        checkComplete();
                    } else {
                        GarageApp.showWarning('Lỗi khi xóa khu vực: ' + (response.message || ''));
                        checkComplete();
                    }
                },
                error: function() {
                    GarageApp.showWarning('Lỗi khi xóa khu vực');
                    checkComplete();
                }
            });
        });

        // Xóa bins
        binsToDelete.forEach(function(bin) {
            // Cần xác định zoneId nếu bin thuộc zone (có thể null nếu bin trực tiếp thuộc warehouse)
            var zoneId = bin.warehouseZoneId || 0;
            $.ajax({
                url: '/WarehouseManagement/DeleteBin/' + warehouseId + '/' + bin.id,
                type: 'DELETE',
                success: function(response) {
                    if (response.success) {
                        checkComplete();
                    } else {
                        GarageApp.showWarning('Lỗi khi xóa kệ/ngăn: ' + (response.message || ''));
                        checkComplete();
                    }
                },
                error: function() {
                    GarageApp.showWarning('Lỗi khi xóa kệ/ngăn');
                    checkComplete();
                }
            });
        });
    },

    saveZonesSequentially: function(prefix, warehouseId, zones, callback) {
        var self = this;
        
        if (zones.length === 0) {
            if (callback) callback();
            return;
        }

        var index = 0;
        var processNext = function() {
            if (index >= zones.length) {
                if (callback) callback();
                return;
            }

            var zone = zones[index];
            var zoneData = {
                code: zone.code,
                name: zone.name,
                description: zone.description || '',
                displayOrder: zone.displayOrder || 0,
                isActive: zone.isActive !== false
            };

            if (zone.id && prefix === 'edit') {
                // Update existing zone
                $.ajax({
                    url: '/WarehouseManagement/UpdateZone/' + warehouseId + '/' + zone.id,
                    type: 'PUT',
                    contentType: 'application/json',
                    data: JSON.stringify(zoneData),
                    success: function(response) {
                        if (response.success) {
                            index++;
                            processNext();
                        } else {
                            GarageApp.showError('Lỗi khi cập nhật khu vực: ' + (response.message || ''));
                            index++;
                            processNext();
                        }
                    },
                    error: function() {
                        GarageApp.showError('Lỗi khi cập nhật khu vực');
                        index++;
                        processNext();
                    }
                });
            } else {
                // Create new zone
                $.ajax({
                    url: '/WarehouseManagement/CreateZone/' + warehouseId,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(zoneData),
                    success: function(response) {
                        if (response.success && response.data) {
                            // Update zone ID for later use (bins may reference this zone)
                            zone.id = response.data.id;
                            index++;
                            processNext();
                        } else {
                            GarageApp.showError('Lỗi khi tạo khu vực: ' + (response.message || ''));
                            index++;
                            processNext();
                        }
                    },
                    error: function() {
                        GarageApp.showError('Lỗi khi tạo khu vực');
                        index++;
                        processNext();
                    }
                });
            }
        };

        processNext();
    },

    saveBinsSequentially: function(prefix, warehouseId, bins, zones, callback) {
        var self = this;
        
        if (bins.length === 0) {
            if (callback) callback();
            return;
        }

        var index = 0;
        var processNext = function() {
            if (index >= bins.length) {
                if (callback) callback();
                return;
            }

            var bin = bins[index];
            var binData = {
                warehouseZoneId: bin.warehouseZoneId || null,
                code: bin.code,
                name: bin.name,
                description: bin.description || '',
                capacity: bin.capacity || null,
                isDefault: bin.isDefault || false,
                isActive: bin.isActive !== false
            };

            // Resolve temp zone index to actual zone ID
            if (bin._tempZoneIndex !== undefined) {
                var zone = zones[bin._tempZoneIndex];
                if (zone && zone.id) {
                    binData.warehouseZoneId = zone.id;
                } else {
                    // Zone chưa có ID, bỏ qua bin này (sẽ báo lỗi)
                    GarageApp.showWarning('Kệ/ngăn "' + bin.name + '" không thể lưu vì khu vực chưa được tạo');
                    index++;
                    processNext();
                    return;
                }
            }

            if (bin.id && prefix === 'edit') {
                // Update existing bin
                $.ajax({
                    url: '/WarehouseManagement/UpdateBin/' + warehouseId + '/' + bin.id,
                    type: 'PUT',
                    contentType: 'application/json',
                    data: JSON.stringify(binData),
                    success: function(response) {
                        if (response.success) {
                            index++;
                            processNext();
                        } else {
                            GarageApp.showError('Lỗi khi cập nhật kệ/ngăn: ' + (response.message || ''));
                            index++;
                            processNext();
                        }
                    },
                    error: function() {
                        GarageApp.showError('Lỗi khi cập nhật kệ/ngăn');
                        index++;
                        processNext();
                    }
                });
            } else {
                // Create new bin
                $.ajax({
                    url: '/WarehouseManagement/CreateBin/' + warehouseId,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(binData),
                    success: function(response) {
                        if (response.success) {
                            index++;
                            processNext();
                        } else {
                            GarageApp.showError('Lỗi khi tạo kệ/ngăn: ' + (response.message || ''));
                            index++;
                            processNext();
                        }
                    },
                    error: function() {
                        GarageApp.showError('Lỗi khi tạo kệ/ngăn');
                        index++;
                        processNext();
                    }
                });
            }
        };

        processNext();
    }
};

// Initialize when document is ready
$(document).ready(function() {
    if ($('#warehouseTable').length > 0) {
        WarehouseManagement.init();
    }
});

