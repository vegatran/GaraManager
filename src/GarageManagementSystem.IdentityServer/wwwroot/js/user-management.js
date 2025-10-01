/**
 * User Management JavaScript Module
 * 
 * Mô tả: 
 * - Quản lý tất cả chức năng CRUD cho User Management
 * - Sử dụng các utility chung từ site.js và modal-select2.js
 * - Tách riêng để không conflict với các module khác
 * - Theo pattern của ClientManagement
 */

window.UserManagement = {
    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
    },

    // Initialize DataTable
    initDataTable: function() {
        // Check if DataTable already exists and destroy it first
        if ($.fn.DataTable.isDataTable('#usersTable')) {
            $('#usersTable').DataTable().destroy();
        }
        
        $("#usersTable").DataTable({
            "ajax": {
                url: "/UserManagement/GetUsers",
                type: "GET",
                "dataSrc": "data"
            },
            "columns": [
                { 
                    "data": "email",
                    "responsivePriority": 1
                },
                { 
                    "data": "fullName",
                    "responsivePriority": 2
                },
                { 
                    "data": "roles",
                    "render": function(data) {
                        if (data && data.length > 0) {
                            return data.map(role => `<span class="badge badge-info">${role}</span>`).join(' ');
                        }
                        return '<span class="text-muted">No roles</span>';
                    },
                    "responsivePriority": 3
                },
                { 
                    "data": "isActive",
                    "render": function(data) {
                        return data ? '<span class="badge badge-success">Active</span>' : '<span class="badge badge-danger">Inactive</span>';
                    },
                    "responsivePriority": 4
                },
                {
                    "data": "id",
                    "render": function(data, type, row) {
                        var buttons = '';
                        buttons += '<div class="action-buttons">';
                        buttons += '<button class="btn btn-info btn-sm view-btn" data-id="' + data + '" title="View"><i class="fas fa-eye"></i></button>';
                        buttons += '<button class="btn btn-warning btn-sm edit-btn" data-id="' + data + '" title="Edit"><i class="fas fa-edit"></i></button>';
                        
                        if (row.isActive) {
                            buttons += '<button class="btn btn-danger btn-sm delete-btn" data-id="' + data + '" title="Deactivate"><i class="fas fa-user-times"></i></button>';
                        } else {
                            buttons += '<button class="btn btn-success btn-sm restore-btn" data-id="' + data + '" title="Restore"><i class="fas fa-user-check"></i></button>';
                        }
                        
                        buttons += '</div>';
                        return buttons;
                    },
                    "orderable": false,
                    "responsivePriority": 1
                }
            ],
            "responsive": true,
            "processing": true,
            "pageLength": 10,
            "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
            "order": [[0, "asc"]],
            "language": {
                "emptyTable": "No users found",
                "zeroRecords": "No matching users found"
            },
            "dom": '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                   '<"row"<"col-sm-12"tr>>' +
                   '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
            "autoWidth": false,
            "scrollX": true,
            "scrollCollapse": true
        });
    },

    // Bind event handlers
    bindEvents: function() {
        var self = this;

        // Initialize Select2 for main page (outside modals)
        $('.select2:not(.modal .select2)').select2({
            width: '100%',
            placeholder: 'Select options...',
            allowClear: true
        });

        // Create User Form Submission
        $('#createUserForm').on('submit', function(e) {
            e.preventDefault();
            self.handleCreateUser();
        });

        // Edit User Form Submission
        $(document).on('submit', '#editUserForm', function(e) {
            e.preventDefault();
            self.handleEditUser();
        });

        // View User
        $(document).on('click', '.view-btn', function() {
            var userId = $(this).data('id');
            self.showUserDetails(userId);
        });

        // Edit User
        $(document).on('click', '.edit-btn', function() {
            var userId = $(this).data('id');
            self.showUserEdit(userId);
        });

        // Delete User
        $(document).on('click', '.delete-btn', function() {
            var userId = $(this).data('id');
            self.deleteUser(userId);
        });

        // Restore User
        $(document).on('click', '.restore-btn', function() {
            var userId = $(this).data('id');
            self.restoreUser(userId);
        });

        // Initialize Select2 for Create modal
        $('#createUserModal').on('shown.bs.modal', function() {
            // Initialize Select2 for Roles
            $('#createRoles').select2({
                placeholder: 'Select roles...',
                allowClear: true,
                dropdownParent: $('#createUserModal')
            });
            
            // Add change event handlers for Create modal
            $('#createRoles').on('change', function() {
                $(this).trigger('change.select2');
            });
        });

        // Initialize Select2 for Edit modal
        $('#editUserModal').on('shown.bs.modal', function() {
            // Initialize Select2 for Roles if they exist
            if ($('#editRoles').length) {
                $('#editRoles').select2({
                    placeholder: 'Select roles...',
                    allowClear: true,
                    dropdownParent: $('#editUserModal')
                });
                
                // Set selected values from data attributes
                var selectedRoles = $('#editRoles').data('selected-values');
                if (selectedRoles && selectedRoles.length > 0) {
                    var values = selectedRoles.split(',').filter(v => v.trim() !== '');
                    if (values.length > 0) {
                        $('#editRoles').val(values).trigger('change');
                    }
                }
                
                // Trigger change event for form validation
                $('#editRoles').on('change', function() {
                    $(this).trigger('change.select2');
                });
            }
        });

        // Reset modal forms when closed
        $('#createUserModal').on('hidden.bs.modal', function() {
            $('#createUserForm')[0].reset();
            // Destroy Select2 instances to prevent conflicts
            if ($('#createRoles').hasClass('select2-hidden-accessible')) {
                $('#createRoles').select2('destroy');
            }
        });

        $('#editUserModal').on('hidden.bs.modal', function() {
            // Destroy Select2 instances to prevent conflicts
            if ($('#editRoles').hasClass('select2-hidden-accessible')) {
                $('#editRoles').select2('destroy');
            }
        });
    },

    // Handle Create User
    handleCreateUser: function() {
        var formData = new FormData();
        
        // Basic Information
        formData.append('Email', $('#createEmail').val());
        formData.append('Password', $('#createPassword').val());
        formData.append('FirstName', $('#createFirstName').val());
        formData.append('LastName', $('#createLastName').val());
        
        // Checkbox
        formData.append('IsActive', $('#createIsActive').is(':checked'));
        
        // Roles
        var selectedRoles = $('#createRoles').val() || [];
        selectedRoles.forEach(function(role) {
            formData.append('Roles', role);
        });
        
        // Add antiforgery token
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        $.ajax({
            url: '/UserManagement/Create',
            type: 'POST',
            processData: false,
            contentType: false,
            data: formData,
            success: function(response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: 'User created successfully!',
                        timer: 2000,
                        showConfirmButton: false
                    });
                    $('#createUserModal').modal('hide');
                    $('#createUserForm')[0].reset();
                    UserManagement.refresh();
                } else {
                    var errorText = response.message || 'Failed to create user';
                    if (response.errors && response.errors.length > 0) {
                        errorText += '\n\nValidation Errors:\n' + response.errors.join('\n');
                    }
                    Swal.fire({
                        icon: 'error',
                        title: 'Create Failed',
                        text: errorText,
                        width: '600px'
                    });
                }
            },
            error: function() {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'An error occurred while creating the user'
                });
            }
        });
    },

    // Handle Edit User
    handleEditUser: function() {
        var formData = new FormData();
        var form = $('#editUserForm')[0];
        
        // Get all form data, excluding the antiforgery token from the loop
        for (var i = 0; i < form.elements.length; i++) {
            var element = form.elements[i];
            if (element.name && element.type !== 'file' && element.name !== '__RequestVerificationToken') {
                if (element.type === 'checkbox') {
                    formData.append(element.name, element.checked);
                } else if (element.type === 'select-multiple') {
                    var values = $(element).val() || [];
                    values.forEach(function(value) {
                        formData.append(element.name, value);
                    });
                } else if (element.value) {
                    formData.append(element.name, element.value);
                }
            }
        }
        
        // Add antiforgery token from the main document (not from modal)
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        $.ajax({
            url: '/UserManagement/Edit',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: 'User updated successfully!',
                        timer: 2000,
                        showConfirmButton: false
                    });
                    $('#editUserModal').modal('hide');
                    UserManagement.refresh();
                } else {
                    var errorText = response.message || 'Failed to update user';
                    if (response.errors && response.errors.length > 0) {
                        errorText += '\n\nValidation Errors:\n' + response.errors.join('\n');
                    }
                    Swal.fire({
                        icon: 'error',
                        title: 'Update Failed',
                        text: errorText,
                        width: '600px'
                    });
                }
            },
            error: function(xhr, status, error) {
                var errorMessage = 'An error occurred while updating the user';
                
                // Try to parse error response from server
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                    if (xhr.responseJSON.errors) {
                        errorMessage += '\n\nValidation Errors:\n' + xhr.responseJSON.errors.join('\n');
                    }
                } else if (xhr.responseText) {
                    try {
                        var response = JSON.parse(xhr.responseText);
                        if (response.message) {
                            errorMessage = response.message;
                        }
                    } catch (e) {
                        errorMessage = 'Server Error: ' + xhr.responseText.substring(0, 200);
                    }
                }
                
                Swal.fire({
                    icon: 'error',
                    title: 'Update Failed',
                    text: errorMessage,
                    width: '600px'
                });
            }
        });
    },

    // Show User Details
    showUserDetails: function(userId) {
        $.get('/UserManagement/Details/' + userId, function(data) {
            $('#viewUserContent').html(data);
            $('#viewUserModal').modal('show');
        }).fail(function() {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to load user details'
            });
        });
    },

    // Show User Edit
    showUserEdit: function(userId) {
        $.get('/UserManagement/Edit/' + userId, function(data) {
            $('#editUserContent').html(data);
            $('#editUserModal').modal('show');
        }).fail(function() {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to load user data for editing'
            });
        });
    },

    // Delete User with confirmation
    deleteUser: function(userId) {
        Swal.fire({
            title: 'Deactivate User',
            text: 'Are you sure you want to deactivate this user?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, deactivate it!'
        }).then(function(result) {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/UserManagement/Delete/' + userId,
                    type: 'POST',
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function(response) {
                        if (response.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Success',
                                text: 'User deactivated successfully!',
                                timer: 2000,
                                showConfirmButton: false
                            });
                            UserManagement.refresh();
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'Deactivate Failed',
                                text: response.message || 'Failed to deactivate user'
                            });
                        }
                    },
                    error: function() {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: 'An error occurred while deactivating the user'
                        });
                    }
                });
            }
        });
    },

    // Restore User with confirmation
    restoreUser: function(userId) {
        Swal.fire({
            title: 'Restore User',
            text: 'Are you sure you want to restore this user?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes, restore it!'
        }).then(function(result) {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/UserManagement/Restore/' + userId,
                    type: 'POST',
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function(response) {
                        if (response.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Success',
                                text: 'User restored successfully!',
                                timer: 2000,
                                showConfirmButton: false
                            });
                            UserManagement.refresh();
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'Restore Failed',
                                text: response.message || 'Failed to restore user'
                            });
                        }
                    },
                    error: function() {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: 'An error occurred while restoring the user'
                        });
                    }
                });
            }
        });
    },

    // Refresh DataTable
    refresh: function() {
        $('#usersTable').DataTable().ajax.reload();
    }
};

// Initialize when document is ready (only once)
$(document).ready(function() {
    // Do not auto-initialize here - let pages control initialization manually
    // This prevents duplicate initialization from auto-loading globals
});
