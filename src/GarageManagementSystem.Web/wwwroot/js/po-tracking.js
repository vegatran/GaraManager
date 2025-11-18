/**
 * PO Tracking Module - Phase 4.2.3
 * 
 * Handles PO tracking dashboard and tracking updates
 */

window.POTracking = {
    trackingTable: null,
    currentPage: 1,
    pageSize: 20,
    suppliers: [],
    cache: new Map(), // ✅ OPTIMIZED: Cache for delivery alerts and search results
    searchTimeout: null, // ✅ OPTIMIZED: For debouncing filter inputs
    lastSearchParams: null, // ✅ OPTIMIZED: Track last search to avoid duplicate calls
    initialized: false, // ✅ SAFETY: Prevent multiple initialization

    init: function() {
        // ✅ SAFETY: Prevent multiple initialization
        if (this.initialized) {
            return;
        }
        this.initialized = true;

        this.initDataTable();
        this.bindEvents();
        // ✅ OPTIMIZED: Load suppliers and alerts in parallel
        Promise.all([
            this.loadSuppliers(),
            this.loadDeliveryAlerts()
        ]).then(() => {
            this.loadInTransitOrders();
        }).catch(function(error) {
            // ✅ SAFETY: Handle promise rejection
            console.error('Error loading initial data:', error);
            GarageApp.showError('Có lỗi xảy ra khi tải dữ liệu. Vui lòng thử lại.');
        });
    },

    initDataTable: function() {
        var self = this;
        
        this.trackingTable = $('#trackingTable').DataTable({
            processing: true,
            serverSide: false,
            paging: true,
            searching: false,
            ordering: true,
            info: true,
            autoWidth: false,
            pageLength: self.pageSize,
            deferRender: true, // ✅ OPTIMIZED: Defer rendering for better performance
            language: {
                url: '//cdn.datatables.net/plug-ins/1.11.5/i18n/vi.json'
            },
            columns: [
                { data: 'orderNumber', title: 'Mã PO', width: '10%' },
                { data: 'supplierName', title: 'Nhà Cung Cấp', width: '15%' },
                { 
                    data: 'orderDate', 
                    title: 'Ngày Đặt', 
                    width: '10%',
                    render: function(data) {
                        return data ? new Date(data).toLocaleDateString('vi-VN') : '-';
                    }
                },
                { 
                    data: 'sentDate', 
                    title: 'Ngày Gửi', 
                    width: '10%',
                    render: function(data) {
                        return data ? new Date(data).toLocaleDateString('vi-VN') : '-';
                    }
                },
                { 
                    data: 'expectedDeliveryDate', 
                    title: 'Ngày Dự Kiến', 
                    width: '10%',
                    render: function(data) {
                        return data ? new Date(data).toLocaleDateString('vi-VN') : '-';
                    }
                },
                { 
                    data: 'trackingNumber', 
                    title: 'Mã Vận Đơn', 
                    width: '12%',
                    render: function(data) {
                        return data || '-';
                    }
                },
                { 
                    data: 'deliveryStatus', 
                    title: 'Trạng Thái', 
                    width: '10%',
                    render: function(data, type, row) {
                        return self.renderDeliveryStatus(data, row.daysUntilDelivery);
                    }
                },
                { 
                    data: 'daysUntilDelivery', 
                    title: 'Số Ngày', 
                    width: '8%',
                    render: function(data) {
                        if (data === null || data === undefined) return '-';
                        if (data < 0) {
                            return '<span class="text-danger">' + Math.abs(data) + ' ngày quá hạn</span>';
                        } else if (data === 0) {
                            return '<span class="text-warning">Hôm nay</span>';
                        } else {
                            return '<span class="text-info">Còn ' + data + ' ngày</span>';
                        }
                    }
                },
                { 
                    data: null, 
                    title: 'Thao Tác', 
                    orderable: false,
                    width: '15%',
                    render: function(data, type, row) {
                        return `
                            <button class="btn btn-sm btn-info btn-view-timeline" data-id="${row.id}" title="Xem lịch sử">
                                <i class="fas fa-history"></i>
                            </button>
                            <button class="btn btn-sm btn-primary btn-update-tracking" data-id="${row.id}" title="Cập nhật tracking">
                                <i class="fas fa-edit"></i>
                            </button>
                        `;
                    }
                }
            ],
            order: [[4, 'asc']] // Sort by ExpectedDeliveryDate
        });
    },

    bindEvents: function() {
        var self = this;

        // ✅ OPTIMIZED: Debounced search function
        var performSearch = function() {
            self.currentPage = 1;
            self.loadInTransitOrders();
        };

        // Search button
        $('#btnSearch').on('click', function() {
            if (self.searchTimeout) {
                clearTimeout(self.searchTimeout);
                self.searchTimeout = null;
            }
            performSearch();
        });

        // ✅ OPTIMIZED: Debounce filter changes
        $('#supplierFilter, #deliveryStatusFilter').on('change', function() {
            if (self.searchTimeout) {
                clearTimeout(self.searchTimeout);
            }
            self.searchTimeout = setTimeout(performSearch, 300);
        });

        // ✅ OPTIMIZED: Debounce daysUntilFilter input
        $('#daysUntilFilter').on('input', function() {
            if (self.searchTimeout) {
                clearTimeout(self.searchTimeout);
            }
            self.searchTimeout = setTimeout(performSearch, 500);
        });

        // Refresh button
        $('#btnRefresh').on('click', function() {
            if (self.searchTimeout) {
                clearTimeout(self.searchTimeout);
                self.searchTimeout = null;
            }
            self.cache.clear();
            self.lastSearchParams = null;
            self.loadDeliveryAlerts();
            self.loadInTransitOrders();
        });

        // Filter by status cards
        $('#filterDelayed').on('click', function(e) {
            e.preventDefault();
            $('#deliveryStatusFilter').val('Delayed').trigger('change');
        });

        $('#filterAtRisk').on('click', function(e) {
            e.preventDefault();
            $('#deliveryStatusFilter').val('AtRisk').trigger('change');
        });

        $('#filterOnTime').on('click', function(e) {
            e.preventDefault();
            $('#deliveryStatusFilter').val('OnTime').trigger('change');
        });

        // Update tracking button
        $(document).on('click', '.btn-update-tracking', function() {
            var poId = $(this).data('id');
            self.showUpdateTrackingModal(poId);
        });

        // View timeline button
        $(document).on('click', '.btn-view-timeline', function() {
            var poId = $(this).data('id');
            self.showTimelineModal(poId);
        });

        // Save tracking button
        $('#btnSaveTracking').on('click', function() {
            self.saveTracking();
        });

        // ✅ SAFETY: Reset modal form when closed
        $('#updateTrackingModal').on('hidden.bs.modal', function() {
            $('#updateTrackingForm')[0]?.reset();
            $('#updatePoId').val('');
            $('#markAsInTransit').prop('checked', false);
            // Clear any validation errors
            $('#updateTrackingForm').find('.is-invalid').removeClass('is-invalid');
            $('#updateTrackingForm').find('.invalid-feedback').remove();
        });

        // ✅ SAFETY: Clear search timeout on page unload
        $(window).on('beforeunload', function() {
            if (self.searchTimeout) {
                clearTimeout(self.searchTimeout);
                self.searchTimeout = null;
            }
        });

        // Mark as InTransit button (if needed)
        $(document).on('click', '.btn-mark-in-transit', function() {
            var poId = $(this).data('id');
            self.markAsInTransit(poId);
        });
    },

    loadSuppliers: function() {
        var self = this;
        
        // ✅ OPTIMIZED: Return promise for parallel loading
        return new Promise(function(resolve, reject) {
            $.ajax({
                url: '/Procurement/GetSuppliers',
                type: 'GET'
            })
            .done(function(response) {
                // ✅ FIX: ProcurementController.GetSuppliers trả về List<SupplierDto> trực tiếp
                if (Array.isArray(response)) {
                    self.suppliers = response;
                    
                    var supplierSelect = $('#supplierFilter');
                    
                    // ✅ SAFETY: Destroy existing Select2 before re-init
                    if (supplierSelect.hasClass('select2-hidden-accessible')) {
                        supplierSelect.select2('destroy');
                    }
                    
                    supplierSelect.empty().append('<option value="">-- Tất cả --</option>');
                    
                    self.suppliers.forEach(function(supplier) {
                        if (supplier && supplier.id && supplier.supplierName) {
                            supplierSelect.append(`<option value="${supplier.id}">${supplier.supplierName}</option>`);
                        }
                    });
                    
                    supplierSelect.select2({
                        placeholder: 'Chọn nhà cung cấp',
                        allowClear: true,
                        delay: 250 // ✅ OPTIMIZED: Add delay for Select2
                    });
                }
                resolve();
            })
            .fail(function(xhr) {
                console.error('Error loading suppliers:', xhr);
                resolve(); // Resolve anyway to not block parallel loading
            });
        });
    },

    loadDeliveryAlerts: function() {
        var self = this;
        
        // ✅ OPTIMIZED: Return promise for parallel loading
        return new Promise(function(resolve, reject) {
            // ✅ OPTIMIZED: Check cache first
            var cacheKey = 'delivery_alerts';
            if (self.cache.has(cacheKey)) {
                var cachedData = self.cache.get(cacheKey);
                self.updateSummaryCards(cachedData);
                resolve();
                return;
            }

            $.ajax({
                url: '/PurchaseOrder/GetDeliveryAlerts',
                type: 'GET'
            })
            .done(function(response) {
                // ✅ FIX: PurchaseOrderController.GetDeliveryAlerts trả về ApiResponse<DeliveryAlertsDto>
                // ApiResponse có Success (capital S) và Data (capital D)
                if (response && (response.Success || response.success) && (response.Data || response.data)) {
                    var alertsData = response.Data || response.data;
                    self.cache.set(cacheKey, alertsData);
                    // ✅ OPTIMIZED: Cache size limit is now handled in loadInTransitOrders
                    self.updateSummaryCards(alertsData);
                }
                resolve();
            })
            .fail(function(xhr) {
                if (xhr.status === 401) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    console.error('Error loading delivery alerts:', xhr);
                }
                resolve(); // Resolve anyway to not block parallel loading
            });
        });
    },

    loadInTransitOrders: function() {
        var self = this;
        
        var supplierId = $('#supplierFilter').val() || null;
        var deliveryStatus = $('#deliveryStatusFilter').val() || null;
        var daysUntilInput = $('#daysUntilFilter').val();
        var daysUntilDelivery = null;
        
        // ✅ SAFETY: Validate parseInt result
        if (daysUntilInput) {
            var parsed = parseInt(daysUntilInput, 10);
            if (!isNaN(parsed) && parsed >= 0) {
                daysUntilDelivery = parsed;
            }
        }

        // ✅ OPTIMIZED: Build cache key from search parameters
        var cacheKey = `in_transit_${self.currentPage}_${self.pageSize}_${supplierId || 'all'}_${deliveryStatus || 'all'}_${daysUntilDelivery || 'all'}`;
        var searchParams = {
            pageNumber: self.currentPage,
            pageSize: self.pageSize,
            supplierId: supplierId,
            deliveryStatus: deliveryStatus,
            daysUntilDelivery: daysUntilDelivery
        };

        // ✅ OPTIMIZED: Check if same search params to avoid duplicate calls
        // ✅ SAFETY: Safe JSON.stringify with try-catch
        try {
            if (self.lastSearchParams) {
                var lastParamsStr = JSON.stringify(self.lastSearchParams);
                var currentParamsStr = JSON.stringify(searchParams);
                if (lastParamsStr === currentParamsStr) {
                    return;
                }
            }
        } catch (e) {
            // If JSON.stringify fails, continue with API call
            console.warn('Error comparing search params:', e);
        }

        // ✅ OPTIMIZED: Check cache first
        if (self.cache.has(cacheKey)) {
            var cachedData = self.cache.get(cacheKey);
            self.trackingTable.clear();
            if (Array.isArray(cachedData.data) && cachedData.data.length > 0) {
                self.trackingTable.rows.add(cachedData.data);
            }
            self.trackingTable.draw();
            $('#totalCount').text(cachedData.totalCount || 0);
            self.lastSearchParams = searchParams;
            return;
        }

        GarageApp.showLoading('Đang tải danh sách PO...');

        $.ajax({
            url: '/PurchaseOrder/GetInTransitOrders',
            type: 'GET',
            data: searchParams
        })
        .done(function(response) {
            GarageApp.hideLoading();
            
            // ✅ FIX: PurchaseOrderController.GetInTransitOrders trả về PagedResponse<PurchaseOrderDto> trực tiếp
            // PagedResponse có Success (capital S) và Data (capital D)
            if (response && (response.Success || response.success) && (response.Data || response.data)) {
                var data = response.Data || response.data;
                var totalCount = response.TotalCount || response.totalCount || 0;
                
                // ✅ OPTIMIZED: Cache the result
                self.cache.set(cacheKey, { data: data, totalCount: totalCount });
                // Limit cache size to 20 entries
                if (self.cache.size > 20) {
                    var firstKey = self.cache.keys().next().value;
                    self.cache.delete(firstKey);
                }
                
                self.trackingTable.clear();
                if (Array.isArray(data) && data.length > 0) {
                    self.trackingTable.rows.add(data);
                }
                self.trackingTable.draw();
                
                // Update total count
                $('#totalCount').text(totalCount);
                self.lastSearchParams = searchParams;
            } else {
                var message = response.Message || response.message || 'Không có dữ liệu.';
                GarageApp.showWarning(message);
                self.trackingTable.clear().draw();
                self.lastSearchParams = searchParams;
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (xhr.status === 401) {
                AuthHandler.handleUnauthorized(xhr, true);
            } else {
                var errorMsg = xhr.responseJSON?.errorMessage || xhr.responseJSON?.message || 'Lỗi khi tải danh sách PO.';
                GarageApp.showError(errorMsg);
            }
        });
    },

    updateSummaryCards: function(alertsData) {
        $('#delayedCount').text(alertsData.delayedCount || 0);
        $('#atRiskCount').text(alertsData.atRiskCount || 0);
        
        // Calculate on-time count from total
        var onTimeCount = (alertsData.totalCount || 0) - (alertsData.delayedCount || 0) - (alertsData.atRiskCount || 0);
        $('#onTimeCount').text(Math.max(0, onTimeCount));
    },

    renderDeliveryStatus: function(status, daysUntilDelivery) {
        if (!status) return '<span class="badge badge-secondary">-</span>';
        
        var badgeClass = 'badge-secondary';
        var text = '';
        
        switch(status) {
            case 'OnTime':
                badgeClass = 'badge-success';
                text = 'Đúng Hạn';
                break;
            case 'AtRisk':
                badgeClass = 'badge-warning';
                text = 'Sắp Đến Hạn';
                break;
            case 'Delayed':
                badgeClass = 'badge-danger';
                text = 'Quá Hạn';
                break;
            default:
                badgeClass = 'badge-secondary';
                text = status;
        }
        
        return `<span class="badge ${badgeClass}">${text}</span>`;
    },

    showUpdateTrackingModal: function(poId) {
        var self = this;
        
        // ✅ SAFETY: Validate poId
        if (!poId) {
            GarageApp.showError('ID PO không hợp lệ.');
            return;
        }
        
        var poIdNum = parseInt(poId, 10);
        if (isNaN(poIdNum) || poIdNum <= 0) {
            GarageApp.showError('ID PO không hợp lệ.');
            return;
        }
        
        GarageApp.showLoading('Đang tải thông tin tracking...');

        $.ajax({
            url: '/PurchaseOrder/GetTrackingInfo/' + poIdNum,
            type: 'GET'
        })
        .done(function(response) {
            GarageApp.hideLoading();
            
            // ✅ FIX: PurchaseOrderController.GetTrackingInfo trả về ApiResponse<PurchaseOrderTrackingDto>
            // ApiResponse có Success (capital S) và Data (capital D)
            if (response && (response.Success || response.success) && (response.Data || response.data)) {
                var data = response.Data || response.data;
                
                $('#updatePoId').val(poIdNum);
                $('#updateTrackingNumber').val(data.trackingNumber || '');
                $('#updateShippingMethod').val(data.shippingMethod || '');
                
                // ✅ SAFETY: Safe date parsing
                var formatDateForInput = function(dateValue) {
                    if (!dateValue) return '';
                    try {
                        var date = new Date(dateValue);
                        if (isNaN(date.getTime())) return '';
                        return date.toISOString().split('T')[0];
                    } catch (e) {
                        return '';
                    }
                };
                
                $('#updateExpectedDeliveryDate').val(formatDateForInput(data.expectedDeliveryDate));
                $('#updateInTransitDate').val(formatDateForInput(data.inTransitDate));
                $('#updateDeliveryNotes').val(data.deliveryNotes || '');
                $('#markAsInTransit').prop('checked', false);
                
                $('#updateTrackingModal').modal('show');
            } else {
                var errorMsg = response.ErrorMessage || response.errorMessage || 'Không thể tải thông tin tracking.';
                GarageApp.showError(errorMsg);
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (xhr.status === 401) {
                AuthHandler.handleUnauthorized(xhr, true);
            } else {
                var errorMsg = xhr.responseJSON?.errorMessage || xhr.responseJSON?.message || 'Lỗi khi tải thông tin tracking.';
                GarageApp.showError(errorMsg);
            }
        });
    },

    saveTracking: function() {
        var self = this;
        var poId = $('#updatePoId').val();
        
        if (!poId) {
            GarageApp.showWarning('Không tìm thấy ID PO.');
            return;
        }

        var dto = {
            trackingNumber: $('#updateTrackingNumber').val() || null,
            shippingMethod: $('#updateShippingMethod').val() || null,
            expectedDeliveryDate: $('#updateExpectedDeliveryDate').val() || null,
            inTransitDate: $('#updateInTransitDate').val() || null,
            deliveryNotes: $('#updateDeliveryNotes').val() || null,
            markAsInTransit: $('#markAsInTransit').is(':checked')
        };

        // Remove null/empty values
        Object.keys(dto).forEach(key => {
            if (dto[key] === null || dto[key] === '') {
                delete dto[key];
            }
        });

        // ✅ SAFETY: Validate poId is a valid number
        var poIdNum = parseInt(poId, 10);
        if (isNaN(poIdNum) || poIdNum <= 0) {
            GarageApp.showError('ID PO không hợp lệ.');
            return;
        }

        // ✅ SAFETY: Safe JSON.stringify
        var jsonData;
        try {
            jsonData = JSON.stringify(dto);
        } catch (e) {
            GarageApp.showError('Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.');
            console.error('JSON.stringify error:', e);
            return;
        }

        GarageApp.showLoading('Đang cập nhật...');

        $.ajax({
            url: '/PurchaseOrder/UpdateTracking/' + poIdNum,
            type: 'PUT',
            contentType: 'application/json',
            data: jsonData,
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        })
        .done(function(response) {
            GarageApp.hideLoading();
            
            // ✅ FIX: PurchaseOrderController.UpdateTracking trả về ApiResponse<PurchaseOrderDto>
            if (response && (response.Success || response.success)) {
                GarageApp.showSuccess('Cập nhật thông tin tracking thành công.');
                $('#updateTrackingModal').modal('hide');
                // ✅ OPTIMIZED: Clear cache and reload
                self.cache.clear();
                self.lastSearchParams = null;
                self.loadDeliveryAlerts();
                self.loadInTransitOrders();
            } else {
                var errorMsg = response.ErrorMessage || response.errorMessage || 'Lỗi khi cập nhật thông tin tracking.';
                GarageApp.showError(errorMsg);
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (xhr.status === 401) {
                AuthHandler.handleUnauthorized(xhr, true);
            } else {
                var errorMsg = xhr.responseJSON?.errorMessage || xhr.responseJSON?.message || 'Lỗi khi cập nhật thông tin tracking.';
                GarageApp.showError(errorMsg);
            }
        });
    },

    showTimelineModal: function(poId) {
        var self = this;
        
        // ✅ SAFETY: Validate poId
        if (!poId) {
            GarageApp.showError('ID PO không hợp lệ.');
            return;
        }
        
        var poIdNum = parseInt(poId, 10);
        if (isNaN(poIdNum) || poIdNum <= 0) {
            GarageApp.showError('ID PO không hợp lệ.');
            return;
        }
        
        GarageApp.showLoading('Đang tải lịch sử...');

        $.ajax({
            url: '/PurchaseOrder/GetTrackingInfo/' + poIdNum,
            type: 'GET'
        })
        .done(function(response) {
            GarageApp.hideLoading();
            
            // ✅ FIX: PurchaseOrderController.GetTrackingInfo trả về ApiResponse<PurchaseOrderTrackingDto>
            if (response && (response.Success || response.success) && (response.Data || response.data)) {
                var data = response.Data || response.data;
                self.renderTimeline(data);
                $('#timelineModal').modal('show');
            } else {
                var errorMsg = response.ErrorMessage || response.errorMessage || 'Không thể tải lịch sử.';
                GarageApp.showError(errorMsg);
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (xhr.status === 401) {
                AuthHandler.handleUnauthorized(xhr, true);
            } else {
                var errorMsg = xhr.responseJSON?.errorMessage || xhr.responseJSON?.message || 'Lỗi khi tải lịch sử.';
                GarageApp.showError(errorMsg);
            }
        });
    },

    renderTimeline: function(trackingData) {
        // ✅ SAFETY: Null check
        if (!trackingData) {
            $('#timelineContent').html('<p class="text-muted">Không có dữ liệu tracking.</p>');
            return;
        }

        // ✅ SAFETY: Helper function to safely format date
        var formatDate = function(dateValue) {
            if (!dateValue) return '-';
            try {
                var date = new Date(dateValue);
                if (isNaN(date.getTime())) return '-';
                return date.toLocaleString('vi-VN');
            } catch (e) {
                return '-';
            }
        };

        // ✅ SAFETY: Helper function to escape HTML (basic)
        var escapeHtml = function(text) {
            if (!text) return '';
            var div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        };

        var orderNumber = escapeHtml(trackingData.orderNumber || 'N/A');
        var supplierName = escapeHtml(trackingData.supplierName || 'N/A');
        
        var timelineHtml = `
            <div class="timeline">
                <div class="time-label">
                    <span class="bg-blue">PO: ${orderNumber}</span>
                </div>
                
                <div>
                    <i class="fas fa-shopping-cart bg-blue"></i>
                    <div class="timeline-item">
                        <span class="time"><i class="fas fa-clock"></i> ${formatDate(trackingData.orderDate)}</span>
                        <h3 class="timeline-header">Đặt Hàng</h3>
                        <div class="timeline-body">
                            Nhà cung cấp: <strong>${supplierName}</strong>
                        </div>
                    </div>
                </div>
        `;

        if (trackingData.sentDate) {
            timelineHtml += `
                <div>
                    <i class="fas fa-paper-plane bg-green"></i>
                    <div class="timeline-item">
                        <span class="time"><i class="fas fa-clock"></i> ${formatDate(trackingData.sentDate)}</span>
                        <h3 class="timeline-header">Đã Gửi</h3>
                        <div class="timeline-body">
                            PO đã được gửi cho nhà cung cấp
                        </div>
                    </div>
                </div>
            `;
        }

        if (trackingData.inTransitDate) {
            var trackingNumber = trackingData.trackingNumber ? escapeHtml(trackingData.trackingNumber) : '';
            var shippingMethod = trackingData.shippingMethod ? escapeHtml(trackingData.shippingMethod) : '';
            
            timelineHtml += `
                <div>
                    <i class="fas fa-truck bg-yellow"></i>
                    <div class="timeline-item">
                        <span class="time"><i class="fas fa-clock"></i> ${formatDate(trackingData.inTransitDate)}</span>
                        <h3 class="timeline-header">Đang Vận Chuyển</h3>
                        <div class="timeline-body">
                            ${trackingNumber ? 'Mã vận đơn: <strong>' + trackingNumber + '</strong><br>' : ''}
                            ${shippingMethod ? 'Phương thức: <strong>' + shippingMethod + '</strong>' : ''}
                        </div>
                    </div>
                </div>
            `;
        }

        if (trackingData.expectedDeliveryDate) {
            var statusClass = 'bg-info';
            var statusText = 'Dự Kiến';
            if (trackingData.deliveryStatus === 'Delayed') {
                statusClass = 'bg-danger';
                statusText = 'Quá Hạn';
            } else if (trackingData.deliveryStatus === 'AtRisk') {
                statusClass = 'bg-warning';
                statusText = 'Sắp Đến Hạn';
            }
            
            var daysText = '';
            if (trackingData.daysUntilDelivery !== null && trackingData.daysUntilDelivery !== undefined) {
                if (trackingData.daysUntilDelivery < 0) {
                    daysText = '<br><strong>' + Math.abs(trackingData.daysUntilDelivery) + ' ngày quá hạn</strong>';
                } else {
                    daysText = '<br><strong>Còn ' + trackingData.daysUntilDelivery + ' ngày</strong>';
                }
            }
            
            timelineHtml += `
                <div>
                    <i class="fas fa-calendar-check ${statusClass}"></i>
                    <div class="timeline-item">
                        <span class="time"><i class="fas fa-clock"></i> ${formatDate(trackingData.expectedDeliveryDate)}</span>
                        <h3 class="timeline-header">${statusText}</h3>
                        <div class="timeline-body">
                            Ngày dự kiến nhận hàng
                            ${daysText}
                        </div>
                    </div>
                </div>
            `;
        }

        if (trackingData.actualDeliveryDate) {
            timelineHtml += `
                <div>
                    <i class="fas fa-check-circle bg-success"></i>
                    <div class="timeline-item">
                        <span class="time"><i class="fas fa-clock"></i> ${formatDate(trackingData.actualDeliveryDate)}</span>
                        <h3 class="timeline-header">Đã Nhận Hàng</h3>
                        <div class="timeline-body">
                            PO đã được nhận và nhập kho
                        </div>
                    </div>
                </div>
            `;
        }

        // Add status history
        if (trackingData.statusHistory && Array.isArray(trackingData.statusHistory) && trackingData.statusHistory.length > 0) {
            trackingData.statusHistory.forEach(function(history) {
                if (!history) return; // ✅ SAFETY: Skip null entries
                
                var iconClass = 'bg-gray';
                switch(history.status) {
                    case 'Sent': iconClass = 'bg-green'; break;
                    case 'InTransit': iconClass = 'bg-yellow'; break;
                    case 'Received': iconClass = 'bg-success'; break;
                    case 'Delayed': iconClass = 'bg-danger'; break;
                }
                
                var statusText = escapeHtml(history.status || '');
                var notesHtml = '';
                if (history.notes) {
                    notesHtml = '<div class="timeline-body">' + escapeHtml(history.notes) + '</div>';
                }
                
                timelineHtml += `
                    <div>
                        <i class="fas fa-info-circle ${iconClass}"></i>
                        <div class="timeline-item">
                            <span class="time"><i class="fas fa-clock"></i> ${formatDate(history.statusDate)}</span>
                            <h3 class="timeline-header">${statusText}</h3>
                            ${notesHtml}
                        </div>
                    </div>
                `;
            });
        }

        timelineHtml += '</div>';
        
        $('#timelineContent').html(timelineHtml);
    },

    markAsInTransit: function(poId) {
        var self = this;
        
        // ✅ SAFETY: Validate poId
        if (!poId) {
            GarageApp.showError('ID PO không hợp lệ.');
            return;
        }
        
        var poIdNum = parseInt(poId, 10);
        if (isNaN(poIdNum) || poIdNum <= 0) {
            GarageApp.showError('ID PO không hợp lệ.');
            return;
        }
        
        if (!confirm('Bạn có chắc chắn muốn đánh dấu PO này đã gửi hàng?')) {
            return;
        }

        GarageApp.showLoading('Đang cập nhật...');

        // ✅ SAFETY: Safe JSON.stringify
        var jsonData;
        try {
            jsonData = JSON.stringify({});
        } catch (e) {
            GarageApp.showError('Có lỗi xảy ra. Vui lòng thử lại.');
            console.error('JSON.stringify error:', e);
            return;
        }

        $.ajax({
            url: '/PurchaseOrder/MarkAsInTransit/' + poIdNum,
            type: 'PUT',
            contentType: 'application/json',
            data: jsonData,
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        })
        .done(function(response) {
            GarageApp.hideLoading();
            
            // ✅ FIX: PurchaseOrderController.MarkAsInTransit trả về ApiResponse<PurchaseOrderDto>
            if (response && (response.Success || response.success)) {
                GarageApp.showSuccess('Đã đánh dấu PO đã gửi hàng thành công.');
                // ✅ OPTIMIZED: Clear cache and reload
                self.cache.clear();
                self.lastSearchParams = null;
                self.loadDeliveryAlerts();
                self.loadInTransitOrders();
            } else {
                var errorMsg = response.ErrorMessage || response.errorMessage || 'Lỗi khi đánh dấu PO đã gửi hàng.';
                GarageApp.showError(errorMsg);
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (xhr.status === 401) {
                AuthHandler.handleUnauthorized(xhr, true);
            } else {
                var errorMsg = xhr.responseJSON?.errorMessage || xhr.responseJSON?.message || 'Lỗi khi đánh dấu PO đã gửi hàng.';
                GarageApp.showError(errorMsg);
            }
        });
    }
};

// Initialize on page load
$(document).ready(function() {
    if ($('#trackingTable').length > 0) {
        POTracking.init();
    }
});

