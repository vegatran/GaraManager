/**
 * ✅ 2.4: Quality Control Management Module
 * 
 * Handles all QC-related operations:
 * - List JO waiting for QC
 * - Start QC inspection
 * - Complete QC (Pass/Fail)
 * - View QC information
 * - Handover vehicle
 */

window.GarageApp = window.GarageApp || {};
GarageApp.QC = {
    table: null,
    checklistItemCounter: 0,

    init: function() {
        this.initTable();
        this.bindEvents();
    },

    initTable: function() {
        var self = this;
        self.table = $('#qcTable').DataTable({
            processing: true,
            serverSide: true,
            searching: false,
            pageLength: 10,
            lengthMenu: [10, 25, 50],
            language: GarageApp.dataTableVi || {},
            responsive: true,
            order: [[3, 'desc']], // Sort by completed date desc
            rowId: 'id',
            dom: "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6 text-right'>>" +
                 "<'row'<'col-sm-12'tr>>" +
                 "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
            ajax: function(data, callback) {
                var pageNumber = Math.floor(data.start / data.length) + 1;
                var pageSize = data.length;
                $.ajax({
                    url: '/QCManagement/GetWaitingForQC',
                    type: 'GET',
                    data: { pageNumber: pageNumber, pageSize: pageSize },
                    success: function(res) {
                        if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                            return callback({ data: [], recordsTotal: 0, recordsFiltered: 0 });
                        }
                        if (res && res.success) {
                            callback({
                                data: res.data,
                                recordsTotal: res.totalCount,
                                recordsFiltered: res.totalCount
                            });
                        } else {
                            GarageApp.showError(GarageApp.parseErrorMessage(res) || 'Lỗi khi tải danh sách JO chờ QC');
                            callback({ data: [], recordsTotal: 0, recordsFiltered: 0 });
                        }
                    },
                    error: function(xhr) {
                        if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi tải danh sách JO chờ QC');
                        }
                        callback({ data: [], recordsTotal: 0, recordsFiltered: 0 });
                    }
                });
            },
            columns: [
                { data: 'orderNumber', title: 'Số Đơn Hàng' },
                { data: 'customerName', title: 'Khách Hàng' },
                { data: 'vehiclePlate', title: 'Xe' },
                { data: 'completedDate', title: 'Ngày Hoàn Thành' },
                { data: 'totalActualHours', title: 'Giờ Công Thực Tế' },
                { data: 'qcFailedCount', title: 'Số Lần QC Không Đạt' },
                { 
                    data: null, 
                    orderable: false,
                    searchable: false,
                    render: function(data, type, row) {
                        var actions = '<div class="btn-group">';
                        actions += '<button class="btn btn-sm btn-primary qc-start-btn" data-id="' + row.id + '" title="Bắt đầu QC">';
                        actions += '<i class="fas fa-play"></i>';
                        actions += '</button>';
                        actions += '<button class="btn btn-sm btn-info qc-view-btn" data-id="' + row.id + '" title="Xem QC">';
                        actions += '<i class="fas fa-eye"></i>';
                        actions += '</button>';
                        actions += '</div>';
                        return actions;
                    }
                }
            ]
        });
    },

    bindEvents: function() {
        var self = this;

        // Start QC button
        $(document).on('click', '.qc-start-btn', function() {
            var orderId = $(this).data('id');
            self.showStartQCModal(orderId);
        });

        // View QC button
        $(document).on('click', '.qc-view-btn', function() {
            var orderId = $(this).data('id');
            self.viewQC(orderId);
        });

        // Add checklist item
        $(document).on('click', '#btnAddChecklistItem', function() {
            self.addChecklistItem();
        });

        // Remove checklist item
        $(document).on('click', '.btn-remove-checklist-item', function() {
            $(this).closest('.checklist-item-row').remove();
        });

        // Submit Start QC
        $(document).on('click', '#btnSubmitStartQC', function() {
            self.submitStartQC();
        });

        // Complete QC modal events
        $(document).on('click', '.qc-complete-btn', function() {
            var orderId = $(this).data('id');
            self.showCompleteQCModal(orderId);
        });

        // Complete QC checkbox toggle
        $(document).on('change', '#completeQCReworkRequired', function() {
            if ($(this).is(':checked')) {
                $('#reworkNotesGroup').show();
            } else {
                $('#reworkNotesGroup').hide();
                $('#completeQCReworkNotes').val('');
            }
        });

        // QC Result radio buttons
        $(document).on('change', 'input[name="qcResult"]', function() {
            var result = $(this).val();
            if (result === 'Fail') {
                $('#completeQCReworkRequired').prop('checked', true);
                $('#reworkNotesGroup').show();
            }
        });

        // Add checklist item in Complete QC modal
        $(document).on('click', '#btnAddCompleteChecklistItem', function() {
            self.addCompleteChecklistItem();
        });

        // Submit Complete QC
        $(document).on('click', '#btnSubmitCompleteQC', function() {
            self.submitCompleteQC();
        });

        // Handover button
        $(document).on('click', '.qc-handover-btn', function() {
            var orderId = $(this).data('id');
            self.showHandoverModal(orderId);
        });

        // Submit Handover
        $(document).on('click', '#btnSubmitHandover', function() {
            self.submitHandover();
        });

        // Modal reset events
        $('#startQCModal').on('hidden.bs.modal', function() {
            self.resetStartQCModal();
        });

        $('#completeQCModal').on('hidden.bs.modal', function() {
            self.resetCompleteQCModal();
        });

        $('#handoverModal').on('hidden.bs.modal', function() {
            self.resetHandoverModal();
        });
    },

    showStartQCModal: function(orderId) {
        var self = this;
        
        // Load order details
        $.ajax({
            url: '/OrderManagement/GetOrder/' + orderId,
            type: 'GET',
            success: function(res) {
                if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                    return;
                }
                
                if (res && res.success && res.data) {
                    var order = res.data;
                    
                    // Populate modal
                    $('#startQCServiceOrderId').val(orderId);
                    $('#startQCOrderNumber').val(order.orderNumber || '');
                    $('#startQCCustomerName').val(order.customer?.name || '');
                    $('#startQCVehiclePlate').val(order.vehicle?.licensePlate || '');
                    
                    // Load total actual hours
                    self.loadTotalActualHours(orderId, function(hours) {
                        $('#startQCTotalActualHours').val(hours ? hours.toFixed(2) + ' giờ' : '0.00 giờ');
                    });
                    
                    // Reset checklist
                    $('#qcChecklistItems').empty();
                    self.checklistItemCounter = 0;
                    
                    // ✅ OPTIMIZED: Load template từ API với fallback về hardcode
                    // ✅ FIX: Đảm bảo vehicleType và serviceType không null/undefined
                    var vehicleType = (order.vehicle && order.vehicle.vehicleType) ? order.vehicle.vehicleType : null;
                    var serviceType = order.serviceType || null;
                    
                    // ✅ FIX: Đảm bảo callback chỉ được gọi 1 lần và modal chỉ show 1 lần
                    var callbackCalled = false;
                    self.loadQCTemplate(vehicleType, serviceType, function(templateItems) {
                        if (callbackCalled) {
                            console.warn('QC template callback đã được gọi, bỏ qua lần gọi thứ 2');
                            return;
                        }
                        callbackCalled = true;
                        
                        if (templateItems && templateItems.length > 0) {
                            // Load từ template
                            templateItems.forEach(function(item) {
                                if (item && item.checklistItemName) {
                                    self.addChecklistItem(item.checklistItemName);
                                }
                            });
                        } else {
                            // Fallback: Sử dụng hardcode mặc định
                            self.addChecklistItem('Kiểm tra chất lượng sơn');
                            self.addChecklistItem('Kiểm tra lắp ráp phụ tùng');
                            self.addChecklistItem('Kiểm tra hoạt động động cơ');
                            self.addChecklistItem('Kiểm tra hệ thống điện');
                            self.addChecklistItem('Kiểm tra an toàn');
                        }
                        
                        // ✅ FIX: Chỉ show modal nếu chưa được show
                        if (!$('#startQCModal').is(':visible')) {
                            $('#startQCModal').modal('show');
                        }
                    });
                } else {
                    GarageApp.showError('Không thể tải thông tin đơn hàng');
                }
            },
            error: function(xhr) {
                if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin đơn hàng');
                }
            }
        });
    },

    addChecklistItem: function(itemName) {
        var self = this;
        var template = $('#checklistItemTemplate').html();
        var $item = $(template);
        
        self.checklistItemCounter++;
        $item.attr('data-display-order', self.checklistItemCounter);
        
        if (itemName) {
            $item.find('.checklist-item-name').val(itemName);
        }
        
        $('#qcChecklistItems').append($item);
    },

    // ✅ OPTIMIZED: Load QC Template từ API với fallback
    loadQCTemplate: function(vehicleType, serviceType, callback) {
        var self = this;
        var url = '/QCManagement/GetQCTemplate';
        var params = [];
        
        // ✅ FIX: Validate và sanitize input
        if (vehicleType && typeof vehicleType === 'string' && vehicleType.trim() !== '') {
            params.push('vehicleType=' + encodeURIComponent(vehicleType.trim()));
        }
        if (serviceType && typeof serviceType === 'string' && serviceType.trim() !== '') {
            params.push('serviceType=' + encodeURIComponent(serviceType.trim()));
        }
        
        if (params.length > 0) {
            url += '?' + params.join('&');
        }
        
        $.ajax({
            url: url,
            type: 'GET',
            success: function(res) {
                if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                    // Fallback về hardcode nếu có lỗi auth
                    if (callback) callback([]);
                    return;
                }
                
                // ✅ FIX: Kiểm tra kỹ hơn về structure của response
                if (res && res.success && res.data) {
                    // ✅ FIX: Kiểm tra res.data có phải là object và có templateItems không
                    var templateData = res.data.data || res.data.Data || res.data;
                    var templateItems = (templateData && templateData.templateItems) ? templateData.templateItems : 
                                       (templateData && templateData.TemplateItems) ? templateData.TemplateItems :
                                       (res.data.templateItems) ? res.data.templateItems :
                                       (res.data.TemplateItems) ? res.data.TemplateItems : [];
                    
                    if (Array.isArray(templateItems) && templateItems.length > 0) {
                        // Load từ template thành công
                        var items = templateItems.map(function(item) {
                            return {
                                checklistItemName: (item.checklistItemName || item.ChecklistItemName || '').trim(),
                                displayOrder: parseInt(item.displayOrder || item.DisplayOrder || 0, 10),
                                isRequired: item.isRequired || item.IsRequired || false
                            };
                        }).filter(function(item) {
                            // ✅ FIX: Lọc bỏ items không có tên
                            return item.checklistItemName && item.checklistItemName.length > 0;
                        }).sort(function(a, b) {
                            return a.displayOrder - b.displayOrder;
                        });
                        
                        if (items.length > 0 && callback) {
                            callback(items);
                            return;
                        }
                    }
                }
                
                // Không có template hoặc template rỗng, fallback về hardcode
                if (callback) callback([]);
            },
            error: function(xhr) {
                // Lỗi khi load template, fallback về hardcode
                console.warn('Error loading QC template, using hardcoded default', xhr);
                if (callback) callback([]);
            }
        });
    },

    addCompleteChecklistItem: function(itemName) {
        var self = this;
        var template = $('#checklistItemTemplate').html();
        var $item = $(template);
        
        self.checklistItemCounter++;
        $item.attr('data-display-order', self.checklistItemCounter);
        
        if (itemName) {
            $item.find('.checklist-item-name').val(itemName);
        }
        
        $('#completeQCChecklistItems').append($item);
    },

    loadTotalActualHours: function(orderId, callback) {
        $.ajax({
            url: '/QCManagement/GetTotalActualHours/' + orderId,
            type: 'GET',
            success: function(res) {
                if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                    if (callback) callback(0);
                    return;
                }
                
                if (res && res.success && res.data !== undefined) {
                    if (callback) callback(res.data);
                } else {
                    if (callback) callback(0);
                }
            },
            error: function(xhr) {
                if (callback) callback(0);
            }
        });
    },

    submitStartQC: function() {
        var self = this;
        var orderId = $('#startQCServiceOrderId').val();
        
        if (!orderId) {
            GarageApp.showError('Không tìm thấy đơn hàng');
            return;
        }
        
        // Collect checklist items
        var checklistItems = [];
        $('#qcChecklistItems .checklist-item-row').each(function(index) {
            var $row = $(this);
            var item = {
                checklistItemName: $row.find('.checklist-item-name').val() || '',
                isChecked: $row.find('.checklist-item-checkbox').is(':checked'),
                result: $row.find('.checklist-item-result').val() || null,
                notes: $row.find('.checklist-item-notes').val() || null,
                displayOrder: index
            };
            
            if (item.checklistItemName.trim() !== '') {
                checklistItems.push(item);
            }
        });
        
        var data = {
            checklistItems: checklistItems
        };
        
        $.ajax({
            url: '/QCManagement/StartQC/' + orderId,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function(res) {
                if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                    return;
                }
                
                if (res && res.success) {
                    GarageApp.showSuccess(res.message || 'Đã bắt đầu kiểm tra QC');
                    $('#startQCModal').modal('hide');
                    self.table.ajax.reload();
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(res) || 'Lỗi khi bắt đầu kiểm tra QC');
                }
            },
            error: function(xhr) {
                if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi bắt đầu kiểm tra QC');
                }
            }
        });
    },

    viewQC: function(orderId) {
        var self = this;
        
        $.ajax({
            url: '/QCManagement/GetQC/' + orderId,
            type: 'GET',
            success: function(res) {
                if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                    return;
                }
                
                if (res && res.success && res.data) {
                    var qc = res.data;
                    
                    // Populate view modal
                    $('#viewQCOrderNumber').text(qc.orderNumber || '');
                    $('#viewQCInspectorName').text(qc.qcInspectorName || 'Chưa xác định');
                    $('#viewQCDate').text(qc.qcDate ? new Date(qc.qcDate).toLocaleString('vi-VN') : '');
                    $('#viewQCCompletedDate').text(qc.qcCompletedDate ? new Date(qc.qcCompletedDate).toLocaleString('vi-VN') : 'Chưa hoàn thành');
                    
                    // QC Result
                    var resultBadge = '';
                    if (qc.qcResult === 'Pass') {
                        resultBadge = '<span class="badge badge-success">Đạt</span>';
                    } else if (qc.qcResult === 'Fail') {
                        resultBadge = '<span class="badge badge-danger">Không Đạt</span>';
                    } else {
                        resultBadge = '<span class="badge badge-warning">Chờ Xử Lý</span>';
                    }
                    $('#viewQCResult').html(resultBadge);
                    
                    $('#viewQCReworkRequired').text(qc.reworkRequired ? 'Có' : 'Không');
                    $('#viewQCNotes').text(qc.qcNotes || 'Không có ghi chú');
                    
                    if (qc.reworkNotes) {
                        $('#viewQCReworkNotes').text(qc.reworkNotes);
                        $('#reworkNotesCard').show();
                    } else {
                        $('#reworkNotesCard').hide();
                    }
                    
                    // Checklist items
                    var $tbody = $('#viewQCChecklistItems');
                    $tbody.empty();
                    
                    if (qc.qcChecklistItems && qc.qcChecklistItems.length > 0) {
                        qc.qcChecklistItems.forEach(function(item, index) {
                            var resultBadge = '';
                            if (item.result === 'Pass') {
                                resultBadge = '<span class="badge badge-success badge-sm">Đạt</span>';
                            } else if (item.result === 'Fail') {
                                resultBadge = '<span class="badge badge-danger badge-sm">Không Đạt</span>';
                            } else {
                                resultBadge = '<span class="badge badge-secondary badge-sm">-</span>';
                            }
                            
                            var row = '<tr>';
                            row += '<td>' + (index + 1) + '</td>';
                            row += '<td>' + (item.isChecked ? '<i class="fas fa-check text-success"></i> ' : '') + item.checklistItemName + '</td>';
                            row += '<td>' + resultBadge + '</td>';
                            row += '<td>' + (item.notes || '-') + '</td>';
                            row += '</tr>';
                            $tbody.append(row);
                        });
                    } else {
                        $tbody.append('<tr><td colspan="4" class="text-center text-muted">Không có checklist items</td></tr>');
                    }
                    
                    $('#viewQCModal').modal('show');
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(res) || 'Không tìm thấy thông tin QC');
                }
            },
            error: function(xhr) {
                if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin QC');
                }
            }
        });
    },

    showCompleteQCModal: function(orderId) {
        var self = this;
        
        // Load QC info first
        $.ajax({
            url: '/QCManagement/GetQC/' + orderId,
            type: 'GET',
            success: function(res) {
                if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                    return;
                }
                
                if (res && res.success && res.data) {
                    var qc = res.data;
                    
                    // Populate modal
                    $('#completeQCServiceOrderId').val(orderId);
                    $('#completeQCOrderNumber').val(qc.orderNumber || '');
                    $('#completeQCInspectorName').val(qc.qcInspectorName || 'Chưa xác định');
                    $('#completeQCNotes').val(qc.qcNotes || '');
                    $('#completeQCReworkRequired').prop('checked', qc.reworkRequired || false);
                    $('#completeQCReworkNotes').val(qc.reworkNotes || '');
                    
                    if (qc.reworkRequired) {
                        $('#reworkNotesGroup').show();
                    }
                    
                    // Load checklist items
                    $('#completeQCChecklistItems').empty();
                    self.checklistItemCounter = 0;
                    
                    if (qc.qcChecklistItems && qc.qcChecklistItems.length > 0) {
                        qc.qcChecklistItems.forEach(function(item) {
                            var $item = $(self.getChecklistItemHTML(item));
                            $('#completeQCChecklistItems').append($item);
                            self.checklistItemCounter++;
                        });
                    }
                    
                    // Reset QC result
                    $('input[name="qcResult"][value="Pass"]').prop('checked', true);
                    $('input[name="qcResult"]').closest('label').removeClass('active');
                    $('input[name="qcResult"][value="Pass"]').closest('label').addClass('active');
                    
                    $('#completeQCModal').modal('show');
                } else {
                    GarageApp.showError('Không tìm thấy thông tin QC. Vui lòng bắt đầu QC trước.');
                }
            },
            error: function(xhr) {
                if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin QC');
                }
            }
        });
    },

    getChecklistItemHTML: function(item) {
        var checked = item.isChecked ? 'checked' : '';
        var resultValue = item.result || '';
        var resultSelected = resultValue === 'Pass' ? 'selected' : (resultValue === 'Fail' ? 'selected' : '');
        
        var html = '<div class="checklist-item-row mb-2 border-bottom pb-2" data-display-order="' + (item.displayOrder || 0) + '">';
        html += '<div class="row">';
        html += '<div class="col-md-1">';
        html += '<input type="checkbox" class="checklist-item-checkbox form-control-sm" ' + checked + ' />';
        html += '</div>';
        html += '<div class="col-md-5">';
        html += '<input type="text" class="form-control form-control-sm checklist-item-name" value="' + (item.checklistItemName || '') + '" placeholder="Tên hạng mục kiểm tra" />';
        html += '</div>';
        html += '<div class="col-md-2">';
        html += '<select class="form-control form-control-sm checklist-item-result">';
        html += '<option value="">-- Kết quả --</option>';
        html += '<option value="Pass" ' + (resultValue === 'Pass' ? 'selected' : '') + '>Đạt</option>';
        html += '<option value="Fail" ' + (resultValue === 'Fail' ? 'selected' : '') + '>Không đạt</option>';
        html += '</select>';
        html += '</div>';
        html += '<div class="col-md-3">';
        html += '<input type="text" class="form-control form-control-sm checklist-item-notes" value="' + (item.notes || '') + '" placeholder="Ghi chú (tùy chọn)" />';
        html += '</div>';
        html += '<div class="col-md-1">';
        html += '<button type="button" class="btn btn-sm btn-danger btn-remove-checklist-item"><i class="fas fa-times"></i></button>';
        html += '</div>';
        html += '</div>';
        html += '</div>';
        
        return html;
    },

    submitCompleteQC: function() {
        var self = this;
        var orderId = $('#completeQCServiceOrderId').val();
        
        if (!orderId) {
            GarageApp.showError('Không tìm thấy đơn hàng');
            return;
        }
        
        var qcResult = $('input[name="qcResult"]:checked').val();
        if (!qcResult) {
            GarageApp.showError('Vui lòng chọn kết quả QC');
            return;
        }
        
        // Collect checklist items
        var checklistItems = [];
        $('#completeQCChecklistItems .checklist-item-row').each(function(index) {
            var $row = $(this);
            var item = {
                checklistItemName: $row.find('.checklist-item-name').val() || '',
                isChecked: $row.find('.checklist-item-checkbox').is(':checked'),
                result: $row.find('.checklist-item-result').val() || null,
                notes: $row.find('.checklist-item-notes').val() || null,
                displayOrder: index
            };
            
            if (item.checklistItemName.trim() !== '') {
                checklistItems.push(item);
            }
        });
        
        var data = {
            qcResult: qcResult,
            qcNotes: $('#completeQCNotes').val() || null,
            reworkRequired: $('#completeQCReworkRequired').is(':checked'),
            reworkNotes: $('#completeQCReworkNotes').val() || null,
            checklistItems: checklistItems
        };
        
        // ✅ SỬA: Nếu Fail → Gọi FailQC endpoint, nếu Pass → Gọi CompleteQC endpoint
        var endpointUrl = qcResult === 'Fail' 
            ? '/QCManagement/FailQC/' + orderId 
            : '/QCManagement/CompleteQC/' + orderId;
        
        $.ajax({
            url: endpointUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function(res) {
                if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                    return;
                }
                
                if (res && res.success) {
                    GarageApp.showSuccess(res.message || 'Đã hoàn thành QC');
                    $('#completeQCModal').modal('hide');
                    self.table.ajax.reload();
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(res) || 'Lỗi khi hoàn thành QC');
                }
            },
            error: function(xhr) {
                if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi hoàn thành QC');
                }
            }
        });
    },

    showHandoverModal: function(orderId) {
        var self = this;
        
        // Load order details
        $.ajax({
            url: '/OrderManagement/GetOrder/' + orderId,
            type: 'GET',
            success: function(res) {
                if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                    return;
                }
                
                if (res && res.success && res.data) {
                    var order = res.data;
                    
                    // Populate modal
                    $('#handoverServiceOrderId').val(orderId);
                    $('#handoverOrderNumber').val(order.orderNumber || '');
                    $('#handoverCustomerName').val(order.customer?.name || '');
                    $('#handoverVehiclePlate').val(order.vehicle?.licensePlate || '');
                    
                    // Load QC info to show result
                    $.ajax({
                        url: '/QCManagement/GetQC/' + orderId,
                        type: 'GET',
                        success: function(qcRes) {
                            if (qcRes && qcRes.success && qcRes.data) {
                                var qc = qcRes.data;
                                var resultText = '';
                                if (qc.qcResult === 'Pass') {
                                    resultText = '<span class="badge badge-success">Đạt</span>';
                                } else if (qc.qcResult === 'Fail') {
                                    resultText = '<span class="badge badge-danger">Không Đạt</span>';
                                } else {
                                    resultText = '<span class="badge badge-warning">Chờ Xử Lý</span>';
                                }
                                $('#handoverQCResult').html(resultText);
                            }
                        }
                    });
                    
                    // Set default date to now
                    var now = new Date();
                    now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
                    $('#handoverDate').val(now.toISOString().slice(0, 16));
                    
                    $('#handoverModal').modal('show');
                } else {
                    GarageApp.showError('Không thể tải thông tin đơn hàng');
                }
            },
            error: function(xhr) {
                if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin đơn hàng');
                }
            }
        });
    },

    submitHandover: function() {
        var self = this;
        var orderId = $('#handoverServiceOrderId').val();
        
        if (!orderId) {
            GarageApp.showError('Không tìm thấy đơn hàng');
            return;
        }
        
        var handoverDate = $('#handoverDate').val();
        var handoverLocation = $('#handoverLocation').val() || null;
        var handoverNotes = $('#handoverNotes').val() || null;
        
        var data = {
            handoverDate: handoverDate ? new Date(handoverDate).toISOString() : null,
            handoverLocation: handoverLocation,
            handoverNotes: handoverNotes
        };
        
        $.ajax({
            url: '/QCManagement/Handover/' + orderId,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function(res) {
                if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                    return;
                }
                
                if (res && res.success) {
                    GarageApp.showSuccess(res.message || 'Đã bàn giao xe thành công');
                    $('#handoverModal').modal('hide');
                    self.table.ajax.reload();
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(res) || 'Lỗi khi bàn giao xe');
                }
            },
            error: function(xhr) {
                if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi bàn giao xe');
                }
            }
        });
    },

    resetStartQCModal: function() {
        $('#startQCForm')[0].reset();
        $('#qcChecklistItems').empty();
        this.checklistItemCounter = 0;
    },

    resetCompleteQCModal: function() {
        $('#completeQCForm')[0].reset();
        $('#completeQCChecklistItems').empty();
        $('#reworkNotesGroup').hide();
        this.checklistItemCounter = 0;
        $('input[name="qcResult"][value="Pass"]').prop('checked', true);
    },

    resetHandoverModal: function() {
        $('#handoverForm')[0].reset();
        var now = new Date();
        now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
        $('#handoverDate').val(now.toISOString().slice(0, 16));
    }
};

// Initialize on document ready
$(document).ready(function() {
    if ($('#qcTable').length > 0) {
        GarageApp.QC.init();
    }
});

