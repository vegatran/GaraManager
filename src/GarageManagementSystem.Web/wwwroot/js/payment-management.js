// Payment Management Module
window.PaymentManagement = {
    paymentTable: null,

    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.loadServiceOrders();
    },

    initDataTable: function() {
        var self = this;
        
        var columns = [
                { data: 'id', title: 'ID', width: '5%' },
                { data: 'paymentNumber', title: 'Số Biên Nhận', width: '12%' },
                { data: 'invoiceNumber', title: 'Mã Đơn Hàng', width: '10%' },
                { 
                    data: 'amount', 
                    title: 'Số Tiền', 
                    width: '12%',
                    className: 'text-right',
                    render: function(data, type, row) {
                        if (type === 'display') {
                            var amount = parseFloat(data) || 0;
                            return amount.toLocaleString('vi-VN') + ' VNĐ';
                        }
                        return data;
                    }
                },
                { data: 'paymentMethod', title: 'Phương Thức', width: '12%' },
                { data: 'paymentDate', title: 'Ngày Thanh Toán', width: '12%' },
                { 
                    data: 'status', 
                    title: 'Trạng Thái', 
                    width: '10%',
                    render: function(data) {
                        var badgeClass = data === 'Refund' ? 'badge-danger' : 'badge-success';
                        return `<span class="badge ${badgeClass}">${data}</span>`;
                    }
                },
                { data: 'referenceNumber', title: 'Mã Tham Chiếu', width: '12%' },
                {
                    data: null,
                    title: 'Thao Tác',
                    width: '10%',
                    orderable: false,
                    render: function(data, type, row) {
                        return `
                            <button class="btn btn-info btn-sm view-payment" data-id="${row.id}" title="Xem chi tiết">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-danger btn-sm delete-payment" data-id="${row.id}" title="Xóa">
                                <i class="fas fa-trash"></i>
                            </button>
                        `;
                    }
                }
            ];
        
        this.paymentTable = DataTablesUtility.initServerSideTable(
            '#paymentTable',
            '/PaymentManagement/GetPayments',
            columns,
            {
                order: [[0, 'desc']],
                pageLength: 10
            }
        );
    },

    bindEvents: function() {
        var self = this;

        // Unbind existing events to prevent duplicates
        $('#createPaymentForm').off('submit');
        $(document).off('click', '.view-payment');
        $(document).off('click', '.delete-payment');
        $('#searchInput').off('keyup');

        // Create payment
        $('#createPaymentForm').on('submit', function(e) {
            e.preventDefault();
            self.createPayment();
        });

        // View payment
        $(document).on('click', '.view-payment', function() {
            var paymentId = $(this).data('id');
            self.viewPayment(paymentId);
        });

        // Delete payment
        $(document).on('click', '.delete-payment', function() {
            var paymentId = $(this).data('id');
            self.deletePayment(paymentId);
        });

        // Search (client-side - server-side search is handled by DataTables)
        $('#searchInput').on('keyup', function() {
            if (self.paymentTable) {
                self.paymentTable.search(this.value).draw();
            }
        });
    },

    loadServiceOrders: function() {
        $.ajax({
            url: '/OrderManagement/GetOrders',
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var options = '<option value="">Chọn đơn hàng</option>';
                    response.data.forEach(function(order) {
                        options += `<option value="${order.id}">${order.orderNumber} - ${order.customerName} - ${order.finalAmount} VNĐ</option>`;
                    });
                    $('#createServiceOrderId').html(options);
                }
            },
            error: function(xhr) {
            }
        });
    },

    createPayment: function() {
        var formData = {
            ServiceOrderId: parseInt($('#createServiceOrderId').val()),
            Amount: parseFloat($('#createAmount').val()) || 0,
            PaymentMethod: $('#createPaymentMethod').val(),
            TransactionReference: $('#createTransactionReference').val(),
            CardType: $('#createCardType').val(),
            CardLastFourDigits: $('#createCardLastFourDigits').val(),
            Notes: $('#createNotes').val(),
            IsRefund: $('#createIsRefund').is(':checked'),
            RefundReason: $('#createRefundReason').val()
        };

        // Validate required fields
        if (!formData.ServiceOrderId || formData.Amount <= 0) {
            GarageApp.showError('Vui lòng điền đầy đủ thông tin bắt buộc');
            return;
        }

        $.ajax({
            url: '/PaymentManagement/CreatePayment',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess('Ghi nhận thanh toán thành công');
                    $('#createPaymentModal').modal('hide');
                    $('#createPaymentForm')[0].reset();
                    $('#cardDetailsRow').hide();
                    $('#refundReasonRow').hide();
                    window.PaymentManagement.paymentTable.ajax.reload();
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Không thể ghi nhận thanh toán');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi ghi nhận thanh toán');
                }
            }
        });
    },

    viewPayment: function(paymentId) {
        $.ajax({
            url: '/PaymentManagement/GetPayment/' + paymentId,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    window.PaymentManagement.populateViewModal(response.data);
                    $('#viewPaymentModal').modal('show');
                } else {
                    GarageApp.showError('Không thể tải thông tin thanh toán');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin thanh toán');
                }
            }
        });
    },

    populateViewModal: function(payment) {
        $('#viewReceiptNumber').text(payment.receiptNumber);
        $('#viewPaymentDate').text(new Date(payment.paymentDate).toLocaleString('vi-VN'));
        $('#viewServiceOrderId').text('Đơn hàng #' + payment.serviceOrderId);
        $('#viewAmount').text(payment.amount.toLocaleString() + ' VNĐ');
        $('#viewPaymentMethod').text(payment.paymentMethod);
        $('#viewTransactionReference').text(payment.transactionReference || 'N/A');
        $('#viewReceivedBy').text(payment.receivedBy?.name || 'N/A');
        $('#viewIsRefund').text(payment.isRefund ? 'Hoàn tiền' : 'Thanh toán');
        $('#viewNotes').text(payment.notes || 'N/A');
        $('#viewCreatedAt').text(new Date(payment.createdAt).toLocaleString('vi-VN'));

        // Show card details if available
        if (payment.cardType || payment.cardLastFourDigits) {
            $('#viewCardType').text(payment.cardType || 'N/A');
            $('#viewCardLastFourDigits').text(payment.cardLastFourDigits || 'N/A');
            $('#viewCardDetailsRow').show();
        } else {
            $('#viewCardDetailsRow').hide();
        }

        // Show refund reason if available
        if (payment.isRefund && payment.refundReason) {
            $('#viewRefundReason').text(payment.refundReason);
            $('#viewRefundReasonRow').show();
        } else {
            $('#viewRefundReasonRow').hide();
        }
    },

    deletePayment: function(paymentId) {
        Swal.fire({
            title: 'Xác nhận xóa?',
            text: 'Bạn có chắc chắn muốn xóa giao dịch thanh toán này? Hành động này không thể hoàn tác.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/PaymentManagement/DeletePayment/' + paymentId,
                    type: 'DELETE',
                    success: function(response) {
                        if (response.success) {
                            GarageApp.showSuccess('Xóa giao dịch thanh toán thành công');
                            window.PaymentManagement.paymentTable.ajax.reload();
                        } else {
                            GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Không thể xóa giao dịch thanh toán');
                        }
                    },
                    error: function(xhr) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi xóa giao dịch thanh toán');
                        }
                    }
                });
            }
        });
    }
};

// Initialize when document is ready
$(document).ready(function() {
    if ($('#paymentTable').length) {
        PaymentManagement.init();
    }
});

