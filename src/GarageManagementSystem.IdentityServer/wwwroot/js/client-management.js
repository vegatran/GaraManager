/**
 * Client Management JavaScript Module
 * 
 * Mô tả: 
 * - Quản lý tất cả chức năng CRUD cho Client Management
 * - Sử dụng các utility chung từ site.js và modal-select2.js
 * - Tách riêng để không conflict với các module khác
 */

window.ClientManagement = {
    // Initialize module
    init: function() {
        this.initDataTable();
        this.loadApiData();
        this.bindEvents();
    },

    // Initialize DataTable
    initDataTable: function() {
        // Check if DataTable already exists and destroy it first
        if ($.fn.DataTable.isDataTable('#clientsTable')) {
            $('#clientsTable').DataTable().destroy();
        }
        
        $("#clientsTable").DataTable({
            "ajax": {
                url: "/ClientManagement/GetClients",
                type: "GET"
            },
            "columns": [
                { 
                    "data": "clientId",
                    "responsivePriority": 1
                },
                { 
                    "data": "clientName",
                    "responsivePriority": 2
                },
                { 
                    "data": "protocolType", 
                    "defaultContent": "oidc",
                    "responsivePriority": 4
                },
                { 
                    "data": "allowedGrantTypes",
                    "render": function(data) {
                        return data ? data.join(', ') : '';
                    },
                    "responsivePriority": 5
                },
                { 
                    "data": "allowedScopes",
                    "render": function(data) {
                        return data ? data.join(', ') : '';
                    },
                    "responsivePriority": 6
                },
                { 
                    "data": "enabled",
                    "render": function(data) {
                        return data ? '<span class="badge badge-success">Enabled</span>' : '<span class="badge badge-danger">Disabled</span>';
                    },
                    "responsivePriority": 3
                },
                {
                    "data": "id",
                    "render": function(data, type, row) {
                        var buttons = '';
                        buttons += '<div class="action-buttons">';
                        buttons += '<button class="btn btn-info btn-sm view-btn" data-id="' + row.clientId + '" title="View"><i class="fas fa-eye"></i></button>';
                        buttons += '<button class="btn btn-warning btn-sm edit-btn" data-id="' + row.clientId + '" title="Edit"><i class="fas fa-edit"></i></button>';
                        buttons += '<button class="btn btn-danger btn-sm delete-btn" data-id="' + data + '" data-client-id="' + row.clientId + '" title="Delete"><i class="fas fa-trash"></i></button>';
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
                "emptyTable": "No clients found",
                "zeroRecords": "No matching clients found"
            },
            "dom": '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                   '<"row"<"col-sm-12"tr>>' +
                   '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
            "autoWidth": false,
            "scrollX": true,
            "scrollCollapse": true
        });
    },

    // Load API data for select dropdowns
    loadApiData: function() {
        // Load Grant Types - simplified  
        $.get('/ClientManagement/GetAvailableGrantTypes', function(data) {
            $('#createGrantTypes, #editGrantTypes').empty();
            $.each(data, function(index, item) {
                $('#createGrantTypes, #editGrantTypes').append(new Option(item.text, item.value));
            });
        }).fail(function() {
            // Fallback options if API fails
            $('#createGrantTypes, #editGrantTypes').empty().append(
                '<option value="authorization_code">Authorization Code</option>' +
                '<option value="client_credentials">Client Credentials</option>' +
                '<option value="implicit">Implicit</option>'
            );
        });

        // Load Scopes - simplified
        $.get('/ClientManagement/GetAvailableScopes', function(data) {
            $('#createScopes, #editScopes').empty();
            $.each(data, function(index, item) {
                $('#createScopes, #editScopes').append(new Option(item.text, item.value));
            });
        }).fail(function() {
            // Fallback options if API fails
            $('#createScopes, #editScopes').empty().append(
                '<option value="openid">OpenID</option>' +
                '<option value="profile">Profile</option>' +
                '<option value="email">Email</option>'
            );
        });
    },

    // Bind event handlers
    bindEvents: function() {
        var self = this;

        // Initialize Select2
        $('.select2').select2({
            width: '100%'
        });

        // Create Client Form Submission
        $('#createClientForm').on('submit', function(e) {
            e.preventDefault();
            self.handleCreateClient();
        });

        // Edit Client Form Submission
        $(document).on('submit', '#editClientForm', function(e) {
            e.preventDefault();
            self.handleEditClient();
        });

        // View Client
        $(document).on('click', '.view-btn', function() {
            var clientId = $(this).data('id');
            self.showClientDetails(clientId);
        });

        // Edit Client
        $(document).on('click', '.edit-btn', function() {
            var clientId = $(this).data('id');
            self.showClientEdit(clientId);
        });

        // Delete Client
        $(document).on('click', '.delete-btn', function() {
            var clientId = $(this).data('id');
            var clientDisplayId = $(this).data('client-id');
            self.deleteClient(clientId, clientDisplayId);
        });

        // Initialize Select2 for Create modal
        $('#createClientModal').on('shown.bs.modal', function() {
            // Initialize Select2 for Grant Types and Scopes
            $('#createGrantTypes').select2({
                placeholder: 'Select grant types...',
                allowClear: true,
                dropdownParent: $('#createClientModal')
            });
            
            $('#createScopes').select2({
                placeholder: 'Select scopes...',
                allowClear: true,
                dropdownParent: $('#createClientModal')
            });
            
            // Add change event handlers for Create modal
            $('#createGrantTypes').on('change', function() {
                $(this).trigger('change.select2');
            });
            
            $('#createScopes').on('change', function() {
                $(this).trigger('change.select2');
            });
        });

        // Initialize Select2 for Edit modal
        $('#editClientModal').on('shown.bs.modal', function() {
            // Initialize Select2 for Grant Types and Scopes if they exist
            if ($('#editGrantTypes').length) {
                $('#editGrantTypes').select2({
                    placeholder: 'Select grant types...',
                    allowClear: true,
                    dropdownParent: $('#editClientModal')
                });
            }
            
            if ($('#editScopes').length) {
                $('#editScopes').select2({
                    placeholder: 'Select scopes...',
                    allowClear: true,
                    dropdownParent: $('#editClientModal')
                });
            }
        });

        // Reset modal forms when closed
        $('#createClientModal').on('hidden.bs.modal', function() {
            $('#createClientForm')[0].reset();
        });

        $('#editClientModal').on('hidden.bs.modal', function() {
            // Destroy Select2 instances to prevent conflicts
            if ($('#editGrantTypes').hasClass('select2-hidden-accessible')) {
                $('#editGrantTypes').select2('destroy');
            }
            if ($('#editScopes').hasClass('select2-hidden-accessible')) {
                $('#editScopes').select2('destroy');
            }
        });
    },

    // Handle Create Client
    handleCreateClient: function() {
        var formData = new FormData();
        
        // Basic Information
        formData.append('ClientId', $('#createClientId').val());
        formData.append('ClientName', $('#createClientName').val());
        formData.append('ClientSecret', $('#createClientSecret').val());
        formData.append('Description', $('#createDescription').val());
        formData.append('ProtocolType', $('#createProtocolType').val());
        
        // Checkboxes
        formData.append('Enabled', $('#createEnabled').is(':checked'));
        formData.append('RequireClientSecret', $('#createRequireClientSecret').is(':checked'));
        formData.append('RequirePkce', $('#createRequirePkce').is(':checked'));
        formData.append('AllowOfflineAccess', $('#createAllowOfflineAccess').is(':checked'));
        formData.append('IncludeJwtId', $('#createIncludeJwtId').is(':checked'));
        formData.append('RequireConsent', $('#createRequireConsent').is(':checked'));
        formData.append('AllowRememberConsent', $('#createAllowRememberConsent').is(':checked'));
        formData.append('AllowAccessTokensViaBrowser', $('#createAllowAccessTokensViaBrowser').is(':checked'));
        formData.append('RequireRequestObject', $('#createRequireRequestObject').is(':checked'));
        formData.append('FrontChannelLogoutSessionRequired', $('#createFrontChannelLogoutSessionRequired').is(':checked'));
        formData.append('BackChannelLogoutSessionRequired', $('#createBackChannelLogoutSessionRequired').is(':checked'));
        
        // Collections
        var grantTypes = $('#createGrantTypes').val() || [];
        grantTypes.forEach(function(gt) {
            formData.append('GrantTypes', gt);
        });
        
        var scopes = $('#createScopes').val() || [];
        scopes.forEach(function(scope) {
            formData.append('Scopes', scope);
        });
        
        var redirectUris = $('#createRedirectUris').val().split('\n').filter(function(uri) { return uri.trim() !== ''; });
        redirectUris.forEach(function(uri) {
            formData.append('RedirectUris', uri);
        });
        
        var postLogoutUris = $('#createPostLogoutRedirectUris').val().split('\n').filter(function(uri) { return uri.trim() !== ''; });
        postLogoutUris.forEach(function(uri) {
            formData.append('PostLogoutRedirectUris', uri);
        });
        
        var corsOrigins = $('#createAllowedCorsOrigins').val().split('\n').filter(function(origin) { return origin.trim() !== ''; });
        corsOrigins.forEach(function(origin) {
            formData.append('AllowedCorsOrigins', origin);
        });
        
        // Add antiforgery token
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        $.ajax({
            url: '/ClientManagement/Create',
            type: 'POST',
            processData: false,
            contentType: false,
            data: formData,
            success: function(response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: 'Client created successfully!',
                        timer: 2000,
                        showConfirmButton: false
                    });
                    $('#createClientModal').modal('hide');
                    $('#createClientForm')[0].reset();
                    ClientManagement.refresh();
                } else {
                    var errorText = response.message || 'Failed to create client';
                    if (response.errorMessages && response.errorMessages.length > 0) {
                        errorText += '\n\nValidation Errors:\n' + response.errorMessages.join('\n');
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
                    text: 'An error occurred while creating the client'
                });
            }
        });
    },

    // Handle Edit Client
    handleEditClient: function() {
        // Include antiforgery token in form data
        var formData = new FormData($('#editClientForm')[0]);
        
        // Ensure required fields are included (readonly fields might not be included in FormData)
        var clientId = $('#editClientIdInput').val();
        var clientName = $('#editClientNameInput').val();

        // Manually add required fields if they're missing or empty
        if (!formData.has('ClientId') || !formData.get('ClientId')) {
            formData.set('ClientId', clientId);
        }
        if (!formData.has('ClientName') || !formData.get('ClientName')) {
            formData.set('ClientName', clientName);
        }
        // Add antiforgery token from the main document (not from modal)
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        } 
        var clientId = $('#editClientDbId').val();
        
        
        $.ajax({
            url: '/ClientManagement/Edit/' + clientId,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: 'Client updated successfully!',
                        timer: 2000,
                        showConfirmButton: false
                    });
                    $('#editClientModal').modal('hide');
                    ClientManagement.refresh();
                } else {
                    var errorText = response.message || 'Failed to update client';
                    if (response.errorMessages && response.errorMessages.length > 0) {
                        errorText += '\n\nValidation Errors:\n' + response.errorMessages.join('\n');
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
                var errorMessage = 'An error occurred while updating the client';
                
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
                
                console.log('Update Error:', {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    responseText: xhr.responseText,
                    responseJSON: xhr.responseJSON
                });
                
                // Log detailed error for debugging
                if (xhr.responseJSON && xhr.responseJSON.details) {
                    console.log('Server Error Details:', xhr.responseJSON.details);
                }
            }
        });
    },

    // Show Client Details
    showClientDetails: function(clientId) {
        $.get('/ClientManagement/Details/' + clientId, function(data) {
            $('#viewClientContent').html(data);
            $('#viewClientModal').modal('show');
        }).fail(function() {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to load client details'
            });
        });
    },

    // Show Client Edit
    showClientEdit: function(clientId) {
        $.get('/ClientManagement/Edit/' + clientId, function(data) {
            $('#editClientModalBody').html(data);
            $('#editClientModal').modal('show');
            
            // Load API data first, then initialize Select2
            this.loadApiDataForEdit();
        }.bind(this)).fail(function() {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to load client data for editing'
            });
        });
    },

    // Load API data specifically for Edit modal
    loadApiDataForEdit: function() {
        var self = this;
        
        // Load Grant Types
        $.get('/ClientManagement/GetAvailableGrantTypes', function(data) {
            console.log('Grant Types API Response:', data);
            $('#editGrantTypes').empty();
            $.each(data, function(index, item) {
                $('#editGrantTypes').append(new Option(item.text, item.value));
            });
            
            // Initialize Select2 after options are loaded
            $('#editGrantTypes').select2({
                placeholder: 'Select grant types...',
                allowClear: true,
                dropdownParent: $('#editClientModal')
            });
            
            // Set selected values
            var selectedGrantTypes = $('#editGrantTypes').data('selected-values');
            console.log('Selected Grant Types from data attribute:', selectedGrantTypes);
            if (selectedGrantTypes) {
                var values = selectedGrantTypes.split(',');
                console.log('Setting Grant Types values:', values);
                $('#editGrantTypes').val(values).trigger('change');
                
                // Trigger change event for form validation
                $('#editGrantTypes').on('change', function() {
                    $(this).trigger('change.select2');
                });
            }
        }).fail(function() {
            // Fallback options
            $('#editGrantTypes').empty().append(
                '<option value="authorization_code">Authorization Code</option>' +
                '<option value="client_credentials">Client Credentials</option>' +
                '<option value="implicit">Implicit</option>'
            );
            
            $('#editGrantTypes').select2({
                placeholder: 'Select grant types...',
                allowClear: true,
                dropdownParent: $('#editClientModal')
            });
        });

        // Load Scopes
        $.get('/ClientManagement/GetAvailableScopes', function(data) {
            console.log('Scopes API Response:', data);
            $('#editScopes').empty();
            $.each(data, function(index, item) {
                $('#editScopes').append(new Option(item.text, item.value));
            });
            
            // Initialize Select2 after options are loaded
            $('#editScopes').select2({
                placeholder: 'Select scopes...',
                allowClear: true,
                dropdownParent: $('#editClientModal')
            });
            
            // Set selected values
            var selectedScopes = $('#editScopes').data('selected-values');
            console.log('Selected Scopes from data attribute:', selectedScopes);
            if (selectedScopes) {
                var values = selectedScopes.split(',');
                console.log('Setting Scopes values:', values);
                $('#editScopes').val(values).trigger('change');
                
                // Trigger change event for form validation
                $('#editScopes').on('change', function() {
                    $(this).trigger('change.select2');
                });
            }
        }).fail(function() {
            // Fallback options
            $('#editScopes').empty().append(
                '<option value="openid">OpenID</option>' +
                '<option value="profile">Profile</option>' +
                '<option value="email">Email</option>'
            );
            
            $('#editScopes').select2({
                placeholder: 'Select scopes...',
                allowClear: true,
                dropdownParent: $('#editClientModal')
            });
        });
    },

    // Delete Client with confirmation
    deleteClient: function(clientId, clientDisplayId) {
        Swal.fire({
            title: 'Delete Client',
            text: 'Are you sure you want to delete client "' + clientDisplayId + '"?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!'
        }).then(function(result) {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/ClientManagement/Delete/' + clientId,
                    type: 'DELETE',
                    success: function(response) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Success',
                            text: 'Client deleted successfully!',
                            timer: 2000,
                            showConfirmButton: false
                        });
                        ClientManagement.refresh();
                    },
                    error: function() {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: 'An error occurred while deleting the client'
                        });
                    }
                });
            }
        });
    },

    // Refresh DataTable
    refresh: function() {
        $('#clientsTable').DataTable().ajax.reload();
    }
};

// Initialize when document is ready (only once)
$(document).ready(function() {
    // Do not auto-initialize here - let pages control initialization manually
    // This prevents duplicate initialization from auto-loading globals
});
