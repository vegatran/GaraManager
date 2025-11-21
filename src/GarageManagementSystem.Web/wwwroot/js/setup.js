;(function ($) {
    'use strict';

    function initSetupPage() {
        if (typeof window.setupConfig === 'undefined') {
            console.error('setupConfig is not defined');
            return;
        }

        if (typeof Swal === 'undefined') {
            console.error('SweetAlert2 not loaded');
            return;
        }

        loadSetupModules();
        loadDataStatus();

        bindCommonEvents();
        bindPhaseButtons();
        bindPhaseClearButtons();
    }

    function bindCommonEvents() {
        $('#refreshStatus').on('click', function () {
            loadDataStatus();
        });

        $('#setupAll').on('click', function () {
            setupAllModules();
        });

        $('#clearAll').on('click', function () {
            clearAllData();
        });
    }

    function bindPhaseButtons() {
        $('#btnPhase1').on('click', function () {
            createPhaseData(1);
        });
        $('#btnPhase2').on('click', function () {
            createPhaseData(2);
        });
        $('#btnPhase3').on('click', function () {
            createPhaseData(3);
        });
        $('#btnPhase4').on('click', function () {
            createPhaseData(4);
        });
    }

    function bindPhaseClearButtons() {
        $('#btnClearPhase1').on('click', function () {
            clearPhaseData(1);
        });
        $('#btnClearPhase2').on('click', function () {
            clearPhaseData(2);
        });
        $('#btnClearPhase3').on('click', function () {
            clearPhaseData(3);
        });
    }

    function loadSetupModules() {
        $.ajax({
            url: window.setupConfig.getModulesUrl,
            type: 'GET',
            success: function (response) {
                if (response && response.length > 0) {
                    displaySetupModules(response);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error loading setup modules:', error);
                $('#setupModules').html('<div class="col-12"><div class="alert alert-danger">Lỗi khi tải danh sách modules</div></div>');
            }
        });
    }

    function displaySetupModules(modules) {
        var html = '';
        var currentPhase = '';

        modules.forEach(function (module) {
            if (module.phase !== currentPhase) {
                if (currentPhase !== '') {
                    html += '</div></div>';
                }
                html += '<div class="col-12 mb-4">';
                html += '<h5 class="text-primary"><i class="fas fa-layer-group"></i> ' + module.phase + '</h5>';
                html += '<p class="text-muted small">' + module.phaseDescription + '</p>';
                html += '<div class="row">';
                currentPhase = module.phase;
            }

            html += '<div class="col-md-4 col-lg-3 mb-3">';
            html += '<div class="card setup-module" data-module="' + module.id + '">';
            html += '<div class="card-body text-center">';
            html += '<i class="' + module.icon + ' fa-2x text-primary mb-2"></i>';
            html += '<h6 class="card-title">' + module.name + '</h6>';
            html += '<p class="card-text small text-muted">' + module.description + '</p>';
            html += '<button class="btn btn-sm btn-outline-primary setup-module-btn" data-module="' + module.id + '">';
            html += '<i class="fas fa-play"></i> Tạo dữ liệu';
            html += '</button></div></div></div>';
        });

        if (currentPhase !== '') {
            html += '</div></div>';
        }

        $('#setupModules').html(html);
        $('.setup-module-btn').on('click', function () {
            setupModule($(this).data('module'));
        });
    }

    function loadDataStatus() {
        $.ajax({
            url: window.setupConfig.statusUrl,
            type: 'GET',
            success: function (response) {
                if (response.success && response.data) {
                    displayDataStatus(response.data);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error loading data status:', error);
                $('#statusCards').html('<div class="col-12"><div class="alert alert-danger">Lỗi khi tải trạng thái dữ liệu</div></div>');
            }
        });
    }

    function displayDataStatus(status) {
        var html = '';
        var statusItems = [
            { key: 'customers', label: 'Khách hàng', icon: 'fas fa-users' },
            { key: 'vehicles', label: 'Xe', icon: 'fas fa-car' },
            { key: 'employees', label: 'Nhân viên', icon: 'fas fa-user-tie' },
            { key: 'services', label: 'Dịch vụ', icon: 'fas fa-tools' },
            { key: 'parts', label: 'Phụ tùng', icon: 'fas fa-cogs' },
            { key: 'suppliers', label: 'Nhà cung cấp', icon: 'fas fa-truck' },
            { key: 'inspections', label: 'Kiểm tra xe', icon: 'fas fa-search' },
            { key: 'quotations', label: 'Báo giá', icon: 'fas fa-file-invoice-dollar' },
            { key: 'orders', label: 'Đơn hàng', icon: 'fas fa-clipboard-list' },
            { key: 'payments', label: 'Thanh toán', icon: 'fas fa-credit-card' },
            { key: 'appointments', label: 'Lịch hẹn', icon: 'fas fa-calendar-alt' }
        ];

        statusItems.forEach(function (item) {
            var count = status[item.key] || 0;
            html += '<div class="col-md-3 col-sm-6 mb-3">';
            html += '<div class="status-card">';
            html += '<div class="status-number">' + count + '</div>';
            html += '<div class="status-label"><i class="' + item.icon + '"></i> ' + item.label + '</div>';
            html += '</div></div>';
        });

        $('#statusCards').html(html);
    }

    function setupModule(moduleId) {
        $.ajax({
            url: window.setupConfig.createModuleUrl + '?moduleId=' + moduleId,
            type: 'POST',
            beforeSend: function () {
                $('.setup-module[data-module="' + moduleId + '"]').addClass('setup-progress');
                $('.setup-module-btn[data-module="' + moduleId + '"]').prop('disabled', true);
            },
            success: function (response) {
                if (response.success) {
                    $('.setup-module[data-module="' + moduleId + '"]').removeClass('setup-progress').addClass('setup-success');
                    Swal.fire({ icon: 'success', title: 'Thành công!', text: 'Tạo dữ liệu ' + moduleId + ' thành công: ' + response.message, timer: 3000, showConfirmButton: false });
                    loadDataStatus();
                } else {
                    $('.setup-module[data-module="' + moduleId + '"]').removeClass('setup-progress').addClass('setup-error');
                    Swal.fire({ icon: 'error', title: 'Lỗi!', text: 'Lỗi tạo dữ liệu ' + moduleId + ': ' + response.message });
                }
            },
            error: function () {
                $('.setup-module[data-module="' + moduleId + '"]').removeClass('setup-progress').addClass('setup-error');
                Swal.fire({ icon: 'error', title: 'Lỗi!', text: 'Lỗi tạo dữ liệu ' + moduleId });
            },
            complete: function () {
                $('.setup-module-btn[data-module="' + moduleId + '"]').prop('disabled', false);
            }
        });
    }

    function setupAllModules() {
        if (!confirm('Bạn có chắc muốn tạo tất cả dữ liệu demo?')) {
            return;
        }

        $('#progressModal').modal('show');
        var modules = ['customers', 'employees', 'suppliers', 'vehicles', 'parts', 'services', 'inspections', 'quotations', 'orders', 'payments', 'appointments'];
        var currentIndex = 0;

        function setupNextModule() {
            if (currentIndex >= modules.length) {
                $('#progressModal').modal('hide');
                Swal.fire({ icon: 'success', title: 'Hoàn thành!', text: 'Đã tạo tất cả dữ liệu demo thành công!', timer: 3000, showConfirmButton: false });
                loadDataStatus();
                loadSetupModules();
                return;
            }

            var moduleId = modules[currentIndex];
            var progress = Math.round((currentIndex / modules.length) * 100);
            $('#progressBar').css('width', progress + '%').text(progress + '%');
            $('#progressText').text('Đang tạo dữ liệu: ' + moduleId);

            $.ajax({
                url: window.setupConfig.createModuleUrl + '?moduleId=' + moduleId,
                type: 'POST',
                success: function (response) {
                    var statusClass = response.success ? 'success' : 'error';
                    var statusText = response.success ? 'Thành công' : 'Lỗi: ' + response.message;
                    $('#progressDetails').append('<div class="progress-step ' + statusClass + '">' + moduleId + ': ' + statusText + '</div>');
                    currentIndex++;
                    setTimeout(setupNextModule, 1000);
                },
                error: function () {
                    $('#progressDetails').append('<div class="progress-step error">' + moduleId + ': Lỗi kết nối</div>');
                    currentIndex++;
                    setTimeout(setupNextModule, 1000);
                }
            });
        }

        setupNextModule();
    }

    function createPhaseData(phase) {
        var phaseNames = {
            1: 'Giai đoạn 1: Tiếp nhận & Báo giá',
            2: 'Giai đoạn 2: Sửa chữa & Quản lý xuất kho',
            3: 'Giai đoạn 3: Quyết toán & Chăm sóc hậu mãi',
            4: 'Giai đoạn 4: Chuẩn hóa quản lý phụ tùng & Procurement'
        };

        Swal.fire({
            title: 'Xác nhận',
            text: 'Bạn có chắc muốn tạo demo data cho ' + phaseNames[phase] + '?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Có, tạo ngay!',
            cancelButtonText: 'Hủy'
        }).then(function (result) {
            if (!result.isConfirmed) {
                return;
            }

            var btnId = '#btnPhase' + phase;
            var $btn = $(btnId);
            var originalHtml = $btn.html();
            $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang tạo...');

            $.ajax({
                url: window.setupConfig.createPhaseUrls[phase],
                type: 'POST',
                success: function (response) {
                    handlePhaseResponse(response, $btn, originalHtml, 'tạo');
                },
                error: function (xhr) {
                    handleAjaxError(xhr);
                },
                complete: function () {
                    $btn.prop('disabled', false).html(originalHtml);
                }
            });
        });
    }

    function clearPhaseData(phase) {
        var phaseNames = {
            1: 'Giai đoạn 1',
            2: 'Giai đoạn 2',
            3: 'Giai đoạn 3'
        };

        Swal.fire({
            title: 'Xác nhận',
            text: 'Bạn có chắc muốn xóa toàn bộ dữ liệu demo cho ' + phaseNames[phase] + '?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Có, xóa ngay!',
            cancelButtonText: 'Hủy'
        }).then(function (result) {
            if (!result.isConfirmed) {
                return;
            }

            $.ajax({
                url: window.setupConfig.clearPhaseUrls[phase],
                type: 'POST',
                success: function (response) {
                    if (response.success) {
                        Swal.fire({ icon: 'success', title: 'Đã xóa', text: response.message || 'Đã xóa dữ liệu thành công', timer: 2500, showConfirmButton: false });
                        loadDataStatus();
                        loadSetupModules();
                    } else {
                        Swal.fire({ icon: 'error', title: 'Lỗi', text: response.message || 'Không thể xóa dữ liệu' });
                    }
                },
                error: function (xhr) {
                    handleAjaxError(xhr);
                }
            });
        });
    }

    function clearAllData() {
        Swal.fire({
            title: 'Xác nhận xóa',
            text: 'Bạn có chắc muốn XÓA TẤT CẢ demo data? Hành động này không thể hoàn tác!',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Có, xóa ngay!',
            cancelButtonText: 'Hủy'
        }).then(function (result) {
            if (!result.isConfirmed) {
                return;
            }

            var $btn = $('#clearAll');
            var originalHtml = $btn.html();
            $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang xóa...');

            $.ajax({
                url: window.setupConfig.clearAllUrl,
                type: 'POST',
                success: function (response) {
                    if (response.success) {
                        Swal.fire({ icon: 'success', title: 'Thành công!', text: 'Đã xóa tất cả demo data thành công!', timer: 3000, showConfirmButton: false });
                        loadDataStatus();
                        loadSetupModules();
                    } else {
                        Swal.fire({ icon: 'error', title: 'Lỗi!', text: 'Lỗi xóa dữ liệu: ' + (response.message || 'Không xác định') });
                    }
                },
                error: function (xhr) {
                    handleAjaxError(xhr);
                },
                complete: function () {
                    $btn.prop('disabled', false).html(originalHtml);
                }
            });
        });
    }

    function handlePhaseResponse(response, $btn, originalHtml, action) {
        if (response.success) {
            Swal.fire({ icon: 'success', title: 'Thành công!', html: '<p>' + (response.message || 'Đã ' + action + ' demo data thành công') + '</p>', timer: 3000, showConfirmButton: false });
            loadDataStatus();
        } else {
            var errorMsg = extractErrorMessage(response);
            Swal.fire({ icon: 'error', title: 'Lỗi!', text: errorMsg });
        }
    }

    function handleAjaxError(xhr) {
        var errorMsg = 'Lỗi kết nối đến server';
        if (xhr.responseJSON && xhr.responseJSON.message) {
            errorMsg = xhr.responseJSON.message;
        } else if (xhr.responseText) {
            errorMsg = 'Lỗi: ' + xhr.responseText.substring(0, 200);
        }
        Swal.fire({ icon: 'error', title: 'Lỗi!', text: errorMsg });
    }

    function extractErrorMessage(response) {
        if (response.message) {
            return response.message;
        }
        if (response.data && response.data.message) {
            return response.data.message;
        }
        if (response.errors && Array.isArray(response.errors)) {
            return response.errors.join(', ');
        }
        return 'Có lỗi xảy ra';
    }

    $(document).ready(initSetupPage);
})(jQuery);
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