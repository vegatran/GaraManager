/**
 * Service Management Module
 * 
 * Handles all service-related operations
 * CRUD operations for services
 */

window.ServiceManagement = {
    // DataTable instance
    serviceTable: null,

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable('#serviceTable')) {
            $('#serviceTable').DataTable().destroy();
        }
        
        this.serviceTable = DataTablesVietnamese.init('#serviceTable', {
            processing: true,
            serverSide: false,
            ajax: {
                url: '/ServiceManagement/GetServices',
                type: 'GET',
                error: function(xhr, status, error) {
                    if (AuthHandler.isUnauthorized(xhr)) {
                        AuthHandler.handleUnauthorized(xhr, true);
                    } else {
                        GarageApp.showError('Error loading services');
                    }
                }
            },
            columns: [
                { data: 'id', title: 'ID', width: '60px' },
                { data: 'serviceName', title: 'Tên Dịch Vụ' },
                { data: 'serviceCode', title: 'Mã Dịch Vụ' },
                { data: 'description', title: 'Mô Tả' },
                { data: 'price', title: 'Giá' },
                { data: 'duration', title: 'Thời Gian (phút)' },
                {
                    data: null,
                    title: 'Thao Tác',
                    orderable: false,
                    searchable: false,
                    render: function(data, type, row) {
                        return `
                            <button class="btn btn-info btn-sm view-service" data-id="${row.id}">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-warning btn-sm edit-service" data-id="${row.id}">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-danger btn-sm delete-service" data-id="${row.id}">
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

        // Add service button
        $(document).on('click', '#addServiceBtn', function() {
            self.showCreateModal();
        });

        // View service
        $(document).on('click', '.view-service', function() {
            var id = $(this).data('id');
            self.viewService(id);
        });

        // Edit service
        $(document).on('click', '.edit-service', function() {
            var id = $(this).data('id');
            self.editService(id);
        });

        // Delete service
        $(document).on('click', '.delete-service', function() {
            var id = $(this).data('id');
            self.deleteService(id);
        });

        // Create service form
        $(document).on('submit', '#createServiceForm', function(e) {
            e.preventDefault();
            self.createService();
        });

        // Update service form
        $(document).on('submit', '#editServiceForm', function(e) {
            e.preventDefault();
            self.updateService();
        });
    },

    // Show create modal
    showCreateModal: function() {
        $('#createServiceModal').modal('show');
        $('#createServiceForm')[0].reset();
    },

    // View service
    viewService: function(id) {
        var self = this;
        
        $.ajax({
            url: '/ServiceManagement/Details/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        self.populateViewModal(response.data);
                        $('#viewServiceModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Error loading service');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error loading service');
                }
            }
        });
    },

    // Edit service
    editService: function(id) {
        var self = this;
        
        $.ajax({
            url: '/ServiceManagement/Details/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        self.populateEditModal(response.data);
                        $('#editServiceModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Error loading service');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error loading service');
                }
            }
        });
    },

    // Delete service
    deleteService: function(id) {
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
                    url: '/ServiceManagement/Delete/' + id,
                    type: 'DELETE',
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Service deleted successfully!');
                                self.serviceTable.ajax.reload();
                            } else {
                                GarageApp.showError(response.message || 'Error deleting service');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Error deleting service');
                        }
                    }
                });
            }
        });
    },

    // Create service
    createService: function() {
        var self = this;
        var formData = {
            ServiceName: $('#createServiceName').val(),
            ServiceCode: $('#createServiceCode').val(),
            Description: $('#createDescription').val(),
            Price: parseFloat($('#createPrice').val()) || 0,
            Duration: parseInt($('#createDuration').val()) || 0,
            Category: $('#createCategory').val()
        };

        $.ajax({
            url: '/ServiceManagement/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Service created successfully!');
                        $('#createServiceModal').modal('hide');
                        self.serviceTable.ajax.reload();
                    } else {
                        GarageApp.showError(response.message || 'Error creating service');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error creating service');
                }
            }
        });
    },

    // Update service
    updateService: function() {
        var self = this;
        var serviceId = $('#editId').val();
        var formData = {
            Id: parseInt(serviceId),
            ServiceName: $('#editServiceName').val(),
            ServiceCode: $('#editServiceCode').val(),
            Description: $('#editDescription').val(),
            Price: parseFloat($('#editPrice').val()) || 0,
            Duration: parseInt($('#editDuration').val()) || 0,
            Category: $('#editCategory').val()
        };

        $.ajax({
            url: '/ServiceManagement/Edit/' + serviceId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Service updated successfully!');
                        $('#editServiceModal').modal('hide');
                        self.serviceTable.ajax.reload();
                    } else {
                        GarageApp.showError(response.message || 'Error updating service');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error updating service');
                }
            }
        });
    },

    // Populate view modal
    populateViewModal: function(service) {
        $('#viewServiceName').text(service.serviceName || '');
        $('#viewServiceCode').text(service.serviceCode || '');
        $('#viewDescription').text(service.description || '');
        $('#viewPrice').text(service.price || '');
        $('#viewDuration').text(service.duration || '');
        $('#viewCategory').text(service.category || '');
    },

    // Populate edit modal
    populateEditModal: function(service) {
        $('#editId').val(service.id);
        $('#editServiceName').val(service.serviceName);
        $('#editServiceCode').val(service.serviceCode);
        $('#editDescription').val(service.description);
        $('#editPrice').val(service.price);
        $('#editDuration').val(service.duration);
        $('#editCategory').val(service.category);
    }
};

// Initialize when document is ready
$(document).ready(function() {
    ServiceManagement.init();
});