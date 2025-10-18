/**
 * AJAX Utility Module
 * 
 * Provides common utility functions for AJAX calls
 * Standardized error handling and success callbacks
 * CSRF token management
 */

window.AjaxUtility = {
    // Default AJAX settings
    defaultSettings: {
        timeout: 30000, // 30 seconds
        cache: false,
        headers: {}
    },

    // Initialize CSRF token
    initCsrfToken: function() {
        var token = $('input[name="__RequestVerificationToken"]').first().val();
        if (token) {
            this.defaultSettings.headers['RequestVerificationToken'] = token;
        }
    },

    // Standard success callback
    onSuccess: function(data, textStatus, xhr) {
        if (data && data.success) {
            if (data.message) {
                GarageApp.showSuccess(data.message);
            }
        }
    },

    // Standard error callback
    handleError: function(xhr, status, error) {
        console.error('AJAX Error:', {
            status: xhr.status,
            statusText: xhr.statusText,
            responseText: xhr.responseText,
            error: error
        });

        var errorMessage = 'An error occurred';
        
        if (xhr.responseJSON && xhr.responseJSON.message) {
            errorMessage = xhr.responseJSON.message;
        } else if (xhr.responseText) {
            try {
                var response = JSON.parse(xhr.responseText);
                if (GarageApp.parseErrorMessage(response)) {
                    errorMessage = GarageApp.parseErrorMessage(response);
                }
            } catch (e) {
                errorMessage = xhr.responseText;
            }
        }

        // Check for 401 Unauthorized - handled by AuthHandler
        if (xhr.status === 401) {
            if (window.AuthHandler) {
                AuthHandler.handleUnauthorized(xhr, true);
                return;
            }
        }

        // Check for requiresLogin flag in response
        if (xhr.responseJSON && xhr.responseJSON.requiresLogin === true) {
            if (window.AuthHandler) {
                AuthHandler.handleUnauthorized(xhr.responseJSON, true);
                return;
            }
        }

        // Show error message
        if (window.GarageApp && GarageApp.showError) {
            GarageApp.showError(errorMessage);
        } else {
            alert(errorMessage);
        }
    },

    // GET request wrapper
    get: function(url, data, successCallback, errorCallback) {
        var settings = $.extend({}, this.defaultSettings, {
            url: url,
            type: 'GET',
            data: data,
            success: successCallback || this.onSuccess,
            error: errorCallback || this.handleError
        });

        return $.ajax(settings);
    },

    // POST request wrapper
    post: function(url, data, successCallback, errorCallback) {
        var settings = $.extend({}, this.defaultSettings, {
            url: url,
            type: 'POST',
            data: data,
            success: successCallback || this.onSuccess,
            error: errorCallback || this.handleError
        });

        return $.ajax(settings);
    },

    // PUT request wrapper
    put: function(url, data, successCallback, errorCallback) {
        var settings = $.extend({}, this.defaultSettings, {
            url: url,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: successCallback || this.onSuccess,
            error: errorCallback || this.handleError
        });

        return $.ajax(settings);
    },

    // DELETE request wrapper
    delete: function(url, data, successCallback, errorCallback) {
        var settings = $.extend({}, this.defaultSettings, {
            url: url,
            type: 'DELETE',
            data: data,
            success: successCallback || this.onSuccess,
            error: errorCallback || this.handleError
        });

        return $.ajax(settings);
    },

    // Upload file with progress
    uploadFile: function(url, formData, progressCallback, successCallback, errorCallback) {
        var settings = $.extend({}, this.defaultSettings, {
            url: url,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            xhr: function() {
                var xhr = new window.XMLHttpRequest();
                if (progressCallback) {
                    xhr.upload.addEventListener("progress", function(evt) {
                        if (evt.lengthComputable) {
                            var percentComplete = evt.loaded / evt.total;
                            progressCallback(percentComplete);
                        }
                    }, false);
                }
                return xhr;
            },
            success: successCallback || this.onSuccess,
            error: errorCallback || this.handleError
        });

        return $.ajax(settings);
    }
};

// Initialize CSRF token when document is ready
$(document).ready(function() {
    AjaxUtility.initCsrfToken();
});