/**
 * Purchase Order Management Module
 * 
 * Handles all purchase order-related operations
 * View, confirm receipt, print purchase orders
 */

window.PurchaseOrderManagement = {
    // DataTable instance
    purchaseOrderTable: null,
    currentReferenceNumber: null,

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        // Sử dụng DataTablesUtility với style chung
        var columns = [
            { data: 'referenceNumber', title: 'Số Phiếu', width: '15%' },
            { 
                data: 'transactionDate', 
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
                render: function(data, type, row) {
                    var badgeClass = 'badge-secondary';
                    switch(data) {
                        case 'Chờ nhập': badgeClass = 'badge-warning'; break;
                        case 'Đã nhập': badgeClass = 'badge-success'; break;
                        case 'Hủy': badgeClass = 'badge-danger'; break;
                    }
                    return `<span class="badge ${badgeClass}">${data}</span>`;
                }
            },
            {
                data: null,
                title: 'Thao Tác',
                width: '16%',
                orderable: false,
                searchable: false,
                className: 'text-center',
                render: function(data, type, row) {
                    return `
                        <button class="btn btn-info btn-sm view-purchase-order" data-ref="${row.referenceNumber}">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-primary btn-sm print-purchase-order" data-ref="${row.referenceNumber}">
                            <i class="fas fa-print"></i>
                        </button>
                        <button class="btn btn-success btn-sm confirm-receipt" data-ref="${row.referenceNumber}">
                            <i class="fas fa-check"></i>
                        </button>
                    `;
                }
            }
        ];

        this.purchaseOrderTable = DataTablesUtility.initServerSideTable('#purchaseOrdersTable', '/api/purchaseorders', columns, {
            order: [[1, 'desc']], // Sort by date desc
            pageLength: 10
        });
    },

    // Bind events
    bindEvents: function() {
        var self = this;

        // Search functionality
        $('#searchInput').on('keyup', function() {
            self.purchaseOrderTable.search(this.value).draw();
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
    },

    // View purchase order details
    viewPurchaseOrderDetails: function(referenceNumber) {
        var self = this;
        self.currentReferenceNumber = referenceNumber;

        $.ajax({
            url: '/PurchaseOrder/GetPurchaseOrderDetails/' + encodeURIComponent(referenceNumber),
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
            url: '/PurchaseOrder/GetPurchaseOrderDetails/' + encodeURIComponent(referenceNumber),
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
    }
};

$(document).ready(function() {
    PurchaseOrderManagement.init();
});
