/**
 * Purchase Order Management Module
 * 
 * Handles all purchase order-related operations
 * View, confirm receipt, print purchase orders
 */

window.PurchaseOrderManagement = {
    // DataTable instance
    purchaseOrderTable: null,
    currentEditData: null, // ✅ THÊM: Store data for edit modal
    currentReferenceNumber: null,
    itemCounter: 0, // Counter for dynamic item rows
    allParts: [], // Cache all parts for dropdown

    // Initialize module
    init: function() {
        // Guard clause to prevent duplicate initialization
        if (this.initialized) {
            return;
        }
        
        this.initDataTable();
        this.bindEvents();
        this.loadSuppliers();
        this.loadParts();
        
        this.initialized = true;
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        // Sử dụng DataTablesUtility với style chung
        var columns = [
            { data: 'orderNumber', title: 'Số Phiếu', width: '15%' },
            { 
                data: 'orderDate', 
                title: 'Ngày Tạo', 
                width: '12%',
                render: DataTablesUtility.renderDate
            },
            { data: 'supplierName', title: 'Nhà Cung Cấp', width: '20%' },
            { data: 'itemCount', title: 'Số Loại', width: '10%', className: 'text-center' },
            { 
                data: 'totalAmount', 
                title: 'Tổng Tiền', 
                width: '15%',
                className: 'text-right',
                render: DataTablesUtility.renderCurrency
            },
            { 
                data: 'status', 
                title: 'Trạng Thái', 
                width: '12%',
                render: DataTablesUtility.renderStatus
            },
            {
                data: null,
                title: 'Thao Tác',
                width: '20%',
                orderable: false,
                searchable: false,
                className: 'text-center',
                render: function(data, type, row) {
                    var buttons = '';
                    
                    // Always show view button
                    buttons += `<button class="btn btn-info btn-sm view-purchase-order" data-ref="${row.orderNumber}" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                    </button> `;
                    
                    // Always show print button
                    buttons += `<button class="btn btn-secondary btn-sm print-purchase-order" data-ref="${row.orderNumber}" title="In phiếu">
                            <i class="fas fa-print"></i>
                    </button> `;
                    
                    // Dynamic buttons based on status
                    if (row.status === 'Draft') {
                        buttons += `<button class="btn btn-primary btn-sm edit-po-btn" data-id="${row.id}" title="Chỉnh sửa">
                            <i class="fas fa-edit"></i>
                        </button> `;
                        buttons += `<button class="btn btn-success btn-sm send-po-btn" data-id="${row.id}" title="Gửi PO">
                            <i class="fas fa-paper-plane"></i>
                        </button> `;
                        buttons += `<button class="btn btn-danger btn-sm cancel-po-btn" data-id="${row.id}" title="Hủy PO">
                            <i class="fas fa-ban"></i>
                        </button>`;
                    } else if (row.status === 'Sent') {
                        buttons += `<button class="btn btn-warning btn-sm cancel-po-btn" data-id="${row.id}" title="Hủy PO">
                            <i class="fas fa-ban"></i>
                        </button> `;
                        buttons += `<button class="btn btn-success btn-sm confirm-receipt" data-ref="${row.orderNumber}" title="Xác nhận nhập kho">
                            <i class="fas fa-check"></i>
                        </button>`;
                    } else if (row.status === 'Received') {
                        buttons += `<span class="badge badge-success">Đã nhận</span>`;
                    } else if (row.status === 'Cancelled') {
                        buttons += `<span class="badge badge-danger">Đã hủy</span>`;
                    }
                    
                    return buttons;
                }
            }
        ];

        this.purchaseOrderTable = DataTablesUtility.initServerSideTable('#purchaseOrdersTable', '/PurchaseOrder/GetPurchaseOrders', columns, {
            order: [[1, 'desc']], // Sort by date desc
            pageLength: 10
        });
    },

    // Bind events
    bindEvents: function() {
        var self = this;

        // Guard clause to prevent duplicate event binding
        if (this.eventsBound) {
            return;
        }

        // Add purchase order button - FOLLOW EMPLOYEE PATTERN
        $(document).on('click', '#addPurchaseOrderBtn', function() {
            self.showCreateModal();
        });
        
        // Add purchase order item button
        $(document).on('click', '#addPurchaseOrderItemBtn', function() {
            self.addPurchaseOrderItem();
        });
        
        // Remove item button
        $(document).on('click', '.remove-item-btn', function() {
            $(this).closest('tr').remove();
            
            // Show empty message if no items
            if ($('#purchaseOrderItemsBody tr').length === 0) {
                $('#purchaseOrderItemsBody').html(`
                    <tr>
                        <td colspan="7" class="text-center text-muted">
                            <i class="fas fa-info-circle"></i> Chưa có phụ tùng nào. Nhấn "Thêm Phụ Tùng" để bắt đầu.
                        </td>
                    </tr>
                `);
            }
            
            self.updateTotalAmount();
        });
        
        // Auto-fill price when part selected - REMOVED: No longer needed with typeahead
        
        // Update total when quantity or price changes
        $(document).on('input', '.item-quantity, .item-unit-price', function() {
            self.updateTotalAmount();
        });
        
        // Update total when VAT rate changes
        $(document).on('change', '#createVATRate', function() {
            self.updateTotalAmount();
        });
        
        // Submit create purchase order form
        $('#createPurchaseOrderForm').on('submit', function(e) {
            e.preventDefault();
            self.createPurchaseOrder();
        });

        // View purchase order details
        $(document).on('click', '.view-purchase-order', function() {
            var refNumber = $(this).data('ref');
            self.viewPurchaseOrderDetails(refNumber);
        });

        // Print purchase order
        $(document).on('click', '.print-purchase-order', function() {
            var refNumber = $(this).data('ref');
            self.printPurchaseOrder(refNumber);
        });

        // Confirm receipt
        $(document).on('click', '.confirm-receipt', function() {
            var refNumber = $(this).data('ref');
            self.showConfirmReceiptModal(refNumber);
        });

        // Confirm receipt form
        $(document).on('submit', '#confirmReceiptForm', function(e) {
            e.preventDefault();
            self.confirmReceipt();
        });

        // Print button in modal
        $(document).on('click', '#printPurchaseOrderBtn', function() {
            if (self.currentReferenceNumber) {
                self.printPurchaseOrder(self.currentReferenceNumber);
            }
        });

        // Confirm receipt button in modal
        $(document).on('click', '#confirmReceiptBtn', function() {
            if (self.currentReferenceNumber) {
                self.showConfirmReceiptModal(self.currentReferenceNumber);
            }
        });

        // Send PO button
        $(document).on('click', '.send-po-btn', function() {
            var poId = $(this).data('id');
            self.sendPurchaseOrder(poId);
        });

        // Cancel PO button
        $(document).on('click', '.cancel-po-btn', function() {
            var poId = $(this).data('id');
            self.showCancelModal(poId);
        });

        // Edit PO button
        $(document).on('click', '.edit-po-btn', function() {
            var poId = $(this).data('id');
            self.editPurchaseOrder(poId);
        });
        
        // Mark events as bound
        this.eventsBound = true;
    },

    // View purchase order details
    viewPurchaseOrderDetails: function(referenceNumber) {
        var self = this;
        self.currentReferenceNumber = referenceNumber;

        $.ajax({
            url: '/PurchaseOrder/GetPurchaseOrder/' + encodeURIComponent(referenceNumber),
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var data = response.data;
                    var html = `
                        <div class="row">
                            <div class="col-md-6">
                                <h5><strong>Thông Tin Phiếu Nhập</strong></h5>
                                <table class="table table-sm">
                                    <tr><td><strong>Số phiếu:</strong></td><td>${data.referenceNumber}</td></tr>
                                    <tr><td><strong>Ngày tạo:</strong></td><td>${new Date(data.transactionDate).toLocaleDateString('vi-VN')}</td></tr>
                                    <tr><td><strong>Nhà cung cấp:</strong></td><td>${data.supplierName}</td></tr>
                                    <tr><td><strong>Số loại:</strong></td><td>${data.itemCount}</td></tr>
                                    <tr><td><strong>Tổng tiền:</strong></td><td>${(data.totalAmount || 0).toLocaleString('vi-VN')} VNĐ</td></tr>
                                </table>
                            </div>
                            <div class="col-md-6">
                                <h5><strong>Chi Tiết Hàng Hóa</strong></h5>
                                <div class="table-responsive">
                                    <table class="table table-sm table-bordered">
                                        <thead>
                                            <tr>
                                                <th>Phụ Tùng</th>
                                                <th>Số Lượng</th>
                                                <th>Đơn Giá</th>
                                                <th>Thành Tiền</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                    `;
                    
                    data.items.forEach(function(item) {
                        html += `
                            <tr>
                                <td>${item.partName || 'N/A'}</td>
                                <td class="text-center">${item.quantity || 0}</td>
                                <td class="text-right">${(item.unitPrice || 0).toLocaleString('vi-VN')} VNĐ</td>
                                <td class="text-right">${(item.totalAmount || 0).toLocaleString('vi-VN')} VNĐ</td>
                            </tr>
                        `;
                    });
                    
                    html += `
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        </div>
                    `;
                    
                    $('#purchaseOrderDetailsContent').html(html);
                    $('#viewPurchaseOrderModal').modal('show');
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.error || 'Không thể tải chi tiết phiếu nhập',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function(xhr, status, error) {
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra khi tải chi tiết phiếu nhập: ' + error,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        });
    },

    // Print purchase order
    printPurchaseOrder: function(referenceNumber) {
        var printWindow = window.open('/PurchaseOrder/PrintPurchaseOrder/' + encodeURIComponent(referenceNumber), '_blank');
        if (printWindow) {
            printWindow.focus();
        } else {
            Swal.fire({
                title: 'Lỗi!',
                text: 'Không thể mở cửa sổ in. Vui lòng kiểm tra popup blocker.',
                icon: 'error',
                confirmButtonText: 'OK'
            });
        }
    },

    // Show confirm receipt modal
    showConfirmReceiptModal: function(referenceNumber) {
        var self = this;
        self.currentReferenceNumber = referenceNumber;

        // Load purchase order details
        $.ajax({
            url: '/PurchaseOrder/GetPurchaseOrder/' + encodeURIComponent(referenceNumber),
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var data = response.data;
                    var html = '';
                    
                    data.items.forEach(function(item) {
                        html += `
                            <tr>
                                <td>${item.partName}</td>
                                <td class="text-center">${item.quantity}</td>
                                <td class="text-right">${item.unitPrice.toLocaleString('vi-VN')} VNĐ</td>
                                <td class="text-right">${item.totalAmount.toLocaleString('vi-VN')} VNĐ</td>
                                <td class="text-center">
                                    <input type="number" class="form-control form-control-sm actual-quantity" 
                                           value="${item.quantity}" min="0" max="${item.quantity}" 
                                           data-transaction-id="${item.id}" required>
                                </td>
                                <td>
                                    <input type="text" class="form-control form-control-sm item-notes" 
                                           data-transaction-id="${item.id}" placeholder="Ghi chú...">
                                </td>
                            </tr>
                        `;
                    });
                    
                    $('#receiptItemsBody').html(html);
                    $('#confirmReceiptModal').modal('show');
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.error || 'Không thể tải chi tiết phiếu nhập',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function(xhr, status, error) {
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra khi tải chi tiết phiếu nhập: ' + error,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        });
    },

    // Confirm receipt
    confirmReceipt: function() {
        var self = this;
        var items = [];
        
        $('.actual-quantity').each(function() {
            var transactionId = $(this).data('transaction-id');
            var actualQuantity = parseInt($(this).val());
            var notes = $('.item-notes[data-transaction-id="' + transactionId + '"]').val();
            
            items.push({
                StockTransactionId: transactionId,
                ActualQuantity: actualQuantity,
                Notes: notes
            });
        });

        var formData = {
            Items: items,
            Notes: $('#receiptNotes').val()
        };

        $.ajax({
            url: '/PurchaseOrder/ConfirmReceipt/' + encodeURIComponent(self.currentReferenceNumber),
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    Swal.fire({
                        title: 'Thành công!',
                        text: response.message || 'Đã xác nhận nhập kho thành công',
                        icon: 'success',
                        confirmButtonText: 'OK'
                    }).then(() => {
                        $('#confirmReceiptModal').modal('hide');
                        self.purchaseOrderTable.ajax.reload();
                    });
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.error || 'Có lỗi xảy ra khi xác nhận nhập kho',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function(xhr, status, error) {
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra khi xác nhận nhập kho: ' + error,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        });
    },

    // Load suppliers for dropdown
    loadSuppliers: function() {
        var self = this;
        $.ajax({
            url: '/SupplierManagement/GetSuppliers?pageNumber=1&pageSize=1000',
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var $select = $('#createSupplierId');
                    $select.empty().append('<option value="">-- Chọn Nhà Cung Cấp --</option>');
                    
                    response.data.forEach(function(supplier) {
                        $select.append(`<option value="${supplier.id}">${supplier.supplierName}</option>`);
                    });
                }
            }
        });
    },

    // Load parts for dropdown
    loadParts: function() {
        var self = this;
        $.ajax({
            url: '/PartsManagement/GetParts?pageNumber=1&pageSize=10000',
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    self.allParts = response.data;
                }
            }
        });
    },

    // Show create purchase order modal - FOLLOW EMPLOYEE PATTERN
    showCreateModal: function() {
        var self = this;
        $('#createPurchaseOrderModal').modal('show');
        
        // Reset form when modal is fully shown
        $('#createPurchaseOrderModal').on('shown.bs.modal', function() {
            // Reset form fields
            $('#createPurchaseOrderForm')[0].reset();
            $('.text-danger').text('');
            
            // Set default date to today
            var today = new Date().toISOString().split('T')[0];
            $('#createOrderDate').val(today);
            
            // Clear items table
            self.itemCounter = 0;
            $('#purchaseOrderItemsBody').html(`
                <tr>
                    <td colspan="7" class="text-center text-muted">
                        <i class="fas fa-info-circle"></i> Chưa có phụ tùng nào. Nhấn "Thêm Phụ Tùng" để bắt đầu.
                    </td>
                </tr>
            `);
            self.updateTotalAmount();
            
            // Remove the event listener to prevent multiple bindings
            $('#createPurchaseOrderModal').off('shown.bs.modal');
        });
    },

    // Add purchase order item row
    addPurchaseOrderItem: function() {
        var self = this;
        self.itemCounter++;
        
        var row = `
            <tr data-item-id="${self.itemCounter}">
                <td class="text-center">${self.itemCounter}</td>
                <td>
                    <input type="text" class="form-control form-control-sm part-typeahead" placeholder="Tìm kiếm phụ tùng..." required>
                    <input type="hidden" class="part-id-input" value="">
                </td>
                <td>
                    <input type="number" class="form-control form-control-sm item-quantity" min="1" value="1" required>
                </td>
                <td>
                    <input type="number" class="form-control form-control-sm item-unit-price" min="0" step="0.01" required>
                </td>
                <td class="text-right item-subtotal">0 VNĐ</td>
                <td>
                    <input type="text" class="form-control form-control-sm item-notes" placeholder="Ghi chú...">
                </td>
                <td class="text-center">
                    <button type="button" class="btn btn-danger btn-sm remove-item-btn">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
        
        // Remove empty message if exists
        if ($('#purchaseOrderItemsBody tr td').length === 1) {
            $('#purchaseOrderItemsBody').empty();
        }
        
        $('#purchaseOrderItemsBody').append(row);
        
        // Initialize typeahead for the new part input
        self.initializePartTypeahead($('#purchaseOrderItemsBody .part-typeahead').last());
    },

    // Initialize part typeahead
    initializePartTypeahead: function(input) {
        var self = this;
        
        input.typeahead({
            source: function(query, process) {
                $.ajax({
                    url: '/StockManagement/SearchParts',
                    type: 'GET',
                    data: { q: query },
                    success: function(response) {
                        if (response.success && response.data && Array.isArray(response.data)) {
                            var parts = response.data.map(function(part) {
                                return {
                                    id: part.id,
                                    name: part.text,
                                    costPrice: part.costPrice || 0,
                                    sellPrice: part.sellPrice || 0
                                };
                            });
                            process(parts);
                        } else {
                            process([]);
                        }
                    },
                    error: function(xhr, status, error) {
                        process([]);
                    }
                });
            },
            displayText: function(item) {
                return item.name;
            },
            afterSelect: function(item) {
                var row = input.closest('tr');
                
                // Set hidden input with part ID
                row.find('.part-id-input').val(item.id);
                
                // Auto-fill cost price
                row.find('.item-unit-price').val(item.costPrice || 0);
                
                // Update subtotal
                self.updateTotalAmount();
                
                // Set input value
                input.val(item.name);
            },
            delay: 300,
        });
    },

    // Calculate and update total amount with VAT
    updateTotalAmount: function() {
        var subtotal = 0;
        var vatRate = parseFloat($('#createVATRate').val()) || 0;
        
        // Calculate subtotal
        $('#purchaseOrderItemsBody tr').each(function() {
            var quantity = parseFloat($(this).find('.item-quantity').val()) || 0;
            var unitPrice = parseFloat($(this).find('.item-unit-price').val()) || 0;
            var itemSubtotal = quantity * unitPrice;
            
            $(this).find('.item-subtotal').text(itemSubtotal.toLocaleString('vi-VN') + ' VNĐ');
            subtotal += itemSubtotal;
        });
        
        // Calculate VAT amount
        var vatAmount = subtotal * (vatRate / 100);
        var totalAmount = subtotal + vatAmount;
        
        // Update display
        $('#subTotalAmount').text(subtotal.toLocaleString('vi-VN') + ' VNĐ');
        $('#vatAmount').text(vatAmount.toLocaleString('vi-VN') + ' VNĐ');
        $('#totalAmount').text(totalAmount.toLocaleString('vi-VN') + ' VNĐ');
        $('#vatRateDisplay').text(vatRate);
    },

    // Create purchase order
    createPurchaseOrder: function() {
        var self = this;
        
        // Validate items
        if ($('#purchaseOrderItemsBody tr').length === 0 || $('#purchaseOrderItemsBody tr td').length === 1) {
            Swal.fire({
                title: 'Cảnh báo!',
                text: 'Vui lòng thêm ít nhất một phụ tùng',
                icon: 'warning',
                confirmButtonText: 'OK'
            });
            return;
        }
        
        // Collect items
        var items = [];
        var isValid = true;
        
        $('#purchaseOrderItemsBody tr').each(function() {
            var partId = $(this).find('.part-id-input').val();
            var quantity = parseFloat($(this).find('.item-quantity').val());
            var unitPrice = parseFloat($(this).find('.item-unit-price').val());
            var notes = $(this).find('.item-notes').val();
            
            if (!partId || !quantity || !unitPrice) {
                isValid = false;
                return false;
            }
            
            items.push({
                PartId: parseInt(partId),
                QuantityOrdered: quantity,
                UnitPrice: unitPrice,
                Notes: notes
            });
        });
        
        if (!isValid) {
            Swal.fire({
                title: 'Cảnh báo!',
                text: 'Vui lòng điền đầy đủ thông tin cho tất cả phụ tùng',
                icon: 'warning',
                confirmButtonText: 'OK'
            });
            return;
        }
        
        var formData = {
            SupplierId: parseInt($('#createSupplierId').val()),
            OrderDate: $('#createOrderDate').val(),
            ExpectedDeliveryDate: $('#createExpectedDeliveryDate').val() || null,
            PaymentTerms: $('#createPaymentTerms').val(),
            DeliveryAddress: $('#createDeliveryAddress').val(),
            VATRate: parseFloat($('#createVATRate').val()) || 0,
            Notes: $('#createNotes').val(),
            Items: items
        };
        
        console.log('Creating Purchase Order:', formData);
        
        $.ajax({
            url: '/PurchaseOrder/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    Swal.fire({
                        title: 'Thành công!',
                        text: response.message || 'Đã tạo phiếu nhập hàng thành công',
                        icon: 'success',
                        confirmButtonText: 'OK'
                    }).then(() => {
                        $('#createPurchaseOrderModal').modal('hide');
                        self.purchaseOrderTable.ajax.reload();
                    });
                } else {
                    // Handle validation errors
                    if (response.errors) {
                        self.displayValidationErrors(response.errors);
                    } else {
                        Swal.fire({
                            title: 'Lỗi!',
                            text: response.error || 'Có lỗi xảy ra khi tạo phiếu nhập hàng',
                            icon: 'error',
                            confirmButtonText: 'OK'
                        });
                    }
                }
            },
            error: function(xhr, status, error) {
                console.error('Create PO Error:', xhr.responseText);
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra khi tạo phiếu nhập hàng: ' + error,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        });
    },

    // Display validation errors
    displayValidationErrors: function(errors) {
        // Clear previous errors
        $('.text-danger').text('');
        
        // Display new errors
        for (var field in errors) {
            var errorMessages = errors[field];
            var errorText = Array.isArray(errorMessages) ? errorMessages.join(', ') : errorMessages;
            $('#create' + field + '-error').text(errorText);
        }
        
        Swal.fire({
            title: 'Lỗi!',
            text: 'Vui lòng kiểm tra lại thông tin đã nhập',
            icon: 'error',
            confirmButtonText: 'OK'
        });
    },

    // Send Purchase Order
    sendPurchaseOrder: function(poId) {
        var self = this;
        
        Swal.fire({
            title: 'Xác nhận gửi PO',
            text: 'Bạn có chắc chắn muốn gửi PO này cho supplier?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Có, gửi',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/api/PurchaseOrders/' + poId + '/send',
                    type: 'PUT',
                    contentType: 'application/json',
                    success: function(response) {
                        if (response.success) {
                            Swal.fire({
                                title: 'Thành công!',
                                text: response.message || 'Đã gửi PO thành công',
                                icon: 'success',
                                confirmButtonText: 'OK'
                            }).then(() => {
                                self.purchaseOrderTable.ajax.reload();
                            });
                        } else {
                            Swal.fire({
                                title: 'Lỗi!',
                                text: response.message || 'Có lỗi xảy ra',
                                icon: 'error',
                                confirmButtonText: 'OK'
                            });
                        }
                    },
                    error: function(xhr, status, error) {
                        Swal.fire({
                            title: 'Lỗi!',
                            text: 'Có lỗi xảy ra khi gửi PO: ' + error,
                            icon: 'error',
                            confirmButtonText: 'OK'
                        });
                    }
                });
            }
        });
    },

    // Show Cancel Modal
    showCancelModal: function(poId) {
        var self = this;
        
        Swal.fire({
            title: 'Hủy Purchase Order',
            input: 'textarea',
            inputLabel: 'Lý do hủy',
            inputPlaceholder: 'Nhập lý do hủy PO...',
            inputValidator: (value) => {
                if (!value) {
                    return 'Vui lòng nhập lý do hủy!';
                }
            },
            showCancelButton: true,
            confirmButtonText: 'Hủy PO',
            cancelButtonText: 'Đóng',
            confirmButtonColor: '#dc3545'
        }).then((result) => {
            if (result.isConfirmed) {
                self.cancelPurchaseOrder(poId, result.value);
            }
        });
    },

    // Cancel Purchase Order
    cancelPurchaseOrder: function(poId, reason) {
        var self = this;
        
        $.ajax({
            url: '/api/PurchaseOrders/' + poId + '/cancel',
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ Reason: reason }),
            success: function(response) {
                if (response.success) {
                    Swal.fire({
                        title: 'Thành công!',
                        text: response.message || 'Đã hủy PO thành công',
                        icon: 'success',
                        confirmButtonText: 'OK'
                    }).then(() => {
                        self.purchaseOrderTable.ajax.reload();
                    });
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.message || 'Có lỗi xảy ra',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function(xhr, status, error) {
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra khi hủy PO: ' + error,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        });
    },

    // Edit Purchase Order (placeholder - cần implement modal edit)
    editPurchaseOrder: function(poId) {
        Swal.fire({
            title: 'Chức năng đang phát triển',
            text: 'Chức năng chỉnh sửa PO sẽ được implement trong phiên bản tiếp theo',
            icon: 'info',
            confirmButtonText: 'OK'
        });
    }
};

// Initialize when document is ready
$(document).ready(function() {
    // Guard clause to prevent duplicate initialization
    if (window.PurchaseOrderManagement && window.PurchaseOrderManagement.initialized) {
        return;
    }
    
    PurchaseOrderManagement.init();
    window.PurchaseOrderManagement.initialized = true;
});
