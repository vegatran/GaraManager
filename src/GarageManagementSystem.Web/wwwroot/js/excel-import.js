/**
 * Excel Import Utility for Stock Management
 * Tiện ích import Excel cho quản lý kho
 */

window.ExcelImport = {
    init: function() {
        this.bindEvents();
    },

    bindEvents: function() {
        var self = this;

        // Download template
        $('#downloadTemplateBtn').on('click', function() {
            self.downloadTemplate();
        });

        // File input change
        $('#excelFile').on('change', function() {
            var fileName = $(this).val().split('\\').pop();
            $(this).next('.custom-file-label').text(fileName || 'Chọn file Excel...');
        });

        // Validate file
        $('#validateFileBtn').on('click', function() {
            self.validateFile();
        });

        // Import form submit
        $('#importForm').on('submit', function(e) {
            e.preventDefault();
            self.importData();
        });

        // Modal events
        $('#importModal').on('hidden.bs.modal', function() {
            self.resetForm();
        });
    },

    downloadTemplate: function() {
        $.ajax({
            url: '/StockManagement/DownloadTemplate',
            type: 'GET',
            xhrFields: {
                responseType: 'blob'
            },
            success: function(data) {
                var blob = new Blob([data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                var url = window.URL.createObjectURL(blob);
                var a = document.createElement('a');
                a.href = url;
                a.download = `Stock_Import_Template_${new Date().toISOString().slice(0,10).replace(/-/g,'')}.xlsx`;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                window.URL.revokeObjectURL(url);
                
                GarageApp.showSuccess('Template đã được tải xuống');
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi tải template: ' + error);
                }
            }
        });
    },

    validateFile: function() {
        var fileInput = document.getElementById('excelFile');
        var file = fileInput.files[0];

        if (!file) {
            GarageApp.showWarning('Vui lòng chọn file Excel để validate');
            return;
        }

        if (!this.isValidExcelFile(file)) {
            GarageApp.showError('Chỉ hỗ trợ file Excel (.xlsx, .xls)');
            return;
        }

        var formData = new FormData();
        formData.append('file', file);

        // Show loading
        $('#validateFileBtn').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang validate...');

        $.ajax({
            url: '/StockManagement/ValidateExcel',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                ExcelImport.displayValidationResults(response.data);
                $('#validateFileBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Validate');
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi validate file: ' + error);
                }
                $('#validateFileBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Validate');
            }
        });
    },

    importData: function() {
        var fileInput = document.getElementById('excelFile');
        var file = fileInput.files[0];

        if (!file) {
            GarageApp.showWarning('Vui lòng chọn file Excel để import');
            return;
        }

        if (!this.isValidExcelFile(file)) {
            GarageApp.showError('Chỉ hỗ trợ file Excel (.xlsx, .xls)');
            return;
        }

        // Confirm import
        GarageApp.showConfirm(
            'Xác nhận Import',
            'Bạn có chắc chắn muốn import dữ liệu từ file Excel này không?',
            function() {
                ExcelImport.performImport(file);
            }
        );
    },

    performImport: function(file) {
        var formData = new FormData();
        formData.append('file', file);

        // Show progress
        $('#importProgress').show();
        $('#importForm button[type="submit"]').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang import...');

        $.ajax({
            url: '/StockManagement/ImportExcel',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            xhr: function() {
                var xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener("progress", function(evt) {
                    if (evt.lengthComputable) {
                        var percentComplete = evt.loaded / evt.total * 100;
                        $('#importProgress .progress-bar').css('width', percentComplete + '%');
                    }
                }, false);
                return xhr;
            },
            success: function(response) {
                ExcelImport.handleImportSuccess(response);
            },
            error: function(xhr, status, error) {
                ExcelImport.handleImportError(xhr, status, error);
            }
        });
    },

    handleImportSuccess: function(response) {
        $('#importProgress').hide();
        $('#importForm button[type="submit"]').prop('disabled', false).html('Import Dữ Liệu');

        if (response.data && response.data.success) {
            var result = response.data;
            var message = `Import thành công!\n\n` +
                         `📊 Tổng số dòng: ${result.totalRows}\n` +
                         `✅ Thành công: ${result.successCount}\n` +
                         `❌ Lỗi: ${result.errorCount}`;

            if (result.errorCount > 0 && result.errors && result.errors.length > 0) {
                message += `\n\n📋 Chi tiết lỗi:\n`;
                result.errors.slice(0, 5).forEach(function(error) {
                    message += `• Dòng ${error.rowNumber}: ${error.errorMessage}\n`;
                });
                if (result.errors.length > 5) {
                    message += `... và ${result.errors.length - 5} lỗi khác`;
                }
            }

            GarageApp.showSuccess(message);

            // Refresh table if successful
            if (result.successCount > 0) {
                setTimeout(function() {
                    $('#importModal').modal('hide');
                    if (window.StockManagement && window.StockManagement.refreshTable) {
                        window.StockManagement.refreshTable();
                    }
                }, 2000);
            }
        } else {
            GarageApp.showError(response.message || 'Import thất bại');
        }
    },

    handleImportError: function(xhr, status, error) {
        $('#importProgress').hide();
        $('#importForm button[type="submit"]').prop('disabled', false).html('Import Dữ Liệu');

        if (AuthHandler.isUnauthorized(xhr)) {
            AuthHandler.handleUnauthorized(xhr, true);
        } else {
            var errorMessage = 'Lỗi import: ' + error;
            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            }
            GarageApp.showError(errorMessage);
        }
    },

    displayValidationResults: function(result) {
        var content = '';
        
        if (result.success) {
            content = `
                <div class="alert alert-success">
                    <h6><i class="fas fa-check-circle"></i> Validation Thành Công</h6>
                    <p><strong>${result.totalRows}</strong> dòng dữ liệu hợp lệ, sẵn sàng import.</p>
                </div>
            `;
        } else {
            content = `
                <div class="alert alert-danger">
                    <h6><i class="fas fa-exclamation-triangle"></i> Validation Có Lỗi</h6>
                    <p><strong>${result.errorCount}</strong> lỗi trong <strong>${result.totalRows}</strong> dòng dữ liệu.</p>
            `;
            
            if (result.errors && result.errors.length > 0) {
                content += '<ul class="mb-0 mt-2">';
                result.errors.slice(0, 10).forEach(function(error) {
                    content += `<li><strong>Dòng ${error.rowNumber}</strong>: ${error.errorMessage}</li>`;
                });
                if (result.errors.length > 10) {
                    content += `<li>... và ${result.errors.length - 10} lỗi khác</li>`;
                }
                content += '</ul>';
            }
            
            content += '</div>';
        }

        $('#validationContent').html(content);
        $('#validationResults').show();
    },

    isValidExcelFile: function(file) {
        var validTypes = [
            'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', // .xlsx
            'application/vnd.ms-excel' // .xls
        ];
        
        var validExtensions = ['.xlsx', '.xls'];
        var fileName = file.name.toLowerCase();
        
        return validTypes.includes(file.type) || 
               validExtensions.some(ext => fileName.endsWith(ext));
    },

    resetForm: function() {
        $('#excelFile').val('');
        $('#excelFile').next('.custom-file-label').text('Chọn file Excel...');
        $('#validationResults').hide();
        $('#importProgress').hide();
        $('#importForm button[type="submit"]').prop('disabled', false).html('Import Dữ Liệu');
        $('#validateFileBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Validate');
    }
};

// Initialize when document is ready
$(document).ready(function() {
    ExcelImport.init();
});
