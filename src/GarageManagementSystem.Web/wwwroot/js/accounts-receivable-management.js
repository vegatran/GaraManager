/**
 * ✅ 4.3.2.2: Accounts Receivable Management JavaScript Module
 * Quản lý Công Nợ Phải Thu (Accounts Receivable Management)
 */

window.AccountsReceivableManagement = {
    // DataTable instance
    receivablesTable: null,

    // Summary data
    summary: {
        totalReceivable: 0,
        overdueReceivable: 0,
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
        this.loadCustomers();
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
            { data: 'type', title: 'Loại', width: '8%', 
                render: function(data) {
                    var badges = {
                        'Invoice': '<span class="badge badge-primary">Hóa Đơn</span>',
                        'ServiceOrder': '<span class="badge badge-info">Phiếu SC</span>'
                    };
                    return badges[data] || '<span class="badge badge-secondary">' + (data || 'N/A') + '</span>';
                }
            },
            { data: 'customerName', title: 'Khách Hàng', width: '15%' },
            { data: 'customerPhone', title: 'Điện Thoại', width: '10%', defaultContent: '-' },
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
                data: 'issuedDate', 
                title: 'Ngày Phát Hành', 
                width: '10%',
                render: function(data) {
                    if (!data) return '-';
                    try {
                        var date = new Date(data);
                        // ✅ SỬA: Check invalid date
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
                        // ✅ SỬA: Check invalid date
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
                    // ✅ SỬA: Safe check cho row.type và row.referenceId
                    if (!row || !row.type || !row.referenceId) return '-';
                    
                    var viewUrl = row.type === 'Invoice' 
                        ? '/PaymentManagement/InvoiceDetails/' + row.referenceId
                        : '/OrderManagement/Details/' + row.referenceId;
                    
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
            this.receivablesTable = DataTablesUtility.initServerSideTable(
                '#receivablesTable',
                '/AccountsReceivableManagement/GetAccountsReceivable',
                columns,
                {
                    order: [[8, 'desc']], // Sort by overdue days DESC
                    pageLength: 10,
                    ajax: {
                        url: '/AccountsReceivableManagement/GetAccountsReceivable',
                        type: 'GET',
                        data: function(d) {
                            // ✅ SỬA: Convert empty string to null để API handle đúng
                            var customerIdValue = $('#customerFilter').val();
                            d.customerId = customerIdValue && customerIdValue !== '' ? customerIdValue : null;
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
            this.receivablesTable = $('#receivablesTable').DataTable({
                processing: true,
                serverSide: false,
                ajax: {
                    url: '/AccountsReceivableManagement/GetAccountsReceivable',
                    type: 'GET',
                    data: function(d) {
                        // ✅ SỬA: Convert empty string to null để API handle đúng
                        var customerIdValue = $('#customerFilter').val();
                        d.customerId = customerIdValue && customerIdValue !== '' ? customerIdValue : null;
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
        $('#customerFilter, #paymentStatusFilter, #overdueDaysFilter, #fromDateFilter, #toDateFilter').on('change', function() {
            self.reloadTable();
            self.loadSummary();
        });

        // Clear filters
        $(document).on('click', '#clearFiltersBtn', function() {
            $('#customerFilter').val('').trigger('change');
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
        // Initialize Select2 for customer dropdown
        if ($('#customerFilter').length > 0) {
            if ($('#customerFilter').hasClass('select2-hidden-accessible')) {
                $('#customerFilter').select2('destroy');
            }
            $('#customerFilter').select2({
                placeholder: '-- Tất cả Khách Hàng --',
                allowClear: true,
                width: '100%'
            });
        }
    },

    // Load customers for dropdown
    loadCustomers: function() {
        var self = this;
        
        $.ajax({
            url: '/AccountsReceivableManagement/GetAvailableCustomers',
            type: 'GET',
            success: function(data) {
                var $select = $('#customerFilter');
                
                if (data && data.length > 0) {
                    data.forEach(function(customer) {
                        $select.append('<option value="' + customer.value + '">' + customer.text + '</option>');
                    });
                }
                
                // Reinitialize Select2
                if ($select.hasClass('select2-hidden-accessible')) {
                    $select.select2('destroy');
                }
                $select.select2({
                    placeholder: '-- Tất cả Khách Hàng --',
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
        
        // ✅ SỬA: Convert empty string to null để API handle đúng
        var customerIdValue = $('#customerFilter').val();
        var customerId = customerIdValue && customerIdValue !== '' ? customerIdValue : null;
        var fromDate = $('#fromDateFilter').val() || null;
        var toDate = $('#toDateFilter').val() || null;
        
        $.ajax({
            url: '/AccountsReceivableManagement/GetSummary',
            type: 'GET',
            data: {
                customerId: customerId, // Already null if empty
                fromDate: fromDate,
                toDate: toDate
            },
            success: function(response) {
                if (response && response.success && response.data) {
                    var data = response.data;
                    
                    // Update summary cards
                    $('#totalReceivable').text((data.totalReceivable || 0).toLocaleString('vi-VN'));
                    $('#overdueReceivable').text((data.overdueReceivable || 0).toLocaleString('vi-VN'));
                    $('#overdue30Days').text((data.overdue30Days || 0).toLocaleString('vi-VN'));
                    $('#overdue60Days').text((data.overdue60Days || 0).toLocaleString('vi-VN'));
                    $('#overdue90Days').text((data.overdue90Days || 0).toLocaleString('vi-VN'));
                    $('#totalCount').text(data.totalCount || 0);
                    $('#overdueCount').text(data.overdueCount || 0);
                    
                    // Update by customer breakdown
                    self.updateCustomerBreakdown(data.byCustomer || []);
                    
                    // Store in module
                    self.summary = {
                        totalReceivable: data.totalReceivable || 0,
                        overdueReceivable: data.overdueReceivable || 0,
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

    // Update customer breakdown display
    updateCustomerBreakdown: function(customers) {
        var self = this;
        var $container = $('#customerBreakdown');
        $container.empty();

        if (!customers || customers.length === 0) {
            $container.html('<div class="col-12"><p class="text-muted">Không có dữ liệu</p></div>');
            return;
        }

        customers.forEach(function(customer) {
            // ✅ SỬA: Safe division by zero check và null checks
            var totalReceivable = parseFloat(customer.totalReceivable) || 0;
            var percentage = self.summary.totalReceivable > 0 
                ? ((totalReceivable / self.summary.totalReceivable) * 100).toFixed(1)
                : 0;
            
            var card = `
                <div class="col-md-3 col-sm-6 mb-3">
                    <div class="info-box">
                        <span class="info-box-icon bg-primary elevation-1">
                            <i class="fas fa-user"></i>
                        </span>
                        <div class="info-box-content">
                            <span class="info-box-text">${customer.customerName || 'N/A'}</span>
                            <span class="info-box-number">
                                ${(totalReceivable).toLocaleString('vi-VN')} VNĐ
                                <small>(${customer.invoiceCount || 0} phiếu)</small>
                            </span>
                            <div class="progress">
                                <div class="progress-bar bg-primary" style="width: ${percentage}%"></div>
                            </div>
                            <span class="progress-description">
                                ${percentage}% tổng nợ
                                ${customer.overdueCount > 0 ? ' | <span class="text-danger">' + customer.overdueCount + ' quá hạn</span>' : ''}
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
        if (this.receivablesTable) {
            if (typeof this.receivablesTable.ajax !== 'undefined') {
                this.receivablesTable.ajax.reload();
            } else if (typeof this.receivablesTable.reload !== 'undefined') {
                this.receivablesTable.reload();
            } else {
                this.receivablesTable.draw();
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
    if ($('#receivablesTable').length > 0) {
        AccountsReceivableManagement.init();
    }
});

