// Align with EmployeeManagement pattern: single module under GarageApp namespace,
// robust error handling via AuthHandler/GarageApp helpers, server-side DataTable
window.GarageApp = window.GarageApp || {};
GarageApp.MR = {
    table: null,

    init: function() {
        this.initTable();
        this.bindEvents();
    },

    initTable: function() {
        var self = this;
        self.table = $('#mrTable').DataTable({
            processing: true,
            serverSide: true,
            searching: false,
            pageLength: 10,
            lengthMenu: [10, 25, 50],
            language: GarageApp.dataTableVi || {},
            responsive: true,
            order: [],
            rowId: 'id',
            dom: "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6 text-right'>>" +
                 "<'row'<'col-sm-12'tr>>" +
                 "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
            ajax: function(data, callback) {
                var pageNumber = Math.floor(data.start / data.length) + 1;
                var pageSize = data.length;
                $.ajax({
                    url: '/MaterialRequestManagement/GetMRsPaged',
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
                            GarageApp.showError(GarageApp.parseErrorMessage(res) || 'Lỗi khi tải danh sách MR');
                            callback({ data: [], recordsTotal: 0, recordsFiltered: 0 });
                        }
                    },
                    error: function(xhr) {
                        if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi tải danh sách MR');
                        }
                        callback({ data: [], recordsTotal: 0, recordsFiltered: 0 });
                    }
                });
            },
            columns: [
                { data: 'mrNumber', title: 'MR #' },
                { data: 'serviceOrderId', title: 'Service Order' },
                { data: 'status', title: 'Trạng thái' },
                { data: null, orderable: false, render: function(row) {
                    return '<div class="btn-group">'
                        + '<button class="btn btn-sm btn-info mr-view" data-id="'+row.id+'"><i class="fas fa-eye"></i></button>'
                        + '</div>';
                }}
            ]
        });
    },

    bindEvents: function() {
        var self = this;
        $(document).on('click', '#btnShowCreateMR', function(){
            self.showCreateModal();
        });

        $(document).on('click', '#btnAddMRItem', function(){
            self.addItemRow();
        });

        $(document).on('click', '.mr-remove-item', function(){
            $(this).closest('tr').remove();
        });

        $(document).on('click', '.mr-view', function() {
            var mrId = $(this).data('id');
            if (mrId) {
                self.showMRDetails(mrId);
            }
        });

        $(document).on('click', '#btnSubmitCreateMR', function(){
            self.submitCreateMR();
        });

        // ✅ THÊM: Event listeners cho JO parts section
        $(document).on('click', '.jo-add-part-btn', function(){
            var partId = $(this).data('part-id');
            var partName = $(this).data('part-name');
            var quantity = $(this).data('quantity');
            self.addPartToMR(partId, partName, quantity);
        });

        $(document).on('click', '#btnAddAllParts', function(){
            $('#joPartsTableBody tr').each(function(){
                var checkbox = $(this).find('.jo-part-checkbox');
                if (checkbox.length > 0) {
                    var partId = checkbox.data('part-id');
                    var partName = checkbox.data('part-name');
                    var quantity = checkbox.data('quantity');
                    self.addPartToMR(partId, partName, quantity);
                }
            });
            GarageApp.showSuccess('Đã thêm tất cả phụ tùng vào MR');
        });

        $(document).on('click', '#btnClearSelectedParts', function(){
            $('#joPartsTableBody tr').each(function(){
                var checkbox = $(this).find('.jo-part-checkbox');
                if (checkbox.is(':checked')) {
                    var partId = checkbox.data('part-id');
                    // Xóa từ bảng MR items nếu đã thêm
                    $('#mrItemsTable tbody tr').each(function(){
                        var rowPartId = parseInt($(this).find('.part-id').val() || '0');
                        if (rowPartId === partId) {
                            $(this).remove();
                        }
                    });
                }
            });
            GarageApp.showSuccess('Đã xóa các phụ tùng đã chọn khỏi MR');
        });
    },

    addItemRow: function(){
        var row = $('<tr>');
        var partInput = $('<input type="text" class="form-control form-control-sm part-typeahead" placeholder="Tìm kiếm phụ tùng...">');
        var partIdHidden = $('<input type="hidden" class="part-id">');
        row.append($('<td>').append(partInput).append(partIdHidden));
        row.append($('<td style="width:120px">').append('<input type="number" class="form-control form-control-sm qty" value="1" min="1">'));
        row.append($('<td style="width:60px">').append('<button type="button" class="btn btn-sm btn-outline-danger mr-remove-item"><i class="fas fa-times"></i></button>'));
        $('#mrItemsTable tbody').append(row);
        this.initPartTypeahead(partInput);
    },

    // ✅ THÊM: Hàm thêm phụ tùng từ JO vào MR
    addPartToMR: function(partId, partName, quantity) {
        var self = this;
        
        // Kiểm tra xem phụ tùng đã tồn tại chưa
        var exists = false;
        $('#mrItemsTable tbody tr').each(function(){
            var rowPartId = parseInt($(this).find('.part-id').val() || '0');
            if (rowPartId === partId) {
                // Đã tồn tại, cập nhật số lượng
                var currentQty = parseInt($(this).find('.qty').val() || '1');
                $(this).find('.qty').val(currentQty + quantity);
                exists = true;
                return false; // Break loop
            }
        });
        
        if (!exists) {
            // Thêm mới
            var row = $('<tr>');
            var partInput = $('<input type="text" class="form-control form-control-sm part-typeahead" placeholder="Tìm kiếm phụ tùng..." value="' + partName + '" readonly>');
            var partIdHidden = $('<input type="hidden" class="part-id" value="' + partId + '">');
            row.append($('<td>').append(partInput).append(partIdHidden));
            row.append($('<td style="width:120px">').append('<input type="number" class="form-control form-control-sm qty" value="' + quantity + '" min="1">'));
            row.append($('<td style="width:60px">').append('<button type="button" class="btn btn-sm btn-outline-danger mr-remove-item"><i class="fas fa-times"></i></button>'));
            $('#mrItemsTable tbody').append(row);
            
            // Đánh dấu checkbox đã được thêm
            $('#joPartsTableBody tr').each(function(){
                var checkbox = $(this).find('.jo-part-checkbox');
                if (checkbox.data('part-id') === partId) {
                    checkbox.prop('checked', true);
                }
            });
        }
    },

    showCreateModal: function() {
        var self = this;
        // Reset form
        $('#mrServiceOrderId').val('').trigger('change');
        $('#mrNotes').val('');
        $('#mrItemsTable tbody').empty();
        $('#joPartsSection').hide();
        $('#joPartsTableBody').empty();
        // ✅ THÊM: Reset nút "Tạo MR"
        $('#btnSubmitCreateMR').prop('disabled', false).removeAttr('title');
        self.addItemRow();
        
        // Load Service Orders vào dropdown
        self.loadServiceOrders(function() {
            // Hiển thị modal sau khi load xong
            $('#createMRModal').modal('show');
        });
    },

    showMRDetails: function(mrId) {
        var modal = $('#viewMRModal');
        modal.find('#viewMRItemsBody').empty();
        modal.find('#viewMRNumber').text('');
        modal.find('#viewServiceOrderId').text('');
        modal.find('#viewMRStatus').text('');
        modal.find('#viewMRNotes').text('');
        modal.modal('show');

        $.ajax({
            url: '/MaterialRequestManagement/GetMR/' + mrId,
            type: 'GET',
            success: function(res) {
                if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                    return;
                }

                if (!res || !res.success || !res.data) {
                    GarageApp.showError(GarageApp.parseErrorMessage(res) || 'Không thể tải chi tiết MR');
                    return;
                }

                var mr = res.data;
                modal.find('#viewMRNumber').text(mr.mrNumber);
                modal.find('#viewServiceOrderId').text(mr.serviceOrderId);
                modal.find('#viewMRStatus').text(mr.status);
                modal.find('#viewMRNotes').text(mr.notes || 'Không có ghi chú');

                var itemsBody = modal.find('#viewMRItemsBody');
                itemsBody.empty();
                if (mr.items && mr.items.length > 0) {
                    mr.items.forEach(function(item) {
                        var row = $('<tr>');
                        row.append($('<td>').text(item.partName || ''));
                        row.append($('<td>').text(item.quantityRequested || 0));
                        row.append($('<td>').text(item.quantityApproved || 0));
                        row.append($('<td>').text(item.quantityPicked || 0));
                        row.append($('<td>').text(item.quantityIssued || 0));
                        itemsBody.append(row);
                    });
                } else {
                    itemsBody.append('<tr><td colspan="5" class="text-center text-muted">Không có vật tư</td></tr>');
                }
            },
            error: function(xhr) {
                if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải chi tiết MR');
                }
                modal.modal('hide');
            }
        });
    },

    loadServiceOrders: function(callback) {
        var self = this;
        
        // Nếu Select2 chưa được khởi tạo, khởi tạo nó
        if (!$('#mrServiceOrderId').hasClass('select2-hidden-accessible')) {
            $('#mrServiceOrderId').select2({
                placeholder: '-- Chọn Phiếu Sửa Chữa --',
                allowClear: true,
                width: '100%'
            });
        }
        
        // ✅ THÊM: Event listener khi chọn JO
        $('#mrServiceOrderId').off('change').on('change', function() {
            var serviceOrderId = $(this).val();
            if (serviceOrderId && serviceOrderId !== '') {
                self.loadServiceOrderParts(parseInt(serviceOrderId));
            } else {
                $('#joPartsSection').hide();
                $('#joPartsTableBody').empty();
            }
        });
        
        // Load danh sách Service Orders
        $.ajax({
            url: '/MaterialRequestManagement/GetAvailableServiceOrders',
            type: 'GET',
            success: function(res) {
                if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                    if (callback) callback();
                    return;
                }
                
                if (res && res.success && res.data) {
                    // Clear existing options (giữ option đầu tiên)
                    $('#mrServiceOrderId').find('option').not(':first').remove();
                    
                    // Add Service Orders
                    res.data.forEach(function(order) {
                        var option = $('<option></option>')
                            .attr('value', order.id)
                            .text(order.text);
                        $('#mrServiceOrderId').append(option);
                    });
                    
                    // Refresh Select2
                    $('#mrServiceOrderId').trigger('change');
                } else {
                    GarageApp.showError('Không thể tải danh sách phiếu sửa chữa');
                }
                
                if (callback) callback();
            },
            error: function(xhr) {
                if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải danh sách phiếu sửa chữa');
                }
                if (callback) callback();
            }
        });
    },

    loadServiceOrderParts: function(serviceOrderId) {
        var self = this;
        
        $.ajax({
            url: '/MaterialRequestManagement/GetServiceOrderParts/' + serviceOrderId,
            type: 'GET',
            success: function(res) {
                if (AuthHandler && !AuthHandler.validateApiResponse(res)) {
                    return;
                }
                
                // ✅ SỬA: Hiển thị thông báo nếu không có phụ tùng
                if (res && res.success) {
                    // ✅ THÊM: Kiểm tra hasParts flag
                    if (!res.hasParts || !res.parts || res.parts.length === 0) {
                        // Ẩn section phụ tùng
                        $('#joPartsSection').hide();
                        $('#joPartsTableBody').empty();
                        
                        // ✅ THÊM: Disable nút "Tạo MR" và hiển thị thông báo
                        $('#btnSubmitCreateMR').prop('disabled', true)
                            .attr('title', 'Không có phụ tùng cần xuất kho, không cần tạo MR');
                        
                        // ✅ THÊM: Hiển thị thông báo với hướng dẫn bước tiếp theo
                        var message = res.message || 'Không có phụ tùng cần xuất kho cho đơn hàng này.\n\n' +
                            '👉 Bước tiếp theo:\n' +
                            '1. Đóng modal này\n' +
                            '2. Vào "Phiếu Sửa Chữa"\n' +
                            '3. Chuyển trạng thái sang "Chờ Phân Công"\n' +
                            '4. Phân công KTV và bắt đầu sửa chữa';
                        
                        GarageApp.showInfo(message.replace(/\n/g, '<br>'), 'Không cần tạo MR');
                        return;
                    }
                    
                    // ✅ Có phụ tùng, enable nút "Tạo MR"
                    $('#btnSubmitCreateMR').prop('disabled', false).removeAttr('title');
                    
                    // Có phụ tùng, hiển thị section
                    $('#joPartsSection').show();
                    
                    // Cập nhật thông tin JO
                    $('#joPartsInfo').html(
                        '<strong>JO:</strong> ' + res.orderNumber + 
                        ' | <strong>KH:</strong> ' + res.customerName + 
                        ' | <strong>Xe:</strong> ' + res.vehiclePlate +
                        (res.description && res.description !== 'Không có mô tả' ? ' | <strong>Mô tả:</strong> ' + res.description : '')
                    );
                    
                    // Clear và populate bảng phụ tùng
                    $('#joPartsTableBody').empty();
                    res.parts.forEach(function(part) {
                        var row = $('<tr>');
                        row.append($('<td>').html('<input type="checkbox" class="jo-part-checkbox" data-part-id="' + part.partId + '" data-part-name="' + part.partName + '" data-quantity="' + part.quantity + '">'));
                        row.append($('<td>').text(part.partName));
                        row.append($('<td>').text(part.quantity));
                        row.append($('<td>').html('<button type="button" class="btn btn-sm btn-outline-primary jo-add-part-btn" data-part-id="' + part.partId + '" data-part-name="' + part.partName + '" data-quantity="' + part.quantity + '"><i class="fas fa-plus"></i> Thêm</button>'));
                        $('#joPartsTableBody').append(row);
                    });
                } else {
                    // Ẩn section nếu không có phụ tùng
                    $('#joPartsSection').hide();
                }
            },
            error: function(xhr) {
                if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    console.error('Lỗi khi load phụ tùng từ JO:', xhr);
                    $('#joPartsSection').hide();
                }
            }
        });
    },

    initPartTypeahead: function(input){
        input.typeahead({
            source: function(query, process){
                $.get('/StockManagement/SearchParts', { q: query }, function(res){
                    if (AuthHandler && !AuthHandler.validateApiResponse(res)) { return process([]); }
                    if (res && res.success && res.data) {
                        var arr = res.data.map(function(p){ return { id: p.id, name: p.text }; });
                        process(arr);
                    } else {
                        process([]);
                    }
                });
            },
            displayText: function(item){ return item.name; },
            afterSelect: function(item){
                var tr = input.closest('tr');
                tr.find('.part-id').val(item.id);
                input.val(item.name);
            },
            delay: 300
        });
    },

    submitCreateMR: function(){
        var serviceOrderId = parseInt($('#mrServiceOrderId').val() || '0');
        if (!serviceOrderId || serviceOrderId <= 0) { 
            GarageApp.showError('Vui lòng chọn Phiếu Sửa Chữa'); 
            return; 
        }
        var items = [];
        $('#mrItemsTable tbody tr').each(function(){
            var partId = parseInt($(this).find('.part-id').val() || '0');
            var qty = parseInt($(this).find('.qty').val() || '0');
            if (partId > 0 && qty > 0) {
                items.push({ partId: partId, quantityRequested: qty });
            }
        });
        if (items.length === 0) { GarageApp.showError('Vui lòng thêm ít nhất 1 vật tư'); return; }

        var dto = { serviceOrderId: serviceOrderId, notes: $('#mrNotes').val() || '', items: items };
        $.ajax({
            url: '/MaterialRequestManagement/CreateMR',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(dto),
            success: function(res){
                if (AuthHandler && !AuthHandler.validateApiResponse(res)) { return; }
                if (res && res.success) {
                    GarageApp.showSuccess('Tạo MR thành công');
                    $('#createMRModal').modal('hide');
                    GarageApp.MR.table.ajax.reload();
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(res) || 'Lỗi khi tạo MR');
                }
            },
            error: function(xhr){
                if (AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tạo MR');
                }
            }
        });
    }
};

$(document).ready(function(){
    GarageApp.MR.init();
    
    // ✅ 2.3.3: Tự động mở modal tạo MR nếu có serviceOrderId trong query string
    var urlParams = new URLSearchParams(window.location.search);
    var serviceOrderId = urlParams.get('serviceOrderId');
    if (serviceOrderId) {
        // Đợi một chút để đảm bảo init đã hoàn thành
        setTimeout(function() {
            GarageApp.MR.showCreateModal();
            // Set Service Order sau khi modal đã mở và dropdown đã load
            setTimeout(function() {
                $('#mrServiceOrderId').val(serviceOrderId).trigger('change');
            }, 500);
        }, 500);
    }
});


