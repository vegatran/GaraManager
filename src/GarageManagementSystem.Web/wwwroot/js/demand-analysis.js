/**
 * Demand Analysis Module - Phase 4.2.1
 * 
 * Handles demand analysis and bulk PO creation
 */

window.DemandAnalysis = {
    demandTable: null,
    selectedItems: new Map(), // Map<partId, itemData>
    suppliers: [],

    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.loadSuppliers();
        this.updateSummary();
    },

    initDataTable: function() {
        var self = this;
        
        var columns = [
            {
                data: null,
                title: '<input type="checkbox" id="selectAll" />',
                orderable: false,
                width: '3%',
                render: function(data, type, row) {
                    return `<input type="checkbox" class="item-checkbox" data-part-id="${row.partId}" />`;
                }
            },
            { data: 'partNumber', title: 'Mã Phụ Tùng', width: '10%' },
            { data: 'partName', title: 'Tên Phụ Tùng', width: '20%' },
            { 
                data: 'currentStock', 
                title: 'Tồn Kho', 
                width: '8%',
                render: function(data) {
                    return data || 0;
                }
            },
            { 
                data: 'minimumStock', 
                title: 'Tồn TT', 
                width: '8%',
                render: function(data) {
                    return data || 0;
                }
            },
            { 
                data: 'suggestedQuantity', 
                title: 'Số Lượng Đề Xuất', 
                width: '10%',
                className: 'text-center font-weight-bold'
            },
            { 
                data: 'priority', 
                title: 'Ưu Tiên', 
                width: '8%',
                render: function(data) {
                    var badgeClass = 'badge-secondary';
                    var text = data || 'N/A';
                    if (data === 'High') badgeClass = 'badge-danger';
                    else if (data === 'Medium') badgeClass = 'badge-warning';
                    else if (data === 'Low') badgeClass = 'badge-info';
                    return `<span class="badge ${badgeClass}">${text}</span>`;
                }
            },
            { 
                data: 'source', 
                title: 'Nguồn', 
                width: '10%',
                render: function(data) {
                    var icon = '';
                    var text = data || 'N/A';
                    if (data === 'InventoryAlert') {
                        icon = '<i class="fas fa-exclamation-triangle text-warning"></i> ';
                        text = 'Cảnh Báo';
                    } else if (data === 'ServiceOrder') {
                        icon = '<i class="fas fa-wrench text-info"></i> ';
                        text = 'Đơn Hàng';
                    }
                    return icon + text;
                }
            },
            { 
                data: 'requiredByDate', 
                title: 'Ngày Cần', 
                width: '10%',
                render: function(data) {
                    if (!data) return '<span class="text-muted">-</span>';
                    return DataTablesUtility.renderDate(data);
                }
            },
            { 
                data: 'estimatedCost', 
                title: 'Chi Phí Dự Kiến', 
                width: '12%',
                render: DataTablesUtility.renderCurrency
            },
            {
                data: null,
                title: 'Thao Tác',
                width: '8%',
                orderable: false,
                render: function(data, type, row) {
                    return `
                        <button class="btn btn-sm btn-primary view-detail" data-part-id="${row.partId}" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                    `;
                }
            }
        ];
        
        this.demandTable = DataTablesUtility.initServerSideTable('#demandAnalysisTable', '/Procurement/GetDemandAnalysis', columns, {
            order: [[5, 'desc']], // Sort by priority
            pageLength: 20
        });
    },

    bindEvents: function() {
        var self = this;

        // Select all checkbox
        $(document).on('change', '#selectAll', function() {
            var isChecked = $(this).prop('checked');
            $('.item-checkbox').prop('checked', isChecked);
            self.updateSelectedItems();
        });

        // Individual checkbox
        $(document).on('change', '.item-checkbox', function() {
            self.updateSelectedItems();
        });

        // Apply filters
        $('#btnApplyFilters').on('click', function() {
            self.applyFilters();
        });

        // Reset filters
        $('#btnResetFilters').on('click', function() {
            self.resetFilters();
        });

        // Bulk create PO
        $('#btnBulkCreatePO').on('click', function() {
            if (self.selectedItems.size === 0) {
                GarageApp.showWarning('Vui lòng chọn ít nhất một phụ tùng để tạo PO.');
                return;
            }
            self.showBulkCreatePOModal();
        });

        // Confirm create PO
        $('#btnConfirmCreatePO').on('click', function() {
            self.createBulkPO();
        });

        // Load suppliers when modal opens
        $('#bulkCreatePOModal').on('shown.bs.modal', function() {
            if (self.suppliers.length === 0) {
                self.loadSuppliers();
            }
        });
    },

    updateSelectedItems: function() {
        var self = this;
        this.selectedItems.clear();

        $('.item-checkbox:checked').each(function() {
            var partId = parseInt($(this).data('part-id'), 10);
            var row = self.demandTable.row($(this).closest('tr')).data();
            
            if (row) {
                self.selectedItems.set(partId, {
                    partId: partId,
                    partNumber: row.partNumber,
                    partName: row.partName,
                    suggestedQuantity: row.suggestedQuantity,
                    estimatedCost: row.estimatedCost,
                    unitPrice: row.estimatedCost / (row.suggestedQuantity || 1)
                });
            }
        });

        // Update button state
        $('#btnBulkCreatePO').prop('disabled', this.selectedItems.size === 0);
    },

    applyFilters: function() {
        var priority = $('#filterPriority').val();
        var source = $('#filterSource').val();

        // Update DataTable with new filters
        if (this.demandTable) {
            // Reload with new parameters
            this.demandTable.ajax.url('/Procurement/GetDemandAnalysis?priority=' + encodeURIComponent(priority || '') + '&source=' + encodeURIComponent(source || ''));
            this.demandTable.ajax.reload();
        }
    },

    resetFilters: function() {
        $('#filterPriority').val('');
        $('#filterSource').val('');
        this.applyFilters();
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
                self.populateSupplierSelect();
            }
        })
        .fail(function() {
            GarageApp.showError('Không thể tải danh sách nhà cung cấp.');
        });
    },

    populateSupplierSelect: function() {
        var select = $('#supplierSelect');
        select.empty();
        select.append('<option value="">-- Chọn nhà cung cấp --</option>');
        
        this.suppliers.forEach(function(supplier) {
            if (supplier.isActive && !supplier.isDeleted) {
                select.append(`<option value="${supplier.id}">${supplier.supplierName}</option>`);
            }
        });
    },

    showBulkCreatePOModal: function() {
        var self = this;
        
        // Populate selected items table
        var tbody = $('#selectedItemsTableBody');
        tbody.empty();
        
        var totalAmount = 0;
        this.selectedItems.forEach(function(item) {
            var row = `
                <tr>
                    <td>${item.partNumber}</td>
                    <td>${item.partName}</td>
                    <td class="text-center">${item.suggestedQuantity}</td>
                    <td class="text-right">${self.formatCurrency(item.unitPrice)}</td>
                    <td class="text-right font-weight-bold">${self.formatCurrency(item.estimatedCost)}</td>
                </tr>
            `;
            tbody.append(row);
            totalAmount += item.estimatedCost;
        });

        // Add total row
        tbody.append(`
            <tr class="table-info">
                <td colspan="4" class="text-right font-weight-bold">Tổng cộng:</td>
                <td class="text-right font-weight-bold">${self.formatCurrency(totalAmount)}</td>
            </tr>
        `);

        // Set default dates
        var today = new Date().toISOString().split('T')[0];
        $('#orderDate').val(today);
        
        // Show modal
        $('#bulkCreatePOModal').modal('show');
    },

    createBulkPO: function() {
        var self = this;
        
        // Validate
        var supplierId = parseInt($('#supplierSelect').val(), 10);
        if (!supplierId) {
            GarageApp.showWarning('Vui lòng chọn nhà cung cấp.');
            return;
        }

        // Build request
        var suggestions = [];
        this.selectedItems.forEach(function(item) {
            suggestions.push({
                partId: item.partId,
                quantity: item.suggestedQuantity,
                supplierId: supplierId,
                unitPrice: item.unitPrice,
                expectedDeliveryDate: $('#expectedDeliveryDate').val() || null
            });
        });

        var dto = {
            suggestions: suggestions,
            supplierId: supplierId,
            orderDate: $('#orderDate').val() || null,
            expectedDeliveryDate: $('#expectedDeliveryDate').val() || null,
            notes: $('#notes').val() || null
        };

        GarageApp.showLoading('Đang tạo PO...');

        $.ajax({
            url: '/Procurement/BulkCreatePO',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(dto),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        })
        .done(function(response) {
            GarageApp.hideLoading();
            
            // ✅ FIX: ApiService returns ApiResponse<T> with camelCase properties
            // Response structure: { success: true, data: PurchaseOrderDto, message: "...", errorMessage: null }
            if (response.success && response.data) {
                var orderNumber = response.data.orderNumber || 'N/A';
                GarageApp.showSuccess('Tạo PO thành công! Số PO: ' + orderNumber);
                $('#bulkCreatePOModal').modal('hide');
                self.demandTable.ajax.reload();
                self.selectedItems.clear();
                $('#selectAll').prop('checked', false);
                $('.item-checkbox').prop('checked', false);
                $('#btnBulkCreatePO').prop('disabled', true);
                
                // Redirect to PO detail page if needed
                // if (response.data.id) {
                //     window.location.href = '/PurchaseOrder/View/' + response.data.id;
                // }
            } else {
                GarageApp.showError(response.errorMessage || response.message || 'Lỗi khi tạo PO.');
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (xhr.status === 401) {
                AuthHandler.handleUnauthorized(xhr, true);
            } else {
                var errorMsg = xhr.responseJSON?.errorMessage || xhr.responseJSON?.message || 'Lỗi khi tạo PO.';
                GarageApp.showError(errorMsg);
            }
        });
    },

    updateSummary: function() {
        // This will be called after data is loaded
        // For now, we'll update it in the DataTable draw callback
        var self = this;
        
        if (this.demandTable) {
            this.demandTable.on('draw', function() {
                var data = self.demandTable.rows({ search: 'applied' }).data().toArray();
                
                var highCount = 0, mediumCount = 0, lowCount = 0, totalCost = 0;
                
                data.forEach(function(item) {
                    if (item.priority === 'High') highCount++;
                    else if (item.priority === 'Medium') mediumCount++;
                    else if (item.priority === 'Low') lowCount++;
                    totalCost += item.estimatedCost || 0;
                });
                
                $('#highPriorityCount').text(highCount);
                $('#mediumPriorityCount').text(mediumCount);
                $('#lowPriorityCount').text(lowCount);
                $('#totalEstimatedCost').text(self.formatCurrency(totalCost));
            });
        }
    },

    formatCurrency: function(value) {
        if (!value && value !== 0) return '0';
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(value);
    }
};

// Initialize when document is ready
$(document).ready(function() {
    if ($('#demandAnalysisTable').length > 0) {
        DemandAnalysis.init();
    }
});

