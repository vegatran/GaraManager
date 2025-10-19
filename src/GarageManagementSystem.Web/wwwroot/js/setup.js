/**
 * Setup Module
 * 
 * Handles setup and configuration operations
 */

window.Setup = {
    // Initialize setup module
    init: function() {
        this.bindEvents();
        this.loadSettings();
    },

    // Bind events
    bindEvents: function() {
        var self = this;

        // Save settings button
        $(document).on('click', '#saveSettingsBtn', function() {
            self.saveSettings();
        });

        // Test connection button
        $(document).on('click', '#testConnectionBtn', function() {
            self.testConnection();
        });
    },

    // Load settings
    loadSettings: function() {
        $.ajax({
            url: '/Setup/GetSettings',
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    Setup.populateSettings(response.data);
                }
            },
            error: function(xhr, status, error) {
            }
        });
    },

    // Populate settings form
    populateSettings: function(settings) {
        $('#garageName').val(settings.garageName || '');
        $('#garageAddress').val(settings.garageAddress || '');
        $('#garagePhone').val(settings.garagePhone || '');
        $('#garageEmail').val(settings.garageEmail || '');
        $('#taxRate').val(settings.taxRate || 0);
        $('#currency').val(settings.currency || 'USD');
    },

    // Save settings
    saveSettings: function() {
        var formData = {
            GarageName: $('#garageName').val(),
            GarageAddress: $('#garageAddress').val(),
            GaragePhone: $('#garagePhone').val(),
            GarageEmail: $('#garageEmail').val(),
            TaxRate: parseFloat($('#taxRate').val()) || 0,
            Currency: $('#currency').val()
        };

        $.ajax({
            url: '/Setup/SaveSettings',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess('Settings saved successfully!');
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error saving settings');
                }
            },
            error: function(xhr, status, error) {
                GarageApp.showError('Error saving settings');
            }
        });
    },

    // Test database connection
    testConnection: function() {
        $.ajax({
            url: '/Setup/TestConnection',
            type: 'GET',
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess('Connection successful!');
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Connection failed');
                }
            },
            error: function(xhr, status, error) {
                GarageApp.showError('Connection test failed');
            }
        });
    }
};

// Initialize setup when document is ready
$(document).ready(function() {
    if ($('#setupPage').length > 0) {
        Setup.init();
    }
});