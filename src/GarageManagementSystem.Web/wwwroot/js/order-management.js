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
        this.loadDropdowns();
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        // Sử dụng DataTablesUtility với style chung
        var columns = [
            { data: 'id', title: 'ID', width: '60px' },
            { data: 'orderNumber', title: 'Số Đơn Hàng' },
            { data: 'customerName', title: 'Khách Hàng' },
            { data: 'vehiclePlate', title: 'Xe' },
            { 
                data: 'orderDate', 
                title: 'Ngày Đặt',
                render: DataTablesUtility.renderDate
            },
            { 
                data: 'finalAmount', 
                title: 'Tổng Tiền',
                render: DataTablesUtility.renderCurrency
            },
            { 
                data: 'status', 
                title: 'Trạng Thái',
                render: DataTablesUtility.renderStatus
            },
            {
                data: null,
                title: 'Thao Tác',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    var actions = `
                        <button class="btn btn-info btn-sm view-order" data-id="${row.id}">
                            <i class="fas fa-eye"></i>
                        </button>
                    `;
                    
                    // Chỉ hiển thị nút Edit và Delete khi trạng thái chưa hoàn thành
                    var isCompleted = row.status === 'Completed' || 
                                     row.status === 'Hoàn Thành' ||  // Chữ T viết hoa (từ TranslateStatus)
                                     row.status === 'Hoàn thành' || 
                                     row.status === 'completed' || 
                                     row.status === 'hoàn thành' ||
                                     (row.status && row.status.toLowerCase().includes('hoàn thành')) ||
                                     (row.status && row.status.toLowerCase().includes('completed'));
                    
                    if (!isCompleted) {
                        actions += `
                            <button class="btn btn-warning btn-sm edit-order" data-id="${row.id}">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-danger btn-sm delete-order" data-id="${row.id}">
                                <i class="fas fa-trash"></i>
                            </button>
                        `;
                    }
                    
                    return actions;
                }
            }
        ];
        
        this.orderTable = DataTablesUtility.initAjaxTable('#orderTable', '/OrderManagement/GetOrders', columns, {
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
            self.orderTable.search(this.value).draw();
        });

        // Create order form
        $(document).on('submit', '#createOrderForm', function(e) {
            e.preventDefault();
            self.createOrder();
        });

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

        // Bind ServiceQuotation change event
        $(document).on('change', '#createServiceQuotationId', function() {
            self.onQuotationChange();
        });
    },

    // Create order
    createOrder: function() {
        var self = this;
        
        var formData = {
            ServiceQuotationId: parseInt($('#createServiceQuotationId').val()),
            VehicleId: parseInt($('#createVehicleId').val()),
            CustomerId: parseInt($('#createCustomerId').val()),
            OrderDate: $('#createOrderDate').val() || new Date().toISOString().split('T')[0],
            EstimatedStartDate: $('#createEstimatedStartDate').val() || null,
            EstimatedEndDate: $('#createEstimatedEndDate').val() || null,
            Priority: $('#createPriority').val() || 'Normal',
            Notes: $('#createNotes').val() || null,
            Status: 'Pending'
        };

        // Validate required fields
        if (!formData.ServiceQuotationId || !formData.VehicleId || !formData.CustomerId) {
            GarageApp.showError('Vui lòng điền đầy đủ thông tin bắt buộc');
            return;
        }

        $.ajax({
            url: '/OrderManagement/CreateOrder',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Tạo phiếu sửa chữa thành công!');
                        $('#createOrderModal').modal('hide');
                        self.orderTable.ajax.reload();
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi tạo phiếu sửa chữa');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tạo phiếu sửa chữa');
                }
            }
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
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error loading order');
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
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error loading order');
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
                                GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error deleting order');
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

    // Load dropdown data
    loadDropdowns: function() {
        this.loadQuotations();
        this.loadCustomers();
        this.loadVehicles();
    },

    loadQuotations: function() {
        var self = this;
        $.ajax({
            url: '/OrderManagement/GetAvailableQuotations',
            type: 'GET',
            success: function(data) {
                var $select = $('#createServiceQuotationId');
                $select.empty().append('<option value="">-- Chọn Báo Giá --</option>');
                
                if (data && data.length > 0) {
                    $.each(data, function(index, item) {
                        $select.append(`<option value="${item.value}" 
                            data-vehicle-id="${item.vehicleId}" 
                            data-customer-id="${item.customerId}" 
                            data-vehicle-info="${item.vehicleInfo}" 
                            data-customer-name="${item.customerName}" 
                            data-total-amount="${item.totalAmount}" 
                            data-quotation-date="${item.quotationDate}">${item.text}</option>`);
                    });
                }
                
                $select.select2({
                    placeholder: '-- Chọn Báo Giá --',
                    allowClear: true
                });
            },
            error: function(xhr, status, error) {
                console.error('Error loading quotations:', error);
                GarageApp.showError('Lỗi khi tải danh sách báo giá');
            }
        });
    },

    loadCustomers: function() {
        $.ajax({
            url: '/OrderManagement/GetCustomers',
            type: 'GET',
            success: function(response) {
                if (response && response.success && response.data) {
                    var $select = $('#createCustomerId');
                    $select.empty().append('<option value="">-- Chọn Khách Hàng --</option>');
                    
                    $.each(response.data, function(index, customer) {
                        $select.append(`<option value="${customer.id}">${customer.name}</option>`);
                    });
                    
                    $select.select2({
                        placeholder: '-- Chọn Khách Hàng --',
                        allowClear: true
                    });
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading customers:', error);
            }
        });
    },

    loadVehicles: function() {
        // Vehicles will be loaded based on selected customer
        var $select = $('#createVehicleId');
        $select.empty().append('<option value="">-- Chọn Xe --</option>');
        $select.select2({
            placeholder: '-- Chọn Xe --',
            allowClear: true
        });
    },

    onQuotationChange: function() {
        var selectedOption = $('#createServiceQuotationId option:selected');
        
        if (selectedOption.val()) {
            // Tự động điền thông tin từ ServiceQuotation
            var vehicleId = selectedOption.data('vehicle-id');
            var customerId = selectedOption.data('customer-id');
            var vehicleInfo = selectedOption.data('vehicle-info');
            var customerName = selectedOption.data('customer-name');
            var totalAmount = selectedOption.data('total-amount');
            var quotationDate = selectedOption.data('quotation-date');
            
            // Điền thông tin xe
            $('#createVehicleId').val(vehicleId).trigger('change');
            
            // Điền thông tin khách hàng
            $('#createCustomerId').val(customerId).trigger('change');
            
            // Hiển thị thông tin đã chọn
            console.log('Selected Quotation:', {
                vehicleId: vehicleId,
                customerId: customerId,
                vehicleInfo: vehicleInfo,
                customerName: customerName,
                totalAmount: totalAmount,
                quotationDate: quotationDate
            });
        } else {
            // Reset các field khi không chọn quotation
            $('#createVehicleId').val('').trigger('change');
            $('#createCustomerId').val('').trigger('change');
        }
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
                    url: '/OrderManagement/UpdateOrder/' + id,
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
        if (order.serviceOrderItems && order.serviceOrderItems.length > 0) {
            var itemsHtml = '';
            order.serviceOrderItems.forEach(function(item) {
                itemsHtml += `
                    <tr>
                        <td>${item.service?.name || item.partName || ''}</td>
                        <td>${item.quantity || 1}</td>
                        <td>${(item.unitPrice || 0).toLocaleString()} VNĐ</td>
                        <td>${(item.totalPrice || 0).toLocaleString()} VNĐ</td>
                    </tr>
                `;
            });
            $('#viewOrderItems').html(itemsHtml);
        } else {
            $('#viewOrderItems').html('<tr><td colspan="4" class="text-center text-muted">Không có dịch vụ nào</td></tr>');
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