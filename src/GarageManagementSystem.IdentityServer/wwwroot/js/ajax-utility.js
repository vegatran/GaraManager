/**
 * AJAX Utility Module
 * 
 * Mô tả:
 * - Cung cấp các utility function chung cho AJAX calls
 * - Error handling và success callbacks chuẩn hóa
 * - Retry mechanism và caching capabilities
 * - CSRF token management
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
        var token = $('meta[name="csrf-token"]').attr('content');
        if (token) {
            $.ajaxSetup({
                beforeSend: function(xhr, settings) {
                    if (!this.crossDomain && settings.type !== 'GET') {
                        xhr.setRequestHeader('X-CSRF-TOKEN', token);
                    }
                }
            });
        }
    },

    // Generic GET request
    get: function(url, successCallback, errorCallback, options) {
        var settings = $.extend({
            url: url,
            type: 'GET',
            success: successCallback,
            error: errorCallback || this.handleError
        }, this.defaultSettings, options);

        return $.ajax(settings);
    },

    // Generic POST request
    post: function(url, data, successCallback, errorCallback, options) {
        var settings = $.extend({
            url: url,
            type: 'POST',
            data: typeof data === 'object' ? JSON.stringify(data) : data,
            contentType: typeof data === 'object' ? 'application/json' : undefined,
            success: successCallback,
            error: errorCallback || this.handleError
        }, this.defaultSettings, options);

        return $.ajax(settings);
    },

    // Generic PUT request
    put: function(url, data, successCallback, errorCallback, options) {
        var settings = $.extend({
            url: url,
            type: 'PUT',
            data: typeof data === 'object' ? JSON.stringify(data) : data,
            contentType: typeof data === 'object' ? 'application/json' : undefined,
            success: successCallback,
            error: errorCallback || this.handleError
        }, this.defaultSettings, options);

        return $.ajax(settings);
    },

    // Generic DELETE request
    delete: function(url, successCallback, errorCallback, options) {
        var settings = $.extend({
            url: url,
            type: 'DELETE',
            success: successCallback,
            error: errorCallback || this.handleError
        }, this.defaultSettings, options);

        return $.ajax(settings);
    },

    // POST with FormData (for file uploads)
    postFormData: function(url, formData, successCallback, errorCallback, options) {
        var settings = $.extend({
            url: url,
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: successCallback,
            error: errorCallback || this.handleError
        }, this.defaultSettings, options);

        return $.ajax(settings);
    },

    // Error handler
    handleError: function(xhr, status, error) {
        var message = 'An error occurred while processing the request.';
        
        if (xhr.responseJSON && xhr.responseJSON.message) {
            message = xhr.responseJSON.message;
        } else if (xhr.responseText) {
            message = xhr.responseText;
        }

        console.error('AJAX Error:', {
            status: status,
            error: error,
            response: xhr.responseText
        });

        IdentityServer.showError(message);
    },

    // Success handler wrapper
    handleSuccess: function(response, successMessage) {
        if (response && response.success === false) {
            IdentityServer.showError(response.message || 'Operation failed');
            return false;
        }

        if (successMessage) {
            IdentityServer.showSuccess(successMessage);
        }
        
        return true;
    },

    // Sequential requests
    sequence: function(requests, finalCallback) {
        var results = [];
        var current = 0;

        function nextResult(result) {
            results.push(result);
            current++;

            if (current < requests.length) {
                requests.forEach(function(request) {
                    NextRequest(current).then(nextResult).fail(function(error) {
                        finalCallback(null, error);
                    });
                });
            } else {
                finalCallback(results, null);
            }
        }
    },

    // Parallel requests
    parallel: function(requests) {
        var promises = requests.map(function(request) {
            return $.ajax(request);
        });

        return Promise.allSettled(promises);
    },

    // Request with retry
    withRetry: function(options, maxRetries, retryDelay) {
        maxRetries = maxRetries || 3;
        retryDelay = retryDelay || 1000;

        var attempt = 0;

        function makeRequest() {
            return $.ajax(options).fail(function(xhr, status, error) {
                attempt++;

                if (attempt < maxRetries && (status === 'timeout' || status === 'error')) {
                    console.warn('Request failed, retrying... Attempt:', attempt);
                    
                    return new Promise(function(resolve, reject) {
                        setTimeout(function() {
                            makeRequest().then(resolve).catch(reject);
                        }, retryDelay);
                    });
                } else {
                    throw new Error('Max retries exceeded');
                }
            });
        }

        return makeRequest();
    },

    // Cache management
    cache: {
        store: {},

        get: function(key, maxAge) {
            var cached = this.store[key];
            
            if (cached && (!maxAge || Date.now() - cached.timestamp < maxAge)) {
                return cached.data;
            }
            
            if (cached) {
                delete this.store[key];
            }
            
            return null;
        },

        set: function(key, data) {
            this.store[key] = {
                data: data,
                timestamp: Date.now()
            };
        },

        clear: function(key) {
            if (key) {
                delete this.store[key];
            } else {
                this.store = {};
            }
        }
    },

    // Request with caching
    getCached: function(url, cacheKey, maxAge, successCallback, errorCallback) {
        maxAge = maxAge || 5 * 60 * 1000; // 5 minutes default

        var cachedData = this.cache.get(cacheKey || url, maxAge);
        
        if (cachedData) {
            successCallback(cachedData);
            return Promise.resolve(cachedData);
        }

        return this.get(url, function(data) {
            this.cache.set(cacheKey || url, data);
            successCallback(data);
        }.bind(this), errorCallback);
    },

    // Cancel request helper
    cancellableRequest: function(options) {
        var xhr = $.ajax(options);
        
        return {
            xhr: xhr,
            cancel: function() {
                if (xhr) {
                    xhr.abort();
                }
            },
            promise: xhr
        };
    },

    // Progress tracking for uploads
    uploadWithProgress: function(url, formData, progressCallback, successCallback, errorCallback) {
        return $.ajax({
            url: url,
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            xhr: function() {
                var xhr = new window.XMLHttpRequest();
                
                xhr.upload.addEventListener('progress', function(evt) {
                    if (evt.lengthComputable) {
                        var percentComplete = evt.loaded / evt.total * 100;
                        if (progressCallback) {
                            progressCallback(percentComplete);
                        }
                    }
                }, false);
                
                return xhr;
            },
            success: successCallback,
            error: errorCallback || this.handleError
        });
    },

    // Download helper
    download: function(url, filename, options) {
        options = options || {};
        
        return $.ajax($.extend({
            url: url,
            method: 'GET',
            xhrFields: {
                responseType: 'blob'
            },
            success: function(data, status, xhr) {
                var contentType = xhr.getResponseHeader('Content-Type');
                var blob = new Blob([data], { type: contentType });
                
                var downloadUrl = window.URL.createObjectURL(blob);
                var link = document.createElement('a');
                link.href = downloadUrl;
                link.download = filename || 'download';
                
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                
                window.URL.revokeObjectURL(downloadUrl);
            }
        }, options));
    },

    // Submit form with FormData
    submitForm: function(url, formData, callbacks) {
        callbacks = callbacks || {};
        
        return this.postFormData(url, formData, 
            function(response) {
                if (response && response.success === false) {
                    var errorMessage = response.message || 'Operation failed';
                    if (response.errorMessages) {
                        errorMessage += '<br>' + response.errorMessages.join('<br>');
                    }
                    if (callbacks.error) {
                        callbacks.error(errorMessage);
                    } else {
                        this.handleError({ responseJSON: { message: errorMessage } });
                    }
                } else if (callbacks.success) {
                    callbacks.success(response);
                }
            }.bind(this),
            function(xhr, status, error) {
                var errorMessage = 'An unknown error occurred.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                    if (xhr.responseJSON.errorMessages) {
                        errorMessage += '<br>' + xhr.responseJSON.errorMessages.join('<br>');
                    }
                } else if (xhr.responseText) {
                    errorMessage = xhr.responseText;
                }
                
                if (callbacks.error) {
                    callbacks.error(errorMessage);
                } else {
                    this.handleError({ responseJSON: { message: errorMessage } });
                }
            }.bind(this)
        );
    },

        // Load partial view into modal
        loadPartialView: function(url, targetSelector, modalSelector) {
            return this.get(url, 
                function(data) {
                    $(targetSelector).html(data);
                    $(modalSelector).modal('show');
                },
                function(xhr, status, error) {
                
                var errorMessage = 'Failed to load data.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                } else if (xhr.responseText) {
                    errorMessage = xhr.responseText;
                }
                
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: errorMessage
                });
            }
        );
    },

    // Confirm delete with SweetAlert2
    confirmDelete: function(title, text, callback) {
        Swal.fire({
            title: title || 'Are you sure?',
            text: text || "You won't be able to revert this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!'
        }).then(function(result) {
            if (result.isConfirmed && callback) {
                callback();
            }
        });
    },

    // Delete record with consistent error handling
    deleteRecord: function(url, callbacks) {
        callbacks = callbacks || {};
        
        // Add antiforgery token
        var token = $('input[name="__RequestVerificationToken"]').first().val();
        var headers = {};
        if (token) {
            headers['RequestVerificationToken'] = token;
        }
        
        return this.post(url, null,
            function(response) {
                if (response && response.success === false) {
                    var errorMessage = response.message || 'Delete failed';
                    if (callbacks.error) {
                        callbacks.error(errorMessage);
                    } else {
                        this.handleError({ responseJSON: { message: errorMessage } });
                    }
                } else if (callbacks.success) {
                    callbacks.success(response);
                }
            }.bind(this),
            function(xhr, status, error) {
                var errorMessage = 'An error occurred while deleting the record.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                } else if (xhr.responseText) {
                    errorMessage = xhr.responseText;
                }
                
                if (callbacks.error) {
                    callbacks.error(errorMessage);
                } else {
                    this.handleError({ responseJSON: { message: errorMessage } });
                }
            }.bind(this),
            { headers: headers }
        );
    }
};

// Initialize CSRF token when document is ready
$(document).ready(function() {
    AjaxUtility.initCsrfToken();
});
