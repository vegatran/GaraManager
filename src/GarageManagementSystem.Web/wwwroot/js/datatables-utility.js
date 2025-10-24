/**
 * DataTables Utility Module
 * 
 * Provides common DataTables configurations and utilities
 * Cung cấp cấu hình và tiện ích chung cho DataTables
 */

window.DataTablesUtility = {
    // Default configuration với style chung
    defaultConfig: {
        processing: true,
        serverSide: false,
        responsive: true,
        autoWidth: false,
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Tất cả"]],
        dom: "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'>>" +
             "<'row'<'col-sm-12'tr>>" +
             "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        language: {
            "sProcessing": "Đang xử lý...",
            "sLengthMenu": "Hiển thị _MENU_ mục",
            "sZeroRecords": "Không tìm thấy dữ liệu phù hợp",
            "sEmptyTable": "Không có dữ liệu trong bảng",
            "sInfo": "Hiển thị _START_ đến _END_ của _TOTAL_ mục",
            "sInfoEmpty": "Hiển thị 0 đến 0 của 0 mục",
            "sInfoFiltered": "(được lọc từ _MAX_ mục)",
            "sInfoPostFix": "",
            "sSearch": "Tìm kiếm:",
            "sUrl": "",
            "sInfoThousands": ",",
            "sLoadingRecords": "Đang tải...",
            "oPaginate": {
                "sFirst": "Đầu",
                "sPrevious": "Trước",
                "sNext": "Tiếp",
                "sLast": "Cuối"
            },
            "oAria": {
                "sSortAscending": ": sắp xếp cột tăng dần",
                "sSortDescending": ": sắp xếp cột giảm dần"
            }
        },
        // CSS classes chung cho styling
        classes: {
            sWrapper: "dataTables_wrapper dt-bootstrap4",
            sFilterInput: "form-control form-control-sm",
            sLengthSelect: "custom-select custom-select-sm form-control form-control-sm",
            sProcessing: "dataTables_processing card",
            sPageButton: "paginate_button page-item",
            sPageButtonActive: "paginate_button page-item active",
            sPageButtonDisabled: "paginate_button page-item disabled",
            sPageButtonPrevious: "paginate_button page-item previous",
            sPageButtonNext: "paginate_button page-item next"
        }
    },

    // Initialize DataTable with custom config - Hàm chính để khởi tạo DataTable
    init: function(selector, config) {
        var mergedConfig = $.extend(true, {}, this.defaultConfig, config);
        return $(selector).DataTable(mergedConfig);
    },

    // Khởi tạo DataTable với cấu hình chuẩn cho tất cả màn hình
    initStandardTable: function(selector, config) {
        var self = this;
        
        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable(selector)) {
            $(selector).DataTable().destroy();
        }
        
        // Cấu hình chuẩn cho tất cả bảng
        var standardConfig = {
            processing: true,
            serverSide: false,
            responsive: true,
            autoWidth: false,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Tất cả"]],
            dom: "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'>>" +
                 "<'row'<'col-sm-12'tr>>" +
                 "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
            language: this.defaultConfig.language,
            classes: this.defaultConfig.classes,
            // Error handling chung
            ajax: {
                error: function(xhr, status, error) {
                    if (window.AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                        AuthHandler.handleUnauthorized(xhr, true);
                    } else if (window.GarageApp) {
                        GarageApp.showError('Lỗi khi tải dữ liệu');
                    } else {
                    }
                }
            },
            // Column defaults
            columnDefs: [
                {
                    targets: -1, // Last column (Actions)
                    orderable: false,
                    searchable: false
                }
            ]
        };
        
        // Merge với config được truyền vào
        var finalConfig = $.extend(true, {}, standardConfig, config);
        
        return $(selector).DataTable(finalConfig);
    },

    // Khởi tạo DataTable với AJAX và error handling chuẩn
    initAjaxTable: function(selector, ajaxUrl, columns, config) {
        var ajaxConfig = {
            ajax: {
                url: ajaxUrl,
                type: 'GET'
            },
            columns: columns
        };
        
        var finalConfig = $.extend(true, {}, ajaxConfig, config);
        return this.initStandardTable(selector, finalConfig);
    },

    // Khởi tạo DataTable với Server-Side Pagination
    initServerSideTable: function(selector, ajaxUrl, columns, config) {
        var self = this;
        
        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable(selector)) {
            $(selector).DataTable().destroy();
        }
        
        var serverSideConfig = {
            processing: true,
            serverSide: true,
            responsive: true,
            autoWidth: false,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
            dom: "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
                 "<'row'<'col-sm-12'tr>>" +
                 "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
            language: this.defaultConfig.language,
            columns: columns,
            ajax: {
                url: ajaxUrl,
                type: 'GET',
                data: function(d) {
                    // Convert DataTables parameters to API parameters
                    var params = {
                        pageNumber: (d.start / d.length) + 1,
                        pageSize: d.length,
                        searchTerm: d.search.value
                    };
                    
                    // Add custom filters if provided
                    if (config && config.customFilters) {
                        $.extend(params, config.customFilters);
                    }
                    
                    return params;
                },
                dataSrc: function(json) {
                    // Handle PagedResponse from API
                    if (json.success && json.data) {
                        // Set pagination info for DataTables
                        json.recordsTotal = json.totalCount || 0;
                        json.recordsFiltered = json.totalCount || 0;
                        // Return data array for table rendering
                        return json.data;
                    }
                    return [];
                },
                error: function(xhr, status, error) {
                    if (window.AuthHandler && AuthHandler.isUnauthorized(xhr)) {
                        AuthHandler.handleUnauthorized(xhr, true);
                    } else if (window.GarageApp) {
                        GarageApp.showError('Lỗi khi tải dữ liệu');
                    } else {
                    }
                }
            }
        };
        
        // Merge with custom config
        var finalConfig = $.extend(true, {}, serverSideConfig, config);
        
        // Remove customFilters from final config as it's not a DataTables option
        delete finalConfig.customFilters;
        
        return $(selector).DataTable(finalConfig);
    },

    // Reload table data
    reload: function(table, resetPaging) {
        if (table) {
            table.ajax.reload(null, resetPaging || false);
        }
    },

    // Clear table
    clear: function(table) {
        if (table) {
            table.clear().draw();
        }
    },

    // Add row
    addRow: function(table, data) {
        if (table) {
            table.row.add(data).draw();
        }
    },

    // Update row
    updateRow: function(table, selector, data) {
        if (table) {
            table.row(selector).data(data).draw();
        }
    },

    // Remove row
    removeRow: function(table, selector) {
        if (table) {
            table.row(selector).remove().draw();
        }
    },

    // Get selected rows
    getSelectedRows: function(table) {
        if (table) {
            return table.rows({ selected: true }).data();
        }
        return [];
    },

    // Export to Excel
    exportToExcel: function(table, filename) {
        if (table) {
            var data = table.buttons.exportData();
            // Implementation for Excel export would go here
        }
    },

    // Export to PDF
    exportToPDF: function(table, filename) {
        if (table) {
            var data = table.buttons.exportData();
            // Implementation for PDF export would go here
        }
    },

    // Render functions chung
    renderStatus: function(data, type, row) {
        // Purchase Order statuses
        if (data === 'Draft') {
            return '<span class="badge badge-secondary">Nháp</span>';
        } else if (data === 'Sent') {
            return '<span class="badge badge-info">Đã gửi</span>';
        } else if (data === 'Received') {
            return '<span class="badge badge-success">Đã nhận</span>';
        } else if (data === 'Cancelled') {
            return '<span class="badge badge-danger">Đã hủy</span>';
        }
        // Legacy statuses
        else if (data === true || data === 'true' || 
            data === 'Active' || data === 'active' || 
            data === 'Đang Làm Việc' || data === 'Hoạt động') {
            return '<span class="badge badge-success">Hoạt động</span>';
        } else {
            return '<span class="badge badge-danger">Không hoạt động</span>';
        }
    },

    renderDate: function(data, type, row) {
        if (data) {
            var date = new Date(data);
            return date.toLocaleDateString('vi-VN');
        }
        return '';
    },

    renderCurrency: function(data, type, row) {
        if (data) {
            // ✅ SỬA: Xử lý string đã được format từ API
            var numericValue = typeof data === 'string' ? 
                parseFloat(data.replace(/[^\d]/g, '')) : 
                parseFloat(data);
            
            if (!isNaN(numericValue)) {
                return numericValue.toLocaleString('vi-VN') + ' VNĐ';
            }
        }
        return '0 VNĐ';
    },

    renderNumber: function(data, type, row) {
        if (data && !isNaN(data)) {
            return new Intl.NumberFormat('vi-VN').format(data);
        }
        return data || '0';
    },

    renderActions: function(data, type, row, actionButtons) {
        var actions = '';
        if (actionButtons) {
            actionButtons.forEach(function(button) {
                var icon = button.icon || 'fa-edit';
                var color = button.color || 'primary';
                var title = button.title || 'Thao tác';
                actions += '<button type="button" class="btn btn-sm btn-' + color + ' ' + button.action + '-btn" data-id="' + row.id + '" title="' + title + '">';
                actions += '<i class="fa ' + icon + '"></i>';
                actions += '</button> ';
            });
        }
        return actions;
    }
};

// jQuery plugin wrapper
$.fn.dataTable.ext.errMode = 'throw';

// Debounce utility for search
$.debounce = function(delay, fn) {
    var timer = null;
    return function() {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = setTimeout(function() {
            fn.apply(context, args);
        }, delay);
    };
};