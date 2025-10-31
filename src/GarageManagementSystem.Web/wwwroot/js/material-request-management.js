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
            $('#mrItemsTable tbody').empty();
            self.addItemRow();
            $('#createMRModal').modal('show');
        });

        $(document).on('click', '#btnAddMRItem', function(){
            self.addItemRow();
        });

        $(document).on('click', '.mr-remove-item', function(){
            $(this).closest('tr').remove();
        });

        $(document).on('click', '#btnSubmitCreateMR', function(){
            self.submitCreateMR();
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
        if (!serviceOrderId || serviceOrderId <= 0) { GarageApp.showError('Vui lòng nhập ID Phiếu Sửa Chữa'); return; }
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
});


