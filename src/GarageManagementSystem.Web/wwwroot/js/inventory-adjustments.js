/**
 * Inventory Adjustments Management Module
 * 
 * Handles all inventory adjustment-related operations
 * CRUD operations for inventory adjustments
 */

window.InventoryAdjustments = {
    // DataTable instance
    adjustmentsTable: null,
    warehouses: [],
    currentAdjustmentId: null,
    createItems: [], // Items for create manual adjustment
    currentItemIndex: null, // Current item index being edited
    selectedAdjustments: new Set(), // Selected adjustment IDs for bulk operations

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.loadWarehouses();
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable('#inventoryAdjustmentsTable')) {
            $('#inventoryAdjustmentsTable').DataTable().destroy();
        }
        
        var columns = [
            {
                data: null,
                title: '',
                width: '30px',
                orderable: false,
                searchable: false,
                className: 'text-center',
                render: function(data, type, row) {
                    var id = row.id ? parseInt(row.id, 10) : 0;
                    if (isNaN(id) || id <= 0) {
                        return '';
                    }
                    // Only show checkbox for Pending adjustments
                    if (row.status === 'Pending') {
                        return `<input type="checkbox" class="adjustment-checkbox" data-id="${id}" title="Chọn">`;
                    }
                    return '';
                }
            },
            { 
                data: 'adjustmentNumber', 
                title: 'Mã Phiếu', 
                width: '120px',
                render: function(data, type, row) {
                    if (!data) return '<span class="text-muted">-</span>';
                    return self.escapeHtml(data);
                }
            },
            { 
                data: 'adjustmentDate', 
                title: 'Ngày Điều Chỉnh',
                render: DataTablesUtility.renderDate
            },
            { 
                data: 'inventoryCheckCode', 
                title: 'Phiếu Kiểm Kê',
                render: function(data, type, row) {
                    if (!data) return '<span class="text-muted">-</span>';
                    return self.escapeHtml(data);
                }
            },
            { 
                data: 'warehouseName', 
                title: 'Kho',
                render: function(data, type, row) {
                    if (!data) return '<span class="text-muted">-</span>';
                    return self.escapeHtml(data);
                }
            },
            { 
                data: 'warehouseZoneName', 
                title: 'Khu Vực',
                render: function(data, type, row) {
                    if (!data) return '<span class="text-muted">-</span>';
                    return self.escapeHtml(data);
                }
            },
            { 
                data: 'warehouseBinName', 
                title: 'Kệ/Ngăn',
                render: function(data, type, row) {
                    if (!data) return '<span class="text-muted">-</span>';
                    return self.escapeHtml(data);
                }
            },
            { 
                data: 'status', 
                title: 'Trạng Thái',
                render: function(data, type, row) {
                    var badgeClass = 'badge-secondary';
                    var statusText = 'Chờ duyệt';
                    switch(data) {
                        case 'Pending': 
                            badgeClass = 'badge-warning';
                            statusText = 'Chờ duyệt';
                            break;
                        case 'Approved': 
                            badgeClass = 'badge-success';
                            statusText = 'Đã duyệt';
                            break;
                        case 'Rejected': 
                            badgeClass = 'badge-danger';
                            statusText = 'Đã từ chối';
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
                data: 'reason', 
                title: 'Lý Do',
                render: function(data, type, row) {
                    if (!data) return '<span class="text-muted">-</span>';
                    var truncated = data.length > 50 ? data.substring(0, 50) + '...' : data;
                    return self.escapeHtml(truncated);
                }
            },
            { 
                data: 'approvedByEmployeeName', 
                title: 'Người Duyệt',
                render: function(data, type, row) {
                    if (!data) return '<span class="text-muted">-</span>';
                    return self.escapeHtml(data);
                }
            },
            {
                data: null,
                title: 'Thao Tác',
                width: '150px',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    var id = row.id ? parseInt(row.id, 10) : 0;
                    if (isNaN(id) || id <= 0) {
                        return '';
                    }
                    
                    var actions = `
                        <button class="btn btn-info btn-sm view-adjustment" data-id="${id}" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                    `;
                    
                    if (row.status === 'Pending') {
                        actions += `
                            <button class="btn btn-danger btn-sm delete-adjustment" data-id="${id}" title="Xóa">
                                <i class="fas fa-trash"></i>
                            </button>
                        `;
                    }
                    
                    return actions;
                }
            }
        ];
        
        this.adjustmentsTable = DataTablesUtility.initAjaxTable('#inventoryAdjustmentsTable', '/InventoryAdjustments/GetInventoryAdjustments', columns, {
            order: [[2, 'desc']], // Sort by adjustmentDate descending (column index changed due to checkbox column)
            pageLength: 10,
            ajax: {
                dataSrc: function(json) {
                    if (json.data && Array.isArray(json.data)) {
                        return json.data;
                    }
                    return [];
                }
            }
        });

        // Handle select all checkbox
        $('#selectAllAdjustments').on('change', function() {
            var isChecked = $(this).prop('checked');
            $('.adjustment-checkbox').each(function() {
                $(this).prop('checked', isChecked);
                var id = parseInt($(this).data('id'), 10);
                if (isChecked && id > 0) {
                    self.selectedAdjustments.add(id);
                } else {
                    self.selectedAdjustments.delete(id);
                }
            });
            self.updateBulkActionButtons();
        });

        // Handle individual checkbox change
        $(document).on('change', '.adjustment-checkbox', function() {
            var id = parseInt($(this).data('id'), 10);
            if ($(this).prop('checked')) {
                self.selectedAdjustments.add(id);
            } else {
                self.selectedAdjustments.delete(id);
                $('#selectAllAdjustments').prop('checked', false);
            }
            self.updateBulkActionButtons();
        });
    },

    // Bind events
    bindEvents: function() {
        var self = this;

        // Create adjustment button
        $(document).on('click', '#createAdjustmentBtn', function() {
            self.createAdjustment();
        });

        // Create adjustment form submit
        $(document).on('submit', '#createAdjustmentForm', function(e) {
            e.preventDefault();
            self.submitCreateAdjustment();
        });

        // Add item to create form
        $(document).on('click', '#addItemToCreateBtn', function() {
            self.openAddItemModal();
        });

        // Create item form submit
        $(document).on('submit', '#createItemForm', function(e) {
            e.preventDefault();
            self.submitCreateItem();
        });

        // Remove item from create form
        $(document).on('click', '.remove-create-item', function() {
            var index = parseInt($(this).data('index'), 10);
            if (!isNaN(index) && index >= 0) {
                self.removeCreateItem(index);
            }
        });

        // Edit item in create form
        $(document).on('click', '.edit-create-item', function() {
            var index = parseInt($(this).data('index'), 10);
            if (!isNaN(index) && index >= 0) {
                self.editCreateItem(index);
            }
        });

        // Warehouse change handler for create
        $(document).on('change', '#createWarehouse', function() {
            self.onCreateWarehouseChange();
        });

        // Zone change handler for create
        $(document).on('change', '#createWarehouseZone', function() {
            self.onCreateWarehouseZoneChange();
        });

        // Calculate quantity after when quantity change changes
        $(document).on('input change', '#createItemQuantityChange', function() {
            self.calculateCreateItemQuantityAfter();
        });

        // Create modal cleanup
        $('#createAdjustmentModal').on('hidden.bs.modal', function() {
            self.resetCreateForm();
        });

        // Create item modal cleanup
        $('#createItemModal').on('hidden.bs.modal', function() {
            self.resetCreateItemForm();
        });

        // View adjustment
        $(document).on('click', '.view-adjustment', function() {
            var id = parseInt($(this).data('id'), 10);
            if (isNaN(id) || id <= 0) {
                if (window.GarageApp) {
                    GarageApp.showError('ID không hợp lệ');
                }
                return;
            }
            self.viewAdjustment(id);
        });

        // Delete adjustment
        $(document).on('click', '.delete-adjustment', function() {
            var id = parseInt($(this).data('id'), 10);
            if (isNaN(id) || id <= 0) {
                if (window.GarageApp) {
                    GarageApp.showError('ID không hợp lệ');
                }
                return;
            }
            self.deleteAdjustment(id);
        });

        // Apply filters
        $(document).on('click', '#applyFiltersBtn', function() {
            self.applyFilters();
        });

        // Clear filters
        $(document).on('click', '#clearFiltersBtn', function() {
            self.clearFilters();
        });

        // Approve adjustment
        $(document).on('click', '#btnApproveAdjustment', function() {
            if (self.currentAdjustmentId) {
                $('#approveAdjustmentModal').modal('show');
            }
        });

        // Reject adjustment
        $(document).on('click', '#btnRejectAdjustment', function() {
            if (self.currentAdjustmentId) {
                $('#rejectAdjustmentModal').modal('show');
            }
        });

        // Confirm approve
        $(document).on('click', '#confirmApproveBtn', function() {
            if (self.currentAdjustmentId) {
                self.approveAdjustment(self.currentAdjustmentId);
            }
        });

        // Confirm reject
        $(document).on('click', '#confirmRejectBtn', function() {
            var reason = $('#rejectionReason').val().trim();
            if (!reason) {
                if (window.GarageApp) {
                    GarageApp.showError('Vui lòng nhập lý do từ chối');
                }
                return;
            }
            if (self.currentAdjustmentId) {
                self.rejectAdjustment(self.currentAdjustmentId, reason);
            }
        });

        // Create from check
        $(document).on('click', '#confirmCreateFromCheckBtn', function() {
            self.submitCreateFromCheck();
        });

        // Add comment button
        $(document).on('click', '#addCommentBtn', function() {
            var adjustmentId = self.currentAdjustmentId;
            var commentText = $('#newCommentText').val().trim();
            if (!commentText) {
                if (window.GarageApp) {
                    GarageApp.showError('Vui lòng nhập nội dung ghi chú');
                }
                return;
            }
            if (adjustmentId) {
                self.addComment(adjustmentId, commentText);
            }
        });

        // Delete comment button
        $(document).on('click', '.delete-comment-btn', function() {
            var commentId = parseInt($(this).data('comment-id'), 10);
            if (commentId > 0 && confirm('Bạn có chắc chắn muốn xóa ghi chú này?')) {
                self.deleteComment(commentId);
            }
        });

        // Tab change handler - load data when tab is clicked
        $(document).on('shown.bs.tab', '#history-tab', function() {
            if (self.currentAdjustmentId) {
                self.loadAdjustmentHistory(self.currentAdjustmentId);
            }
        });

        $(document).on('shown.bs.tab', '#comments-tab', function() {
            if (self.currentAdjustmentId) {
                self.loadAdjustmentComments(self.currentAdjustmentId);
            }
        });

        // Modal cleanup
        $('#viewAdjustmentModal').on('hidden.bs.modal', function() {
            self.currentAdjustmentId = null;
            $('#newCommentText').val('');
            // Reset to first tab
            $('#info-tab').tab('show');
        });

        $('#approveAdjustmentModal').on('hidden.bs.modal', function() {
            $('#approveNotes').val('');
        });

        $('#rejectAdjustmentModal').on('hidden.bs.modal', function() {
            $('#rejectionReason').val('');
        });

        // Bulk approve button
        $(document).on('click', '#bulkApproveBtn', function() {
            var selectedIds = self.getSelectedAdjustmentIds();
            if (selectedIds.length === 0) {
                if (window.GarageApp) {
                    GarageApp.showError('Vui lòng chọn ít nhất một phiếu điều chỉnh');
                }
                return;
            }
            $('#bulkApproveCount').text(selectedIds.length);
            $('#bulkApproveNotes').val('');
            $('#bulkApproveModal').modal('show');
        });

        // Bulk reject button
        $(document).on('click', '#bulkRejectBtn', function() {
            var selectedIds = self.getSelectedAdjustmentIds();
            if (selectedIds.length === 0) {
                if (window.GarageApp) {
                    GarageApp.showError('Vui lòng chọn ít nhất một phiếu điều chỉnh');
                }
                return;
            }
            $('#bulkRejectCount').text(selectedIds.length);
            $('#bulkRejectionReason').val('');
            $('#bulkRejectModal').modal('show');
        });

        // Confirm bulk approve
        $(document).on('click', '#confirmBulkApproveBtn', function() {
            var selectedIds = self.getSelectedAdjustmentIds();
            if (selectedIds.length === 0) {
                if (window.GarageApp) {
                    GarageApp.showError('Vui lòng chọn ít nhất một phiếu điều chỉnh');
                }
                return;
            }
            var notes = $('#bulkApproveNotes').val().trim();
            self.bulkApproveAdjustments(selectedIds, notes);
        });

        // Confirm bulk reject
        $(document).on('click', '#confirmBulkRejectBtn', function() {
            var selectedIds = self.getSelectedAdjustmentIds();
            if (selectedIds.length === 0) {
                if (window.GarageApp) {
                    GarageApp.showError('Vui lòng chọn ít nhất một phiếu điều chỉnh');
                }
                return;
            }
            var reason = $('#bulkRejectionReason').val().trim();
            if (!reason) {
                if (window.GarageApp) {
                    GarageApp.showError('Vui lòng nhập lý do từ chối');
                }
                return;
            }
            self.bulkRejectAdjustments(selectedIds, reason);
        });

        // Modal cleanup
        $('#bulkApproveModal').on('hidden.bs.modal', function() {
            $('#bulkApproveNotes').val('');
        });

        $('#bulkRejectModal').on('hidden.bs.modal', function() {
            $('#bulkRejectionReason').val('');
        });
    },

    // Load warehouses for filter dropdown
    loadWarehouses: function() {
        var self = this;
        $.ajax({
            url: '/InventoryAdjustments/Warehouses',
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
            },
            error: function() {
                // Silently fail - filter will just be empty
            }
        });
    },

    // Apply filters
    applyFilters: function() {
        if (this.adjustmentsTable) {
            this.adjustmentsTable.ajax.reload();
        }
    },

    // Clear filters
    clearFilters: function() {
        $('#filterStatus').val('');
        $('#filterWarehouse').val('');
        $('#filterStartDate').val('');
        $('#filterEndDate').val('');
        this.applyFilters();
    },

    // View adjustment details
    viewAdjustment: function(id) {
        var self = this;
        if (!id) {
            if (window.GarageApp) {
                GarageApp.showError('ID không hợp lệ');
            }
            return;
        }
        this.currentAdjustmentId = id;

        $.ajax({
            url: `/api/inventoryadjustments/${id}`,
            type: 'GET',
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                if (response.success && response.data) {
                    self.populateViewModal(response.data);
                    // Load history and comments when modal opens
                    self.loadAdjustmentHistory(id);
                    self.loadAdjustmentComments(id);
                    $('#viewAdjustmentModal').modal('show');
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Không thể tải thông tin phiếu điều chỉnh');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = 'Lỗi khi tải thông tin phiếu điều chỉnh';
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

        $('#viewAdjustmentNumber').text(data.adjustmentNumber || '-');
        
        // Handle date format
        var adjustmentDateText = '-';
        if (data.adjustmentDate) {
            try {
                var date = new Date(data.adjustmentDate);
                if (!isNaN(date.getTime())) {
                    adjustmentDateText = date.toLocaleDateString('vi-VN');
                }
            } catch (e) {
                // Keep default '-'
            }
        }
        $('#viewAdjustmentDate').text(adjustmentDateText);
        
        $('#viewInventoryCheck').text(data.inventoryCheckCode || '-');
        $('#viewWarehouse').text(data.warehouseName || '-');
        $('#viewWarehouseZone').text(data.warehouseZoneName || '-');
        $('#viewWarehouseBin').text(data.warehouseBinName || '-');
        $('#viewReason').text(data.reason || '-');
        $('#viewNotes').text(data.notes || '-');

        // Status badge
        var statusBadge = this.getStatusBadge(data.status);
        $('#viewAdjustmentStatus').html(statusBadge);

        // Approval info
        if (data.status === 'Approved') {
            $('#viewApprovalInfo').show();
            $('#viewApprovedBy').text(data.approvedByEmployeeName || '-');
            var approvedAtText = '-';
            if (data.approvedAt) {
                try {
                    var approvedDate = new Date(data.approvedAt);
                    if (!isNaN(approvedDate.getTime())) {
                        approvedAtText = approvedDate.toLocaleDateString('vi-VN') + ' ' + approvedDate.toLocaleTimeString('vi-VN');
                    }
                } catch (e) {
                    // Keep default '-'
                }
            }
            $('#viewApprovedAt').text(approvedAtText);
            $('#viewRejectionInfo').hide();
        } else if (data.status === 'Rejected') {
            $('#viewApprovalInfo').hide();
            $('#viewRejectionInfo').show();
            $('#viewRejectionReason').text(data.rejectionReason || '-');
        } else {
            $('#viewApprovalInfo').hide();
            $('#viewRejectionInfo').hide();
        }

        // Show/hide action buttons
        if (data.status === 'Pending') {
            $('#btnApproveAdjustment').show();
            $('#btnRejectAdjustment').show();
        } else {
            $('#btnApproveAdjustment').hide();
            $('#btnRejectAdjustment').hide();
        }

        // Populate items
        this.populateItemsTable(data.items || []);
    },

    // Populate items table
    populateItemsTable: function(items) {
        var self = this;
        var tbody = $('#viewAdjustmentItems');
        tbody.empty();

        if (!items || items.length === 0) {
            tbody.append('<tr><td colspan="7" class="text-center text-muted">Không có items</td></tr>');
            return;
        }

        items.forEach(function(item, index) {
            var quantityChangeClass = item.quantityChange > 0 ? 'text-success' : 'text-danger';
            var quantityChangeSign = item.quantityChange > 0 ? '+' : '';
            
            var row = `
                <tr>
                    <td>${index + 1}</td>
                    <td>${self.escapeHtml(item.partNumber || '-')}</td>
                    <td>${self.escapeHtml(item.partName || '-')}</td>
                    <td class="text-right">${item.systemQuantityBefore || 0}</td>
                    <td class="text-right ${quantityChangeClass}">${quantityChangeSign}${item.quantityChange || 0}</td>
                    <td class="text-right">${item.systemQuantityAfter || 0}</td>
                    <td>${self.escapeHtml(item.notes || '-')}</td>
                </tr>
            `;
            tbody.append(row);
        });
    },

    // Get status badge
    getStatusBadge: function(status) {
        var badgeClass = 'badge-secondary';
        var statusText = 'Chờ duyệt';
        switch(status) {
            case 'Pending': 
                badgeClass = 'badge-warning';
                statusText = 'Chờ duyệt';
                break;
            case 'Approved': 
                badgeClass = 'badge-success';
                statusText = 'Đã duyệt';
                break;
            case 'Rejected': 
                badgeClass = 'badge-danger';
                statusText = 'Đã từ chối';
                break;
        }
        return `<span class="badge ${badgeClass}">${statusText}</span>`;
    },

    // Approve adjustment
    approveAdjustment: function(id) {
        var self = this;
        var notes = $('#approveNotes').val().trim();

        $.ajax({
            url: `/api/inventoryadjustments/${id}/approve`,
            type: 'POST',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify({ notes: notes || null }),
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                if (response.success) {
                    if (window.GarageApp) {
                        GarageApp.showSuccess('Duyệt phiếu điều chỉnh thành công');
                    }
                    $('#approveAdjustmentModal').modal('hide');
                    $('#viewAdjustmentModal').modal('hide');
                    if (self.adjustmentsTable) {
                        self.adjustmentsTable.ajax.reload(null, false);
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi duyệt phiếu điều chỉnh');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = 'Lỗi khi duyệt phiếu điều chỉnh';
                if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                    errorMsg = xhr.responseJSON.errorMessage;
                }
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Reject adjustment
    rejectAdjustment: function(id, reason) {
        var self = this;

        $.ajax({
            url: `/api/inventoryadjustments/${id}/reject`,
            type: 'POST',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify({ rejectionReason: reason }),
            success: function(response) {
                if (window.AuthHandler && !AuthHandler.validateApiResponse(response)) {
                    return;
                }
                if (response.success) {
                    if (window.GarageApp) {
                        GarageApp.showSuccess('Từ chối phiếu điều chỉnh thành công');
                    }
                    $('#rejectAdjustmentModal').modal('hide');
                    $('#viewAdjustmentModal').modal('hide');
                    if (self.adjustmentsTable) {
                        self.adjustmentsTable.ajax.reload(null, false);
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi từ chối phiếu điều chỉnh');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = 'Lỗi khi từ chối phiếu điều chỉnh';
                if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                    errorMsg = xhr.responseJSON.errorMessage;
                }
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Delete adjustment
    deleteAdjustment: function(id) {
        var self = this;
        
        if (!window.confirm('Bạn có chắc chắn muốn xóa phiếu điều chỉnh này?')) {
            return;
        }

        $.ajax({
            url: `/api/inventoryadjustments/${id}`,
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
                        GarageApp.showSuccess('Xóa phiếu điều chỉnh thành công');
                    }
                    if (self.adjustmentsTable) {
                        self.adjustmentsTable.ajax.reload(null, false);
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi xóa phiếu điều chỉnh');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = 'Lỗi khi xóa phiếu điều chỉnh';
                if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                    errorMsg = xhr.responseJSON.errorMessage;
                }
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Create adjustment from check (called from Inventory Checks page)
    createAdjustmentFromCheck: function(checkId, checkCode) {
        var self = this;
        
        $('#fromCheckInventoryCheckId').val(checkId || '');
        $('#fromCheckInventoryCheckCode').val(checkCode || '');
        $('#fromCheckReason').val('');
        $('#fromCheckNotes').val('');
        $('#createFromCheckModal').modal('show');
    },

    // Submit create from check
    submitCreateFromCheck: function() {
        var self = this;
        var checkId = parseInt($('#fromCheckInventoryCheckId').val(), 10);
        var reason = $('#fromCheckReason').val().trim();

        if (!checkId || isNaN(checkId) || checkId <= 0) {
            if (window.GarageApp) {
                GarageApp.showError('ID phiếu kiểm kê không hợp lệ');
            }
            return;
        }

        if (!reason) {
            if (window.GarageApp) {
                GarageApp.showError('Vui lòng nhập lý do điều chỉnh');
            }
            return;
        }

        var formData = {
            reason: reason,
            notes: $('#fromCheckNotes').val().trim() || null
        };

        $.ajax({
            url: `/api/inventoryadjustments/from-check/${checkId}`,
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
                        GarageApp.showSuccess('Tạo phiếu điều chỉnh thành công');
                    }
                    $('#createFromCheckModal').modal('hide');
                    if (self.adjustmentsTable) {
                        self.adjustmentsTable.ajax.reload(null, false);
                    }
                    // Reload inventory checks table if exists
                    if (window.InventoryChecks && window.InventoryChecks.checksTable) {
                        window.InventoryChecks.checksTable.ajax.reload(null, false);
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi tạo phiếu điều chỉnh');
                    }
                }
            },
            error: function(xhr) {
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = 'Lỗi khi tạo phiếu điều chỉnh';
                if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                    errorMsg = xhr.responseJSON.errorMessage;
                }
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Create adjustment (manual)
    createAdjustment: function() {
        this.resetCreateForm();
        this.loadWarehousesForCreate();
        var today = new Date().toISOString().split('T')[0];
        $('#createAdjustmentDate').val(today);
        $('#createAdjustmentModal').modal('show');
    },

    // Reset create form
    resetCreateForm: function() {
        if ($('#createAdjustmentForm').length > 0 && $('#createAdjustmentForm')[0]) {
            $('#createAdjustmentForm')[0].reset();
        }
        this.createItems = [];
        this.currentItemIndex = null;
        this.renderCreateItemsTable();
        $('#createWarehouseZone').prop('disabled', true).html('<option value="">-- Chọn khu vực --</option>');
        $('#createWarehouseBin').prop('disabled', true).html('<option value="">-- Chọn kệ/ngăn --</option>');
    },

    // Load warehouses for create form
    loadWarehousesForCreate: function() {
        var self = this;
        var $warehouseSelect = $('#createWarehouse');
        $warehouseSelect.html('<option value="">-- Chọn kho --</option>');
        
        if (this.warehouses && this.warehouses.length > 0) {
            this.warehouses.forEach(function(warehouse) {
                $warehouseSelect.append($('<option></option>')
                    .attr('value', warehouse.id)
                    .text(self.escapeHtml(warehouse.name || '')));
            });
        }
    },

    // Handle warehouse change for create
    onCreateWarehouseChange: function() {
        var self = this;
        var warehouseId = parseInt($('#createWarehouse').val(), 10);
        var $zoneSelect = $('#createWarehouseZone');
        var $binSelect = $('#createWarehouseBin');
        
        $zoneSelect.prop('disabled', true).html('<option value="">-- Chọn khu vực --</option>');
        $binSelect.prop('disabled', true).html('<option value="">-- Chọn kệ/ngăn --</option>');
        
        if (isNaN(warehouseId) || warehouseId <= 0) {
            return;
        }
        
        // Load zones for selected warehouse
        $.ajax({
            url: '/Warehouses/GetZones',
            type: 'GET',
            data: { warehouseId: warehouseId },
            success: function(response) {
                if (response && response.success && response.data && Array.isArray(response.data)) {
                    $zoneSelect.prop('disabled', false);
                    response.data.forEach(function(zone) {
                        $zoneSelect.append($('<option></option>')
                            .attr('value', zone.id)
                            .text(self.escapeHtml(zone.name || '')));
                    });
                }
            },
            error: function(xhr) {
                console.warn('Error loading zones:', xhr);
            }
        });
    },

    // Handle zone change for create
    onCreateWarehouseZoneChange: function() {
        var self = this;
        var zoneId = parseInt($('#createWarehouseZone').val(), 10);
        var $binSelect = $('#createWarehouseBin');
        
        $binSelect.prop('disabled', true).html('<option value="">-- Chọn kệ/ngăn --</option>');
        
        if (isNaN(zoneId) || zoneId <= 0) {
            return;
        }
        
        // Load bins for selected zone
        $.ajax({
            url: '/Warehouses/GetBins',
            type: 'GET',
            data: { zoneId: zoneId },
            success: function(response) {
                if (response && response.success && response.data && Array.isArray(response.data)) {
                    $binSelect.prop('disabled', false);
                    response.data.forEach(function(bin) {
                        $binSelect.append($('<option></option>')
                            .attr('value', bin.id)
                            .text(self.escapeHtml(bin.name || '')));
                    });
                }
            },
            error: function(xhr) {
                console.warn('Error loading bins:', xhr);
            }
        });
    },

    // Open add item modal
    openAddItemModal: function() {
        this.currentItemIndex = null;
        this.resetCreateItemForm();
        $('#createItemModalTitle').text('Thêm Item');
        this.initCreateItemTypeahead();
        $('#createItemModal').modal('show');
    },

    // Edit create item
    editCreateItem: function(index) {
        if (index < 0 || index >= this.createItems.length) {
            return;
        }
        
        var item = this.createItems[index];
        this.currentItemIndex = index;
        this.resetCreateItemForm();
        $('#createItemModalTitle').text('Sửa Item');
        
        // Populate form
        $('#createItemPartId').val(item.partId || '');
        $('#createItemPartSearch').val(item.partName || item.partNumber || '');
        $('#createItemPartNumber').text(item.partNumber || '-');
        $('#createItemPartName').text(item.partName || '-');
        $('#createItemPartSku').text(item.partSku || '-');
        $('#createItemPartStock').text(item.systemQuantityBefore || 0);
        $('#createItemSystemQuantityBefore').val(item.systemQuantityBefore || 0);
        $('#createItemQuantityChange').val(item.quantityChange || 0);
        $('#createItemSystemQuantityAfter').val(item.systemQuantityAfter || 0);
        $('#createItemNotes').val(item.notes || '');
        $('#createItemPartInfo').show();
        
        this.initCreateItemTypeahead();
        $('#createItemModal').modal('show');
    },

    // Remove create item
    removeCreateItem: function(index) {
        if (index < 0 || index >= this.createItems.length) {
            return;
        }
        
        if (window.GarageApp) {
            GarageApp.showConfirm('Bạn có chắc chắn muốn xóa item này?', function() {
                this.createItems.splice(index, 1);
                this.renderCreateItemsTable();
            }.bind(this));
        } else {
            this.createItems.splice(index, 1);
            this.renderCreateItemsTable();
        }
    },

    // Render create items table
    renderCreateItemsTable: function() {
        var self = this;
        var $tbody = $('#createItemsTableBody');
        $tbody.empty();
        
        if (this.createItems.length === 0) {
            $tbody.html('<tr id="createItemsEmptyRow"><td colspan="9" class="text-center text-muted py-3"><i class="fas fa-info-circle"></i> Chưa có item nào. Vui lòng thêm item.</td></tr>');
            return;
        }
        
        this.createItems.forEach(function(item, index) {
            var quantityChangeClass = '';
            var quantityChangeText = item.quantityChange || 0;
            if (quantityChangeText > 0) {
                quantityChangeClass = 'text-success';
                quantityChangeText = '+' + quantityChangeText;
            } else if (quantityChangeText < 0) {
                quantityChangeClass = 'text-danger';
            }
            
            var row = `
                <tr>
                    <td>${index + 1}</td>
                    <td>${self.escapeHtml(item.partNumber || '-')}</td>
                    <td>${self.escapeHtml(item.partName || '-')}</td>
                    <td>${self.escapeHtml(item.partSku || '-')}</td>
                    <td class="text-right">${item.systemQuantityBefore || 0}</td>
                    <td class="text-right ${quantityChangeClass}">${quantityChangeText}</td>
                    <td class="text-right">${item.systemQuantityAfter || 0}</td>
                    <td>${self.escapeHtml(item.notes || '-')}</td>
                    <td class="text-center">
                        <button type="button" class="btn btn-sm btn-info edit-create-item" data-index="${index}" title="Sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-danger remove-create-item" data-index="${index}" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
            $tbody.append(row);
        });
    },

    // Init typeahead for create item
    initCreateItemTypeahead: function() {
        var self = this;
        var $input = $('#createItemPartSearch');
        
        // Destroy existing typeahead if any
        try {
            if ($input.data('typeahead')) {
                $input.typeahead('destroy');
            }
        } catch (e) {
            console.warn('Error destroying typeahead:', e);
        }
        
        var currentRequest = null;
        
        $input.typeahead({
            source: function(query, process) {
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
                    self.selectCreateItemPart(item);
                }
            }
        });
    },

    // Select part for create item
    selectCreateItemPart: function(part) {
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
        
        // Check if part already exists in items
        var existingIndex = this.createItems.findIndex(function(item) {
            return item.partId === partId;
        });
        
        if (existingIndex >= 0 && (this.currentItemIndex === null || existingIndex !== this.currentItemIndex)) {
            if (window.GarageApp) {
                GarageApp.showError('Phụ tùng này đã có trong danh sách');
            }
            return;
        }
        
        $('#createItemPartId').val(partId);
        $('#createItemPartNumber').text(this.escapeHtml(partNumber));
        $('#createItemPartName').text(this.escapeHtml(partName));
        $('#createItemPartSku').text(this.escapeHtml(partSku));
        $('#createItemPartStock').text(quantityInStock);
        $('#createItemSystemQuantityBefore').val(quantityInStock);
        $('#createItemPartInfo').show();
        
        // Reset quantity change and recalculate
        $('#createItemQuantityChange').val(0);
        this.calculateCreateItemQuantityAfter();
    },

    // Calculate quantity after for create item
    calculateCreateItemQuantityAfter: function() {
        var systemQtyBefore = parseInt($('#createItemSystemQuantityBefore').val() || 0, 10);
        var quantityChange = parseInt($('#createItemQuantityChange').val() || 0, 10);
        var quantityAfter = systemQtyBefore + quantityChange;
        
        $('#createItemSystemQuantityAfter').val(quantityAfter);
        
        var $errorDiv = $('#createItemQuantityAfterError');
        if (quantityAfter < 0) {
            $errorDiv.show();
            $('#createItemSystemQuantityAfter').addClass('is-invalid');
        } else {
            $errorDiv.hide();
            $('#createItemSystemQuantityAfter').removeClass('is-invalid');
        }
    },

    // Submit create item
    submitCreateItem: function() {
        var partId = parseInt($('#createItemPartId').val(), 10);
        var systemQuantityBefore = parseInt($('#createItemSystemQuantityBefore').val() || 0, 10);
        var quantityChange = parseInt($('#createItemQuantityChange').val() || 0, 10);
        var systemQuantityAfter = parseInt($('#createItemSystemQuantityAfter').val() || 0, 10);
        var notes = $('#createItemNotes').val().trim();
        
        // Validation
        if (isNaN(partId) || partId <= 0) {
            if (window.GarageApp) {
                GarageApp.showError('Vui lòng chọn phụ tùng');
            }
            return;
        }
        
        if (systemQuantityAfter < 0) {
            if (window.GarageApp) {
                GarageApp.showError('Tồn kho sau điều chỉnh không được âm');
            }
            return;
        }
        
        if (systemQuantityAfter !== systemQuantityBefore + quantityChange) {
            if (window.GarageApp) {
                GarageApp.showError('Tồn kho sau điều chỉnh không khớp với tính toán');
            }
            return;
        }
        
        var item = {
            partId: partId,
            partNumber: $('#createItemPartNumber').text(),
            partName: $('#createItemPartName').text(),
            partSku: $('#createItemPartSku').text(),
            systemQuantityBefore: systemQuantityBefore,
            quantityChange: quantityChange,
            systemQuantityAfter: systemQuantityAfter,
            notes: notes
        };
        
        if (this.currentItemIndex !== null && this.currentItemIndex >= 0 && this.currentItemIndex < this.createItems.length) {
            // Update existing item
            this.createItems[this.currentItemIndex] = item;
        } else {
            // Add new item
            this.createItems.push(item);
        }
        
        this.renderCreateItemsTable();
        $('#createItemModal').modal('hide');
    },

    // Reset create item form
    resetCreateItemForm: function() {
        if ($('#createItemForm').length > 0 && $('#createItemForm')[0]) {
            $('#createItemForm')[0].reset();
        }
        $('#createItemIndex').val('');
        $('#createItemPartId').val('');
        $('#createItemPartInfo').hide();
        $('#createItemSystemQuantityBefore').val('');
        $('#createItemQuantityChange').val('');
        $('#createItemSystemQuantityAfter').val('');
        $('#createItemQuantityAfterError').hide();
        $('#createItemSystemQuantityAfter').removeClass('is-invalid');
        
        try {
            var $input = $('#createItemPartSearch');
            if ($input.data('typeahead')) {
                $input.typeahead('destroy');
            }
        } catch (e) {
            // Ignore
        }
    },

    // Submit create adjustment
    submitCreateAdjustment: function() {
        var self = this;
        
        // Validation
        if (this.createItems.length === 0) {
            if (window.GarageApp) {
                GarageApp.showError('Vui lòng thêm ít nhất một item');
            }
            return;
        }
        
        var adjustmentDate = $('#createAdjustmentDate').val();
        var reason = $('#createReason').val().trim();
        var notes = $('#createNotes').val().trim();
        var warehouseId = $('#createWarehouse').val() ? parseInt($('#createWarehouse').val(), 10) : null;
        var warehouseZoneId = $('#createWarehouseZone').val() ? parseInt($('#createWarehouseZone').val(), 10) : null;
        var warehouseBinId = $('#createWarehouseBin').val() ? parseInt($('#createWarehouseBin').val(), 10) : null;
        
        if (!adjustmentDate) {
            if (window.GarageApp) {
                GarageApp.showError('Vui lòng chọn ngày điều chỉnh');
            }
            return;
        }
        
        if (!reason) {
            if (window.GarageApp) {
                GarageApp.showError('Vui lòng nhập lý do điều chỉnh');
            }
            return;
        }
        
        // Prepare data
        var data = {
            adjustmentDate: adjustmentDate,
            reason: reason,
            notes: notes || null,
            warehouseId: warehouseId,
            warehouseZoneId: warehouseZoneId,
            warehouseBinId: warehouseBinId,
            items: this.createItems.map(function(item) {
                return {
                    partId: item.partId,
                    quantityChange: item.quantityChange,
                    systemQuantityBefore: item.systemQuantityBefore,
                    systemQuantityAfter: item.systemQuantityAfter,
                    notes: item.notes || null
                };
            })
        };
        
        // Show loading
        var $submitBtn = $('#submitCreateAdjustmentBtn');
        var originalText = $submitBtn.html();
        $submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang tạo...');
        
        // Get token
        var token = $('input[name="__RequestVerificationToken"]').val();
        
        $.ajax({
            url: '/api/inventory-adjustments',
            type: 'POST',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': token
            },
            data: JSON.stringify(data),
            success: function(response) {
                $submitBtn.prop('disabled', false).html(originalText);
                
                if (response && response.success) {
                    if (window.GarageApp) {
                        GarageApp.showSuccess('Tạo phiếu điều chỉnh thành công');
                    }
                    $('#createAdjustmentModal').modal('hide');
                    if (self.adjustmentsTable) {
                        self.adjustmentsTable.ajax.reload(null, false);
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi tạo phiếu điều chỉnh');
                    }
                }
            },
            error: function(xhr) {
                $submitBtn.prop('disabled', false).html(originalText);
                
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                
                var errorMsg = 'Lỗi khi tạo phiếu điều chỉnh';
                if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                    errorMsg = xhr.responseJSON.errorMessage;
                } else if (xhr.responseJSON && xhr.responseJSON.errors && Array.isArray(xhr.responseJSON.errors)) {
                    errorMsg = xhr.responseJSON.errors.join(', ');
                }
                
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Update bulk action buttons visibility
    updateBulkActionButtons: function() {
        var count = this.selectedAdjustments.size;
        if (count > 0) {
            $('#bulkApproveBtn').show();
            $('#bulkRejectBtn').show();
            $('#bulkApproveBtn span#selectedCount').text(count);
            $('#bulkRejectBtn span#selectedCount').text(count);
        } else {
            $('#bulkApproveBtn').hide();
            $('#bulkRejectBtn').hide();
        }
        $('#selectAllAdjustments').prop('checked', false);
    },

    // Get selected adjustment IDs
    getSelectedAdjustmentIds: function() {
        return Array.from(this.selectedAdjustments);
    },

    // Bulk approve adjustments
    bulkApproveAdjustments: function(adjustmentIds, notes) {
        var self = this;
        var $modal = $('#bulkApproveModal');
        var $btn = $('#confirmBulkApproveBtn');
        var originalText = $btn.html();
        
        $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang xử lý...');

        $.ajax({
            url: '/api/InventoryAdjustments/bulk-approve',
            type: 'POST',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify({
                adjustmentIds: adjustmentIds,
                notes: notes || null
            }),
            success: function(response) {
                $btn.prop('disabled', false).html(originalText);
                $modal.modal('hide');

                if (response && response.success) {
                    if (window.GarageApp) {
                        var message = `Đã duyệt thành công ${response.data?.successCount || adjustmentIds.length} phiếu điều chỉnh`;
                        if (response.data?.failedCount > 0) {
                            message += `. ${response.data.failedCount} phiếu thất bại.`;
                        }
                        GarageApp.showSuccess(message);
                    }
                    
                    // Clear selections before reload
                    self.selectedAdjustments.clear();
                    $('#selectAllAdjustments').prop('checked', false);
                    self.updateBulkActionButtons();
                    
                    // Reload table
                    if (self.adjustmentsTable) {
                        self.adjustmentsTable.ajax.reload(null, false);
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi duyệt phiếu điều chỉnh');
                    }
                }
            },
            error: function(xhr) {
                $btn.prop('disabled', false).html(originalText);
                
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                
                var errorMsg = 'Lỗi khi duyệt phiếu điều chỉnh';
                if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                    errorMsg = xhr.responseJSON.errorMessage;
                } else if (xhr.responseJSON && xhr.responseJSON.errors && Array.isArray(xhr.responseJSON.errors)) {
                    errorMsg = xhr.responseJSON.errors.join(', ');
                }
                
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Bulk reject adjustments
    bulkRejectAdjustments: function(adjustmentIds, rejectionReason) {
        var self = this;
        var $modal = $('#bulkRejectModal');
        var $btn = $('#confirmBulkRejectBtn');
        var originalText = $btn.html();
        
        $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang xử lý...');

        $.ajax({
            url: '/api/InventoryAdjustments/bulk-reject',
            type: 'POST',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify({
                adjustmentIds: adjustmentIds,
                rejectionReason: rejectionReason
            }),
            success: function(response) {
                $btn.prop('disabled', false).html(originalText);
                $modal.modal('hide');

                if (response && response.success) {
                    if (window.GarageApp) {
                        var message = `Đã từ chối thành công ${response.data?.successCount || adjustmentIds.length} phiếu điều chỉnh`;
                        if (response.data?.failedCount > 0) {
                            message += `. ${response.data.failedCount} phiếu thất bại.`;
                        }
                        GarageApp.showSuccess(message);
                    }
                    
                    // Clear selections before reload
                    self.selectedAdjustments.clear();
                    $('#selectAllAdjustments').prop('checked', false);
                    self.updateBulkActionButtons();
                    
                    // Reload table
                    if (self.adjustmentsTable) {
                        self.adjustmentsTable.ajax.reload(null, false);
                    }
                } else {
                    if (window.GarageApp) {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi từ chối phiếu điều chỉnh');
                    }
                }
            },
            error: function(xhr) {
                $btn.prop('disabled', false).html(originalText);
                
                if (window.AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                
                var errorMsg = 'Lỗi khi từ chối phiếu điều chỉnh';
                if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                    errorMsg = xhr.responseJSON.errorMessage;
                } else if (xhr.responseJSON && xhr.responseJSON.errors && Array.isArray(xhr.responseJSON.errors)) {
                    errorMsg = xhr.responseJSON.errors.join(', ');
                }
                
                if (window.GarageApp) {
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Load adjustment history
    loadAdjustmentHistory: function(adjustmentId) {
        var self = this;
        var $tbody = $('#historyTableBody');
        
        $.ajax({
            url: `/api/inventoryadjustments/${adjustmentId}/history`,
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

    // Load adjustment comments
    loadAdjustmentComments: function(adjustmentId) {
        var self = this;
        var $timeline = $('#commentsTimeline');
        
        $.ajax({
            url: `/api/inventoryadjustments/${adjustmentId}/comments`,
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
                        var commentHtml = self.renderComment(comment);
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

    // Render comment HTML
    renderComment: function(comment) {
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

    // Add comment
    addComment: function(adjustmentId, commentText) {
        var self = this;
        var $btn = $('#addCommentBtn');
        var originalText = $btn.html();
        
        $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang thêm...');

        $.ajax({
            url: `/api/inventoryadjustments/${adjustmentId}/comments`,
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
                    self.loadAdjustmentComments(adjustmentId);
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

    // Delete comment
    deleteComment: function(commentId) {
        var self = this;
        var adjustmentId = this.currentAdjustmentId;
        
        if (!adjustmentId) {
            if (window.GarageApp) {
                GarageApp.showError('Không tìm thấy phiếu điều chỉnh');
            }
            return;
        }

        $.ajax({
            url: `/api/inventoryadjustments/${adjustmentId}/comments/${commentId}`,
            type: 'DELETE',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    // Reload comments
                    self.loadAdjustmentComments(adjustmentId);
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
    },

    // Escape HTML to prevent XSS
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
    }
};

// Initialize when document is ready
$(document).ready(function() {
    if ($('#inventoryAdjustmentsTable').length) {
        InventoryAdjustments.init();
    }
});

