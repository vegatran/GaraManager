/**
 * Supplier Quotation Management Module - Phase 4.2.2
 * 
 * Handles supplier quotation requests and management
 */

window.SupplierQuotationManagement = {
    quotationTable: null,
    parts: [],
    suppliers: [],

    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.loadParts();
        this.loadSuppliers();
    },

    initDataTable: function() {
        var self = this;
        
        var columns = [
            { data: 'id', title: 'ID', width: '5%' },
            { data: 'quotationNumber', title: 'Số Báo Giá', width: '12%' },
            { 
                data: 'supplierName', 
                title: 'Nhà Cung Cấp', 
                width: '15%',
                render: function(data, type, row) {
                    return data || '<span class="text-muted">-</span>';
                }
            },
            { 
                data: 'partName', 
                title: 'Phụ Tùng', 
                width: '15%',
                render: function(data, type, row) {
                    // ✅ SECURITY: Escape HTML to prevent XSS
                    var partNumber = (row.partNumber || '').replace(/[<>&"']/g, function(m) {
                        return {'<': '&lt;', '>': '&gt;', '&': '&amp;', '"': '&quot;', "'": '&#39;'}[m];
                    });
                    var partName = (data || '-').replace(/[<>&"']/g, function(m) {
                        return {'<': '&lt;', '>': '&gt;', '&': '&amp;', '"': '&quot;', "'": '&#39;'}[m];
                    });
                    return `<div><strong>${partNumber}</strong><br><small>${partName}</small></div>`;
                }
            },
            { 
                data: 'unitPrice', 
                title: 'Giá Đơn Vị', 
                width: '10%',
                render: function(data) {
                    if (!data || data === 0) return '<span class="text-muted">Chưa có</span>';
                    return DataTablesUtility.renderCurrency(data);
                }
            },
            { 
                data: 'requestedQuantity', 
                title: 'Số Lượng YC', 
                width: '8%',
                className: 'text-center',
                render: function(data) {
                    return data || '<span class="text-muted">-</span>';
                }
            },
            { 
                data: 'requestedDate', 
                title: 'Ngày Yêu Cầu', 
                width: '10%',
                render: function(data) {
                    if (!data) return '<span class="text-muted">-</span>';
                    return DataTablesUtility.renderDate(data);
                }
            },
            { 
                data: 'responseDate', 
                title: 'Ngày Phản Hồi', 
                width: '10%',
                render: function(data) {
                    if (!data) return '<span class="text-muted">-</span>';
                    return DataTablesUtility.renderDate(data);
                }
            },
            { 
                data: 'status', 
                title: 'Trạng Thái', 
                width: '10%',
                render: function(data) {
                    var badgeClass = 'badge-secondary';
                    var text = data || 'N/A';
                    if (data === 'Requested') badgeClass = 'badge-info';
                    else if (data === 'Pending') badgeClass = 'badge-warning';
                    else if (data === 'Accepted') badgeClass = 'badge-success';
                    else if (data === 'Rejected') badgeClass = 'badge-danger';
                    else if (data === 'Expired') badgeClass = 'badge-dark';
                    return `<span class="badge ${badgeClass}">${text}</span>`;
                }
            },
            {
                data: null,
                title: 'Thao Tác',
                width: '15%',
                orderable: false,
                render: function(data, type, row) {
                    var actions = '';
                    actions += `<button class="btn btn-info btn-sm view-quotation" data-id="${row.id}" title="Xem chi tiết">
                        <i class="fas fa-eye"></i>
                    </button> `;
                    
                    // Only allow update if status is "Requested"
                    if (row.status === 'Requested') {
                        actions += `<button class="btn btn-warning btn-sm update-quotation" data-id="${row.id}" title="Cập nhật báo giá">
                            <i class="fas fa-edit"></i>
                        </button> `;
                    }
                    
                    // Only allow accept/reject if status is "Pending"
                    if (row.status === 'Pending') {
                        actions += `<button class="btn btn-success btn-sm accept-quotation" data-id="${row.id}" title="Chấp nhận">
                            <i class="fas fa-check"></i>
                        </button> `;
                        actions += `<button class="btn btn-danger btn-sm reject-quotation" data-id="${row.id}" title="Từ chối">
                            <i class="fas fa-times"></i>
                        </button>`;
                    }
                    
                    return actions || '<span class="text-muted">-</span>';
                }
            }
        ];
        
        this.quotationTable = DataTablesUtility.initServerSideTable('#quotationTable', '/Procurement/GetQuotations', columns, {
            order: [[0, 'desc']],
            pageLength: 20
        });
    },

    bindEvents: function() {
        var self = this;

        // Request quotation button
        $(document).on('click', '#requestQuotationBtn', function() {
            self.showRequestModal();
        });

        // Apply filters
        $(document).on('click', '#applyFiltersBtn', function() {
            self.applyFilters();
        });

        // Reset filters
        $(document).on('click', '#resetFiltersBtn', function() {
            self.resetFilters();
        });

        // Request quotation form
        $(document).on('submit', '#requestQuotationForm', function(e) {
            e.preventDefault();
            self.requestQuotation();
        });

        // Update quotation form
        $(document).on('submit', '#updateQuotationForm', function(e) {
            e.preventDefault();
            self.updateQuotation();
        });

        // View quotation
        $(document).on('click', '.view-quotation', function() {
            var id = $(this).data('id');
            self.viewQuotation(id);
        });

        // Update quotation
        $(document).on('click', '.update-quotation', function() {
            var id = $(this).data('id');
            self.showUpdateModal(id);
        });

        // Accept quotation
        $(document).on('click', '.accept-quotation', function() {
            var id = $(this).data('id');
            self.acceptQuotation(id);
        });

        // Reject quotation
        $(document).on('click', '.reject-quotation', function() {
            var id = $(this).data('id');
            self.rejectQuotation(id);
        });

        // Reset modals on close
        $('#requestQuotationModal').on('hidden.bs.modal', function() {
            self.resetRequestModal();
        });

        $('#updateQuotationModal').on('hidden.bs.modal', function() {
            self.resetUpdateModal();
        });
    },

    loadParts: function() {
        var self = this;
        $.ajax({
            url: '/Procurement/GetParts',
            type: 'GET'
        })
        .done(function(response) {
            if (Array.isArray(response)) {
                self.parts = response;
                var $select = $('#requestPartId, #filterPartId');
                $select.empty().append('<option value="">Tất cả</option>');
                response.forEach(function(part) {
                    $select.append(`<option value="${part.id}">${part.partNumber} - ${part.partName}</option>`);
                });
                $select.trigger('change');
            }
        })
        .fail(function() {
            GarageApp.showError('Không thể tải danh sách phụ tùng.');
        });
    },

    loadSuppliers: function() {
        var self = this;
        $.ajax({
            url: '/Procurement/GetSuppliers',
            type: 'GET'
        })
        .done(function(response) {
            if (Array.isArray(response)) {
                self.suppliers = response;
                var $select = $('#requestSupplierIds, #filterSupplierId');
                $select.empty().append('<option value="">Tất cả</option>');
                response.forEach(function(supplier) {
                    $select.append(`<option value="${supplier.id}">${supplier.supplierCode} - ${supplier.supplierName}</option>`);
                });
                $select.trigger('change');
            }
        })
        .fail(function() {
            GarageApp.showError('Không thể tải danh sách nhà cung cấp.');
        });
    },

    applyFilters: function() {
        var partId = $('#filterPartId').val();
        var supplierId = $('#filterSupplierId').val();
        var status = $('#filterStatus').val();

        var url = '/Procurement/GetQuotations?';
        var params = [];
        if (partId) params.push('partId=' + partId);
        if (supplierId) params.push('supplierId=' + supplierId);
        if (status) params.push('status=' + encodeURIComponent(status));
        
        this.quotationTable.ajax.url(url + params.join('&')).load();
    },

    resetFilters: function() {
        $('#filterPartId').val('').trigger('change');
        $('#filterSupplierId').val('').trigger('change');
        $('#filterStatus').val('');
        this.quotationTable.ajax.url('/Procurement/GetQuotations').load();
    },

    showRequestModal: function() {
        $('#requestQuotationModal').modal('show');
    },

    resetRequestModal: function() {
        $('#requestQuotationForm')[0].reset();
        $('#requestPartId').val('').trigger('change');
        $('#requestSupplierIds').val(null).trigger('change');
    },

    requestQuotation: function() {
        var self = this;
        var partId = $('#requestPartId').val();
        var supplierIds = $('#requestSupplierIds').val();
        var quantity = $('#requestQuantity').val();
        var notes = $('#requestNotes').val();
        var requiredByDate = $('#requiredByDate').val();

        if (!partId || !supplierIds || supplierIds.length === 0 || !quantity) {
            GarageApp.showWarning('Vui lòng điền đầy đủ thông tin bắt buộc.');
            return;
        }

        // ✅ VALIDATION: Validate quantity
        var quantityNum = parseInt(quantity);
        if (isNaN(quantityNum) || quantityNum <= 0) {
            GarageApp.showWarning('Số lượng yêu cầu phải lớn hơn 0.');
            return;
        }
        
        // ✅ VALIDATION: Remove duplicates and filter invalid IDs
        var validSupplierIds = supplierIds
            .map(function(id) { return parseInt(id); })
            .filter(function(id) { return !isNaN(id) && id > 0; });
        
        // Remove duplicates
        validSupplierIds = validSupplierIds.filter(function(id, index) {
            return validSupplierIds.indexOf(id) === index;
        });
        
        if (validSupplierIds.length === 0) {
            GarageApp.showWarning('Vui lòng chọn ít nhất một nhà cung cấp hợp lệ.');
            return;
        }
        
        var data = {
            partId: parseInt(partId),
            supplierIds: validSupplierIds,
            requestedQuantity: quantityNum,
            requestNotes: notes || null,
            requiredByDate: requiredByDate || null
        };

        GarageApp.showLoading('Đang gửi yêu cầu báo giá...');

        $.ajax({
            url: '/Procurement/RequestQuotation',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        })
        .done(function(response) {
            GarageApp.hideLoading();
            if (response.success) {
                GarageApp.showSuccess(response.message || 'Gửi yêu cầu báo giá thành công!');
                $('#requestQuotationModal').modal('hide');
                self.quotationTable.ajax.reload();
            } else {
                GarageApp.showError(response.errorMessage || 'Gửi yêu cầu báo giá thất bại.');
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (AuthHandler.isUnauthorized(xhr)) {
                AuthHandler.handleUnauthorized(xhr, true);
                return;
            }
            GarageApp.showError('Không thể gửi yêu cầu báo giá.');
        });
    },

    showUpdateModal: function(id) {
        var self = this;
        $('#updateQuotationId').val(id);
        
        // ✅ OPTIMIZED: Load quotation data to populate form
        GarageApp.showLoading('Đang tải thông tin...');
        
        $.ajax({
            url: '/Procurement/GetQuotation/' + id,
            type: 'GET'
        })
        .done(function(response) {
            GarageApp.hideLoading();
            if (response.success && response.data) {
                var q = response.data;
                $('#updateUnitPrice').val(q.unitPrice || 0);
                $('#updateMinOrderQty').val(q.minimumOrderQuantity || 1);
                $('#updateLeadTime').val(q.leadTimeDays || '');
                $('#updateValidUntil').val(q.validUntil ? q.validUntil.split('T')[0] : '');
                $('#updateWarrantyPeriod').val(q.warrantyPeriod || '');
                $('#updateWarrantyTerms').val(q.warrantyTerms || '');
                $('#updateResponseNotes').val(q.responseNotes || '');
                $('#updateQuotationModal').modal('show');
            } else {
                GarageApp.showError(response.errorMessage || 'Không thể tải thông tin báo giá.');
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (AuthHandler.isUnauthorized(xhr)) {
                AuthHandler.handleUnauthorized(xhr, true);
                return;
            }
            GarageApp.showError('Không thể tải thông tin báo giá.');
        });
    },

    resetUpdateModal: function() {
        $('#updateQuotationForm')[0].reset();
        $('#updateQuotationId').val('');
    },

    updateQuotation: function() {
        var self = this;
        var id = $('#updateQuotationId').val();
        
        // ✅ VALIDATION: Validate required fields
        var unitPrice = parseFloat($('#updateUnitPrice').val());
        var minOrderQty = parseInt($('#updateMinOrderQty').val());
        
        if (isNaN(unitPrice) || unitPrice <= 0) {
            GarageApp.showWarning('Vui lòng nhập giá đơn vị hợp lệ (lớn hơn 0).');
            return;
        }
        
        if (isNaN(minOrderQty) || minOrderQty <= 0) {
            GarageApp.showWarning('Vui lòng nhập số lượng tối thiểu hợp lệ (lớn hơn 0).');
            return;
        }
        
        var data = {
            unitPrice: unitPrice,
            minimumOrderQuantity: minOrderQty,
            leadTimeDays: $('#updateLeadTime').val() ? parseInt($('#updateLeadTime').val()) : null,
            validUntil: $('#updateValidUntil').val() || null,
            warrantyPeriod: $('#updateWarrantyPeriod').val() || null,
            warrantyTerms: $('#updateWarrantyTerms').val() || null,
            responseNotes: $('#updateResponseNotes').val() || null,
            status: 'Pending'
        };

        GarageApp.showLoading('Đang cập nhật báo giá...');

        $.ajax({
            url: '/Procurement/UpdateQuotation/' + id,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        })
        .done(function(response) {
            GarageApp.hideLoading();
            if (response.success) {
                GarageApp.showSuccess(response.message || 'Cập nhật báo giá thành công!');
                $('#updateQuotationModal').modal('hide');
                self.quotationTable.ajax.reload();
            } else {
                GarageApp.showError(response.errorMessage || 'Cập nhật báo giá thất bại.');
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (AuthHandler.isUnauthorized(xhr)) {
                AuthHandler.handleUnauthorized(xhr, true);
                return;
            }
            GarageApp.showError('Không thể cập nhật báo giá.');
        });
    },

    viewQuotation: function(id) {
        var self = this;
        $('#viewQuotationContent').html('<p><i class="fas fa-spinner fa-spin"></i> Đang tải thông tin...</p>');
        $('#viewQuotationModal').modal('show');
        
        // ✅ OPTIMIZED: Load quotation details via API
        $.ajax({
            url: '/Procurement/GetQuotation/' + id,
            type: 'GET'
        })
        .done(function(response) {
            if (response.success && response.data) {
                var q = response.data;
                var html = `
                    <div class="row">
                        <div class="col-md-6">
                            <h5>Thông Tin Cơ Bản</h5>
                            <table class="table table-sm">
                                <tr><th width="40%">Số Báo Giá:</th><td>${self.escapeHtml(q.quotationNumber)}</td></tr>
                                <tr><th>Nhà Cung Cấp:</th><td>${self.escapeHtml(q.supplierName)} (${self.escapeHtml(q.supplierCode)})</td></tr>
                                <tr><th>Phụ Tùng:</th><td>${self.escapeHtml(q.partNumber)} - ${self.escapeHtml(q.partName)}</td></tr>
                                <tr><th>Trạng Thái:</th><td><span class="badge badge-${self.getStatusBadgeClass(q.status)}">${self.escapeHtml(q.status)}</span></td></tr>
                                <tr><th>Ngày Yêu Cầu:</th><td>${q.requestedDate ? new Date(q.requestedDate).toLocaleDateString('vi-VN') : '-'}</td></tr>
                                <tr><th>Ngày Phản Hồi:</th><td>${q.responseDate ? new Date(q.responseDate).toLocaleDateString('vi-VN') : '-'}</td></tr>
                            </table>
                        </div>
                        <div class="col-md-6">
                            <h5>Chi Tiết Báo Giá</h5>
                            <table class="table table-sm">
                                <tr><th width="40%">Giá Đơn Vị:</th><td>${q.unitPrice ? q.unitPrice.toLocaleString('vi-VN') + ' VNĐ' : 'Chưa có'}</td></tr>
                                <tr><th>Số Lượng Yêu Cầu:</th><td>${q.requestedQuantity || '-'}</td></tr>
                                <tr><th>Số Lượng Tối Thiểu:</th><td>${q.minimumOrderQuantity || '-'}</td></tr>
                                <tr><th>Thời Gian Giao Hàng:</th><td>${q.leadTimeDays ? q.leadTimeDays + ' ngày' : '-'}</td></tr>
                                <tr><th>Ngày Hết Hạn:</th><td>${q.validUntil ? new Date(q.validUntil).toLocaleDateString('vi-VN') : '-'}</td></tr>
                                <tr><th>Bảo Hành:</th><td>${self.escapeHtml(q.warrantyPeriod || '-')}</td></tr>
                            </table>
                        </div>
                    </div>
                    ${q.requestNotes ? `<div class="mt-3"><strong>Ghi Chú Yêu Cầu:</strong><p class="text-muted">${self.escapeHtml(q.requestNotes)}</p></div>` : ''}
                    ${q.responseNotes ? `<div class="mt-3"><strong>Ghi Chú Phản Hồi:</strong><p class="text-muted">${self.escapeHtml(q.responseNotes)}</p></div>` : ''}
                    ${q.notes ? `<div class="mt-3"><strong>Ghi Chú:</strong><p class="text-muted">${self.escapeHtml(q.notes)}</p></div>` : ''}
                `;
                $('#viewQuotationContent').html(html);
            } else {
                $('#viewQuotationContent').html('<p class="text-danger">Không thể tải thông tin báo giá.</p>');
            }
        })
        .fail(function(xhr) {
            if (AuthHandler.isUnauthorized(xhr)) {
                AuthHandler.handleUnauthorized(xhr, true);
                return;
            }
            $('#viewQuotationContent').html('<p class="text-danger">Lỗi khi tải thông tin báo giá.</p>');
        });
    },
    
    escapeHtml: function(text) {
        if (!text) return '';
        var map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, function(m) { return map[m]; });
    },
    
    getStatusBadgeClass: function(status) {
        // ✅ SECURITY: Map status to badge class safely (prevent XSS)
        if (!status) return 'info';
        var statusLower = status.toLowerCase();
        if (statusLower === 'accepted') return 'success';
        if (statusLower === 'pending') return 'warning';
        if (statusLower === 'rejected') return 'danger';
        if (statusLower === 'requested') return 'info';
        if (statusLower === 'expired') return 'dark';
        return 'info'; // Default
    },

    acceptQuotation: function(id) {
        var self = this;
        if (!confirm('Bạn có chắc chắn muốn chấp nhận báo giá này?')) {
            return;
        }

        GarageApp.showLoading('Đang xử lý...');

        $.ajax({
            url: '/Procurement/AcceptQuotation/' + id,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({}),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        })
        .done(function(response) {
            GarageApp.hideLoading();
            if (response.success) {
                GarageApp.showSuccess(response.message || 'Chấp nhận báo giá thành công!');
                self.quotationTable.ajax.reload();
            } else {
                GarageApp.showError(response.errorMessage || 'Chấp nhận báo giá thất bại.');
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (AuthHandler.isUnauthorized(xhr)) {
                AuthHandler.handleUnauthorized(xhr, true);
                return;
            }
            GarageApp.showError('Không thể chấp nhận báo giá.');
        });
    },

    rejectQuotation: function(id) {
        var self = this;
        var notes = prompt('Nhập lý do từ chối (tùy chọn):');
        if (notes === null) return; // User cancelled

        GarageApp.showLoading('Đang xử lý...');

        $.ajax({
            url: '/Procurement/RejectQuotation/' + id,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ notes: notes || null }),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        })
        .done(function(response) {
            GarageApp.hideLoading();
            if (response.success) {
                GarageApp.showSuccess(response.message || 'Từ chối báo giá thành công!');
                self.quotationTable.ajax.reload();
            } else {
                GarageApp.showError(response.errorMessage || 'Từ chối báo giá thất bại.');
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (AuthHandler.isUnauthorized(xhr)) {
                AuthHandler.handleUnauthorized(xhr, true);
                return;
            }
            GarageApp.showError('Không thể từ chối báo giá.');
        });
    }
};

