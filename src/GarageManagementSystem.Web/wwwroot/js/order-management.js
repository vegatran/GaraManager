/**
 * Order Management Module
 * 
 * Handles all order-related operations
 * CRUD operations for service orders
 */

window.OrderManagement = {
    // DataTable instance
    orderTable: null,

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable('#orderTable')) {
            $('#orderTable').DataTable().destroy();
        }
        
        this.orderTable = DataTablesVietnamese.init('#orderTable', {
            processing: true,
            serverSide: false,
            ajax: {
                url: '/OrderManagement/GetOrders',
                type: 'GET',
                error: function(xhr, status, error) {
                    if (AuthHandler.isUnauthorized(xhr)) {
                        AuthHandler.handleUnauthorized(xhr, true);
                    } else {
                        GarageApp.showError('Error loading orders');
                    }
                }
            },
            columns: [
                { data: 'id', title: 'ID', width: '60px' },
                { data: 'orderNumber', title: 'Số Đơn Hàng' },
                { data: 'customerName', title: 'Khách Hàng' },
                { data: 'vehicleLicensePlate', title: 'Xe' },
                { data: 'orderDate', title: 'Ngày Đặt' },
                { data: 'totalAmount', title: 'Tổng Tiền' },
                { data: 'status', title: 'Trạng Thái' },
                {
                    data: null,
                    title: 'Thao Tác',
                    orderable: false,
                    searchable: false,
                    render: function(data, type, row) {
                        return `
                            <button class="btn btn-info btn-sm view-order" data-id="${row.id}">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-warning btn-sm edit-order" data-id="${row.id}">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-danger btn-sm delete-order" data-id="${row.id}">
                                <i class="fas fa-trash"></i>
                            </button>
                        `;
                    }
                }
            ],
            order: [[0, 'desc']],
            pageLength: 10,
            responsive: true
        });
    },

    // Bind events
    bindEvents: function() {
        var self = this;

        // View order
        $(document).on('click', '.view-order', function() {
            var id = $(this).data('id');
            self.viewOrder(id);
        });

        // Edit order
        $(document).on('click', '.edit-order', function() {
            var id = $(this).data('id');
            self.editOrder(id);
        });

        // Delete order
        $(document).on('click', '.delete-order', function() {
            var id = $(this).data('id');
            self.deleteOrder(id);
        });

        // Update order status
        $(document).on('click', '.update-status', function() {
            var id = $(this).data('id');
            self.updateOrderStatus(id);
        });
    },

    // View order
    viewOrder: function(id) {
        var self = this;
        
        $.ajax({
            url: '/OrderManagement/GetOrder/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        self.populateViewModal(response.data);
                        $('#viewOrderModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Error loading order');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error loading order');
                }
            }
        });
    },

    // Edit order
    editOrder: function(id) {
        var self = this;
        
        $.ajax({
            url: '/OrderManagement/GetOrder/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        self.populateEditModal(response.data);
                        $('#editOrderModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Error loading order');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error loading order');
                }
            }
        });
    },

    // Delete order
    deleteOrder: function(id) {
        var self = this;
        
        Swal.fire({
            title: 'Are you sure?',
            text: "You won't be able to revert this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/OrderManagement/DeleteOrder/' + id,
                    type: 'DELETE',
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Order deleted successfully!');
                                self.orderTable.ajax.reload();
                            } else {
                                GarageApp.showError(response.message || 'Error deleting order');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Error deleting order');
                        }
                    }
                });
            }
        });
    },

    // Update order status
    updateOrderStatus: function(id) {
        var self = this;
        
        Swal.fire({
            title: 'Update Order Status',
            input: 'select',
            inputOptions: {
                'Pending': 'Pending',
                'In Progress': 'In Progress',
                'Completed': 'Completed',
                'Cancelled': 'Cancelled'
            },
            inputPlaceholder: 'Select status',
            showCancelButton: true,
            confirmButtonText: 'Update',
            showLoaderOnConfirm: true,
            preConfirm: (status) => {
                return $.ajax({
                    url: '/OrderManagement/UpdateOrderStatus/' + id,
                    type: 'PUT',
                    contentType: 'application/json',
                    data: JSON.stringify({ Status: status })
                });
            },
            allowOutsideClick: () => !Swal.isLoading()
        }).then((result) => {
            if (result.isConfirmed) {
                if (AuthHandler.validateApiResponse(result.value)) {
                    if (result.value.success) {
                        GarageApp.showSuccess('Order status updated successfully!');
                        self.orderTable.ajax.reload();
                    } else {
                        GarageApp.showError(result.value.message || 'Error updating order status');
                    }
                }
            }
        });
    },

    // Populate view modal
    populateViewModal: function(order) {
        $('#viewOrderNumber').text(order.orderNumber || '');
        $('#viewCustomerName').text(order.customerName || '');
        $('#viewVehicleLicensePlate').text(order.vehicleLicensePlate || '');
        $('#viewOrderDate').text(order.orderDate || '');
        $('#viewTotalAmount').text(order.totalAmount || '');
        $('#viewStatus').text(order.status || '');
        $('#viewDescription').text(order.description || '');
        
        // Populate order items if available
        if (order.orderItems && order.orderItems.length > 0) {
            var itemsHtml = '';
            order.orderItems.forEach(function(item) {
                itemsHtml += `
                    <tr>
                        <td>${item.serviceName || item.partName || ''}</td>
                        <td>${item.quantity || 1}</td>
                        <td>${item.unitPrice || 0}</td>
                        <td>${item.totalPrice || 0}</td>
                    </tr>
                `;
            });
            $('#viewOrderItems').html(itemsHtml);
        }
    },

    // Populate edit modal
    populateEditModal: function(order) {
        $('#editOrderId').val(order.id);
        $('#editOrderNumber').val(order.orderNumber);
        $('#editStatus').val(order.status);
        $('#editDescription').val(order.description);
    }
};

// Initialize when document is ready
$(document).ready(function() {
    OrderManagement.init();
});