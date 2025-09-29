$(function () {
    // AdminLTE specific initialization
    // This is usually handled by AdminLTE's own JS, but ensure it's loaded
    // $('[data-widget="treeview"]').Treeview('init'); // Example if needed

    // Initialize SweetAlert2 for notifications
    if (typeof Swal !== 'undefined') {
        // Set default options for notifications
        window.showNotification = function(title, message, type = 'success') {
            Swal.fire({
                title: title,
                text: message,
                icon: type,
                timer: 3000,
                showConfirmButton: false,
                toast: true,
                position: 'top-end'
            });
        };
    }
    
    // Initialize SweetAlert2
    if (typeof Swal !== 'undefined') {
        // Set default options
        Swal.mixin({
            customClass: {
                confirmButton: 'btn btn-success',
                cancelButton: 'btn btn-danger'
            },
            buttonsStyling: false
        });
    }
});

// Utility functions
window.IdentityServer = {
    // Show success message
    showSuccess: function(message) {
        if (typeof showNotification !== 'undefined') {
            showNotification('Success', message, 'success');
        } else if (typeof Swal !== 'undefined') {
            Swal.fire('Success', message, 'success');
        } else {
            alert(message);
        }
    },
    
    // Show error message
    showError: function(message) {
        if (typeof showNotification !== 'undefined') {
            showNotification('Error', message, 'error');
        } else if (typeof Swal !== 'undefined') {
            Swal.fire('Error', message, 'error');
        } else {
            alert(message);
        }
    },
    
    // Show warning message
    showWarning: function(message) {
        if (typeof showNotification !== 'undefined') {
            showNotification('Warning', message, 'warning');
        } else if (typeof Swal !== 'undefined') {
            Swal.fire('Warning', message, 'warning');
        } else {
            alert(message);
        }
    },
    
    // Show info message
    showInfo: function(message) {
        if (typeof showNotification !== 'undefined') {
            showNotification('Info', message, 'info');
        } else if (typeof Swal !== 'undefined') {
            Swal.fire('Info', message, 'info');
        } else {
            alert(message);
        }
    },
    
    // Confirm action
    confirm: function(message, callback) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'Are you sure?',
                text: message,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Yes, proceed!',
                cancelButtonText: 'No, cancel!',
                reverseButtons: true
            }).then(callback);
        } else {
            if (confirm(message)) {
                callback({ isConfirmed: true });
            } else {
                callback({ isConfirmed: false });
            }
        }
    }
};