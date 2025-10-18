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
        
        // Sử dụng DataTablesUtility với style chung
        var columns = [
            { data: 'id', title: 'ID', width: '60px' },
            { data: 'name', title: 'Tên' },
            { data: 'email', title: 'Email' },
            { data: 'phone', title: 'Số Điện Thoại' },
            { data: 'address', title: 'Địa Chỉ' },
            { 
                data: 'createdDate', 
                title: 'Ngày Tạo',
                render: DataTablesUtility.renderDate
            },
            { 
                data: 'isActive', 
                title: 'Trạng Thái',
                render: DataTablesUtility.renderStatus
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
        ];
        
        this.customerTable = DataTablesUtility.initAjaxTable('#customerTable', '/CustomerManagement/GetCustomers', columns, {
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
            self.customerTable.search(this.value).draw();
        });

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
        $(document).on('submit', '#editCustomerForm', function(e) {
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
                console.log('🔍 View Customer API Response:', response);
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        console.log('🔍 Customer Data:', response.data);
                        self.populateViewModal(response.data);
                        $('#viewCustomerModal').modal('show');
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error loading customer');
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
                console.log('🔍 Edit Customer API Response:', response);
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        console.log('🔍 Customer Data:', response.data);
                        self.populateEditModal(response.data);
                        $('#editCustomerModal').modal('show');
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error loading customer');
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
                                GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error deleting customer');
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
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error creating customer');
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
        var customerId = $('#editId').val();
        var formData = {
            Id: parseInt(customerId),
            Name: $('#editName').val(),
            Email: $('#editEmail').val(),
            Phone: $('#editPhone').val(),
            Address: $('#editAddress').val(),
            AlternativePhone: $('#editAlternativePhone').val(),
            ContactPersonName: $('#editContactPersonName').val()
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
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error updating customer');
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
        $('#editId').val(customer.id);
        $('#editName').val(customer.name);
        $('#editEmail').val(customer.email);
        $('#editPhone').val(customer.phone);
        $('#editAddress').val(customer.address);
        $('#editAlternativePhone').val(customer.alternativePhone);
        $('#editContactPersonName').val(customer.contactPersonName);
    }
};

// Initialize when document is ready
$(document).ready(function() {
    CustomerManagement.init();
});