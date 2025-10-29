/**
 * Site-wide JavaScript
 * 
 * Global functions and utilities
 */

// Global GarageApp object
window.GarageApp = {
    // Show success message
    showSuccess: function(message, title) {
        Swal.fire({
            icon: 'success',
            title: title || 'Success',
            text: message,
            timer: 3000,
            timerProgressBar: true
        });
    },

    // Show error message
    showError: function(message, title) {
        Swal.fire({
            icon: 'error',
            title: title || 'Error',
            text: message
        });
    },

    // Show warning message
    showWarning: function(message, title) {
        Swal.fire({
            icon: 'warning',
            title: title || 'Warning',
            text: message
        });
    },

    // Show info message
    showInfo: function(message, title) {
        Swal.fire({
            icon: 'info',
            title: title || 'Information',
            text: message
        });
    },

    // Confirm dialog
    confirm: function(message, title, callback) {
        Swal.fire({
            title: title || 'Are you sure?',
            text: message,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes',
            cancelButtonText: 'No'
        }).then((result) => {
            if (result.isConfirmed && callback) {
                callback();
            }
        });
    },

    // Format currency
    formatCurrency: function(amount, currency) {
        currency = currency || '$';
        return currency + parseFloat(amount).toFixed(2);
    },

    // Format date
    formatDate: function(date, format) {
        format = format || 'YYYY-MM-DD';
        return moment(date).format(format);
    },

    // Validate form
    validateForm: function(formId) {
        var form = $(formId);
        if (form.length === 0) return false;
        
        var isValid = true;
        form.find('[required]').each(function() {
            if (!$(this).val()) {
                isValid = false;
                $(this).addClass('is-invalid');
            } else {
                $(this).removeClass('is-invalid');
            }
        });
        
        return isValid;
    },

    // Clear form
    clearForm: function(formId) {
        var form = $(formId);
        if (form.length > 0) {
            form[0].reset();
            form.find('.is-invalid').removeClass('is-invalid');
        }
    },

    // Show loading spinner
    showLoading: function() {
        Swal.fire({
            title: 'Please wait...',
            html: 'Loading data',
            allowOutsideClick: false,
            allowEscapeKey: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    },

    // Hide loading spinner
    hideLoading: function() {
        Swal.close();
    },

    // Parse error message from API response
    parseErrorMessage: function(response) {
        // Nếu có error trực tiếp (từ Web controller), dùng nó
        if (response.error && response.error.trim() !== '') {
            return response.error;
        }
        
        // Nếu có message trực tiếp, dùng nó
        if (response.message && response.message.trim() !== '') {
            return response.message;
        }
        
        // Nếu có errorMessage, thử parse JSON bên trong
        if (response.errorMessage) {
            try {
                // Tìm JSON trong errorMessage (sau "API Error: BadRequest - ")
                const jsonMatch = response.errorMessage.match(/\{.*\}/);
                if (jsonMatch) {
                    const innerResponse = JSON.parse(jsonMatch[0]);
                    if (innerResponse.message) {
                        return innerResponse.message;
                    }
                }
                // Nếu không parse được JSON, dùng errorMessage gốc
                return response.errorMessage;
            } catch (e) {
                // Nếu parse lỗi, dùng errorMessage gốc
                return response.errorMessage;
            }
        }
        
        // Fallback message
        return 'Đã xảy ra lỗi không xác định';
    }
};

// Document ready handlers
$(document).ready(function() {
    // Initialize tooltips
    $('[data-toggle="tooltip"]').tooltip();

    // Initialize popovers
    $('[data-toggle="popover"]').popover();

    // Auto-hide alerts after 5 seconds
    $('.alert').delay(5000).slideUp(300);

    // Form validation on submit
    $('form').on('submit', function(e) {
        var form = $(this);
        if (form.hasClass('needs-validation')) {
            if (!form[0].checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
            }
            form.addClass('was-validated');
        }
    });

    // Remove invalid class on input change
    $('input, select, textarea').on('change', function() {
        if ($(this).hasClass('is-invalid')) {
            $(this).removeClass('is-invalid');
        }
    });

    // Sidebar toggle
    $('#sidebarToggle').on('click', function() {
        $('body').toggleClass('sidebar-toggled');
        $('.sidebar').toggleClass('toggled');
    });

    // Prevent double submission
    $('form').on('submit', function() {
        var submitBtn = $(this).find('button[type="submit"]');
        submitBtn.prop('disabled', true);
        setTimeout(function() {
            submitBtn.prop('disabled', false);
        }, 3000);
    });
});

// Window resize handler
$(window).on('resize', function() {
    // Auto-hide sidebar on small screens
    if ($(window).width() < 768) {
        $('.sidebar').addClass('toggled');
        $('body').addClass('sidebar-toggled');
    }
});