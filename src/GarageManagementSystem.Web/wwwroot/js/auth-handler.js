/**
 * Authentication Handler Module
 * 
 * Handles token timeout and authentication errors
 * Provides centralized authentication management
 */

window.AuthHandler = {
    // Configuration will be loaded from server
    _config: null,

    /**
     * Load configuration from server
     */
    loadConfig: function() {
        var self = this;
        if (this._config) return Promise.resolve(this._config);
        
        return $.get('/Home/GetConfig')
            .done(function(config) {
                self._config = config;
            })
            .fail(function(xhr, status, error) {
                
                self._config = {
                    IdentityServerAuthority: 'https://localhost:44333',
                    ApiBaseUrl: 'https://localhost:44303/api/'
                };
            });
    },

    /**
     * Check if response is unauthorized or requires login
     * @param {object} response - The AJAX response object (xhr or API response data)
     * @returns {boolean}
     */
    isUnauthorized: function(response) {
        return response.status === 401 || 
               (response.responseJSON && response.responseJSON.requiresLogin === true);
    },

    /**
     * Handle 401 Unauthorized response
     */
    handleUnauthorized: function(response, showMessage = true) {
        // Clear any existing alerts
        if (Swal.isVisible()) {
            Swal.close();
        }
        
        if (showMessage) {
            // Show message
            Swal.fire({
                icon: 'warning',
                title: 'Session Expired',
                text: 'Your session has expired. Please login again to continue.',
                confirmButtonText: 'Login Again',
                allowOutsideClick: false,
                allowEscapeKey: false,
                showCancelButton: false
            }).then((result) => {
                this.redirectToLogin();
            });
        } else {
            // Redirect immediately
            this.redirectToLogin();
        }
    },

    /**
     * Redirect to login page
     */
    redirectToLogin: function() {
        var self = this;
        
        // Clear local storage/session storage if needed
        localStorage.clear();
        sessionStorage.clear();
        
        // Load config if not available
        this.loadConfig().then(function() {
            if (!self._config || !self._config.IdentityServerAuthority) {
                window.location.href = '/Home/Login';
                return;
            }
            var currentUrl = encodeURIComponent(window.location.href);
            var loginUrl = self._config.IdentityServerAuthority + '/Account/Login?ReturnUrl=' + currentUrl;
            window.location.href = loginUrl;
        }).catch(function(error) {
            console.error('❌ Error loading config for redirect:', error);
            window.location.href = '/Home/Login';
        });
    },

    /**
     * Setup global AJAX error handler
     */
    setupGlobalHandler: function() {
        var self = this;
        
        // Load config before setting up handler
        this.loadConfig().then(function() {
            $(document).ajaxError(function(event, xhr, settings, error) {
                if (self.isUnauthorized(xhr)) {
                    console.log('🔒 401 Unauthorized detected in global handler');
                    self.handleUnauthorized(xhr, true);
                    return false; // Prevent default error handling
                }
            });
        });
    },

    /**
     * Wrapper for AJAX calls with automatic 401 handling
     */
    ajax: function(options) {
        var originalError = options.error;
        options.error = function(xhr, status, error) {
            if (AuthHandler.isUnauthorized(xhr)) {
                AuthHandler.handleUnauthorized(xhr, true);
            } else if (originalError) {
                originalError(xhr, status, error);
            } else {
                // Fallback to default error handling if no custom handler and not 401
                if (window.AjaxUtility && AjaxUtility.handleError) {
                    AjaxUtility.handleError(xhr, status, error);
                } else {
                    alert('An unexpected error occurred.');
                }
            }
        };
        return $.ajax(options);
    },

    /**
     * Validate API response for requiresLogin flag
     * @param {object} apiResponse - The API response object
     * @returns {boolean} True if no unauthorized issue, false if unauthorized and handled
     */
    validateApiResponse: function(apiResponse) {
        if (apiResponse && apiResponse.requiresLogin === true) {
            AuthHandler.handleUnauthorized(apiResponse, true);
            return false;
        }
        return true;
    }
};

// Initialize global handler on document ready
$(document).ready(function() {
    AuthHandler.setupGlobalHandler();
});