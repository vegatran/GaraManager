/**
 * ✅ 4.3.2.4: Accounts Payable Management JavaScript Module
 * Quản lý Công Nợ Phải Trả (Accounts Payable Management)
 */

window.AccountsPayableManagement = {
    // DataTable instance
    payablesTable: null,

    // Summary data
    summary: {
        totalPayable: 0,
        overduePayable: 0,
        overdue30Days: 0,
        overdue60Days: 0,
        overdue90Days: 0,
        totalCount: 0,
        overdueCount: 0
    },

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.loadSuppliers();
        this.loadSummary();
        this.initSelect2();
        this.setDefaultDateRange();
    },

    // Set default date range (last 3 months)
    setDefaultDateRange: function() {
        const today = new Date();
        const threeMonthsAgo = new Date(today.getFullYear(), today.getMonth() - 3, 1);
        
        $('#fromDateFilter').val(this.formatDateForInput(threeMonthsAgo));
        $('#toDateFilter').val(this.formatDateForInput(today));
    },

    // Format date for input type="date"
    formatDateForInput: function(date) {
        if (!date) return '';
        const d = new Date(date);
        const year = d.getFullYear();
        const month = String(d.getMonth() + 1).padStart(2, '0');
        const day = String(d.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        var columns = [
            { data: 'referenceNumber', title: 'Số Phiếu', width: '12%' },
            { data: 'supplierName', title: 'Nhà Cung Cấp', width: '15%' },
            { data: 'supplierPhone', title: 'Điện Thoại', width: '10%', defaultContent: '-' },
            { data: 'supplierEmail', title: 'Email', width: '12%', defaultContent: '-' },
            { 
                data: 'totalAmount', 
                title: 'Tổng Tiền', 
                width: '12%',
                className: 'text-right',
                render: function(data, type, row) {
                    var amount = parseFloat(data) || 0;
                    return amount.toLocaleString('vi-VN') + ' VNĐ';
                }
            },
            { 
                data: 'paidAmount', 
                title: 'Đã Trả', 
                width: '12%',
                className: 'text-right',
                render: function(data, type, row) {
                    var amount = parseFloat(data) || 0;
                    return amount.toLocaleString('vi-VN') + ' VNĐ';
                }
            },
            { 
                data: 'remainingAmount', 
                title: 'Còn Nợ', 
                width: '12%',
                className: 'text-right font-weight-bold',
                render: function(data, type, row) {
                    var amount = parseFloat(data) || 0;
                    var color = row.overdueDays > 0 ? 'text-danger' : 'text-primary';
                    return '<span class="' + color + '">' + amount.toLocaleString('vi-VN') + ' VNĐ</span>';
                }
            },
            { 
                data: 'paymentStatus', 
                title: 'Trạng Thái', 
                width: '10%',
                render: function(data) {
                    var badges = {
                        'Unpaid': '<span class="badge badge-danger">Chưa Trả</span>',
                        'Partial': '<span class="badge badge-warning">Trả Một Phần</span>',
                        'Paid': '<span class="badge badge-success">Đã Trả</span>'
                    };
                    return badges[data] || '<span class="badge badge-secondary">' + (data || 'N/A') + '</span>';
                }
            },
            { 
                data: 'overdueDays', 
                title: 'Quá Hạn', 
                width: '10%',
                className: 'text-center',
                render: function(data, type, row) {
                    var days = parseInt(data) || 0;
                    if (days === 0) {
                        return '<span class="badge badge-success">Đúng Hạn</span>';
                    } else if (days <= 30) {
                        return '<span class="badge badge-warning">' + days + ' ngày</span>';
                    } else if (days <= 60) {
                        return '<span class="badge badge-danger">' + days + ' ngày</span>';
                    } else {
                        return '<span class="badge badge-danger font-weight-bold">' + days + ' ngày</span>';
                    }
                }
            },
            { 
                data: 'orderDate', 
                title: 'Ngày Đặt Hàng', 
                width: '10%',
                render: function(data) {
                    if (!data) return '-';
                    try {
                        var date = new Date(data);
                        if (isNaN(date.getTime())) return '-';
                        return date.toLocaleDateString('vi-VN');
                    } catch (e) {
                        return '-';
                    }
                }
            },
            { 
                data: 'receivedDate', 
                title: 'Ngày Nhận Hàng', 
                width: '10%',
                render: function(data) {
                    if (!data) return '-';
                    try {
                        var date = new Date(data);
                        if (isNaN(date.getTime())) return '-';
                        return date.toLocaleDateString('vi-VN');
                    } catch (e) {
                        return '-';
                    }
                }
            },
            { 
                data: 'dueDate', 
                title: 'Hạn Thanh Toán', 
                width: '10%',
                render: function(data) {
                    if (!data) return '-';
                    try {
                        var date = new Date(data);
                        if (isNaN(date.getTime())) return '-';
                        return date.toLocaleDateString('vi-VN');
                    } catch (e) {
                        return '-';
                    }
                }
            },
            {
                data: null,
                title: 'Thao Tác',
                width: '10%',
                orderable: false,
                searchable: false,
                className: 'text-center',
                render: function(data, type, row) {
                    if (!row || !row.referenceId) return '-';
                    
                    var viewUrl = '/PurchaseOrder/Details/' + row.referenceId;
                    
                    return `
                        <a href="${viewUrl}" class="btn btn-info btn-sm" target="_blank" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </a>
                    `;
                }
            }
        ];
        
        // Check if DataTablesUtility exists
        if (typeof DataTablesUtility !== 'undefined' && DataTablesUtility.initServerSideTable) {
            this.payablesTable = DataTablesUtility.initServerSideTable(
                '#payablesTable',
                '/AccountsPayableManagement/GetAccountsPayable',
                columns,
                {
                    order: [[8, 'desc']], // Sort by overdue days DESC
                    pageLength: 10,
                    ajax: {
                        url: '/AccountsPayableManagement/GetAccountsPayable',
                        type: 'GET',
                        data: function(d) {
                            var supplierIdValue = $('#supplierFilter').val();
                            d.supplierId = supplierIdValue && supplierIdValue !== '' ? supplierIdValue : null;
                            d.fromDate = $('#fromDateFilter').val() || null;
                            d.toDate = $('#toDateFilter').val() || null;
                            var overdueDaysValue = $('#overdueDaysFilter').val();
                            d.overdueDays = overdueDaysValue && overdueDaysValue !== '' ? parseInt(overdueDaysValue) : null;
                            d.paymentStatus = $('#paymentStatusFilter').val() || null;
                            return d;
                        }
                    }
                }
            );
        } else {
            // Fallback: Use regular DataTable
            this.payablesTable = $('#payablesTable').DataTable({
                processing: true,
                serverSide: false,
                ajax: {
                    url: '/AccountsPayableManagement/GetAccountsPayable',
                    type: 'GET',
                    data: function(d) {
                        var supplierIdValue = $('#supplierFilter').val();
                        d.supplierId = supplierIdValue && supplierIdValue !== '' ? supplierIdValue : null;
                        d.fromDate = $('#fromDateFilter').val() || null;
                        d.toDate = $('#toDateFilter').val() || null;
                        var overdueDaysValue = $('#overdueDaysFilter').val();
                        d.overdueDays = overdueDaysValue && overdueDaysValue !== '' ? parseInt(overdueDaysValue) : null;
                        d.paymentStatus = $('#paymentStatusFilter').val() || null;
                        return d;
                    },
                    dataSrc: function(json) {
                        if (json.success && json.data) {
                            return json.data;
                        }
                        return [];
                    }
                },
                columns: columns,
                order: [[8, 'desc']],
                pageLength: 10,
                language: {
                    emptyTable: "Không có dữ liệu",
                    processing: "Đang xử lý..."
                }
            });
        }
    },

    // Bind events
    bindEvents: function() {
        var self = this;

        // Filter changes
        $('#supplierFilter, #paymentStatusFilter, #overdueDaysFilter, #fromDateFilter, #toDateFilter').on('change', function() {
            self.reloadTable();
            self.loadSummary();
        });

        // Clear filters
        $(document).on('click', '#clearFiltersBtn', function() {
            $('#supplierFilter').val('').trigger('change');
            $('#paymentStatusFilter').val('');
            $('#overdueDaysFilter').val('');
            self.setDefaultDateRange();
            self.reloadTable();
            self.loadSummary();
        });

        // Export buttons
        $(document).on('click', '#exportExcelBtn', function() {
            self.exportExcel();
        });

        $(document).on('click', '#exportPdfBtn', function() {
            self.exportPdf();
        });
    },

    // Initialize Select2 dropdowns
    initSelect2: function() {
        // Initialize Select2 for supplier dropdown
        if ($('#supplierFilter').length > 0) {
            if ($('#supplierFilter').hasClass('select2-hidden-accessible')) {
                $('#supplierFilter').select2('destroy');
            }
            $('#supplierFilter').select2({
                placeholder: '-- Tất cả Nhà Cung Cấp --',
                allowClear: true,
                width: '100%'
            });
        }
    },

    // Load suppliers for dropdown
    loadSuppliers: function() {
        var self = this;
        
        $.ajax({
            url: '/AccountsPayableManagement/GetAvailableSuppliers',
            type: 'GET',
            success: function(data) {
                var $select = $('#supplierFilter');
                
                if (data && data.length > 0) {
                    data.forEach(function(supplier) {
                        $select.append('<option value="' + supplier.value + '">' + supplier.text + '</option>');
                    });
                }
                
                // Reinitialize Select2
                if ($select.hasClass('select2-hidden-accessible')) {
                    $select.select2('destroy');
                }
                $select.select2({
                    placeholder: '-- Tất cả Nhà Cung Cấp --',
                    allowClear: true,
                    width: '100%'
                });
            },
            error: function(xhr, status, error) {
                if (typeof AuthHandler !== 'undefined' && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                }
            }
        });
    },

    // Load summary data
    loadSummary: function() {
        var self = this;
        
        var supplierIdValue = $('#supplierFilter').val();
        var supplierId = supplierIdValue && supplierIdValue !== '' ? supplierIdValue : null;
        var fromDate = $('#fromDateFilter').val() || null;
        var toDate = $('#toDateFilter').val() || null;
        
        $.ajax({
            url: '/AccountsPayableManagement/GetSummary',
            type: 'GET',
            data: {
                supplierId: supplierId,
                fromDate: fromDate,
                toDate: toDate
            },
            success: function(response) {
                if (response && response.success && response.data) {
                    var data = response.data;
                    
                    // Update summary cards
                    $('#totalPayable').text((data.totalPayable || 0).toLocaleString('vi-VN'));
                    $('#overduePayable').text((data.overduePayable || 0).toLocaleString('vi-VN'));
                    $('#overdue30Days').text((data.overdue30Days || 0).toLocaleString('vi-VN'));
                    $('#overdue60Days').text((data.overdue60Days || 0).toLocaleString('vi-VN'));
                    $('#overdue90Days').text((data.overdue90Days || 0).toLocaleString('vi-VN'));
                    $('#totalCount').text(data.totalCount || 0);
                    $('#overdueCount').text(data.overdueCount || 0);
                    
                    // Update by supplier breakdown
                    self.updateSupplierBreakdown(data.bySupplier || []);
                    
                    // Store in module
                    self.summary = {
                        totalPayable: data.totalPayable || 0,
                        overduePayable: data.overduePayable || 0,
                        overdue30Days: data.overdue30Days || 0,
                        overdue60Days: data.overdue60Days || 0,
                        overdue90Days: data.overdue90Days || 0,
                        totalCount: data.totalCount || 0,
                        overdueCount: data.overdueCount || 0
                    };
                }
            },
            error: function(xhr, status, error) {
                if (typeof AuthHandler !== 'undefined' && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                }
            }
        });
    },

    // Update supplier breakdown display
    updateSupplierBreakdown: function(suppliers) {
        var self = this;
        var $container = $('#supplierBreakdown');
        $container.empty();

        if (!suppliers || suppliers.length === 0) {
            $container.html('<div class="col-12"><p class="text-muted">Không có dữ liệu</p></div>');
            return;
        }

        suppliers.forEach(function(supplier) {
            var totalPayable = parseFloat(supplier.totalPayable) || 0;
            var percentage = self.summary.totalPayable > 0 
                ? ((totalPayable / self.summary.totalPayable) * 100).toFixed(1)
                : 0;
            
            var card = `
                <div class="col-md-3 col-sm-6 mb-3">
                    <div class="info-box">
                        <span class="info-box-icon bg-primary elevation-1">
                            <i class="fas fa-truck"></i>
                        </span>
                        <div class="info-box-content">
                            <span class="info-box-text">${supplier.supplierName || 'N/A'}</span>
                            <span class="info-box-number">
                                ${(totalPayable).toLocaleString('vi-VN')} VNĐ
                                <small>(${supplier.purchaseOrderCount || 0} phiếu)</small>
                            </span>
                            <div class="progress">
                                <div class="progress-bar bg-primary" style="width: ${percentage}%"></div>
                            </div>
                            <span class="progress-description">
                                ${percentage}% tổng nợ
                                ${supplier.overdueCount > 0 ? ' | <span class="text-danger">' + supplier.overdueCount + ' quá hạn</span>' : ''}
                            </span>
                        </div>
                    </div>
                </div>
            `;
            $container.append(card);
        });
    },

    // Reload table with filters
    reloadTable: function() {
        if (this.payablesTable) {
            if (typeof this.payablesTable.ajax !== 'undefined') {
                this.payablesTable.ajax.reload();
            } else if (typeof this.payablesTable.reload !== 'undefined') {
                this.payablesTable.reload();
            } else {
                this.payablesTable.draw();
            }
        }
    },

    // Export to Excel
    exportExcel: function() {
        Swal.fire({
            icon: 'info',
            title: 'Xuất Excel',
            text: 'Tính năng đang được phát triển',
            confirmButtonText: 'Đóng'
        });
    },

    // Export to PDF
    exportPdf: function() {
        Swal.fire({
            icon: 'info',
            title: 'Xuất PDF',
            text: 'Tính năng đang được phát triển',
            confirmButtonText: 'Đóng'
        });
    }
};

// Initialize when DOM is ready
$(document).ready(function() {
    if ($('#payablesTable').length > 0) {
        AccountsPayableManagement.init();
    }
});

