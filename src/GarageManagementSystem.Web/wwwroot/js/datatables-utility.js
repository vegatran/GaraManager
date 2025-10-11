/**
 * DataTables Utility Module
 * 
 * Provides common DataTables configurations and utilities
 */

window.DataTablesUtility = {
    // Default configuration
    defaultConfig: {
        processing: true,
        serverSide: false,
        responsive: true,
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
        dom: "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
             "<'row'<'col-sm-12'tr>>" +
             "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
        language: {
            processing: "Loading...",
            search: "Search:",
            lengthMenu: "Show _MENU_ entries",
            info: "Showing _START_ to _END_ of _TOTAL_ entries",
            infoEmpty: "Showing 0 to 0 of 0 entries",
            infoFiltered: "(filtered from _MAX_ total entries)",
            loadingRecords: "Loading...",
            zeroRecords: "No matching records found",
            emptyTable: "No data available",
            paginate: {
                first: "First",
                previous: "Previous",
                next: "Next",
                last: "Last"
            }
        }
    },

    // Initialize DataTable with custom config
    init: function(selector, config) {
        var mergedConfig = $.extend(true, {}, this.defaultConfig, config);
        return $(selector).DataTable(mergedConfig);
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
            console.log('Exporting to Excel:', filename, data);
        }
    },

    // Export to PDF
    exportToPDF: function(table, filename) {
        if (table) {
            var data = table.buttons.exportData();
            // Implementation for PDF export would go here
            console.log('Exporting to PDF:', filename, data);
        }
    }
};

// jQuery plugin wrapper
$.fn.dataTable.ext.errMode = 'throw';