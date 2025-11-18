/**
 * Inventory Alerts Badge Management
 * Hiển thị badge count trên menu "Cảnh Báo Tồn Kho"
 * ✅ Real-time: Sử dụng SignalR để push notifications thay vì polling
 */
window.InventoryAlertsBadge = {
    badgeElement: null,
    updateInterval: null,
    updateIntervalMs: 300000, // 5 phút (fallback nếu SignalR fail)
    connection: null,
    isSignalRConnected: false,

    init: function() {
        // ✅ FIX: Tránh init nhiều lần
        if (this.updateInterval !== null || this.connection !== null) {
            console.warn('InventoryAlertsBadge already initialized');
            return;
        }

        this.badgeElement = $('#inventoryAlertsBadge');
        if (!this.badgeElement.length) {
            console.warn('Inventory alerts badge element not found');
            return;
        }

        // Load badge count khi trang load
        this.loadBadgeCount();

        // ✅ Real-time: Initialize SignalR connection
        this.initSignalR();

        // ✅ Fallback: Update badge count định kỳ (nếu SignalR fail)
        var self = this;
        this.updateInterval = setInterval(function() {
            if (!self.isSignalRConnected) {
                self.loadBadgeCount();
            }
        }, this.updateIntervalMs);
    },

    initSignalR: function() {
        var self = this;
        
        // Check if SignalR is available
        if (typeof signalR === 'undefined') {
            console.warn('SignalR not available, using polling fallback');
            return;
        }

        try {
            // Get API base URL from configuration or use default
            var apiBaseUrl = window.apiBaseUrl || 'https://localhost:5001';
            var hubUrl = apiBaseUrl + '/hubs/notifications';

            // Create connection
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(hubUrl, {
                    skipNegotiation: false,
                    transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
                })
                .withAutomaticReconnect({
                    nextRetryDelayInMilliseconds: retryContext => {
                        if (retryContext.elapsedMilliseconds < 60000) {
                            // Retry after 2 seconds for the first minute
                            return 2000;
                        } else {
                            // Retry after 10 seconds after the first minute
                            return 10000;
                        }
                    }
                })
                .build();

            // Handle connection events
            this.connection.onclose(function(error) {
                self.isSignalRConnected = false;
                console.warn('SignalR connection closed', error);
            });

            this.connection.onreconnecting(function(error) {
                self.isSignalRConnected = false;
                console.warn('SignalR reconnecting...', error);
            });

            this.connection.onreconnected(function(connectionId) {
                self.isSignalRConnected = true;
                console.log('SignalR reconnected. Connection ID: ' + connectionId);
                
                // ✅ FIX: Rejoin group after reconnection
                self.connection.invoke('JoinGroup', 'inventory-alerts')
                    .then(function() {
                        console.log('Rejoined inventory-alerts group after reconnection');
                    })
                    .catch(function(err) {
                        console.warn('Error rejoining inventory-alerts group:', err);
                    });
            });

            // ✅ Real-time: Listen for inventory alert count updates
            this.connection.on('InventoryAlertUpdated', function(data) {
                if (data && data.count !== undefined) {
                    self.updateBadge(data.count);
                }
            });

            // ✅ Real-time: Listen for new inventory alerts
            this.connection.on('InventoryAlertCreated', function(data) {
                if (data) {
                    // Reload badge count to get latest
                    self.loadBadgeCount();
                    
                    // Optional: Show browser notification
                    if (typeof Notification !== 'undefined' && Notification.permission === 'granted') {
                        try {
                            new Notification('Cảnh Báo Tồn Kho', {
                                body: data.message || 'Có cảnh báo tồn kho mới',
                                icon: '/lib/adminlte/img/AdminLTELogo.png'
                            });
                        } catch (err) {
                            console.warn('Error showing browser notification:', err);
                        }
                    }
                }
            });

            // ✅ Real-time: Listen for resolved alerts
            this.connection.on('InventoryAlertResolved', function(data) {
                if (data) {
                    // Reload badge count to get latest
                    self.loadBadgeCount();
                }
            });

            // Start connection
            this.connection.start()
                .then(function() {
                    self.isSignalRConnected = true;
                    console.log('SignalR connected for inventory alerts');
                    
                    // Join inventory alerts group (optional)
                    return self.connection.invoke('JoinGroup', 'inventory-alerts');
                })
                .then(function() {
                    console.log('Joined inventory-alerts group');
                })
                .catch(function(err) {
                    self.isSignalRConnected = false;
                    console.error('SignalR connection error:', err);
                    // Fallback to polling
                });
        } catch (error) {
            console.error('Error initializing SignalR:', error);
            // Fallback to polling
        }
    },

    loadBadgeCount: function() {
        var self = this;
        
        $.ajax({
            url: '/InventoryAlerts/GetAlertsCount',
            type: 'GET',
            success: function(response) {
                if (response.success && response.count !== undefined) {
                    var count = response.count || 0;
                    self.updateBadge(count);
                } else {
                    console.warn('Failed to load alerts count:', response);
                    self.updateBadge(0);
                }
            },
            error: function(xhr) {
                // Không log error để tránh spam console
                // Chỉ update badge về 0 nếu có lỗi
                self.updateBadge(0);
            }
        });
    },

    updateBadge: function(count) {
        if (!this.badgeElement.length) {
            return;
        }

        if (count > 0) {
            this.badgeElement.text(count);
            this.badgeElement.show();
            
            // ✅ THÊM: Đảm bảo badge có class danger
            this.badgeElement.addClass('badge-danger').removeClass('badge-secondary');
            
            // ✅ THÊM: Thêm class để highlight badge (AdminLTE có sẵn animation)
            this.badgeElement.addClass('badge-danger');
        } else {
            this.badgeElement.hide();
        }
    },

    destroy: function() {
        if (this.updateInterval) {
            clearInterval(this.updateInterval);
            this.updateInterval = null;
        }
        
        // ✅ Real-time: Stop SignalR connection
        if (this.connection) {
            this.connection.stop()
                .then(function() {
                    console.log('SignalR connection stopped');
                })
                .catch(function(err) {
                    console.error('Error stopping SignalR connection:', err);
                });
            this.connection = null;
        }
        this.isSignalRConnected = false;
    }
};

// ✅ FIX: Cleanup khi trang unload để tránh memory leak
$(window).on('beforeunload', function() {
    if (typeof InventoryAlertsBadge !== 'undefined') {
        InventoryAlertsBadge.destroy();
    }
});

// ✅ THÊM: Auto-init khi document ready (chỉ nếu chưa được init từ _Layout.cshtml)
$(document).ready(function() {
    if (typeof InventoryAlertsBadge !== 'undefined' && InventoryAlertsBadge.updateInterval === null) {
        InventoryAlertsBadge.init();
    }
});

