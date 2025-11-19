/**
 * ✅ 4.3.1.7-4.3.1.8: Expense Management JavaScript Module
 * Quản lý Chi Phí (Expense Management)
 */

window.ExpenseManagement = {
    // DataTable instance
    expensesTable: null,

    // Summary data
    summary: {
        totalExpense: 0,
        pendingExpense: 0,
        approvedExpense: 0,
        expenseCount: 0,
        categoryBreakdown: []
    },

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.loadCategories();
        this.loadSummary();
        this.initSelect2();
        this.setDefaultDateRange();
    },

    // Set default date range (current month)
    setDefaultDateRange: function() {
        const today = new Date();
        const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
        const lastDayOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0);
        
        $('#fromDateFilter').val(this.formatDateForInput(firstDayOfMonth));
        $('#toDateFilter').val(this.formatDateForInput(lastDayOfMonth));
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
            { data: 'transactionNumber', title: 'Mã Phiếu', width: '12%' },
            { data: 'category', title: 'Danh Mục', width: '15%' },
            { data: 'subCategory', title: 'Danh Mục Phụ', width: '12%', defaultContent: '-' },
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
                render: function(data) {
                    if (!data) return '';
                    var date = new Date(data);
                    return date.toLocaleDateString('vi-VN');
                }
            },
            { data: 'paymentMethod', title: 'Phương Thức', width: '10%', defaultContent: '-' },
            { data: 'employeeName', title: 'Nhân Viên', width: '10%', defaultContent: '-' },
            { 
                data: 'status', 
                title: 'Trạng Thái', 
                width: '10%',
                render: function(data) {
                    var badges = {
                        'Pending': '<span class="badge badge-warning">Chờ Xử Lý</span>',
                        'Approved': '<span class="badge badge-success">Đã Duyệt</span>',
                        'Completed': '<span class="badge badge-info">Hoàn Thành</span>',
                        'Cancelled': '<span class="badge badge-danger">Đã Hủy</span>'
                    };
                    return badges[data] || '<span class="badge badge-secondary">' + (data || 'N/A') + '</span>';
                }
            },
            {
                data: 'isApproved',
                title: 'Duyệt',
                width: '8%',
                className: 'text-center',
                render: function(data) {
                    if (data === true) {
                        return '<span class="badge badge-success"><i class="fas fa-check"></i> Đã Duyệt</span>';
                    }
                    return '<span class="badge badge-warning"><i class="fas fa-clock"></i> Chưa Duyệt</span>';
                }
            },
            { data: 'notes', title: 'Ghi Chú', width: '15%', defaultContent: '-' },
            {
                data: null,
                title: 'Thao Tác',
                width: '12%',
                orderable: false,
                searchable: false,
                className: 'text-center',
                render: function(data, type, row) {
                    return `
                        <button class="btn btn-info btn-sm view-expense mr-1" data-id="${row.id}" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-warning btn-sm edit-expense mr-1" data-id="${row.id}" title="Chỉnh sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn btn-danger btn-sm delete-expense" data-id="${row.id}" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    `;
                }
            }
        ];
        
        // Check if DataTablesUtility exists
        if (typeof DataTablesUtility !== 'undefined' && DataTablesUtility.initServerSideTable) {
            this.expensesTable = DataTablesUtility.initServerSideTable(
                '#expensesTable',
                '/ExpenseManagement/GetExpenses',
                columns,
                {
                    order: [[4, 'desc']], // Sort by date DESC
                    pageLength: 10,
                    ajax: {
                        url: '/ExpenseManagement/GetExpenses',
                        type: 'GET',
                        data: function(d) {
                            // Add filter parameters
                            d.category = $('#categoryFilter').val();
                            d.status = $('#statusFilter').val();
                            d.isApproved = $('#approvedFilter').val() === '' ? null : $('#approvedFilter').val() === 'true';
                            d.fromDate = $('#fromDateFilter').val();
                            d.toDate = $('#toDateFilter').val();
                            return d;
                        }
                    }
                }
            );
        } else {
            // Fallback: Use regular DataTable
            this.expensesTable = $('#expensesTable').DataTable({
                processing: true,
                serverSide: false,
                ajax: {
                    url: '/ExpenseManagement/GetExpenses',
                    type: 'GET',
                    data: function(d) {
                        d.category = $('#categoryFilter').val();
                        d.status = $('#statusFilter').val();
                        d.isApproved = $('#approvedFilter').val() === '' ? null : $('#approvedFilter').val() === 'true';
                        d.fromDate = $('#fromDateFilter').val();
                        d.toDate = $('#toDateFilter').val();
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
                order: [[4, 'desc']],
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
        $('#categoryFilter, #statusFilter, #approvedFilter, #fromDateFilter, #toDateFilter').on('change', function() {
            self.reloadTable();
            self.loadSummary();
        });

        // Clear filters
        $(document).on('click', '#clearFiltersBtn', function() {
            $('#categoryFilter').val('');
            $('#statusFilter').val('');
            $('#approvedFilter').val('');
            self.setDefaultDateRange();
            self.reloadTable();
            self.loadSummary();
        });

        // Create expense
        $(document).on('click', '#createExpenseBtn', function() {
            self.createExpense();
        });

        // View expense
        $(document).on('click', '.view-expense', function() {
            var id = $(this).data('id');
            self.viewExpense(id);
        });

        // Edit expense
        $(document).on('click', '.edit-expense', function() {
            var id = $(this).data('id');
            self.editExpense(id);
        });

        // Delete expense
        $(document).on('click', '.delete-expense', function() {
            var id = $(this).data('id');
            self.deleteExpense(id);
        });

        // Submit create form
        $('#createTransactionForm').on('submit', function(e) {
            e.preventDefault();
            self.submitCreateExpense();
        });

        // Submit edit form
        $('#editTransactionForm').on('submit', function(e) {
            e.preventDefault();
            self.submitEditExpense();
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
        // Initialize Select2 for employee dropdowns if they exist
        if ($('#createEmployeeId').length > 0) {
            if ($('#createEmployeeId').hasClass('select2-hidden-accessible')) {
                $('#createEmployeeId').select2('destroy');
            }
            $('#createEmployeeId').select2({
                placeholder: '-- Chọn Nhân Viên --',
                allowClear: true,
                width: '100%'
            });
        }
        if ($('#editEmployeeId').length > 0) {
            if ($('#editEmployeeId').hasClass('select2-hidden-accessible')) {
                $('#editEmployeeId').select2('destroy');
            }
            $('#editEmployeeId').select2({
                placeholder: '-- Chọn Nhân Viên --',
                allowClear: true,
                width: '100%'
            });
        }
    },

    // Load categories
    loadCategories: function() {
        var self = this;
        
        $.ajax({
            url: '/ExpenseManagement/GetCategories',
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
                if (typeof AuthHandler !== 'undefined' && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                }
            }
        });
    },

    // Load summary data
    loadSummary: function() {
        var self = this;
        
        var fromDate = $('#fromDateFilter').val();
        var toDate = $('#toDateFilter').val();
        
        $.ajax({
            url: '/ExpenseManagement/GetExpenseSummary',
            type: 'GET',
            data: {
                fromDate: fromDate,
                toDate: toDate
            },
            success: function(response) {
                if (response && response.success && response.data) {
                    var data = response.data;
                    
                    // Update summary cards
                    $('#totalExpense').text((data.totalExpense || 0).toLocaleString('vi-VN'));
                    $('#pendingExpense').text((data.pendingExpense || 0).toLocaleString('vi-VN'));
                    $('#approvedExpense').text((data.approvedExpense || 0).toLocaleString('vi-VN'));
                    $('#expenseCount').text(data.totalCount || 0);
                    
                    // Update category breakdown
                    self.updateCategoryBreakdown(data.categoryBreakdown || []);
                    
                    // Store in module
                    self.summary = {
                        totalExpense: data.totalExpense || 0,
                        pendingExpense: data.pendingExpense || 0,
                        approvedExpense: data.approvedExpense || 0,
                        expenseCount: data.totalCount || 0,
                        categoryBreakdown: data.categoryBreakdown || []
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

    // Update category breakdown display
    updateCategoryBreakdown: function(categories) {
        var self = this;
        var $container = $('#categoryBreakdown');
        $container.empty();

        if (!categories || categories.length === 0) {
            $container.html('<div class="col-12"><p class="text-muted">Không có dữ liệu</p></div>');
            return;
        }

        categories.forEach(function(item) {
            var percentage = self.summary.totalExpense > 0 
                ? ((item.Total / self.summary.totalExpense) * 100).toFixed(1)
                : 0;
            
            var card = `
                <div class="col-md-3 col-sm-6 mb-3">
                    <div class="info-box">
                        <span class="info-box-icon bg-danger elevation-1">
                            <i class="fas fa-tag"></i>
                        </span>
                        <div class="info-box-content">
                            <span class="info-box-text">${item.Category || 'N/A'}</span>
                            <span class="info-box-number">
                                ${(item.Total || 0).toLocaleString('vi-VN')} VNĐ
                                <small>(${item.Count || 0} phiếu)</small>
                            </span>
                            <div class="progress">
                                <div class="progress-bar bg-danger" style="width: ${percentage}%"></div>
                            </div>
                            <span class="progress-description">${percentage}% tổng chi</span>
                        </div>
                    </div>
                </div>
            `;
            $container.append(card);
        });
    },

    // Reload table with filters
    reloadTable: function() {
        if (this.expensesTable) {
            if (typeof this.expensesTable.ajax !== 'undefined') {
                this.expensesTable.ajax.reload();
            } else if (typeof this.expensesTable.reload !== 'undefined') {
                this.expensesTable.reload();
            } else {
                this.expensesTable.draw();
            }
        }
    },

    // Load employees for dropdown
    loadEmployees: function(selectId) {
        var self = this;
        
        $.ajax({
            url: '/ExpenseManagement/GetAvailableEmployees',
            type: 'GET',
            success: function(data) {
                var $select = $(selectId);
                $select.empty().append('<option value="">-- Chọn Nhân Viên --</option>');
                
                if (data && data.length > 0) {
                    data.forEach(function(emp) {
                        $select.append('<option value="' + emp.value + '">' + emp.text + '</option>');
                    });
                }
                
                // Reinitialize Select2
                if ($select.hasClass('select2-hidden-accessible')) {
                    $select.select2('destroy');
                }
                $select.select2({
                    placeholder: '-- Chọn Nhân Viên --',
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

    // Create expense
    createExpense: function() {
        var self = this;
        
        // Reset form
        $('#createTransactionForm')[0].reset();
        
        // Set default values for Expense
        $('#createTransactionType').val('Expense').prop('disabled', true);
        $('#createTransactionDate').val(new Date().toISOString().slice(0, 16));
        $('#createPaymentMethod').val('Cash');
        $('#createCurrency').val('VND');
        
        // Load employees
        self.loadEmployees('#createEmployeeId');
        
        // Show modal
        $('#createTransactionModal').modal('show');
    },

    // Submit create expense
    submitCreateExpense: function() {
        var self = this;

        if (!$('#createTransactionForm')[0].checkValidity()) {
            $('#createTransactionForm')[0].reportValidity();
            return;
        }

        function safeParseInt(value) {
            if (!value) return null;
            var parsed = parseInt(value);
            return isNaN(parsed) ? null : parsed;
        }

        function safeParseDate(dateString) {
            if (!dateString) return new Date().toISOString();
            var date = new Date(dateString);
            if (isNaN(date.getTime())) {
                return new Date().toISOString();
            }
            return date.toISOString();
        }

        var formData = {
            transactionType: 'Expense', // ✅ Always Expense
            category: $('#createCategory').val(),
            subCategory: $('#createSubCategory').val() || null,
            amount: parseFloat($('#createAmount').val()) || 0,
            currency: 'VND',
            transactionDate: safeParseDate($('#createTransactionDate').val()),
            paymentMethod: $('#createPaymentMethod').val() || 'Cash',
            referenceNumber: $('#createReferenceNumber').val() || null,
            description: $('#createDescription').val() || null,
            employeeId: safeParseInt($('#createEmployeeId').val()),
            notes: $('#createNotes').val() || null
        };

        $.ajax({
            url: '/ExpenseManagement/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    if (typeof GarageApp !== 'undefined') {
                        GarageApp.showSuccess(response.message || 'Tạo phiếu chi thành công');
                    } else {
                        Swal.fire('Thành công', response.message || 'Tạo phiếu chi thành công', 'success');
                    }
                    $('#createTransactionModal').modal('hide');
                    self.reloadTable();
                    self.loadSummary();
                } else {
                    if (typeof GarageApp !== 'undefined') {
                        GarageApp.showError(response.error || 'Lỗi khi tạo phiếu chi');
                    } else {
                        Swal.fire('Lỗi', response.error || 'Lỗi khi tạo phiếu chi', 'error');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (typeof AuthHandler !== 'undefined' && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    var errorMsg = xhr.responseJSON?.error || 'Lỗi khi tạo phiếu chi';
                    if (typeof GarageApp !== 'undefined') {
                        GarageApp.showError(errorMsg);
                    } else {
                        Swal.fire('Lỗi', errorMsg, 'error');
                    }
                }
            }
        });
    },

    // Edit expense
    editExpense: function(id) {
        var self = this;
        
        $.ajax({
            url: '/ExpenseManagement/GetById/' + id,
            type: 'GET',
            success: function(response) {
                if (response && response.success && response.data) {
                    var expense = response.data;
                    
                    // Populate form
                    $('#editTransactionId').val(expense.id);
                    $('#editTransactionType').val('Expense').prop('disabled', true);
                    $('#editCategory').val(expense.category);
                    $('#editSubCategory').val(expense.subCategory || '');
                    $('#editAmount').val(expense.amount);
                    
                    // Set date
                    if (expense.transactionDate) {
                        var transactionDate = new Date(expense.transactionDate);
                        if (!isNaN(transactionDate.getTime())) {
                            var localDateTime = transactionDate.toISOString().slice(0, 16);
                            $('#editTransactionDate').val(localDateTime);
                        } else {
                            $('#editTransactionDate').val(new Date().toISOString().slice(0, 16));
                        }
                    } else {
                        $('#editTransactionDate').val(new Date().toISOString().slice(0, 16));
                    }
                    
                    $('#editPaymentMethod').val(expense.paymentMethod || 'Cash');
                    $('#editReferenceNumber').val(expense.referenceNumber || '');
                    $('#editDescription').val(expense.description || '');
                    $('#editNotes').val(expense.notes || '');
                    
                    // Load employees and set selected value
                    self.loadEmployees('#editEmployeeId');
                    if (expense.employeeId) {
                        setTimeout(function() {
                            $('#editEmployeeId').val(expense.employeeId.toString()).trigger('change');
                        }, 500);
                    }
                    
                    // Show modal
                    $('#editTransactionModal').modal('show');
                } else {
                    if (typeof GarageApp !== 'undefined') {
                        GarageApp.showError('Không tìm thấy phiếu chi');
                    } else {
                        Swal.fire('Lỗi', 'Không tìm thấy phiếu chi', 'error');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (typeof AuthHandler !== 'undefined' && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    if (typeof GarageApp !== 'undefined') {
                        GarageApp.showError('Lỗi khi tải thông tin phiếu chi');
                    } else {
                        Swal.fire('Lỗi', 'Lỗi khi tải thông tin phiếu chi', 'error');
                    }
                }
            }
        });
    },

    // Submit edit expense
    submitEditExpense: function() {
        var self = this;
        var id = $('#editTransactionId').val();

        if (!id) {
            if (typeof GarageApp !== 'undefined') {
                GarageApp.showError('Không tìm thấy ID phiếu chi');
            } else {
                Swal.fire('Lỗi', 'Không tìm thấy ID phiếu chi', 'error');
            }
            return;
        }

        if (!$('#editTransactionForm')[0].checkValidity()) {
            $('#editTransactionForm')[0].reportValidity();
            return;
        }

        function safeParseInt(value) {
            if (!value) return null;
            var parsed = parseInt(value);
            return isNaN(parsed) ? null : parsed;
        }

        function safeParseDate(dateString) {
            if (!dateString) return new Date().toISOString();
            var date = new Date(dateString);
            if (isNaN(date.getTime())) {
                return new Date().toISOString();
            }
            return date.toISOString();
        }

        var formData = {
            transactionType: 'Expense', // ✅ Always Expense
            category: $('#editCategory').val(),
            subCategory: $('#editSubCategory').val() || null,
            amount: parseFloat($('#editAmount').val()) || 0,
            currency: 'VND',
            transactionDate: safeParseDate($('#editTransactionDate').val()),
            paymentMethod: $('#editPaymentMethod').val() || 'Cash',
            referenceNumber: $('#editReferenceNumber').val() || null,
            description: $('#editDescription').val() || null,
            employeeId: safeParseInt($('#editEmployeeId').val()),
            notes: $('#editNotes').val() || null
        };

        $.ajax({
            url: '/ExpenseManagement/Update/' + id,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    if (typeof GarageApp !== 'undefined') {
                        GarageApp.showSuccess(response.message || 'Cập nhật phiếu chi thành công');
                    } else {
                        Swal.fire('Thành công', response.message || 'Cập nhật phiếu chi thành công', 'success');
                    }
                    $('#editTransactionModal').modal('hide');
                    self.reloadTable();
                    self.loadSummary();
                } else {
                    if (typeof GarageApp !== 'undefined') {
                        GarageApp.showError(response.error || 'Lỗi khi cập nhật phiếu chi');
                    } else {
                        Swal.fire('Lỗi', response.error || 'Lỗi khi cập nhật phiếu chi', 'error');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (typeof AuthHandler !== 'undefined' && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    var errorMsg = xhr.responseJSON?.error || 'Lỗi khi cập nhật phiếu chi';
                    if (typeof GarageApp !== 'undefined') {
                        GarageApp.showError(errorMsg);
                    } else {
                        Swal.fire('Lỗi', errorMsg, 'error');
                    }
                }
            }
        });
    },

    // View expense
    viewExpense: function(id) {
        var self = this;
        
        $.ajax({
            url: '/ExpenseManagement/GetById/' + id,
            type: 'GET',
            success: function(response) {
                if (response && response.success && response.data) {
                    // Use existing view modal from FinancialTransactionManagement
                    if (typeof FinancialTransactionManagement !== 'undefined' && FinancialTransactionManagement.viewTransaction) {
                        FinancialTransactionManagement.viewTransaction(id);
                    } else {
                        // Fallback: Show basic info
                        var expense = response.data;
                        Swal.fire({
                            title: 'Chi Tiết Phiếu Chi',
                            html: `
                                <div class="text-left">
                                    <p><strong>Mã Phiếu:</strong> ${expense.transactionNumber || 'N/A'}</p>
                                    <p><strong>Danh Mục:</strong> ${expense.category || 'N/A'}</p>
                                    <p><strong>Số Tiền:</strong> ${(expense.amount || 0).toLocaleString('vi-VN')} VNĐ</p>
                                    <p><strong>Ngày GD:</strong> ${expense.transactionDate ? new Date(expense.transactionDate).toLocaleDateString('vi-VN') : 'N/A'}</p>
                                    <p><strong>Trạng Thái:</strong> ${expense.status || 'N/A'}</p>
                                </div>
                            `,
                            icon: 'info'
                        });
                    }
                } else {
                    if (typeof GarageApp !== 'undefined') {
                        GarageApp.showError('Không tìm thấy phiếu chi');
                    } else {
                        Swal.fire('Lỗi', 'Không tìm thấy phiếu chi', 'error');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (typeof AuthHandler !== 'undefined' && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    if (typeof GarageApp !== 'undefined') {
                        GarageApp.showError('Lỗi khi tải thông tin phiếu chi');
                    } else {
                        Swal.fire('Lỗi', 'Lỗi khi tải thông tin phiếu chi', 'error');
                    }
                }
            }
        });
    },

    // Delete expense
    deleteExpense: function(id) {
        var self = this;
        
        Swal.fire({
            title: 'Xác nhận xóa',
            text: 'Bạn có chắc chắn muốn xóa phiếu chi này?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/ExpenseManagement/Delete/' + id,
                    type: 'DELETE',
                    success: function(response) {
                        if (response.success) {
                            if (typeof GarageApp !== 'undefined') {
                                GarageApp.showSuccess(response.message || 'Xóa phiếu chi thành công');
                            } else {
                                Swal.fire('Thành công', response.message || 'Xóa phiếu chi thành công', 'success');
                            }
                            self.reloadTable();
                            self.loadSummary();
                        } else {
                            if (typeof GarageApp !== 'undefined') {
                                GarageApp.showError(response.error || 'Lỗi khi xóa phiếu chi');
                            } else {
                                Swal.fire('Lỗi', response.error || 'Lỗi khi xóa phiếu chi', 'error');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (typeof AuthHandler !== 'undefined' && AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            var errorMsg = xhr.responseJSON?.error || 'Lỗi khi xóa phiếu chi';
                            if (typeof GarageApp !== 'undefined') {
                                GarageApp.showError(errorMsg);
                            } else {
                                Swal.fire('Lỗi', errorMsg, 'error');
                            }
                        }
                    }
                });
            }
        });
    },

    // Export Excel
    exportExcel: function() {
        Swal.fire({
            icon: 'info',
            title: 'Xuất Excel',
            text: 'Tính năng xuất Excel đang được phát triển'
        });
    },

    // Export PDF
    exportPdf: function() {
        Swal.fire({
            icon: 'info',
            title: 'Xuất PDF',
            text: 'Tính năng xuất PDF đang được phát triển'
        });
    }
};

// Initialize when document is ready
$(document).ready(function() {
    if ($('#expensesTable').length > 0) {
        ExpenseManagement.init();
    }
});

