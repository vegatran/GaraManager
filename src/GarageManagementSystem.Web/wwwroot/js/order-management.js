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
        
        // ✅ THÊM: Populate order items với thông tin phân công
        if (order.serviceOrderItems && order.serviceOrderItems.length > 0) {
            var itemsHtml = '';
            order.serviceOrderItems.forEach(function(item) {
                itemsHtml += `
                    <tr>
                        <td>${item.service?.name || item.serviceName || ''}</td>
                        <td>${item.quantity || 1}</td>
                        <td>${(item.unitPrice || 0).toLocaleString()} VNĐ</td>
                        <td>${(item.totalPrice || 0).toLocaleString()} VNĐ</td>
                        <td>${item.assignedTechnicianName || '<span class="text-muted">Chưa phân công</span>'}</td>
                        <td>${item.estimatedHours ? item.estimatedHours + ' giờ' : '<span class="text-muted">-</span>'}</td>
                    </tr>
                `;
            });
            $('#viewOrderItems').html(itemsHtml);
        } else {
            $('#viewOrderItems').html('<tr><td colspan="6" class="text-center text-muted">Không có dịch vụ nào</td></tr>');
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
    }
};

// Initialize when document is ready
$(document).ready(function() {
    OrderManagement.init();
});