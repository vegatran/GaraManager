/**
 * Identity Resource Management JavaScript Module
 * 
 * Mô tả: 
 * - Quản lý tất cả chức năng CRUD cho Identity Resource Management
 * - Sử dụng các utility chung từ site.js và modal-select2.js
 * - Tách riêng để không conflict với các module khác
 */

window.IdentityResourceManagement = {
    // Initialize module
    init: function() {
        this.initDataTable();
        this.loadApiData();
        this.bindEvents();
    },

    // Initialize DataTable
    initDataTable: function() {
        // Check if DataTable already exists and destroy it first
        if ($.fn.DataTable.isDataTable('#identityResourcesTable')) {
            $('#identityResourcesTable').DataTable().destroy();
        }
        
        $("#identityResourcesTable").DataTable({
            "ajax": {
                url: "/IdentityResourceManagement/GetIdentityResources",
                type: "GET"
            },
            "columns": [
                { 
                    "data": "name",
                    "responsivePriority": 1
                },
                { 
                    "data": "displayName",
                    "responsivePriority": 2
                },
                { 
                    "data": "description",
                    "responsivePriority": 3
                },
                { 
                    "data": "required",
                    "render": function(data) {
                        return data ? '<span class="badge badge-warning">Required</span>' : '<span class="badge badge-secondary">Optional</span>';
                    },
                    "responsivePriority": 4
                },
                { 
                    "data": "emphasize",
                    "render": function(data) {
                        return data ? '<span class="badge badge-info">Emphasize</span>' : '<span class="badge badge-secondary">Normal</span>';
                    },
                    "responsivePriority": 5
                },
                { 
                    "data": "showInDiscoveryDocument",
                    "render": function(data) {
                        return data ? '<span class="badge badge-success">Yes</span>' : '<span class="badge badge-danger">No</span>';
                    },
                    "responsivePriority": 6
                },
                { 
                    "data": "userClaims",
                    "render": function(data) {
                        if (data && data.length > 0) {
                            return data.map(claim => `<span class="badge badge-primary mr-1">${claim}</span>`).join('');
                        }
                        return '<span class="text-muted">None</span>';
                    },
                    "responsivePriority": 7
                },
                {
                    "data": null,
                    "orderable": false,
                    "responsivePriority": 8,
                    "render": function(data, type, row) {
                        var buttons = '<div class="action-buttons">';
                        
                        // View button
                        buttons += `<button class="btn btn-info btn-sm" onclick="IdentityResourceManagement.showIdentityResourceDetails(${row.id})" title="View">
                                       <i class="fas fa-eye"></i>
                                   </button>`;
                        
                        // Edit button
                        buttons += `<button class="btn btn-warning btn-sm" onclick="IdentityResourceManagement.showEditIdentityResource(${row.id})" title="Edit">
                                       <i class="fas fa-edit"></i>
                                   </button>`;
                        
                        // Conditional Delete/Restore button
                        if (row.isActive) {
                            // Show Delete button for active resources
                            buttons += `<button class="btn btn-danger btn-sm" onclick="IdentityResourceManagement.deleteIdentityResource(${row.id})" title="Delete">
                                           <i class="fas fa-trash"></i>
                                       </button>`;
                        } else {
                            // Show Restore button for inactive resources
                            buttons += `<button class="btn btn-success btn-sm" onclick="IdentityResourceManagement.restoreIdentityResource(${row.id})" title="Restore">
                                           <i class="fas fa-undo"></i>
                                       </button>`;
                        }
                        
                        buttons += '</div>';
                        return buttons;
                    }
                }
            ],
            "responsive": true,
            "processing": true,
            "serverSide": false,
            "pageLength": 25,
            "language": {
                "processing": "Loading Identity Resources...",
                "emptyTable": "No Identity Resources found",
                "info": "Showing _START_ to _END_ of _TOTAL_ Identity Resources",
                "infoEmpty": "Showing 0 to 0 of 0 Identity Resources",
                "infoFiltered": "(filtered from _MAX_ total Identity Resources)"
            }
        });
    },

    // Load API data
    loadApiData: function() {
        // This function can be used to preload any necessary data
        // For now, it's kept for consistency with other management modules
    },

    // Bind events
    bindEvents: function() {
        var self = this;

        // Create Identity Resource Form
        $('#createIdentityResourceForm').on('submit', function(e) {
            e.preventDefault();
            self.handleCreateIdentityResource();
        });

        // Edit Identity Resource Form
        $(document).on('submit', '#editIdentityResourceForm', function(e) {
            e.preventDefault();
            self.handleEditIdentityResource();
        });

        // Create Modal Events - Initialize Select2
        $('#createIdentityResourceModal').on('shown.bs.modal', function() {
            // Load claims options first
            $.get('/IdentityResourceManagement/GetAvailableClaims', function(data) {
                $('#createUserClaims').empty();
                $.each(data, function(index, item) {
                    $('#createUserClaims').append(new Option(item.text, item.value));
                });
                
                // Initialize Select2 after options are loaded
                $('#createUserClaims').select2({
                    placeholder: 'Select user claims...',
                    allowClear: true,
                    dropdownParent: $('#createIdentityResourceModal')
                });
            }).fail(function() {
                // Fallback options if API fails
                $('#createUserClaims').empty().append(
                    '<option value="sub">Subject ID</option>' +
                    '<option value="name">Name</option>' +
                    '<option value="email">Email</option>' +
                    '<option value="role">Role</option>'
                );
                
                $('#createUserClaims').select2({
                    placeholder: 'Select user claims...',
                    allowClear: true,
                    dropdownParent: $('#createIdentityResourceModal')
                });
            });
        });

        // Edit Modal Events - Initialize Select2 when modal is shown
        $('#editIdentityResourceModal').on('shown.bs.modal', function() {
            // Initialize Select2 for User Claims
            if ($('#editUserClaims').length) {
                // Load claims options first
                $.get('/IdentityResourceManagement/GetAvailableClaims', function(data) {
                    $('#editUserClaims').empty();
                    $.each(data, function(index, item) {
                        $('#editUserClaims').append(new Option(item.text, item.value));
                    });
                    
                    // Initialize Select2 after options are loaded
                    $('#editUserClaims').select2({
                        placeholder: 'Select user claims...',
                        allowClear: true,
                        dropdownParent: $('#editIdentityResourceModal')
                    });
                    
                    // Set selected values from data attributes
                    var selectedClaims = $('#editUserClaims').data('selected-values');
                    if (selectedClaims && selectedClaims.length > 0) {
                        var values = selectedClaims.split(',').filter(v => v.trim() !== '');
                        if (values.length > 0) {
                            $('#editUserClaims').val(values).trigger('change');
                        }
                    }
                }).fail(function() {
                    // Fallback options if API fails
                    $('#editUserClaims').empty().append(
                        '<option value="sub">Subject ID</option>' +
                        '<option value="name">Name</option>' +
                        '<option value="email">Email</option>' +
                        '<option value="role">Role</option>'
                    );
                    
                    $('#editUserClaims').select2({
                        placeholder: 'Select user claims...',
                        allowClear: true,
                        dropdownParent: $('#editIdentityResourceModal')
                    });
                });
            }
        });

        // Modal Hide Events - Destroy Select2
        $('#createIdentityResourceModal, #editIdentityResourceModal').on('hide.bs.modal', function() {
            $(this).find('select').each(function() {
                try {
                    if ($(this).hasClass('select2-hidden-accessible')) {
                        $(this).select2('destroy');
                    }
                } catch (e) {
                    // Ignore destroy errors
                }
            });
        });
    },

    // Handle Create Identity Resource
    handleCreateIdentityResource: function() {
        var formData = new FormData();
        
        // Basic Information
        formData.append('Name', $('#createIdentityResourceForm input[name="Name"]').val());
        formData.append('DisplayName', $('#createIdentityResourceForm input[name="DisplayName"]').val());
        formData.append('Description', $('#createIdentityResourceForm textarea[name="Description"]').val());
        
        // Settings
        formData.append('Required', $('#createIdentityResourceForm input[name="Required"]').is(':checked'));
        formData.append('Emphasize', $('#createIdentityResourceForm input[name="Emphasize"]').is(':checked'));
        formData.append('ShowInDiscoveryDocument', $('#createIdentityResourceForm input[name="ShowInDiscoveryDocument"]').is(':checked'));
        
        // Collections
        var userClaims = $('#createUserClaims').val() || [];
        userClaims.forEach(function(claim) {
            formData.append('UserClaims', claim);
        });
        
        // Add antiforgery token
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        // Use AjaxUtility for consistent error handling
        AjaxUtility.submitForm('/IdentityResourceManagement/Create', formData, {
            success: function(response) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Success',
                            text: response.message,
                            showConfirmButton: false,
                            timer: 1500
                        }).then(function() {
                            $('#createIdentityResourceModal').modal('hide');
                            $('#createIdentityResourceForm')[0].reset();
                            IdentityResourceManagement.refresh();
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

    // Show Edit Identity Resource Modal
    showEditIdentityResource: function(id) {
        AjaxUtility.loadPartialView('/IdentityResourceManagement/Edit/' + id, 
            '#editIdentityResourceModalBody', 
            '#editIdentityResourceModal');
    },

    // Handle Edit Identity Resource
    handleEditIdentityResource: function() {
        var formData = new FormData();
        var form = $('#editIdentityResourceForm')[0];
        
        // Get all form data
        for (var i = 0; i < form.elements.length; i++) {
            var element = form.elements[i];
            if (element.name && element.type !== 'file') {
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
        
        // Add antiforgery token from the main document
        var token = $('input[name="__RequestVerificationToken"]').first().val();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        // Use AjaxUtility for consistent error handling
        AjaxUtility.submitForm('/IdentityResourceManagement/Edit', formData, {
            success: function(response) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Success',
                            text: response.message,
                            showConfirmButton: false,
                            timer: 1500
                        }).then(function() {
                            $('#editIdentityResourceModal').modal('hide');
                            IdentityResourceManagement.refresh();
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

    // Show Identity Resource Details Modal
    showIdentityResourceDetails: function(id) {
        AjaxUtility.loadPartialView('/IdentityResourceManagement/Details/' + id, 
            '#viewIdentityResourceModalBody', 
            '#viewIdentityResourceModal');
    },

    // Delete Identity Resource (Soft Delete)
    deleteIdentityResource: function(id) {
        AjaxUtility.confirmDelete('Are you sure?', "You won't be able to revert this!", function() {
            AjaxUtility.deleteRecord('/IdentityResourceManagement/Delete/' + id, {
                success: function(response) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Deleted!',
                        text: response.message,
                        showConfirmButton: false,
                        timer: 1500
                    }).then(function() {
                        IdentityResourceManagement.refresh();
                    });
                },
                error: function(errorMessage) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Delete Failed',
                        html: errorMessage
                    });
                }
            });
        });
    },

    // Restore Identity Resource
    restoreIdentityResource: function(id) {
        AjaxUtility.confirmDelete('Are you sure?', "This will restore the Identity Resource to active status!", function() {
            AjaxUtility.deleteRecord('/IdentityResourceManagement/Restore/' + id, {
                success: function(response) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Restored!',
                        text: response.message,
                        showConfirmButton: false,
                        timer: 1500
                    }).then(function() {
                        IdentityResourceManagement.refresh();
                    });
                },
                error: function(errorMessage) {
                    Swal.fire({
                        icon: 'error',
                        title: 'Restore Failed',
                        html: errorMessage
                    });
                }
            });
        });
    },

    // Refresh DataTable
    refresh: function() {
        $('#identityResourcesTable').DataTable().ajax.reload();
    }
};

// Initialize when document is ready (only once)
$(document).ready(function() {
    // Do not auto-initialize here - let pages control initialization manually
    // This prevents duplicate initialization from auto-loading globals
});
