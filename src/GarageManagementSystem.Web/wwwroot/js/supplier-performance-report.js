/**
 * Supplier Performance Report Module - Phase 4.2.4
 * 
 * Handles supplier performance reporting, ranking, and alerts
 */

window.SupplierPerformanceReport = {
    performanceTable: null,
    rankingTable: null,
    topSuppliersChart: null,
    performanceDistributionChart: null,
    suppliers: [],
    currentPage: 1,
    pageSize: 20,
    cache: new Map(), // Cache for performance data
    searchTimeout: null, // For debouncing
    initialized: false,

    init: function() {
        // ✅ SAFETY: Prevent multiple initialization
        if (this.initialized) {
            return;
        }
        this.initialized = true;

        this.initDataTables();
        this.bindEvents();
        this.loadSuppliers();
        this.loadRanking();
        this.loadPerformanceReport();
        this.loadPerformanceAlerts();
    },

    initDataTables: function() {
        var self = this;
        
        // Performance Report Table
        this.performanceTable = $('#performanceTable').DataTable({
            processing: true,
            serverSide: false,
            paging: true,
            searching: false,
            ordering: true,
            info: true,
            autoWidth: false,
            pageLength: self.pageSize,
            deferRender: true,
            language: {
                url: '//cdn.datatables.net/plug-ins/1.11.5/i18n/vi.json'
            },
            columns: [
                { data: 'supplierName', title: 'Nhà Cung Cấp', width: '15%' },
                { data: 'totalOrders', title: 'Tổng Đơn', width: '8%' },
                { data: 'onTimeDeliveries', title: 'Giao Đúng Hạn', width: '10%' },
                { 
                    data: 'onTimeDeliveryRate', 
                    title: 'Tỷ Lệ Đúng Hạn', 
                    width: '10%',
                    render: function(data) {
                        return data ? data.toFixed(1) + '%' : '-';
                    }
                },
                { 
                    data: 'averageLeadTimeDays', 
                    title: 'Thời Gian Giao TB', 
                    width: '10%',
                    render: function(data) {
                        return data ? data + ' ngày' : '-';
                    }
                },
                { 
                    data: 'defectRate', 
                    title: 'Tỷ Lệ Lỗi', 
                    width: '8%',
                    render: function(data) {
                        return data ? data.toFixed(1) + '%' : '-';
                    }
                },
                { 
                    data: 'averagePrice', 
                    title: 'Giá TB', 
                    width: '10%',
                    render: function(data) {
                        return data ? data.toLocaleString('vi-VN') + ' VNĐ' : '-';
                    }
                },
                { 
                    data: 'priceStability', 
                    title: 'Ổn Định Giá', 
                    width: '10%',
                    render: function(data) {
                        return data ? data.toFixed(1) + '%' : '-';
                    }
                },
                { 
                    data: 'overallScore', 
                    title: 'Điểm Tổng Thể', 
                    width: '10%',
                    render: function(data, type, row) {
                        return self.renderScore(data);
                    }
                },
                { 
                    data: 'calculatedAt', 
                    title: 'Ngày Tính', 
                    width: '9%',
                    render: function(data) {
                        return data ? new Date(data).toLocaleDateString('vi-VN') : '-';
                    }
                }
            ],
            order: [[8, 'desc']] // Sort by OverallScore
        });

        // Ranking Table
        this.rankingTable = $('#rankingTable').DataTable({
            processing: true,
            serverSide: false,
            paging: false,
            searching: false,
            ordering: false,
            info: false,
            autoWidth: false,
            deferRender: true,
            language: {
                url: '//cdn.datatables.net/plug-ins/1.11.5/i18n/vi.json'
            },
            columns: [
                { data: 'rank', title: 'Hạng', width: '5%' },
                { data: 'supplierName', title: 'Nhà Cung Cấp', width: '20%' },
                { 
                    data: 'overallScore', 
                    title: 'Điểm Tổng Thể', 
                    width: '10%',
                    render: function(data, type, row) {
                        return self.renderScore(data);
                    }
                },
                { 
                    data: 'onTimeDeliveryRate', 
                    title: 'Giao Hàng Đúng Hạn', 
                    width: '10%',
                    render: function(data) {
                        return data ? data.toFixed(1) + '%' : '-';
                    }
                },
                { 
                    data: 'defectRate', 
                    title: 'Tỷ Lệ Lỗi', 
                    width: '10%',
                    render: function(data) {
                        return data ? data.toFixed(1) + '%' : '-';
                    }
                },
                { 
                    data: 'averagePrice', 
                    title: 'Giá Trung Bình', 
                    width: '12%',
                    render: function(data) {
                        return data ? data.toLocaleString('vi-VN') + ' VNĐ' : '-';
                    }
                },
                { 
                    data: 'averageLeadTimeDays', 
                    title: 'Thời Gian Giao', 
                    width: '10%',
                    render: function(data) {
                        return data ? data + ' ngày' : '-';
                    }
                },
                { data: 'totalOrders', title: 'Tổng Đơn', width: '8%' },
                { 
                    data: 'performanceCategory', 
                    title: 'Phân Loại', 
                    width: '15%',
                    render: function(data) {
                        return self.renderCategory(data);
                    }
                }
            ]
        });
    },

    bindEvents: function() {
        var self = this;

        // Search button
        $('#btnSearch').on('click', function() {
            self.currentPage = 1;
            self.loadPerformanceReport();
        });

        // Calculate button
        $('#btnCalculate').on('click', function() {
            self.calculatePerformance();
        });

        // Refresh button
        $('#btnRefresh').on('click', function() {
            self.cache.clear();
            self.loadRanking();
            self.loadPerformanceReport();
            self.loadPerformanceAlerts();
        });

        // Sort buttons
        $('#btnSortByScore').on('click', function() {
            self.loadRanking('OverallScore', null, false);
        });

        $('#btnSortByOnTime').on('click', function() {
            self.loadRanking('OnTimeDelivery', null, false);
        });

        $('#btnSortByDefect').on('click', function() {
            self.loadRanking('DefectRate', null, false);
        });

        // ✅ SAFETY: Clear search timeout on page unload
        $(window).on('beforeunload', function() {
            if (self.searchTimeout) {
                clearTimeout(self.searchTimeout);
                self.searchTimeout = null;
            }
        });
    },

    loadSuppliers: function() {
        var self = this;
        
        $.ajax({
            url: '/Procurement/GetSuppliers',
            type: 'GET'
        })
        .done(function(response) {
            if (Array.isArray(response)) {
                self.suppliers = response;
                
                var supplierSelect = $('#supplierFilter');
                
                // ✅ SAFETY: Destroy existing Select2 before re-init
                if (supplierSelect.hasClass('select2-hidden-accessible')) {
                    supplierSelect.select2('destroy');
                }
                
                supplierSelect.empty().append('<option value="">-- Tất cả --</option>');
                
                self.suppliers.forEach(function(supplier) {
                    if (supplier && supplier.id && supplier.supplierName) {
                        supplierSelect.append(`<option value="${supplier.id}">${supplier.supplierName}</option>`);
                    }
                });
                
                supplierSelect.select2({
                    placeholder: 'Chọn nhà cung cấp',
                    allowClear: true,
                    delay: 250
                });
            }
        })
        .fail(function(xhr) {
            console.error('Error loading suppliers:', xhr);
        });
    },

    loadPerformanceReport: function() {
        var self = this;
        
        var supplierId = $('#supplierFilter').val() || null;
        var startDate = $('#startDateFilter').val() || null;
        var endDate = $('#endDateFilter').val() || null;

        GarageApp.showLoading('Đang tải báo cáo...');

        $.ajax({
            url: '/Procurement/GetSupplierPerformanceReport',
            type: 'GET',
            data: {
                pageNumber: self.currentPage,
                pageSize: self.pageSize,
                supplierId: supplierId,
                startDate: startDate,
                endDate: endDate
            }
        })
        .done(function(response) {
            GarageApp.hideLoading();
            
            if (response && (response.Success || response.success) && (response.Data || response.data)) {
                var data = response.Data || response.data;
                
                self.performanceTable.clear();
                if (Array.isArray(data) && data.length > 0) {
                    self.performanceTable.rows.add(data);
                }
                self.performanceTable.draw();
                
                // Update summary cards
                self.updateSummaryCards(data);
            } else {
                var message = response.Message || response.message || 'Không có dữ liệu.';
                GarageApp.showWarning(message);
                self.performanceTable.clear().draw();
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (xhr.status === 401) {
                AuthHandler.handleUnauthorized(xhr, true);
            } else {
                var errorMsg = xhr.responseJSON?.errorMessage || xhr.responseJSON?.message || 'Lỗi khi tải báo cáo.';
                GarageApp.showError(errorMsg);
            }
        });
    },

    loadRanking: function(sortBy, topN, worstPerformers) {
        var self = this;
        
        sortBy = sortBy || 'OverallScore';
        worstPerformers = worstPerformers || false;

        GarageApp.showLoading('Đang tải ranking...');

        $.ajax({
            url: '/Procurement/GetSupplierRanking',
            type: 'GET',
            data: {
                sortBy: sortBy,
                topN: topN || 10,
                worstPerformers: worstPerformers
            }
        })
        .done(function(response) {
            GarageApp.hideLoading();
            
            if (response && (response.Success || response.success) && (response.Data || response.data)) {
                var rankings = response.Data || response.data;
                
                self.rankingTable.clear();
                if (Array.isArray(rankings) && rankings.length > 0) {
                    self.rankingTable.rows.add(rankings);
                    self.rankingTable.draw();
                    
                    // Update charts
                    self.updateTopSuppliersChart(rankings);
                    self.updatePerformanceDistributionChart(rankings);
                } else {
                    self.rankingTable.clear().draw();
                }
            } else {
                var errorMsg = response.ErrorMessage || response.errorMessage || 'Không có dữ liệu ranking.';
                GarageApp.showWarning(errorMsg);
                self.rankingTable.clear().draw();
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (xhr.status === 401) {
                AuthHandler.handleUnauthorized(xhr, true);
            } else {
                var errorMsg = xhr.responseJSON?.errorMessage || xhr.responseJSON?.message || 'Lỗi khi tải ranking.';
                GarageApp.showError(errorMsg);
            }
        });
    },

    loadPerformanceAlerts: function() {
        var self = this;
        
        $.ajax({
            url: '/Procurement/GetPerformanceAlerts',
            type: 'GET'
        })
        .done(function(response) {
            if (response && (response.Success || response.success) && (response.Data || response.data)) {
                var alerts = response.Data || response.data;
                self.renderAlerts(alerts);
            }
        })
        .fail(function(xhr) {
            if (xhr.status === 401) {
                AuthHandler.handleUnauthorized(xhr, true);
            } else {
                console.error('Error loading performance alerts:', xhr);
            }
        });
    },

    calculatePerformance: function() {
        var self = this;
        
        if (!confirm('Bạn có chắc chắn muốn tính toán lại hiệu suất nhà cung cấp? Quá trình này có thể mất vài phút.')) {
            return;
        }

        GarageApp.showLoading('Đang tính toán hiệu suất...');

        $.ajax({
            url: '/Procurement/CalculatePerformance',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({}),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        })
        .done(function(response) {
            GarageApp.hideLoading();
            
            if (response && (response.Success || response.success)) {
                GarageApp.showSuccess('Tính toán hiệu suất thành công.');
                self.cache.clear();
                self.loadRanking();
                self.loadPerformanceReport();
                self.loadPerformanceAlerts();
            } else {
                var errorMsg = response.ErrorMessage || response.errorMessage || 'Lỗi khi tính toán hiệu suất.';
                GarageApp.showError(errorMsg);
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (xhr.status === 401) {
                AuthHandler.handleUnauthorized(xhr, true);
            } else {
                var errorMsg = xhr.responseJSON?.errorMessage || xhr.responseJSON?.message || 'Lỗi khi tính toán hiệu suất.';
                GarageApp.showError(errorMsg);
            }
        });
    },

    updateSummaryCards: function(data) {
        if (!Array.isArray(data)) return;

        var excellent = data.filter(d => d.overallScore >= 80).length;
        var good = data.filter(d => d.overallScore >= 60 && d.overallScore < 80).length;
        var average = data.filter(d => d.overallScore >= 40 && d.overallScore < 60).length;
        var poor = data.filter(d => d.overallScore < 40).length;

        $('#excellentCount').text(excellent);
        $('#goodCount').text(good);
        $('#averageCount').text(average);
        $('#poorCount').text(poor);
    },

    updateTopSuppliersChart: function(rankings) {
        var self = this;
        
        if (!Array.isArray(rankings) || rankings.length === 0) return;

        var top10 = rankings.slice(0, 10);
        var labels = top10.map(r => r.supplierName);
        var scores = top10.map(r => r.overallScore);

        var ctx = document.getElementById('topSuppliersChart');
        if (!ctx) return;

        // Destroy existing chart if exists
        if (self.topSuppliersChart) {
            self.topSuppliersChart.destroy();
        }

        self.topSuppliersChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Điểm Tổng Thể',
                    data: scores,
                    backgroundColor: scores.map(s => 
                        s >= 80 ? 'rgba(40, 167, 69, 0.8)' :
                        s >= 60 ? 'rgba(23, 162, 184, 0.8)' :
                        s >= 40 ? 'rgba(255, 193, 7, 0.8)' : 'rgba(220, 53, 69, 0.8)'
                    ),
                    borderColor: scores.map(s => 
                        s >= 80 ? 'rgba(40, 167, 69, 1)' :
                        s >= 60 ? 'rgba(23, 162, 184, 1)' :
                        s >= 40 ? 'rgba(255, 193, 7, 1)' : 'rgba(220, 53, 69, 1)'
                    ),
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 100
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    }
                }
            }
        });
    },

    updatePerformanceDistributionChart: function(rankings) {
        var self = this;
        
        if (!Array.isArray(rankings) || rankings.length === 0) return;

        var excellent = rankings.filter(r => r.performanceCategory === 'Excellent').length;
        var good = rankings.filter(r => r.performanceCategory === 'Good').length;
        var average = rankings.filter(r => r.performanceCategory === 'Average').length;
        var poor = rankings.filter(r => r.performanceCategory === 'Poor').length;

        var ctx = document.getElementById('performanceDistributionChart');
        if (!ctx) return;

        // Destroy existing chart if exists
        if (self.performanceDistributionChart) {
            self.performanceDistributionChart.destroy();
        }

        self.performanceDistributionChart = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: ['Xuất Sắc', 'Tốt', 'Trung Bình', 'Kém'],
                datasets: [{
                    data: [excellent, good, average, poor],
                    backgroundColor: [
                        'rgba(40, 167, 69, 0.8)',
                        'rgba(23, 162, 184, 0.8)',
                        'rgba(255, 193, 7, 0.8)',
                        'rgba(220, 53, 69, 0.8)'
                    ],
                    borderColor: [
                        'rgba(40, 167, 69, 1)',
                        'rgba(23, 162, 184, 1)',
                        'rgba(255, 193, 7, 1)',
                        'rgba(220, 53, 69, 1)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    },

    renderAlerts: function(alerts) {
        if (!Array.isArray(alerts) || alerts.length === 0) {
            $('#alertsContainer').html('<p class="text-muted">Không có cảnh báo nào.</p>');
            return;
        }

        var html = '<div class="table-responsive"><table class="table table-sm">';
        html += '<thead><tr><th>Nhà Cung Cấp</th><th>Loại Cảnh Báo</th><th>Thông Điệp</th><th>Mức Độ</th><th>Ngày</th></tr></thead><tbody>';

        alerts.forEach(function(alert) {
            var severityClass = alert.severity === 'High' ? 'danger' : 
                               alert.severity === 'Medium' ? 'warning' : 'info';
            
            html += `<tr>
                <td>${alert.supplierName || 'N/A'}</td>
                <td>${alert.alertType || '-'}</td>
                <td>${alert.alertMessage || '-'}</td>
                <td><span class="badge badge-${severityClass}">${alert.severity || '-'}</span></td>
                <td>${alert.alertDate ? new Date(alert.alertDate).toLocaleDateString('vi-VN') : '-'}</td>
            </tr>`;
        });

        html += '</tbody></table></div>';
        $('#alertsContainer').html(html);
    },

    renderScore: function(score) {
        if (score === null || score === undefined) return '-';
        
        var badgeClass = 'badge-secondary';
        if (score >= 80) badgeClass = 'badge-success';
        else if (score >= 60) badgeClass = 'badge-info';
        else if (score >= 40) badgeClass = 'badge-warning';
        else badgeClass = 'badge-danger';
        
        return `<span class="badge ${badgeClass}">${score.toFixed(1)}</span>`;
    },

    renderCategory: function(category) {
        if (!category) return '-';
        
        var badgeClass = 'badge-secondary';
        var text = category;
        
        switch(category) {
            case 'Excellent':
                badgeClass = 'badge-success';
                text = 'Xuất Sắc';
                break;
            case 'Good':
                badgeClass = 'badge-info';
                text = 'Tốt';
                break;
            case 'Average':
                badgeClass = 'badge-warning';
                text = 'Trung Bình';
                break;
            case 'Poor':
                badgeClass = 'badge-danger';
                text = 'Kém';
                break;
        }
        
        return `<span class="badge ${badgeClass}">${text}</span>`;
    }
};

// Initialize on page load
$(document).ready(function() {
    if ($('#performanceTable').length > 0) {
        SupplierPerformanceReport.init();
    }
});

