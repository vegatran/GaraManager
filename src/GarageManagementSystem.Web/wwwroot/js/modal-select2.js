/**
 * Modal Select2 Utility
 * 
 * Handles Select2 initialization within Bootstrap modals
 * Prevents z-index issues with Select2 dropdowns
 */

window.ModalSelect2 = {
    // Initialize Select2 on elements within a modal
    init: function(modalSelector) {
        var $modal = $(modalSelector);
        
        // Initialize Select2 on all select elements within the modal
        $modal.find('select.select2').each(function() {
            var $select = $(this);
            
            // Destroy existing Select2 instance if any
            if ($select.hasClass('select2-hidden-accessible')) {
                $select.select2('destroy');
            }
            
            // Initialize Select2
            $select.select2({
                dropdownParent: $modal,
                width: '100%',
                placeholder: $select.data('placeholder') || 'Select an option',
                allowClear: true
            });
        });
    },

    // Destroy Select2 instances within a modal
    destroy: function(modalSelector) {
        $(modalSelector).find('select.select2').each(function() {
            if ($(this).hasClass('select2-hidden-accessible')) {
                $(this).select2('destroy');
            }
        });
    },

    // Reinitialize Select2 when modal is shown
    setupAutoInit: function(modalSelector) {
        var self = this;
        $(modalSelector).on('shown.bs.modal', function() {
            self.init(modalSelector);
        });
        
        $(modalSelector).on('hidden.bs.modal', function() {
            self.destroy(modalSelector);
        });
    }
};

// Auto-initialize Select2 in modals when document is ready
$(document).ready(function() {
    // Find all modals with Select2 elements
    $('.modal').each(function() {
        var modalId = '#' + $(this).attr('id');
        if ($(this).find('select.select2').length > 0) {
            ModalSelect2.setupAutoInit(modalId);
        }
    });
});