/**
 * Appointment Management Module
 * 
 * Handles all appointment-related operations
 * CRUD operations for appointments
 */

window.AppointmentManagement = {
    // DataTable instance
    appointmentTable: null,

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
            { data: 'id', title: 'Mã', width: '60px' },
            { data: 'customerName', title: 'Khách Hàng' },
            { data: 'vehicleLicensePlate', title: 'Xe' },
            { 
                data: 'appointmentDate', 
                title: 'Ngày',
                render: DataTablesUtility.renderDate
            },
            { data: 'appointmentTime', title: 'Giờ' },
            { data: 'serviceType', title: 'Loại Dịch Vụ' },
            { 
                data: 'status', 
                title: 'Trạng Thái',
                render: function(data, type, row) {
                    var badgeClass = 'badge-secondary';
                    var displayText = data;
                    
                    switch(data) {
                        case 'Đã Đặt Lịch': badgeClass = 'badge-primary'; break;
                        case 'Đã Xác Nhận': badgeClass = 'badge-info'; break;
                        case 'Đang Thực Hiện': badgeClass = 'badge-warning'; break;
                        case 'Hoàn Thành': badgeClass = 'badge-success'; break;
                        case 'Đã Hủy': badgeClass = 'badge-danger'; break;
                        case 'Không Đến': badgeClass = 'badge-dark'; break;
                    }
                    return `<span class="badge ${badgeClass}">${displayText}</span>`;
                }
            },
            {
                data: null,
                title: 'Thao Tác',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    var actions = `
                        <button class="btn btn-info btn-sm view-appointment" data-id="${row.id}">
                            <i class="fas fa-eye"></i>
                        </button>
                    `;
                    
                    // Chỉ hiển thị nút Edit và Delete khi trạng thái chưa hoàn thành
                    // Kiểm tra nhiều trường hợp có thể của trạng thái "hoàn thành"
                    var isCompleted = row.status === 'Completed' || 
                                     row.status === 'Hoàn Thành' ||  // Chữ T viết hoa (từ TranslateStatus)
                                     row.status === 'Hoàn thành' || 
                                     row.status === 'completed' || 
                                     row.status === 'hoàn thành' ||
                                     (row.status && row.status.toLowerCase().includes('hoàn thành')) ||
                                     (row.status && row.status.toLowerCase().includes('completed'));
                    
                    if (!isCompleted) {
                        actions += `
                            <button class="btn btn-warning btn-sm edit-appointment" data-id="${row.id}">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-danger btn-sm delete-appointment" data-id="${row.id}">
                                <i class="fas fa-trash"></i>
                            </button>
                        `;
                    }
                    
                    return actions;
                }
            }
        ];
        
        // ✅ FIX: Gọi qua AppointmentManagementController thay vì gọi trực tiếp API
        // API endpoint trả về PagedResponse, nhưng controller Web đã xử lý và trả về format phù hợp với DataTables
        this.appointmentTable = DataTablesUtility.initServerSideTable('#appointmentTable', '/AppointmentManagement/GetAppointments', columns, {
            order: [[0, 'desc']],
            pageLength: 10
        });
    },

    // Load dropdowns
    loadDropdowns: function() {
        this.loadCustomers();
        this.loadEmployees();
        this.loadAppointmentTypes();
    },

    // Load customers
    loadCustomers: function(callback) {
        $.ajax({
            url: '/CustomerManagement/GetActiveCustomers',
            type: 'GET',
            success: function(response) {
                // GetActiveCustomers returns data directly, not wrapped in success/data
                if (response && Array.isArray(response)) {
                    var customers = response;
                    var options = '<option value="">Chọn khách hàng</option>';
                    customers.forEach(function(customer) {
                        options += `<option value="${customer.id}">${customer.name}</option>`;
                    });
                    $('#createCustomerId, #editCustomerId').html(options);
                    
                    // ✅ FIX: Gọi callback nếu có
                    if (callback && typeof callback === 'function') {
                        callback();
                    }
                } else {
                    if (callback && typeof callback === 'function') {
                        callback();
                    }
                }
            },
            error: function(xhr, status, error) {
                if (callback && typeof callback === 'function') {
                    callback();
                }
            }
        });
    },

    // Load employees
    loadEmployees: function(callback) {
        $.ajax({
            url: '/AppointmentManagement/GetEmployees',
            type: 'GET',
            success: function(response) {
                if (response && Array.isArray(response)) {
                    var employees = response;
                    var options = '<option value="">Chọn nhân viên</option>';
                    employees.forEach(function(employee) {
                        options += `<option value="${employee.id}">${employee.text}</option>`;
                    });
                    $('#createAssignedEmployeeId, #editAssignedEmployeeId').html(options);
                    
                    // ✅ FIX: Gọi callback nếu có
                    if (callback && typeof callback === 'function') {
                        callback();
                    }
                } else {
                    if (callback && typeof callback === 'function') {
                        callback();
                    }
                }
            },
            error: function(xhr, status, error) {
                if (callback && typeof callback === 'function') {
                    callback();
                }
            }
        });
    },

    // Load appointment types
    loadAppointmentTypes: function() {
        $.ajax({
            url: '/AppointmentManagement/GetAppointmentTypes',
            type: 'GET',
            success: function(response) {
                if (response && Array.isArray(response)) {
                    var types = response;
                    var options = '<option value="">-- Chọn Loại Dịch Vụ --</option>';
                    types.forEach(function(type) {
                        options += `<option value="${type.id}">${type.text}</option>`;
                    });
                    $('#createServiceType, #editServiceType').html(options);
                } else {
                }
            },
            error: function(xhr, status, error) {
            }
        });
    },

    // Load available vehicles (not currently in service)
    loadAvailableVehicles: function(targetSelect, callback) {
        var self = this;
        $.ajax({
            url: '/AppointmentManagement/GetAvailableVehicles',
            type: 'GET',
            success: function(response) {
                if (response && Array.isArray(response)) {
                    var vehicles = response;
                    var options = '<option value="">-- Chọn Xe --</option>';
                    vehicles.forEach(function(vehicle) {
                        options += `<option value="${vehicle.id}" data-customer-id="${vehicle.customerId}" data-customer-name="${vehicle.customerName}">${vehicle.text}</option>`;
                    });
                    $(targetSelect).html(options);
                    
                    // Setup vehicle change handler if this is for create/edit modal
                    if (targetSelect === '#createVehicleId' || targetSelect === '#editVehicleId') {
                        self.setupVehicleChangeHandler(targetSelect);
                    }
                    
                    // ✅ FIX: Gọi callback nếu có để set value sau khi load xong
                    if (callback && typeof callback === 'function') {
                        callback();
                    }
                } else {
                    $(targetSelect).html('<option value="">-- Chọn Xe --</option>');
                    if (callback && typeof callback === 'function') {
                        callback();
                    }
                }
            },
            error: function(xhr, status, error) {
                $(targetSelect).html('<option value="">-- Chọn Xe --</option>');
                if (callback && typeof callback === 'function') {
                    callback();
                }
            }
        });
    },

    setupVehicleChangeHandler: function(vehicleSelect) {
        var self = this;
        
        // Event handler cho dropdown xe
        $(vehicleSelect).on('change', function() {
            var selectedVehicle = $(this).find('option:selected');
            var customerId = selectedVehicle.data('customer-id');
            var customerName = selectedVehicle.data('customer-name');
            
            if (customerId && customerName) {
                // Tự động set khách hàng tương ứng
                var customerSelect = $(this).closest('.modal').find('#createCustomerId, #editCustomerId');
                customerSelect.val(customerId).trigger('change');
                
                // Disable dropdown khách hàng
                customerSelect.prop('disabled', true);
                customerSelect.attr('title', 'Khách hàng được tự động chọn theo xe đã chọn');
            } else {
                // Reset về trạng thái ban đầu
                var customerSelect = $(this).closest('.modal').find('#createCustomerId, #editCustomerId');
                customerSelect.prop('disabled', false);
                customerSelect.removeAttr('title');
                customerSelect.val('');
            }
        });
    },

    // Load vehicles by customer (keep for backward compatibility)
    loadVehiclesByCustomer: function(customerId, targetSelect, callback) {
        var self = this;
        if (!customerId) {
            $(targetSelect).html('<option value="">-- Chọn Xe --</option>');
            if (callback && typeof callback === 'function') {
                callback();
            }
            return;
        }

        $.ajax({
            url: '/AppointmentManagement/GetVehiclesByCustomer/' + customerId,
            type: 'GET',
            success: function(response) {
                if (response && Array.isArray(response)) {
                    var vehicles = response;
                    var options = '<option value="">-- Chọn Xe --</option>';
                    vehicles.forEach(function(vehicle) {
                        options += `<option value="${vehicle.id}">${vehicle.text}</option>`;
                    });
                    $(targetSelect).html(options);
                    
                    // Setup vehicle change handler if this is for create/edit modal
                    if (targetSelect === '#createVehicleId' || targetSelect === '#editVehicleId') {
                        // Remove existing handler to avoid duplicate
                        $(targetSelect).off('change');
                        // Setup new handler
                        if (self && self.setupVehicleChangeHandler) {
                            self.setupVehicleChangeHandler(targetSelect);
                        }
                    }
                    
                    // ✅ FIX: Gọi callback nếu có để set value sau khi load xong
                    if (callback && typeof callback === 'function') {
                        callback();
                    }
                } else {
                    $(targetSelect).html('<option value="">-- Chọn Xe --</option>');
                    if (callback && typeof callback === 'function') {
                        callback();
                    }
                }
            },
            error: function(xhr, status, error) {
                $(targetSelect).html('<option value="">-- Chọn Xe --</option>');
                if (callback && typeof callback === 'function') {
                    callback();
                }
            }
        });
    },

    // Bind events
    bindEvents: function() {
        var self = this;

        // Add appointment button
        $(document).on('click', '#addAppointmentBtn', function() {
            self.showCreateModal();
        });

        // Load available vehicles when modal opens
        $(document).on('shown.bs.modal', '#createAppointmentModal', function() {
            // Reset customer dropdown
            $('#createCustomerId').prop('disabled', false).val('').trigger('change');
            $('#createVehicleId').html('<option value="">-- Chọn Xe --</option>');
        });

        $(document).on('shown.bs.modal', '#editAppointmentModal', function() {
            // ✅ FIX: Lấy appointment data từ modal data attribute
            var appointment = $('#editAppointmentModal').data('appointment-data');
            
            if (appointment) {
                // Reset và enable dropdowns
                $('#editCustomerId').prop('disabled', false);
                $('#editVehicleId').html('<option value="">-- Chọn Xe --</option>');
                
                // ✅ FIX: Đợi một chút để đảm bảo DOM đã render và dropdowns đã được populate
                // Modal animation cần thời gian để hoàn thành
                setTimeout(function() {
                    setCustomerAndVehicle();
                    setEmployee();
                }, 100);
                
                var setCustomerAndVehicle = function() {
                    var $customerSelect = $('#editCustomerId');
                    var customerId = appointment.customerId;
                    
                    if (customerId) {
                        // Kiểm tra xem option đã tồn tại chưa
                        var optionExists = $customerSelect.find('option[value="' + customerId + '"]').length > 0;
                        
                        if (!optionExists) {
                            // Nếu option chưa tồn tại, thêm option mới
                            var customerName = appointment.customer?.name || appointment.customerName || 'Khách hàng #' + customerId;
                            $customerSelect.append(`<option value="${customerId}">${customerName}</option>`);
                        }
                        
                        // ✅ FIX: Destroy Select2 trước nếu đã có
                        if ($customerSelect.hasClass('select2-hidden-accessible')) {
                            $customerSelect.select2('destroy');
                        }
                        
                        // ✅ FIX: Set value TRƯỚC khi reinitialize Select2
                        $customerSelect.val(customerId);
                        
                        // ✅ FIX: Reinitialize Select2 SAU KHI đã set value
                        $customerSelect.select2({
                            placeholder: '-- Chọn Khách Hàng --',
                            allowClear: true,
                            width: '100%'
                        });
                        
                        // ✅ FIX: Trigger change để Select2 update display
                        $customerSelect.trigger('change.select2');
                        
                        // ✅ FIX: Load vehicles của customer này, sau đó mới set vehicle value
                        AppointmentManagement.loadVehiclesByCustomer(customerId, '#editVehicleId', function() {
                            // Sau khi load vehicles xong, set vehicle value
                            var $vehicleSelect = $('#editVehicleId');
                            var vehicleId = appointment.vehicleId;
                            
                            if (vehicleId) {
                                // Kiểm tra xem option đã tồn tại chưa
                                var vehicleOptionExists = $vehicleSelect.find('option[value="' + vehicleId + '"]').length > 0;
                                
                                if (!vehicleOptionExists) {
                                    // Nếu option chưa tồn tại, thêm option mới
                                    var vehicleText = appointment.vehicle?.licensePlate || 
                                                     (appointment.vehicle?.brand && appointment.vehicle?.model 
                                                      ? appointment.vehicle.brand + ' ' + appointment.vehicle.model + ' - ' + appointment.vehicle.licensePlate
                                                      : 'Xe #' + vehicleId);
                                    
                                    $vehicleSelect.append(`<option value="${vehicleId}">${vehicleText}</option>`);
                                }
                                
                                // ✅ FIX: Destroy Select2 trước nếu đã có
                                if ($vehicleSelect.hasClass('select2-hidden-accessible')) {
                                    $vehicleSelect.select2('destroy');
                                }
                                
                                // ✅ FIX: Set value TRƯỚC khi reinitialize Select2
                                $vehicleSelect.val(vehicleId);
                                
                                // ✅ FIX: Reinitialize Select2 SAU KHI đã set value
                                $vehicleSelect.select2({
                                    placeholder: '-- Chọn Xe --',
                                    allowClear: true,
                                    width: '100%'
                                });
                                
                                // ✅ FIX: Trigger change để Select2 update display
                                $vehicleSelect.trigger('change.select2');
                            }
                        });
                    } else {
                        // Nếu không có customerId, load tất cả available vehicles
                        AppointmentManagement.loadAvailableVehicles('#editVehicleId', function() {
                            var $vehicleSelect = $('#editVehicleId');
                            var vehicleId = appointment.vehicleId;
                            
                            if (vehicleId) {
                                var vehicleOptionExists = $vehicleSelect.find('option[value="' + vehicleId + '"]').length > 0;
                                
                                if (!vehicleOptionExists) {
                                    var vehicleText = appointment.vehicle?.licensePlate || 
                                                     (appointment.vehicle?.brand && appointment.vehicle?.model 
                                                      ? appointment.vehicle.brand + ' ' + appointment.vehicle.model + ' - ' + appointment.vehicle.licensePlate
                                                      : 'Xe #' + vehicleId);
                                    
                                    $vehicleSelect.append(`<option value="${vehicleId}">${vehicleText}</option>`);
                                }
                                
                                // ✅ FIX: Destroy Select2 trước nếu đã có
                                if ($vehicleSelect.hasClass('select2-hidden-accessible')) {
                                    $vehicleSelect.select2('destroy');
                                }
                                
                                // ✅ FIX: Set value TRƯỚC khi reinitialize Select2
                                $vehicleSelect.val(vehicleId);
                                
                                // ✅ FIX: Reinitialize Select2 SAU KHI đã set value
                                $vehicleSelect.select2({
                                    placeholder: '-- Chọn Xe --',
                                    allowClear: true,
                                    width: '100%'
                                });
                                
                                // ✅ FIX: Trigger change để Select2 update display
                                $vehicleSelect.trigger('change.select2');
                            }
                        });
                    }
                };
                
                // ✅ FIX: Set employee (kỹ thuật viên) value
                var setEmployee = function() {
                    var $employeeSelect = $('#editAssignedEmployeeId');
                    var assignedToId = appointment.assignedToId;
                    
                    if (assignedToId) {
                        // Kiểm tra xem option đã tồn tại chưa
                        var optionExists = $employeeSelect.find('option[value="' + assignedToId + '"]').length > 0;
                        
                        if (!optionExists) {
                            // Nếu option chưa tồn tại, thêm option mới
                            var employeeName = appointment.assignedTo?.name || appointment.assignedEmployeeName || 'Nhân viên #' + assignedToId;
                            $employeeSelect.append(`<option value="${assignedToId}">${employeeName}</option>`);
                        }
                        
                        // ✅ FIX: Set value (không cần Select2 vì employee dropdown thường là select thông thường)
                        $employeeSelect.val(assignedToId);
                        
                        // ✅ FIX: Trigger change để update display
                        $employeeSelect.trigger('change');
                    }
                };
            }
        });
        
        // ✅ FIX: Load vehicles khi customer change (cho cả create và edit modal)
        $(document).on('change', '#createCustomerId, #editCustomerId', function() {
            var customerId = $(this).val();
            var isCreate = $(this).attr('id') === 'createCustomerId';
            var vehicleSelect = isCreate ? '#createVehicleId' : '#editVehicleId';
            
            if (customerId) {
                self.loadVehiclesByCustomer(customerId, vehicleSelect);
            } else {
                $(vehicleSelect).html('<option value="">-- Chọn Xe --</option>');
            }
        });

        // View appointment
        $(document).on('click', '.view-appointment', function() {
            var id = $(this).data('id');
            self.viewAppointment(id);
        });

        // Edit appointment
        $(document).on('click', '.edit-appointment', function() {
            var id = $(this).data('id');
            self.editAppointment(id);
        });

        // Delete appointment
        $(document).on('click', '.delete-appointment', function() {
            var id = $(this).data('id');
            self.deleteAppointment(id);
        });

        // Create appointment form
        $(document).on('submit', '#createAppointmentForm', function(e) {
            e.preventDefault();
            self.createAppointment();
        });

        // Update appointment form
        $(document).on('submit', '#updateAppointmentForm', function(e) {
            e.preventDefault();
            self.updateAppointment();
        });
    },

    // Show create modal
    showCreateModal: function() {
        $('#createAppointmentModal').modal('show');
        $('#createAppointmentForm')[0].reset();
        $('#createVehicleId').html('<option value="">Select Vehicle</option>');
    },

    // View appointment
    viewAppointment: function(id) {
        var self = this;
        
        $.ajax({
            url: '/AppointmentManagement/GetAppointment/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        self.populateViewModal(response.data);
                        $('#viewAppointmentModal').modal('show');
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error loading appointment');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error loading appointment');
                }
            }
        });
    },

    // Edit appointment
    editAppointment: function(id) {
        var self = this;
        
        $.ajax({
            url: '/AppointmentManagement/GetAppointment/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        // ✅ FIX: Lưu appointment data vào modal data attribute để xử lý trong shown.bs.modal
                        $('#editAppointmentModal').data('appointment-data', response.data);
                        self.populateEditModal(response.data);
                        $('#editAppointmentModal').modal('show');
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error loading appointment');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error loading appointment');
                }
            }
        });
    },

    // Delete appointment
    deleteAppointment: function(id) {
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
                    url: '/AppointmentManagement/DeleteAppointment/' + id,
                    type: 'DELETE',
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Appointment deleted successfully!');
                                self.appointmentTable.ajax.reload();
                            } else {
                                GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error deleting appointment');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Error deleting appointment');
                        }
                    }
                });
            }
        });
    },

    // Create appointment
    createAppointment: function() {
        var self = this;
        var formData = {
            CustomerId: parseInt($('#createCustomerId').val()),
            VehicleId: parseInt($('#createVehicleId').val()),
            AppointmentDate: $('#createAppointmentDate').val(),
            AppointmentTime: $('#createAppointmentTime').val(),
            ServiceType: $('#createServiceType').val(),
            Description: $('#createDescription').val(),
            AssignedEmployeeId: $('#createAssignedEmployeeId').val() ? parseInt($('#createAssignedEmployeeId').val()) : null
        };

        $.ajax({
            url: '/AppointmentManagement/CreateAppointment',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Appointment created successfully!');
                        $('#createAppointmentModal').modal('hide');
                        self.appointmentTable.ajax.reload();
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error creating appointment');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error creating appointment');
                }
            }
        });
    },

    // Update appointment
    updateAppointment: function() {
        var self = this;
        var appointmentId = $('#editId').val();
        var formData = {
            Id: parseInt(appointmentId),
            CustomerId: parseInt($('#editCustomerId').val()),
            VehicleId: parseInt($('#editVehicleId').val()),
            ScheduledDateTime: $('#editAppointmentDate').val() + 'T' + $('#editAppointmentTime').val(),
            AppointmentType: $('#editServiceType').val(),
            ServiceRequested: $('#editDescription').val(),
            Status: $('#editStatus').val(),
            AssignedToId: $('#editAssignedEmployeeId').val() ? parseInt($('#editAssignedEmployeeId').val()) : null
        };

        $.ajax({
            url: '/AppointmentManagement/UpdateAppointment/' + appointmentId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Appointment updated successfully!');
                        $('#editAppointmentModal').modal('hide');
                        self.appointmentTable.ajax.reload();
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error updating appointment');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error updating appointment');
                }
            }
        });
    },

    // Populate view modal
    populateViewModal: function(appointment) {
        $('#viewCustomer').text(appointment.customer?.name || appointment.customerName || 'Không xác định');
        $('#viewVehicle').text(appointment.vehicle?.licensePlate || appointment.vehicleLicensePlate || 'Không xác định');
        $('#viewAppointmentDate').text(appointment.scheduledDateTime ? new Date(appointment.scheduledDateTime).toLocaleDateString('vi-VN') : appointment.appointmentDate || '');
        $('#viewAppointmentTime').text(appointment.scheduledDateTime ? new Date(appointment.scheduledDateTime).toLocaleTimeString('vi-VN', {hour: '2-digit', minute: '2-digit'}) : appointment.appointmentTime || '');
        $('#viewServiceType').text(this.translateServiceType(appointment.appointmentType || appointment.serviceType || ''));
        $('#viewDescription').text(appointment.serviceRequested || appointment.description || 'Không có mô tả');
        $('#viewStatus').text(this.translateStatus(appointment.status || ''));
        $('#viewAssignedEmployee').text(appointment.assignedTo?.name || appointment.assignedEmployeeName || 'Chưa phân công');
    },

    // Translate service type to Vietnamese
    translateServiceType: function(serviceType) {
        const translations = {
            'Maintenance': 'Bảo Dưỡng',
            'Repair': 'Sửa Chữa',
            'Inspection': 'Kiểm Tra',
            'Diagnostic': 'Chẩn Đoán',
            'Service': 'Dịch Vụ',
            'Oil Change': 'Thay Dầu',
            'Other': 'Khác'
        };
        return translations[serviceType] || serviceType;
    },

    // Translate status to Vietnamese
    translateStatus: function(status) {
        const translations = {
            'Scheduled': 'Đã Đặt Lịch',
            'Confirmed': 'Đã Xác Nhận',
            'InProgress': 'Đang Thực Hiện',
            'Completed': 'Hoàn Thành',
            'Cancelled': 'Đã Hủy',
            'NoShow': 'Không Đến'
        };
        return translations[status] || status;
    },

    // Populate edit modal
    populateEditModal: function(appointment) {
        var self = this;
        
        // ✅ FIX: Load customers với callback để đảm bảo đã load xong
        this.loadCustomers(function() {
            // Sau khi customers đã load xong, set các values khác
            $('#editId').val(appointment.id);
            
            // Set other values
            $('#editAppointmentDate').val(appointment.scheduledDateTime ? new Date(appointment.scheduledDateTime).toISOString().split('T')[0] : '');
            $('#editAppointmentTime').val(appointment.scheduledDateTime ? new Date(appointment.scheduledDateTime).toTimeString().slice(0, 5) : '');
            $('#editServiceType').val(appointment.appointmentType);
            $('#editDescription').val(appointment.serviceRequested || appointment.description);
            $('#editStatus').val(appointment.status);
            $('#editAssignedEmployeeId').val(appointment.assignedToId);
        });
        
        // Load other dropdowns
        this.loadEmployees();
        this.loadAppointmentTypes();
        
        // ✅ FIX: Customer và Vehicle sẽ được xử lý trong shown.bs.modal event
        // Nhưng đảm bảo loadCustomers đã hoàn thành trước
    }
};

// Initialize when document is ready
$(document).ready(function() {
    AppointmentManagement.init();
});