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
        console.log('Dashboard data received:', data);
        
        // Map data to view elements
        // bg-info = Khách hàng (Customers)
        // bg-success = Xe (Vehicles)  
        // bg-warning = Dịch vụ (Services) - should be service count but API returns newOrders
        // bg-danger = Đơn hàng (Orders) - should be active/pending orders
        
        // Get counts in order
        var customerCount = data.totalCustomers || 0;
        var vehicleCount = data.totalVehicles || 0;
        var serviceCount = data.completedOrders || data.newOrders || 0; // Fallback to newOrders
        var orderCount = data.pendingOrders || 0;
        
        console.log('Updating cards:', {customerCount, vehicleCount, serviceCount, orderCount});
        
        // Update the small boxes (stat cards)
        $('.small-box.bg-info h3').text(customerCount);
        $('.small-box.bg-success h3').text(vehicleCount);
        $('.small-box.bg-warning h3').text(serviceCount);
        $('.small-box.bg-danger h3').text(orderCount);
        
        // Update other elements if they exist
        $('#totalCustomers').text(customerCount);
        $('#totalVehicles').text(vehicleCount);
        $('#pendingOrders').text(orderCount);
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
        // Load real data and create charts
        $.ajax({
            url: '/Home/GetDashboardStatistics',
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var data = response.data;
                    
                    // Create revenue chart
                    var revenueCtx = document.getElementById('revenue-chart-canvas').getContext('2d');
                    new Chart(revenueCtx, {
                        type: 'line',
                        data: {
                            labels: ['Tháng trước', 'Tháng này'],
                            datasets: [{
                                label: 'Doanh thu (VNĐ)',
                                data: [data.lastMonthRevenue || 0, data.monthlyRevenue || 0],
                                borderColor: '#28a745',
                                backgroundColor: 'rgba(40, 167, 69, 0.1)',
                                tension: 0.4
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {
                                y: {
                                    beginAtZero: true,
                                    ticks: {
                                        callback: function(value) {
                                            return value.toLocaleString() + ' VNĐ';
                                        }
                                    }
                                }
                            },
                            plugins: {
                                legend: {
                                    display: false
                                }
                            }
                        }
                    });
                    
                    // Create sales chart
                    var salesCtx = document.getElementById('sales-chart-canvas').getContext('2d');
                    new Chart(salesCtx, {
                        type: 'doughnut',
                        data: {
                            labels: ['Hoàn thành', 'Đang xử lý', 'Chờ duyệt'],
                            datasets: [{
                                data: [
                                    data.completedOrders || 0,
                                    data.inProgressOrders || 0,
                                    data.pendingOrders || 0
                                ],
                                backgroundColor: ['#28a745', '#ffc107', '#17a2b8'],
                                borderWidth: 2
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
                }
            },
            error: function() {
                console.log('Error loading service statistics');
            }
        });
    },

    // Load system information
    loadSystemInformation: function() {
        // Load real data from dashboard statistics
        $.ajax({
            url: '/Home/GetDashboardStatistics',
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var data = response.data;
                    
                    // Update sparklines with real data (using Chart.js instead)
                    $('#sparkline-1').html(`<canvas width="40" height="40"></canvas>`);
                    $('#sparkline-2').html(`<canvas width="40" height="40"></canvas>`);
                    $('#sparkline-3').html(`<canvas width="40" height="40"></canvas>`);
                    
                    // Create mini charts for sparklines
                    new Chart($('#sparkline-1 canvas')[0], {
                        type: 'bar',
                        data: {
                            labels: [''],
                            datasets: [{
                                data: [data.totalCustomers],
                                backgroundColor: '#17a2b8',
                                borderWidth: 0
                            }]
                        },
                        options: {
                            responsive: false,
                            maintainAspectRatio: false,
                            plugins: { legend: { display: false } },
                            scales: { x: { display: false }, y: { display: false } }
                        }
                    });
                    
                    new Chart($('#sparkline-2 canvas')[0], {
                        type: 'bar',
                        data: {
                            labels: [''],
                            datasets: [{
                                data: [data.totalVehicles],
                                backgroundColor: '#28a745',
                                borderWidth: 0
                            }]
                        },
                        options: {
                            responsive: false,
                            maintainAspectRatio: false,
                            plugins: { legend: { display: false } },
                            scales: { x: { display: false }, y: { display: false } }
                        }
                    });
                    
                    new Chart($('#sparkline-3 canvas')[0], {
                        type: 'bar',
                        data: {
                            labels: [''],
                            datasets: [{
                                data: [data.pendingOrders],
                                backgroundColor: '#ffc107',
                                borderWidth: 0
                            }]
                        },
                        options: {
                            responsive: false,
                            maintainAspectRatio: false,
                            plugins: { legend: { display: false } },
                            scales: { x: { display: false }, y: { display: false } }
                        }
                    });
                    
                    // Update world map area with summary info
                    var mapContent = `
                        <div class="text-center text-white p-4">
                            <h4><i class="fas fa-chart-line"></i> Tổng Quan Hệ Thống</h4>
                            <div class="row mt-3">
                                <div class="col-4">
                                    <div class="info-box bg-info-transparent">
                                        <span class="info-box-icon"><i class="fas fa-users"></i></span>
                                        <div class="info-box-content">
                                            <span class="info-box-text">Khách hàng</span>
                                            <span class="info-box-number">${data.totalCustomers}</span>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-4">
                                    <div class="info-box bg-success-transparent">
                                        <span class="info-box-icon"><i class="fas fa-car"></i></span>
                                        <div class="info-box-content">
                                            <span class="info-box-text">Xe</span>
                                            <span class="info-box-number">${data.totalVehicles}</span>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-4">
                                    <div class="info-box bg-warning-transparent">
                                        <span class="info-box-icon"><i class="fas fa-wrench"></i></span>
                                        <div class="info-box-content">
                                            <span class="info-box-text">Đơn hàng</span>
                                            <span class="info-box-number">${data.pendingOrders}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                    $('#world-map').html(mapContent);
                }
            },
            error: function() {
                console.log('Error loading system information');
            }
        });
    },

    // Load inventory alerts
    loadInventoryAlerts: function() {
        var self = this;
        
        // Load low stock
        $.ajax({
            url: '/Home/GetLowStockAlerts',
            type: 'GET',
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
            url: '/Home/GetOutOfStockAlerts',
            type: 'GET',
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
            url: '/Home/GetReorderSuggestions',
            type: 'GET',
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