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
        
        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable('#appointmentTable')) {
            $('#appointmentTable').DataTable().destroy();
        }
        
        this.appointmentTable = DataTablesVietnamese.init('#appointmentTable', {
            processing: true,
            serverSide: false,
            ajax: {
                url: '/AppointmentManagement/GetAppointments',
                type: 'GET',
                error: function(xhr, status, error) {
                    if (AuthHandler.isUnauthorized(xhr)) {
                        AuthHandler.handleUnauthorized(xhr, true);
                    } else {
                        GarageApp.showError('Error loading appointments');
                    }
                }
            },
            columns: [
                { data: 'id', title: 'Mã', width: '60px' },
                { data: 'customerName', title: 'Khách Hàng' },
                { data: 'vehicleLicensePlate', title: 'Xe' },
                { data: 'appointmentDate', title: 'Ngày' },
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
            ],
            order: [[0, 'desc']],
            pageLength: 10,
            responsive: true
        });
    },

    // Load dropdowns
    loadDropdowns: function() {
        this.loadCustomers();
        this.loadEmployees();
        this.loadAppointmentTypes();
    },

    // Load customers
    loadCustomers: function() {
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
                } else {
                    console.error('Invalid customer data format:', response);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading customers:', error);
            }
        });
    },

    // Load employees
    loadEmployees: function() {
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
                } else {
                    console.error('Invalid employee data format:', response);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading employees:', error);
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
                    console.error('Invalid appointment types data format:', response);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading appointment types:', error);
            }
        });
    },

    // Load available vehicles (not currently in service)
    loadAvailableVehicles: function(targetSelect) {
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
                } else {
                    console.error('Invalid vehicle data format:', response);
                    $(targetSelect).html('<option value="">-- Chọn Xe --</option>');
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading available vehicles:', error);
                $(targetSelect).html('<option value="">-- Chọn Xe --</option>');
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
    loadVehiclesByCustomer: function(customerId, targetSelect) {
        if (!customerId) {
            $(targetSelect).html('<option value="">Select Vehicle</option>');
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
                } else {
                    console.error('Invalid vehicle data format:', response);
                    $(targetSelect).html('<option value="">-- Chọn Xe --</option>');
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading vehicles:', error);
                $(targetSelect).html('<option value="">-- Chọn Xe --</option>');
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
            self.loadAvailableVehicles('#createVehicleId');
            // Reset customer dropdown
            $('#createCustomerId').prop('disabled', true).val('').trigger('change');
        });

        $(document).on('shown.bs.modal', '#editAppointmentModal', function() {
            self.loadAvailableVehicles('#editVehicleId');
            // Reset customer dropdown  
            $('#editCustomerId').prop('disabled', true).val('').trigger('change');
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
                        GarageApp.showError(response.message || 'Error loading appointment');
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
                        self.populateEditModal(response.data);
                        $('#editAppointmentModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Error loading appointment');
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
                                GarageApp.showError(response.message || 'Error deleting appointment');
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
                        GarageApp.showError(response.message || 'Error creating appointment');
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
                        GarageApp.showError(response.message || 'Error updating appointment');
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
        
        // Load all dropdowns first
        this.loadCustomers();
        this.loadEmployees();
        this.loadAppointmentTypes();
        
        // Set values after dropdowns are loaded
        setTimeout(function() {
            $('#editId').val(appointment.id);
            
            // Set other values
            $('#editAppointmentDate').val(appointment.scheduledDateTime ? new Date(appointment.scheduledDateTime).toISOString().split('T')[0] : '');
            $('#editAppointmentTime').val(appointment.scheduledDateTime ? new Date(appointment.scheduledDateTime).toTimeString().slice(0, 5) : '');
            $('#editServiceType').val(appointment.appointmentType);
            $('#editDescription').val(appointment.serviceRequested || appointment.description);
            $('#editStatus').val(appointment.status);
            $('#editAssignedEmployeeId').val(appointment.assignedToId);
            
            // Set vehicle first, customer will be auto-set by vehicle change handler
            $('#editVehicleId').val(appointment.vehicleId);
            $('#editVehicleId').trigger('change');
        }, 200);
    }
};

// Initialize when document is ready
$(document).ready(function() {
    AppointmentManagement.init();
});