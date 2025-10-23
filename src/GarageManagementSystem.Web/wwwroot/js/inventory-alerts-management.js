// Inventory Alerts Management Module
window.InventoryAlertsManagement = {
    alertsTable: null,
    currentResolveId: null,

    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.loadSummaryCards();
    },

    initDataTable: function() {
        var self = this;
        
        var columns = [
            { data: 'id', title: 'ID', width: '5%' },
            { 
                data: 'partName', 
                title: 'Phụ Tùng', 
                width: '15%',
                render: function(data, type, row) {
                    return `<strong>${data}</strong><br><small class="text-muted">${row.partNumber}</small>`;
                }
            },
            { 
                data: 'alertType', 
                title: 'Loại Cảnh Báo', 
                width: '12%',
                render: function(data, type, row) {
                    var badgeClass = '';
                    var text = '';
                    switch(data) {
                        case 'LowStock':
                            badgeClass = 'badge-warning';
                            text = 'Tồn Kho Thấp';
                            break;
                        case 'OutOfStock':
                            badgeClass = 'badge-danger';
                            text = 'Hết Hàng';
                            break;
                        case 'Expired':
                            badgeClass = 'badge-dark';
                            text = 'Hết Hạn';
                            break;
                        case 'NearExpiry':
                            badgeClass = 'badge-info';
                            text = 'Sắp Hết Hạn';
                            break;
                        default:
                            badgeClass = 'badge-secondary';
                            text = data;
                    }
                    return `<span class="badge ${badgeClass}">${text}</span>`;
                }
            },
            { 
                data: 'severity', 
                title: 'Mức Độ', 
                width: '10%',
                render: function(data, type, row) {
                    var badgeClass = '';
                    var text = '';
                    switch(data) {
                        case 'Critical':
                            badgeClass = 'badge-danger';
                            text = 'Nghiêm Trọng';
                            break;
                        case 'High':
                            badgeClass = 'badge-warning';
                            text = 'Cao';
                            break;
                        case 'Medium':
                            badgeClass = 'badge-info';
                            text = 'Trung Bình';
                            break;
                        case 'Low':
                            badgeClass = 'badge-success';
                            text = 'Thấp';
                            break;
                        default:
                            badgeClass = 'badge-secondary';
                            text = data;
                    }
                    return `<span class="badge ${badgeClass}">${text}</span>`;
                }
            },
            { data: 'message', title: 'Thông Báo', width: '20%' },
            { 
                data: 'currentQuantity', 
                title: 'Số Lượng', 
                width: '8%',
                render: function(data, type, row) {
                    var html = `<strong>${data}</strong>`;
                    if (row.minimumQuantity) {
                        html += `<br><small class="text-muted">Min: ${row.minimumQuantity}</small>`;
                    }
                    return html;
                }
            },
            { 
                data: 'alertDate', 
                title: 'Ngày Cảnh Báo', 
                width: '10%',
                render: DataTablesUtility.renderDate
            },
            { 
                data: 'isResolved', 
                title: 'Trạng Thái', 
                width: '8%',
                render: function(data, type, row) {
                    if (data) {
                        return `<span class="badge badge-success">Đã Xử Lý</span>`;
                    } else {
                        return `<span class="badge badge-warning">Chưa Xử Lý</span>`;
                    }
                }
            },
            {
                data: null,
                title: 'Thao Tác',
                width: '12%',
                orderable: false,
                render: function(data, type, row) {
                    if (row.isResolved) {
                        return `<span class="text-muted">Đã xử lý</span>`;
                    } else {
                        return `
                            <button class="btn btn-success btn-sm resolve-alert" data-id="${row.id}" title="Đánh dấu đã xử lý">
                                <i class="fas fa-check"></i>
                            </button>
                        `;
                    }
                }
            }
        ];
        
        this.alertsTable = DataTablesUtility.initServerSideTable('#alertsTable', '/InventoryAlerts/GetAlerts', columns, {
            order: [[0, 'desc']],
            pageLength: 10,
            processing: true,
            language: {
                processing: "Đang tải dữ liệu..."
            },
            ajax: {
                data: function(d) {
                    d.alertType = $('#alertTypeFilter').val();
                    d.severity = $('#severityFilter').val();
                    d.status = $('#statusFilter').val();
                }
            }
        });
    },

    bindEvents: function() {
        var self = this;

        // Filter events
        $('#alertTypeFilter, #severityFilter, #statusFilter').on('change', function() {
            self.alertsTable.ajax.reload();
        });

        // Clear filters
        $('#clearFilters').on('click', function() {
            $('#alertTypeFilter, #severityFilter, #statusFilter').val('');
            self.alertsTable.ajax.reload();
        });

        // Mark all as resolved
        $('#markAllResolved').on('click', function() {
            self.markAllAsResolved();
        });

        // Refresh alerts
        $('#refreshAlerts').on('click', function() {
            self.alertsTable.ajax.reload();
            self.loadSummaryCards();
        });

        // Resolve individual alert
        $(document).on('click', '.resolve-alert', function() {
            var alertId = $(this).data('id');
            self.showResolveModal(alertId);
        });

        // Confirm resolve
        $('#confirmResolve').on('click', function() {
            self.resolveAlert();
        });
    },

    loadSummaryCards: function() {
        var self = this;
        
        // This would typically come from a separate API endpoint
        // For now, we'll calculate from the current table data
        $.ajax({
            url: '/InventoryAlerts/GetAlerts?pageSize=1000',
            type: 'GET',
            success: function(response) {
                if (response.data) {
                    var alerts = response.data;
                    var critical = alerts.filter(a => a.severity === 'Critical' && !a.isResolved).length;
                    var high = alerts.filter(a => a.severity === 'High' && !a.isResolved).length;
                    var medium = alerts.filter(a => a.severity === 'Medium' && !a.isResolved).length;
                    var resolved = alerts.filter(a => a.isResolved).length;

                    $('#criticalAlerts').text(critical);
                    $('#highAlerts').text(high);
                    $('#mediumAlerts').text(medium);
                    $('#resolvedAlerts').text(resolved);
                }
            },
            error: function() {
                console.log('Error loading summary cards');
            }
        });
    },

    showResolveModal: function(alertId) {
        this.currentResolveId = alertId;
        $('#resolveAlertId').val(alertId);
        $('#resolutionNotes').val('');
        $('#resolveAlertModal').modal('show');
    },

    resolveAlert: function() {
        var self = this;
        var alertId = this.currentResolveId;
        var notes = $('#resolutionNotes').val();

        if (!alertId) {
            GarageApp.showError('Không tìm thấy ID cảnh báo');
            return;
        }

        $.ajax({
            url: '/InventoryAlerts/MarkAsResolved',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ alertId: alertId, resolutionNotes: notes }),
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess(response.message);
                    $('#resolveAlertModal').modal('hide');
                    self.alertsTable.ajax.reload();
                    self.loadSummaryCards();
                } else {
                    GarageApp.showError(response.message);
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi xử lý cảnh báo');
                }
            }
        });
    },

    markAllAsResolved: function() {
        var self = this;
        var alertType = $('#alertTypeFilter').val();

        Swal.fire({
            title: 'Xác nhận',
            text: 'Bạn có chắc chắn muốn đánh dấu tất cả cảnh báo là đã xử lý?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Xác nhận',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/InventoryAlerts/MarkAllAsResolved',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ alertType: alertType }),
                    success: function(response) {
                        if (response.success) {
                            GarageApp.showSuccess(response.message);
                            self.alertsTable.ajax.reload();
                            self.loadSummaryCards();
                        } else {
                            GarageApp.showError(response.message);
                        }
                    },
                    error: function(xhr) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi xử lý cảnh báo');
                        }
                    }
                });
            }
        });
    }
};

// Initialize when document is ready
$(document).ready(function() {
    InventoryAlertsManagement.init();
});
