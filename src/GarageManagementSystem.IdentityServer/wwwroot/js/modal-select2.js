/**
 * Modal Select2 Manager
 * Quản lý lifecycle của Select2 trong modal để tránh conflict
 */
window.ModalSelect2 = {
    // Khởi tạo Select2 trong modal
    init: function(modalId, options) {
        var defaultOptions = {
            width: '100%',
            placeholder: 'Select options...',
            allowClear: true,
            dropdownParent: $('#' + modalId)
        };
        
        var finalOptions = $.extend({}, defaultOptions, options);
        
        $('#' + modalId).on('shown.bs.modal', function () {
            $('#' + modalId + ' .select2').select2(finalOptions);
        });
        
        // Destroy Select2 trước khi modal hide (khi modal vẫn còn visible)
        $('#' + modalId).on('hide.bs.modal', function () {
            $('#' + modalId + ' .select2').each(function() {
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
    
    // Set selected values cho Select2 trong modal
    setSelectedValues: function(modalId, fieldId, values) {
        $('#' + modalId).on('shown.bs.modal', function () {
            setTimeout(function() {
                $('#' + fieldId).val(values).trigger('change');
            }, 100); // Delay để đảm bảo Select2 đã được khởi tạo
        });
    },
    
    // Khởi tạo tất cả Select2 trong modal với custom options
    initWithCustomOptions: function(modalId, selectors) {
        $('#' + modalId).on('shown.bs.modal', function () {
            $.each(selectors, function(selector, options) {
                var defaultOptions = {
                    width: '100%',
                    placeholder: 'Select options...',
                    allowClear: true,
                    dropdownParent: $('#' + modalId)
                };
                
                var finalOptions = $.extend({}, defaultOptions, options);
                $('#' + modalId + ' ' + selector).select2(finalOptions);
            });
        });
        
        // Destroy tất cả Select2 trước khi modal hide (khi modal vẫn còn visible)
        $('#' + modalId).on('hide.bs.modal', function () {
            $.each(selectors, function(selector, options) {
                $('#' + modalId + ' ' + selector).each(function() {
                    try {
                        if ($(this).hasClass('select2-hidden-accessible')) {
                            $(this).select2('destroy');
                        }
                    } catch (e) {
                        // Ignore destroy errors
                    }
                });
            });
        });
    },
    
    // Reset form và destroy Select2
    reset: function(modalId) {
        $('#' + modalId + ' form')[0].reset();
        $('#' + modalId + ' .select2').each(function() {
            try {
                if ($(this).hasClass('select2-hidden-accessible')) {
                    $(this).select2('destroy');
                }
            } catch (e) {
                // Ignore destroy errors
            }
        });
    }
};

// Auto-initialize cho các modal có class 'modal-with-select2'
$(document).ready(function() {
    $('.modal-with-select2').each(function() {
        var modalId = $(this).attr('id');
        if (modalId) {
            ModalSelect2.init(modalId);
        }
    });
});
