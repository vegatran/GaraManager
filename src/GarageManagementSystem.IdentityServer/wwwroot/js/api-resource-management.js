var ApiResourceManagement = {
    init: function() {
        this.bindEvents();
        this.loadAvailableScopes();
    },

    bindEvents: function() {
        var self = this;

        // Create API Resource Form
        $('#createApiResourceForm').on('submit', function(e) {
            e.preventDefault();
            self.handleCreateApiResource();
        });

        // Edit API Resource Form
        $(document).on('submit', '#editApiResourceForm', function(e) {
            e.preventDefault();
            self.handleEditApiResource();
        });

        // Create Modal Events - Use ModalSelect2
        $('#createApiResourceModal').on('shown.bs.modal', function() {
            $('#createApiResourceUserClaims').select2({
                placeholder: 'Select user claims',
                dropdownParent: $('#createApiResourceModal')
            });
            
            $('#createApiResourceScopes').select2({
                placeholder: 'Select API scopes',
                dropdownParent: $('#createApiResourceModal'),
                ajax: {
                    url: '/ApiResourceManagement/GetAvailableScopes',
                    dataType: 'json',
                    processResults: function(data) {
                        return {
                            results: $.map(data, function(item) {
                                return {
                                    id: item.name,
                                    text: item.displayName || item.name
                                };
                            })
                        };
                    }
                }
            });
        });

        // Edit Modal Events - Initialize Select2 when modal is shown
        $('#editApiResourceModal').on('shown.bs.modal', function() {
            // Initialize Select2 for User Claims
            if ($('#editApiResourceUserClaims').length) {
                // Load claims options first
                $.get('/ClaimsManagement/GetAvailableClaims', function(data) {
                    $('#editApiResourceUserClaims').empty();
                    $.each(data, function(index, item) {
                        $('#editApiResourceUserClaims').append(new Option(item.text, item.value));
                    });
                    
                    // Initialize Select2 after options are loaded
                    $('#editApiResourceUserClaims').select2({
                        placeholder: 'Select user claims...',
                        allowClear: true,
                        dropdownParent: $('#editApiResourceModal')
                    });
                    
                    // Set selected values from data attributes
                    var selectedClaims = $('#editApiResourceUserClaims').data('selected-values');
                    if (selectedClaims && selectedClaims.length > 0) {
                        var values = selectedClaims.split(',').filter(v => v.trim() !== '');
                        if (values.length > 0) {
                            $('#editApiResourceUserClaims').val(values).trigger('change');
                        }
                    }
                }).fail(function() {
                    // Fallback options if API fails
                    $('#editApiResourceUserClaims').empty().append(
                        '<option value="sub">Subject ID</option>' +
                        '<option value="name">Name</option>' +
                        '<option value="given_name">Given Name</option>' +
                        '<option value="family_name">Family Name</option>'
                    );
                    
                    $('#editApiResourceUserClaims').select2({
                        placeholder: 'Select user claims...',
                        allowClear: true,
                        dropdownParent: $('#editApiResourceModal')
                    });
                });
            }
            
            // Initialize Select2 for API Scopes
            if ($('#editApiResourceScopes').length) {
                // Load scopes options first
                $.get('/ApiResourceManagement/GetAvailableScopes', function(data) {
                    $('#editApiResourceScopes').empty();
                    $.each(data, function(index, item) {
                        $('#editApiResourceScopes').append(new Option(item.text, item.value));
                    });
                    
                    // Initialize Select2 after options are loaded
                    $('#editApiResourceScopes').select2({
                        placeholder: 'Select API scopes...',
                        allowClear: true,
                        dropdownParent: $('#editApiResourceModal')
                    });
                    
                    // Set selected values from data attributes
                    var selectedScopes = $('#editApiResourceScopes').data('selected-values');
                    if (selectedScopes && selectedScopes.length > 0) {
                        var values = selectedScopes.split(',').filter(v => v.trim() !== '');
                        if (values.length > 0) {
                            $('#editApiResourceScopes').val(values).trigger('change');
                        }
                    }
                }).fail(function() {
                    // Fallback options if API fails
                    $('#editApiResourceScopes').empty().append(
                        '<option value="admin.api">Admin API</option>' +
                        '<option value="user.api">User API</option>'
                    );
                    
                    $('#editApiResourceScopes').select2({
                        placeholder: 'Select API scopes...',
                        allowClear: true,
                        dropdownParent: $('#editApiResourceModal')
                    });
                });
            }
        });

        // Modal Hide Events - Destroy Select2
        $('#createApiResourceModal, #editApiResourceModal').on('hide.bs.modal', function() {
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

    loadAvailableScopes: function() {
        // This is now handled by AJAX in Select2 initialization
        // Keeping for backward compatibility if needed
    },

    handleCreateApiResource: function() {
        var formData = new FormData();
        
        // Basic Information
        formData.append('Name', $('#createApiResourceName').val());
        formData.append('DisplayName', $('#createApiResourceDisplayName').val());
        formData.append('Description', $('#createApiResourceDescription').val());
        
        // Settings
        formData.append('Enabled', $('#createApiResourceEnabled').is(':checked'));
        formData.append('ShowInDiscoveryDocument', $('#createApiResourceShowInDiscoveryDocument').is(':checked'));
        
        // Collections
        var userClaims = $('#createApiResourceUserClaims').val() || [];
        userClaims.forEach(function(claim) {
            formData.append('UserClaims', claim);
        });
        
        var scopes = $('#createApiResourceScopes').val() || [];
        scopes.forEach(function(scope) {
            formData.append('Scopes', scope);
        });
        
        var secrets = $('#createApiResourceSecrets').val().split('\n').filter(function(secret) { 
            return secret.trim() !== ''; 
        });
        secrets.forEach(function(secret) {
            formData.append('Secrets', secret.trim());
        });
        
        // Add antiforgery token
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        // Use AjaxUtility for consistent error handling
        AjaxUtility.submitForm('/ApiResourceManagement/Create', formData, {
            success: function(response) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: response.message,
                    showConfirmButton: false,
                    timer: 1500
                }).then(function() {
                    $('#createApiResourceModal').modal('hide');
                    $('#createApiResourceForm')[0].reset();
                    DataTablesUtility.refresh('#apiResourcesTable');
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

    showEditApiResource: function(id) {
        var self = this;
        
        // Load modal content first
        AjaxUtility.loadPartialView('/ApiResourceManagement/Edit/' + id, 
            '#editApiResourceModalBody', 
            '#editApiResourceModal');
    },


    handleEditApiResource: function() {
        var formData = new FormData();
        var form = $('#editApiResourceForm')[0];
        
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
        AjaxUtility.submitForm('/ApiResourceManagement/Edit', formData, {
            success: function(response) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: response.message,
                    showConfirmButton: false,
                    timer: 1500
                }).then(function() {
                    $('#editApiResourceModal').modal('hide');
                    DataTablesUtility.refresh('#apiResourcesTable');
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

    showApiResourceDetails: function(name) {
        AjaxUtility.loadPartialView('/ApiResourceManagement/Details/' + encodeURIComponent(name), 
            '#viewApiResourceModalBody', 
            '#viewApiResourceModal');
    },

    deleteApiResource: function(id) {
        AjaxUtility.confirmDelete('Are you sure?', "You won't be able to revert this!", function() {
            AjaxUtility.deleteRecord('/ApiResourceManagement/Delete/' + id, {
                success: function(response) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Deleted!',
                        text: response.message,
                        showConfirmButton: false,
                        timer: 1500
                    }).then(function() {
                        DataTablesUtility.refresh('#apiResourcesTable');
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

    restoreApiResource: function(id) {
        AjaxUtility.confirmDelete('Are you sure?', "This will restore the API Resource to active status!", function() {
            AjaxUtility.deleteRecord('/ApiResourceManagement/Restore/' + id, {
                success: function(response) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Restored!',
                        text: response.message,
                        showConfirmButton: false,
                        timer: 1500
                    }).then(function() {
                        DataTablesUtility.refresh('#apiResourcesTable');
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
    }
};

// Initialize when document is ready
$(document).ready(function() {
    ApiResourceManagement.init();
});
