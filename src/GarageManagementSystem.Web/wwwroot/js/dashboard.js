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
            }
        });
    },

    // Update statistics display
    updateStatistics: function(data) {
        $('#totalCustomers').text(data.totalCustomers || 0);
        $('#totalVehicles').text(data.totalVehicles || 0);
        $('#pendingOrders').text(data.pendingOrders || 0);
        $('#todayAppointments').text(data.todayAppointments || 0);
        $('#monthlyRevenue').text('$' + (data.monthlyRevenue || 0).toFixed(2));
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
                        <td>$${order.totalAmount}</td>
                        <td><span class="badge badge-${order.statusClass}">${order.status}</span></td>
                    </tr>
                `;
            });
        } else {
            html = '<tr><td colspan="5" class="text-center">No recent orders</td></tr>';
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
            html = '<tr><td colspan="5" class="text-center">No upcoming appointments</td></tr>';
        }
        $('#upcomingAppointmentsTable tbody').html(html);
    }
};

// Initialize dashboard when document is ready
$(document).ready(function() {
    if ($('#dashboardPage').length > 0) {
        Dashboard.init();
    }
});