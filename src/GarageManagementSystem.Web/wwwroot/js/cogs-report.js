window.CogsReport = {
    formatCurrency: function(value) {
        if (!value || isNaN(value)) {
            return '0 VNĐ';
        }
        return Number(value).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });
    },

    formatNumber: function(value) {
        if (!value || isNaN(value)) {
            return '0';
        }
        return Number(value).toLocaleString('vi-VN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    },

    formatDate: function(dateString) {
        if (!dateString) {
            return '';
        }
        var date = new Date(dateString);
        if (isNaN(date.getTime())) {
            return '';
        }
        return date.toLocaleDateString('vi-VN');
    },

    getFilters: function() {
        return {
            startDate: $('#cogsStartDate').val(),
            endDate: $('#cogsEndDate').val(),
            method: $('#cogsMethod').val()
        };
    },

    buildQuery: function(filters) {
        var query = [];
        if (filters.startDate) {
            query.push('startDate=' + encodeURIComponent(filters.startDate));
        }
        if (filters.endDate) {
            query.push('endDate=' + encodeURIComponent(filters.endDate));
        }
        if (filters.method) {
            query.push('method=' + encodeURIComponent(filters.method));
        }
        return query.length ? '?' + query.join('&') : '';
    },

    loadReport: function() {
        var self = this;
        var filters = self.getFilters();

        GarageApp.showLoading('Đang tải dữ liệu COGS...');

        $.ajax({
            url: '/OrderManagement/GetCogsSummary' + self.buildQuery(filters),
            type: 'GET'
        })
        .done(function(response) {
            GarageApp.hideLoading();

            if (!AuthHandler.validateApiResponse(response)) {
                return;
            }

            if (!response.success || !response.data || !response.data.data) {
                GarageApp.showWarning(response.errorMessage || response.message || 'Không có dữ liệu.');
                self.renderSummary(null);
                self.renderTable([]);
                return;
            }

            var report = response.data.data;
            self.renderSummary(report);
            self.renderTable(report.orders || []);
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (AuthHandler.isUnauthorized(xhr)) {
                AuthHandler.handleUnauthorized(xhr, true);
                return;
            }
            GarageApp.showError('Không thể tải báo cáo COGS.');
        });
    },

    renderSummary: function(summary) {
        summary = summary || {
            totalRevenue: 0,
            totalCogs: 0,
            totalGrossProfit: 0,
            averageGrossMargin: 0
        };

        $('#summaryTotalRevenue').text(this.formatCurrency(summary.totalRevenue));
        $('#summaryTotalCogs').text(this.formatCurrency(summary.totalCogs));
        $('#summaryGrossProfit').text(this.formatCurrency(summary.totalGrossProfit));
        $('#summaryGrossMargin').text(this.formatNumber(summary.averageGrossMargin) + ' %');
    },

    renderTable: function(orders) {
        var self = this;
        var $tbody = $('#cogsReportTableBody');

        if (!orders || orders.length === 0) {
            $tbody.html('<tr><td colspan=\"9\" class=\"text-center text-muted py-3\">Không có dữ liệu phù hợp</td></tr>');
            return;
        }

        var rows = orders.map(function(order) {
            var grossMarginDisplay = self.formatNumber(order.grossMargin) + ' %';
            var methodDisplay = order.cogsCalculationMethod || '-';
            return '<tr>' +
                '<td>' + (order.orderNumber || '-') + '</td>' +
                '<td>' + (order.customerName || '-') + '</td>' +
                '<td>' + (self.formatDate(order.completedDate) || '-') + '</td>' +
                '<td class=\"text-right\">' + self.formatCurrency(order.totalRevenue) + '</td>' +
                '<td class=\"text-right\">' + self.formatCurrency(order.totalCogs) + '</td>' +
                '<td class=\"text-right\">' + self.formatCurrency(order.grossProfit) + '</td>' +
                '<td class=\"text-right\">' + grossMarginDisplay + '</td>' +
                '<td>' + methodDisplay + '</td>' +
                '<td>' + (order.cogsCalculationDate ? new Date(order.cogsCalculationDate).toLocaleString('vi-VN') : '-') + '</td>' +
            '</tr>';
        }).join('');

        $tbody.html(rows);
    },

    exportReport: function() {
        var filters = this.getFilters();
        var url = '/OrderManagement/ExportCogsSummary' + this.buildQuery(filters);
        window.location.href = url;
    },

    exportReportExcel: function() {
        var filters = this.getFilters();
        var url = '/OrderManagement/ExportCogsSummaryExcel' + this.buildQuery(filters);
        window.location.href = url;
    },

    init: function() {
        var today = new Date();
        var startDate = new Date();
        startDate.setDate(today.getDate() - 30);

        $('#cogsStartDate').val(startDate.toISOString().substring(0, 10));
        $('#cogsEndDate').val(today.toISOString().substring(0, 10));

        $('#btnFilterCogs').on('click', this.loadReport.bind(this));
        $('#btnExportCogs').on('click', this.exportReport.bind(this));
        $('#btnExportCogsExcel').on('click', this.exportReportExcel.bind(this));

        this.loadReport();
    }
};

$(document).ready(function() {
    CogsReport.init();
});

