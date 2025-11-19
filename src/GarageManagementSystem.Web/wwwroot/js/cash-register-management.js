/**
 * ✅ 4.3.1.6: Cash Register Management JavaScript
 * Quản lý Sổ Quỹ Tiền Mặt/Ngân Hàng
 */

$(document).ready(function () {
    // Initialize DataTables
    let cashTable = null;
    let bankTable = null;

    // Set default date range (current month)
    const today = new Date();
    const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
    $('#filterFromDate').val(formatDateForInput(firstDayOfMonth));
    $('#filterToDate').val(formatDateForInput(today));

    // Initialize tables
    initCashTable();
    initBankTable();

    // Bind events
    bindEvents();

    // Load initial data
    loadCashRegister();
    
    // Tab change event
    $('#registerTabs a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        const targetTab = $(e.target).attr('href');
        if (targetTab === '#cash-register' && !cashTable) {
            loadCashRegister();
        } else if (targetTab === '#bank-register' && !bankTable) {
            loadBankRegister();
        }
    });

    /**
     * Initialize Cash Transactions DataTable
     */
    function initCashTable() {
        cashTable = $('#cashTransactionsTable').DataTable({
            processing: true,
            serverSide: false,
            searching: false,
            paging: false,
            ordering: true,
            info: false,
            autoWidth: false,
            responsive: true,
            order: [[1, 'desc']], // Order by date descending
            columns: [
                { data: null, orderable: false, defaultContent: '' },
                { data: 'transactionDate', render: formatDate },
                { data: 'transactionNumber' },
                { 
                    data: 'transactionType',
                    render: function (data) {
                        if (data === 'Income') {
                            return '<span class="badge badge-success">Thu</span>';
                        } else if (data === 'Expense') {
                            return '<span class="badge badge-danger">Chi</span>';
                        }
                        return '<span class="badge badge-secondary">' + data + '</span>';
                    }
                },
                { data: 'category' },
                { data: 'description', defaultContent: '' },
                {
                    data: null,
                    render: function (data, type, row) {
                        if (row.transactionType === 'Income') {
                            return formatCurrency(row.amount);
                        }
                        return '-';
                    }
                },
                {
                    data: null,
                    render: function (data, type, row) {
                        if (row.transactionType === 'Expense') {
                            return formatCurrency(row.amount);
                        }
                        return '-';
                    }
                },
                { data: 'employeeName', defaultContent: '-' },
                {
                    data: 'status',
                    render: function (data) {
                        const statusClass = {
                            'Completed': 'badge-success',
                            'Pending': 'badge-warning',
                            'Cancelled': 'badge-danger'
                        }[data] || 'badge-secondary';
                        return '<span class="badge ' + statusClass + '">' + data + '</span>';
                    }
                }
            ],
            language: {
                emptyTable: "Không có dữ liệu"
            }
        });
    }

    /**
     * Initialize Bank Transactions DataTable
     */
    function initBankTable() {
        bankTable = $('#bankTransactionsTable').DataTable({
            processing: true,
            serverSide: false,
            searching: false,
            paging: false,
            ordering: true,
            info: false,
            autoWidth: false,
            responsive: true,
            order: [[1, 'desc']], // Order by date descending
            columns: [
                { data: null, orderable: false, defaultContent: '' },
                { data: 'transactionDate', render: formatDate },
                { data: 'transactionNumber' },
                { 
                    data: 'transactionType',
                    render: function (data) {
                        if (data === 'Income') {
                            return '<span class="badge badge-success">Thu</span>';
                        } else if (data === 'Expense') {
                            return '<span class="badge badge-danger">Chi</span>';
                        }
                        return '<span class="badge badge-secondary">' + data + '</span>';
                    }
                },
                { data: 'category' },
                { data: 'description', defaultContent: '' },
                {
                    data: null,
                    render: function (data, type, row) {
                        if (row.transactionType === 'Income') {
                            return formatCurrency(row.amount);
                        }
                        return '-';
                    }
                },
                {
                    data: null,
                    render: function (data, type, row) {
                        if (row.transactionType === 'Expense') {
                            return formatCurrency(row.amount);
                        }
                        return '-';
                    }
                },
                { data: 'employeeName', defaultContent: '-' },
                {
                    data: 'status',
                    render: function (data) {
                        const statusClass = {
                            'Completed': 'badge-success',
                            'Pending': 'badge-warning',
                            'Cancelled': 'badge-danger'
                        }[data] || 'badge-secondary';
                        return '<span class="badge ' + statusClass + '">' + data + '</span>';
                    }
                }
            ],
            language: {
                emptyTable: "Không có dữ liệu"
            }
        });
    }

    /**
     * Bind events
     */
    function bindEvents() {
        $('#btnFilter').on('click', function () {
            const activeTab = $('#registerTabs .nav-link.active').attr('href');
            if (activeTab === '#cash-register') {
                loadCashRegister();
            } else if (activeTab === '#bank-register') {
                loadBankRegister();
            }
        });

        $('#btnResetFilter').on('click', function () {
            const today = new Date();
            const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
            $('#filterFromDate').val(formatDateForInput(firstDayOfMonth));
            $('#filterToDate').val(formatDateForInput(today));
            $('#btnFilter').click();
        });

        $('#btnExportExcel').on('click', function () {
            const activeTab = $('#registerTabs .nav-link.active').attr('href');
            const fromDate = $('#filterFromDate').val();
            const toDate = $('#filterToDate').val();
            const type = activeTab === '#cash-register' ? 'cash' : 'bank';
            
            // TODO: Implement export Excel
            Swal.fire({
                icon: 'info',
                title: 'Xuất Excel',
                text: 'Tính năng xuất Excel đang được phát triển'
            });
        });
    }

    /**
     * Load Cash Register data
     */
    function loadCashRegister() {
        const fromDate = $('#filterFromDate').val();
        const toDate = $('#filterToDate').val();

        $.ajax({
            url: '/FinancialTransactionManagement/GetCashRegister',
            type: 'GET',
            data: {
                fromDate: fromDate,
                toDate: toDate,
                pageNumber: 1,
                pageSize: 1000 // Load all transactions for now
            },
            success: function (response) {
                if (response.success && response.data) {
                    updateCashSummary(response.data);
                    updateCashTable(response.data.transactions || []);
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: response.error || 'Không thể tải dữ liệu sổ quỹ tiền mặt'
                    });
                }
            },
            error: function (xhr, status, error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Lỗi khi tải dữ liệu: ' + error
                });
            }
        });
    }

    /**
     * Load Bank Register data
     */
    function loadBankRegister() {
        const fromDate = $('#filterFromDate').val();
        const toDate = $('#filterToDate').val();

        $.ajax({
            url: '/FinancialTransactionManagement/GetBankRegister',
            type: 'GET',
            data: {
                fromDate: fromDate,
                toDate: toDate,
                pageNumber: 1,
                pageSize: 1000 // Load all transactions for now
            },
            success: function (response) {
                if (response.success && response.data) {
                    updateBankSummary(response.data);
                    updateBankTable(response.data.transactions || []);
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: response.error || 'Không thể tải dữ liệu sổ quỹ ngân hàng'
                    });
                }
            },
            error: function (xhr, status, error) {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Lỗi khi tải dữ liệu: ' + error
                });
            }
        });
    }

    /**
     * Update Cash Summary Cards
     */
    function updateCashSummary(data) {
        $('#cashOpeningBalance').text(formatCurrency(data.openingBalance || 0));
        $('#cashTotalIncome').text(formatCurrency(data.totalIncome || 0));
        $('#cashTotalExpense').text(formatCurrency(data.totalExpense || 0));
        $('#cashClosingBalance').text(formatCurrency(data.closingBalance || 0));
        $('#cashTransactionCount').text(data.transactionCount || 0);
    }

    /**
     * Update Bank Summary Cards
     */
    function updateBankSummary(data) {
        $('#bankOpeningBalance').text(formatCurrency(data.openingBalance || 0));
        $('#bankTotalIncome').text(formatCurrency(data.totalIncome || 0));
        $('#bankTotalExpense').text(formatCurrency(data.totalExpense || 0));
        $('#bankClosingBalance').text(formatCurrency(data.closingBalance || 0));
        $('#bankTransactionCount').text(data.transactionCount || 0);
    }

    /**
     * Update Cash Transactions Table
     */
    function updateCashTable(transactions) {
        if (cashTable) {
            cashTable.clear();
            transactions.forEach(function (transaction, index) {
                cashTable.row.add({
                    transactionDate: transaction.transactionDate,
                    transactionNumber: transaction.transactionNumber,
                    transactionType: transaction.transactionType,
                    category: transaction.category,
                    description: transaction.description || '',
                    amount: transaction.amount,
                    employeeName: transaction.employeeName || '',
                    status: transaction.status
                });
            });
            cashTable.draw(false);
            
            // Update row numbers
            cashTable.on('order.dt search.dt', function () {
                cashTable.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                    cell.innerHTML = i + 1;
                });
            }).draw();
        }
    }

    /**
     * Update Bank Transactions Table
     */
    function updateBankTable(transactions) {
        if (bankTable) {
            bankTable.clear();
            transactions.forEach(function (transaction, index) {
                bankTable.row.add({
                    transactionDate: transaction.transactionDate,
                    transactionNumber: transaction.transactionNumber,
                    transactionType: transaction.transactionType,
                    category: transaction.category,
                    description: transaction.description || '',
                    amount: transaction.amount,
                    employeeName: transaction.employeeName || '',
                    status: transaction.status
                });
            });
            bankTable.draw(false);
            
            // Update row numbers
            bankTable.on('order.dt search.dt', function () {
                bankTable.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
                    cell.innerHTML = i + 1;
                });
            }).draw();
        }
    }

    /**
     * Format currency
     */
    function formatCurrency(value) {
        if (!value) return '0';
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(value);
    }

    /**
     * Format date
     */
    function formatDate(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
    }

    /**
     * Format date for input type="date"
     */
    function formatDateForInput(date) {
        if (!date) return '';
        const d = new Date(date);
        const year = d.getFullYear();
        const month = String(d.getMonth() + 1).padStart(2, '0');
        const day = String(d.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }
});

