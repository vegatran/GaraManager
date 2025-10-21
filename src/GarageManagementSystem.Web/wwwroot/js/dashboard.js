/**
 * Dashboard Module
 * 
 * Handles dashboard data loading and visualization
 */

window.Dashboard = {
    // Initialize dashboard
    init: function() {
        this.loadStatistics();
        this.loadRecentOrders();
        this.loadUpcomingAppointments();
        this.loadServiceStatistics();
        this.loadSystemInformation();
        this.loadInventoryAlerts();
    },

    // Load statistics
    loadStatistics: function() {
        $.ajax({
            url: '/Home/GetDashboardStatistics',
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    Dashboard.updateStatistics(response.data);
                }
            },
            error: function(xhr, status, error) {
                console.log('Error loading statistics:', error);
            }
        });
    },

    // Update statistics display
    updateStatistics: function(data) {
        $('#totalCustomers').text(data.totalCustomers || 0);
        $('#totalVehicles').text(data.totalVehicles || 0);
        $('#pendingOrders').text(data.pendingOrders || 0);
        $('#todayAppointments').text(data.todayAppointments || 0);
        $('#monthlyRevenue').text('₫' + (data.monthlyRevenue || 0).toLocaleString());
    },

    // Load recent orders
    loadRecentOrders: function() {
        $.ajax({
            url: '/Home/GetRecentOrders',
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    Dashboard.displayRecentOrders(response.data);
                }
            },
            error: function(xhr, status, error) {
                console.log('Error loading recent orders:', error);
            }
        });
    },

    // Display recent orders
    displayRecentOrders: function(orders) {
        var html = '';
        if (orders && orders.length > 0) {
            orders.forEach(function(order) {
                html += `
                    <tr>
                        <td>${order.orderNumber}</td>
                        <td>${order.customerName}</td>
                        <td>${order.orderDate}</td>
                        <td>₫${order.totalAmount.toLocaleString()}</td>
                        <td><span class="badge badge-${order.statusClass}">${order.status}</span></td>
                    </tr>
                `;
            });
        } else {
            html = '<tr><td colspan="5" class="text-center">Không có đơn hàng gần đây</td></tr>';
        }
        $('#recentOrdersTable tbody').html(html);
    },

    // Load upcoming appointments
    loadUpcomingAppointments: function() {
        $.ajax({
            url: '/Home/GetUpcomingAppointments',
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    Dashboard.displayUpcomingAppointments(response.data);
                }
            },
            error: function(xhr, status, error) {
                console.log('Error loading upcoming appointments:', error);
            }
        });
    },

    // Display upcoming appointments
    displayUpcomingAppointments: function(appointments) {
        var html = '';
        if (appointments && appointments.length > 0) {
            appointments.forEach(function(appointment) {
                html += `
                    <tr>
                        <td>${appointment.customerName}</td>
                        <td>${appointment.vehicleLicensePlate}</td>
                        <td>${appointment.appointmentDate}</td>
                        <td>${appointment.appointmentTime}</td>
                        <td>${appointment.serviceType}</td>
                    </tr>
                `;
            });
        } else {
            html = '<tr><td colspan="5" class="text-center">Không có lịch hẹn sắp tới</td></tr>';
        }
        $('#upcomingAppointmentsTable tbody').html(html);
    },

    // Load service statistics
    loadServiceStatistics: function() {
        // Tạm thời hiển thị dữ liệu mẫu
        var serviceStatsHtml = `
            <div class="row">
                <div class="col-md-6">
                    <h5>Dịch vụ phổ biến</h5>
                    <ul class="list-group">
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            Bảo dưỡng định kỳ
                            <span class="badge badge-primary badge-pill">15</span>
                        </li>
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            Sửa chữa động cơ
                            <span class="badge badge-primary badge-pill">8</span>
                        </li>
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            Thay phụ tùng
                            <span class="badge badge-primary badge-pill">12</span>
                        </li>
                    </ul>
                </div>
                <div class="col-md-6">
                    <h5>Doanh thu tháng</h5>
                    <div class="progress mb-2">
                        <div class="progress-bar bg-success" role="progressbar" style="width: 75%">75%</div>
                    </div>
                    <small class="text-muted">₫45,000,000 / ₫60,000,000</small>
                </div>
            </div>
        `;
        $('#serviceStatisticsContent').html(serviceStatsHtml);
    },

    // Load system information
    loadSystemInformation: function() {
        // Tạm thời hiển thị dữ liệu mẫu
        var systemInfoHtml = `
            <div class="row">
                <div class="col-md-4 text-center">
                    <div class="info-box">
                        <span class="info-box-icon bg-info"><i class="fas fa-users"></i></span>
                        <div class="info-box-content">
                            <span class="info-box-text">Khách hàng</span>
                            <span class="info-box-number">4</span>
                        </div>
                    </div>
                </div>
                <div class="col-md-4 text-center">
                    <div class="info-box">
                        <span class="info-box-icon bg-success"><i class="fas fa-car"></i></span>
                        <div class="info-box-content">
                            <span class="info-box-text">Xe</span>
                            <span class="info-box-number">4</span>
                        </div>
                    </div>
                </div>
                <div class="col-md-4 text-center">
                    <div class="info-box">
                        <span class="info-box-icon bg-warning"><i class="fas fa-wrench"></i></span>
                        <div class="info-box-content">
                            <span class="info-box-text">Dịch vụ</span>
                            <span class="info-box-number">8</span>
                        </div>
                    </div>
                </div>
            </div>
        `;
        $('#systemInformationContent').html(systemInfoHtml);
    },

    // Load inventory alerts
    loadInventoryAlerts: function() {
        var self = this;
        
        // Load low stock
        $.ajax({
            url: '/api/inventory-alerts/low-stock',
            type: 'GET',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('access_token')
            },
            success: function(response) {
                if (response.success && response.data) {
                    var count = response.data.length;
                    $('#lowStockCount').text(count);
                    $('#lowStockNumber').text(count);
                    
                    // Auto-expand if there are alerts
                    if (count > 0) {
                        $('.card-warning.collapsed-card').removeClass('collapsed-card');
                        $('.card-warning .card-body').show();
                        $('.card-warning .card-header .btn-tool i').removeClass('fa-plus').addClass('fa-minus');
                        
                        // Show table if items exist
                        if (count <= 10) {
                            $('#lowStockTable').show();
                            self.displayLowStockItems(response.data);
                        }
                    }
                }
            },
            error: function() {
                console.log('Error loading low stock alerts');
            }
        });
        
        // Load out of stock
        $.ajax({
            url: '/api/inventory-alerts/out-of-stock',
            type: 'GET',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('access_token')
            },
            success: function(response) {
                if (response.success && response.data) {
                    $('#outOfStockNumber').text(response.data.length);
                }
            },
            error: function() {
                console.log('Error loading out of stock alerts');
            }
        });
        
        // Load reorder suggestions
        $.ajax({
            url: '/api/inventory-alerts/reorder-suggestions',
            type: 'GET',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('access_token')
            },
            success: function(response) {
                if (response.success && response.data) {
                    $('#reorderNumber').text(response.data.length);
                }
            },
            error: function() {
                console.log('Error loading reorder suggestions');
            }
        });
    },

    // Display low stock items in table
    displayLowStockItems: function(items) {
        var tbody = $('#lowStockTableBody');
        tbody.empty();
        
        items.slice(0, 10).forEach(function(item) {
            var row = `
                <tr>
                    <td>${item.partNumber || 'N/A'}</td>
                    <td>${item.partName}</td>
                    <td><span class="badge badge-warning">${item.currentStock}</span></td>
                    <td><span class="badge badge-info">${item.minimumStock}</span></td>
                    <td>
                        <a href="/PartsManagement/Index" class="btn btn-xs btn-primary">
                            <i class="fas fa-edit"></i> Quản lý
                        </a>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });
    }
};

// Initialize dashboard when document is ready
$(document).ready(function() {
    Dashboard.init();
});