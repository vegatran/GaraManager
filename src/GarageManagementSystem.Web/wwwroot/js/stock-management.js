/**
 * Stock Management Module
 * 
 * Handles all stock transaction-related operations
 * CRUD operations for stock transactions
 */

window.StockManagement = {
    // DataTable instance
    stockTable: null,

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.loadDropdownData();
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        // Sử dụng DataTablesUtility với style chung
        var columns = [
            { data: 'transactionNumber', title: 'Mã Giao Dịch', width: '120px' },
            { data: 'partName', title: 'Phụ Tùng' },
            { 
                data: 'transactionType', 
                title: 'Loại Giao Dịch',
                render: function(data, type, row) {
                    var badgeClass = 'badge-secondary';
                    switch(data) {
                        case 'Nhập kho': badgeClass = 'badge-success'; break;
                        case 'Xuất kho': badgeClass = 'badge-danger'; break;
                        case 'Điều chỉnh': badgeClass = 'badge-warning'; break;
                        case 'Tồn đầu kỳ': badgeClass = 'badge-info'; break;
                    }
                    return `<span class="badge ${badgeClass}">${data}</span>`;
                }
            },
            { 
                data: 'quantity', 
                title: 'Số Lượng',
                className: 'text-center',
                render: DataTablesUtility.renderNumber
            },
            { 
                data: 'quantityBefore', 
                title: 'Tồn Trước',
                className: 'text-center',
                render: DataTablesUtility.renderNumber
            },
            { 
                data: 'quantityAfter', 
                title: 'Tồn Sau',
                className: 'text-center',
                render: DataTablesUtility.renderNumber
            },
            { 
                data: 'unitPrice', 
                title: 'Đơn Giá',
                render: DataTablesUtility.renderCurrency
            },
            { 
                data: 'totalAmount', 
                title: 'Thành Tiền',
                render: DataTablesUtility.renderCurrency
            },
            { 
                data: 'transactionDate', 
                title: 'Ngày Giao Dịch',
                render: DataTablesUtility.renderDate
            },
            { 
                data: 'notes', 
                title: 'Ghi Chú',
                render: function(data) {
                    if (data && data !== 'N/A' && data.length > 30) {
                        return `<span title="${data}">${data.substring(0, 30)}...</span>`;
                    }
                    return data || 'N/A';
                }
            },
            {
                data: null,
                title: 'Thao Tác',
                width: '100px',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    return `
                        <button class="btn btn-info btn-sm view-transaction" data-id="${row.id}" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                    `;
                }
            }
        ];
        
        this.stockTable = DataTablesUtility.initAjaxTable('#stockTransactionsTable', '/StockManagement/GetStockTransactions', columns, {
            order: [[0, 'desc']],
            pageLength: 25,
            dom: 'rtip'  // Chỉ hiển thị table, paging, info, processing (không có search box và length menu)
        });
    },

    // Bind events
    bindEvents: function() {
        var self = this;

        // Search functionality
        $('#searchInput').on('keyup', function() {
            self.stockTable.search(this.value).draw();
        });

        // Create transaction form
        $(document).on('submit', '#createTransactionForm', function(e) {
            e.preventDefault();
            self.createStockTransaction();
        });
        
        // Add item button
        $(document).on('click', '#addItemBtn', function() {
            self.addItemRow();
        });
        
        // Remove item button
        $(document).on('click', '.remove-item-btn', function() {
            self.removeItemRow($(this));
        });
        
        // Calculate totals when quantity or price changes
        $(document).on('input', '.item-quantity, .item-unit-price', function() {
            self.calculateItemTotal($(this));
            self.calculateGrandTotal();
        });

        // Import form
        $(document).on('submit', '#importForm', function(e) {
            e.preventDefault();
            self.importOpeningBalance();
        });

        // View transaction
        $(document).on('click', '.view-transaction', function() {
            var id = $(this).data('id');
            self.viewTransaction(id);
        });

        // Download template
        $(document).on('click', '#downloadTemplateBtn', function() {
            self.downloadTemplate();
        });

        // ✅ THÊM: Initialize Typeahead khi modal mở
        $('#createTransactionModal').on('shown.bs.modal', function() {
            // Initialize Typeahead cho dòng đầu tiên nếu có
            var firstInput = $('#itemsTableBody tr:first .item-part-typeahead');
            var firstHidden = $('#itemsTableBody tr:first .item-part-id');
            if (firstInput.length > 0) {
                self.initializeTypeahead(firstInput, firstHidden);
            }
        });

        // File input change
        $(document).on('change', '#excelFile', function() {
            var fileName = $(this).val().split('\\').pop();
            $(this).next('.custom-file-label').text(fileName);
        });

        // Validate file button
        $(document).on('click', '#validateFileBtn', function() {
            self.validateExcelFile();
        });
        
        // Initialize first item row when modal opens
        $(document).on('shown.bs.modal', '#createTransactionModal', function() {
            if ($('#itemsTableBody tr').length === 0) {
                self.addItemRow();
            }
            // Set current date/time
            var now = new Date();
            var dateTimeLocal = now.toISOString().slice(0, 16);
            $('#transactionDate').val(dateTimeLocal);
            
            // ✅ TỐI ƯU: Keyboard shortcuts
            $(this).off('keydown.stockManagement').on('keydown.stockManagement', function(e) {
                // Ctrl+Enter để submit form
                if (e.ctrlKey && e.keyCode === 13) {
                    e.preventDefault();
                    $('#createTransactionForm').submit();
                }
                // F9 để thêm item mới
                if (e.keyCode === 120) { // F9
                    e.preventDefault();
                    self.addItemRow();
                }
            });
        });
    },

    // Load dropdown data
    loadDropdownData: function() {
        this.loadParts();
        this.loadTransactionTypes();
        this.loadSuppliers();
    },

    // Load parts for dropdowns
    loadParts: function() {
        var self = this;
        
        $.ajax({
            url: '/StockManagement/GetAvailableParts',
            type: 'GET',
            success: function(response) {
                console.log('🔍 Parts API Response:', response);
                
                if (response.success && response.data) {
                    // Clear existing options
                    $('#partId').empty();
                    $('#partId').append('<option value="">-- Chọn phụ tùng --</option>');
                    
                    response.data.forEach(function(part) {
                        $('#partId').append(
                            '<option value="' + part.id + '">' + part.text + '</option>'
                        );
                    });
                    
                    // Initialize Select2
                    $('#partId').select2({
                        placeholder: "-- Chọn phụ tùng --",
                        allowClear: true,
                        width: '100%'
                    });
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading parts:', error);
                if (AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                }
            }
        });
    },

    // Load transaction types for dropdowns
    loadTransactionTypes: function() {
        var self = this;
        
        $.ajax({
            url: '/StockManagement/GetTransactionTypes',
            type: 'GET',
            success: function(response) {
                console.log('🔍 Transaction Types API Response:', response);
                
                if (response.success && response.data) {
                    // Clear existing options
                    $('#transactionType').empty();
                    $('#transactionType').append('<option value="">-- Chọn loại giao dịch --</option>');
                    
                    response.data.forEach(function(type) {
                        $('#transactionType').append(
                            '<option value="' + type.id + '">' + type.text + '</option>'
                        );
                    });
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading transaction types:', error);
            }
        });
    },

    // Load suppliers for dropdowns
    loadSuppliers: function() {
        var self = this;
        
        $.ajax({
            url: '/StockManagement/GetAvailableSuppliers',
            type: 'GET',
            success: function(response) {
                console.log('🔍 Suppliers API Response:', response);
                
                if (response.success && response.data) {
                    // Clear existing options
                    $('#supplierId').empty();
                    $('#supplierId').append('<option value="">-- Chọn nhà cung cấp --</option>');
                    
                    response.data.forEach(function(supplier) {
                        $('#supplierId').append(
                            '<option value="' + supplier.id + '">' + supplier.text + '</option>'
                        );
                    });
                    
                    // Initialize Select2
                    $('#supplierId').select2({
                        placeholder: "-- Chọn nhà cung cấp --",
                        allowClear: true,
                        width: '100%'
                    });
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading suppliers:', error);
                if (AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                }
            }
        });
    },

    // Create stock transaction
    createStockTransaction: function() {
        var self = this;
        var formData = {
            PartId: parseInt($('#partId').val()),
            TransactionType: $('#transactionType').val(),
            Quantity: parseInt($('#quantity').val()),
            UnitPrice: parseFloat($('#unitPrice').val()),
            SupplierId: $('#supplierId').val() ? parseInt($('#supplierId').val()) : null,
            ReferenceNumber: $('#referenceNumber').val(),
            Notes: $('#notes').val()
        };

        $.ajax({
            url: '/StockManagement/CreateStockTransaction',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    Swal.fire({
                        title: 'Thành công!',
                        text: response.message,
                        icon: 'success',
                        confirmButtonText: 'OK'
                    }).then(() => {
                        $('#createTransactionModal').modal('hide');
                        self.stockTable.ajax.reload();
                        $('#createTransactionForm')[0].reset();
                        $('#partId, #supplierId').val(null).trigger('change');
                    });
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.message,
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: 'Có lỗi xảy ra khi tạo giao dịch kho',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            }
        });
    },

    // View transaction details
    viewTransaction: function(id) {
        var self = this;
        
        $.ajax({
            url: '/StockManagement/GetStockTransaction/' + id,
            type: 'GET',
            success: function(response) {
                if (response.success) {
                    var data = response.data;
                    var html = `
                        <div class="row">
                            <div class="col-md-6">
                                <strong>Mã giao dịch:</strong> ${data.transactionNumber || 'N/A'}<br>
                                <strong>Phụ tùng:</strong> ${data.part?.partName || 'N/A'}<br>
                                <strong>Loại giao dịch:</strong> ${data.transactionTypeDisplay || 'N/A'}<br>
                                <strong>Số lượng:</strong> ${data.quantity || 0}
                            </div>
                            <div class="col-md-6">
                                <strong>Tồn trước:</strong> ${data.quantityBefore || 0}<br>
                                <strong>Tồn sau:</strong> ${data.quantityAfter || 0}<br>
                                <strong>Đơn giá:</strong> ${(data.unitPrice || 0).toLocaleString()} VNĐ<br>
                                <strong>Thành tiền:</strong> ${(data.totalAmount || 0).toLocaleString()} VNĐ
                            </div>
                        </div>
                        <div class="row mt-3">
                            <div class="col-12">
                                <strong>Ngày giao dịch:</strong> ${new Date(data.transactionDate).toLocaleString('vi-VN')}<br>
                                <strong>Ghi chú:</strong> ${data.notes || 'Không có'}
                            </div>
                        </div>
                    `;
                    
                    Swal.fire({
                        title: 'Chi tiết giao dịch kho',
                        html: html,
                        icon: 'info',
                        confirmButtonText: 'Đóng',
                        width: '600px'
                    });
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.message,
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler && AuthHandler.isUnauthorized && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: 'Có lỗi xảy ra khi tải dữ liệu',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            }
        });
    },

    // Download template
    downloadTemplate: function() {
        window.location.href = '/StockManagement/DownloadTemplate';
    },

    // Validate Excel file
    validateExcelFile: function() {
        var fileInput = document.getElementById('excelFile');
        var file = fileInput.files[0];
        
        if (!file) {
            Swal.fire({
                title: 'Lỗi!',
                text: 'Vui lòng chọn file Excel để validate',
                icon: 'warning',
                confirmButtonText: 'OK'
            });
            return;
        }

        var formData = new FormData();
        formData.append('file', file);

        $.ajax({
            url: '/StockManagement/ValidateExcel',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                if (response.success) {
                    $('#validationResults').show();
                    $('#validationContent').html('<div class="alert alert-success">File hợp lệ và sẵn sàng import!</div>');
                } else {
                    $('#validationResults').show();
                    $('#validationContent').html('<div class="alert alert-danger">' + response.message + '</div>');
                }
            },
            error: function() {
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra khi validate file',
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        });
    },

    // Import opening balance
    importOpeningBalance: function() {
        var fileInput = document.getElementById('excelFile');
        var file = fileInput.files[0];
        
        if (!file) {
            Swal.fire({
                title: 'Lỗi!',
                text: 'Vui lòng chọn file Excel để import',
                icon: 'warning',
                confirmButtonText: 'OK'
            });
            return;
        }

        var formData = new FormData();
        formData.append('file', file);

        // Show progress
        $('#importProgress').show();
        $('#importProgress .progress-bar').css('width', '0%');

        $.ajax({
            url: '/StockManagement/ImportExcel',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            xhr: function() {
                var xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress", function(evt) {
                    if (evt.lengthComputable) {
                        var percentComplete = evt.loaded / evt.total * 100;
                        $('#importProgress .progress-bar').css('width', percentComplete + '%');
                    }
                }, false);
                return xhr;
            },
            success: function(response) {
                $('#importProgress').hide();
                if (response.success) {
                    Swal.fire({
                        title: 'Thành công!',
                        text: response.message,
                        icon: 'success',
                        confirmButtonText: 'OK'
                    }).then(() => {
                        $('#importModal').modal('hide');
                        self.stockTable.ajax.reload();
                        $('#excelFile').val('');
                        $('#validationResults').hide();
                    });
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.message,
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function() {
                $('#importProgress').hide();
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra khi import dữ liệu',
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        });
    },

    // ✅ THÊM: Add item row to the table
    addItemRow: function() {
        var template = document.getElementById('itemRowTemplate');
        var clone = template.content.cloneNode(true);
        var itemIndex = $('#itemsTableBody tr').length;
        
        // Update index and name attributes
        clone.querySelector('.item-index').textContent = itemIndex + 1;
        clone.querySelector('.item-part-typeahead').name = `Items[${itemIndex}].PartName`;
        clone.querySelector('.item-part-id').name = `Items[${itemIndex}].PartId`;
        clone.querySelector('.item-quantity').name = `Items[${itemIndex}].Quantity`;
        clone.querySelector('.item-unit-price').name = `Items[${itemIndex}].UnitPrice`;
        clone.querySelector('.item-has-invoice').name = `Items[${itemIndex}].HasInvoice`;
        
        // Add to table
        $('#itemsTableBody').append(clone);
        
        // ✅ SỬA: Initialize Typeahead cho input mới (sau khi đã append vào DOM)
        var newInput = $('#itemsTableBody tr:last .item-part-typeahead');
        var hiddenInput = $('#itemsTableBody tr:last .item-part-id');
        if (newInput.length > 0) {
            this.initializeTypeahead(newInput, hiddenInput);
            newInput.focus();
        }
        
        // Update remove button visibility
        this.updateRemoveButtons();
    },

    // ✅ THÊM: Remove item row from the table
    removeItemRow: function(button) {
        var row = button.closest('tr');
        row.remove();
        
        // Update indices
        this.updateRowIndices();
        this.updateRemoveButtons();
        this.calculateGrandTotal();
    },

    // ✅ THÊM: Update row indices after removal
    updateRowIndices: function() {
        $('#itemsTableBody tr').each(function(index) {
            $(this).find('.item-index').text(index + 1);
            $(this).find('.item-part-typeahead').attr('name', `Items[${index}].PartName`);
            $(this).find('.item-part-id').attr('name', `Items[${index}].PartId`);
            $(this).find('.item-quantity').attr('name', `Items[${index}].Quantity`);
            $(this).find('.item-unit-price').attr('name', `Items[${index}].UnitPrice`);
            $(this).find('.item-has-invoice').attr('name', `Items[${index}].HasInvoice`);
        });
    },

    // ✅ THÊM: Update remove button visibility
    updateRemoveButtons: function() {
        var rowCount = $('#itemsTableBody tr').length;
        $('.remove-item-btn').toggle(rowCount > 1);
    },

    // ✅ THÊM: Calculate individual item total
    calculateItemTotal: function(input) {
        var row = input.closest('tr');
        var quantity = parseFloat(row.find('.item-quantity').val()) || 0;
        var unitPrice = parseFloat(row.find('.item-unit-price').val()) || 0;
        var total = quantity * unitPrice;
        
        row.find('.item-total').text(total.toLocaleString('vi-VN') + ' VNĐ');
    },

    // ✅ THÊM: Calculate grand total
    calculateGrandTotal: function() {
        var grandTotal = 0;
        $('.item-total').each(function() {
            var text = $(this).text().replace(/[^\d]/g, '');
            grandTotal += parseFloat(text) || 0;
        });
        
        $('#totalAmount').text(grandTotal.toLocaleString('vi-VN') + ' VNĐ');
    },

    // ✅ THÊM: Initialize Typeahead cho tìm kiếm phụ tùng (sử dụng Bootstrap Typeahead)
    initializeTypeahead: function(inputElement, hiddenElement) {
        var self = this;
        
        inputElement.typeahead({
            source: function(query, process) {
                $.ajax({
                    url: '/StockManagement/SearchParts',
                    type: 'GET',
                    data: { q: query },
                    success: function(response) {
                        if (response && response.success && Array.isArray(response.data)) {
                            var parts = response.data.map(function(part) {
                                return {
                                    id: part.id,
                                    name: part.text
                                };
                            });
                            process(parts);
                        } else {
                            process([]);
                        }
                    },
                    error: function(xhr, status, error) {
                        console.error('Error searching parts:', error);
                        process([]);
                    }
                });
            },
            displayText: function(item) {
                return item.name;
            },
            afterSelect: function(item) {
                // Set hidden input value
                hiddenElement.val(item.id);
                // Set input value
                inputElement.val(item.name);
            },
            delay: 300
        });
    },


    // ✅ THÊM: Create multiple stock transactions (Purchase Order)
    createStockTransaction: function() {
        var self = this;
        
        // Validate form
        if (!$('#createTransactionForm')[0].checkValidity()) {
            $('#createTransactionForm')[0].reportValidity();
            return;
        }
        
        // Check if at least one item is added
        if ($('#itemsTableBody tr').length === 0) {
            Swal.fire({
                title: 'Cảnh báo!',
                text: 'Vui lòng thêm ít nhất một phụ tùng vào đơn hàng',
                icon: 'warning',
                confirmButtonText: 'OK'
            });
            return;
        }
        
        // Collect form data
        var formData = new FormData($('#createTransactionForm')[0]);
        var transactionData = {
            TransactionType: $('#transactionType').val(),
            SupplierId: $('#supplierId').val(),
            ReferenceNumber: $('#referenceNumber').val(),
            TransactionDate: $('#transactionDate').val(),
            Notes: $('#notes').val(),
            Items: []
        };
        
        // Collect items data with validation
        $('#itemsTableBody tr').each(function(index) {
            var row = $(this);
            var partId = row.find('.item-part-id').val();
            var partName = row.find('.item-part-typeahead').val();
            var quantity = parseInt(row.find('.item-quantity').val());
            var unitPrice = parseFloat(row.find('.item-unit-price').val());
            
            // ✅ TỐI ƯU: Validation từng field
            if (!partId || !partName) {
                Swal.fire({
                    title: 'Lỗi!',
                    text: `Vui lòng chọn phụ tùng cho dòng ${index + 1}`,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
                return false;
            }
            
            if (!quantity || quantity <= 0) {
                Swal.fire({
                    title: 'Lỗi!',
                    text: `Vui lòng nhập số lượng hợp lệ cho dòng ${index + 1}`,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
                return false;
            }
            
            if (!unitPrice || unitPrice <= 0) {
                Swal.fire({
                    title: 'Lỗi!',
                    text: `Vui lòng nhập đơn giá hợp lệ cho dòng ${index + 1}`,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
                return false;
            }
            
            var item = {
                PartId: parseInt(partId),
                Quantity: quantity,
                UnitPrice: unitPrice,
                HasInvoice: row.find('.item-has-invoice').is(':checked')
            };
            
            transactionData.Items.push(item);
        });
        
        // Validate items
        if (transactionData.Items.length === 0) {
            Swal.fire({
                title: 'Cảnh báo!',
                text: 'Vui lòng kiểm tra lại thông tin các phụ tùng',
                icon: 'warning',
                confirmButtonText: 'OK'
            });
            return;
        }
        
        // ✅ TỐI ƯU: Show loading với progress
        Swal.fire({
            title: 'Đang xử lý...',
            text: `Đang tạo đơn nhập hàng với ${transactionData.Items.length} phụ tùng`,
            allowOutsideClick: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
        
        // Send request
        $.ajax({
            url: '/StockManagement/CreatePurchaseOrder',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(transactionData),
            success: function(response) {
                Swal.close();
                if (response.success) {
                    Swal.fire({
                        title: 'Thành công!',
                        text: `Đã tạo đơn nhập hàng với ${transactionData.Items.length} phụ tùng`,
                        icon: 'success',
                        confirmButtonText: 'OK'
                    }).then(() => {
                        $('#createTransactionModal').modal('hide');
                        self.stockTable.ajax.reload();
                        self.resetForm();
                    });
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.message || 'Có lỗi xảy ra khi tạo đơn hàng',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function() {
                Swal.close();
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra khi tạo đơn hàng',
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        });
    },

    // ✅ THÊM: Reset form
    resetForm: function() {
        $('#createTransactionForm')[0].reset();
        $('#itemsTableBody').empty();
        $('#totalAmount').text('0 VNĐ');
    }
};

// Initialize when document is ready
$(document).ready(function() {
    StockManagement.init();
});
