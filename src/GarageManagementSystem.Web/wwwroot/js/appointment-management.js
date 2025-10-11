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
                { data: 'id', title: 'ID', width: '60px' },
                { data: 'customerName', title: 'Khách Hàng' },
                { data: 'vehicleLicensePlate', title: 'Xe' },
                { data: 'appointmentDate', title: 'Ngày' },
                { data: 'appointmentTime', title: 'Giờ' },
                { data: 'serviceType', title: 'Loại Dịch Vụ' },
                { data: 'status', title: 'Trạng Thái' },
                {
                    data: null,
                    title: 'Thao Tác',
                    orderable: false,
                    searchable: false,
                    render: function(data, type, row) {
                        return `
                            <button class="btn btn-info btn-sm view-appointment" data-id="${row.id}">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-warning btn-sm edit-appointment" data-id="${row.id}">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-danger btn-sm delete-appointment" data-id="${row.id}">
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

    // Load dropdowns
    loadDropdowns: function() {
        this.loadCustomers();
        this.loadEmployees();
    },

    // Load customers
    loadCustomers: function() {
        $.ajax({
            url: '/CustomerManagement/GetActiveCustomers',
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var customers = response.data;
                    var options = '<option value="">Select Customer</option>';
                    customers.forEach(function(customer) {
                        options += `<option value="${customer.id}">${customer.name}</option>`;
                    });
                    $('#createCustomerId, #updateCustomerId').html(options);
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
            url: '/EmployeeManagement/GetActiveEmployees',
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var employees = response.data;
                    var options = '<option value="">Select Employee</option>';
                    employees.forEach(function(employee) {
                        options += `<option value="${employee.id}">${employee.name}</option>`;
                    });
                    $('#createAssignedEmployeeId, #updateAssignedEmployeeId').html(options);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading employees:', error);
            }
        });
    },

    // Load vehicles by customer
    loadVehiclesByCustomer: function(customerId, targetSelect) {
        if (!customerId) {
            $(targetSelect).html('<option value="">Select Vehicle</option>');
            return;
        }

        $.ajax({
            url: '/VehicleManagement/GetVehiclesByCustomer/' + customerId,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var vehicles = response.data;
                    var options = '<option value="">Select Vehicle</option>';
                    vehicles.forEach(function(vehicle) {
                        options += `<option value="${vehicle.id}">${vehicle.licensePlate} - ${vehicle.make} ${vehicle.model}</option>`;
                    });
                    $(targetSelect).html(options);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading vehicles:', error);
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

        // Customer change event - load vehicles
        $(document).on('change', '#createCustomerId', function() {
            var customerId = $(this).val();
            self.loadVehiclesByCustomer(customerId, '#createVehicleId');
        });

        $(document).on('change', '#updateCustomerId', function() {
            var customerId = $(this).val();
            self.loadVehiclesByCustomer(customerId, '#updateVehicleId');
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
        var appointmentId = $('#updateAppointmentId').val();
        var formData = {
            Id: parseInt(appointmentId),
            CustomerId: parseInt($('#updateCustomerId').val()),
            VehicleId: parseInt($('#updateVehicleId').val()),
            AppointmentDate: $('#updateAppointmentDate').val(),
            AppointmentTime: $('#updateAppointmentTime').val(),
            ServiceType: $('#updateServiceType').val(),
            Description: $('#updateDescription').val(),
            Status: $('#updateStatus').val(),
            AssignedEmployeeId: $('#updateAssignedEmployeeId').val() ? parseInt($('#updateAssignedEmployeeId').val()) : null
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
        $('#viewCustomerName').text(appointment.customerName || '');
        $('#viewVehicleLicensePlate').text(appointment.vehicleLicensePlate || '');
        $('#viewAppointmentDate').text(appointment.appointmentDate || '');
        $('#viewAppointmentTime').text(appointment.appointmentTime || '');
        $('#viewServiceType').text(appointment.serviceType || '');
        $('#viewDescription').text(appointment.description || '');
        $('#viewStatus').text(appointment.status || '');
        $('#viewAssignedEmployeeName').text(appointment.assignedEmployeeName || '');
    },

    // Populate edit modal
    populateEditModal: function(appointment) {
        $('#updateAppointmentId').val(appointment.id);
        $('#updateCustomerId').val(appointment.customerId);
        
        // Load vehicles for selected customer
        var self = this;
        this.loadVehiclesByCustomer(appointment.customerId, '#updateVehicleId');
        
        // Wait a bit for vehicles to load, then set the selected vehicle
        setTimeout(function() {
            $('#updateVehicleId').val(appointment.vehicleId);
        }, 500);
        
        $('#updateAppointmentDate').val(appointment.appointmentDate);
        $('#updateAppointmentTime').val(appointment.appointmentTime);
        $('#updateServiceType').val(appointment.serviceType);
        $('#updateDescription').val(appointment.description);
        $('#updateStatus').val(appointment.status);
        $('#updateAssignedEmployeeId').val(appointment.assignedEmployeeId);
    }
};

// Initialize when document is ready
$(document).ready(function() {
    AppointmentManagement.init();
});