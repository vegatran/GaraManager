/**
 * Order Management Module
 * 
 * Handles all order-related operations
 * CRUD operations for service orders
 */

window.OrderManagement = {
    // DataTable instance
    orderTable: null,
    settlementLoadedOrderId: null,
    warrantyLoadedOrderId: null,
    currentWarranty: null,
    feesLoadedOrderId: null,
    currentFeeSummary: null,
    feeTypesCache: [],
    isEditingFees: false,

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

        // ✅ 3.1: Áp dụng phương pháp tính COGS
        $(document).on('click', '#btnApplyCogsMethod', function() {
            self.applyCogsMethod();
        });

        // ✅ 3.1: Tính lại COGS
        $(document).on('click', '#btnRecalculateCogs', function() {
            self.recalculateCogs();
        });

        // ✅ 3.1: Làm mới dữ liệu COGS
        $(document).on('click', '#btnRefreshCogsData', function() {
            self.refreshSettlementData();
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
        // ✅ FIX: Store order status để loadQCInfo có thể check
        $('#viewOrderModal').data('order-status', order.status);
        this.currentServiceOrderId = order.id;
        this.settlementLoadedOrderId = null;
        this.warrantyLoadedOrderId = null;
        this.currentWarranty = null;
        this.feesLoadedOrderId = null;
        this.currentFeeSummary = null;
        this.initializeSettlementSection(order);
        this.resetWarrantySection(false);
        this.resetFeesSection(false);

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
        
        // ✅ 2.3.3: Hiển thị GrandTotalAmount và AdditionalOrdersTotalAmount nếu là JO gốc
        if (!order.isAdditionalOrder && order.additionalOrdersTotalAmount !== undefined) {
            var grandTotalHtml = '<div class="mt-2">';
            grandTotalHtml += '<label class="font-weight-bold text-primary">Tổng Tiền JO Gốc:</label> ';
            grandTotalHtml += '<span class="text-primary font-weight-bold">' + (order.totalAmount ? order.totalAmount.toLocaleString('vi-VN') + ' VNĐ' : '0 VNĐ') + '</span>';
            
            if (order.additionalOrdersTotalAmount > 0) {
                grandTotalHtml += '<br><label class="font-weight-bold text-info">Tổng Tiền LSC Bổ Sung:</label> ';
                grandTotalHtml += '<span class="text-info">' + order.additionalOrdersTotalAmount.toLocaleString('vi-VN') + ' VNĐ</span>';
                grandTotalHtml += '<br><label class="font-weight-bold text-success">Tổng Tiền Cuối Cùng:</label> ';
                grandTotalHtml += '<span class="text-success font-weight-bold">' + (order.grandTotalAmount ? order.grandTotalAmount.toLocaleString('vi-VN') + ' VNĐ' : '0 VNĐ') + '</span>';
            }
            grandTotalHtml += '</div>';
            $('#viewTotalAmount').html(grandTotalHtml);
        }
        
        // ✅ THÊM: Format status sang tiếng Việt
        var statusText = OrderManagement.formatServiceOrderStatus(order.status || '');
        $('#viewStatus').html(statusText);
        $('#viewDescription').text(order.description || '');
        
        // ✅ THÊM: Populate order items với thông tin phân công và tiến độ
        if (order.serviceOrderItems && order.serviceOrderItems.length > 0) {
            var itemsHtml = '';
            var self = this;
            var hasPartsItems = false; // ✅ 2.3.3: Check if order has parts items
            
            // ✅ HP3: Tính tổng EstimatedHours và ActualHours
            var totalEstimatedHours = 0;
            var totalActualHours = 0;
            var totalReworkHours = 0;
            
            order.serviceOrderItems.forEach(function(item) {
                // Tính tổng EstimatedHours
                if (item.estimatedHours) {
                    totalEstimatedHours += parseFloat(item.estimatedHours);
                }
                
                // Tính tổng ActualHours
                if (item.actualHours) {
                    totalActualHours += parseFloat(item.actualHours);
                }
                
                // Tính tổng ReworkHours
                if (item.reworkHours) {
                    totalReworkHours += parseFloat(item.reworkHours);
                }
                
                var statusBadge = self.formatItemStatus(item.status);
                var actualHoursDisplay = item.actualHours ? 
                    item.actualHours.toFixed(2) + ' giờ' : 
                    '<span class="text-muted">-</span>';
                
                // ✅ 2.4.3: Hiển thị giờ công làm lại
                var reworkHoursDisplay = item.reworkHours ? 
                    item.reworkHours.toFixed(2) + ' giờ' : 
                    '<span class="text-muted">-</span>';
                
                // ✅ 2.3.3: Check if item is a part (has PartId or ServiceType is 'parts')
                if (item.partId || item.serviceType === 'parts' || item.itemCategory === 'Material') {
                    hasPartsItems = true;
                }
                
                // ✅ 2.3.1: Thêm nút Start/Stop/Complete dựa trên status
                var actionButtons = self.getItemActionButtons(order.id, item.id, item.status, item.startTime, item.completedTime, order.status);
                
                // ✅ 2.4.3: Thêm button "Ghi Nhận Giờ Công Làm Lại" nếu QC Fail và item đã Completed/InProgress
                var showReworkButton = false;
                if (order.status === 'InProgress' && (item.status === 'Completed' || item.status === 'InProgress')) {
                    // Check if latest QC is Fail
                    // This will be checked when loading QC info
                    showReworkButton = true; // Will be updated after QC info is loaded
                }
                
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
                        <td>${reworkHoursDisplay}</td>
                        <td>${actionButtons}</td>
                    </tr>
                `;
            });

            if (order.serviceOrderParts && order.serviceOrderParts.length > 0) {
                order.serviceOrderParts.forEach(function(part) {
                    var displayName = part.partName || part.notes || 'Phụ tùng';
                    itemsHtml += `
                        <tr data-part-id="${part.id}">
                            <td><strong>${displayName}</strong></td>
                            <td>${part.quantity || 1}</td>
                            <td>${DataTablesUtility.renderCurrency(part.unitPrice)}</td>
                            <td>${DataTablesUtility.renderCurrency(part.totalPrice)}</td>
                            <td><span class="text-muted">-</span></td>
                            <td><span class="text-muted">-</span></td>
                            <td>${self.formatItemStatus(part.status)}</td>
                            <td><span class="text-muted">-</span></td>
                            <td><span class="text-muted">-</span></td>
                            <td><span class="text-muted">-</span></td>
                        </tr>
                    `;
                });
            }
            $('#viewOrderItems').html(itemsHtml);
            
            // ✅ HP3: Hiển thị tổng giờ công và progress indicator
            self.renderHoursSummary(totalEstimatedHours, totalActualHours, totalReworkHours);
            
            // ✅ 2.3.3: Hiển thị button "Tạo MR" nếu có parts items và chưa có MR
            if (hasPartsItems && !order.isAdditionalOrder) {
                $('#btnCreateMRFromOrder').show().off('click').on('click', function() {
                    window.location.href = '/MaterialRequestManagement?serviceOrderId=' + order.id;
                });
        } else {
                $('#btnCreateMRFromOrder').hide();
            }
            
            // ✅ 2.4: Show/Hide QC buttons based on status
            self.updateQCButtons(order);
            
            // ✅ 2.3.1: Bind event handlers cho các nút action
            self.bindItemActionEvents(order.id);
            
            // ✅ 2.4.3: Check QC Fail và hiển thị button "Ghi Nhận Giờ Công Làm Lại"
            if (order.status === 'InProgress') {
                self.checkAndShowReworkButtons(order.id, order.serviceOrderItems || []);
            }
        } else {
            $('#viewOrderItems').html('<tr><td colspan="10" class="text-center text-muted">Không có dịch vụ nào</td></tr>');
            $('#btnCreateMRFromOrder').hide();
            // ✅ 2.4: Hide all QC buttons if no items
            $('#btnCompleteTechnical, #btnStartQC, #btnCompleteQC, #btnHandover').hide();
            // ✅ HP3: Hide hours summary nếu không có items
            $('#hoursSummary').hide();
        }
    },

    // ✅ HP3: Hiển thị tổng giờ công và progress indicator
    renderHoursSummary: function(totalEstimatedHours, totalActualHours, totalReworkHours) {
        // Hiển thị summary card
        $('#hoursSummary').show();
        
        // Tính toán giờ công còn lại
        var remainingHours = Math.max(0, totalEstimatedHours - totalActualHours);
        
        // Tính tỷ lệ hoàn thành (dựa trên ActualHours / EstimatedHours)
        var progressPercentage = 0;
        if (totalEstimatedHours > 0) {
            progressPercentage = Math.min(100, (totalActualHours / totalEstimatedHours) * 100);
        }
        
        // Hiển thị giá trị
        $('#totalEstimatedHours').text(totalEstimatedHours.toFixed(2));
        $('#totalActualHours').text(totalActualHours.toFixed(2));
        $('#remainingHours').text(remainingHours.toFixed(2));
        $('#progressPercentage').text(progressPercentage.toFixed(0) + '%');
        
        // Cập nhật progress bar
        var progressBar = $('#hoursProgressBar');
        progressBar.css('width', progressPercentage + '%');
        
        // Đổi màu progress bar dựa trên tỷ lệ
        progressBar.removeClass('bg-success bg-warning bg-danger');
        if (progressPercentage <= 100) {
            progressBar.addClass('bg-success');
        } else if (progressPercentage <= 150) {
            progressBar.addClass('bg-warning');
        } else {
            progressBar.addClass('bg-danger');
        }
        
        // Hiển thị warning nếu ActualHours > EstimatedHours * 1.5 (vượt quá 50%)
        if (totalEstimatedHours > 0 && totalActualHours > totalEstimatedHours * 1.5) {
            $('#hoursWarning').show();
            
            // Cập nhật text warning với chi tiết
            var excessPercentage = ((totalActualHours / totalEstimatedHours - 1) * 100).toFixed(0);
            var excessHours = (totalActualHours - totalEstimatedHours).toFixed(2);
            $('#hoursWarning').html(
                '<i class="fas fa-exclamation-triangle mr-2"></i>' +
                '<strong>Cảnh báo:</strong> Giờ công thực tế vượt quá ' + excessPercentage + '% so với dự kiến ' +
                '(vượt ' + excessHours + ' giờ). Giờ công thực tế: ' + totalActualHours.toFixed(2) + ' giờ, ' +
                'Dự kiến: ' + totalEstimatedHours.toFixed(2) + ' giờ.'
            );
        } else {
            $('#hoursWarning').hide();
        }
    },

    // ✅ 3.1: Khởi tạo giao diện Quyết toán
    initializeSettlementSection: function(order) {
        if (!$('#settlement').length) {
            return;
        }

        var method = this.normalizeCogsMethod(order ? (order.cogsCalculationMethod || order.cogsMethod) : null);
        var totalCogs = order ? (order.totalCogs ?? order.totalCOGS ?? 0) : 0;
        var totalRevenue = order ? (order.finalAmount ?? order.totalAmount ?? 0) : 0;

        $('#cogsMethodSelect').val(method);
        $('#cogsMethodDisplay').text(this.getCogsMethodDisplay(method));
        $('#cogsCalculationDate').text(this.formatDateTime(order ? order.cogsCalculationDate : null));
        $('#totalCogsDisplay').text(this.formatCurrency(totalCogs));
        $('#grossTotalCogsDisplay').text(this.formatCurrency(totalCogs));
        $('#totalRevenueDisplay').text(this.formatCurrency(totalRevenue));
        $('#grossProfitDisplay').text('-');
        $('#grossProfitMarginDisplay').text('-');
        $('#grossProfitNote').text('');
        $('#cogsLastUpdatedNote').text('');
        $('#settlementEmptyMessage').text('Chưa có dữ liệu giá vốn cho phiếu sửa chữa này. Vui lòng tính COGS trước.');

        this.resetSettlementSection(false);
    },

    // ✅ 3.1: Chuẩn hóa phương pháp tính COGS
    normalizeCogsMethod: function(method) {
        if (!method) {
            return 'FIFO';
        }

        var normalized = method.toString().replace(/[_\s]/g, '').toUpperCase();
        if (normalized === 'WEIGHTEDAVERAGE' || normalized === 'AVERAGE') {
            return 'WeightedAverage';
        }
        return 'FIFO';
    },

    // ✅ 3.1: Hiển thị phương pháp tính COGS thân thiện
    getCogsMethodDisplay: function(method) {
        var normalized = this.normalizeCogsMethod(method);
        if (normalized === 'WeightedAverage') {
            return 'Bình quân gia quyền';
        }
        return 'FIFO (Nhập trước - Xuất trước)';
    },

    // ✅ 3.1: Format tiền tệ
    formatCurrency: function(amount) {
        var parsed = Number(amount);
        if (!isFinite(parsed)) {
            parsed = 0;
        }
        return parsed.toLocaleString('vi-VN') + ' VNĐ';
    },

    // ✅ 3.1: Format ngày giờ
    formatDateTime: function(dateValue) {
        if (!dateValue) {
            return '-';
        }
        var date = new Date(dateValue);
        if (isNaN(date.getTime())) {
            return '-';
        }
        return date.toLocaleString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    },

    // ✅ 3.1: Reset trạng thái tab Quyết toán
    resetSettlementSection: function(showSpinner) {
        if (!$('#settlement').length) {
            return;
        }

        if (showSpinner === undefined || showSpinner) {
            $('#settlementLoading').removeClass('d-none');
        } else {
            $('#settlementLoading').addClass('d-none');
        }

        $('#settlementContent').addClass('d-none');
        $('#settlementEmpty').addClass('d-none');
        $('#settlementError').addClass('d-none');
    },

    // ✅ 3.1: Tải dữ liệu Quyết toán
    loadSettlementData: function(serviceOrderId, forceRefresh) {
        var self = this;

        if (!$('#settlement').length) {
            return;
        }

        if (!serviceOrderId) {
            GarageApp.showError('Không xác định được phiếu sửa chữa để tải dữ liệu Quyết toán.');
            return;
        }

        if (!forceRefresh && self.settlementLoadedOrderId === serviceOrderId) {
            $('#settlementLoading').addClass('d-none');
            $('#settlementContent').removeClass('d-none');
            return;
        }

        self.resetSettlementSection(true);

        self.fetchOrderCogs(serviceOrderId)
            .done(function(response) {
                if (!AuthHandler.validateApiResponse(response)) {
                    return;
                }

                if (response.success) {
                    var cogsData = response.data || {};
                    self.updateSettlementSummary(cogsData);
                    self.renderCogsBreakdown(cogsData.itemDetails || []);

                    $('#settlementLoading').addClass('d-none');
                    $('#settlementEmpty').addClass('d-none');
                    $('#settlementContent').removeClass('d-none');

                    self.settlementLoadedOrderId = serviceOrderId;

                    self.fetchGrossProfit(serviceOrderId)
                        .done(function(grossResponse) {
                            if (!AuthHandler.validateApiResponse(grossResponse)) {
                                return;
                            }

                            if (grossResponse.success) {
                                self.updateGrossProfitSummary(grossResponse.data);
                            } else {
                                self.updateGrossProfitSummary(null);
                                if (grossResponse.error) {
                                    $('#grossProfitNote').text(grossResponse.error);
                                }
                            }
                        })
                        .fail(function(xhr) {
                            if (AuthHandler.isUnauthorized(xhr)) {
                                AuthHandler.handleUnauthorized(xhr, true);
                                return;
                            }
                            self.updateGrossProfitSummary(null);
                            $('#grossProfitNote').text('Không thể lấy lợi nhuận gộp hiện tại.');
                        });
                } else {
                    self.settlementLoadedOrderId = null;
                    $('#settlementLoading').addClass('d-none');

                    var emptyMessage = response.error || response.message || 'Chưa có dữ liệu giá vốn cho phiếu sửa chữa này. Vui lòng tính COGS.';
                    $('#settlementEmptyMessage').text(emptyMessage);
                    $('#settlementEmpty').removeClass('d-none');
                    self.updateGrossProfitSummary(null);
                }
            })
            .fail(function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = GarageApp.parseErrorMessage ? GarageApp.parseErrorMessage(xhr) : 'Không thể tải dữ liệu Quyết toán.';
                self.handleSettlementError(errorMsg);
            });
    },

    // ✅ 3.1: Lấy chi tiết COGS từ server
    fetchOrderCogs: function(serviceOrderId) {
        return $.ajax({
            url: '/OrderManagement/GetOrderCogs/' + serviceOrderId,
            type: 'GET'
        });
    },

    // ✅ 3.1: Lấy lợi nhuận gộp từ server
    fetchGrossProfit: function(serviceOrderId) {
        return $.ajax({
            url: '/OrderManagement/GetOrderGrossProfit/' + serviceOrderId,
            type: 'GET'
        });
    },

    // ✅ 3.1: Cập nhật thông tin tổng quan COGS
    updateSettlementSummary: function(cogsData) {
        if (!cogsData) {
            return;
        }

        var method = this.normalizeCogsMethod(cogsData.calculationMethod || cogsData.method);
        var totalCogs = cogsData.totalCogs ?? cogsData.totalCOGS ?? 0;

        $('#cogsMethodDisplay').text(this.getCogsMethodDisplay(method));
        $('#cogsMethodSelect').val(method);
        $('#cogsCalculationDate').text(this.formatDateTime(cogsData.calculationDate));
        $('#totalCogsDisplay').text(this.formatCurrency(totalCogs));
        $('#grossTotalCogsDisplay').text(this.formatCurrency(totalCogs));

        if (cogsData.calculationDate) {
            $('#cogsLastUpdatedNote').text('Cập nhật lần cuối: ' + this.formatDateTime(cogsData.calculationDate));
        } else {
            $('#cogsLastUpdatedNote').text('');
        }
    },

    // ✅ 3.1: Hiển thị bảng chi tiết COGS
    renderCogsBreakdown: function(items) {
        var $tbody = $('#cogsBreakdownBody');
        if (!$tbody.length) {
            return;
        }

        if (!items || items.length === 0) {
            $tbody.html('<tr><td colspan="7" class="text-center text-muted py-3">Chưa có dữ liệu giá vốn.</td></tr>');
            return;
        }

        var self = this;
        var rows = items.map(function(item) {
            var quantity = item.quantityUsed ?? 0;
            var quantityValue = Number(quantity);
            if (!isFinite(quantityValue)) {
                quantityValue = 0;
            }
            var quantityDisplay = quantityValue.toLocaleString('vi-VN');
            var unitCostDisplay = self.formatCurrency(item.unitCost ?? 0);
            var totalCostDisplay = self.formatCurrency(item.totalCost ?? 0);
            var batchNumber = item.batchNumber || '-';
            var batchDate = self.formatDateTime(item.batchReceiveDate);
            var partName = item.partName || '';
            var partNumber = item.partNumber || '';

            return (
                '<tr>' +
                '<td>' + partName + '</td>' +
                '<td>' + (partNumber || '-') + '</td>' +
                '<td>' + quantityDisplay + '</td>' +
                '<td>' + unitCostDisplay + '</td>' +
                '<td>' + totalCostDisplay + '</td>' +
                '<td>' + batchNumber + '</td>' +
                '<td>' + (batchDate === '-' ? '-' : batchDate) + '</td>' +
                '</tr>'
            );
        }).join('');

        $tbody.html(rows);
    },

    // ✅ 3.1: Cập nhật thông tin lợi nhuận gộp
    updateGrossProfitSummary: function(grossData) {
        if (!$('#grossProfitDisplay').length) {
            return;
        }

        if (!grossData) {
            $('#grossProfitDisplay').text('-');
            $('#grossProfitMarginDisplay').text('-');
            $('#grossProfitNote').text('');
            return;
        }

        var totalRevenue = grossData.totalRevenue ?? 0;
        var totalCogs = grossData.totalCogs ?? grossData.totalCOGS ?? 0;
        var grossProfit = grossData.grossProfit ?? 0;
        var grossMargin = Number(grossData.grossProfitMargin);

        $('#totalRevenueDisplay').text(this.formatCurrency(totalRevenue));
        $('#grossTotalCogsDisplay').text(this.formatCurrency(totalCogs));
        $('#grossProfitDisplay').text(this.formatCurrency(grossProfit));
        if (isFinite(grossMargin)) {
            $('#grossProfitMarginDisplay').text(grossMargin.toFixed(2) + '%');
        } else {
            $('#grossProfitMarginDisplay').text('-');
        }
        $('#grossProfitNote').text('Được tính dựa trên doanh thu và giá vốn hiện tại của phiếu sửa chữa.');
    },

    // ✅ 3.1: Xử lý lỗi khi tải Quyết toán
    handleSettlementError: function(message) {
        $('#settlementLoading').addClass('d-none');
        $('#settlementContent').addClass('d-none');
        $('#settlementEmpty').addClass('d-none');

        $('#settlementErrorMessage').text(message || 'Không thể tải dữ liệu Quyết toán.');
        $('#settlementError').removeClass('d-none');
    },

    // ✅ 3.2: Reset trạng thái tab bảo hành
    resetWarrantySection: function(showSpinner) {
        if (!$('#warranty').length) {
            return;
        }

        if (showSpinner === undefined || showSpinner) {
            $('#warrantyLoading').removeClass('d-none');
        } else {
            $('#warrantyLoading').addClass('d-none');
        }

        $('#warrantyContent').addClass('d-none');
        $('#warrantyEmpty').addClass('d-none');
        $('#warrantyError').addClass('d-none');
        $('#btnCreateWarrantyClaim').addClass('d-none');
        $('#btnShowCreateClaim').addClass('d-none');

        $('#warrantyCodeDisplay').text('-');
        $('#warrantyStatusBadge').attr('class', 'badge badge-secondary').text('Chưa tạo');
        $('#warrantyStartDate').text('-');
        $('#warrantyEndDate').text('-');
        $('#warrantyCustomerName').text('-');
        $('#warrantyVehiclePlate').text('-');
        $('#warrantyHandoverBy').text('-');
        $('#warrantyHandoverLocation').text('-');
        $('#warrantyItemCount').text('0');
        $('#warrantyClaimCount').text('0');
        $('#warrantyExpiryAlert, #warrantyExpiringAlert, #warrantyExpiredAlert').addClass('d-none');

        $('#warrantyItemsBody').html('<tr><td colspan="6" class="text-center text-muted py-3">Chưa có dữ liệu</td></tr>');
        $('#warrantyClaimsBody').html('<tr><td colspan="5" class="text-center text-muted py-3">Chưa có khiếu nại</td></tr>');
    },

    // ✅ 3.2: Tải dữ liệu bảo hành
    loadWarrantyData: function(serviceOrderId, forceRefresh) {
        var self = this;

        if (!$('#warranty').length) {
            return;
        }

        if (!serviceOrderId) {
            GarageApp.showError('Không xác định được phiếu sửa chữa để tải dữ liệu bảo hành.');
            return;
        }

        if (!forceRefresh && self.warrantyLoadedOrderId === serviceOrderId && self.currentWarranty) {
            $('#warrantyLoading').addClass('d-none');
            $('#warrantyContent').removeClass('d-none');
            return;
        }

        self.resetWarrantySection(true);

        self.fetchWarranty(serviceOrderId)
            .done(function(response) {
                if (!AuthHandler.validateApiResponse(response)) {
                    return;
                }

        var payload = response.data || response.Data || null;
        var warranty = null;
        var message = response.message || response.errorMessage || null;
        var isApiError = !response.success;

        if (!isApiError && payload) {
            // payload có thể là ApiResponse<WarrantyDto> hoặc trực tiếp WarrantyDto
            if (typeof payload.success === 'boolean') {
                if (payload.success) {
                    warranty = payload.data || payload.Data || null;
                    message = payload.message || payload.errorMessage || message;
                } else {
                    message = payload.message || payload.errorMessage || 'Chưa có thông tin bảo hành.';
                }
            } else {
                warranty = payload;
            }
        } else if (isApiError) {
            message = response.errorMessage || response.message || message;
        }

        if (warranty) {
            self.currentWarranty = warranty;
            self.updateWarrantySummary(warranty);
            self.renderWarrantyItems(warranty.items || []);
            self.renderWarrantyClaims(warranty.claims || []);
            self.updateWarrantyAlerts(warranty);

            $('#warrantyLoading').addClass('d-none');
            $('#warrantyContent').removeClass('d-none');
            $('#warrantyEmpty').addClass('d-none');
            $('#warrantyError').addClass('d-none');

            $('#btnCreateWarrantyClaim').toggleClass('d-none', !warranty.id);
            $('#btnShowCreateClaim').toggleClass('d-none', !warranty.id);

            self.warrantyLoadedOrderId = serviceOrderId;
        } else {
            self.currentWarranty = null;
            if (isApiError) {
                self.handleWarrantyError(message || 'Không thể tải dữ liệu bảo hành.');
            } else {
                self.handleWarrantyEmpty(message || 'Chưa có thông tin bảo hành.');
            }
        }
            })
            .fail(function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = GarageApp.parseErrorMessage ? GarageApp.parseErrorMessage(xhr) : 'Không thể tải dữ liệu bảo hành.';
                self.handleWarrantyError(errorMsg);
            });
    },

    // ✅ 3.2: Gọi controller lấy thông tin bảo hành
    fetchWarranty: function(serviceOrderId) {
        return $.ajax({
            url: '/OrderManagement/GetWarranty/' + serviceOrderId,
            type: 'GET'
        });
    },

    // ✅ 3.2: Cập nhật thông tin bảo hành
    updateWarrantySummary: function(warranty) {
        $('#warrantyCodeDisplay').text(warranty.warrantyCode || '-');

        var status = (warranty.status || '').toLowerCase();
        var $statusBadge = $('#warrantyStatusBadge');
        $statusBadge.removeClass('badge-secondary badge-success badge-warning badge-danger badge-info');

        switch (status) {
            case 'active':
            case 'đang hiệu lực':
                $statusBadge.addClass('badge-success').text('Đang hiệu lực');
                break;
            case 'expired':
            case 'hết hạn':
                $statusBadge.addClass('badge-danger').text('Đã hết hạn');
                break;
            case 'voided':
            case 'hủy':
                $statusBadge.addClass('badge-secondary').text('Đã hủy');
                break;
            default:
                $statusBadge.addClass('badge-info').text(warranty.status || 'Không xác định');
                break;
        }

        $('#warrantyStartDate').text(this.formatDateTime(warranty.warrantyStartDate));
        $('#warrantyEndDate').text(this.formatDateTime(warranty.warrantyEndDate));
        $('#warrantyCustomerName').text(warranty.customerName || '-');
        $('#warrantyVehiclePlate').text(warranty.vehicleLicensePlate || '-');
        $('#warrantyHandoverBy').text(warranty.handoverBy || '-');
        $('#warrantyHandoverLocation').text(warranty.handoverLocation || '-');

        var itemCount = (warranty.items || []).filter(function(item) { return !item.isDeleted; }).length;
        var claimCount = (warranty.claims || []).filter(function(claim) { return !claim.isDeleted; }).length;
        $('#warrantyItemCount').text(itemCount);
        $('#warrantyClaimCount').text(claimCount);

        $('#btnCreateWarrantyClaim').toggleClass('d-none', !warranty.id);
        $('#btnShowCreateClaim').toggleClass('d-none', !warranty.id);
    },

    // ✅ 3.2: Render danh sách vật tư bảo hành
    renderWarrantyItems: function(items) {
        var self = this;
        var $tbody = $('#warrantyItemsBody');
        if (!items || items.length === 0) {
            $tbody.html('<tr><td colspan="6" class="text-center text-muted py-3">Chưa có dữ liệu</td></tr>');
            return;
        }

        var rows = items.map(function(item) {
            var statusText = item.status || 'Không xác định';
            var statusClass = 'badge-secondary';
            var status = (item.status || '').toLowerCase();
            if (status === 'active' || status === 'đang hiệu lực') {
                statusClass = 'badge-success';
            } else if (status === 'expired' || status === 'hết hạn') {
                statusClass = 'badge-danger';
            }

            return (
                '<tr>' +
                '<td>' + (item.partName || '-') + '</td>' +
                '<td>' + (item.partNumber || '-') + '</td>' +
                '<td>' + (item.warrantyMonths || 0) + '</td>' +
                '<td>' + self.formatDateTime(item.warrantyStartDate) + '</td>' +
                '<td>' + self.formatDateTime(item.warrantyEndDate) + '</td>' +
                '<td><span class="badge ' + statusClass + '">' + statusText + '</span></td>' +
                '</tr>'
            );
        }).join('');

        $tbody.html(rows);
    },

    // ✅ 3.2: Render danh sách khiếu nại
    renderWarrantyClaims: function(claims) {
        var self = this;
        var $tbody = $('#warrantyClaimsBody');
        if (!claims || claims.length === 0) {
            $tbody.html('<tr><td colspan="5" class="text-center text-muted py-3">Chưa có khiếu nại</td></tr>');
            return;
        }

        var rows = claims.map(function(claim) {
            var status = claim.status || 'Không xác định';
            var statusClass = 'badge-secondary';
            var statusLower = status.toLowerCase();

            if (statusLower === 'pending' || statusLower === 'đang xử lý') {
                statusClass = 'badge-warning';
            } else if (statusLower === 'approved' || statusLower === 'completed' || statusLower === 'hoàn tất') {
                statusClass = 'badge-success';
            } else if (statusLower === 'rejected' || statusLower === 'từ chối') {
                statusClass = 'badge-danger';
            }

            return (
                '<tr>' +
                '<td>' + (claim.claimNumber || '-') + '</td>' +
                '<td>' + self.formatDateTime(claim.claimDate) + '</td>' +
                '<td>' + (claim.issueDescription || '-') + '</td>' +
                '<td><span class="badge ' + statusClass + '">' + status + '</span></td>' +
                '<td>' + self.formatDateTime(claim.resolvedDate) + '</td>' +
                '</tr>'
            );
        }).join('');

        $tbody.html(rows);
    },

    // ✅ 3.2: Cập nhật cảnh báo hết hạn
    updateWarrantyAlerts: function(warranty) {
        $('#warrantyExpiryAlert, #warrantyExpiringAlert, #warrantyExpiredAlert').addClass('d-none');

        if (!warranty || !warranty.warrantyEndDate) {
            return;
        }

        var endDate = new Date(warranty.warrantyEndDate);
        if (isNaN(endDate.getTime())) {
            return;
        }

        var now = new Date();
        var diffDays = Math.ceil((endDate - now) / (1000 * 60 * 60 * 24));

        if (diffDays >= 30) {
            $('#warrantyExpiryAlert').removeClass('d-none');
        } else if (diffDays >= 0) {
            $('#warrantyExpiringAlert').removeClass('d-none');
        } else {
            $('#warrantyExpiredAlert').removeClass('d-none');
        }
    },

    // ✅ 3.2: Hiển thị thông báo khi không có dữ liệu bảo hành
    handleWarrantyEmpty: function(message) {
        $('#warrantyLoading').addClass('d-none');
        $('#warrantyContent').addClass('d-none');
        $('#warrantyError').addClass('d-none');
        $('#warrantyEmpty').removeClass('d-none').find('span').text(message || 'Chưa có thông tin bảo hành.');
    },

    // ✅ 3.2: Xử lý lỗi bảo hành
    handleWarrantyError: function(message) {
        $('#warrantyLoading').addClass('d-none');
        $('#warrantyContent').addClass('d-none');
        $('#warrantyEmpty').addClass('d-none');
        $('#warrantyError').removeClass('d-none').find('span').text(message || 'Không thể tải dữ liệu bảo hành.');
    },

    // ✅ 3.2: Tạo hoặc tái tạo bảo hành
    generateWarranty: function(serviceOrderId) {
        var self = this;
        var hasExisting = !!self.currentWarranty;

        Swal.fire({
            title: hasExisting ? 'Tạo lại bảo hành?' : 'Tạo bảo hành?',
            text: hasExisting
                ? 'Phiếu sửa chữa đã có bảo hành. Bạn có muốn tạo lại để cập nhật thông tin mới nhất?'
                : 'Hệ thống sẽ tạo bảo hành dựa trên thông tin bàn giao hiện tại.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: hasExisting ? 'Tạo lại' : 'Tạo',
            cancelButtonText: 'Hủy'
        }).then(function(result) {
            if (result.isConfirmed) {
                self.executeGenerateWarranty(serviceOrderId, hasExisting);
            }
        });
    },

    executeGenerateWarranty: function(serviceOrderId, forceRegenerate) {
        var self = this;
        $('#btnGenerateWarranty').prop('disabled', true);

        $.ajax({
            url: '/OrderManagement/GenerateWarranty/' + serviceOrderId,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                ForceRegenerate: !!forceRegenerate
            })
        })
            .done(function(response) {
                if (!AuthHandler.validateApiResponse(response)) {
                    return;
                }

                if (response.success) {
                    GarageApp.showSuccess(response.message || 'Đã tạo bảo hành thành công.');
                    self.warrantyLoadedOrderId = null;
                    self.loadWarrantyData(serviceOrderId, true);
                } else {
                    var errorMsg = GarageApp.parseErrorMessage(response) || response.errorMessage || response.message || 'Không thể tạo bảo hành.';
                    GarageApp.showError(errorMsg);
                }
            })
            .fail(function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = GarageApp.parseErrorMessage ? GarageApp.parseErrorMessage(xhr) : 'Không thể tạo bảo hành.';
                GarageApp.showError(errorMsg);
            })
            .always(function() {
                $('#btnGenerateWarranty').prop('disabled', false);
            });
    },

    // ✅ 3.2: Hiển thị form tạo khiếu nại bảo hành
    openCreateWarrantyClaimModal: function(serviceOrderId, warranty) {
        var self = this;
        var defaultDate = new Date().toISOString().split('T')[0];

        Swal.fire({
            title: 'Tạo khiếu nại bảo hành',
            html:
                '<div class="form-group text-left">' +
                '   <label for="warrantyClaimDate" class="font-weight-bold">Ngày khiếu nại</label>' +
                '   <input type="date" id="warrantyClaimDate" class="form-control" value="' + defaultDate + '">' +
                '</div>' +
                '<div class="form-group text-left">' +
                '   <label for="warrantyClaimDescription" class="font-weight-bold">Mô tả vấn đề <span class="text-danger">*</span></label>' +
                '   <textarea id="warrantyClaimDescription" class="form-control" rows="3" placeholder="Mô tả chi tiết vấn đề bảo hành"></textarea>' +
                '</div>' +
                '<div class="form-group text-left">' +
                '   <label for="warrantyClaimNotes" class="font-weight-bold">Ghi chú</label>' +
                '   <textarea id="warrantyClaimNotes" class="form-control" rows="2" placeholder="Ghi chú thêm (nếu có)"></textarea>' +
                '</div>',
            focusConfirm: false,
            showCancelButton: true,
            confirmButtonText: 'Tạo khiếu nại',
            cancelButtonText: 'Hủy',
            preConfirm: function() {
                var description = ($('#warrantyClaimDescription').val() || '').trim();
                if (!description) {
                    Swal.showValidationMessage('Vui lòng nhập mô tả vấn đề bảo hành.');
                    return false;
                }

                return {
                    claimDate: $('#warrantyClaimDate').val(),
                    issueDescription: description,
                    notes: ($('#warrantyClaimNotes').val() || '').trim()
                };
            }
        }).then(function(result) {
            if (result.isConfirmed && result.value) {
                self.submitWarrantyClaim(warranty.id, serviceOrderId, result.value);
            }
        });
    },

    // ✅ 3.2: Gửi yêu cầu tạo khiếu nại bảo hành
    submitWarrantyClaim: function(warrantyId, serviceOrderId, formData) {
        var self = this;
        if (!warrantyId) {
            GarageApp.showError('Không xác định được bảo hành.');
            return;
        }

        var payload = {
            ClaimDate: formData.claimDate || null,
            IssueDescription: formData.issueDescription,
            Notes: formData.notes || null,
            ServiceOrderId: serviceOrderId || null
        };

        $.ajax({
            url: '/OrderManagement/CreateWarrantyClaim/' + warrantyId,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload)
        })
            .done(function(response) {
                if (!AuthHandler.validateApiResponse(response)) {
                    return;
                }

                if (response.success) {
                    GarageApp.showSuccess(response.message || 'Đã tạo khiếu nại bảo hành.');
                    self.warrantyLoadedOrderId = null;
                    self.loadWarrantyData(serviceOrderId, true);
                } else {
                    var errorMsg = GarageApp.parseErrorMessage(response) || response.errorMessage || response.message || 'Không thể tạo khiếu nại.';
                    GarageApp.showError(errorMsg);
                }
            })
            .fail(function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = GarageApp.parseErrorMessage ? GarageApp.parseErrorMessage(xhr) : 'Không thể tạo khiếu nại bảo hành.';
                GarageApp.showError(errorMsg);
            });
    },

    // ✅ 3.1: Áp dụng phương pháp tính COGS
    applyCogsMethod: function() {
        var self = this;
        var serviceOrderId = self.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (!serviceOrderId) {
            GarageApp.showError('Không xác định được phiếu sửa chữa.');
            return;
        }

        var method = self.normalizeCogsMethod($('#cogsMethodSelect').val());
        var $button = $('#btnApplyCogsMethod');
        $button.prop('disabled', true);

        $.ajax({
            url: '/OrderManagement/SetOrderCogsMethod/' + serviceOrderId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ Method: method })
        })
            .done(function(response) {
                if (!AuthHandler.validateApiResponse(response)) {
                    return;
                }

                if (response.success) {
                    GarageApp.showSuccess(response.message || 'Đã cập nhật phương pháp tính COGS.');
                    self.settlementLoadedOrderId = null;
                    self.loadSettlementData(serviceOrderId, true);
                } else {
                    var errorMsg = GarageApp.parseErrorMessage(response) || response.error || response.message || 'Không thể cập nhật phương pháp tính COGS.';
                    GarageApp.showError(errorMsg);
                }
            })
            .fail(function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = GarageApp.parseErrorMessage ? GarageApp.parseErrorMessage(xhr) : 'Không thể cập nhật phương pháp tính COGS.';
                GarageApp.showError(errorMsg);
            })
            .always(function() {
                $button.prop('disabled', false);
            });
    },

    // ✅ 3.1: Yêu cầu tính lại COGS
    recalculateCogs: function() {
        var self = this;
        var serviceOrderId = self.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (!serviceOrderId) {
            GarageApp.showError('Không xác định được phiếu sửa chữa.');
            return;
        }

        var method = self.normalizeCogsMethod($('#cogsMethodSelect').val());

        Swal.fire({
            title: 'Tính lại COGS?',
            text: 'Hệ thống sẽ tính lại giá vốn theo phương pháp đã chọn. Thao tác này có thể mất vài giây.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Tính lại',
            cancelButtonText: 'Hủy'
        }).then(function(result) {
            if (result.isConfirmed) {
                self.executeRecalculateCogs(serviceOrderId, method);
            }
        });
    },

    // ✅ 3.1: Gọi API tính lại COGS
    executeRecalculateCogs: function(serviceOrderId, method) {
        var self = this;
        var $button = $('#btnRecalculateCogs');
        $button.prop('disabled', true);

        self.resetSettlementSection(true);

        $.ajax({
            url: '/OrderManagement/CalculateOrderCogs/' + serviceOrderId,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ Method: method })
        })
            .done(function(response) {
                if (!AuthHandler.validateApiResponse(response)) {
                    return;
                }

                if (response.success) {
                    GarageApp.showSuccess(response.message || 'Đã tính lại COGS thành công.');
                    self.settlementLoadedOrderId = null;
                    self.loadSettlementData(serviceOrderId, true);
                    if (self.orderTable) {
                        self.orderTable.ajax.reload(null, false);
                    }
                } else {
                    var errorMsg = GarageApp.parseErrorMessage(response) || response.error || response.message || 'Không thể tính lại COGS.';
                    GarageApp.showError(errorMsg);
                    self.handleSettlementError(errorMsg);
                }
            })
            .fail(function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = GarageApp.parseErrorMessage ? GarageApp.parseErrorMessage(xhr) : 'Không thể tính lại COGS.';
                GarageApp.showError(errorMsg);
                self.handleSettlementError(errorMsg);
            })
            .always(function() {
                $button.prop('disabled', false);
            });
    },

    // ✅ 3.1: Làm mới dữ liệu Quyết toán
    refreshSettlementData: function() {
        var serviceOrderId = this.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (!serviceOrderId) {
            GarageApp.showError('Không xác định được phiếu sửa chữa.');
            return;
        }
        this.settlementLoadedOrderId = null;
        this.loadSettlementData(serviceOrderId, true);
    },

    // ✅ 2.4: Update QC buttons visibility based on order status
    updateQCButtons: function(order) {
        // ✅ FIX: Validate order exists
        if (!order) {
            return;
        }
        
        var status = order.status || '';
        var allItemsCompleted = false;
        
        // ✅ FIX: Validate order.id exists
        if (!order.id || order.id <= 0) {
            console.warn('updateQCButtons: order.id is invalid', order);
            return;
        }
        
        // Check if all items are completed
        if (order.serviceOrderItems && order.serviceOrderItems.length > 0) {
            allItemsCompleted = order.serviceOrderItems.every(function(item) {
                return item.status === 'Completed' || item.status === 'Cancelled';
            });
        }
        
        // Hide all QC buttons first
        $('#btnCompleteTechnical, #btnStartQC, #btnCompleteQC, #btnHandover').hide();
        
        // ✅ 2.4.1: Show "Hoàn thành Kỹ thuật" when status = Completed or InProgress and all items completed
        if ((status === 'Completed' || status === 'InProgress') && allItemsCompleted) {
            $('#btnCompleteTechnical').show().off('click').on('click', function() {
                OrderManagement.completeTechnical(order.id);
            });
        }
        
        // ✅ 2.4.2: Show "Tạo Phiếu QC" when status = WaitingForQC
        // Kiểm tra xem đã có phiếu QC chưa trước khi hiển thị button
        if (status === 'WaitingForQC') {
            var self = this;
            // ✅ FIX: Kiểm tra xem đã có phiếu QC chưa
            $.ajax({
                url: '/QCManagement/GetQC/' + order.id,
                type: 'GET',
                success: function(res) {
                    if (res && res.success && res.data) {
                        // Đã có phiếu QC → Hiển thị button "Xem QC" hoặc "Tiếp Tục QC"
                        // Nhưng theo logic, nếu status = WaitingForQC thì không thể có QC record
                        // Nên đây là trường hợp edge case, vẫn hiển thị "Tạo Phiếu QC"
                        $('#btnStartQC').html('<i class="fas fa-plus"></i> Tạo Phiếu QC').show().off('click').on('click', function() {
                            if (window.GarageApp && window.GarageApp.QC) {
                                window.GarageApp.QC.showStartQCModal(order.id);
                            } else {
                                GarageApp.showError('QC module chưa được tải. Vui lòng reload trang.');
                            }
                        });
                    } else {
                        // Chưa có phiếu QC → Hiển thị button "Tạo Phiếu QC"
                        $('#btnStartQC').html('<i class="fas fa-plus"></i> Tạo Phiếu QC').show().off('click').on('click', function() {
                            if (window.GarageApp && window.GarageApp.QC) {
                                window.GarageApp.QC.showStartQCModal(order.id);
                            } else {
                                GarageApp.showError('QC module chưa được tải. Vui lòng reload trang.');
                            }
                        });
                    }
                },
                error: function() {
                    // Lỗi khi kiểm tra → Vẫn hiển thị button "Tạo Phiếu QC" (mặc định)
                    $('#btnStartQC').html('<i class="fas fa-plus"></i> Tạo Phiếu QC').show().off('click').on('click', function() {
                        if (window.GarageApp && window.GarageApp.QC) {
                            window.GarageApp.QC.showStartQCModal(order.id);
                        } else {
                            GarageApp.showError('QC module chưa được tải. Vui lòng reload trang.');
                        }
                    });
                }
            });
        }
        
        // ✅ 2.4.2: Show "Hoàn thành QC" when status = QCInProgress
        if (status === 'QCInProgress') {
            $('#btnCompleteQC').show().off('click').on('click', function() {
                if (window.GarageApp && window.GarageApp.QC) {
                    window.GarageApp.QC.showCompleteQCModal(order.id);
                } else {
                    GarageApp.showError('QC module chưa được tải. Vui lòng reload trang.');
                }
            });
        }
        
        // ✅ 2.4.4: Show "Bàn giao xe" when status = ReadyToBill (need to check QC passed)
        if (status === 'ReadyToBill') {
            // Load QC info to verify
            var self = this;
            $.ajax({
                url: '/QCManagement/GetQC/' + order.id,
                type: 'GET',
                success: function(res) {
                    if (res && res.success && res.data && res.data.qcResult === 'Pass') {
                        $('#btnHandover').show().off('click').on('click', function() {
                            if (window.GarageApp && window.GarageApp.QC) {
                                window.GarageApp.QC.showHandoverModal(order.id);
                            } else {
                                GarageApp.showError('QC module chưa được tải. Vui lòng reload trang.');
                            }
                        });
                    }
                }
            });
        }
    },

    // ✅ 2.4.1: Complete Technical function
    completeTechnical: function(orderId) {
        var self = this;
        
        // ✅ FIX: Validate orderId
        if (!orderId || orderId <= 0) {
            GarageApp.showError('ID phiếu sửa chữa không hợp lệ');
            return;
        }
        
        // ✅ FIX: Prevent multiple simultaneous requests
        var $btn = $('#btnCompleteTechnical');
        // ✅ FIX: Check if button exists before accessing
        if ($btn.length === 0) {
            GarageApp.showError('Không tìm thấy nút xử lý. Vui lòng reload trang.');
            return;
        }
        if ($btn.data('processing')) {
            GarageApp.showWarning('Đang xử lý, vui lòng đợi...');
            return;
        }
        $btn.data('processing', true).prop('disabled', true);
        
        Swal.fire({
            title: 'Xác nhận',
            text: 'Bạn có chắc chắn muốn hoàn thành kỹ thuật và chuyển JO sang chờ QC?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Xác nhận',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/QCManagement/CompleteTechnical/' + orderId,
                    type: 'POST',
                    success: function(res) {
                        // ✅ FIX: Reset processing flag
                        $btn.data('processing', false).prop('disabled', false);
                        
                        if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                            // ✅ FIX: validateApiResponse đã handle unauthorized, nhưng cần show error nếu có
                            if (res && res.errorMessage) {
                                GarageApp.showError(res.errorMessage);
                            }
                            return;
                        }
                        
                        if (res && res.success) {
                            GarageApp.showSuccess(res.message || 'Đã hoàn thành kỹ thuật');
                            
                            // ✅ FIX: Reload order data trong modal thay vì đóng modal
                            // Reload order data để cập nhật status mới (WaitingForQC)
                            $.ajax({
                                url: '/OrderManagement/GetOrder/' + orderId,
                                type: 'GET',
                                success: function(response) {
                                    // ✅ FIX: Check if modal is still open before updating
                                    var $modal = $('#viewOrderModal');
                                    if (!$modal.is(':visible')) {
                                        // Modal đã đóng, không cần reload table vì đã reload ở trên
                                        return;
                                    }
                                    
                                    // ✅ FIX: Validate response exists
                                    if (!response) {
                                        console.error('GetOrder returned null/undefined response');
                                        GarageApp.showError('Không nhận được phản hồi từ server');
                                        return;
                                    }
                                    
                                    if (AuthHandler && AuthHandler.validateApiResponse(response)) {
                                        // ✅ FIX: Validate response.data exists and is not null
                                        if (response && response.success && response.data) {
                                            try {
                                                // ✅ FIX: Reset button processing flag trước khi populate modal
                                                // Vì populateViewModal sẽ gọi updateQCButtons và bind lại event handler
                                                var $btnCompleteTechnical = $('#btnCompleteTechnical');
                                                if ($btnCompleteTechnical.length > 0) {
                                                    $btnCompleteTechnical.data('processing', false).prop('disabled', false);
                                                }
                                                
                                                // ✅ FIX: Preserve active tab trước khi populate modal
                                                // Vì populateViewModal có thể trigger tab reset
                                                var activeTabBeforePopulate = $('#viewOrderModal .nav-tabs .nav-link.active');
                                                var activeTabHref = null;
                                                if (activeTabBeforePopulate.length > 0) {
                                                    activeTabHref = activeTabBeforePopulate.attr('href');
                                                }
                                                
                                                // Update modal với order data mới
                                                self.populateViewModal(response.data);
                                                
                                                // ✅ FIX: Restore active tab sau khi populate (nếu có)
                                                if (activeTabHref) {
                                                    var $targetTab = $('#viewOrderModal .nav-tabs .nav-link[href="' + activeTabHref + '"]');
                                                    if ($targetTab.length > 0) {
                                                        // ✅ FIX: Check if tab is already active to avoid unnecessary tab.show()
                                                        // Nếu tab đã active, tab.show() sẽ không trigger shown.bs.tab event
                                                        var isAlreadyActive = $targetTab.hasClass('active');
                                                        
                                                        if (!isAlreadyActive) {
                                                            // Tab chưa active, gọi tab.show() để activate và trigger event
                                                            $targetTab.tab('show');
                                                        } else {
                                                            // Tab đã active, chỉ cần reload data nếu là QC tab
                                                            // ✅ FIX: Check both possible tab href formats
                                                            if (activeTabHref === '#qc-tab' || activeTabHref === '#qc') {
                                                                try {
                                                                    self.loadQCInfo(orderId);
                                                                } catch (qcEx) {
                                                                    console.error('Error loading QC info:', qcEx);
                                                                    // Non-blocking error, chỉ log
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            } catch (ex) {
                                                console.error('Error populating modal:', ex);
                                                GarageApp.showError('Lỗi khi cập nhật thông tin phiếu sửa chữa');
                                                // Fallback: reload table and close modal
                                                if (self.orderTable && typeof self.orderTable.ajax === 'function') {
                                                    self.orderTable.ajax.reload();
                                                }
                                                $('#viewOrderModal').modal('hide');
                                            }
                                        } else {
                                            // ✅ FIX: Handle case where response.success but no data
                                            GarageApp.showWarning('Không thể tải thông tin phiếu sửa chữa mới. Vui lòng làm mới trang.');
                                            // Không reload table ở đây vì đã reload ở trên
                                        }
                                    } else {
                                        // ✅ FIX: validateApiResponse returned false, show error if available
                                        if (response && response.errorMessage) {
                                            GarageApp.showError(response.errorMessage);
                                        }
                                    }
                                },
                                error: function(xhr) {
                                    if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                                        AuthHandler.handleUnauthorized(xhr, true);
                                    } else {
                                        // ✅ FIX: Show error message to user
                                        GarageApp.showError('Không thể tải thông tin phiếu sửa chữa mới. Vui lòng làm mới trang.');
                                        // Nếu reload fail, vẫn reload table và đóng modal
                                        if (self.orderTable && typeof self.orderTable.ajax === 'function') {
                                            self.orderTable.ajax.reload();
                                        }
                                        $('#viewOrderModal').modal('hide');
                                    }
                                }
                            });
                            
                            // ✅ FIX: Reload table để cập nhật danh sách
                            // Note: Reload này sẽ chạy ngay lập tức, không đợi GetOrder response
                            // Điều này đảm bảo table được cập nhật ngay cả khi GetOrder fail
                            // Không reload lại trong GetOrder success callback để tránh double reload
                            if (self.orderTable && typeof self.orderTable.ajax === 'function') {
                                self.orderTable.ajax.reload();
                            }
                        } else {
                            GarageApp.showError(GarageApp.parseErrorMessage(res) || 'Lỗi khi hoàn thành kỹ thuật');
                        }
                    },
                    error: function(xhr) {
                        // ✅ FIX: Reset processing flag on error
                        $btn.data('processing', false).prop('disabled', false);
                        
                        if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi hoàn thành kỹ thuật');
                        }
                    }
                });
            } else {
                // ✅ FIX: Reset processing flag if user cancels
                $btn.data('processing', false).prop('disabled', false);
            }
        });
    },

    // ✅ 2.3.4: Load và hiển thị tiến độ Service Order
    loadOrderProgress: function(serviceOrderId) {
        var self = this;
        
        $('#progressContent').html(`
            <div class="text-center py-3">
                <div class="spinner-border text-primary" role="status">
                    <span class="sr-only">Đang tải...</span>
                </div>
                <p class="text-muted mt-2">Đang tải tiến độ...</p>
            </div>
        `);

        $.ajax({
            url: '/OrderManagement/GetOrderProgress/' + serviceOrderId,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success && response.data) {
                        self.renderProgress(response.data);
                    } else {
                        $('#progressContent').html('<p class="text-danger">Không thể tải tiến độ: ' + (response.error || 'Lỗi không xác định') + '</p>');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    $('#progressContent').html('<p class="text-danger">Lỗi khi tải tiến độ: ' + error + '</p>');
                }
            }
        });
    },

    // ✅ 2.3.4: Render progress dashboard
    renderProgress: function(progress) {
        var self = this; // ✅ FIX: Define self để có thể dùng trong forEach
        var html = '<div class="progress-dashboard">';
        
        // Overall Progress Bar
        html += '<div class="card mb-3">';
        html += '<div class="card-header bg-primary text-white">';
        html += '<h5 class="mb-0"><i class="fas fa-tasks mr-2"></i>Tiến Độ Tổng Thể</h5>';
        html += '</div>';
        html += '<div class="card-body">';
        html += '<div class="progress mb-3" style="height: 30px;">';
        html += '<div class="progress-bar progress-bar-striped progress-bar-animated bg-success" role="progressbar"';
        html += ' style="width: ' + progress.progressPercentage + '%" aria-valuenow="' + progress.progressPercentage + '"';
        html += ' aria-valuemin="0" aria-valuemax="100">';
        html += '<strong>' + progress.progressPercentage.toFixed(1) + '%</strong>';
        html += '</div>';
        html += '</div>';
        
        // Statistics
        html += '<div class="row">';
        html += '<div class="col-md-3"><div class="info-box bg-info"><span class="info-box-icon"><i class="fas fa-list"></i></span>';
        html += '<div class="info-box-content"><span class="info-box-text">Tổng Hạng Mục</span><span class="info-box-number">' + progress.totalItems + '</span></div></div></div>';
        html += '<div class="col-md-3"><div class="info-box bg-warning"><span class="info-box-icon"><i class="fas fa-clock"></i></span>';
        html += '<div class="info-box-content"><span class="info-box-text">Chờ Xử Lý</span><span class="info-box-number">' + progress.pendingItems + '</span></div></div></div>';
        html += '<div class="col-md-3"><div class="info-box bg-primary"><span class="info-box-icon"><i class="fas fa-spinner"></i></span>';
        html += '<div class="info-box-content"><span class="info-box-text">Đang Làm</span><span class="info-box-number">' + progress.inProgressItems + '</span></div></div></div>';
        html += '<div class="col-md-3"><div class="info-box bg-success"><span class="info-box-icon"><i class="fas fa-check"></i></span>';
        html += '<div class="info-box-content"><span class="info-box-text">Đã Hoàn Thành</span><span class="info-box-number">' + progress.completedItems + '</span></div></div></div>';
        html += '</div>';
        
        // Time Statistics
        html += '<div class="row mt-3">';
        html += '<div class="col-md-4"><div class="small-box bg-info"><div class="inner">';
        html += '<h3>' + (progress.totalEstimatedHours || 0).toFixed(2) + '</h3><p>Giờ Công Dự Kiến</p></div></div></div>';
        html += '<div class="col-md-4"><div class="small-box bg-success"><div class="inner">';
        html += '<h3>' + (progress.totalActualHours || 0).toFixed(2) + '</h3><p>Giờ Công Thực Tế</p></div></div></div>';
        html += '<div class="col-md-4"><div class="small-box bg-warning"><div class="inner">';
        html += '<h3>' + (progress.remainingEstimatedHours || 0).toFixed(2) + '</h3><p>Giờ Công Còn Lại</p></div></div></div>';
        html += '</div>';
        
        html += '</div></div>';
        
        // Items Progress
        if (progress.items && progress.items.length > 0) {
            html += '<div class="card">';
            html += '<div class="card-header bg-secondary text-white">';
            html += '<h5 class="mb-0"><i class="fas fa-list-ul mr-2"></i>Tiến Độ Từng Hạng Mục</h5>';
            html += '</div>';
            html += '<div class="card-body">';
            html += '<div class="table-responsive">';
            html += '<table class="table table-sm table-bordered">';
            html += '<thead class="bg-light"><tr>';
            html += '<th>Hạng Mục</th><th>KTV</th><th>Tiến Độ</th><th>Giờ Công Dự Kiến</th><th>Giờ Công Thực Tế</th><th>Trạng Thái</th>';
            html += '</tr></thead><tbody>';
            
            progress.items.forEach(function(item) {
                var statusBadge = self.formatItemStatus(item.status);
                html += '<tr>';
                html += '<td>' + item.itemName + '</td>';
                html += '<td>' + (item.assignedTechnicianName || '<span class="text-muted">Chưa phân công</span>') + '</td>';
                html += '<td>';
                html += '<div class="progress" style="height: 20px;">';
                html += '<div class="progress-bar ' + (item.progressPercentage >= 100 ? 'bg-success' : item.progressPercentage > 0 ? 'bg-primary' : 'bg-secondary') + '"';
                html += ' style="width: ' + item.progressPercentage + '%">' + item.progressPercentage.toFixed(0) + '%</div>';
                html += '</div>';
                html += '</td>';
                html += '<td>' + (item.estimatedHours ? item.estimatedHours.toFixed(2) + ' giờ' : '-') + '</td>';
                html += '<td>' + (item.actualHours ? item.actualHours.toFixed(2) + ' giờ' : '-') + '</td>';
                html += '<td>' + statusBadge + '</td>';
                html += '</tr>';
            });
            
            html += '</tbody></table></div></div></div>';
        }
        
        html += '</div>';
        $('#progressContent').html(html);
    },

    // ✅ 2.4: Load QC information
    loadQCInfo: function(serviceOrderId) {
        var self = this;
        
        $('#qcContent').html(`
            <div class="text-center py-3">
                <div class="spinner-border text-primary" role="status">
                    <span class="sr-only">Đang tải...</span>
                </div>
                <p class="text-muted mt-2">Đang tải thông tin QC...</p>
            </div>
        `);

        $.ajax({
            url: '/QCManagement/GetQC/' + serviceOrderId,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success && response.data) {
                        self.renderQCInfo(response.data);
                    } else {
                        // ✅ FIX: Kiểm tra status của ServiceOrder để hiển thị message phù hợp
                        // Nếu status = WaitingForQC, hiển thị message với hướng dẫn bắt đầu QC
                        // Nếu status khác, hiển thị message yêu cầu hoàn thành kỹ thuật
                        var $modal = $('#viewOrderModal');
                        var modalOrderStatus = $modal.data('order-status');
                        var orderStatusText = $('#viewStatus').text() || '';
                        
                        // ✅ FIX: Check status từ modal data (ưu tiên) hoặc từ text hiển thị
                        var isWaitingForQC = modalOrderStatus === 'WaitingForQC' || 
                                            orderStatusText.includes('Chờ QC');
                        
                        var messageHtml = '';
                        if (isWaitingForQC) {
                            messageHtml = `
                                <div class="alert alert-info">
                                    <i class="fas fa-info-circle"></i> 
                                    <strong>JO đã sẵn sàng cho QC.</strong><br>
                                    Vui lòng bấm nút <strong>"Bắt Đầu QC"</strong> ở phía dưới để bắt đầu kiểm tra chất lượng.
                                </div>
                            `;
                        } else {
                            messageHtml = `
                                <div class="alert alert-info">
                                    <i class="fas fa-info-circle"></i> Chưa có thông tin QC. 
                                    Vui lòng hoàn thành kỹ thuật và bắt đầu kiểm tra QC.
                                </div>
                            `;
                        }
                        $('#qcContent').html(messageHtml);
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    // ✅ FIX: Kiểm tra status để hiển thị message phù hợp khi error
                    var $modal = $('#viewOrderModal');
                    var modalOrderStatus = $modal.data('order-status');
                    var orderStatusText = $('#viewStatus').text() || '';
                    
                    // ✅ FIX: Check status từ modal data (ưu tiên) hoặc từ text hiển thị
                    var isWaitingForQC = modalOrderStatus === 'WaitingForQC' || 
                                        orderStatusText.includes('Chờ QC');
                    
                    var messageHtml = '';
                    if (isWaitingForQC) {
                        messageHtml = `
                            <div class="alert alert-info">
                                <i class="fas fa-info-circle"></i> 
                                <strong>JO đã sẵn sàng cho QC.</strong><br>
                                Vui lòng bấm nút <strong>"Bắt Đầu QC"</strong> ở phía dưới để bắt đầu kiểm tra chất lượng.
                            </div>
                        `;
                    } else {
                        messageHtml = `
                            <div class="alert alert-info">
                                <i class="fas fa-info-circle"></i> Chưa có thông tin QC. 
                                Vui lòng hoàn thành kỹ thuật và bắt đầu kiểm tra QC.
                            </div>
                        `;
                    }
                    $('#qcContent').html(messageHtml);
                }
            }
        });
    },

    // ✅ 2.4: Render QC information
    renderQCInfo: function(qc) {
        var html = '<div class="qc-info">';
        
        // Basic Info Card
        html += '<div class="card mb-3">';
        html += '<div class="card-header bg-info text-white">';
        html += '<h5 class="mb-0"><i class="fas fa-clipboard-check mr-2"></i>Thông Tin QC</h5>';
        html += '</div>';
        html += '<div class="card-body">';
        html += '<div class="row">';
        html += '<div class="col-md-6">';
        html += '<strong>Người Kiểm Tra:</strong> ' + (qc.qcInspectorName || 'Chưa xác định') + '<br>';
        html += '<strong>Ngày Kiểm Tra:</strong> ' + (qc.qcDate ? new Date(qc.qcDate).toLocaleString('vi-VN') : '') + '<br>';
        html += '</div>';
        html += '<div class="col-md-6">';
        html += '<strong>Ngày Hoàn Thành:</strong> ' + (qc.qcCompletedDate ? new Date(qc.qcCompletedDate).toLocaleString('vi-VN') : 'Chưa hoàn thành') + '<br>';
        html += '<strong>Kết Quả:</strong> ';
        if (qc.qcResult === 'Pass') {
            html += '<span class="badge badge-success">Đạt</span>';
        } else if (qc.qcResult === 'Fail') {
            html += '<span class="badge badge-danger">Không Đạt</span>';
        } else {
            html += '<span class="badge badge-warning">Chờ Xử Lý</span>';
        }
        html += '</div>';
        html += '</div>';
        
        if (qc.qcNotes) {
            html += '<div class="mt-2">';
            html += '<strong>Ghi Chú QC:</strong><br>';
            html += '<p class="mb-0">' + qc.qcNotes + '</p>';
            html += '</div>';
        }
        
        if (qc.reworkRequired && qc.reworkNotes) {
            html += '<div class="mt-2 alert alert-warning">';
            html += '<strong>Cần Làm Lại:</strong><br>';
            html += '<p class="mb-0">' + qc.reworkNotes + '</p>';
            html += '</div>';
        }
        
        html += '</div></div>';
        
        // Checklist Card
        if (qc.qcChecklistItems && qc.qcChecklistItems.length > 0) {
            html += '<div class="card">';
            html += '<div class="card-header bg-secondary text-white">';
            html += '<h5 class="mb-0"><i class="fas fa-list-check mr-2"></i>QC Checklist</h5>';
            html += '</div>';
            html += '<div class="card-body">';
            html += '<div class="table-responsive">';
            html += '<table class="table table-sm table-bordered">';
            html += '<thead class="bg-light"><tr>';
            html += '<th style="width: 50px">#</th>';
            html += '<th>Hạng Mục Kiểm Tra</th>';
            html += '<th style="width: 100px">Kết Quả</th>';
            html += '<th>Ghi Chú</th>';
            html += '</tr></thead><tbody>';
            
            qc.qcChecklistItems.forEach(function(item, index) {
                var resultBadge = '';
                if (item.result === 'Pass') {
                    resultBadge = '<span class="badge badge-success badge-sm">Đạt</span>';
                } else if (item.result === 'Fail') {
                    resultBadge = '<span class="badge badge-danger badge-sm">Không Đạt</span>';
                } else {
                    resultBadge = '<span class="badge badge-secondary badge-sm">-</span>';
                }
                
                html += '<tr>';
                html += '<td>' + (index + 1) + '</td>';
                html += '<td>' + (item.isChecked ? '<i class="fas fa-check text-success"></i> ' : '') + item.checklistItemName + '</td>';
                html += '<td>' + resultBadge + '</td>';
                html += '<td>' + (item.notes || '-') + '</td>';
                html += '</tr>';
            });
            
            html += '</tbody></table>';
            html += '</div></div></div>';
        }
        
        html += '</div>';
        
        $('#qcContent').html(html);
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
            'Cancelled': '<span class="badge badge-danger">Đã Hủy</span>',
            'WaitingForQC': '<span class="badge badge-primary">Chờ QC</span>',
            'Waiting For QC': '<span class="badge badge-primary">Chờ QC</span>',
            'QCInProgress': '<span class="badge badge-info">Đang QC</span>',
            'QC In Progress': '<span class="badge badge-info">Đang QC</span>',
            'ReadyToBill': '<span class="badge badge-success">Sẵn Sàng Thanh Toán</span>',
            'Ready To Bill': '<span class="badge badge-success">Sẵn Sàng Thanh Toán</span>',
            'OnHold': '<span class="badge badge-warning">Tạm Dừng</span>'
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
    getItemActionButtons: function(orderId, itemId, status, startTime, completedTime, orderStatus) {
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
        
        // ✅ 2.4.3: Thêm button "Ghi Nhận Giờ Công Làm Lại" nếu order status = InProgress (QC Fail)
        // Button này sẽ được hiển thị sau khi check QC Fail
        // Được thêm vào bằng checkAndShowReworkButtons()
        
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
        
        // ✅ 2.4.3: Ghi nhận giờ công làm lại
        $(document).off('click', '.btn-record-rework').on('click', '.btn-record-rework', function() {
            var itemId = $(this).data('item-id');
            var itemName = $(this).data('item-name');
            self.showRecordReworkHoursModal(orderId, itemId, itemName);
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
                
                // ✅ 2.3.3: Logic hiển thị nút Edit và Delete dựa trên status
                // - Status = "Rejected" hoặc "Identified" → Cho phép Edit và Delete
                // - Status = "Approved" hoặc "Repaired" → Không cho phép Edit và Delete
                var canEdit = issue.status === 'Rejected' || issue.status === 'Identified' || 
                             issue.status === 'Reported' || issue.status === 'Quoted';
                var canDelete = issue.status === 'Rejected' || issue.status === 'Identified';
                
                // Edit button - chỉ hiển thị khi có quyền
                if (canEdit) {
                    var editTitle = issue.status === 'Rejected' 
                        ? 'Chỉnh sửa để tạo lại báo giá mới' 
                        : 'Sửa';
                    html += '<button class="btn btn-sm btn-warning mr-1" onclick="OrderManagement.editAdditionalIssue(' + issue.id + ')" title="' + editTitle + '">';
                    html += '<i class="fas fa-edit"></i></button>';
                }
                
                // Delete button - chỉ hiển thị khi có quyền
                if (canDelete) {
                    var deleteTitle = issue.status === 'Rejected' 
                        ? 'Xóa phát sinh đã từ chối' 
                        : 'Xóa';
                    html += '<button class="btn btn-sm btn-danger" onclick="OrderManagement.deleteAdditionalIssue(' + issue.id + ', \'' + issue.status + '\')" title="' + deleteTitle + '">';
                    html += '<i class="fas fa-trash"></i></button>';
                }
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
        $('#resetStatusSection').hide(); // ✅ Ẩn reset section khi tạo mới

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

                    // ✅ Hiển thị option reset Status nếu Status = "Rejected"
                    if (issue.status === 'Rejected') {
                        $('#resetStatusSection').show();
                        $('#resetToIdentified').prop('checked', false);
                    } else {
                        $('#resetStatusSection').hide();
                    }

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
    deleteAdditionalIssue: function(issueId, issueStatus) {
        var self = this;
        
        // ✅ Custom message dựa trên status
        var title = 'Xác nhận xóa?';
        var text = 'Bạn có chắc chắn muốn xóa phát sinh này?';
        var warningMessage = '';
        
        if (issueStatus === 'Rejected') {
            title = 'Xác nhận xóa phát sinh đã từ chối?';
            text = 'Phát sinh này đã bị khách hàng từ chối.';
            warningMessage = 'Xóa sẽ mất lịch sử từ chối. Bạn có chắc chắn muốn xóa?';
        }
        
        Swal.fire({
            title: title,
            html: '<p>' + text + '</p>' + (warningMessage ? '<p class="text-danger"><strong>' + warningMessage + '</strong></p>' : ''),
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
    },
    
    // ✅ 2.4.3: Check QC Fail và hiển thị button "Ghi Nhận Giờ Công Làm Lại"
    checkAndShowReworkButtons: function(orderId, items) {
        var self = this;
        
        // Check if latest QC is Fail
        $.ajax({
            url: '/QCManagement/GetQC/' + orderId,
            type: 'GET',
            success: function(res) {
                if (res && res.success && res.data && res.data.qcResult === 'Fail') {
                    // QC Fail -> Show rework button for completed/in-progress items
                    items.forEach(function(item) {
                        if (item.status === 'Completed' || item.status === 'InProgress') {
                            var row = $(`tr[data-item-id="${item.id}"]`);
                            var actionCell = row.find('td:last-child');
                            
                            // Check if button already exists
                            if (actionCell.find('.btn-record-rework').length === 0) {
                                var reworkButton = `<button class="btn btn-sm btn-warning btn-record-rework ml-1" 
                                    data-order-id="${orderId}" data-item-id="${item.id}" 
                                    data-item-name="${item.service?.name || item.serviceName || ''}" 
                                    title="Ghi nhận giờ công làm lại">
                                    <i class="fas fa-redo"></i> Ghi Nhận Làm Lại
                                </button>`;
                                actionCell.append(reworkButton);
                            }
                        }
                    });
                }
            },
            error: function() {
                // Silently fail - button won't be shown
            }
        });
    },
    
    // ✅ 2.4.3: Show modal để ghi nhận giờ công làm lại
    showRecordReworkHoursModal: function(orderId, itemId, itemName) {
        $('#reworkOrderId').val(orderId);
        $('#reworkItemId').val(itemId);
        $('#reworkItemName').text(itemName || '');
        $('#reworkHours').val('');
        $('#reworkNotes').val('');
        $('#recordReworkHoursModal').modal('show');
    },

    // ✅ 3.3: Reset trạng thái tab phí dịch vụ
    resetFeesSection: function(showSpinner) {
        if (!$('#fees').length) {
            return;
        }

        this.currentFeeSummary = null;

        if (showSpinner === undefined || showSpinner) {
            $('#feesLoading').removeClass('d-none');
        } else {
            $('#feesLoading').addClass('d-none');
        }

        $('#feesContent').addClass('d-none');
        $('#feesEmpty').addClass('d-none');
        $('#feesError').addClass('d-none');
        $('#feesTableBody').html('<tr><td colspan="6" class="text-center text-muted py-3">Chưa có dữ liệu</td></tr>');
        $('#feeTotalAmount').text('0 VNĐ');
        $('#feeTotalVat').text('0 VNĐ');
        $('#feeTotalDiscount').text('0 VNĐ');
    },

    fetchFees: function(serviceOrderId) {
        return $.ajax({
            url: '/OrderManagement/GetOrderFees/' + serviceOrderId,
            type: 'GET'
        });
    },

    loadFeesData: function(serviceOrderId, forceRefresh) {
        var self = this;
        if (!$('#fees').length) {
            return;
        }

        if (!serviceOrderId) {
            GarageApp.showError('Không xác định được phiếu sửa chữa để tải dữ liệu phí dịch vụ.');
            return;
        }

        if (!forceRefresh && self.feesLoadedOrderId === serviceOrderId && self.currentFeeSummary) {
            $('#feesLoading').addClass('d-none');
            $('#feesContent').removeClass('d-none');
            return;
        }

        self.resetFeesSection(true);

        self.fetchFees(serviceOrderId)
            .done(function(response) {
                if (!AuthHandler.validateApiResponse(response)) {
                    return;
                }

                var payload = response.data || response.Data || null;
                var summary = null;
                var message = response.message || response.errorMessage || null;
                var isApiError = !response.success;

                if (!isApiError && payload) {
                    if (typeof payload.success === 'boolean') {
                        if (payload.success) {
                            summary = payload.data || payload.Data || null;
                            message = payload.message || payload.errorMessage || message;
                        } else {
                            message = payload.message || payload.errorMessage || 'Chưa có dữ liệu phí dịch vụ.';
                        }
                    } else {
                        summary = payload;
                    }
                } else if (isApiError) {
                    message = response.errorMessage || response.message || message;
                }

                if (summary) {
                    self.currentFeeSummary = summary;
                    self.feeTypesCache = summary.feeTypes || [];
                    self.updateFeesSummary(summary);
                    self.renderFeesTable(summary.fees || []);

                    $('#feesLoading').addClass('d-none');
                    $('#feesContent').removeClass('d-none');
                    $('#feesEmpty').addClass('d-none');
                    $('#feesError').addClass('d-none');

                    self.feesLoadedOrderId = serviceOrderId;
                } else {
                    if (isApiError) {
                        self.handleFeesError(message || 'Không thể tải dữ liệu phí dịch vụ.');
                    } else {
                        self.handleFeesEmpty(message || 'Chưa có dữ liệu phí dịch vụ.');
                    }
                }
            })
            .fail(function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = GarageApp.parseErrorMessage ? GarageApp.parseErrorMessage(xhr) : 'Không thể tải dữ liệu phí dịch vụ.';
                self.handleFeesError(errorMsg);
            });
    },

    updateFeesSummary: function(summary) {
        $('#feeTotalAmount').text(this.formatCurrency(summary.totalAmount || 0));
        $('#feeTotalVat').text(this.formatCurrency(summary.totalVat || 0));
        $('#feeTotalDiscount').text(this.formatCurrency(summary.totalDiscount || 0));
    },

    renderFeesTable: function(fees) {
        var self = this;
        var $tbody = $('#feesTableBody');
        if (!fees || fees.length === 0) {
            $tbody.html('<tr><td colspan="6" class="text-center text-muted py-3">Chưa có dữ liệu</td></tr>');
            return;
        }

        var rows = fees.map(function(fee) {
            return (
                '<tr>' +
                '<td>' + (fee.serviceFeeTypeName || '-') + '</td>' +
                '<td>' + self.formatCurrency(fee.amount || 0) + '</td>' +
                '<td>' + self.formatCurrency(fee.vatAmount || 0) + '</td>' +
                '<td>' + self.formatCurrency(fee.discountAmount || 0) + '</td>' +
                '<td>' + (fee.notes || '-') + '</td>' +
                '<td class="text-center"><span class="badge badge-light">&mdash;</span></td>' +
                '</tr>'
            );
        }).join('');

        $tbody.html(rows);
    },

    openFeesEditor: function() {
        var self = this;
        if (!self.currentFeeSummary) {
            GarageApp.showError('Không có dữ liệu phí để chỉnh sửa.');
            return;
        }

        if (!self.feeTypesCache || self.feeTypesCache.length === 0) {
            GarageApp.showError('Chưa cấu hình loại phí. Vui lòng thêm loại phí trước.');
            return;
        }

        var rowsHtml = self.buildFeeEditorRows(self.currentFeeSummary.fees || []);

        Swal.fire({
            title: 'Chỉnh sửa phí dịch vụ',
            html:
                '<div class="table-responsive fee-editor-table">' +
                '  <table class="table table-sm table-bordered mb-2">' +
                '    <thead class="thead-light">' +
                '      <tr>' +
                '        <th>Loại phí</th>' +
                '        <th style="width: 120px;">Số tiền</th>' +
                '        <th style="width: 120px;">VAT</th>' +
                '        <th style="width: 120px;">Giảm trừ</th>' +
                '        <th>Ghi chú</th>' +
                '        <th style="width: 60px;"></th>' +
                '      </tr>' +
                '    </thead>' +
                '    <tbody id="feeEditorBody">' + rowsHtml + '</tbody>' +
                '  </table>' +
                '  <button type="button" class="btn btn-outline-primary btn-sm" id="btnAddFeeRow"><i class="fas fa-plus mr-1"></i>Thêm dòng</button>' +
                '</div>',
            width: '70%',
            focusConfirm: false,
            showCancelButton: true,
            confirmButtonText: 'Lưu thay đổi',
            cancelButtonText: 'Hủy',
            didOpen: function(popup) {
                self.attachFeeEditorEvents(popup);
            },
            preConfirm: function() {
                var popup = Swal.getPopup();
                var payload = self.collectFeeEditorData(popup);
                if (!payload) {
                    return false;
                }
                return payload;
            }
        }).then(function(result) {
            if (result.isConfirmed && result.value) {
                self.saveFees(result.value);
            }
        });
    },

    buildFeeEditorRows: function(fees) {
        var self = this;
        var list = fees && fees.length ? fees : [{ id: null, serviceFeeTypeId: (self.feeTypesCache[0] || {}).id || 0, amount: 0, vatAmount: 0, discountAmount: 0, notes: '', isManual: true }];
        return list.map(function(fee, index) {
            return (
                '<tr class="fee-editor-row" data-index="' + index + '">' +
                '  <td>' +
                '    <select class="form-control form-control-sm fee-type-select">' + self.getFeeTypeOptions(fee.serviceFeeTypeId) + '</select>' +
                '    <input type="hidden" class="fee-id-input" value="' + (fee.id || '') + '">' +
                '  </td>' +
                '  <td><input type="number" class="form-control form-control-sm fee-amount-input" min="0" step="0.01" value="' + (fee.amount || 0) + '"></td>' +
                '  <td><input type="number" class="form-control form-control-sm fee-vat-input" min="0" step="0.01" value="' + (fee.vatAmount || 0) + '"></td>' +
                '  <td><input type="number" class="form-control form-control-sm fee-discount-input" min="0" step="0.01" value="' + (fee.discountAmount || 0) + '"></td>' +
                '  <td><input type="text" class="form-control form-control-sm fee-notes-input" value="' + (fee.notes || '') + '"></td>' +
                '  <td class="text-center"><button type="button" class="btn btn-outline-danger btn-sm remove-fee-row" title="Xóa dòng"><i class="fas fa-trash"></i></button></td>' +
                '</tr>'
            );
        }).join('');
    },

    getFeeTypeOptions: function(selectedId) {
        return this.feeTypesCache.map(function(type) {
            var selected = type.id === selectedId ? 'selected' : '';
            return '<option value="' + type.id + '" ' + selected + '>' + type.name + '</option>';
        }).join('');
    },

    attachFeeEditorEvents: function(popup) {
        var self = this;
        var body = $(popup).find('#feeEditorBody');
        $(popup).find('#btnAddFeeRow').on('click', function() {
            var newIndex = body.find('.fee-editor-row').length;
            var newRow = self.buildFeeEditorRows([{ id: null, serviceFeeTypeId: (self.feeTypesCache[0] || {}).id || 0, amount: 0, vatAmount: 0, discountAmount: 0, notes: '', isManual: true }]);
            body.append(newRow);
        });

        body.on('click', '.remove-fee-row', function() {
            if (body.find('.fee-editor-row').length === 1) {
                GarageApp.showWarning('Phải có ít nhất một dòng phí.');
                return;
            }
            $(this).closest('.fee-editor-row').remove();
        });
    },

    collectFeeEditorData: function(popup) {
        var rows = $(popup).find('.fee-editor-row');
        if (!rows.length) {
            GarageApp.showError('Phải có ít nhất một dòng phí.');
            return false;
        }

        var result = [];
        var hasError = false;

        rows.each(function() {
            if (hasError) {
                return;
            }

            var $row = $(this);
            var feeTypeId = parseInt($row.find('.fee-type-select').val(), 10);
            var amount = parseFloat($row.find('.fee-amount-input').val() || '0');
            var vat = parseFloat($row.find('.fee-vat-input').val() || '0');
            var discount = parseFloat($row.find('.fee-discount-input').val() || '0');
            var notes = $row.find('.fee-notes-input').val() || '';
            var idValue = $row.find('.fee-id-input').val();

            if (!feeTypeId) {
                GarageApp.showError('Vui lòng chọn loại phí.');
                hasError = true;
                return;
            }

            if (amount < 0 || vat < 0 || discount < 0) {
                GarageApp.showError('Số tiền không được nhỏ hơn 0.');
                hasError = true;
                return;
            }

            result.push({
                Id: idValue ? parseInt(idValue, 10) : null,
                ServiceFeeTypeId: feeTypeId,
                Amount: amount,
                VatAmount: vat,
                DiscountAmount: discount,
                ReferenceSource: null,
                Notes: notes,
                IsManual: true
            });
        });

        if (hasError) {
            return false;
        }

        return { Fees: result };
    },

    saveFees: function(payload) {
        var self = this;
        var serviceOrderId = self.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (!serviceOrderId) {
            GarageApp.showError('Không xác định được phiếu sửa chữa.');
            return;
        }

        self.isEditingFees = true;
        GarageApp.showLoading('Đang lưu phí dịch vụ...');

        $.ajax({
            url: '/OrderManagement/UpdateOrderFees/' + serviceOrderId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(payload)
        })
            .done(function(response) {
                if (!AuthHandler.validateApiResponse(response)) {
                    return;
                }

                if (response.success && response.data) {
                    var summaryPayload = response.data.data || response.data.Data || response.data;
                    if (summaryPayload) {
                        self.currentFeeSummary = summaryPayload;
                        self.feeTypesCache = summaryPayload.feeTypes || [];
                        self.updateFeesSummary(summaryPayload);
                        self.renderFeesTable(summaryPayload.fees || []);
                        self.feesLoadedOrderId = serviceOrderId;
                        $('#feesLoading').addClass('d-none');
                        $('#feesContent').removeClass('d-none');
                        $('#feesEmpty').addClass('d-none');
                        $('#feesError').addClass('d-none');
                        GarageApp.showSuccess(response.data.message || 'Đã cập nhật phí dịch vụ thành công.');
                    } else {
                        GarageApp.showError('Không lấy được dữ liệu sau khi cập nhật.');
                    }
                } else {
                    var errorMsg = GarageApp.parseErrorMessage(response) || response.errorMessage || response.message || 'Không thể cập nhật phí dịch vụ.';
                    GarageApp.showError(errorMsg);
                }
            })
            .fail(function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                    return;
                }
                var errorMsg = GarageApp.parseErrorMessage ? GarageApp.parseErrorMessage(xhr) : 'Không thể cập nhật phí dịch vụ.';
                GarageApp.showError(errorMsg);
            })
            .always(function() {
                self.isEditingFees = false;
                GarageApp.hideLoading();
            });
    },

    handleFeesEmpty: function(message) {
        $('#feesLoading').addClass('d-none');
        $('#feesContent').addClass('d-none');
        $('#feesError').addClass('d-none');
        $('#feesEmpty').removeClass('d-none').find('span').text(message || 'Chưa có dữ liệu phí dịch vụ.');
    },

    handleFeesError: function(message) {
        $('#feesLoading').addClass('d-none');
        $('#feesContent').addClass('d-none');
        $('#feesEmpty').addClass('d-none');
        $('#feesError').removeClass('d-none').find('span').text(message || 'Không thể tải dữ liệu phí dịch vụ.');
    },
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

    // ✅ 2.3.4: Load progress when progress tab is shown
    $('#progress-tab').on('shown.bs.tab', function(e) {
        var serviceOrderId = OrderManagement.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (serviceOrderId) {
            OrderManagement.loadOrderProgress(serviceOrderId);
        } else {
            $('#progressContent').html('<p class="text-muted">Vui lòng xem chi tiết phiếu sửa chữa trước.</p>');
        }
    });

    // ✅ 3.1: Load settlement data when tab is shown
    $('#settlement-tab').on('shown.bs.tab', function() {
        var serviceOrderId = OrderManagement.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (serviceOrderId) {
            OrderManagement.loadSettlementData(serviceOrderId);
        } else {
            OrderManagement.resetSettlementSection(false);
            $('#settlementEmptyMessage').text('Vui lòng xem chi tiết phiếu sửa chữa trước.');
            $('#settlementEmpty').removeClass('d-none');
        }
    });

    $('#warranty-tab').on('shown.bs.tab', function() {
        var serviceOrderId = OrderManagement.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (serviceOrderId) {
            OrderManagement.loadWarrantyData(serviceOrderId);
        } else {
            OrderManagement.resetWarrantySection(false);
            $('#warrantyEmpty').removeClass('d-none');
        }
    });

    $('#btnRefreshWarranty').on('click', function() {
        var serviceOrderId = OrderManagement.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (serviceOrderId) {
            OrderManagement.loadWarrantyData(serviceOrderId, true);
        }
    });

    $('#btnGenerateWarranty').on('click', function() {
        var serviceOrderId = OrderManagement.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (serviceOrderId) {
            OrderManagement.generateWarranty(serviceOrderId);
        }
    });

    $('#btnCreateWarrantyClaim, #btnShowCreateClaim').on('click', function() {
        var serviceOrderId = OrderManagement.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (serviceOrderId && OrderManagement.currentWarranty) {
            OrderManagement.openCreateWarrantyClaimModal(serviceOrderId, OrderManagement.currentWarranty);
        }
    });

    // ✅ 2.4: Load QC when QC tab is shown
    $('#qc-tab').on('shown.bs.tab', function(e) {
        var serviceOrderId = OrderManagement.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (serviceOrderId) {
            OrderManagement.loadQCInfo(serviceOrderId);
        } else {
            $('#qcContent').html('<p class="text-muted">Vui lòng xem chi tiết phiếu sửa chữa trước.</p>');
        }
    });
    
    // ✅ 2.4.3: Bind event cho Record Rework Hours Modal
    $('#btnSubmitReworkHours').on('click', function() {
        var orderId = $('#reworkOrderId').val();
        var itemId = $('#reworkItemId').val();
        var reworkHours = $('#reworkHours').val();
        var notes = $('#reworkNotes').val();
        
        if (!reworkHours || parseFloat(reworkHours) <= 0) {
            GarageApp.showError('Vui lòng nhập số giờ công làm lại lớn hơn 0');
            return;
        }
        
        var data = {
            reworkHours: parseFloat(reworkHours),
            notes: notes || null
        };
        
        $.ajax({
            url: '/QCManagement/RecordReworkHours/' + orderId + '/' + itemId,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess(response.message || 'Đã ghi nhận giờ công làm lại thành công');
                        $('#recordReworkHoursModal').modal('hide');
                        // Reload order details
                        OrderManagement.viewOrder(orderId);
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi ghi nhận giờ công làm lại');
                    }
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi ghi nhận giờ công làm lại');
                }
            }
        });
    });
    
    // Reset form khi modal đóng
    $('#recordReworkHoursModal').on('hidden.bs.modal', function() {
        $('#recordReworkHoursForm')[0].reset();
        $('#reworkOrderId').val('');
        $('#reworkItemId').val('');
        $('#reworkItemName').text('');
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

        // ✅ Nếu đang edit và checkbox reset được chọn → Set Status = "Identified"
        if (isEdit && $('#resetToIdentified').is(':checked')) {
            formData.append('Status', 'Identified');
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

    $('#fees-tab').on('shown.bs.tab', function() {
        var serviceOrderId = OrderManagement.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (serviceOrderId) {
            OrderManagement.loadFeesData(serviceOrderId);
        } else {
            OrderManagement.resetFeesSection(false);
            $('#feesEmpty').removeClass('d-none').find('span').text('Vui lòng xem chi tiết phiếu sửa chữa trước.');
        }
    });

    $('#btnRefreshFees').on('click', function() {
        var serviceOrderId = OrderManagement.currentServiceOrderId || $('#viewOrderModal').data('order-id');
        if (serviceOrderId) {
            OrderManagement.loadFeesData(serviceOrderId, true);
        }
    });

    $('#btnEditFees').on('click', function() {
        OrderManagement.openFeesEditor();
    });
});