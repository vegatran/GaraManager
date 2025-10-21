/**
 * Parts Classification Presets
 * 
 * Hệ thống preset cho phân loại phụ tùng nhanh
 */

window.PartClassificationPresets = {
    // Định nghĩa các preset phổ biến
    presets: {
        "new_with_invoice": {
            label: "Phụ tùng mới có hóa đơn (Công ty/Bảo hiểm)",
            description: "Phụ tùng mua mới, có hóa đơn VAT, dùng được cho tất cả đối tượng",
            icon: "fa-certificate",
            values: {
                SourceType: "Purchased",
                Condition: "New",
                InvoiceType: "WithInvoice",
                HasInvoice: true,
                CanUseForIndividual: true,
                CanUseForCompany: true,
                CanUseForInsurance: true,
                WarrantyMonths: 12,
                SourceReference: ""
            }
        },
        "new_no_invoice": {
            label: "Phụ tùng mới không hóa đơn (Cá nhân)",
            description: "Phụ tùng mua mới, không hóa đơn VAT, chỉ dùng cho cá nhân",
            icon: "fa-box",
            values: {
                SourceType: "Purchased",
                Condition: "New",
                InvoiceType: "WithoutInvoice",
                HasInvoice: false,
                CanUseForIndividual: true,
                CanUseForCompany: false,
                CanUseForInsurance: false,
                WarrantyMonths: 6,
                SourceReference: ""
            }
        },
        "used_salvage": {
            label: "Phụ tùng tháo xe (Cá nhân only)",
            description: "Phụ tùng tháo từ xe cũ, không hóa đơn, chỉ dùng cho cá nhân",
            icon: "fa-recycle",
            values: {
                SourceType: "Used",
                Condition: "Used",
                InvoiceType: "WithoutInvoice",
                HasInvoice: false,
                CanUseForIndividual: true,
                CanUseForCompany: false,
                CanUseForInsurance: false,
                WarrantyMonths: 0,
                SourceReference: ""
            }
        },
        "oem_parts": {
            label: "Phụ tùng chính hãng OEM",
            description: "Phụ tùng chính hãng, có hóa đơn VAT, bảo hành dài hạn",
            icon: "fa-star",
            values: {
                SourceType: "Purchased",
                Condition: "New",
                InvoiceType: "WithInvoice",
                HasInvoice: true,
                IsOEM: true,
                CanUseForIndividual: true,
                CanUseForCompany: true,
                CanUseForInsurance: true,
                WarrantyMonths: 24,
                SourceReference: ""
            }
        },
        "refurbished": {
            label: "Phụ tùng tái chế/Tân trang",
            description: "Phụ tùng đã qua tái chế, có bảo hành giới hạn",
            icon: "fa-sync",
            values: {
                SourceType: "Refurbished",
                Condition: "Refurbished",
                InvoiceType: "WithoutInvoice",
                HasInvoice: false,
                CanUseForIndividual: true,
                CanUseForCompany: false,
                CanUseForInsurance: false,
                WarrantyMonths: 3,
                SourceReference: ""
            }
        }
    },

    /**
     * Áp dụng preset cho form
     */
    applyPreset: function(presetKey, formPrefix) {
        const preset = this.presets[presetKey];
        if (!preset) return;

        // Apply all values
        Object.keys(preset.values).forEach(key => {
            const element = $(`#${formPrefix}${key}`);
            const value = preset.values[key];

            if (element.is(':checkbox')) {
                element.prop('checked', value);
            } else if (element.is('select')) {
                element.val(value).trigger('change');
            } else {
                element.val(value);
            }
        });

        // Update summary
        this.updateSummary(formPrefix);

        // Show success message
        this.showPresetAppliedMessage(preset.label);
    },

    /**
     * Cập nhật summary hiển thị
     */
    updateSummary: function(formPrefix) {
        const hasInvoice = $(`#${formPrefix}HasInvoice`).is(':checked');
        const canCompany = $(`#${formPrefix}CanUseForCompany`).is(':checked');
        const canInsurance = $(`#${formPrefix}CanUseForInsurance`).is(':checked');
        const canIndividual = $(`#${formPrefix}CanUseForIndividual`).is(':checked');
        const warranty = $(`#${formPrefix}WarrantyMonths`).val() || 0;
        const isOEM = $(`#${formPrefix}IsOEM`).is(':checked');
        const sourceType = $(`#${formPrefix}SourceType`).val();

        let html = '<div class="classification-summary mt-3 p-3 border rounded bg-light">';
        html += '<div class="row">';
        
        // Hóa đơn status
        html += '<div class="col-md-12 mb-2">';
        if (hasInvoice) {
            html += '<span class="badge badge-success mr-2">';
            html += '<i class="fas fa-file-invoice"></i> Có hóa đơn VAT</span>';
        } else {
            html += '<span class="badge badge-warning mr-2">';
            html += '<i class="fas fa-exclamation-triangle"></i> Không có hóa đơn</span>';
        }
        html += '</div>';

        // Đối tượng sử dụng
        html += '<div class="col-md-12 mb-2">';
        html += '<strong>Dùng cho:</strong> ';
        const usageTypes = [];
        if (canIndividual) usageTypes.push('<span class="badge badge-info mr-1">Cá nhân</span>');
        if (canCompany) usageTypes.push('<span class="badge badge-primary mr-1">Công ty</span>');
        if (canInsurance) usageTypes.push('<span class="badge badge-primary mr-1">Bảo hiểm</span>');
        html += usageTypes.join(' ');
        html += '</div>';

        // Warranty & OEM
        html += '<div class="col-md-12">';
        if (warranty > 0) {
            html += '<span class="badge badge-info mr-2">';
            html += '<i class="fas fa-shield-alt"></i> Bảo hành: ' + warranty + ' tháng</span>';
        }
        if (isOEM) {
            html += '<span class="badge badge-success mr-2">';
            html += '<i class="fas fa-star"></i> Chính hãng OEM</span>';
        }
        html += '</div>';

        html += '</div></div>';

        $(`#${formPrefix}ClassificationSummary`).html(html);
    },

    /**
     * Hiển thị thông báo preset đã áp dụng
     */
    showPresetAppliedMessage: function(presetLabel) {
        const toast = $('<div class="toast-preset-applied">')
            .html(`<i class="fas fa-check-circle text-success"></i> Đã áp dụng: ${presetLabel}`)
            .css({
                position: 'fixed',
                top: '20px',
                right: '20px',
                backgroundColor: '#fff',
                padding: '15px 20px',
                borderRadius: '5px',
                boxShadow: '0 4px 6px rgba(0,0,0,0.1)',
                zIndex: 9999,
                border: '2px solid #28a745'
            });

        $('body').append(toast);
        toast.fadeIn(300).delay(2000).fadeOut(300, function() {
            $(this).remove();
        });
    },

    /**
     * Validate classification business rules
     */
    validateClassification: function(formPrefix) {
        const errors = [];
        const warnings = [];

        const hasInvoice = $(`#${formPrefix}HasInvoice`).is(':checked');
        const canCompany = $(`#${formPrefix}CanUseForCompany`).is(':checked');
        const canInsurance = $(`#${formPrefix}CanUseForInsurance`).is(':checked');
        const sourceType = $(`#${formPrefix}SourceType`).val();
        const warranty = parseInt($(`#${formPrefix}WarrantyMonths`).val() || 0);

        // Rule 1: Company/Insurance require invoice
        if ((canCompany || canInsurance) && !hasInvoice) {
            errors.push("Phụ tùng cho công ty/bảo hiểm phải có hóa đơn VAT");
            
            // Auto-correct
            $(`#${formPrefix}HasInvoice`).prop('checked', true);
            $(`#${formPrefix}InvoiceType`).val('WithInvoice');
            
            warnings.push("Đã tự động bật 'Có hóa đơn' vì phụ tùng dùng cho công ty/bảo hiểm");
        }

        // Rule 2: Used parts warning
        if (sourceType === "Used" && warranty > 0) {
            warnings.push("Phụ tùng tháo xe thường không có bảo hành. Vui lòng kiểm tra lại.");
        }

        // Rule 3: OEM should have invoice
        const isOEM = $(`#${formPrefix}IsOEM`).is(':checked');
        if (isOEM && !hasInvoice) {
            warnings.push("Phụ tùng chính hãng OEM thường có hóa đơn VAT. Vui lòng kiểm tra lại.");
        }

        return { errors, warnings };
    }
};

