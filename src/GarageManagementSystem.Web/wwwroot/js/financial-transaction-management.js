/**
 * Financial Transaction Management Module
 * 
 * Handles all financial transaction (Phiếu Thu/Chi) operations
 * CRUD operations for financial transactions
 * ✅ 4.3.1.9: Attachment upload/download support
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
        this.initSelect2();
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
                        <button class="btn btn-info btn-sm view-transaction mr-1" data-id="${row.id}" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-warning btn-sm edit-transaction mr-1" data-id="${row.id}" title="Chỉnh sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn btn-danger btn-sm delete-transaction" data-id="${row.id}" title="Xóa">
                            <i class="fas fa-trash"></i>
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
        $(document).off('click', '#createTransactionBtn');
        $(document).off('click', '.view-transaction');
        $(document).off('click', '.edit-transaction');
        $(document).off('click', '.delete-transaction');
        $('#transactionTypeFilter, #categoryFilter, #statusFilter').off('change');
        $('#createTransactionForm').off('submit');
        $('#editTransactionForm').off('submit');

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

        // Create transaction
        $(document).on('click', '#createTransactionBtn', function() {
            self.createTransaction();
        });

        // View transaction
        $(document).on('click', '.view-transaction', function() {
            var id = $(this).data('id');
            self.viewTransaction(id);
        });

        // Edit transaction
        $(document).on('click', '.edit-transaction', function() {
            var id = $(this).data('id');
            self.editTransaction(id);
        });

        // Delete transaction
        $(document).on('click', '.delete-transaction', function() {
            var id = $(this).data('id');
            self.deleteTransaction(id);
        });

        // Submit create form
        $('#createTransactionForm').on('submit', function(e) {
            e.preventDefault();
            self.submitCreateTransaction();
        });

        // Submit edit form
        $('#editTransactionForm').on('submit', function(e) {
            e.preventDefault();
            self.submitEditTransaction();
        });

        // ✅ 4.3.1.9: Upload attachment button
        $(document).on('click', '#uploadAttachmentBtn', function() {
            var transactionId = $('#viewTransactionModal').data('transactionId');
            if (transactionId) {
                self.showUploadAttachmentModal(transactionId);
            }
        });
    },

    // Initialize Select2 dropdowns
    initSelect2: function() {
        // Initialize Select2 for employee dropdowns if they exist
        if ($('#createEmployeeId').length > 0) {
            $('#createEmployeeId').select2({
                placeholder: '-- Chọn Nhân Viên --',
                allowClear: true,
                width: '100%'
            });
        }
        if ($('#editEmployeeId').length > 0) {
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

    // Load employees for dropdown
    loadEmployees: function(selectId) {
        var self = this;
        
        $.ajax({
            url: '/FinancialTransactionManagement/GetAvailableEmployees',
            type: 'GET',
            success: function(data) {
                var $select = $(selectId);
                $select.empty().append('<option value="">-- Chọn Nhân Viên --</option>');
                
                if (data && data.length > 0) {
                    $.each(data, function(i, item) {
                        $select.append('<option value="' + item.value + '">' + item.text + '</option>');
                    });
                }
                
                // Re-initialize Select2
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
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                }
            }
        });
    },

    // Create transaction
    createTransaction: function() {
        var self = this;
        
        // Reset form
        $('#createTransactionForm')[0].reset();
        $('#createTransactionDate').val(new Date().toISOString().slice(0, 16));
        
        // Load employees
        this.loadEmployees('#createEmployeeId');
        
        // Show modal
        $('#createTransactionModal').modal('show');
    },

    // Submit create transaction
    submitCreateTransaction: function() {
        var self = this;
        
        if (!$('#createTransactionForm')[0].checkValidity()) {
            $('#createTransactionForm')[0].reportValidity();
            return;
        }

        // ✅ SỬA: Safe parseInt và Date parsing
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
            transactionType: $('#createTransactionType').val(),
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
            url: '/FinancialTransactionManagement/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess(response.message || 'Tạo phiếu tài chính thành công');
                    $('#createTransactionModal').modal('hide');
                    self.reloadTable();
                    self.loadSummary();
                } else {
                    GarageApp.showError(response.error || 'Lỗi khi tạo phiếu tài chính');
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    var errorMsg = xhr.responseJSON?.error || 'Lỗi khi tạo phiếu tài chính';
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Edit transaction
    editTransaction: function(id) {
        var self = this;
        
        $.ajax({
            url: '/FinancialTransactionManagement/GetFinancialTransactionById/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success && response.data) {
                        var transaction = response.data;
                        
                        // Populate form
                        $('#editTransactionId').val(transaction.id);
                        $('#editTransactionType').val(transaction.transactionType);
                        $('#editCategory').val(transaction.category);
                        $('#editSubCategory').val(transaction.subCategory || '');
                        $('#editAmount').val(transaction.amount);
                        
                        // Set date if exists
                        if (transaction.transactionDate) {
                            var transactionDate = new Date(transaction.transactionDate);
                            if (!isNaN(transactionDate.getTime())) {
                                var localDateTime = transactionDate.toISOString().slice(0, 16);
                                $('#editTransactionDate').val(localDateTime);
                            } else {
                                $('#editTransactionDate').val(new Date().toISOString().slice(0, 16));
                            }
                        } else {
                            $('#editTransactionDate').val(new Date().toISOString().slice(0, 16));
                        }
                        
                        $('#editPaymentMethod').val(transaction.paymentMethod || 'Cash');
                        $('#editReferenceNumber').val(transaction.referenceNumber || '');
                        $('#editDescription').val(transaction.description || '');
                        $('#editNotes').val(transaction.notes || '');
                        
                        // Load employees and set selected value
                        self.loadEmployees('#editEmployeeId');
                        if (transaction.employeeId) {
                            setTimeout(function() {
                                $('#editEmployeeId').val(transaction.employeeId.toString()).trigger('change');
                            }, 500);
                        }
                        
                        // Show modal
                        $('#editTransactionModal').modal('show');
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

    // Submit edit transaction
    submitEditTransaction: function() {
        var self = this;
        var id = $('#editTransactionId').val();

        if (!id) {
            GarageApp.showError('Không tìm thấy ID phiếu tài chính');
            return;
        }

        if (!$('#editTransactionForm')[0].checkValidity()) {
            $('#editTransactionForm')[0].reportValidity();
            return;
        }

        // ✅ SỬA: Safe parseInt và Date parsing
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
            transactionType: $('#editTransactionType').val(),
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
            url: '/FinancialTransactionManagement/Update/' + id,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess(response.message || 'Cập nhật phiếu tài chính thành công');
                    $('#editTransactionModal').modal('hide');
                    self.reloadTable();
                    self.loadSummary();
                } else {
                    GarageApp.showError(response.error || 'Lỗi khi cập nhật phiếu tài chính');
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    var errorMsg = xhr.responseJSON?.error || 'Lỗi khi cập nhật phiếu tài chính';
                    GarageApp.showError(errorMsg);
                }
            }
        });
    },

    // Delete transaction
    deleteTransaction: function(id) {
        var self = this;
        
        Swal.fire({
            title: 'Xác nhận xóa',
            text: 'Bạn có chắc chắn muốn xóa phiếu tài chính này?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/FinancialTransactionManagement/Delete/' + id,
                    type: 'DELETE',
                    success: function(response) {
                        if (response.success) {
                            GarageApp.showSuccess(response.message || 'Xóa phiếu tài chính thành công');
                            self.reloadTable();
                            self.loadSummary();
                        } else {
                            GarageApp.showError(response.error || 'Lỗi khi xóa phiếu tài chính');
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            var errorMsg = xhr.responseJSON?.error || 'Lỗi khi xóa phiếu tài chính';
                            GarageApp.showError(errorMsg);
                        }
                    }
                });
            }
        });
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

    // Show transaction details in modal
    showTransactionDetails: function(transaction) {
        // Populate modal
        $('#viewTransactionNumber').text(transaction.transactionNumber || '');
        
        var typeBadge = transaction.transactionType === 'Income' 
            ? '<span class="badge badge-success">Thu</span>' 
            : '<span class="badge badge-danger">Chi</span>';
        $('#viewTransactionType').html(typeBadge);
        
        $('#viewCategory').text(transaction.category || '');
        $('#viewSubCategory').text(transaction.subCategory || '');
        $('#viewAmount').text((parseFloat(transaction.amount || 0).toLocaleString('vi-VN') + ' VNĐ'));
        
        // ✅ SỬA: Safe Date parsing
        if (transaction.transactionDate) {
            var date = new Date(transaction.transactionDate);
            if (!isNaN(date.getTime())) {
                $('#viewTransactionDate').text(date.toLocaleString('vi-VN'));
            } else {
                $('#viewTransactionDate').text('');
            }
        } else {
            $('#viewTransactionDate').text('');
        }
        
        $('#viewPaymentMethod').text(transaction.paymentMethod || '');
        $('#viewReferenceNumber').text(transaction.referenceNumber || '');
        $('#viewEmployeeName').text(transaction.employeeName || '');
        
        var statusBadges = {
            'Pending': '<span class="badge badge-warning">Chờ Xử Lý</span>',
            'Approved': '<span class="badge badge-success">Đã Duyệt</span>',
            'Completed': '<span class="badge badge-info">Hoàn Thành</span>',
            'Cancelled': '<span class="badge badge-danger">Đã Hủy</span>'
        };
        $('#viewStatus').html(statusBadges[transaction.status] || transaction.status || '');
        
        $('#viewDescription').text(transaction.description || '');
        $('#viewNotes').text(transaction.notes || '');
        
        // ✅ 4.3.1.9: Load attachments
        self.loadAttachments(transaction.id);
        
        // Store transaction ID for upload
        $('#viewTransactionModal').data('transactionId', transaction.id);
        $('#uploadAttachmentBtn').show();
        
        // Show modal
        $('#viewTransactionModal').modal('show');
    },

    // ✅ 4.3.1.9: Load attachments for a transaction
    loadAttachments: function(transactionId) {
        var self = this;
        
        $.ajax({
            url: '/api/' + ApiEndpoints.FinancialTransactionAttachments.GetByTransaction.replace('{0}', transactionId),
            type: 'GET',
            headers: {
                'Authorization': 'Bearer ' + (localStorage.getItem('access_token') || '')
            },
            success: function(response) {
                if (response && response.success && response.data) {
                    self.displayAttachments(response.data);
                } else {
                    $('#viewAttachments').html('<p class="text-muted">Không có chứng từ đính kèm</p>');
                }
            },
            error: function(xhr, status, error) {
                if (typeof AuthHandler !== 'undefined' && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    $('#viewAttachments').html('<p class="text-danger">Lỗi khi tải chứng từ</p>');
                }
            }
        });
    },

    // ✅ 4.3.1.9: Display attachments list
    displayAttachments: function(attachments) {
        var self = this;
        var $container = $('#viewAttachments');
        $container.empty();
        
        if (!attachments || attachments.length === 0) {
            $container.html('<p class="text-muted">Không có chứng từ đính kèm</p>');
            return;
        }
        
        var html = '<div class="list-group">';
        attachments.forEach(function(attachment) {
            var fileSize = (attachment.fileSize / 1024).toFixed(2) + ' KB';
            var fileIcon = self.getFileIcon(attachment.fileName);
            
            html += `
                <div class="list-group-item d-flex justify-content-between align-items-center">
                    <div>
                        <i class="${fileIcon} mr-2"></i>
                        <strong>${attachment.fileName}</strong>
                        <small class="text-muted ml-2">${fileSize}</small>
                        ${attachment.description ? '<br><small class="text-muted">' + attachment.description + '</small>' : ''}
                    </div>
                    <div>
                        <button class="btn btn-sm btn-info download-attachment mr-1" data-id="${attachment.id}" title="Tải xuống">
                            <i class="fas fa-download"></i>
                        </button>
                        <button class="btn btn-sm btn-danger delete-attachment" data-id="${attachment.id}" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
            `;
        });
        html += '</div>';
        
        $container.html(html);
        
        // Bind download and delete events
        $container.off('click', '.download-attachment');
        $container.off('click', '.delete-attachment');
        $container.on('click', '.download-attachment', function() {
            var attachmentId = $(this).data('id');
            self.downloadAttachment(attachmentId);
        });
        $container.on('click', '.delete-attachment', function() {
            var attachmentId = $(this).data('id');
            self.deleteAttachment(attachmentId);
        });
    },

    // ✅ 4.3.1.9: Get file icon based on extension
    getFileIcon: function(fileName) {
        var ext = fileName.split('.').pop().toLowerCase();
        var icons = {
            'pdf': 'fas fa-file-pdf text-danger',
            'jpg': 'fas fa-file-image text-primary',
            'jpeg': 'fas fa-file-image text-primary',
            'png': 'fas fa-file-image text-primary',
            'doc': 'fas fa-file-word text-primary',
            'docx': 'fas fa-file-word text-primary',
            'xls': 'fas fa-file-excel text-success',
            'xlsx': 'fas fa-file-excel text-success'
        };
        return icons[ext] || 'fas fa-file text-secondary';
    },

    // ✅ 4.3.1.9: Download attachment
    downloadAttachment: function(attachmentId) {
        var downloadUrl = '/api/' + ApiEndpoints.FinancialTransactionAttachments.Download.replace('{0}', attachmentId);
        window.open(downloadUrl, '_blank');
    },

    // ✅ 4.3.1.9: Delete attachment
    deleteAttachment: function(attachmentId) {
        var self = this;
        
        Swal.fire({
            title: 'Xác nhận xóa',
            text: 'Bạn có chắc chắn muốn xóa chứng từ này?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/api/' + ApiEndpoints.FinancialTransactionAttachments.Delete.replace('{0}', attachmentId),
                    type: 'DELETE',
                    headers: {
                        'Authorization': 'Bearer ' + (localStorage.getItem('access_token') || '')
                    },
                    success: function(response) {
                        if (response && response.success) {
                            if (typeof GarageApp !== 'undefined') {
                                GarageApp.showSuccess(response.message || 'Xóa chứng từ thành công');
                            } else {
                                Swal.fire('Thành công', response.message || 'Xóa chứng từ thành công', 'success');
                            }
                            // Reload attachments
                            var transactionId = $('#viewTransactionModal').data('transactionId');
                            if (transactionId) {
                                self.loadAttachments(transactionId);
                            }
                        } else {
                            if (typeof GarageApp !== 'undefined') {
                                GarageApp.showError(response.errorMessage || 'Lỗi khi xóa chứng từ');
                            } else {
                                Swal.fire('Lỗi', response.errorMessage || 'Lỗi khi xóa chứng từ', 'error');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (typeof AuthHandler !== 'undefined' && AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            var errorMsg = xhr.responseJSON?.errorMessage || 'Lỗi khi xóa chứng từ';
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

    // ✅ 4.3.1.9: Show upload attachment modal
    showUploadAttachmentModal: function(transactionId) {
        Swal.fire({
            title: 'Upload Chứng Từ',
            html: `
                <form id="uploadAttachmentForm">
                    <div class="form-group">
                        <label>Chọn File:</label>
                        <input type="file" class="form-control" id="attachmentFile" accept=".pdf,.jpg,.jpeg,.png,.doc,.docx,.xlsx,.xls" required>
                        <small class="form-text text-muted">Chỉ chấp nhận: PDF, JPG, PNG, DOC, DOCX, XLSX, XLS (tối đa 5MB)</small>
                    </div>
                    <div class="form-group">
                        <label>Loại Chứng Từ:</label>
                        <select class="form-control" id="attachmentFileType">
                            <option value="Invoice">Hóa Đơn</option>
                            <option value="Receipt">Biên Lai</option>
                            <option value="Contract">Hợp Đồng</option>
                            <option value="Other">Khác</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label>Mô Tả:</label>
                        <textarea class="form-control" id="attachmentDescription" rows="2"></textarea>
                    </div>
                </form>
            `,
            showCancelButton: true,
            confirmButtonText: 'Upload',
            cancelButtonText: 'Hủy',
            preConfirm: () => {
                var file = document.getElementById('attachmentFile').files[0];
                if (!file) {
                    Swal.showValidationMessage('Vui lòng chọn file');
                    return false;
                }
                if (file.size > 5 * 1024 * 1024) {
                    Swal.showValidationMessage('File không được vượt quá 5MB');
                    return false;
                }
                return {
                    file: file,
                    fileType: document.getElementById('attachmentFileType').value,
                    description: document.getElementById('attachmentDescription').value
                };
            }
        }).then((result) => {
            if (result.isConfirmed && result.value) {
                this.uploadAttachment(transactionId, result.value.file, result.value.fileType, result.value.description);
            }
        });
    },

    // ✅ 4.3.1.9: Upload attachment
    uploadAttachment: function(transactionId, file, fileType, description) {
        var self = this;
        
        var formData = new FormData();
        formData.append('financialTransactionId', transactionId);
        formData.append('file', file);
        formData.append('fileType', fileType);
        if (description) {
            formData.append('description', description);
        }
        
        $.ajax({
            url: '/api/' + ApiEndpoints.FinancialTransactionAttachments.Upload,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            headers: {
                'Authorization': 'Bearer ' + (localStorage.getItem('access_token') || '')
            },
            success: function(response) {
                if (response && response.success) {
                    if (typeof GarageApp !== 'undefined') {
                        GarageApp.showSuccess(response.message || 'Upload chứng từ thành công');
                    } else {
                        Swal.fire('Thành công', response.message || 'Upload chứng từ thành công', 'success');
                    }
                    // Reload attachments
                    self.loadAttachments(transactionId);
                } else {
                    if (typeof GarageApp !== 'undefined') {
                        GarageApp.showError(response.errorMessage || 'Lỗi khi upload chứng từ');
                    } else {
                        Swal.fire('Lỗi', response.errorMessage || 'Lỗi khi upload chứng từ', 'error');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (typeof AuthHandler !== 'undefined' && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    var errorMsg = xhr.responseJSON?.errorMessage || 'Lỗi khi upload chứng từ';
                    if (typeof GarageApp !== 'undefined') {
                        GarageApp.showError(errorMsg);
                    } else {
                        Swal.fire('Lỗi', errorMsg, 'error');
                    }
                }
            }
        });
    }
};

// Initialize on document ready
$(document).ready(function() {
    if ($('#financialTransactionsTable').length > 0) {
        FinancialTransactionManagement.init();
    }
});
