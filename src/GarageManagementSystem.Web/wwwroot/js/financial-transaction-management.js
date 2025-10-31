/**
 * Financial Transaction Management Module
 * 
 * Handles all financial transaction (Phiếu Thu/Chi) operations
 * CRUD operations for financial transactions
 */

window.FinancialTransactionManagement = {
    // DataTable instance
    financialTransactionTable: null,

    // Summary data
    summary: {
        totalIncome: 0,
        totalExpense: 0,
        pendingCount: 0,
        netAmount: 0
    },

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.loadCategories();
        this.loadSummary();
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        // Sử dụng DataTablesUtility với style chung
        var columns = [
            { data: 'transactionNumber', title: 'Mã Phiếu', width: '12%' },
            { 
                data: 'transactionType', 
                title: 'Loại', 
                width: '10%',
                render: function(data, type, row) {
                    if (data === 'Income') {
                        return '<span class="badge badge-success"><i class="fas fa-arrow-up"></i> Thu</span>';
                    } else if (data === 'Expense') {
                        return '<span class="badge badge-danger"><i class="fas fa-arrow-down"></i> Chi</span>';
                    }
                    return '<span class="badge badge-secondary">' + data + '</span>';
                }
            },
            { data: 'category', title: 'Danh Mục', width: '15%' },
            { 
                data: 'amount', 
                title: 'Số Tiền', 
                width: '12%',
                className: 'text-right',
                render: function(data, type, row) {
                    var amount = parseFloat(data) || 0;
                    return amount.toLocaleString('vi-VN') + ' VNĐ';
                }
            },
            { 
                data: 'transactionDate', 
                title: 'Ngày GD', 
                width: '10%',
                render: DataTablesUtility.renderDate
            },
            { data: 'employeeName', title: 'Người Tạo', width: '10%' },
            { 
                data: 'status', 
                title: 'Trạng Thái', 
                width: '10%',
                render: function(data, type, row) {
                    var badges = {
                        'Pending': '<span class="badge badge-warning">Chờ Xử Lý</span>',
                        'Approved': '<span class="badge badge-success">Đã Duyệt</span>',
                        'Completed': '<span class="badge badge-info">Hoàn Thành</span>',
                        'Cancelled': '<span class="badge badge-danger">Đã Hủy</span>'
                    };
                    return badges[data] || '<span class="badge badge-secondary">' + (data || 'N/A') + '</span>';
                }
            },
            { data: 'notes', title: 'Ghi Chú', width: '15%' },
            {
                data: null,
                title: 'Thao Tác',
                width: '10%',
                orderable: false,
                searchable: false,
                className: 'text-center',
                render: function(data, type, row) {
                    return `
                        <button class="btn btn-info btn-sm view-transaction" data-id="${row.id}" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                    `;
                }
            }
        ];
        
        this.financialTransactionTable = DataTablesUtility.initServerSideTable(
            '#financialTransactionsTable',
            '/FinancialTransactionManagement/GetFinancialTransactions',
            columns,
            {
                order: [[4, 'desc']], // Sort by date DESC (index 4 = transactionDate)
                pageLength: 10,
                ajax: {
                    url: '/FinancialTransactionManagement/GetFinancialTransactions',
                    type: 'GET',
                    data: function(d) {
                        // Add filter parameters
                        d.transactionType = $('#transactionTypeFilter').val();
                        d.category = $('#categoryFilter').val();
                        d.status = $('#statusFilter').val();
                        return d;
                    }
                }
            }
        );
    },

    // Bind events
    bindEvents: function() {
        var self = this;

        // Unbind existing events to prevent duplicates
        $(document).off('click', '#filterIncomeBtn');
        $(document).off('click', '#filterExpenseBtn');
        $(document).off('click', '#clearFiltersBtn');
        $(document).off('click', '.view-transaction');
        $('#transactionTypeFilter, #categoryFilter, #statusFilter').off('change');

        // Filter buttons
        $(document).on('click', '#filterIncomeBtn', function() {
            $('#transactionTypeFilter').val('Income');
            self.reloadTable();
        });

        $(document).on('click', '#filterExpenseBtn', function() {
            $('#transactionTypeFilter').val('Expense');
            self.reloadTable();
        });

        // Apply filters
        $('#transactionTypeFilter, #categoryFilter, #statusFilter').on('change', function() {
            self.reloadTable();
        });

        // Clear filters
        $(document).on('click', '#clearFiltersBtn', function() {
            $('#transactionTypeFilter').val('');
            $('#categoryFilter').val('');
            $('#statusFilter').val('');
            self.reloadTable();
        });

        // View transaction
        $(document).on('click', '.view-transaction', function() {
            var id = $(this).data('id');
            self.viewTransaction(id);
        });
    },

    // Load categories
    loadCategories: function() {
        var self = this;
        
        $.ajax({
            url: '/FinancialTransactionManagement/GetCategories',
            type: 'GET',
            success: function(response) {
                if (response && response.length > 0) {
                    response.forEach(function(category) {
                        $('#categoryFilter').append(
                            '<option value="' + category + '">' + category + '</option>'
                        );
                    });
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                }
            }
        });
    },

    // Load summary data
    loadSummary: function() {
        var self = this;
        
        $.ajax({
            url: '/FinancialTransactionManagement/GetSummary',
            type: 'GET',
            success: function(response) {
                if (response && response.success && response.data) {
                    var data = response.data;
                    
                    // Update summary cards
                    $('#totalIncome').text((data.totalIncome || 0).toLocaleString('vi-VN'));
                    $('#totalExpense').text((data.totalExpense || 0).toLocaleString('vi-VN'));
                    $('#pendingCount').text(data.pendingCount || 0);
                    
                    // Calculate net amount
                    var netAmount = (data.totalIncome || 0) - (data.totalExpense || 0);
                    $('#netAmount').text(netAmount.toLocaleString('vi-VN'));
                    
                    // Store in module
                    self.summary = {
                        totalIncome: data.totalIncome || 0,
                        totalExpense: data.totalExpense || 0,
                        pendingCount: data.pendingCount || 0,
                        netAmount: netAmount
                    };
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                }
            }
        });
    },

    // Reload table with filters
    reloadTable: function() {
        if (this.financialTransactionTable) {
            this.financialTransactionTable.ajax.reload();
        }
    },

    // View transaction details
    viewTransaction: function(id) {
        var self = this;
        
        $.ajax({
            url: '/FinancialTransactionManagement/GetFinancialTransactionById/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success && response.data) {
                        self.showTransactionDetails(response.data);
                    } else {
                        GarageApp.showError('Không tìm thấy phiếu tài chính');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin phiếu tài chính');
                }
            }
        });
    },

    // Show transaction details
    showTransactionDetails: function(transaction) {
        var html = `
            <div class="card">
                <div class="card-header">
                    <h5 class="card-title">Chi Tiết Phiếu Tài Chính</h5>
                </div>
                <div class="card-body">
                    <table class="table table-bordered">
                        <tr>
                            <th width="30%">Mã Phiếu:</th>
                            <td>${transaction.transactionNumber || ''}</td>
                        </tr>
                        <tr>
                            <th>Loại:</th>
                            <td>${transaction.transactionType === 'Income' ? '<span class="badge badge-success">Thu</span>' : '<span class="badge badge-danger">Chi</span>'}</td>
                        </tr>
                        <tr>
                            <th>Danh Mục:</th>
                            <td>${transaction.category || ''}</td>
                        </tr>
                        <tr>
                            <th>Danh Mục Con:</th>
                            <td>${transaction.subCategory || ''}</td>
                        </tr>
                        <tr>
                            <th>Số Tiền:</th>
                            <td><strong>${parseFloat(transaction.amount || 0).toLocaleString('vi-VN')} VNĐ</strong></td>
                        </tr>
                        <tr>
                            <th>Ngày GD:</th>
                            <td>${new Date(transaction.transactionDate).toLocaleDateString('vi-VN')}</td>
                        </tr>
                        <tr>
                            <th>Người Tạo:</th>
                            <td>${transaction.employeeName || ''}</td>
                        </tr>
                        <tr>
                            <th>Trạng Thái:</th>
                            <td>${transaction.status || ''}</td>
                        </tr>
                        <tr>
                            <th>Ghi Chú:</th>
                            <td>${transaction.notes || ''}</td>
                        </tr>
                    </table>
                </div>
            </div>
        `;
        
        Swal.fire({
            title: 'Chi Tiết Phiếu Tài Chính',
            html: html,
            width: 800,
            showCloseButton: true,
            showConfirmButton: false
        });
    }
};

// Initialize on document ready
$(document).ready(function() {
    if ($('#financialTransactionsTable').length > 0) {
        FinancialTransactionManagement.init();
    }
});
