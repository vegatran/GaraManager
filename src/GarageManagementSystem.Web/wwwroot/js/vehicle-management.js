/**
 * Vehicle Management Module
 * 
 * Handles all vehicle-related operations
 * CRUD operations for vehicles
 */

window.VehicleManagement = {
    // DataTable instance
    vehicleTable: null,

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.loadCustomers();
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        this.vehicleTable = DataTablesVietnamese.init('#vehicleTable', {
            ajax: {
                url: '/VehicleManagement/GetVehicles',
                type: 'GET',
                error: function(xhr, status, error) {
                    if (AuthHandler.isUnauthorized(xhr)) {
                        AuthHandler.handleUnauthorized(xhr, true);
                    } else {
                        GarageApp.showError('Lỗi khi tải danh sách xe');
                    }
                }
            },
            columns: [
                { data: 'id', title: 'ID', width: '60px' },
                { data: 'licensePlate', title: 'Biển Số Xe' },
                { data: 'brand', title: 'Hãng Xe' },
                { data: 'model', title: 'Mẫu Xe' },
                { data: 'year', title: 'Năm SX' },
                { data: 'customerName', title: 'Khách Hàng' },
                {
                    data: null,
                    title: 'Thao Tác',
                    orderable: false,
                    searchable: false,
                    render: function(data, type, row) {
                        return `
                            <button class="btn btn-info btn-sm view-vehicle" data-id="${row.id}">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-warning btn-sm edit-vehicle" data-id="${row.id}">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-danger btn-sm delete-vehicle" data-id="${row.id}">
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

    // Load customers for dropdown
    loadCustomers: function() {
        $.ajax({
            url: '/VehicleManagement/GetActiveCustomers',
            type: 'GET',
            success: function(response) {
                // GetActiveCustomers returns data directly, not wrapped in success/data
                if (response && Array.isArray(response)) {
                    var customers = response;
                    var options = '<option value="">Select Customer</option>';
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

    // Bind events
    bindEvents: function() {
        var self = this;

        // Add vehicle button - using data-target selector
        $(document).on('click', '[data-target="#createVehicleModal"]', function() {
            self.showCreateModal();
        });

        // View vehicle
        $(document).on('click', '.view-vehicle', function() {
            var id = $(this).data('id');
            self.viewVehicle(id);
        });

        // Edit vehicle
        $(document).on('click', '.edit-vehicle', function() {
            var id = $(this).data('id');
            self.editVehicle(id);
        });

        // Delete vehicle
        $(document).on('click', '.delete-vehicle', function() {
            var id = $(this).data('id');
            self.deleteVehicle(id);
        });

        // Create vehicle form
        $(document).on('submit', '#createVehicleForm', function(e) {
            e.preventDefault();
            self.createVehicle();
        });

        // Update vehicle form
        $(document).on('submit', '#editVehicleForm', function(e) {
            e.preventDefault();
            self.updateVehicle();
        });
    },

    // Show create modal
    showCreateModal: function() {
        $('#createVehicleModal').modal('show');
        $('#createVehicleForm')[0].reset();
    },

    // View vehicle
    viewVehicle: function(id) {
        var self = this;
        
        $.ajax({
            url: '/VehicleManagement/GetVehicle/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        self.populateViewModal(response.data);
                        $('#viewVehicleModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Lỗi khi tải thông tin xe');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin xe');
                }
            }
        });
    },

    // Edit vehicle
    editVehicle: function(id) {
        var self = this;
        
        $.ajax({
            url: '/VehicleManagement/GetVehicle/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        self.populateEditModal(response.data);
                        $('#editVehicleModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Lỗi khi tải thông tin xe');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin xe');
                }
            }
        });
    },

    // Delete vehicle
    deleteVehicle: function(id) {
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
                    url: '/VehicleManagement/DeleteVehicle/' + id,
                    type: 'DELETE',
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Vehicle deleted successfully!');
                                self.vehicleTable.ajax.reload();
                            } else {
                                GarageApp.showError(response.message || 'Error deleting vehicle');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Error deleting vehicle');
                        }
                    }
                });
            }
        });
    },

    // Create vehicle
    createVehicle: function() {
        var self = this;
        var formData = {
            CustomerId: parseInt($('#createCustomerId').val()),
            LicensePlate: $('#createLicensePlate').val(),
            Brand: $('#createBrand').val(),
            Model: $('#createModel').val(),
            Year: $('#createYear').val() || null,
            Color: $('#createColor').val() || null,
            VIN: $('#createVIN').val() || null,
            EngineNumber: $('#createEngineNumber').val() || null,
            Mileage: $('#createMileage').val() ? parseInt($('#createMileage').val()) : null,
            VehicleType: "Personal"
        };

        // Validate required fields
        if (!formData.CustomerId || !formData.LicensePlate || !formData.Brand || !formData.Model) {
            GarageApp.showError('Vui lòng điền đầy đủ thông tin bắt buộc');
            return;
        }

        $.ajax({
            url: '/VehicleManagement/CreateVehicle',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Vehicle created successfully!');
                        $('#createVehicleModal').modal('hide');
                        self.vehicleTable.ajax.reload();
                    } else {
                        GarageApp.showError(response.message || 'Error creating vehicle');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error creating vehicle');
                }
            }
        });
    },

    // Update vehicle
    updateVehicle: function() {
        var self = this;
        var vehicleId = $('#editId').val();
        var formData = {
            Id: parseInt(vehicleId),
            CustomerId: parseInt($('#editCustomerId').val()),
            LicensePlate: $('#editLicensePlate').val(),
            Brand: $('#editBrand').val(),
            Model: $('#editModel').val(),
            Year: $('#editYear').val() || null,
            Color: $('#editColor').val() || null,
            VIN: $('#editVIN').val() || null,
            EngineNumber: $('#editEngineNumber').val() || null,
            Mileage: $('#editMileage').val() ? parseInt($('#editMileage').val()) : null,
            VehicleType: "Personal"
        };

        // Validate required fields
        if (!formData.CustomerId || !formData.LicensePlate || !formData.Brand || !formData.Model) {
            GarageApp.showError('Vui lòng điền đầy đủ thông tin bắt buộc');
            return;
        }

        $.ajax({
            url: '/VehicleManagement/UpdateVehicle/' + vehicleId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Vehicle updated successfully!');
                        $('#editVehicleModal').modal('hide');
                        self.vehicleTable.ajax.reload();
                    } else {
                        GarageApp.showError(response.message || 'Error updating vehicle');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error updating vehicle');
                }
            }
        });
    },

    // Populate view modal
    populateViewModal: function(vehicle) {
        $('#viewCustomer').text(vehicle.customerName || '');
        $('#viewLicensePlate').text(vehicle.licensePlate || '');
        $('#viewMake').text(vehicle.brand || '');
        $('#viewModel').text(vehicle.model || '');
        $('#viewYear').text(vehicle.year || '');
        $('#viewColor').text(vehicle.color || '');
        $('#viewVIN').text(vehicle.vin || '');
        $('#viewEngineNumber').text(vehicle.engineNumber || '');
        $('#viewMileage').text(vehicle.mileage || '');
    },

    // Populate edit modal
    populateEditModal: function(vehicle) {
        $('#editId').val(vehicle.id);
        $('#editCustomerId').val(vehicle.customerId);
        $('#editLicensePlate').val(vehicle.licensePlate);
        $('#editBrand').val(vehicle.brand);
        $('#editModel').val(vehicle.model);
        $('#editYear').val(vehicle.year);
        $('#editColor').val(vehicle.color);
        $('#editVIN').val(vehicle.vin);
        $('#editEngineNumber').val(vehicle.engineNumber);
        $('#editMileage').val(vehicle.mileage);
    }
};

// Initialize when document is ready
$(document).ready(function() {
    VehicleManagement.init();
});