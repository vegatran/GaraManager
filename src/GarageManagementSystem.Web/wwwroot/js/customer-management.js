/**
 * Customer Management JavaScript Module
 * Follows the same pattern as IdentityServer modules
 */
var CustomerManagement = {
    table: null,

    /**
     * Initialize the customer management module
     */
    init: function() {
        this.initDataTable();
        this.bindEvents();
    },

    /**
     * Initialize DataTable
     */
    initDataTable: function() {
        this.table = $('#customerTable').DataTable({
            processing: true,
            serverSide: false,
            ajax: {
                url: '/CustomerManagement/GetCustomers',
                type: 'GET',
                dataSrc: 'data'
            },
            columns: [
                { data: 'id' },
                { data: 'name' },
                { data: 'email' },
                { data: 'phone' },
                { data: 'address' },
                { data: 'createdDate' },
                { 
                    data: 'isActive',
                    render: function(data, type, row) {
                        return data ? 
                            '<span class="badge badge-success">Active</span>' : 
                            '<span class="badge badge-danger">Inactive</span>';
                    }
                },
                {
                    data: null,
                    orderable: false,
                    render: function(data, type, row) {
                        return `
                            <div class="btn-group" role="group">
                                <button class="btn btn-sm btn-info" onclick="CustomerManagement.viewDetails(${row.id})" title="View Details">
                                    <i class="fas fa-eye"></i>
                                </button>
                                <button class="btn btn-sm btn-warning" onclick="CustomerManagement.editCustomer(${row.id})" title="Edit">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button class="btn btn-sm btn-danger" onclick="CustomerManagement.deleteCustomer(${row.id})" title="Delete">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </div>
                        `;
                    }
                }
            ],
            responsive: true,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "All"]],
            language: {
                processing: "Loading customers...",
                emptyTable: "No customers found",
                zeroRecords: "No matching customers found"
            }
        });
    },

    /**
     * Bind event handlers
     */
    bindEvents: function() {
        var self = this;

        // Create customer form
        $('#createCustomerForm').on('submit', function(e) {
            e.preventDefault();
            self.createCustomer();
        });

        // Edit customer form
        $('#editCustomerForm').on('submit', function(e) {
            e.preventDefault();
            self.updateCustomer();
        });
    },

    /**
     * Create new customer
     */
    createCustomer: function() {
        var formData = new FormData($('#createCustomerForm')[0]);
        var data = {};
        
        formData.forEach((value, key) => {
            data[key] = value;
        });

        $.ajax({
            url: '/CustomerManagement/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: response.message,
                        timer: 2000
                    }).then(() => {
                        $('#createCustomerModal').modal('hide');
                        $('#createCustomerForm')[0].reset();
                        CustomerManagement.table.ajax.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.message
                    });
                }
            },
            error: function() {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Failed to create customer'
                });
            }
        });
    },

    /**
     * Update customer
     */
    updateCustomer: function() {
        var formData = new FormData($('#editCustomerForm')[0]);
        var data = {};
        
        formData.forEach((value, key) => {
            data[key] = value;
        });

        $.ajax({
            url: '/CustomerManagement/Edit',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: response.message,
                        timer: 2000
                    }).then(() => {
                        $('#editCustomerModal').modal('hide');
                        CustomerManagement.table.ajax.reload();
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.message
                    });
                }
            },
            error: function() {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Failed to update customer'
                });
            }
        });
    },

    /**
     * View customer details
     */
    viewDetails: function(id) {
        $.ajax({
            url: '/CustomerManagement/Details',
            type: 'GET',
            data: { id: id },
            success: function(response) {
                if (response.success) {
                    var customer = response.data;
                    var detailsHtml = `
                        <div class="row">
                            <div class="col-md-6">
                                <strong>ID:</strong> ${customer.id}<br>
                                <strong>Name:</strong> ${customer.name}<br>
                                <strong>Email:</strong> ${customer.email || 'N/A'}<br>
                                <strong>Phone:</strong> ${customer.phone || 'N/A'}<br>
                            </div>
                            <div class="col-md-6">
                                <strong>Address:</strong> ${customer.address || 'N/A'}<br>
                                <strong>Created Date:</strong> ${customer.createdDate}<br>
                                <strong>Updated Date:</strong> ${customer.updatedDate || 'N/A'}<br>
                                <strong>Status:</strong> ${customer.isActive ? '<span class="badge badge-success">Active</span>' : '<span class="badge badge-danger">Inactive</span>'}<br>
                            </div>
                        </div>
                    `;
                    $('#customerDetailsContent').html(detailsHtml);
                    $('#customerDetailsModal').modal('show');
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.message
                    });
                }
            },
            error: function() {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Failed to load customer details'
                });
            }
        });
    },

    /**
     * Edit customer
     */
    editCustomer: function(id) {
        $.ajax({
            url: '/CustomerManagement/Details',
            type: 'GET',
            data: { id: id },
            success: function(response) {
                if (response.success) {
                    var customer = response.data;
                    $('#editId').val(customer.id);
                    $('#editName').val(customer.name);
                    $('#editEmail').val(customer.email);
                    $('#editPhone').val(customer.phone);
                    $('#editAddress').val(customer.address);
                    $('#editCustomerModal').modal('show');
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.message
                    });
                }
            },
            error: function() {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Failed to load customer data'
                });
            }
        });
    },

    /**
     * Delete customer
     */
    deleteCustomer: function(id) {
        Swal.fire({
            title: 'Delete Customer',
            text: 'Are you sure you want to delete this customer?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/CustomerManagement/Delete',
                    type: 'POST',
                    data: { id: id },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function(response) {
                        if (response.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Success',
                                text: response.message,
                                timer: 2000
                            }).then(() => {
                                CustomerManagement.table.ajax.reload();
                            });
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'Error',
                                text: response.message
                            });
                        }
                    },
                    error: function() {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: 'Failed to delete customer'
                        });
                    }
                });
            }
        });
    }
};

// Initialize when document is ready
$(document).ready(function() {
    CustomerManagement.init();
});
