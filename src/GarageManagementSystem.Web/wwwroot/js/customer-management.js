/**
 * Customer Management Module
 * 
 * Handles all customer-related operations
 * CRUD operations for customers
 */

window.CustomerManagement = {
    // DataTable instance
    customerTable: null,

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable('#customerTable')) {
            $('#customerTable').DataTable().destroy();
        }
        
        this.customerTable = DataTablesVietnamese.init('#customerTable', {
            ajax: {
                url: '/CustomerManagement/GetCustomers',
                type: 'GET',
                error: function(xhr, status, error) {
                    if (AuthHandler.isUnauthorized(xhr)) {
                        AuthHandler.handleUnauthorized(xhr, true);
                    } else {
                        GarageApp.showError('Lỗi khi tải danh sách khách hàng');
                    }
                }
            },
            columns: [
                { data: 'id', title: 'ID', width: '60px' },
                { data: 'name', title: 'Tên' },
                { data: 'email', title: 'Email' },
                { data: 'phone', title: 'Số Điện Thoại' },
                { data: 'address', title: 'Địa Chỉ' },
                { data: 'createdDate', title: 'Ngày Tạo' },
                { 
                    data: 'isActive', 
                    title: 'Trạng Thái',
                    render: function(data, type, row) {
                        return data ? '<span class="badge badge-success">Hoạt động</span>' : '<span class="badge badge-danger">Không hoạt động</span>';
                    }
                },
                {
                    data: null,
                    title: 'Thao Tác',
                    orderable: false,
                    searchable: false,
                    render: function(data, type, row) {
                        return `
                            <button class="btn btn-info btn-sm view-customer" data-id="${row.id}">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-warning btn-sm edit-customer" data-id="${row.id}">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-danger btn-sm delete-customer" data-id="${row.id}">
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

        // Add customer button
        $(document).on('click', '#addCustomerBtn', function() {
            self.showCreateModal();
        });

        // View customer
        $(document).on('click', '.view-customer', function() {
            var id = $(this).data('id');
            self.viewCustomer(id);
        });

        // Edit customer
        $(document).on('click', '.edit-customer', function() {
            var id = $(this).data('id');
            self.editCustomer(id);
        });

        // Delete customer
        $(document).on('click', '.delete-customer', function() {
            var id = $(this).data('id');
            self.deleteCustomer(id);
        });

        // Create customer form
        $(document).on('submit', '#createCustomerForm', function(e) {
            e.preventDefault();
            self.createCustomer();
        });

        // Update customer form
        $(document).on('submit', '#updateCustomerForm', function(e) {
            e.preventDefault();
            self.updateCustomer();
        });
    },

    // Show create modal
    showCreateModal: function() {
        $('#createCustomerModal').modal('show');
        $('#createCustomerForm')[0].reset();
    },

    // View customer
    viewCustomer: function(id) {
        var self = this;
        
        $.ajax({
            url: '/CustomerManagement/GetCustomer/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        self.populateViewModal(response.data);
                        $('#viewCustomerModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Error loading customer');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error loading customer');
                }
            }
        });
    },

    // Edit customer
    editCustomer: function(id) {
        var self = this;
        
        $.ajax({
            url: '/CustomerManagement/GetCustomer/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        self.populateEditModal(response.data);
                        $('#editCustomerModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Error loading customer');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error loading customer');
                }
            }
        });
    },

    // Delete customer
    deleteCustomer: function(id) {
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
                    url: '/CustomerManagement/DeleteCustomer/' + id,
                    type: 'DELETE',
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Customer deleted successfully!');
                                self.customerTable.ajax.reload();
                            } else {
                                GarageApp.showError(response.message || 'Error deleting customer');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Error deleting customer');
                        }
                    }
                });
            }
        });
    },

    // Create customer
    createCustomer: function() {
        var self = this;
        var formData = {
            Name: $('#createName').val(),
            Email: $('#createEmail').val(),
            Phone: $('#createPhone').val(),
            Address: $('#createAddress').val(),
            AlternativePhone: $('#createAlternativePhone').val(),
            ContactPersonName: $('#createContactPersonName').val()
        };

        $.ajax({
            url: '/CustomerManagement/CreateCustomer',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Customer created successfully!');
                        $('#createCustomerModal').modal('hide');
                        self.customerTable.ajax.reload();
                    } else {
                        GarageApp.showError(response.message || 'Error creating customer');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error creating customer');
                }
            }
        });
    },

    // Update customer
    updateCustomer: function() {
        var self = this;
        var customerId = $('#updateCustomerId').val();
        var formData = {
            Id: parseInt(customerId),
            Name: $('#updateName').val(),
            Email: $('#updateEmail').val(),
            Phone: $('#updatePhone').val(),
            Address: $('#updateAddress').val(),
            AlternativePhone: $('#updateAlternativePhone').val(),
            ContactPersonName: $('#updateContactPersonName').val()
        };

        $.ajax({
            url: '/CustomerManagement/UpdateCustomer/' + customerId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Customer updated successfully!');
                        $('#editCustomerModal').modal('hide');
                        self.customerTable.ajax.reload();
                    } else {
                        GarageApp.showError(response.message || 'Error updating customer');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error updating customer');
                }
            }
        });
    },

    // Populate view modal
    populateViewModal: function(customer) {
        $('#viewName').text(customer.name || '');
        $('#viewEmail').text(customer.email || '');
        $('#viewPhone').text(customer.phone || '');
        $('#viewAddress').text(customer.address || '');
        $('#viewAlternativePhone').text(customer.alternativePhone || '');
        $('#viewContactPersonName').text(customer.contactPersonName || '');
    },

    // Populate edit modal
    populateEditModal: function(customer) {
        $('#updateCustomerId').val(customer.id);
        $('#updateName').val(customer.name);
        $('#updateEmail').val(customer.email);
        $('#updatePhone').val(customer.phone);
        $('#updateAddress').val(customer.address);
        $('#updateAlternativePhone').val(customer.alternativePhone);
        $('#updateContactPersonName').val(customer.contactPersonName);
    }
};

// Initialize when document is ready
$(document).ready(function() {
    CustomerManagement.init();
});