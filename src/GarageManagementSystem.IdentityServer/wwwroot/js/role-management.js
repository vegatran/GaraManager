/**
 * Role Management JavaScript Module
 * 
 * Mô tả: 
 * - Quản lý tất cả chức năng CRUD cho Role Management
 * - Sử dụng các utility chung từ site.js và modal-select2.js
 * - Tách riêng để không conflict với các module khác
 */

window.RoleManagement = {
    // Initialize module
    init: function() {
        this.unbindEvents(); // Unbind any existing events first
        this.initDataTable();
        this.bindEvents();
    },

    // Initialize DataTable
    initDataTable: function() {
        // Check if DataTable already exists and destroy it first
        if ($.fn.DataTable.isDataTable('#rolesTable')) {
            $('#rolesTable').DataTable().destroy();
        }
        
        $("#rolesTable").DataTable({
            "ajax": {
                url: "/RoleManagement/GetRoles",
                type: "GET",
                dataSrc: "data" // Ensure data is read from the 'data' property
            },
            "columns": [
                { "data": "name", "responsivePriority": 2 },
                { 
                    "data": "description", 
                    "render": function (data) {
                        return data && data.length > 50 ? data.substring(0, 50) + "..." : (data || "No description");
                    },
                    "responsivePriority": 3 
                },
                {
                    "data": "isActive",
                    "render": function (data) {
                        return data ? '<span class="badge badge-success">Active</span>' : '<span class="badge badge-danger">Inactive</span>';
                    },
                    "responsivePriority": 4
                },
                { "data": "createdAt", "responsivePriority": 4 },
                {
                    "data": "id",
                    "render": function(data, type, row) {
                        var buttons = '';
                        buttons += '<div class="action-buttons">';
                        buttons += '<button class="btn btn-info btn-sm view-btn" data-id="' + data + '" title="View"><i class="fas fa-eye"></i></button>';
                        buttons += '<button class="btn btn-warning btn-sm edit-btn" data-id="' + data + '" title="Edit"><i class="fas fa-edit"></i></button>';
                        
                        if (row.isActive) {
                            buttons += '<button class="btn btn-danger btn-sm delete-btn" data-id="' + data + '" title="Deactivate"><i class="fas fa-times"></i></button>';
                        } else {
                            buttons += '<button class="btn btn-success btn-sm restore-btn" data-id="' + data + '" title="Restore"><i class="fas fa-check"></i></button>';
                        }
                        
                        buttons += '</div>';
                        return buttons;
                    },
                    "orderable": false,
                    "searchable": false,
                    "responsivePriority": 1
                }
            ],
            "responsive": true,
            "processing": true,
            "pageLength": 10,
            "lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],
            "order": [[0, "asc"]],
            "language": {
                "emptyTable": "No roles found",
                "zeroRecords": "No matching roles found"
            },
            "dom": '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                   '<"row"<"col-sm-12"tr>>' +
                   '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
            "autoWidth": false,
            "scrollX": true,
            "scrollCollapse": true
        });
    },

    // Unbind event handlers to prevent duplicates
    unbindEvents: function() {
        // Remove all event handlers
        $('#createRoleForm').off('submit');
        $(document).off('submit', '#editRoleForm');
        $(document).off('click', '.view-btn');
        $(document).off('click', '.edit-btn');
        $(document).off('click', '.delete-btn');
        $(document).off('click', '.restore-btn');
        $('#createRoleModal').off('hidden.bs.modal');
        $('#editRoleModal').off('hidden.bs.modal');
    },

    // Bind event handlers
    bindEvents: function() {
        var self = this;

        // Create Role Form Submission
        $('#createRoleForm').on('submit', function(e) {
            e.preventDefault();
            self.handleCreateRole();
        });

        // Edit Role Form Submission
        $(document).on('submit', '#editRoleForm', function(e) {
            e.preventDefault();
            self.handleEditRole();
        });

        // View Role
        $(document).on('click', '.view-btn', function() {
            var roleId = $(this).data('id');
            self.showRoleDetails(roleId);
        });

        // Edit Role
        $(document).on('click', '.edit-btn', function() {
            var roleId = $(this).data('id');
            self.showRoleEdit(roleId);
        });

        // Delete Role
        $(document).on('click', '.delete-btn', function() {
            var roleId = $(this).data('id');
            self.deleteRole(roleId);
        });

        // Restore Role
        $(document).on('click', '.restore-btn', function() {
            var roleId = $(this).data('id');
            self.restoreRole(roleId);
        });

        // Reset modal forms when closed
        $('#createRoleModal').on('hidden.bs.modal', function() {
            $('#createRoleForm')[0].reset();
        });

        $('#editRoleModal').on('hidden.bs.modal', function() {
            // Clean up if needed
        });
    },

    // Handle Create Role
    handleCreateRole: function() {
        var formData = new FormData();
        
        formData.append('Name', $('#createName').val());
        formData.append('Description', $('#createDescription').val());
        formData.append('IsActive', $('#createIsActive').is(':checked'));
        
        // Add antiforgery token
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        AjaxUtility.submitForm('/RoleManagement/Create', formData, {
            success: function(response) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: response.message,
                    timer: 2000,
                    showConfirmButton: false
                }).then(function() {
                    $('#createRoleModal').modal('hide');
                    RoleManagement.refresh();
                });
            },
            error: function(errorMessage) {
                Swal.fire({
                    icon: 'error',
                    title: 'Create Failed',
                    html: errorMessage
                });
            }
        });
    },

    // Show Role Details
    showRoleDetails: function(roleId) {
        AjaxUtility.loadPartialView('/RoleManagement/Details/' + roleId, 
            '#viewRoleContent', 
            '#viewRoleModal');
    },

    // Show Role Edit
    showRoleEdit: function(roleId) {
        AjaxUtility.loadPartialView('/RoleManagement/Edit/' + roleId, 
            '#editRoleContent', 
            '#editRoleModal');
    },

    // Handle Edit Role
    handleEditRole: function() {
        var formData = new FormData($('#editRoleForm')[0]);
        
        // Add antiforgery token from the main document (not from modal)
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        } 
        
        AjaxUtility.submitForm('/RoleManagement/Edit', formData, {
            success: function(response) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: response.message,
                    timer: 2000,
                    showConfirmButton: false
                }).then(function() {
                    $('#editRoleModal').modal('hide');
                    RoleManagement.refresh();
                });
            },
            error: function(errorMessage) {
                Swal.fire({
                    icon: 'error',
                    title: 'Update Failed',
                    html: errorMessage
                });
            }
        });
    },

    // Delete Role with confirmation (soft delete)
    deleteRole: function(roleId) {
        Swal.fire({
            title: 'Deactivate Role',
            text: 'Are you sure you want to deactivate this role?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, deactivate it!'
        }).then(function(result) {
            if (result.isConfirmed) {
                AjaxUtility.deleteRecord('/RoleManagement/Delete/' + roleId, {
                    success: function(response) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Success',
                            text: response.message,
                            timer: 2000,
                            showConfirmButton: false
                        });
                        RoleManagement.refresh();
                    },
                    error: function(errorMessage) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Deactivate Failed',
                            html: errorMessage
                        });
                    }
                });
            }
        });
    },

    // Restore Role with confirmation
    restoreRole: function(roleId) {
        Swal.fire({
            title: 'Restore Role',
            text: 'Are you sure you want to restore this role?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes, restore it!'
        }).then(function(result) {
            if (result.isConfirmed) {
                AjaxUtility.restoreRecord('/RoleManagement/Restore/' + roleId, {
                    success: function(response) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Success',
                            text: response.message,
                            timer: 2000,
                            showConfirmButton: false
                        });
                        RoleManagement.refresh();
                    },
                    error: function(errorMessage) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Restore Failed',
                            html: errorMessage
                        });
                    }
                });
            }
        });
    },

    // Refresh DataTable
    refresh: function() {
        $('#rolesTable').DataTable().ajax.reload();
    }
};
