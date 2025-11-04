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
                        <div class="btn-group" role="group">
                            <button class="btn btn-info btn-sm view-order" data-id="${row.id}" title="Xem">
                                <i class="fas fa-eye"></i>
                            </button>
                    `;
                    
                    // ✅ 2.1.1: Nút "Chuyển sang Chờ Phân công" (chỉ hiện khi status = "Pending")
                    var status = row.status || row.statusOriginal || '';
                    if (status === 'Pending' || status === 'Chờ Xử Lý') {
                        actions += `
                            <button class="btn btn-primary btn-sm change-status-btn" data-id="${row.id}" data-status="PendingAssignment" title="Chuyển sang Chờ Phân công">
                                <i class="fas fa-arrow-right"></i>
                            </button>
                        `;
                    }
                    
                    // ✅ 2.1.2: Nút "Phân công" hoặc "Đổi KTV"
                    // Hiển thị khi status = PendingAssignment, ReadyToWork, InProgress (cho phép phân công/đổi KTV)
                    var canAssignTechnician = status === 'PendingAssignment' || 
                                             status === 'Chờ Phân Công' ||
                                             status === 'ReadyToWork' || 
                                             status === 'Sẵn Sàng' ||
                                             status === 'Sẵn Sàng Làm' ||
                                             status === 'InProgress' || 
                                             status === 'Đang Sửa Chữa' ||
                                             status === 'In Progress';
                    
                    if (canAssignTechnician) {
                        // Nếu đã ReadyToWork/InProgress, button title là "Đổi KTV"
                        var buttonTitle = (status === 'ReadyToWork' || status === 'Sẵn Sàng' || status === 'Sẵn Sàng Làm' || 
                                          status === 'InProgress' || status === 'Đang Sửa Chữa' || status === 'In Progress')
                                          ? 'Đổi KTV' : 'Phân công KTV';
                        var buttonClass = (status === 'ReadyToWork' || status === 'Sẵn Sàng' || status === 'Sẵn Sàng Làm' || 
                                          status === 'InProgress' || status === 'Đang Sửa Chữa' || status === 'In Progress')
                                          ? 'btn-warning' : 'btn-success';
                        
                        actions += `
                            <button class="btn ${buttonClass} btn-sm assign-technician-btn" data-id="${row.id}" title="${buttonTitle}">
                                <i class="fas fa-user-tie"></i>
                            </button>
                        `;
                    }
                    
                    // Chỉ hiển thị nút Edit và Delete khi trạng thái chưa hoàn thành
                    // ✅ SỬA: Chỉ cho phép Edit/Delete khi status = Pending, PendingAssignment, WaitingForParts
                    // Khi đã ReadyToWork/InProgress/Completed thì không cho phép edit (phải dùng workflow)
                    // (status đã được khai báo ở trên)
                    
                    // Kiểm tra Completed trước
                    var isCompleted = status === 'Completed' || 
                                     status === 'Hoàn Thành' ||
                                     status === 'Hoàn thành' || 
                                     status === 'completed' || 
                                     status === 'hoàn thành' ||
                                     (status && status.toLowerCase().includes('hoàn thành')) ||
                                     (status && status.toLowerCase().includes('completed'));
                    
                    // ❌ Không cho phép edit khi đã ReadyToWork, InProgress, Completed
                    var isRestrictedStatus = status === 'ReadyToWork' || 
                                            status === 'Sẵn Sàng' ||
                                            status === 'Sẵn Sàng Làm' ||
                                            status === 'InProgress' || 
                                            status === 'Đang Sửa Chữa' ||
                                            status === 'In Progress' ||
                                            isCompleted;
                    
                    // Chỉ cho phép Edit khi status = Pending, PendingAssignment, WaitingForParts
                    var isEditableStatus = status === 'Pending' || 
                                          status === 'Chờ Xử Lý' ||
                                          status === 'PendingAssignment' || 
                                          status === 'Chờ Phân Công' ||
                                          status === 'WaitingForParts' || 
                                          status === 'Chờ Vật Tư';
                    
                    // Chỉ hiển thị Edit/Delete khi status cho phép edit và không bị restricted
                    if (isEditableStatus && !isRestrictedStatus) {
                        actions += `
                            <button class="btn btn-warning btn-sm edit-order" data-id="${row.id}" title="Sửa">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-danger btn-sm delete-order" data-id="${row.id}" title="Xóa">
                                <i class="fas fa-trash"></i>
                            </button>
                        `;
                    }
                    
                    actions += `</div>`;
                    return actions;
                }
            }
        ];
        
        // ✅ SỬA: Sử dụng endpoint Web controller thay vì API trực tiếp
        // Endpoint này sẽ return format phù hợp với DataTablesUtility
        this.orderTable = DataTablesUtility.initServerSideTable('#orderTable', '/OrderManagement/GetOrdersPaged', columns, {
            order: [[0, 'desc']],
            pageLength: 10
        });
    },

    // Bind events
    bindEvents: function() {
        var self = this;

        // ✅ SỬA: Thêm button handler theo pattern EmployeeManagement
        $(document).on('click', '#addOrderBtn', function() {
            self.showCreateModal();
        });

        // ✅ THÊM: Reset form khi modal đóng
        $('#createOrderModal').on('hidden.bs.modal', function() {
            self.resetCreateModal();
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

        // ✅ 2.1.1: Chuyển trạng thái
        $(document).on('click', '.change-status-btn', function() {
            var id = $(this).data('id');
            var newStatus = $(this).data('status');
            self.changeOrderStatus(id, newStatus);
        });

        // ✅ 2.1.2: Mở modal phân công KTV
        $(document).on('click', '.assign-technician-btn', function() {
            var id = $(this).data('id');
            self.openAssignTechnicianModal(id);
        });

        // ✅ 2.1.2: Phân công từng item
        $(document).on('click', '.btn-assign-item', function() {
            var orderId = $('#assignTechnicianOrderId').val();
            var itemId = $(this).closest('tr').data('item-id');
            self.assignTechnicianToItem(orderId, itemId);
        });

        // ✅ 2.1.2: Phân công hàng loạt
        $(document).on('click', '#btnBulkAssign', function() {
            var orderId = $('#assignTechnicianOrderId').val();
            self.bulkAssignTechnician(orderId);
        });

        // ✅ 2.1.2: Lưu tất cả phân công
        $(document).on('click', '#btnSaveAssignments', function() {
            var orderId = $('#assignTechnicianOrderId').val();
            self.saveAllAssignments(orderId);
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

    // ✅ THÊM: Reset modal form về trạng thái ban đầu
    resetCreateModal: function() {
        var self = this;
        
        // Reset form
        $('#createOrderForm')[0]?.reset();
        
        // ✅ SỬA: Destroy Select2 để reset hoàn toàn
        if ($('#createServiceQuotationId').hasClass('select2-hidden-accessible')) {
            $('#createServiceQuotationId').select2('destroy');
        }
        if ($('#createCustomerId').hasClass('select2-hidden-accessible')) {
            $('#createCustomerId').select2('destroy');
        }
        if ($('#createVehicleId').hasClass('select2-hidden-accessible')) {
            $('#createVehicleId').select2('destroy');
        }
        
        // Clear và reset dropdowns
        $('#createServiceQuotationId').empty().append('<option value="">-- Chọn Báo Giá --</option>');
        $('#createCustomerId').empty().append('<option value="">Sẽ tự động chọn theo báo giá</option>');
        $('#createVehicleId').empty().append('<option value="">Sẽ tự động chọn theo báo giá</option>');
        
        // Reset textareas
        $('#createNotes').val('');
        
        // Reset Priority dropdown
        $('#createPriority').val('Normal');
        
        // Reset disabled state
        $('#createCustomerId').prop('disabled', true);
        $('#createVehicleId').prop('disabled', true);
    },

    // ✅ THÊM: Show create modal theo pattern EmployeeManagement
    showCreateModal: function() {
        var self = this;
        
        // ✅ SỬA: Reset form trước khi mở modal
        self.resetCreateModal();
        
        // Set default dates to today
        var today = new Date().toISOString().split('T')[0];
        $('#createOrderDate').val(today);
        $('#createEstimatedStartDate').val(today); // ✅ THÊM: Ngày dự kiến bắt đầu = hôm nay
        $('#createEstimatedEndDate').val(today);   // ✅ THÊM: Ngày dự kiến kết thúc = hôm nay
        
        // ✅ SỬA: Luôn load lại quotations khi mở modal để đảm bảo có data mới nhất
        self.loadQuotations();
        
        // ✅ THÊM: Đợi một chút để Select2 được khởi tạo xong
        setTimeout(function() {
            $('#createOrderModal').modal('show');
        }, 150);
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
                    return;
                }
                var msg = 'Lỗi khi tạo phiếu sửa chữa';
                try {
                    if (xhr.responseJSON) {
                        // Chuẩn của ApiResponse/PagedResponse
                        if (xhr.responseJSON.error) msg = xhr.responseJSON.error;
                        else if (xhr.responseJSON.message) msg = xhr.responseJSON.message;
                        else if (xhr.responseJSON.errorMessage) msg = xhr.responseJSON.errorMessage;
                    } else if (xhr.responseText) {
                        var obj = JSON.parse(xhr.responseText);
                        msg = obj.error || obj.message || msg;
                    }
                } catch (e) { /* ignore parse errors */ }
                GarageApp.showError(msg);
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
                console.log('[loadQuotations] Response:', data);
                
                var $select = $('#createServiceQuotationId');
                $select.empty().append('<option value="">-- Chọn Báo Giá --</option>');
                
                if (data && data.length > 0) {
                    console.log(`[loadQuotations] Found ${data.length} available quotations`);
                    $.each(data, function(index, item) {
                        $select.append(`<option value="${item.value}" 
                            data-vehicle-id="${item.vehicleId}" 
                            data-customer-id="${item.customerId}" 
                            data-vehicle-info="${item.vehicleInfo}" 
                            data-customer-name="${item.customerName}" 
                            data-total-amount="${item.totalAmount}" 
                            data-quotation-date="${item.quotationDate}">${item.text}</option>`);
                    });
                    
                    // ✅ SỬA: Reinitialize Select2 sau khi append options
                    if ($select.hasClass('select2-hidden-accessible')) {
                        $select.select2('destroy');
                    }
                    $select.select2({
                        placeholder: '-- Chọn Báo Giá --',
                        allowClear: true,
                        width: '100%'
                    });
                } else {
                    console.warn('[loadQuotations] No available quotations found');
                    // ✅ THÊM: Hiển thị thông báo nếu không có báo giá
                    if ($select.find('option').length <= 1) {
                        $select.append('<option value="" disabled>Không có báo giá nào sẵn sàng</option>');
                    }
                    
                    // Reinitialize Select2
                    if ($select.hasClass('select2-hidden-accessible')) {
                        $select.select2('destroy');
                    }
                    $select.select2({
                        placeholder: 'Không có báo giá sẵn sàng',
                        allowClear: false,
                        width: '100%'
                    });
                }
            },
            error: function(xhr, status, error) {
                console.error('[loadQuotations] Error:', xhr, status, error);
                var errorMsg = 'Lỗi khi tải danh sách báo giá';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMsg += ': ' + xhr.responseJSON.message;
                }
                GarageApp.showError(errorMsg);
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
            
            console.log('[onQuotationChange] Selected quotation:', {
                vehicleId: vehicleId,
                customerId: customerId,
                vehicleInfo: vehicleInfo,
                customerName: customerName
            });
            
            // ✅ SỬA: Load và set Customer dropdown
            var $customerSelect = $('#createCustomerId');
            if (customerId && customerName) {
                // Clear và thêm option mới
                $customerSelect.empty();
                $customerSelect.append(`<option value="${customerId}">${customerName}</option>`);
                
                // Enable và set value
                $customerSelect.prop('disabled', false);
                $customerSelect.val(customerId);
                
                // Reinitialize Select2 để update display
                if ($customerSelect.hasClass('select2-hidden-accessible')) {
                    $customerSelect.select2('destroy');
                }
                $customerSelect.select2({
                    placeholder: customerName,
                    allowClear: false,
                    width: '100%'
                });
            }
            
            // ✅ SỬA: Load và set Vehicle dropdown
            var $vehicleSelect = $('#createVehicleId');
            if (vehicleId && vehicleInfo) {
                // Clear và thêm option mới
                $vehicleSelect.empty();
                $vehicleSelect.append(`<option value="${vehicleId}">${vehicleInfo}</option>`);
                
                // Enable và set value
                $vehicleSelect.prop('disabled', false);
                $vehicleSelect.val(vehicleId);
                
                // Reinitialize Select2 để update display
                if ($vehicleSelect.hasClass('select2-hidden-accessible')) {
                    $vehicleSelect.select2('destroy');
                }
                $vehicleSelect.select2({
                    placeholder: vehicleInfo,
                    allowClear: false,
                    width: '100%'
                });
            }
            
            console.log('[onQuotationChange] Set customer and vehicle values successfully');
        } else {
            // Reset các field khi không chọn quotation
            var $customerSelect = $('#createCustomerId');
            var $vehicleSelect = $('#createVehicleId');
            
            $customerSelect.val('').trigger('change');
            $vehicleSelect.val('').trigger('change');
            
            // Reset to placeholder options
            $customerSelect.empty().append('<option value="">Sẽ tự động chọn theo báo giá</option>');
            $vehicleSelect.empty().append('<option value="">Sẽ tự động chọn theo báo giá</option>');
            
            // Disable lại
            $customerSelect.prop('disabled', true);
            $vehicleSelect.prop('disabled', true);
            
            // Reinitialize Select2
            if ($customerSelect.hasClass('select2-hidden-accessible')) {
                $customerSelect.select2('destroy');
            }
            if ($vehicleSelect.hasClass('select2-hidden-accessible')) {
                $vehicleSelect.select2('destroy');
            }
            $customerSelect.select2({
                placeholder: 'Sẽ tự động chọn theo báo giá',
                allowClear: false,
                width: '100%'
            });
            $vehicleSelect.select2({
                placeholder: 'Sẽ tự động chọn theo báo giá',
                allowClear: false,
                width: '100%'
            });
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
        // ✅ 2.3.2: Store serviceOrderId in modal data
        $('#viewOrderModal').data('order-id', order.id);
        this.currentServiceOrderId = order.id;

        $('#viewOrderNumber').text(order.orderNumber || '');
        // ✅ SỬA: Dùng nested objects cho Customer và Vehicle
        $('#viewCustomerName').text(order.customer?.name || order.customerName || '');
        $('#viewVehicleLicensePlate').text(order.vehicle?.licensePlate || order.vehicleLicensePlate || '');
        // ✅ SỬA: Format date đúng cách (dd/MM/yyyy)
        if (order.orderDate) {
            var orderDate = new Date(order.orderDate);
            var formattedDate = orderDate.toLocaleDateString('vi-VN', { 
                day: '2-digit', 
                month: '2-digit', 
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
            $('#viewOrderDate').text(formattedDate);
        } else {
            $('#viewOrderDate').text('');
        }
        // ✅ SỬA: Format currency đúng cách
        $('#viewTotalAmount').text(order.totalAmount ? order.totalAmount.toLocaleString('vi-VN') + ' VNĐ' : '0 VNĐ');
        // ✅ THÊM: Format status sang tiếng Việt
        var statusText = OrderManagement.formatServiceOrderStatus(order.status || '');
        $('#viewStatus').html(statusText);
        $('#viewDescription').text(order.description || '');
        
        // ✅ THÊM: Populate order items với thông tin phân công và tiến độ
        if (order.serviceOrderItems && order.serviceOrderItems.length > 0) {
            var itemsHtml = '';
            var self = this;
            order.serviceOrderItems.forEach(function(item) {
                var statusBadge = self.formatItemStatus(item.status);
                var actualHoursDisplay = item.actualHours ? 
                    item.actualHours.toFixed(2) + ' giờ' : 
                    '<span class="text-muted">-</span>';
                
                // ✅ 2.3.1: Thêm nút Start/Stop/Complete dựa trên status
                var actionButtons = self.getItemActionButtons(order.id, item.id, item.status, item.startTime, item.completedTime);
                
                itemsHtml += `
                    <tr data-item-id="${item.id}">
                        <td>${item.service?.name || item.serviceName || ''}</td>
                        <td>${item.quantity || 1}</td>
                        <td>${(item.unitPrice || 0).toLocaleString()} VNĐ</td>
                        <td>${(item.totalPrice || 0).toLocaleString()} VNĐ</td>
                        <td>${item.assignedTechnicianName || '<span class="text-muted">Chưa phân công</span>'}</td>
                        <td>${item.estimatedHours ? item.estimatedHours.toFixed(2) + ' giờ' : '<span class="text-muted">-</span>'}</td>
                        <td>${statusBadge}</td>
                        <td>${actualHoursDisplay}</td>
                        <td>${actionButtons}</td>
                    </tr>
                `;
            });
            $('#viewOrderItems').html(itemsHtml);
            
            // ✅ 2.3.1: Bind event handlers cho các nút action
            self.bindItemActionEvents(order.id);
        } else {
            $('#viewOrderItems').html('<tr><td colspan="9" class="text-center text-muted">Không có dịch vụ nào</td></tr>');
        }
    },

    // ✅ THÊM: Format Service Order status sang tiếng Việt
    formatServiceOrderStatus: function(status) {
        if (!status) return '<span class="badge badge-secondary">N/A</span>';
        
        var statusMap = {
            'Pending': '<span class="badge badge-warning">Chờ Xử Lý</span>',
            'PendingAssignment': '<span class="badge badge-warning">Chờ Phân Công</span>',
            'Pending Assignment': '<span class="badge badge-warning">Chờ Phân Công</span>',
            'WaitingForParts': '<span class="badge badge-warning">Chờ Vật Tư</span>',
            'Waiting For Parts': '<span class="badge badge-warning">Chờ Vật Tư</span>',
            'ReadyToWork': '<span class="badge badge-success">Sẵn Sàng Làm</span>',
            'Ready To Work': '<span class="badge badge-success">Sẵn Sàng Làm</span>',
            'Ready': '<span class="badge badge-success">Sẵn Sàng Làm</span>',
            'InProgress': '<span class="badge badge-info">Đang Sửa Chữa</span>',
            'In Progress': '<span class="badge badge-info">Đang Sửa Chữa</span>',
            'Completed': '<span class="badge badge-success">Đã Hoàn Thành</span>',
            'Cancelled': '<span class="badge badge-danger">Đã Hủy</span>'
        };
        
        return statusMap[status] || '<span class="badge badge-secondary">' + status + '</span>';
    },

    // Populate edit modal
    populateEditModal: function(order) {
        $('#editOrderId').val(order.id);
        $('#editOrderNumber').val(order.orderNumber);
        $('#editStatus').val(order.status);
        $('#editDescription').val(order.description);
    },

    // ✅ 2.1.1: Chuyển trạng thái ServiceOrder
    changeOrderStatus: function(id, newStatus) {
        var self = this;
        
        Swal.fire({
            title: 'Chuyển trạng thái?',
            text: `Bạn có chắc muốn chuyển trạng thái sang "${newStatus}"?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Xác nhận',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/OrderManagement/ChangeOrderStatus/' + id,
                    type: 'PUT',
                    contentType: 'application/json',
                    data: JSON.stringify({ Status: newStatus }),
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Đã chuyển trạng thái thành công!');
                                self.orderTable.ajax.reload();
                            } else {
                                GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi chuyển trạng thái');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi chuyển trạng thái');
                        }
                    }
                });
            }
        });
    },

    // ✅ 2.1.2: Mở modal phân công KTV
    openAssignTechnicianModal: function(orderId) {
        var self = this;
        
        // Load order details
        $.ajax({
            url: '/OrderManagement/GetOrder/' + orderId,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response) && response.success) {
                    var order = response.data;
                    $('#assignTechnicianOrderId').val(order.id);
                    $('#assignTechnicianOrderNumber').val(order.orderNumber || '');
                    
                    // Populate items
                    self.populateAssignTechnicianItems(order);
                    
                    // Load technicians with workload
                    self.loadTechniciansForAssignment();
                    
                    $('#assignTechnicianModal').modal('show');
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin đơn hàng');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin đơn hàng');
                }
            }
        });
    },

    // ✅ BỔ SUNG: Load technicians với workload info
    loadTechniciansForAssignment: function() {
        var self = this;
        
        $.ajax({
            url: '/OrderManagement/GetActiveEmployees',
            type: 'GET',
            success: function(employees) {
                // Load workload for each employee
                var technicianPromises = employees.map(function(emp) {
                    return $.ajax({
                        url: '/api/employees/' + emp.id + '/workload',
                        type: 'GET'
                    }).then(function(workloadResponse) {
                        var workload = workloadResponse.success && workloadResponse.data ? workloadResponse.data : null;
                        return {
                            employee: emp,
                            workload: workload
                        };
                    }).catch(function() {
                        return { employee: emp, workload: null };
                    });
                });
                
                Promise.all(technicianPromises).then(function(techniciansWithWorkload) {
                    self.populateTechnicianDropdowns(techniciansWithWorkload);
                });
            },
            error: function() {
                GarageApp.showError('Lỗi khi tải danh sách KTV');
            }
        });
    },

    // ✅ BỔ SUNG: Populate technician dropdowns với workload
    populateTechnicianDropdowns: function(techniciansWithWorkload) {
        var defaultOption = '<option value="">-- Chọn KTV --</option>';
        
        // Populate bulk dropdown
        var $bulkSelect = $('#bulkTechnicianSelect');
        $bulkSelect.empty().append(defaultOption);
        
        techniciansWithWorkload.forEach(function(item) {
            var emp = item.employee;
            var workload = item.workload;
            var displayText = emp.text || (emp.name + ' - ' + (emp.position || ''));
            
            // ✅ THÊM: Hiển thị workload nếu có
            if (workload && workload.statistics) {
                var totalHours = workload.activeOrders.totalEstimatedHours || 0;
                var activeCount = workload.activeOrders.count || 0;
                var capacity = workload.statistics.capacityUsed || 0;
                displayText += ` (${totalHours.toFixed(1)}h/8h, ${activeCount} JO, ${capacity.toFixed(0)}% tải)`;
            }
            
            $bulkSelect.append(`<option value="${emp.id}" data-workload='${JSON.stringify(workload || {})}'>${displayText}</option>`);
        });
        
        $bulkSelect.select2({
            placeholder: '-- Chọn KTV --',
            allowClear: true
        });
        
        // Populate individual dropdowns in table rows (will be done in populateAssignTechnicianItems)
    },

    // ✅ BỔ SUNG: Populate items vào modal table
    populateAssignTechnicianItems: function(order) {
        var self = this;
        var tbody = $('#assignTechnicianItemsBody');
        tbody.empty();
        
        if (!order.serviceOrderItems || order.serviceOrderItems.length === 0) {
            tbody.append('<tr><td colspan="6" class="text-center text-muted">Không có hạng mục nào</td></tr>');
            return;
        }
        
        // Load technicians first
        $.ajax({
            url: '/OrderManagement/GetActiveEmployees',
            type: 'GET',
            success: function(employees) {
                order.serviceOrderItems.forEach(function(item, index) {
                    var row = `
                        <tr data-item-id="${item.id}">
                            <td class="text-center">${index + 1}</td>
                            <td><strong>${item.service?.name || item.serviceName || ''}</strong></td>
                            <td>
                                <select class="form-control form-control-sm technician-select" data-item-id="${item.id}">
                                    <option value="">-- Chọn KTV --</option>
                    `;
                    
                    employees.forEach(function(emp) {
                        var selected = item.assignedTechnicianId == emp.id ? 'selected' : '';
                        row += `<option value="${emp.id}" ${selected}>${emp.text || (emp.name + ' - ' + (emp.position || ''))}</option>`;
                    });
                    
                    row += `
                                </select>
                            </td>
                            <td>
                                <input type="number" class="form-control form-control-sm estimated-hours-input" 
                                       data-item-id="${item.id}" 
                                       value="${item.estimatedHours || ''}" 
                                       step="0.1" min="0.1" max="24" 
                                       placeholder="Giờ công">
                            </td>
                            <td class="text-center">
                                <span class="badge badge-${item.status === 'Completed' ? 'success' : 'warning'}">
                                    ${item.status || 'Pending'}
                                </span>
                            </td>
                            <td class="text-center">
                                <button type="button" class="btn btn-sm btn-success btn-assign-item" title="Phân công">
                                    <i class="fas fa-check"></i>
                                </button>
                            </td>
                        </tr>
                    `;
                    
                    tbody.append(row);
                });
            },
            error: function() {
                GarageApp.showError('Lỗi khi tải danh sách KTV');
            }
        });
    },

    // ✅ 2.1.2: Phân công KTV cho một item
    assignTechnicianToItem: function(orderId, itemId) {
        var self = this;
        var row = $(`tr[data-item-id="${itemId}"]`);
        var technicianId = row.find('.technician-select').val();
        var estimatedHours = row.find('.estimated-hours-input').val();
        
        if (!technicianId) {
            GarageApp.showError('Vui lòng chọn KTV');
            return;
        }
        
        $.ajax({
            url: '/OrderManagement/AssignTechnician/' + orderId + '/items/' + itemId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({
                TechnicianId: parseInt(technicianId),
                EstimatedHours: estimatedHours ? parseFloat(estimatedHours) : null,
                Notes: ''
            }),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Phân công KTV thành công!');
                        // Optionally reload modal to show updated info
                        self.openAssignTechnicianModal(orderId);
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi phân công KTV');
                    }
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi phân công KTV');
                }
            }
        });
    },

    // ✅ 2.1.2: Phân công hàng loạt
    bulkAssignTechnician: function(orderId) {
        var self = this;
        var technicianId = $('#bulkTechnicianSelect').val();
        var estimatedHours = $('#bulkEstimatedHours').val();
        
        if (!technicianId) {
            GarageApp.showError('Vui lòng chọn KTV');
            return;
        }
        
        // Get all unassigned items
        var unassignedItems = [];
        $('#assignTechnicianItemsBody tr[data-item-id]').each(function() {
            var itemId = $(this).data('item-id');
            var currentTechnician = $(this).find('.technician-select').val();
            if (!currentTechnician || currentTechnician === '') {
                unassignedItems.push(itemId);
            }
        });
        
        if (unassignedItems.length === 0) {
            GarageApp.showWarning('Tất cả hạng mục đã được phân công');
            return;
        }
        
        $.ajax({
            url: '/OrderManagement/BulkAssignTechnician/' + orderId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({
                TechnicianId: parseInt(technicianId),
                EstimatedHours: estimatedHours ? parseFloat(estimatedHours) : null,
                ItemIds: unassignedItems,
                Notes: 'Phân công hàng loạt'
            }),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess(`Đã phân công ${unassignedItems.length} hạng mục thành công!`);
                        self.openAssignTechnicianModal(orderId);
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi phân công hàng loạt');
                    }
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi phân công hàng loạt');
                }
            }
        });
    },

    // ✅ 2.1.2: Lưu tất cả phân công
    saveAllAssignments: function(orderId) {
        var self = this;
        var assignments = [];
        
        $('#assignTechnicianItemsBody tr[data-item-id]').each(function() {
            var itemId = $(this).data('item-id');
            var technicianId = $(this).find('.technician-select').val();
            var estimatedHours = $(this).find('.estimated-hours-input').val();
            
            if (technicianId) {
                assignments.push({
                    itemId: itemId,
                    technicianId: parseInt(technicianId),
                    estimatedHours: estimatedHours ? parseFloat(estimatedHours) : null
                });
            }
        });
        
        if (assignments.length === 0) {
            GarageApp.showWarning('Không có phân công nào để lưu');
            return;
        }
        
        // Save all assignments
        var promises = assignments.map(function(assignment) {
            return $.ajax({
                url: '/OrderManagement/AssignTechnician/' + orderId + '/items/' + assignment.itemId,
                type: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify({
                    TechnicianId: assignment.technicianId,
                    EstimatedHours: assignment.estimatedHours,
                    Notes: ''
                })
            });
        });
        
        Promise.all(promises).then(function(responses) {
            var successCount = responses.filter(function(r) {
                return AuthHandler.validateApiResponse(r) && r.success;
            }).length;
            
            if (successCount === assignments.length) {
                GarageApp.showSuccess(`Đã lưu ${successCount} phân công thành công!`);
                self.orderTable.ajax.reload();
                $('#assignTechnicianModal').modal('hide');
            } else {
                GarageApp.showError(`Lưu ${successCount}/${assignments.length} phân công. Vui lòng kiểm tra lại.`);
            }
        }).catch(function() {
            GarageApp.showError('Lỗi khi lưu phân công');
        });
    },

    // ✅ 2.3.1: Format item status sang tiếng Việt với badge
    formatItemStatus: function(status) {
        if (!status) return '<span class="badge badge-secondary">N/A</span>';
        
        var statusMap = {
            'Pending': '<span class="badge badge-warning">Chờ</span>',
            'InProgress': '<span class="badge badge-info">Đang Làm</span>',
            'In Progress': '<span class="badge badge-info">Đang Làm</span>',
            'Completed': '<span class="badge badge-success">Hoàn Thành</span>',
            'Cancelled': '<span class="badge badge-danger">Đã Hủy</span>'
        };
        
        return statusMap[status] || '<span class="badge badge-secondary">' + status + '</span>';
    },

    // ✅ 2.3.1: Tạo HTML cho các nút action (Start/Stop/Complete) dựa trên status
    getItemActionButtons: function(orderId, itemId, status, startTime, completedTime) {
        var buttons = '';
        
        if (status === 'Pending' || status === 'ReadyToWork' || status === 'Ready To Work') {
            // Chưa bắt đầu hoặc đã dừng -> Hiển thị nút "Bắt đầu" hoặc "Tiếp tục"
            var buttonText = startTime ? 'Tiếp tục' : 'Bắt đầu';
            var buttonTitle = startTime ? 'Tiếp tục làm việc (đã có giờ công trước đó)' : 'Bắt đầu làm việc';
            buttons = `<button class="btn btn-sm btn-success btn-start-work" data-order-id="${orderId}" data-item-id="${itemId}" title="${buttonTitle}">
                <i class="fas fa-play"></i> ${buttonText}
            </button>`;
        } else if (status === 'InProgress' || status === 'In Progress') {
            // Đang làm -> Hiển thị nút "Dừng" và "Hoàn thành"
            buttons = `
                <button class="btn btn-sm btn-warning btn-stop-work" data-order-id="${orderId}" data-item-id="${itemId}" title="Dừng làm việc (tạm dừng, có thể tiếp tục sau)">
                    <i class="fas fa-pause"></i> Dừng
                </button>
                <button class="btn btn-sm btn-success btn-complete-item" data-order-id="${orderId}" data-item-id="${itemId}" title="Hoàn thành">
                    <i class="fas fa-check"></i> Hoàn thành
                </button>`;
        } else if (status === 'Completed') {
            // Đã hoàn thành -> Hiển thị thông tin
            var completedInfo = completedTime ? 
                '<small class="text-muted d-block">' + new Date(completedTime).toLocaleString('vi-VN') + '</small>' : '';
            buttons = '<span class="text-success"><i class="fas fa-check-circle"></i> Đã hoàn thành</span>' + completedInfo;
        } else {
            buttons = '<span class="text-muted">-</span>';
        }
        
        return buttons;
    },

    // ✅ 2.3.1: Bind event handlers cho các nút action (Start/Stop/Complete)
    bindItemActionEvents: function(orderId) {
        var self = this;
        
        // Bắt đầu làm việc
        $(document).off('click', '.btn-start-work').on('click', '.btn-start-work', function() {
            var itemId = $(this).data('item-id');
            self.startItemWork(orderId, itemId, $(this));
        });
        
        // Dừng làm việc
        $(document).off('click', '.btn-stop-work').on('click', '.btn-stop-work', function() {
            var itemId = $(this).data('item-id');
            self.stopItemWork(orderId, itemId, $(this));
        });
        
        // Hoàn thành item
        $(document).off('click', '.btn-complete-item').on('click', '.btn-complete-item', function() {
            var itemId = $(this).data('item-id');
            self.completeItem(orderId, itemId, $(this));
        });
    },

    // ✅ 2.3.1: KTV bắt đầu làm việc cho một item
    startItemWork: function(orderId, itemId, $button) {
        var self = this;
        
        Swal.fire({
            title: 'Bắt đầu làm việc?',
            text: 'Bạn có chắc muốn bắt đầu làm việc cho hạng mục này?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Bắt đầu',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $button.prop('disabled', true);
                
                $.ajax({
                    url: '/OrderManagement/StartItemWork/' + orderId + '/items/' + itemId,
                    type: 'POST',
                    contentType: 'application/json',
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Đã bắt đầu làm việc!');
                                // Reload order details trong modal
                                self.viewOrder(orderId);
                            } else {
                                GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi bắt đầu làm việc');
                                $button.prop('disabled', false);
                            }
                        } else {
                            $button.prop('disabled', false);
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi bắt đầu làm việc');
                        }
                        $button.prop('disabled', false);
                    }
                });
            }
        });
    },

    // ✅ 2.3.1: KTV dừng làm việc cho một item
    stopItemWork: function(orderId, itemId, $button) {
        var self = this;
        
        Swal.fire({
            title: 'Dừng làm việc?',
            text: 'Bạn có chắc muốn dừng làm việc cho hạng mục này?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Dừng',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $button.prop('disabled', true);
                
                $.ajax({
                    url: '/OrderManagement/StopItemWork/' + orderId + '/items/' + itemId,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(null), // Có thể truyền actualHours nếu cần
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Đã dừng làm việc!');
                                // Reload order details trong modal
                                self.viewOrder(orderId);
                            } else {
                                GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi dừng làm việc');
                                $button.prop('disabled', false);
                            }
                        } else {
                            $button.prop('disabled', false);
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi dừng làm việc');
                        }
                        $button.prop('disabled', false);
                    }
                });
            }
        });
    },

    // ✅ 2.3.1 & 2.3.4: KTV hoàn thành một item
    completeItem: function(orderId, itemId, $button) {
        var self = this;
        
        Swal.fire({
            title: 'Hoàn thành hạng mục?',
            text: 'Bạn có chắc hạng mục này đã hoàn thành?',
            icon: 'success',
            showCancelButton: true,
            confirmButtonText: 'Hoàn thành',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $button.prop('disabled', true);
                
                $.ajax({
                    url: '/OrderManagement/CompleteItem/' + orderId + '/items/' + itemId,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(null), // Có thể truyền actualHours nếu cần
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Đã hoàn thành hạng mục!');
                                // Reload order details trong modal
                                self.viewOrder(orderId);
                                // Reload DataTable
                                self.orderTable.ajax.reload();
                            } else {
                                GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi hoàn thành hạng mục');
                                $button.prop('disabled', false);
                            }
                        } else {
                            $button.prop('disabled', false);
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi hoàn thành hạng mục');
                        }
                        $button.prop('disabled', false);
                    }
                });
            }
        });
    },

    // ✅ 2.3.2: Additional Issues (Phát Sinh) Management
    currentServiceOrderId: null,

    // Load additional issues when tab is clicked
    loadAdditionalIssues: function(serviceOrderId) {
        var self = this;
        self.currentServiceOrderId = serviceOrderId;

        $.ajax({
            url: '/AdditionalIssues/GetByServiceOrder/' + serviceOrderId,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response) && response.success) {
                    self.renderAdditionalIssuesList(response.data || []);
                } else {
                    $('#additionalIssuesList').html('<p class="text-danger">Lỗi khi tải danh sách phát sinh</p>');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    $('#additionalIssuesList').html('<p class="text-danger">Lỗi khi tải danh sách phát sinh</p>');
                }
            }
        });
    },

    // Render additional issues list
    renderAdditionalIssuesList: function(issues) {
        var self = this;
        var html = '';

        if (!issues || issues.length === 0) {
            html = '<p class="text-muted">Chưa có phát sinh nào được báo cáo.</p>';
        } else {
            html = '<div class="table-responsive"><table class="table table-bordered table-sm">';
            html += '<thead class="bg-light"><tr>';
            html += '<th>Tên Phát Sinh</th>';
            html += '<th>Danh Mục</th>';
            html += '<th>Mức Độ</th>';
            html += '<th>Trạng Thái</th>';
            html += '<th>Ngày Báo Cáo</th>';
            html += '<th>KTV</th>';
            html += '<th>Hình Ảnh</th>';
            html += '<th>Thao Tác</th>';
            html += '</tr></thead><tbody>';

            issues.forEach(function(issue) {
                var severityBadge = self.getSeverityBadge(issue.severity);
                var statusBadge = self.getIssueStatusBadge(issue.status);
                var photosCount = issue.photos && issue.photos.length > 0 ? issue.photos.length : 0;

                html += '<tr>';
                html += '<td>' + (issue.issueName || '') + '</td>';
                html += '<td>' + (issue.category || '') + '</td>';
                html += '<td>' + severityBadge + '</td>';
                html += '<td>' + statusBadge + '</td>';
                html += '<td>' + (issue.reportedDate ? new Date(issue.reportedDate).toLocaleDateString('vi-VN') : '') + '</td>';
                html += '<td>' + (issue.reportedByEmployee?.name || '') + '</td>';
                html += '<td>' + (photosCount > 0 ? '<span class="badge badge-info">' + photosCount + ' ảnh</span>' : '<span class="text-muted">Không có</span>') + '</td>';
                html += '<td>';
                html += '<button class="btn btn-sm btn-info mr-1" onclick="OrderManagement.viewAdditionalIssue(' + issue.id + ')" title="Xem chi tiết">';
                html += '<i class="fas fa-eye"></i></button>';
                
                // ✅ 2.3.3: Nút "Tạo Báo Giá" - chỉ hiển thị khi status = "Identified" hoặc "Reported" và chưa có báo giá
                if ((issue.status === 'Identified' || issue.status === 'Reported') && !issue.additionalQuotationId) {
                    html += '<button class="btn btn-sm btn-success mr-1" onclick="OrderManagement.openCreateQuotationModal(' + issue.id + ')" title="Tạo Báo Giá">';
                    html += '<i class="fas fa-file-invoice-dollar"></i></button>';
                }
                
                // Hiển thị link đến báo giá nếu đã có
                if (issue.additionalQuotationId) {
                    html += '<a href="/QuotationManagement?quotationId=' + issue.additionalQuotationId + '" class="btn btn-sm btn-primary mr-1" title="Xem Báo Giá">';
                    html += '<i class="fas fa-file-alt"></i></a>';
                }
                
                html += '<button class="btn btn-sm btn-warning mr-1" onclick="OrderManagement.editAdditionalIssue(' + issue.id + ')" title="Sửa">';
                html += '<i class="fas fa-edit"></i></button>';
                html += '<button class="btn btn-sm btn-danger" onclick="OrderManagement.deleteAdditionalIssue(' + issue.id + ')" title="Xóa">';
                html += '<i class="fas fa-trash"></i></button>';
                html += '</td>';
                html += '</tr>';
            });

            html += '</tbody></table></div>';
        }

        $('#additionalIssuesList').html(html);

        // Update badge count
        var count = issues ? issues.length : 0;
        if (count > 0) {
            $('#additionalIssuesBadge').text(count).show();
        } else {
            $('#additionalIssuesBadge').hide();
        }
    },

    // Get severity badge
    getSeverityBadge: function(severity) {
        var badges = {
            'Critical': '<span class="badge badge-danger">Khẩn cấp</span>',
            'High': '<span class="badge badge-warning">Cao</span>',
            'Medium': '<span class="badge badge-info">Bình thường</span>',
            'Low': '<span class="badge badge-secondary">Thấp</span>'
        };
        return badges[severity] || '<span class="badge badge-secondary">' + severity + '</span>';
    },

    // Get issue status badge
    getIssueStatusBadge: function(status) {
        var badges = {
            'Identified': '<span class="badge badge-primary">Mới phát hiện</span>',
            'Reported': '<span class="badge badge-info">Đã báo cáo</span>',
            'Quoted': '<span class="badge badge-warning">Đã báo giá</span>',
            'Approved': '<span class="badge badge-success">Đã duyệt</span>',
            'Rejected': '<span class="badge badge-danger">Từ chối</span>',
            'Repaired': '<span class="badge badge-success">Đã sửa</span>'
        };
        return badges[status] || '<span class="badge badge-secondary">' + status + '</span>';
    },

    // Open report additional issue modal
    openReportAdditionalIssueModal: function(serviceOrderId, serviceOrderItemId) {
        var self = this;
        
        // Reset form
        $('#reportAdditionalIssueForm')[0].reset();
        $('#reportIssueId').val('');
        $('#reportIssueServiceOrderId').val(serviceOrderId);
        $('#reportIssueServiceOrderItemId').val(serviceOrderItemId || '');
        $('#reportIssuePhotosPreview').empty();
        $('#reportIssueExistingPhotos').empty();

        // Load service order items for dropdown
        self.loadServiceOrderItemsForReport(serviceOrderId);

        // Preview photos on change
        $('#reportIssuePhotos').off('change').on('change', function(e) {
            self.previewPhotos(e.target.files, '#reportIssuePhotosPreview');
        });

        $('#reportAdditionalIssueModal').modal('show');
    },

    // Load service order items for report dropdown
    loadServiceOrderItemsForReport: function(serviceOrderId) {
        var self = this;
        
        $.ajax({
            url: '/OrderManagement/GetOrder/' + serviceOrderId,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response) && response.success && response.data) {
                    var order = response.data;
                    var $select = $('#reportIssueServiceOrderItemId');
                    
                    // Clear existing options except the first one
                    $select.find('option:not(:first)').remove();

                    if (order.serviceOrderItems && order.serviceOrderItems.length > 0) {
                        order.serviceOrderItems.forEach(function(item) {
                            var optionText = (item.serviceName || 'Hạng mục #' + item.id) + 
                                           ' - SL: ' + item.quantity + 
                                           ' - Trạng thái: ' + (item.status || 'Pending');
                            $select.append('<option value="' + item.id + '">' + optionText + '</option>');
                        });
                    }
                }
            }
        });
    },

    // Preview photos before upload
    previewPhotos: function(files, containerSelector) {
        var container = $(containerSelector);
        container.empty();

        if (!files || files.length === 0) return;

        var allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
        var maxSize = 5 * 1024 * 1024; // 5MB

        Array.from(files).forEach(function(file) {
            if (!allowedTypes.includes(file.type)) {
                GarageApp.showError('File ' + file.name + ' không phải là ảnh!');
                return;
            }

            if (file.size > maxSize) {
                GarageApp.showError('File ' + file.name + ' vượt quá 5MB!');
                return;
            }

            var reader = new FileReader();
            reader.onload = function(e) {
                var img = $('<img>').attr('src', e.target.result)
                    .css({ 'max-width': '100px', 'max-height': '100px', 'margin': '5px', 'border': '1px solid #ddd', 'padding': '2px' });
                container.append(img);
            };
            reader.readAsDataURL(file);
        });
    },

    // View additional issue details
    viewAdditionalIssue: function(issueId) {
        var self = this;
        
        $.ajax({
            url: '/AdditionalIssues/GetById/' + issueId,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response) && response.success && response.data) {
                    var issue = response.data;
                    var message = '<strong>Tên:</strong> ' + issue.issueName + '<br>';
                    message += '<strong>Danh mục:</strong> ' + issue.category + '<br>';
                    message += '<strong>Mức độ:</strong> ' + self.getSeverityBadge(issue.severity) + '<br>';
                    message += '<strong>Mô tả:</strong> ' + (issue.description || '') + '<br>';
                    message += '<strong>Ngày báo cáo:</strong> ' + (issue.reportedDate ? new Date(issue.reportedDate).toLocaleDateString('vi-VN') : '') + '<br>';
                    message += '<strong>KTV:</strong> ' + (issue.reportedByEmployee?.name || '') + '<br>';
                    
                    if (issue.photos && issue.photos.length > 0) {
                        message += '<strong>Hình ảnh:</strong><br>';
                        issue.photos.forEach(function(photo) {
                            message += '<img src="/' + photo.filePath + '" style="max-width: 200px; margin: 5px;"><br>';
                        });
                    }

                    Swal.fire({
                        title: 'Chi Tiết Phát Sinh',
                        html: message,
                        icon: 'info',
                        width: '800px'
                    });
                } else {
                    GarageApp.showError('Lỗi khi tải chi tiết phát sinh');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải chi tiết phát sinh');
                }
            }
        });
    },

    // Edit additional issue
    editAdditionalIssue: function(issueId) {
        var self = this;
        
        $.ajax({
            url: '/AdditionalIssues/GetById/' + issueId,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response) && response.success && response.data) {
                    var issue = response.data;
                    
                    // Populate form
                    $('#reportIssueId').val(issue.id);
                    $('#reportIssueServiceOrderId').val(issue.serviceOrderId);
                    $('#reportIssueServiceOrderItemId').val(issue.serviceOrderItemId || '');
                    $('#reportIssueCategory').val(issue.category);
                    $('#reportIssueName').val(issue.issueName);
                    $('#reportIssueDescription').val(issue.description);
                    $('#reportIssueSeverity').val(issue.severity);
                    $('#reportIssueRequiresImmediateAction').prop('checked', issue.requiresImmediateAction || false);
                    $('#reportIssueTechnicianNotes').val(issue.technicianNotes || '');

                    // Load existing photos
                    self.renderExistingPhotos(issue.photos || []);

                    // Load service order items
                    self.loadServiceOrderItemsForReport(issue.serviceOrderId);

                    // Preview photos on change
                    $('#reportIssuePhotos').off('change').on('change', function(e) {
                        self.previewPhotos(e.target.files, '#reportIssuePhotosPreview');
                    });

                    $('#reportAdditionalIssueModal').modal('show');
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin phát sinh');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin phát sinh');
                }
            }
        });
    },

    // Render existing photos for edit
    renderExistingPhotos: function(photos) {
        var container = $('#reportIssueExistingPhotos');
        container.empty();

        if (!photos || photos.length === 0) return;

        var html = '<div class="mb-2"><strong>Ảnh hiện có:</strong></div>';
        photos.forEach(function(photo) {
            html += '<div class="d-inline-block mr-2 mb-2">';
            html += '<img src="/' + photo.filePath + '" style="max-width: 100px; max-height: 100px; border: 1px solid #ddd; padding: 2px;">';
            html += '<button type="button" class="btn btn-sm btn-danger btn-block mt-1" onclick="OrderManagement.markPhotoForDelete(' + photo.id + ')">';
            html += '<i class="fas fa-trash"></i> Xóa</button>';
            html += '<input type="hidden" id="photoToDelete_' + photo.id + '" value="' + photo.id + '">';
            html += '</div>';
        });

        container.html(html);
    },

    // Mark photo for delete
    markPhotoForDelete: function(photoId) {
        $('#photoToDelete_' + photoId).closest('div').remove();
    },

    // Delete additional issue
    deleteAdditionalIssue: function(issueId) {
        var self = this;
        
        Swal.fire({
            title: 'Xác nhận xóa?',
            text: 'Bạn có chắc chắn muốn xóa phát sinh này?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/AdditionalIssues/Delete/' + issueId,
                    type: 'DELETE',
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Đã xóa phát sinh thành công');
                                if (self.currentServiceOrderId) {
                                    self.loadAdditionalIssues(self.currentServiceOrderId);
                                }
                            } else {
                                GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi xóa phát sinh');
                            }
                        }
                    },
                    error: function(xhr) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi xóa phát sinh');
                        }
                    }
                });
            }
        });
    },

    // ✅ 2.3.3: Open modal to create quotation from additional issue
    openCreateQuotationModal: function(issueId) {
        var self = this;
        
        // Reset form và container
        $('#createQuotationFromIssueForm')[0].reset();
        $('#createQuotationItemsContainer').empty();
        $('#createQuotationDiscountAmount').val(0);
        
        // Set default date (7 days from now)
        var defaultDate = new Date();
        defaultDate.setDate(defaultDate.getDate() + 7);
        $('#createQuotationValidUntil').val(defaultDate.toISOString().split('T')[0]);
        
        // Load issue details first
        $.ajax({
            url: '/AdditionalIssues/GetById/' + issueId,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response) && response.success && response.data) {
                    var issue = response.data;
                    
                    // Store issue ID
                    $('#createQuotationFromIssueModal').data('issue-id', issueId);
                    $('#createQuotationFromIssueModal').data('issue', issue);
                    
                    // Populate modal with issue info
                    $('#createQuotationIssueInfo').html(
                        '<strong>Phát sinh:</strong> ' + issue.issueName + '<br>' +
                        '<strong>Danh mục:</strong> ' + issue.category + '<br>' +
                        '<strong>Mô tả:</strong> ' + issue.description
                    );
                    
                    // Show modal
                    $('#createQuotationFromIssueModal').modal('show');
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin phát sinh');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin phát sinh');
                }
            }
        });
    },

    // ✅ 2.3.3: Add item to quotation from issue
    addQuotationItemFromIssue: function() {
        var self = this;
        var itemIndex = $('#createQuotationItemsContainer .quotation-item-card').length;
        var cardId = 'quotation-item-' + itemIndex;
        
        var cardHtml = `
            <div class="quotation-item-card card mb-3" data-item-index="${itemIndex}">
                <div class="card-header bg-light">
                    <h6 class="mb-0"><i class="fas fa-box"></i> Item #${itemIndex + 1}</h6>
                </div>
                <div class="card-body">
                    <div class="row">
                        <!-- Thông tin cơ bản -->
                        <div class="col-md-6">
                            <div class="form-group mb-2">
                                <label class="small font-weight-bold">Tên Dịch Vụ/Phụ Tùng <span class="text-danger">*</span></label>
                                <input type="text" class="form-control form-control-sm item-name service-typeahead" 
                                       placeholder="Gõ tên để tìm kiếm..." required autocomplete="off">
                                <input type="hidden" class="item-service-id" value="">
                                <input type="hidden" class="item-part-id" value="">
                                <input type="hidden" class="item-service-type" value="">
                            </div>
                            <div class="form-group mb-2">
                                <label class="small">Mô Tả</label>
                                <textarea class="form-control form-control-sm item-description" rows="2" placeholder="Mô tả chi tiết (tùy chọn)"></textarea>
                            </div>
                        </div>
                        
                        <!-- Thông tin số lượng và giá -->
                        <div class="col-md-6">
                            <div class="row">
                                <div class="col-md-6">
                                    <div class="form-group mb-2">
                                        <label class="small font-weight-bold">Số Lượng <span class="text-danger">*</span></label>
                                        <input type="number" class="form-control form-control-sm item-quantity" value="1" min="1" required>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="form-group mb-2">
                                        <label class="small font-weight-bold">Đơn Giá (VNĐ) <span class="text-danger">*</span></label>
                                        <input type="number" class="form-control form-control-sm item-unit-price" step="0.01" min="0" placeholder="0" required>
                                    </div>
                                </div>
                            </div>
                            
                            <!-- VAT Settings -->
                            <div class="row">
                                <div class="col-md-6">
                                    <div class="form-group mb-2">
                                        <label class="small font-weight-bold">VAT (%)</label>
                                        <input type="number" class="form-control form-control-sm item-vat-rate" value="0" min="0" max="100" step="0.01" placeholder="0">
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="form-group mb-2">
                                        <label class="small font-weight-bold">Có Hóa Đơn</label>
                                        <div class="form-check mt-2">
                                            <input class="form-check-input item-has-invoice" type="checkbox" id="hasInvoice_${itemIndex}">
                                            <label class="form-check-label small" for="hasInvoice_${itemIndex}">
                                                Có hóa đơn VAT
                                            </label>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <!-- Tính toán tự động -->
                    <div class="row mt-2 pt-2 border-top">
                        <div class="col-md-10">
                            <div class="alert alert-light mb-0">
                                <div class="row text-center">
                                    <div class="col-md-4">
                                        <label class="small text-muted mb-0">Tạm Tính</label>
                                        <div class="h5 mb-0 text-primary item-subtotal-display">0 VNĐ</div>
                                        <input type="hidden" class="item-subtotal" value="0">
                                    </div>
                                    <div class="col-md-4">
                                        <label class="small text-muted mb-0">VAT</label>
                                        <div class="h5 mb-0 text-info item-vat-amount-display">0 VNĐ</div>
                                        <input type="hidden" class="item-vat-amount" value="0">
                                    </div>
                                    <div class="col-md-4">
                                        <label class="small text-muted mb-0">Thành Tiền</label>
                                        <div class="h5 mb-0 text-success font-weight-bold item-total-display">0 VNĐ</div>
                                        <input type="hidden" class="item-total" value="0">
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-2 d-flex align-items-end">
                            <button type="button" class="btn btn-sm btn-outline-danger w-100" onclick="OrderManagement.removeQuotationItemFromIssue(this)">
                                <i class="fas fa-trash"></i> Xóa
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        $('#createQuotationItemsContainer').append(cardHtml);
        
        // Initialize typeahead for service/part search
        var card = $('#createQuotationItemsContainer .quotation-item-card').last();
        var typeaheadInput = card.find('.service-typeahead');
        self.initializeItemTypeahead(typeaheadInput, card);
        
        // Bind events for calculation
        card.find('.item-quantity, .item-unit-price, .item-vat-rate').on('change input', function() {
            self.calculateQuotationItemFromIssue(card);
        });
        
        // ✅ FIX: Xử lý checkbox "Có hóa đơn" - reset VAT khi tắt
        card.find('.item-has-invoice').on('change', function() {
            var isChecked = $(this).is(':checked');
            var vatRateInput = card.find('.item-vat-rate');
            var isPartItem = card.find('.item-service-type').val() === 'parts';
            var partId = card.find('.item-part-id').val();
            
            if (!isChecked) {
                // Tắt checkbox: Lưu VAT rate hiện tại vào data attribute và set về 0
                var currentVATRate = parseFloat(vatRateInput.val()) || 0;
                if (currentVATRate > 0) {
                    vatRateInput.data('original-vat-rate', currentVATRate);
                    // Lưu trạng thái readonly nếu có
                    if (vatRateInput.prop('readonly')) {
                        vatRateInput.data('was-readonly', true);
                    }
                }
                vatRateInput.val(0).prop('readonly', false);
                vatRateInput.removeClass('bg-light text-muted');
                vatRateInput.attr('title', 'VAT (%)');
            } else {
                // Bật checkbox: Khôi phục VAT rate
                var originalVATRate = vatRateInput.data('original-vat-rate');
                var wasReadonly = vatRateInput.data('was-readonly');
                
                if (originalVATRate && originalVATRate > 0) {
                    vatRateInput.val(originalVATRate);
                    
                    // Nếu là Part item và trước đó là readonly, khôi phục readonly
                    if (isPartItem && partId && wasReadonly) {
                        vatRateInput.prop('readonly', true).addClass('bg-light text-muted');
                        vatRateInput.attr('title', `VAT từ phụ tùng: ${originalVATRate}% (Không được chỉnh sửa)`);
                    } else {
                        vatRateInput.prop('readonly', false).removeClass('bg-light text-muted');
                        vatRateInput.attr('title', 'VAT (%)');
                    }
                } else if (parseFloat(vatRateInput.val()) === 0) {
                    // Nếu không có VAT rate đã lưu, set mặc định 10%
                    vatRateInput.val(10);
                    vatRateInput.prop('readonly', false).removeClass('bg-light text-muted');
                    vatRateInput.attr('title', 'VAT (%)');
                }
            }
            
            // Recalculate
            self.calculateQuotationItemFromIssue(card);
        });
    },

    // ✅ 2.3.3: Initialize typeahead for service/part search (combines both Services and Parts)
    initializeItemTypeahead: function(input, card) {
        var self = this;
        
        input.typeahead({
            source: function(query, process) {
                // Search both Services and Parts
                var results = [];
                var partsDone = false;
                var servicesDone = false;
                
                // Search Parts
                $.ajax({
                    url: '/PartsManagement/SearchParts',
                    type: 'GET',
                    data: { searchTerm: query },
                    success: function(response) {
                        if (response && response.success && response.data) {
                            response.data.forEach(function(part) {
                                results.push({
                                    id: part.id,
                                    text: `${part.partName} (${part.partNumber}) - ${part.sellPrice.toLocaleString('vi-VN')} VNĐ`,
                                    name: part.partName,
                                    type: 'part',
                                    partId: part.id,
                                    sellPrice: part.sellPrice,
                                    vatRate: part.vatRate || 10,
                                    hasInvoice: part.hasInvoice !== false,
                                    isVATApplicable: part.isVATApplicable !== false
                                });
                            });
                        }
                        partsDone = true;
                        if (servicesDone) process(results);
                    },
                    error: function() {
                        partsDone = true;
                        if (servicesDone) process(results);
                    }
                });
                
                // Search Services
                $.ajax({
                    url: '/ServiceManagement/SearchServices',
                    type: 'GET',
                    data: { q: query },
                    success: function(response) {
                        if (response && Array.isArray(response)) {
                            response.forEach(function(service) {
                                results.push({
                                    id: service.value,
                                    text: service.text,
                                    name: service.text,
                                    type: 'service',
                                    serviceId: service.value,
                                    sellPrice: service.price || 0,
                                    vatRate: service.vatRate || 0, // ✅ FIX: Lấy VAT từ Service, mặc định 0 nếu không có
                                    hasInvoice: service.isVATApplicable !== false, // ✅ FIX: Lấy từ Service
                                    isVATApplicable: service.isVATApplicable !== false // ✅ FIX: Lấy từ Service
                                });
                            });
                        }
                        servicesDone = true;
                        if (partsDone) process(results);
                    },
                    error: function() {
                        servicesDone = true;
                        if (partsDone) process(results);
                    }
                });
            },
            displayText: function(item) {
                return item.text;
            },
            afterSelect: function(item) {
                var quantity = parseFloat(card.find('.item-quantity').val()) || 1;
                var price = item.sellPrice || 0;
                
                // Set name
                card.find('.item-name').val(item.name);
                
                // Set IDs based on type
                if (item.type === 'part') {
                    card.find('.item-part-id').val(item.partId);
                    card.find('.item-service-id').val('');
                    card.find('.item-service-type').val('parts');
                } else {
                    card.find('.item-service-id').val(item.serviceId);
                    card.find('.item-part-id').val('');
                    card.find('.item-service-type').val('service');
                }
                
                // Set price
                card.find('.item-unit-price').val(price);
                
                // Set VAT settings
                if (item.type === 'part') {
                    // For parts, set VAT from part data
                    card.find('.item-vat-rate').val(item.vatRate);
                    card.find('.item-has-invoice').prop('checked', item.hasInvoice);
                    
                    // Lưu VAT rate vào data attribute để khôi phục sau
                    card.find('.item-vat-rate').data('original-vat-rate', item.vatRate);
                    
                    // If part has VAT, make VAT rate readonly
                    if (item.hasInvoice && item.vatRate > 0) {
                        card.find('.item-vat-rate').prop('readonly', true).addClass('bg-light text-muted');
                        card.find('.item-vat-rate').attr('title', `VAT từ phụ tùng: ${item.vatRate}% (Không được chỉnh sửa)`);
                    } else {
                        card.find('.item-vat-rate').prop('readonly', false).removeClass('bg-light text-muted');
                        card.find('.item-vat-rate').attr('title', 'VAT (%)');
                    }
                } else {
                    // For services, set VAT from service data
                    var vatRate = item.vatRate || 0; // ✅ FIX: Lấy VAT từ Service, mặc định 0
                    card.find('.item-vat-rate').val(vatRate);
                    card.find('.item-has-invoice').prop('checked', item.hasInvoice || false);
                    card.find('.item-vat-rate').prop('readonly', false).removeClass('bg-light text-muted');
                    card.find('.item-vat-rate').attr('title', 'VAT (%)');
                    // Lưu VAT rate vào data attribute
                    card.find('.item-vat-rate').data('original-vat-rate', vatRate);
                }
                
                // Recalculate totals
                self.calculateQuotationItemFromIssue(card);
            },
            delay: 300
        });
    },

    // ✅ 2.3.3: Remove item from quotation
    removeQuotationItemFromIssue: function(button) {
        var self = this;
        $(button).closest('.quotation-item-card').fadeOut(300, function() {
            $(this).remove();
            self.calculateQuotationTotalFromIssue();
        });
    },

    // ✅ 2.3.3: Calculate item totals
    calculateQuotationItemFromIssue: function(card) {
        var self = this;
        var quantity = parseFloat(card.find('.item-quantity').val()) || 0;
        var unitPrice = parseFloat(card.find('.item-unit-price').val()) || 0;
        var hasInvoice = card.find('.item-has-invoice').is(':checked');
        var vatRate = parseFloat(card.find('.item-vat-rate').val()) || 0;
        
        var subtotal = quantity * unitPrice;
        var vatAmount = 0;
        
        if (hasInvoice && vatRate > 0) {
            vatAmount = subtotal * (vatRate / 100);
        }
        
        var total = subtotal + vatAmount;
        
        // Update hidden inputs
        card.find('.item-subtotal').val(subtotal.toFixed(2));
        card.find('.item-vat-amount').val(vatAmount.toFixed(2));
        card.find('.item-total').val(total.toFixed(2));
        
        // Update display
        card.find('.item-subtotal-display').text(subtotal.toLocaleString('vi-VN') + ' VNĐ');
        card.find('.item-vat-amount-display').text(vatAmount.toLocaleString('vi-VN') + ' VNĐ');
        card.find('.item-total-display').text(total.toLocaleString('vi-VN') + ' VNĐ');
        
        self.calculateQuotationTotalFromIssue();
    },

    // ✅ 2.3.3: Calculate quotation total
    calculateQuotationTotalFromIssue: function() {
        var self = this;
        var subtotal = 0;
        var vatAmount = 0;
        
        $('#createQuotationItemsContainer .quotation-item-card').each(function() {
            subtotal += parseFloat($(this).find('.item-subtotal').val()) || 0;
            vatAmount += parseFloat($(this).find('.item-vat-amount').val()) || 0;
        });
        
        var discountAmount = parseFloat($('#createQuotationDiscountAmount').val()) || 0;
        var totalAmount = subtotal + vatAmount - discountAmount;
        
        $('#createQuotationSubtotal').text(subtotal.toLocaleString('vi-VN') + ' VNĐ');
        $('#createQuotationVATAmount').text(vatAmount.toLocaleString('vi-VN') + ' VNĐ');
        $('#createQuotationDiscountDisplay').text(discountAmount.toLocaleString('vi-VN') + ' VNĐ');
        $('#createQuotationTotalAmount').text(totalAmount.toLocaleString('vi-VN') + ' VNĐ');
    },

    // ✅ 2.3.3: Submit create quotation from issue
    submitCreateQuotationFromIssue: function() {
        var self = this;
        var issueId = $('#createQuotationFromIssueModal').data('issue-id');
        
        if (!issueId) {
            GarageApp.showError('Không tìm thấy phát sinh');
            return;
        }
        
        // Validate items
        var items = [];
        $('#createQuotationItemsContainer .quotation-item-card').each(function() {
            var card = $(this);
            var itemName = card.find('.item-name').val();
            var quantity = parseFloat(card.find('.item-quantity').val());
            var unitPrice = parseFloat(card.find('.item-unit-price').val());
            var serviceId = card.find('.item-service-id').val();
            var partId = card.find('.item-part-id').val();
            var serviceType = card.find('.item-service-type').val() || 'service';
            
            if (!itemName || !quantity || !unitPrice) {
                GarageApp.showError('Vui lòng điền đầy đủ thông tin cho tất cả items');
                return false;
            }
            
            items.push({
                itemName: itemName,
                description: card.find('.item-description').val() || '',
                quantity: quantity,
                unitPrice: unitPrice,
                serviceId: serviceId || null,
                partId: partId || null,
                hasInvoice: card.find('.item-has-invoice').is(':checked'),
                isVATApplicable: card.find('.item-has-invoice').is(':checked'),
                vatRate: parseFloat(card.find('.item-vat-rate').val()) || 0,
                serviceType: serviceType,
                itemCategory: serviceType === 'parts' ? 'Material' : 'Service'
            });
        });
        
        if (items.length === 0) {
            GarageApp.showError('Vui lòng thêm ít nhất một item');
            return;
        }
        
        var quotationData = {
            items: items,
            validUntil: $('#createQuotationValidUntil').val() || null,
            description: $('#createQuotationDescription').val() || '',
            terms: $('#createQuotationTerms').val() || '',
            taxRate: 0,
            discountAmount: parseFloat($('#createQuotationDiscountAmount').val()) || 0,
            customerNotes: $('#createQuotationCustomerNotes').val() || ''
        };
        
        $.ajax({
            url: '/AdditionalIssues/CreateQuotation/' + issueId,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(quotationData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Đã tạo báo giá bổ sung từ phát sinh thành công');
                        $('#createQuotationFromIssueModal').modal('hide');
                        
                        // Reload additional issues
                        if (self.currentServiceOrderId) {
                            self.loadAdditionalIssues(self.currentServiceOrderId);
                        }
                        
                        // Redirect to quotation if needed
                        if (response.data && response.data.id) {
                            setTimeout(function() {
                                window.location.href = '/QuotationManagement?quotationId=' + response.data.id;
                            }, 1500);
                        }
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi tạo báo giá');
                    }
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    var errorMsg = 'Lỗi khi tạo báo giá';
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        errorMsg += ': ' + xhr.responseJSON.message;
                    }
                    GarageApp.showError(errorMsg);
                }
            }
        });
    }
};

// Initialize when document is ready
$(document).ready(function() {
    OrderManagement.init();

    // ✅ 2.3.2: Handle tab click to load additional issues
    $('#additional-issues-tab').on('shown.bs.tab', function(e) {
        var serviceOrderId = OrderManagement.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (serviceOrderId) {
            OrderManagement.loadAdditionalIssues(serviceOrderId);
        } else {
            $('#additionalIssuesList').html('<p class="text-muted">Vui lòng xem chi tiết phiếu sửa chữa trước.</p>');
        }
    });

    // ✅ 2.3.2: Handle report additional issue button click
    $('#btnReportAdditionalIssue').on('click', function() {
        var serviceOrderId = OrderManagement.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (serviceOrderId) {
            OrderManagement.openReportAdditionalIssueModal(serviceOrderId);
        }
    });

    // ✅ 2.3.2: Handle form submit for additional issue
    $('#reportAdditionalIssueForm').on('submit', function(e) {
        e.preventDefault();
        var self = OrderManagement;
        var formData = new FormData(this);
        var issueId = $('#reportIssueId').val();
        var isEdit = issueId && issueId !== '';

        // Collect photos to delete
        var deletedPhotoIds = [];
        $('input[id^="photoToDelete_"]').each(function() {
            deletedPhotoIds.push(parseInt($(this).val()));
        });
        if (deletedPhotoIds.length > 0) {
            deletedPhotoIds.forEach(function(id) {
                formData.append('DeletedPhotoIds', id);
            });
        }

        var url = isEdit ? '/AdditionalIssues/Update/' + issueId : '/AdditionalIssues/Create';
        var method = isEdit ? 'PUT' : 'POST';

        $.ajax({
            url: url,
            type: method,
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess(response.message || (isEdit ? 'Đã cập nhật phát sinh thành công' : 'Đã báo cáo phát sinh thành công'));
                        $('#reportAdditionalIssueModal').modal('hide');
                        if (self.currentServiceOrderId) {
                            self.loadAdditionalIssues(self.currentServiceOrderId);
                        }
                        // Reload order items to show updated status
                        var serviceOrderId = $('#reportIssueServiceOrderId').val();
                        if (serviceOrderId) {
                            self.viewOrder(serviceOrderId);
                        }
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi lưu phát sinh');
                    }
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi lưu phát sinh');
                }
            }
        });
    });
});