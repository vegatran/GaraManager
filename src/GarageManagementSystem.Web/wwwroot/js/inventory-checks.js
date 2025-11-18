/**
 * Inventory Checks Management Module
 * 
 * Handles all inventory check-related operations
 * CRUD operations for inventory checks
 */

window.InventoryChecks = {
    // DataTable instance
    checksTable: null,
    warehouses: [],
    currentCheckId: null,

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.loadWarehouses(); // This will call populateWarehouseSelects() after loading
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable('#inventoryChecksTable')) {
            $('#inventoryChecksTable').DataTable().destroy();
        }
        
        // Sử dụng DataTablesUtility với style chung
        var columns = [
            { 
                data: 'code', 
                title: 'Mã Phiếu', 
                width: '120px',
                render: function(data, type, row) {
                    if (!data) return '<span class="text-muted">-</span>';
                    var self = window.InventoryChecks;
                    return self ? self.escapeHtml(data) : data;
                }
            },
            { 
                data: 'name', 
                title: 'Tên Phiếu',
                render: function(data, type, row) {
                    if (!data) return '<span class="text-muted">-</span>';
                    var self = window.InventoryChecks;
                    return self ? self.escapeHtml(data) : data;
                }
            },
            { 
                data: 'checkDate', 
                title: 'Ngày Kiểm Kê',
                render: DataTablesUtility.renderDate
            },
            { 
                data: 'warehouseName', 
                title: 'Kho',
                render: function(data, type, row) {
                    // Escape HTML to prevent XSS
                    if (!data) return '<span class="text-muted">-</span>';
                    var self = window.InventoryChecks;
                    return self ? self.escapeHtml(data) : data;
                }
            },
            { 
                data: 'warehouseZoneName', 
                title: 'Khu Vực',
                render: function(data, type, row) {
                    if (!data) return '<span class="text-muted">-</span>';
                    var self = window.InventoryChecks;
                    return self ? self.escapeHtml(data) : data;
                }
            },
            { 
                data: 'warehouseBinName', 
                title: 'Kệ/Ngăn',
                render: function(data, type, row) {
                    if (!data) return '<span class="text-muted">-</span>';
                    var self = window.InventoryChecks;
                    return self ? self.escapeHtml(data) : data;
                }
            },
            { 
                data: 'status', 
                title: 'Trạng Thái',
                render: function(data, type, row) {
                    var badgeClass = 'badge-secondary';
                    var statusText = 'Nháp';
                    switch(data) {
                        case 'Draft': 
                            badgeClass = 'badge-secondary';
                            statusText = 'Nháp';
                            break;
                        case 'InProgress': 
                            badgeClass = 'badge-info';
                            statusText = 'Đang thực hiện';
                            break;
                        case 'Completed': 
                            badgeClass = 'badge-success';
                            statusText = 'Hoàn thành';
                            break;
                        case 'Cancelled': 
                            badgeClass = 'badge-danger';
                            statusText = 'Đã hủy';
                            break;
                    }
                    return `<span class="badge ${badgeClass}">${statusText}</span>`;
                }
            },
            { 
                data: 'items', 
                title: 'Số Items',
                className: 'text-center',
                render: function(data) {
                    return data ? data.length : 0;
                }
            },
            { 
                data: 'startedByEmployeeName', 
                title: 'Người Thực Hiện',
                render: function(data, type, row) {
                    if (!data) return '<span class="text-muted">-</span>';
                    var self = window.InventoryChecks;
                    return self ? self.escapeHtml(data) : data;
                }
            },
            {
                data: null,
                title: 'Thao Tác',
                width: '150px',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    // Ensure ID is a valid number to prevent XSS
                    var id = row.id ? parseInt(row.id, 10) : 0;
                    if (isNaN(id) || id <= 0) {
                        return '';
                    }
                    
                    var actions = `
                        <button class="btn btn-info btn-sm view-check" data-id="${id}" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                    `;
                    
                    if (row.status !== 'Completed' && row.status !== 'Cancelled') {
                        actions += `
                            <button class="btn btn-warning btn-sm edit-check" data-id="${id}" title="Chỉnh sửa">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-danger btn-sm delete-check" data-id="${id}" title="Xóa">
                                <i class="fas fa-trash"></i>
                            </button>
                        `;
                    }
                    
                    if (row.status === 'Completed') {
                        actions += `
                            <button class="btn btn-success btn-sm" disabled title="Đã hoàn thành">
                                <i class="fas fa-check"></i>
                            </button>
                        `;
                    }
                    
                    return actions;
                }
            }
        ];
        
        this.checksTable = DataTablesUtility.initAjaxTable('#inventoryChecksTable', '/InventoryChecks/GetInventoryChecks', columns, {
            order: [[2, 'desc']], // Sort by checkDate descending
            pageLength: 10,
            ajax: {
                dataSrc: function(json) {
                    // Handle response from GetInventoryChecks
                    if (json.data && Array.isArray(json.data)) {
                        return json.data;
                    }
                    return [];
                }
            }
        });
    },

    // Bind events
    bindEvents: function() {
        var self = this;

        // Create check button
        $(document).on('click', '#createCheckBtn', function() {
            self.createCheck();
        });

        // View check
        $(document).on('click', '.view-check', function() {
            var id = parseInt($(this).data('id'), 10);
            if (isNaN(id) || id <= 0) {
                if (window.GarageApp) {
                    GarageApp.showError('ID không hợp lệ');
                }
                return;
            }
            self.viewCheck(id);
        });

        // Edit check
        $(document).on('click', '.edit-check', function() {
            var id = parseInt($(this).data('id'), 10);
            if (isNaN(id) || id <= 0) {
                if (window.GarageApp) {
                    GarageApp.showError('ID không hợp lệ');
                }
                return;
            }
            self.editCheck(id);
        });

        // Delete check
        $(document).on('click', '.delete-check', function() {
            var id = parseInt($(this).data('id'), 10);
            if (isNaN(id) || id <= 0) {
                if (window.GarageApp) {
                    GarageApp.showError('ID không hợp lệ');
                }
                return;
            }
            self.deleteCheck(id);
        });

        // Apply filters
        $(document).on('click', '#applyFiltersBtn', function() {
            self.applyFilters();
        });

        // Export Excel
        $(document).on('click', '#exportExcelBtn', function() {
            self.exportExcel();
        });

        // Export Detail Excel
        $(document).on('click', '#btnExportDetailExcel', function() {
            if (self.currentCheckId) {
                self.exportDetailExcel(self.currentCheckId);
            }
        });

        // Print Check
        $(document).on('click', '#btnPrintCheck', function() {
            if (self.currentCheckId) {
                self.printCheck();
            }
        });

        // Clear filters
        $(document).on('click', '#clearFiltersBtn', function() {
            self.clearFilters();
        });

        // Create form submit
        $(document).on('submit', '#createCheckForm', function(e) {
            e.preventDefault();
            self.submitCreateCheck();
        });

        // Edit form submit
        $(document).on('submit', '#editCheckForm', function(e) {
            e.preventDefault();
            self.submitEditCheck();
        });

        // Auto-generate code button
        $(document).on('click', '#btnAutoGenerateCode', function() {
            self.autoGenerateCode();
        });

        // Warehouse change handlers
        $(document).on('change', '#createWarehouseSelect', function() {
            self.onWarehouseChange('create');
        });
        $(document).on('change', '#editWarehouseSelect', function() {
            self.onWarehouseChange('edit');
        });

        // Zone change handlers
        $(document).on('change', '#createWarehouseZoneSelect', function() {
            self.onWarehouseZoneChange('create');
        });
        $(document).on('change', '#editWarehouseZoneSelect', function() {
            self.onWarehouseZoneChange('edit');
        });

        // Start check button
        $(document).on('click', '#btnStartCheck', function() {
            if (self.currentCheckId) {
                self.startCheck(self.currentCheckId);
            }
        });

        // Complete check button
        $(document).on('click', '#btnCompleteCheck', function() {
            self.completeCheck();
        });

        // Edit from view button
        $(document).on('click', '#btnEditFromView', function() {
            if (self.currentCheckId) {
                $('#viewCheckModal').modal('hide');
                self.editCheck(self.currentCheckId);
            }
        });

        // Add item button
        $(document).on('click', '#btnAddItem', function() {
            if (self.currentCheckId) {
                self.openAddItemModal(self.currentCheckId);
            }
        });

        // Create adjustment button
        $(document).on('click', '#btnCreateAdjustment', function() {
            if (self.currentCheckId) {
                var checkCode = $('#viewCode').text() || '';
                if (window.InventoryAdjustments && window.InventoryAdjustments.createAdjustmentFromCheck) {
                    window.InventoryAdjustments.createAdjustmentFromCheck(self.currentCheckId, checkCode);
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError('Chức năng tạo điều chỉnh chưa sẵn sàng');
                    }
                }
            }
        });

        // Item form submit
        $(document).on('submit', '#itemForm', function(e) {
            e.preventDefault();
            self.submitItem();
        });

        // Item modal cleanup
        $('#itemModal').on('hidden.bs.modal', function() {
            self.resetItemForm();
        });

        // Add comment button (for check comments)
        $(document).on('click', '#addCommentBtn', function() {
            var checkId = self.currentCheckId;
            var commentText = $('#newCommentText').val().trim();
            if (!commentText) {
                if (window.GarageApp) {
                    GarageApp.showError('Vui lòng nhập nội dung ghi chú');
                }
                return;
            }
            if (checkId) {
                self.addCheckComment(checkId, commentText);
            }
        });

        // Delete comment button (for check comments)
        $(document).on('click', '.delete-comment-btn', function() {
            var commentId = parseInt($(this).data('comment-id'), 10);
            if (commentId > 0 && confirm('Bạn có chắc chắn muốn xóa ghi chú này?')) {
                self.deleteCheckComment(commentId);
            }
        });

        // Tab change handler - load data when tab is clicked
        $(document).on('shown.bs.tab', '#viewCheckTabs a[href="#history"]', function() {
            if (self.currentCheckId) {
                self.loadCheckHistory(self.currentCheckId);
            }
        });

        $(document).on('shown.bs.tab', '#viewCheckTabs a[href="#comments"]', function() {
            if (self.currentCheckId) {
                self.loadCheckComments(self.currentCheckId);
            }
        });

        // Modal cleanup
        $('#viewCheckModal').on('hidden.bs.modal', function() {
            self.currentCheckId = null;
            $('#newCommentText').val('');
            // Reset to first tab
            $('#viewCheckTabs a[href="#info"]').tab('show');
        });

        // Auto-calculate discrepancy when actual quantity changes
        $(document).on('input change', '#itemActualQuantity', function() {
            self.calculateDiscrepancy();
        });

        // Edit item button
        $(document).on('click', '.edit-item', function() {
            var itemId = parseInt($(this).data('item-id'), 10);
            if (isNaN(itemId) || itemId <= 0) {
                if (window.GarageApp) {
                    GarageApp.showError('ID item không hợp lệ');
                }
                return;
            }
            if (self.currentCheckId) {
                self.openEditItemModal(self.currentCheckId, itemId);
            }
        });

        // Delete item button
        $(document).on('click', '.delete-item', function() {
            var itemId = parseInt($(this).data('item-id'), 10);
            if (isNaN(itemId) || itemId <= 0) {
                if (window.GarageApp) {
                    GarageApp.showError('ID item không hợp lệ');
                }
                return;
            }
            if (self.currentCheckId) {
                self.deleteItem(self.currentCheckId, itemId);
            }
        });

        // Modal cleanup handlers
        $('#createCheckModal').on('hidden.bs.modal', function() {
            self.resetCreateForm();
            self.currentCheckId = null;
        });

        $('#editCheckModal').on('hidden.bs.modal', function() {
            $('#editCheckForm')[0]?.reset();
            self.currentCheckId = null;
        });

        $('#viewCheckModal').on('hidden.bs.modal', function() {
            self.currentCheckId = null;
            $('#viewItemsTableBody').html('<tr><td colspan="7" class="text-center text-muted">Đang tải...</td></tr>');
        });
    },

    // Load warehouses for filter dropdown
    loadWarehouses: function() {
        var self = this;
        $.ajax({
            url: '/InventoryChecks/Warehouses',
            type: 'GET',
            success: function(response) {
                var warehouses = [];
                if (response && response.success && response.data && Array.isArray(response.data)) {
                    warehouses = response.data;
                } else if (response && Array.isArray(response)) {
                    warehouses = response;
                }
                
                self.warehouses = warehouses;
                
                // Populate filter dropdown
                var $select = $('#filterWarehouse');
                $select.empty().append('<option value="">Tất cả kho</option>');
                warehouses.forEach(function(warehouse) {
                    var warehouseId = warehouse.id || 0;
                    var warehouseName = warehouse.name ? self.escapeHtml(warehouse.name) : '';
                    $select.append(`<option value="${warehouseId}">${warehouseName}</option>`);
                });
                
                // Populate modal selects
                self.populateWarehouseSelects();
            },
            error: function() {
                console.error('Error loading warehouses');
            }
        });
    },

    // Apply filters
    applyFilters: function() {
        var self = this;
        var filters = {
            status: $('#filterStatus').val() || null,
            warehouseId: $('#filterWarehouse').val() || null,
            startDate: $('#filterStartDate').val() || null,
            endDate: $('#filterEndDate').val() || null
        };

        // Build query string
        var queryParams = [];
        if (filters.status) queryParams.push(`status=${encodeURIComponent(filters.status)}`);
        if (filters.warehouseId) queryParams.push(`warehouseId=${filters.warehouseId}`);
        if (filters.startDate) queryParams.push(`startDate=${filters.startDate}`);
        if (filters.endDate) queryParams.push(`endDate=${filters.endDate}`);

        var url = '/InventoryChecks/GetInventoryChecks';
        if (queryParams.length > 0) {
            url += '?' + queryParams.join('&');
        }

        // Reload DataTable with filters
        if (this.checksTable && $.fn.DataTable.isDataTable('#inventoryChecksTable')) {
            this.checksTable.ajax.url(url).load();
        }
    },

    // Clear filters
    clearFilters: function() {
        $('#filterStatus').val('');
        $('#filterWarehouse').val('');
        $('#filterStartDate').val('');
        $('#filterEndDate').val('');
        
        if (this.checksTable && $.fn.DataTable.isDataTable('#inventoryChecksTable')) {
            this.checksTable.ajax.url('/InventoryChecks/GetInventoryChecks').load();
        }
    },

    // Populate warehouse selects in modals
    populateWarehouseSelects: function() {
        var self = this;
        var options = ['<option value="">-- Chọn kho --</option>'];
        this.warehouses.forEach(function(warehouse) {
            var warehouseId = warehouse.id || 0;
            var warehouseName = warehouse.name ? self.escapeHtml(warehouse.name) : '';
            options.push(`<option value="${warehouseId}">${warehouseName}</option>`);
        });
        $('#createWarehouseSelect, #editWarehouseSelect').html(options.join(''));
    },

    // Warehouse change handler
    onWarehouseChange: function(prefix) {
        var warehouseId = $('#' + prefix + 'WarehouseSelect').val();
        var $zoneSelect = $('#' + prefix + 'WarehouseZoneSelect');
        var $binSelect = $('#' + prefix + 'WarehouseBinSelect');

        $zoneSelect.empty().append('<option value="">-- Chọn khu vực --</option>');
        $binSelect.empty().append('<option value="">-- Chọn kệ/ngăn --</option>').prop('disabled', true);

        if (!warehouseId) {
            $zoneSelect.prop('disabled', true);
            return;
        }

        var warehouse = this.warehouses.find(function(w) { return w.id.toString() === warehouseId; });
        if (!warehouse) {
            $zoneSelect.prop('disabled', true);
            return;
        }

        if (warehouse.zones && warehouse.zones.length > 0) {
            warehouse.zones.forEach(function(zone) {
                var zoneId = zone.id || 0;
                var zoneName = zone.name ? self.escapeHtml(zone.name) : '';
                $zoneSelect.append(`<option value="${zoneId}">${zoneName}</option>`);
            });
            $zoneSelect.prop('disabled', false);
        } else {
            $zoneSelect.prop('disabled', true);
        }

        // Load bins directly under warehouse
        if (warehouse.bins && warehouse.bins.length > 0) {
            warehouse.bins.forEach(function(bin) {
                var binId = bin.id || 0;
                var binName = bin.name ? self.escapeHtml(bin.name) : '';
                $binSelect.append(`<option value="${binId}">${binName}</option>`);
            });
            $binSelect.prop('disabled', false);
        }
    },

    // Zone change handler
    onWarehouseZoneChange: function(prefix) {
        var warehouseId = $('#' + prefix + 'WarehouseSelect').val();
        var zoneId = $('#' + prefix + 'WarehouseZoneSelect').val();
        var $binSelect = $('#' + prefix + 'WarehouseBinSelect');

        if (!warehouseId) {
            $binSelect.prop('disabled', true);
            return;
        }

        var warehouse = this.warehouses.find(function(w) { return w.id.toString() === warehouseId; });
        if (!warehouse) {
            $binSelect.prop('disabled', true);
            return;
        }

        // Clear and reload bins
        $binSelect.empty().append('<option value="">-- Chọn kệ/ngăn --</option>');

        var allBins = [];
        var binIds = new Set();

        // Load bins in zone
        if (zoneId) {
            var zone = (warehouse.zones || []).find(function(z) { return z.id.toString() === zoneId; });
            if (zone && zone.bins && zone.bins.length > 0) {
                zone.bins.forEach(function(bin) {
                    if (!binIds.has(bin.id)) {
                        allBins.push(bin);
                        binIds.add(bin.id);
                    }
                });
            }
        }

        // Load bins directly under warehouse
        if (warehouse.bins && warehouse.bins.length > 0) {
            warehouse.bins.forEach(function(bin) {
                if (!binIds.has(bin.id)) {
                    allBins.push(bin);
                    binIds.add(bin.id);
                }
            });
        }

        if (allBins.length > 0) {
            allBins.forEach(function(bin) {
                var binId = bin.id || 0;
                var binName = bin.name ? self.escapeHtml(bin.name) : '';
                $binSelect.append(`<option value="${binId}">${binName}</option>`);
            });
            $binSelect.prop('disabled', false);
        } else {
            $binSelect.prop('disabled', true);
        }
    },

    // Auto-generate code
    autoGenerateCode: function() {
        var year = new Date().getFullYear();
        var prefix = `IK-${year}`;
        // For now, just show placeholder - actual generation will be done by backend
        $('#createCode').val('').attr('placeholder', 'Để trống để tự động tạo: ' + prefix + '-XXX');
    },

    // Create new check
    createCheck: function() {
        this.resetCreateForm();
        $('#createCheckDate').val(new Date().toISOString().split('T')[0]);
        $('#createCheckModal').modal('show');
    },

    // Reset create form
    resetCreateForm: function() {
        $('#createCheckForm')[0].reset();
        $('#createCode').val('').attr('placeholder', 'Để trống để tự động tạo');
        $('#createWarehouseSelect, #createWarehouseZoneSelect, #createWarehouseBinSelect').val('');
        $('#createWarehouseZoneSelect, #createWarehouseBinSelect').prop('disabled', true);
        this.populateWarehouseSelects();
    },

    // Submit create check
    submitCreateCheck: function() {
        var self = this;
        
        // Validation
        var name = $('#createName').val().trim();
        var checkDate = $('#createCheckDate').val();
        if (!name) {
            if (window.GarageApp) {
                GarageApp.showError('Vui lòng nhập tên phiếu kiểm kê');
            }
            return;
        }
        if (!checkDate) {
            if (window.GarageApp) {
                GarageApp.showError('Vui lòng chọn ngày kiểm kê');
            }
            return;
        }

        var code = $('#createCode').val().trim();
        var formData = {
            Code: code || null, // null để backend auto-generate
            Name: name,
            Description: $('#createDescription').val().trim() || null,
            CheckDate: checkDate,
            WarehouseId: this.parseNullableInt($('#createWarehouseSelect').val()),
            WarehouseZoneId: this.parseNullableInt($('#createWarehouseZoneSelect').val()),
            WarehouseBinId: this.parseNullableInt($('#createWarehouseBinSelect').val()),
            Status: $('#createStatus').val() || 'Draft',
            Notes: $('#createNotes').val().trim() || null,
            Items: []
        };

        $.ajax({
            url: '/api/inventorychecks',
            type: 'POST',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify(formData),
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                if (response.success) {
                    if (window.GarageApp) {
                        GarageApp.showSuccess('Tạo phiếu kiểm kê thành công');
                    }
                    $('#createCheckModal').modal('hide');
                    if (self.checksTable) {
                        self.checksTable.ajax.reload(null, false);
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi tạo phiếu kiểm kê');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = 'Lỗi khi tạo phiếu kiểm kê';
                if (xhr.responseJSON) {
                    if (xhr.responseJSON.errorMessage) {
                        errorMsg = xhr.responseJSON.errorMessage;
                    } else if (xhr.responseJSON.message) {
                        errorMsg = xhr.responseJSON.message;
                    } else if (xhr.responseJSON.error) {
                        errorMsg = xhr.responseJSON.error;
                    }
                }
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // View check details
    viewCheck: function(id) {
        var self = this;
        if (!id) {
            if (window.GarageApp) {
                GarageApp.showError('ID không hợp lệ');
            }
            return;
        }
        this.currentCheckId = id;

        $.ajax({
            url: `/api/inventorychecks/${id}`,
            type: 'GET',
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                if (response.success && response.data) {
                    self.populateViewModal(response.data);
                    // Load history and comments when modal opens
                    self.loadCheckHistory(id);
                    self.loadCheckComments(id);
                    $('#viewCheckModal').modal('show');
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Không thể tải thông tin phiếu kiểm kê');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = 'Lỗi khi tải thông tin phiếu kiểm kê';
                if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                    errorMsg = xhr.responseJSON.errorMessage;
                }
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Populate view modal
    populateViewModal: function(data) {
        if (!data) {
            if (window.GarageApp) {
                GarageApp.showError('Dữ liệu không hợp lệ');
            }
            return;
        }

        $('#viewCode').text(data.code || '-');
        $('#viewName').text(data.name || '-');
        
        // Handle date format
        var checkDateText = '-';
        if (data.checkDate) {
            try {
                var date = new Date(data.checkDate);
                if (!isNaN(date.getTime())) {
                    checkDateText = date.toLocaleDateString('vi-VN');
                }
            } catch (e) {
                // Keep default '-'
            }
        }
        $('#viewCheckDate').text(checkDateText);
        
        $('#viewDescription').text(data.description || '-');
        $('#viewNotes').text(data.notes || '-');
        $('#viewWarehouse').text(data.warehouseName || '-');
        $('#viewWarehouseZone').text(data.warehouseZoneName || '-');
        $('#viewWarehouseBin').text(data.warehouseBinName || '-');
        $('#viewStartedBy').text(data.startedByEmployeeName || '-');
        $('#viewCompletedBy').text(data.completedByEmployeeName || '-');

        // Status badge
        var statusBadge = this.getStatusBadge(data.status);
        $('#viewStatus').html(statusBadge);

        // Statistics
        var items = data.items || [];
        var totalItems = items.length;
        var discrepancyItems = items.filter(function(item) { return item.isDiscrepancy; }).length;
        var shortageItems = items.filter(function(item) { return item.discrepancyQuantity < 0; }).length;
        var surplusItems = items.filter(function(item) { return item.discrepancyQuantity > 0; }).length;

        $('#viewTotalItems').text(totalItems);
        $('#viewDiscrepancyItems').text(discrepancyItems);
        $('#viewShortageItems').text(shortageItems);
        $('#viewSurplusItems').text(surplusItems);

        // Items table - pass check status for button visibility
        this.renderViewItemsTable(items, data.status);

        // Show/hide buttons based on status
        var canStart = data.status === 'Draft';
        var canEdit = data.status !== 'Completed' && data.status !== 'Cancelled';
        var canComplete = data.status === 'InProgress';
        var canCreateAdjustment = data.status === 'Completed' && discrepancyItems > 0;
        $('#btnStartCheck').toggle(canStart);
        $('#btnEditFromView').toggle(canEdit);
        $('#btnCompleteCheck').toggle(canComplete);
        $('#btnAddItem').toggle(canEdit);
        $('#btnCreateAdjustment').toggle(canCreateAdjustment);
    },

    // Render view items table
    renderViewItemsTable: function(items, checkStatus) {
        var self = this;
        var tbody = $('#viewItemsTableBody');
        
        if (!items || items.length === 0) {
            tbody.html('<tr><td colspan="7" class="text-center text-muted">Chưa có items</td></tr>');
            return;
        }

        var canEdit = checkStatus !== 'Completed' && checkStatus !== 'Cancelled';
        var checkId = self.currentCheckId;

        var rows = items.map(function(item) {
            if (!item) return '';
            
            var discrepancyClass = '';
            var discrepancyText = '';
            var discrepancyQty = item.discrepancyQuantity || 0;
            if (discrepancyQty > 0) {
                discrepancyClass = 'text-success';
                discrepancyText = '+' + discrepancyQty;
            } else if (discrepancyQty < 0) {
                discrepancyClass = 'text-danger';
                discrepancyText = discrepancyQty.toString();
            } else {
                discrepancyClass = 'text-muted';
                discrepancyText = '0';
            }

            // Escape HTML to prevent XSS
            var partName = self.escapeHtml(item.partName || item.partNumber || '-');
            var partSku = item.partSku ? self.escapeHtml(item.partSku) : '<span class="text-muted">-</span>';
            var notes = item.notes ? self.escapeHtml(item.notes) : '<span class="text-muted">-</span>';
            var itemId = item.id || 0;
            var systemQty = item.systemQuantity || 0;
            var actualQty = item.actualQuantity || 0;

            var actionButtons = '';
            if (canEdit && checkId) {
                actionButtons = `
                    <button class="btn btn-sm btn-warning edit-item" data-item-id="${itemId}" title="Sửa">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn btn-sm btn-danger delete-item" data-item-id="${itemId}" title="Xóa">
                        <i class="fas fa-trash"></i>
                    </button>
                `;
            }

            return `
                <tr>
                    <td>${partName}</td>
                    <td>${partSku}</td>
                    <td class="text-center">${systemQty}</td>
                    <td class="text-center">${actualQty}</td>
                    <td class="text-center ${discrepancyClass}"><strong>${discrepancyText}</strong></td>
                    <td>${notes}</td>
                    <td class="text-center">${actionButtons}</td>
                </tr>
            `;
        }).join('');

        tbody.html(rows);
    },

    // Get status badge HTML
    getStatusBadge: function(status) {
        var badgeClass = 'badge-secondary';
        var statusText = 'Nháp';
        switch(status) {
            case 'Draft': 
                badgeClass = 'badge-secondary';
                statusText = 'Nháp';
                break;
            case 'InProgress': 
                badgeClass = 'badge-info';
                statusText = 'Đang thực hiện';
                break;
            case 'Completed': 
                badgeClass = 'badge-success';
                statusText = 'Hoàn thành';
                break;
            case 'Cancelled': 
                badgeClass = 'badge-danger';
                statusText = 'Đã hủy';
                break;
        }
        return `<span class="badge ${badgeClass}">${statusText}</span>`;
    },

    // Edit check
    editCheck: function(id) {
        var self = this;
        if (!id) {
            if (window.GarageApp) {
                GarageApp.showError('ID không hợp lệ');
            }
            return;
        }
        this.currentCheckId = id;

        $.ajax({
            url: `/api/inventorychecks/${id}`,
            type: 'GET',
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                if (response.success && response.data) {
                    self.populateEditForm(response.data);
                    $('#editCheckModal').modal('show');
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Không thể tải thông tin phiếu kiểm kê');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = 'Lỗi khi tải thông tin phiếu kiểm kê';
                if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                    errorMsg = xhr.responseJSON.errorMessage;
                }
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Populate edit form
    populateEditForm: function(data) {
        if (!data) {
            if (window.GarageApp) {
                GarageApp.showError('Dữ liệu không hợp lệ');
            }
            return;
        }

        $('#editId').val(data.id || '');
        $('#editCode').val(data.code || '');
        $('#editName').val(data.name || '');
        $('#editDescription').val(data.description || '');
        
        // Handle date format - support both ISO string and Date object
        var checkDate = '';
        if (data.checkDate) {
            if (typeof data.checkDate === 'string') {
                checkDate = data.checkDate.split('T')[0];
            } else if (data.checkDate instanceof Date) {
                checkDate = data.checkDate.toISOString().split('T')[0];
            }
        }
        $('#editCheckDate').val(checkDate);
        
        $('#editStatus').val(data.status || 'Draft');
        $('#editNotes').val(data.notes || '');

        // Warehouse hierarchy - convert to string for select matching
        // Wait for warehouses to load first
        var self = this;
        var warehouseId = data.warehouseId ? data.warehouseId.toString() : '';
        var zoneId = data.warehouseZoneId ? data.warehouseZoneId.toString() : '';
        var binId = data.warehouseBinId ? data.warehouseBinId.toString() : '';

        // Check if warehouses are loaded
        if (this.warehouses.length === 0) {
            // Wait for warehouses to load (max 10 attempts = 1 second)
            var attempts = 0;
            var maxAttempts = 10;
            var checkInterval = setInterval(function() {
                attempts++;
                if (self.warehouses.length > 0 || attempts >= maxAttempts) {
                    clearInterval(checkInterval);
                    if (self.warehouses.length === 0) {
                        console.warn('Warehouses not loaded after ' + maxAttempts + ' attempts, populating form without warehouse data');
                    }
                    self.populateEditFormWarehouse(data, warehouseId, zoneId, binId);
                }
            }, 100);
        } else {
            this.populateEditFormWarehouse(data, warehouseId, zoneId, binId);
        }
    },

    // Helper: Populate warehouse fields in edit form
    populateEditFormWarehouse: function(data, warehouseId, zoneId, binId) {
        $('#editWarehouseSelect').val(warehouseId);
        this.onWarehouseChange('edit');
        
        // Wait a bit for zones to load, then set zone
        var self = this;
        setTimeout(function() {
            $('#editWarehouseZoneSelect').val(zoneId);
            self.onWarehouseZoneChange('edit');
            
            // Wait a bit more for bins to load, then set bin
            setTimeout(function() {
                $('#editWarehouseBinSelect').val(binId);
            }, 150);
        }, 150);
    },

    // Submit edit check
    submitEditCheck: function() {
        var self = this;
        var id = parseInt($('#editId').val());
        if (!id || isNaN(id)) {
            if (window.GarageApp) {
                GarageApp.showError('ID không hợp lệ');
            }
            return;
        }

        // Validation
        var name = $('#editName').val().trim();
        var code = $('#editCode').val().trim();
        var checkDate = $('#editCheckDate').val();
        if (!name) {
            if (window.GarageApp) {
                GarageApp.showError('Vui lòng nhập tên phiếu kiểm kê');
            }
            return;
        }
        if (!code) {
            if (window.GarageApp) {
                GarageApp.showError('Vui lòng nhập mã phiếu kiểm kê');
            }
            return;
        }
        if (!checkDate) {
            if (window.GarageApp) {
                GarageApp.showError('Vui lòng chọn ngày kiểm kê');
            }
            return;
        }

        var formData = {
            Id: id,
            Code: code,
            Name: name,
            Description: $('#editDescription').val().trim() || null,
            CheckDate: checkDate,
            WarehouseId: this.parseNullableInt($('#editWarehouseSelect').val()),
            WarehouseZoneId: this.parseNullableInt($('#editWarehouseZoneSelect').val()),
            WarehouseBinId: this.parseNullableInt($('#editWarehouseBinSelect').val()),
            Status: $('#editStatus').val() || 'Draft',
            Notes: $('#editNotes').val().trim() || null,
            Items: []
        };

        $.ajax({
            url: `/api/inventorychecks/${id}`,
            type: 'PUT',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify(formData),
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                if (response.success) {
                    if (window.GarageApp) {
                        GarageApp.showSuccess('Cập nhật phiếu kiểm kê thành công');
                    }
                    $('#editCheckModal').modal('hide');
                    if (self.checksTable) {
                        self.checksTable.ajax.reload(null, false);
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi cập nhật phiếu kiểm kê');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = 'Lỗi khi cập nhật phiếu kiểm kê';
                if (xhr.responseJSON) {
                    if (xhr.responseJSON.errorMessage) {
                        errorMsg = xhr.responseJSON.errorMessage;
                    } else if (xhr.responseJSON.message) {
                        errorMsg = xhr.responseJSON.message;
                    } else if (xhr.responseJSON.error) {
                        errorMsg = xhr.responseJSON.error;
                    }
                }
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Delete check
    deleteCheck: function(id) {
        var self = this;
        if (!id) {
            if (window.GarageApp) {
                GarageApp.showError('ID không hợp lệ');
            }
            return;
        }

        // Use SweetAlert2 if available, otherwise use confirm
        if (window.Swal && window.Swal.fire) {
            Swal.fire({
                title: 'Bạn có chắc chắn?',
                text: 'Bạn có chắc chắn muốn xóa phiếu kiểm kê này? Hành động này không thể hoàn tác!',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Có, xóa!',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    self.performDeleteCheck(id);
                }
            });
        } else {
            if (!confirm('Bạn có chắc chắn muốn xóa phiếu kiểm kê này?')) {
                return;
            }
            this.performDeleteCheck(id);
        }
    },

    // Perform delete check
    performDeleteCheck: function(id) {
        var self = this;
        $.ajax({
            url: `/api/inventorychecks/${id}`,
            type: 'DELETE',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                if (response.success) {
                    if (window.GarageApp) {
                        GarageApp.showSuccess('Xóa phiếu kiểm kê thành công');
                    }
                    
                    // Reload DataTable
                    if (self.checksTable && $.fn.DataTable.isDataTable('#inventoryChecksTable')) {
                        self.checksTable.ajax.reload(null, false);
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi xóa phiếu kiểm kê');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = 'Lỗi khi xóa phiếu kiểm kê';
                if (xhr.responseJSON) {
                    if (xhr.responseJSON.errorMessage) {
                        errorMsg = xhr.responseJSON.errorMessage;
                    } else if (xhr.responseJSON.message) {
                        errorMsg = xhr.responseJSON.message;
                    } else if (xhr.responseJSON.error) {
                        errorMsg = xhr.responseJSON.error;
                    }
                }
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Start check (change status from Draft to InProgress)
    startCheck: function(id) {
        var self = this;
        if (!id) {
            if (window.GarageApp) {
                GarageApp.showError('ID không hợp lệ');
            }
            return;
        }

        // Load current data first, then update status
        $.ajax({
            url: `/api/inventorychecks/${id}`,
            type: 'GET',
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                if (response.success && response.data) {
                    var data = response.data;
                    
                    // Prepare update data with current values
                    var formData = {
                        Id: data.id,
                        Code: data.code,
                        Name: data.name,
                        Description: data.description || null,
                        CheckDate: data.checkDate,
                        WarehouseId: data.warehouseId || null,
                        WarehouseZoneId: data.warehouseZoneId || null,
                        WarehouseBinId: data.warehouseBinId || null,
                        Status: 'InProgress',
                        Notes: data.notes || null,
                        Items: []
                    };

                    // Update via PUT
                    $.ajax({
                        url: `/api/inventorychecks/${id}`,
                        type: 'PUT',
                        contentType: 'application/json',
                        headers: {
                            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        data: JSON.stringify(formData),
                        success: function(updateResponse) {
                            if (window.AuthHandler && !AuthHandler.validateApiResponse(updateResponse)) {
                                return;
                            }
                            if (updateResponse.success) {
                                if (window.GarageApp) {
                                    GarageApp.showSuccess('Bắt đầu kiểm kê thành công');
                                }
                                
                                // Reload view modal if it's open
                                if ($('#viewCheckModal').is(':visible') && self.currentCheckId == id) {
                                    self.viewCheck(id);
                                }
                                
                                // Reload DataTable
                                if (self.checksTable && $.fn.DataTable.isDataTable('#inventoryChecksTable')) {
                                    self.checksTable.ajax.reload(null, false);
                                }
                            } else {
                                if (window.GarageApp) {
                                    GarageApp.showError(updateResponse.errorMessage || 'Lỗi khi bắt đầu kiểm kê');
                                }
                            }
                        },
                        error: function(xhr) {
                            if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                                AuthHandler.handleUnauthorized(xhr, true);
                                return;
                            }
                            var errorMsg = 'Lỗi khi bắt đầu kiểm kê';
                            if (xhr.responseJSON) {
                                if (xhr.responseJSON.errorMessage) {
                                    errorMsg = xhr.responseJSON.errorMessage;
                                } else if (xhr.responseJSON.message) {
                                    errorMsg = xhr.responseJSON.message;
                                } else if (xhr.responseJSON.error) {
                                    errorMsg = xhr.responseJSON.error;
                                }
                            }
                            if (window.GarageApp) {
                                GarageApp.showError(errorMsg);
                            }
                        }
                    });
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError('Không thể tải thông tin phiếu kiểm kê');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                if (window.GarageApp) {
                    GarageApp.showError('Lỗi khi tải thông tin phiếu kiểm kê');
                }
            }
        });
    },

    // Complete check
    completeCheck: function() {
        var self = this;
        if (!this.currentCheckId) {
            if (window.GarageApp) {
                GarageApp.showError('ID không hợp lệ');
            }
            return;
        }

        if (!confirm('Bạn có chắc chắn muốn hoàn thành phiếu kiểm kê này?')) {
            return;
        }

        $.ajax({
            url: `/api/inventorychecks/${this.currentCheckId}/complete`,
            type: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                if (response.success) {
                    if (window.GarageApp) {
                        GarageApp.showSuccess('Hoàn thành phiếu kiểm kê thành công');
                    }
                    $('#viewCheckModal').modal('hide');
                    if (self.checksTable) {
                        self.checksTable.ajax.reload(null, false);
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi hoàn thành phiếu kiểm kê');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = 'Lỗi khi hoàn thành phiếu kiểm kê';
                if (xhr.responseJSON) {
                    if (xhr.responseJSON.errorMessage) {
                        errorMsg = xhr.responseJSON.errorMessage;
                    } else if (xhr.responseJSON.message) {
                        errorMsg = xhr.responseJSON.message;
                    } else if (xhr.responseJSON.error) {
                        errorMsg = xhr.responseJSON.error;
                    }
                }
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Helper: Parse nullable int from string
    parseNullableInt: function(value) {
        if (!value || value === '' || value === '0') {
            return null;
        }
        var parsed = parseInt(value, 10);
        return isNaN(parsed) ? null : parsed;
    },

    // Helper: Escape HTML to prevent XSS
    escapeHtml: function(text) {
        if (!text) return '';
        var map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return String(text).replace(/[&<>"']/g, function(m) { return map[m]; });
    },

    // ========== ITEM MANAGEMENT ==========

    // Open add item modal
    openAddItemModal: function(checkId) {
        if (!checkId) {
            if (window.GarageApp) {
                GarageApp.showError('ID phiếu kiểm kê không hợp lệ');
            }
            return;
        }

        this.resetItemForm();
        $('#itemInventoryCheckId').val(checkId);
        $('#itemModalTitle').text('Thêm Item');
        
        // Initialize typeahead before showing modal to avoid race condition
        var self = this;
        this.initPartTypeahead();
        
        $('#itemModal').modal('show');
    },

    // Open edit item modal
    openEditItemModal: function(checkId, itemId) {
        var self = this;
        if (!checkId || !itemId) {
            if (window.GarageApp) {
                GarageApp.showError('ID không hợp lệ');
            }
            return;
        }

        // Reset form first
        this.resetItemForm();

        // Load item data
        $.ajax({
            url: `/api/inventorychecks/${checkId}`,
            type: 'GET',
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                if (response.success && response.data && response.data.items) {
                    var item = response.data.items.find(function(i) { return i.id === itemId; });
                    if (item) {
                        self.populateItemForm(item, checkId);
                        $('#itemModalTitle').text('Sửa Item');
                        
                        // Initialize typeahead before showing modal
                        self.initPartTypeahead();
                        
                        $('#itemModal').modal('show');
                    } else {
                        if (window.GarageApp) {
                            GarageApp.showError('Không tìm thấy item');
                        }
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError('Không thể tải thông tin item');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                if (window.GarageApp) {
                    GarageApp.showError('Lỗi khi tải thông tin item');
                }
            }
        });
    },

    // Initialize part typeahead
    initPartTypeahead: function() {
        var self = this;
        var $input = $('#itemPartSearch');
        
        // Destroy existing typeahead if any
        try {
            if ($input.data('typeahead')) {
                $input.typeahead('destroy');
            }
        } catch (e) {
            // Ignore destroy errors
            console.warn('Error destroying typeahead:', e);
        }

        // Store current AJAX request to cancel if needed
        var currentRequest = null;

        $input.typeahead({
            source: function(query, process) {
                // Cancel previous request if still pending
                if (currentRequest && currentRequest.readyState !== 4) {
                    currentRequest.abort();
                }

                if (!query || query.length < 2) {
                    process([]);
                    return;
                }

                currentRequest = $.ajax({
                    url: '/PartsManagement/SearchParts',
                    type: 'GET',
                    data: { searchTerm: query },
                    success: function(response) {
                        var results = [];
                        if (response && response.success && response.data && Array.isArray(response.data)) {
                            response.data.forEach(function(part) {
                                if (part && (part.id || part.partId)) {
                                    results.push({
                                        id: part.id || part.partId,
                                        partId: part.partId || part.id,
                                        partName: part.partName || '',
                                        partNumber: part.partNumber || '',
                                        partSku: part.partSku || '',
                                        quantityInStock: parseInt(part.quantityInStock || 0, 10),
                                        text: part.text || `${part.partName || ''} (${part.partNumber || ''})`
                                    });
                                }
                            });
                        }
                        process(results);
                    },
                    error: function(xhr) {
                        // Only show error if not aborted
                        if (xhr.statusText !== 'abort') {
                            console.warn('Error searching parts:', xhr);
                        }
                        process([]);
                    }
                });
            },
            displayText: function(item) {
                return item ? (item.text || '') : '';
            },
            afterSelect: function(item) {
                if (item) {
                    self.selectPart(item);
                }
            }
        });
    },

    // Select part from typeahead
    selectPart: function(part) {
        if (!part || (!part.partId && !part.id)) {
            if (window.GarageApp) {
                GarageApp.showError('Phụ tùng không hợp lệ');
            }
            return;
        }

        var partId = part.partId || part.id;
        var partNumber = part.partNumber || '-';
        var partName = part.partName || '-';
        var partSku = part.partSku || '-';
        var quantityInStock = parseInt(part.quantityInStock || 0, 10);

        $('#itemPartId').val(partId);
        $('#itemPartNumber').text(this.escapeHtml(partNumber));
        $('#itemPartName').text(this.escapeHtml(partName));
        $('#itemPartSku').text(this.escapeHtml(partSku));
        $('#itemPartStock').text(quantityInStock);
        $('#itemSystemQuantity').val(quantityInStock);
        $('#itemPartInfo').show();
        
        // Recalculate discrepancy
        this.calculateDiscrepancy();
    },

    // Calculate discrepancy
    calculateDiscrepancy: function() {
        var systemQty = parseInt($('#itemSystemQuantity').val() || 0, 10);
        var actualQty = parseInt($('#itemActualQuantity').val() || 0, 10);
        var discrepancy = actualQty - systemQty;
        
        $('#itemDiscrepancyQuantity').val(discrepancy);
        
        // Update color based on discrepancy
        var $discrepancyInput = $('#itemDiscrepancyQuantity');
        $discrepancyInput.removeClass('text-success text-danger text-muted');
        if (discrepancy > 0) {
            $discrepancyInput.addClass('text-success');
        } else if (discrepancy < 0) {
            $discrepancyInput.addClass('text-danger');
        } else {
            $discrepancyInput.addClass('text-muted');
        }
    },

    // Populate item form for editing
    populateItemForm: function(item, checkId) {
        $('#itemId').val(item.id || '');
        $('#itemInventoryCheckId').val(checkId);
        $('#itemPartId').val(item.partId || '');
        $('#itemPartSearch').val(item.partName || item.partNumber || '');
        $('#itemPartNumber').text(item.partNumber || '-');
        $('#itemPartName').text(item.partName || '-');
        $('#itemPartSku').text(item.partSku || '-');
        $('#itemSystemQuantity').val(item.systemQuantity || 0);
        $('#itemActualQuantity').val(item.actualQuantity || 0);
        $('#itemDiscrepancyQuantity').val(item.discrepancyQuantity || 0);
        $('#itemNotes').val(item.notes || '');
        $('#itemPartInfo').show();
        
        // Recalculate to update colors
        this.calculateDiscrepancy();
    },

    // Reset item form
    resetItemForm: function() {
        if ($('#itemForm').length > 0 && $('#itemForm')[0]) {
            $('#itemForm')[0].reset();
        }
        $('#itemId').val('');
        $('#itemInventoryCheckId').val('');
        $('#itemPartId').val('');
        $('#itemPartInfo').hide();
        $('#itemSystemQuantity').val('');
        $('#itemActualQuantity').val('');
        $('#itemDiscrepancyQuantity').val('');
        $('#itemNotes').val('');
        
        // Clear part info display
        $('#itemPartNumber').text('-');
        $('#itemPartName').text('-');
        $('#itemPartSku').text('-');
        $('#itemPartStock').text('-');
        
        // Destroy typeahead safely
        var $input = $('#itemPartSearch');
        if ($input.length > 0) {
            try {
                if ($input.data('typeahead')) {
                    $input.typeahead('destroy');
                }
            } catch (e) {
                console.warn('Error destroying typeahead:', e);
            }
        }
    },

    // Submit item (add or update)
    submitItem: function() {
        var self = this;
        var itemId = $('#itemId').val();
        var checkId = $('#itemInventoryCheckId').val();
        var partId = $('#itemPartId').val();
        var actualQuantityStr = $('#itemActualQuantity').val();
        var actualQuantity = parseInt(actualQuantityStr || 0, 10);

        // Validation
        if (!checkId) {
            if (window.GarageApp) {
                GarageApp.showError('ID phiếu kiểm kê không hợp lệ');
            }
            return;
        }
        var checkIdNum = parseInt(checkId, 10);
        if (isNaN(checkIdNum) || checkIdNum <= 0) {
            if (window.GarageApp) {
                GarageApp.showError('ID phiếu kiểm kê không hợp lệ');
            }
            return;
        }

        if (!partId) {
            if (window.GarageApp) {
                GarageApp.showError('Vui lòng chọn phụ tùng');
            }
            $('#itemPartSearch').focus();
            return;
        }
        var partIdNum = parseInt(partId, 10);
        if (isNaN(partIdNum) || partIdNum <= 0) {
            if (window.GarageApp) {
                GarageApp.showError('Phụ tùng không hợp lệ');
            }
            $('#itemPartSearch').focus();
            return;
        }

        if (actualQuantityStr === '' || isNaN(actualQuantity)) {
            if (window.GarageApp) {
                GarageApp.showError('Vui lòng nhập số lượng thực tế');
            }
            $('#itemActualQuantity').focus();
            return;
        }
        if (actualQuantity < 0) {
            if (window.GarageApp) {
                GarageApp.showError('Số lượng thực tế phải lớn hơn hoặc bằng 0');
            }
            $('#itemActualQuantity').focus();
            return;
        }

        var formData = {
            PartId: partIdNum,
            ActualQuantity: actualQuantity,
            Notes: $('#itemNotes').val().trim() || null
        };

        var url, method;
        if (itemId) {
            // Update
            var itemIdNum = parseInt(itemId, 10);
            if (isNaN(itemIdNum) || itemIdNum <= 0) {
                if (window.GarageApp) {
                    GarageApp.showError('ID item không hợp lệ');
                }
                return;
            }
            formData.Id = itemIdNum;
            url = `/api/inventorychecks/${checkIdNum}/items/${itemIdNum}`;
            method = 'PUT';
        } else {
            // Create
            url = `/api/inventorychecks/${checkIdNum}/items`;
            method = 'POST';
        }

        $.ajax({
            url: url,
            type: method,
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify(formData),
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                if (response.success) {
                    if (window.GarageApp) {
                        GarageApp.showSuccess(itemId ? 'Cập nhật item thành công' : 'Thêm item thành công');
                    }
                    $('#itemModal').modal('hide');
                    
                    // Reload view modal if it's open
                    if ($('#viewCheckModal').is(':visible') && self.currentCheckId == checkIdNum) {
                        self.viewCheck(checkIdNum);
                    }
                    
                    // Reload DataTable to update item count
                    if (self.checksTable && $.fn.DataTable.isDataTable('#inventoryChecksTable')) {
                        self.checksTable.ajax.reload(null, false);
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi lưu item');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = 'Lỗi khi lưu item';
                if (xhr.responseJSON) {
                    if (xhr.responseJSON.errorMessage) {
                        errorMsg = xhr.responseJSON.errorMessage;
                    } else if (xhr.responseJSON.message) {
                        errorMsg = xhr.responseJSON.message;
                    } else if (xhr.responseJSON.error) {
                        errorMsg = xhr.responseJSON.error;
                    }
                }
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Delete item
    deleteItem: function(checkId, itemId) {
        var self = this;
        if (!checkId || !itemId) {
            if (window.GarageApp) {
                GarageApp.showError('ID không hợp lệ');
            }
            return;
        }

        if (!confirm('Bạn có chắc chắn muốn xóa item này?')) {
            return;
        }

        $.ajax({
            url: `/api/inventorychecks/${checkId}/items/${itemId}`,
            type: 'DELETE',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                if (response.success) {
                    if (window.GarageApp) {
                        GarageApp.showSuccess('Xóa item thành công');
                    }
                    
                    // Reload view modal if it's open
                    var checkIdNum = parseInt(checkId, 10);
                    if ($('#viewCheckModal').is(':visible') && self.currentCheckId == checkIdNum) {
                        self.viewCheck(checkIdNum);
                    }
                    
                    // Reload DataTable to update item count
                    if (self.checksTable && $.fn.DataTable.isDataTable('#inventoryChecksTable')) {
                        self.checksTable.ajax.reload(null, false);
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi xóa item');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = 'Lỗi khi xóa item';
                if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                    errorMsg = xhr.responseJSON.errorMessage;
                }
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Export Excel - danh sách phiếu kiểm kê
    exportExcel: function() {
        var self = this;
        
        // Get current filters
        var warehouseId = $('#filterWarehouse').val() || '';
        var status = $('#filterStatus').val() || '';
        var startDate = $('#filterStartDate').val() || '';
        var endDate = $('#filterEndDate').val() || '';

        // Build query string
        var queryParams = [];
        if (warehouseId) queryParams.push('warehouseId=' + encodeURIComponent(warehouseId));
        if (status) queryParams.push('status=' + encodeURIComponent(status));
        if (startDate) queryParams.push('startDate=' + encodeURIComponent(startDate));
        if (endDate) queryParams.push('endDate=' + encodeURIComponent(endDate));

        var url = '/InventoryChecks/ExportExcel';
        if (queryParams.length > 0) {
            url += '?' + queryParams.join('&');
        }

        // Open in new window to trigger download
        window.location.href = url;
    },

    // Export Detail Excel - chi tiết phiếu kiểm kê
    exportDetailExcel: function(id) {
        if (!id) {
            if (window.GarageApp) {
                GarageApp.showError('ID không hợp lệ');
            }
            return;
        }

        var url = '/InventoryChecks/ExportDetailExcel/' + id;
        window.location.href = url;
    },

    // Print Check
    printCheck: function() {
        if (!this.currentCheckId) {
            if (window.GarageApp) {
                GarageApp.showError('ID không hợp lệ');
            }
            return;
        }

        // Open print window
        var printWindow = window.open('', '_blank');
        if (!printWindow) {
            if (window.GarageApp) {
                GarageApp.showError('Không thể mở cửa sổ in. Vui lòng cho phép popup.');
            }
            return;
        }

        // Get modal content
        var modalContent = $('#viewCheckModal .modal-content').clone();
        
        // Remove buttons from print
        modalContent.find('.modal-footer').remove();
        modalContent.find('.modal-header .close').remove();
        
        // Build HTML
        var html = '<!DOCTYPE html><html><head>';
        html += '<title>Phiếu Kiểm Kê</title>';
        html += '<style>';
        html += 'body { font-family: Arial, sans-serif; padding: 20px; }';
        html += 'h3 { margin-top: 0; }';
        html += 'table { width: 100%; border-collapse: collapse; margin-top: 10px; }';
        html += 'table th, table td { border: 1px solid #ddd; padding: 8px; text-align: left; }';
        html += 'table th { background-color: #f2f2f2; font-weight: bold; }';
        html += '.text-danger { color: #dc3545; }';
        html += '.text-success { color: #28a745; }';
        html += '.text-warning { color: #ffc107; }';
        html += '.badge { padding: 4px 8px; border-radius: 4px; }';
        html += '.badge-success { background-color: #28a745; color: white; }';
        html += '.badge-warning { background-color: #ffc107; color: black; }';
        html += '.badge-danger { background-color: #dc3545; color: white; }';
        html += '.badge-info { background-color: #17a2b8; color: white; }';
        html += '@media print { .no-print { display: none; } }';
        html += '</style>';
        html += '</head><body>';
        html += modalContent.html();
        html += '</body></html>';

        printWindow.document.write(html);
        printWindow.document.close();
        
        // Wait for content to load, then print
        setTimeout(function() {
            printWindow.print();
        }, 250);
    },

    // Load check history
    loadCheckHistory: function(checkId) {
        var self = this;
        var $tbody = $('#historyTableBody');
        
        // Check if we're in the view modal
        if ($tbody.length === 0) {
            return;
        }
        
        $.ajax({
            url: `/api/inventorychecks/${checkId}/history`,
            type: 'GET',
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                
                $tbody.empty();
                
                if (response.success && response.data && Array.isArray(response.data) && response.data.length > 0) {
                    response.data.forEach(function(log) {
                        var actionBadge = self.getActionBadge(log.action);
                        var timestamp = log.timestamp ? new Date(log.timestamp).toLocaleString('vi-VN') : '-';
                        var user = log.userName || log.user || '-';
                        var details = log.details || '-';
                        
                        var row = `
                            <tr>
                                <td>${timestamp}</td>
                                <td>${actionBadge}</td>
                                <td>${self.escapeHtml(user)}</td>
                                <td>${self.escapeHtml(details)}</td>
                            </tr>
                        `;
                        $tbody.append(row);
                    });
                } else {
                    $tbody.append('<tr><td colspan="4" class="text-center text-muted">Không có lịch sử</td></tr>');
                }
            },
            error: function(xhr) {
                $tbody.empty();
                $tbody.append('<tr><td colspan="4" class="text-center text-danger">Lỗi khi tải lịch sử</td></tr>');
            }
        });
    },

    // Load check comments
    loadCheckComments: function(checkId) {
        var self = this;
        var $timeline = $('#commentsTimeline');
        
        // Check if we're in the view modal
        if ($timeline.length === 0) {
            return;
        }
        
        $.ajax({
            url: `/api/inventorychecks/${checkId}/comments`,
            type: 'GET',
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                
                $timeline.empty();
                
                if (response.success && response.data && Array.isArray(response.data) && response.data.length > 0) {
                    // Update comments count
                    $('#commentsCount').text(response.data.length);
                    
                    response.data.forEach(function(comment) {
                        var commentHtml = self.renderCheckComment(comment);
                        $timeline.append(commentHtml);
                    });
                } else {
                    $('#commentsCount').text('0');
                    $timeline.append('<div class="text-center text-muted py-3">Chưa có ghi chú nào</div>');
                }
            },
            error: function(xhr) {
                $timeline.empty();
                $timeline.append('<div class="text-center text-danger py-3">Lỗi khi tải ghi chú</div>');
            }
        });
    },

    // Render comment HTML for check
    renderCheckComment: function(comment) {
        var self = this;
        var timestamp = comment.createdAt ? new Date(comment.createdAt).toLocaleString('vi-VN') : '-';
        var userName = comment.createdByEmployeeName || comment.createdBy || 'Unknown';
        var commentText = comment.comment || '';
        var commentId = comment.id || 0;
        
        // Check if current user can delete (simplified - you may want to check permissions)
        var canDelete = true; // TODO: Check user permissions
        
        var deleteBtn = canDelete ? `
            <button type="button" class="btn btn-sm btn-danger delete-comment-btn" data-comment-id="${commentId}" title="Xóa">
                <i class="fas fa-trash"></i>
            </button>
        ` : '';
        
        return `
            <div class="timeline-item mb-3 p-3 border rounded">
                <div class="d-flex justify-content-between align-items-start">
                    <div class="flex-grow-1">
                        <div class="d-flex align-items-center mb-2">
                            <i class="fas fa-user-circle text-primary mr-2"></i>
                            <strong>${self.escapeHtml(userName)}</strong>
                            <span class="text-muted ml-2 small">${timestamp}</span>
                        </div>
                        <div class="ml-4">
                            ${self.escapeHtml(commentText).replace(/\n/g, '<br>')}
                        </div>
                    </div>
                    <div>
                        ${deleteBtn}
                    </div>
                </div>
            </div>
        `;
    },

    // Add comment for check
    addCheckComment: function(checkId, commentText) {
        var self = this;
        var $btn = $('#addCommentBtn');
        var originalText = $btn.html();
        
        $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang thêm...');

        $.ajax({
            url: `/api/inventorychecks/${checkId}/comments`,
            type: 'POST',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify({
                comment: commentText
            }),
            success: function(response) {
                $btn.prop('disabled', false).html(originalText);
                
                if (response.success) {
                    $('#newCommentText').val('');
                    // Reload comments
                    self.loadCheckComments(checkId);
                    if (window.GarageApp) {
                        GarageApp.showSuccess('Đã thêm ghi chú thành công');
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi thêm ghi chú');
                    }
                }
            },
            error: function(xhr) {
                $btn.prop('disabled', false).html(originalText);
                
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                
                var errorMsg = 'Lỗi khi thêm ghi chú';
                if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                    errorMsg = xhr.responseJSON.errorMessage;
                }
                
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Delete comment for check
    deleteCheckComment: function(commentId) {
        var self = this;
        var checkId = this.currentCheckId;
        
        if (!checkId) {
            if (window.GarageApp) {
                GarageApp.showError('Không tìm thấy phiếu kiểm kê');
            }
            return;
        }

        $.ajax({
            url: `/api/inventorychecks/${checkId}/comments/${commentId}`,
            type: 'DELETE',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    // Reload comments
                    self.loadCheckComments(checkId);
                    if (window.GarageApp) {
                        GarageApp.showSuccess('Đã xóa ghi chú thành công');
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi xóa ghi chú');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                
                var errorMsg = 'Lỗi khi xóa ghi chú';
                if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                    errorMsg = xhr.responseJSON.errorMessage;
                }
                
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Get action badge HTML
    getActionBadge: function(action) {
        var badgeClass = 'badge-secondary';
        var actionText = action || 'Unknown';
        
        if (action) {
            action = action.toLowerCase();
            if (action.includes('create') || action.includes('tạo')) {
                badgeClass = 'badge-primary';
                actionText = 'Tạo mới';
            } else if (action.includes('start') || action.includes('bắt đầu')) {
                badgeClass = 'badge-info';
                actionText = 'Bắt đầu';
            } else if (action.includes('complete') || action.includes('hoàn thành')) {
                badgeClass = 'badge-success';
                actionText = 'Hoàn thành';
            } else if (action.includes('approve') || action.includes('duyệt')) {
                badgeClass = 'badge-success';
                actionText = 'Duyệt';
            } else if (action.includes('reject') || action.includes('từ chối')) {
                badgeClass = 'badge-danger';
                actionText = 'Từ chối';
            } else if (action.includes('update') || action.includes('cập nhật')) {
                badgeClass = 'badge-info';
                actionText = 'Cập nhật';
            } else if (action.includes('delete') || action.includes('xóa')) {
                badgeClass = 'badge-danger';
                actionText = 'Xóa';
            }
        }
        
        return `<span class="badge ${badgeClass}">${actionText}</span>`;
    }
};

// Initialize when document is ready
$(document).ready(function() {
    InventoryChecks.init();
});

