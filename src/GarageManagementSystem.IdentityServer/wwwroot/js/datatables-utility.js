/**
 * DataTables Utility Module
 * 
 * Mô tả:
 * - Cung cấp các utility function chung cho DataTables
 * - Configurations và helpers có thể tái sử dụng
 * - Isolation để tránh conflict giữa các page khác nhau
 */

window.DataTablesUtility = {
    // Default DataTable configuration
    defaultConfig: {
        responsive: true,
        processing: true,
        serverSide: false,
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
        language: {
            "processing": "Loading data...",
            "lengthMenu": "Show _MENU_ entries",
            "zeroRecords": "No matching records found",
            "info": "Showing _START_ to _END_ of _TOTAL_ entries",
            "infoEmpty": "Showing 0 to 0 of 0 entries",
            "infoFiltered": "(filtered from _MAX_ total entries)",
            "loadingRecords": "Loading...",
            "paginate": {
                "first": "First",
                "last": "Last",
                "next": "Next",
                "previous": "Previous"
            },
            "search": "Search:",
            "emptyTable": "No data available in table"
        },
        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
             '<"row"<"col-sm-12"tr>>' +
             '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
        drawCallback: function(settings) {
            // Auto-hide processing indicator when complete
            $('.loading-indicator').hide();
        }
    },

    // Initialize DataTable with custom configuration
    initialize: function(tableId, customConfig) {
        var config = $.extend(true, {}, this.defaultConfig, customConfig);
        
        // Show loading indicator
        $('.loading-indicator').show();
        
        return $(tableId).DataTable(config);
    },

    // Quick initialize for common scenarios
    initializeTable: function(tableId, options) {
        var defaults = {
            ajaxUrl: null,
            columns: [],
            pageLength: 25,
            showActions: true,
            actionCallbacks: {
                view: null,
                edit: null,
                delete: null
            },
            language: {
                processing: "Loading data...",
                emptyTable: "No data available",
                info: "Showing _START_ to _END_ of _TOTAL_ entries",
                infoEmpty: "Showing 0 to 0 of 0 entries",
                infoFiltered: "(filtered from _MAX_ total entries)"
            }
        };
        
        var settings = $.extend(true, {}, defaults, options);
        var config = $.extend(true, {}, this.defaultConfig, {
            pageLength: settings.pageLength,
            language: settings.language
        });
        
        // Add AJAX configuration if URL provided
        if (settings.ajaxUrl) {
            config.ajax = {
                url: settings.ajaxUrl,
                type: 'GET',
                dataSrc: 'data'
            };
        }
        
        // Add columns
        config.columns = settings.columns.slice();
        
        // Add action column if requested
        if (settings.showActions) {
            config.columns.push({
                data: null,
                orderable: false,
                render: function(data, type, row) {
                    var buttons = '<div class="action-buttons">';
                    
                        // Determine which value to use for actions
                        var useId = settings.actionCallbacks.useIdForActions === true;
                        var paramValue = useId ? row.id : (row.name || row.clientId || row.id);
                    
                    if (settings.actionCallbacks.view) {
                        buttons += `<button class="btn btn-info btn-sm" onclick="${settings.actionCallbacks.view}(${paramValue})" title="View">
                                       <i class="fas fa-eye"></i>
                                   </button>`;
                    }
                    
                    if (settings.actionCallbacks.edit) {
                        buttons += `<button class="btn btn-warning btn-sm" onclick="${settings.actionCallbacks.edit}(${paramValue})" title="Edit">
                                           <i class="fas fa-edit"></i>
                                       </button>`;
                    }
                    
                    // Conditional actions based on row data
                    if (settings.actionCallbacks.conditionalActions) {
                        if (row.isActive) {
                            // Show Delete button for active claims
                            if (settings.actionCallbacks.delete) {
                                buttons += `<button class="btn btn-danger btn-sm" onclick="${settings.actionCallbacks.delete}(${row.id})" title="Delete">
                                               <i class="fas fa-trash"></i>
                                           </button>`;
                            }
                        } else {
                            // Show Restore button for inactive claims
                            if (settings.actionCallbacks.restore) {
                                buttons += `<button class="btn btn-success btn-sm" onclick="${settings.actionCallbacks.restore}(${row.id})" title="Restore">
                                               <i class="fas fa-undo"></i>
                                           </button>`;
                            }
                        }
                    } else {
                        // Default behavior - always show delete button
                        if (settings.actionCallbacks.delete) {
                            buttons += `<button class="btn btn-danger btn-sm" onclick="${settings.actionCallbacks.delete}(${row.id})" title="Delete">
                                           <i class="fas fa-trash"></i>
                                       </button>`;
                        }
                    }
                    
                    buttons += '</div>';
                    return buttons;
                }
            });
        }
        
        // Destroy existing DataTable if exists
        if ($.fn.DataTable.isDataTable(tableId)) {
            $(tableId).DataTable().destroy();
        }
        
        return $(tableId).DataTable(config);
    },

    // Create action buttons column
    createActionColumn: function(options) {
        var defaults = {
            viewButton: true,
            editButton: true,
            deleteButton: true,
            viewText: 'View',
            editText: 'Edit',
            deleteText: 'Delete',
            viewIcon: 'fas fa-eye',
            editIcon: 'fas fa-edit',
            deleteIcon: 'fas fa-trash',
            viewClass: 'btn btn-info btn-sm',
            editClass: 'btn btn-warning btn-sm',
            deleteClass: 'btn btn-danger btn-sm'
        };
        
        var settings = $.extend({}, defaults, options);
        
        return {
            "data": "id",
            "render": function(data, type, row) {
                var buttons = '';
                
                if (settings.viewButton) {
                    buttons += '<button class="' + settings.viewClass + ' view-btn" data-id="' + data + '">' +
                               '<i class="' + settings.viewIcon + '"></i> ' + settings.viewText + '</button> ';
                }
                
                if (settings.editButton) {
                    buttons += '<button class="' + settings.editClass + ' edit-btn" data-id="' + data + '">' +
                               '<i class="' + settings.editIcon + '"></i> ' + settings.editText + '</button> ';
                }
                
                if (settings.deleteButton) {
                    buttons += '<button class="' + settings.deleteClass + ' delete-btn" data-id="' + data + '" data-display-id="' + (row.name || row.title || row.clientId || data) + '">' +
                               '<i class="' + settings.deleteIcon + '"></i> ' + settings.deleteText + '</button>';
                }
                
                return buttons;
            }
        };
    },

    // Create badge column
    createBadgeColumn: function(dataProperty, mapping) {
        return {
            "data": dataProperty,
            "render": function(data) {
                var badgeClass = 'badge badge-secondary';
                var text = '';
                
                if (mapping && mapping[data]) {
                    var config = mapping[data];
                    badgeClass = config.class || badgeClass;
                    text = config.text || data;
                } else {
                    text = data;
                }
                
                return '<span class="' + badgeClass + '">' + text + '</span>';
            }
        };
    },

    // Create list column (comma-separated values)
    createListColumn: function(dataProperty, separator) {
        separator = separator || ', ';
        
        return {
            "data": dataProperty,
            "render": function(data) {
                if (Array.isArray(data)) {
                    return data.join(separator);
                } else if (typeof data === 'string') {
                    return data;
                }
                return '';
            }
        };
    },

    // Create default column configuration
    createDefaultColumns: function(columns, options) {
        var defaults = {
            viewButton: true,
            editButton: true,
            deleteButton: true
        };
        
        var settings = $.extend({}, defaults, options);
        var result = columns.slice(); // Copy array
        
        // Add action column if not already present
        if (settings.viewButton || settings.editButton || settings.deleteButton) {
            result.push(this.createActionColumn(settings));
        }
        
        return result;
    },

    // Refresh table
    refresh: function(tableId) {
        $(tableId).DataTable().ajax.reload();
    },

    // Show/hide columns
    toggleColumn: function(tableId, columnIndex) {
        var table = $(tableId).DataTable();
        var column = table.column(columnIndex);
        
        column.visible(!column.visible());
    },

    // Export functionality
    exportTable: function(tableId, format, filename) {
        var table = $(tableId).DataTable();
        
        switch (format) {
            case 'csv':
                this.exportCSV(table, filename);
                break;
            case 'excel':
                this.exportExcel(table, filename);
                break;
            case 'pdf':
                this.exportPDF(table, filename);
                break;
            default:
                console.warn('Export format not supported:', format);
        }
    },

    // Export to CSV
    exportCSV: function(table, filename) {
        var csv = table.data().toArray().map(function(row) {
            return row.join(',');
        });
        
        var csvContent = csv.join('\n');
        this.downloadFile(csvContent, filename || 'export.csv', 'text/csv');
    },

    // Export to Excel (HTML format)
    exportExcel: function(table, filename) {
        var clonedTable = table.table().node().cloneNode(true);
        
        // Remove action buttons column
        $(clonedTable).find('th:last-child, td:last-child').remove();
        
        var excelContent = '<table>' + clonedTable.innerHTML + '</table>';
        this.downloadFile(excelContent, filename || 'export.xls', 'application/vnd.ms-excel');
    },

    // Export to PDF (simple HTML format)
    exportPDF: function(table, filename) {
        var clonedTable = table.table().node().cloneNode(true);
        
        // Remove action buttons column
        $(clonedTable).find('th:last-child, td:last-child').remove();
        
        var pdfContent = '<html><body><table>' + clonedTable.innerHTML + '</table></body></html>';
        this.downloadFile(pdfContent, filename || 'export.pdf', 'application/pdf');
    },

    // Download file helper
    downloadFile: function(content, filename, contentType) {
        var blob = new Blob([content], { type: contentType });
        var url = window.URL.createObjectURL(blob);
        
        var link = document.createElement('a');
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        window.URL.revokeObjectURL(url);
    },

    // Search table
    search: function(tableId, searchTerm) {
        $(tableId).DataTable().search(searchTerm).draw();
    },

    // Clear search
    clearSearch: function(tableId) {
        $(tableId).DataTable().search('').draw();
    },

    // Get selected rows
    getSelectedRows: function(tableId) {
        return $(tableId + ' tbody tr.selected');
    },

    // Select row
    selectRow: function(tableId, rowData) {
        $(tableId).DataTable().row(function(idx, data, node) {
            return data.id === rowData.id;
        }).select();
    },

    // Clear selection
    clearSelection: function(tableId) {
        $(tableId).DataTable().rows().deselect();
    }
};
