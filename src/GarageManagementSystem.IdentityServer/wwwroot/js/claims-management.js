/**
 * Claims Management JavaScript Module
 * 
 * Mô tả: 
 * - Quản lý tất cả chức năng CRUD cho Claims Management
 * - Sử dụng các utility chung từ site.js, ajax-utility.js, datatables-utility.js
 * - Tách riêng để không conflict với các module khác
 */

var ClaimsManagement = {
    // Initialize module
    init: function() {
        this.bindEvents();
    },

    // Bind event handlers
    bindEvents: function() {
        var self = this;

        // Create Claim Form Submission
        $(document).on('submit', '#createClaimForm', function(e) {
            e.preventDefault();
            self.handleCreateClaim();
        });

        // Edit Claim Form Submission
        $(document).on('submit', '#editClaimForm', function(e) {
            e.preventDefault();
            self.handleEditClaim();
        });
    },

    // Show Create Claim Modal
    showCreateClaim: function() {
        AjaxUtility.loadPartialView('/ClaimsManagement/Create', 
            '#createClaimModalBody', 
            '#createClaimModal');
    },

    // Handle Create Claim
    handleCreateClaim: function() {
        var formData = new FormData($('#createClaimForm')[0]);
        
        // Add antiforgery token
        var token = $('input[name="__RequestVerificationToken"]').first().val();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        // Use AjaxUtility for consistent error handling
        AjaxUtility.submitForm('/ClaimsManagement/Create', formData, {
            success: function(response) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: response.message,
                    showConfirmButton: false,
                    timer: 1500
                }).then(function() {
                    $('#createClaimModal').modal('hide');
                    $('#createClaimForm')[0].reset();
                    DataTablesUtility.refresh('#claimsTable');
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

    // Show Edit Claim Modal
    showEditClaim: function(id) {
        AjaxUtility.loadPartialView('/ClaimsManagement/Edit/' + id, 
            '#editClaimModalBody', 
            '#editClaimModal');
    },

    // Handle Edit Claim
    handleEditClaim: function() {
        var formData = new FormData($('#editClaimForm')[0]);
        
        // Add antiforgery token
        var token = $('input[name="__RequestVerificationToken"]').first().val();
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        // Use AjaxUtility for consistent error handling
        AjaxUtility.submitForm('/ClaimsManagement/Edit', formData, {
            success: function(response) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: response.message,
                    showConfirmButton: false,
                    timer: 1500
                }).then(function() {
                    $('#editClaimModal').modal('hide');
                    DataTablesUtility.refresh('#claimsTable');
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

    // Show Claim Details
    showClaimDetails: function(id) {
        AjaxUtility.loadPartialView('/ClaimsManagement/Details/' + id, 
            '#viewClaimModalBody', 
            '#viewClaimModal');
    },

            // Delete Claim (Soft Delete)
            deleteClaim: function(id) {
                AjaxUtility.confirmDelete('Are you sure?', "You won't be able to revert this!", function() {
                    AjaxUtility.deleteRecord('/ClaimsManagement/Delete/' + id, {
                        success: function(response) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Deleted!',
                                text: response.message,
                                showConfirmButton: false,
                                timer: 1500
                            });
                            DataTablesUtility.refresh('#claimsTable');
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

            // Restore Claim
            restoreClaim: function(id) {
                AjaxUtility.confirmDelete('Are you sure?', "This will restore the claim to active status!", function() {
                    AjaxUtility.deleteRecord('/ClaimsManagement/Restore/' + id, {
                        success: function(response) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Restored!',
                                text: response.message,
                                showConfirmButton: false,
                                timer: 1500
                            });
                            DataTablesUtility.refresh('#claimsTable');
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
    ClaimsManagement.init();
});
