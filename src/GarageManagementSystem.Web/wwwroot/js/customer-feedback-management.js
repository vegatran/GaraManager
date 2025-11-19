/**
 * Customer Feedback Management Module
 */

window.CustomerFeedbackManagement = {
    feedbackTable: null,

    init: function() {
        this.initDataTable();
        this.bindEvents();
    },

    initDataTable: function() {
        var self = this;

        this.feedbackTable = $('#feedbackTable').DataTable({
            processing: true,
            serverSide: true,
            ajax: {
                url: '/CustomerFeedbackManagement/GetFeedbacks',
                type: 'GET',
                data: function(d) {
                    // ✅ SỬA: Map DataTable parameters correctly
                    return {
                        draw: d.draw,
                        start: d.start,
                        length: d.length,
                        status: $('#filterStatus').val() || null,
                        rating: $('#filterRating').val() || null,
                        source: $('#filterSource').val() || null,
                        startDate: $('#filterStartDate').val() || null,
                        endDate: $('#filterEndDate').val() || null,
                        keyword: d.search.value || null
                    };
                },
                dataSrc: function(json) {
                    // ✅ SỬA: Return data array from DataTable response
                    return json.data || [];
                }
            },
            columns: [
                { data: 'id', title: 'ID', width: '60px' },
                { data: 'customerName', title: 'Khách hàng', defaultContent: '-' },
                { data: 'orderNumber', title: 'Số phiếu', defaultContent: '-' },
                { data: 'feedbackChannelName', title: 'Kênh', defaultContent: '-' },
                { 
                    data: 'rating', 
                    title: 'Đánh giá',
                    render: function(data) {
                        if (!data) return '-';
                        var ratingMap = {
                            'Excellent': '<span class="badge badge-success">Xuất sắc</span>',
                            'Good': '<span class="badge badge-info">Tốt</span>',
                            'Neutral': '<span class="badge badge-secondary">Bình thường</span>',
                            'Poor': '<span class="badge badge-warning">Kém</span>',
                            'VeryPoor': '<span class="badge badge-danger">Rất kém</span>'
                        };
                        return ratingMap[data] || data;
                    }
                },
                { 
                    data: 'content', 
                    title: 'Nội dung',
                    render: function(data) {
                        if (!data) return '-';
                        return data.length > 50 ? data.substring(0, 50) + '...' : data;
                    }
                },
                { 
                    data: 'status', 
                    title: 'Trạng thái',
                    render: function(data) {
                        if (!data) return '-';
                        var statusMap = {
                            'New': '<span class="badge badge-primary">Mới</span>',
                            'InProgress': '<span class="badge badge-warning">Đang xử lý</span>',
                            'Responded': '<span class="badge badge-info">Đã phản hồi</span>',
                            'Resolved': '<span class="badge badge-success">Đã giải quyết</span>',
                            'Closed': '<span class="badge badge-secondary">Đã đóng</span>'
                        };
                        return statusMap[data] || data;
                    }
                },
                { 
                    data: 'createdAt', 
                    title: 'Ngày tạo',
                    render: DataTablesUtility.renderDate
                },
                {
                    data: null,
                    title: 'Thao tác',
                    orderable: false,
                    searchable: false,
                    render: function(data, type, row) {
                        return `
                            <button class="btn btn-info btn-sm view-feedback" data-id="${row.id}" title="Xem">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-warning btn-sm edit-feedback" data-id="${row.id}" title="Sửa">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-danger btn-sm delete-feedback" data-id="${row.id}" title="Xóa">
                                <i class="fas fa-trash"></i>
                            </button>
                        `;
                    }
                }
            ],
            language: DataTablesUtility.getVietnameseLanguage(),
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
            order: [[7, 'desc']],  // Sort by createdAt (column index 7) descending
            columnDefs: [
                {
                    targets: 7,  // createdAt column
                    type: 'date',
                    render: DataTablesUtility.renderDate
                }
            ]
        });
    },

    bindEvents: function() {
        var self = this;

        $('#btnFilter').on('click', function() {
            self.feedbackTable.ajax.reload();
        });

        $('#btnResetFilter').on('click', function() {
            $('#filterStatus').val('');
            $('#filterRating').val('');
            $('#filterSource').val('');
            $('#filterStartDate').val('');
            $('#filterEndDate').val('');
            self.feedbackTable.ajax.reload();
        });

        $(document).on('click', '.view-feedback', function() {
            var id = $(this).data('id');
            self.viewFeedback(id);
        });

        $(document).on('click', '.edit-feedback', function() {
            var id = $(this).data('id');
            self.editFeedback(id);
        });

        $(document).on('click', '.delete-feedback', function() {
            var id = $(this).data('id');
            self.deleteFeedback(id);
        });

        $('#btnCreateFeedback').on('click', function() {
            self.createFeedback();
        });

        // Create feedback form submit
        $(document).on('submit', '#createFeedbackForm', function(e) {
            e.preventDefault();
            self.submitCreateFeedback();
        });

        // Edit feedback form submit
        $(document).on('submit', '#editFeedbackForm', function(e) {
            e.preventDefault();
            self.submitEditFeedback();
        });
    },

    submitCreateFeedback: function() {
        var self = this;
        
        if (!$('#createFeedbackForm')[0].checkValidity()) {
            $('#createFeedbackForm')[0].reportValidity();
            return;
        }

        // ✅ SỬA: Helper function để safely parse int
        function safeParseInt(value) {
            if (!value) return null;
            var parsed = parseInt(value);
            return isNaN(parsed) ? null : parsed;
        }

        // ✅ SỬA: Helper function để safely parse date
        function safeParseDate(dateString) {
            if (!dateString) return null;
            var date = new Date(dateString);
            if (isNaN(date.getTime())) return null;
            return date.toISOString();
        }

        var formData = {
            customerId: safeParseInt($('#createCustomerId').val()),
            serviceOrderId: safeParseInt($('#createServiceOrderId').val()),
            source: $('#createSource').val() || 'Other',
            rating: $('#createRating').val() || 'Neutral',
            topic: $('#createTopic').val() || null,
            content: $('#createContent').val(),
            status: $('#createStatus').val() || 'New',
            followUpDate: safeParseDate($('#createFollowUpDate').val()),
            followUpById: safeParseInt($('#createFollowUpById').val()),
            notes: $('#createNotes').val() || null,
            score: safeParseInt($('#createScore').val()),
            feedbackChannelId: safeParseInt($('#createFeedbackChannelId').val())
        };

        GarageApp.showLoading('Đang lưu...');

        $.ajax({
            url: '/CustomerFeedbackManagement/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData)
        })
        .done(function(response) {
            GarageApp.hideLoading();
            if (!AuthHandler.validateApiResponse(response)) {
                return;
            }

            if (response.Success || response.success) {
                GarageApp.showSuccess('Đã tạo phản hồi thành công');
                $('#createFeedbackModal').modal('hide');
                self.feedbackTable.ajax.reload();
            } else {
                GarageApp.showError(response.ErrorMessage || response.errorMessage || 'Không thể tạo phản hồi');
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (AuthHandler.isUnauthorized(xhr)) {
                AuthHandler.handleUnauthorized(xhr, true);
                return;
            }
            var errorMessage = 'Không thể tạo phản hồi';
            if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                errorMessage = xhr.responseJSON.errorMessage;
            }
            GarageApp.showError(errorMessage);
        });
    },

    submitEditFeedback: function() {
        var self = this;
        var id = $('#editFeedbackId').val();

        // ✅ SỬA: Validate ID
        if (!id) {
            GarageApp.showError('Không xác định được ID phản hồi');
            return;
        }

        // ✅ SỬA: Helper function để safely parse int
        function safeParseInt(value) {
            if (!value) return null;
            var parsed = parseInt(value);
            return isNaN(parsed) ? null : parsed;
        }

        // ✅ SỬA: Helper function để safely parse date
        function safeParseDate(dateString) {
            if (!dateString) return null;
            var date = new Date(dateString);
            if (isNaN(date.getTime())) return null;
            return date.toISOString();
        }

        var formData = {};
        
        // Only include fields that have values (PATCH pattern)
        var rating = $('#editRating').val();
        if (rating) formData.rating = rating;

        var status = $('#editStatus').val();
        if (status) formData.status = status;

        var topic = $('#editTopic').val();
        if (topic) formData.topic = topic;
        else if (topic === '') formData.topic = null;

        var content = $('#editContent').val();
        if (content) formData.content = content;

        var actionTaken = $('#editActionTaken').val();
        if (actionTaken !== undefined) {
            formData.actionTaken = actionTaken || null;
        }

        var followUpDate = safeParseDate($('#editFollowUpDate').val());
        if (followUpDate !== null) {
            formData.followUpDate = followUpDate;
        }

        var followUpById = $('#editFollowUpById').val();
        if (followUpById) {
            var parsedId = safeParseInt(followUpById);
            if (parsedId !== null) formData.followUpById = parsedId;
        } else if (followUpById === '') {
            formData.followUpById = null;
        }

        var notes = $('#editNotes').val();
        if (notes !== undefined) {
            formData.notes = notes || null;
        }

        var score = $('#editScore').val();
        if (score) {
            var parsedScore = safeParseInt(score);
            if (parsedScore !== null) formData.score = parsedScore;
        } else if (score === '') {
            formData.score = null;
        }

        var feedbackChannelId = $('#editFeedbackChannelId').val();
        if (feedbackChannelId) {
            var parsedChannelId = safeParseInt(feedbackChannelId);
            if (parsedChannelId !== null) formData.feedbackChannelId = parsedChannelId;
        } else if (feedbackChannelId === '') {
            formData.feedbackChannelId = null;
        }

        GarageApp.showLoading('Đang cập nhật...');

        $.ajax({
            url: '/CustomerFeedbackManagement/Update/' + id,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData)
        })
        .done(function(response) {
            GarageApp.hideLoading();
            if (!AuthHandler.validateApiResponse(response)) {
                return;
            }

            if (response.Success || response.success) {
                GarageApp.showSuccess('Đã cập nhật phản hồi thành công');
                $('#editFeedbackModal').modal('hide');
                self.feedbackTable.ajax.reload();
            } else {
                GarageApp.showError(response.ErrorMessage || response.errorMessage || 'Không thể cập nhật phản hồi');
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (AuthHandler.isUnauthorized(xhr)) {
                AuthHandler.handleUnauthorized(xhr, true);
                return;
            }
            var errorMessage = 'Không thể cập nhật phản hồi';
            if (xhr.responseJSON && xhr.responseJSON.errorMessage) {
                errorMessage = xhr.responseJSON.errorMessage;
            }
            GarageApp.showError(errorMessage);
        });
    },

    viewFeedback: function(id) {
        var self = this;
        GarageApp.showLoading('Đang tải...');

        $.ajax({
            url: '/CustomerFeedbackManagement/GetFeedback/' + id,
            type: 'GET'
        })
        .done(function(response) {
            GarageApp.hideLoading();
            if (!AuthHandler.validateApiResponse(response)) {
                return;
            }

            // ✅ SỬA: Check cả Success (capital) và success (lowercase) để tương thích
            if ((response.Success || response.success) && (response.Data || response.data)) {
                var feedback = response.Data || response.data;
                $('#viewCustomerName').text(feedback.customerName || '-');
                $('#viewOrderNumber').text(feedback.orderNumber || '-');
                $('#viewChannel').text(feedback.feedbackChannelName || '-');
                $('#viewRating').text(self.getRatingText(feedback.rating));
                $('#viewTopic').text(feedback.topic || '-');
                $('#viewContent').text(feedback.content || '-');
                $('#viewStatus').text(self.getStatusText(feedback.status));
                $('#viewNotes').text(feedback.notes || '-');
                $('#viewFeedbackModal').modal('show');
            } else {
                GarageApp.showError(response.errorMessage || 'Không tìm thấy phản hồi');
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (AuthHandler.isUnauthorized(xhr)) {
                AuthHandler.handleUnauthorized(xhr, true);
                return;
            }
            GarageApp.showError('Không thể tải thông tin phản hồi');
        });
    },

    editFeedback: function(id) {
        var self = this;
        GarageApp.showLoading('Đang tải...');

        $.ajax({
            url: '/CustomerFeedbackManagement/GetFeedback/' + id,
            type: 'GET'
        })
        .done(function(response) {
            GarageApp.hideLoading();
            if (!AuthHandler.validateApiResponse(response)) {
                return;
            }

            if ((response.Success || response.success) && (response.Data || response.data)) {
                var feedback = response.Data || response.data;
                
                // Load dropdowns first
                self.loadEditDropdowns(function() {
                    // Populate form
                    $('#editFeedbackId').val(feedback.id);
                    $('#editCustomerName').text(feedback.customerName || '-');
                    $('#editOrderNumber').text(feedback.orderNumber || '-');
                    $('#editRating').val(feedback.rating || '');
                    $('#editStatus').val(feedback.status || '');
                    $('#editTopic').val(feedback.topic || '');
                    $('#editContent').val(feedback.content || '');
                    $('#editActionTaken').val(feedback.actionTaken || '');
                    $('#editNotes').val(feedback.notes || '');
                    $('#editScore').val(feedback.score || '');
                    
                    // Set select2 values
                    if (feedback.followUpById) {
                        $('#editFollowUpById').val(feedback.followUpById.toString()).trigger('change');
                    }
                    if (feedback.feedbackChannelId) {
                        $('#editFeedbackChannelId').val(feedback.feedbackChannelId.toString()).trigger('change');
                    }
                    
                    // ✅ SỬA: Set date if exists với validation
                    if (feedback.followUpDate) {
                        var followUpDate = new Date(feedback.followUpDate);
                        if (!isNaN(followUpDate.getTime())) {
                            var localDateTime = followUpDate.toISOString().slice(0, 16);
                            $('#editFollowUpDate').val(localDateTime);
                        } else {
                            $('#editFollowUpDate').val('');
                        }
                    } else {
                        $('#editFollowUpDate').val('');
                    }
                    
                    $('#editFeedbackModal').modal('show');
                });
            } else {
                GarageApp.showError(response.errorMessage || 'Không tìm thấy phản hồi');
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (AuthHandler.isUnauthorized(xhr)) {
                AuthHandler.handleUnauthorized(xhr, true);
                return;
            }
            GarageApp.showError('Không thể tải thông tin phản hồi');
        });
    },

    loadEditDropdowns: function(callback) {
        var self = this;
        var loadCount = 0;
        var totalLoads = 2; // Employees + FeedbackChannels

        function checkComplete() {
            loadCount++;
            if (loadCount >= totalLoads && callback) {
                callback();
            }
        }

        // Load Employees
        $.ajax({
            url: '/CustomerFeedbackManagement/GetAvailableEmployees',
            type: 'GET'
        })
        .done(function(data) {
            var $select = $('#editFollowUpById');
            $select.empty().append('<option value="">-- Chọn Nhân Viên --</option>');
            if (data && data.length > 0) {
                $.each(data, function(i, item) {
                    $select.append('<option value="' + item.value + '">' + item.text + '</option>');
                });
            }
            // ✅ SỬA: Check trước khi init Select2
            if ($select.hasClass('select2-hidden-accessible')) {
                $select.select2('destroy');
            }
            $select.select2();
            checkComplete();
        })
        .fail(function() {
            checkComplete();
        });

        // Load FeedbackChannels
        $.ajax({
            url: '/CustomerFeedbackManagement/GetAvailableFeedbackChannels',
            type: 'GET'
        })
        .done(function(data) {
            var $select = $('#editFeedbackChannelId');
            $select.empty().append('<option value="">-- Chọn Kênh --</option>');
            if (data && data.length > 0) {
                $.each(data, function(i, item) {
                    $select.append('<option value="' + item.value + '">' + item.text + '</option>');
                });
            }
            // ✅ SỬA: Check trước khi init Select2
            if ($select.hasClass('select2-hidden-accessible')) {
                $select.select2('destroy');
            }
            $select.select2();
            checkComplete();
        })
        .fail(function() {
            checkComplete();
        });
    },

    deleteFeedback: function(id) {
        var self = this;
        if (!confirm('Bạn có chắc chắn muốn xóa phản hồi này?')) {
            return;
        }

        GarageApp.showLoading('Đang xóa...');

        $.ajax({
            url: '/CustomerFeedbackManagement/Delete/' + id,
            type: 'DELETE'
        })
        .done(function(response) {
            GarageApp.hideLoading();
            if (!AuthHandler.validateApiResponse(response)) {
                return;
            }

            // ✅ SỬA: Check cả Success (capital) và success (lowercase) để tương thích
            if (response.Success || response.success) {
                GarageApp.showSuccess('Đã xóa phản hồi thành công');
                self.feedbackTable.ajax.reload();
            } else {
                GarageApp.showError(response.ErrorMessage || response.errorMessage || 'Không thể xóa phản hồi');
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (AuthHandler.isUnauthorized(xhr)) {
                AuthHandler.handleUnauthorized(xhr, true);
                return;
            }
            GarageApp.showError('Không thể xóa phản hồi');
        });
    },

    createFeedback: function() {
        var self = this;
        
        // Load dropdowns and show modal
        self.loadCreateDropdowns(function() {
            // Reset form
            $('#createFeedbackForm')[0].reset();
            $('#createFeedbackModal').modal('show');
        });
    },

    loadCreateDropdowns: function(callback) {
        var self = this;
        var loadCount = 0;
        var totalLoads = 4; // Customers + ServiceOrders + Employees + FeedbackChannels

        function checkComplete() {
            loadCount++;
            if (loadCount >= totalLoads && callback) {
                callback();
            }
        }

        // Load Customers
        $.ajax({
            url: '/CustomerFeedbackManagement/GetAvailableCustomers',
            type: 'GET'
        })
        .done(function(data) {
            var $select = $('#createCustomerId');
            $select.empty().append('<option value="">-- Chọn Khách Hàng --</option>');
            if (data && data.length > 0) {
                $.each(data, function(i, item) {
                    $select.append('<option value="' + item.value + '">' + item.text + '</option>');
                });
            }
            // ✅ SỬA: Check trước khi init Select2
            if ($select.hasClass('select2-hidden-accessible')) {
                $select.select2('destroy');
            }
            $select.select2();
            checkComplete();
        })
        .fail(function() {
            checkComplete();
        });

        // Load ServiceOrders
        $.ajax({
            url: '/CustomerFeedbackManagement/GetAvailableServiceOrders',
            type: 'GET'
        })
        .done(function(data) {
            var $select = $('#createServiceOrderId');
            $select.empty().append('<option value="">-- Chọn Phiếu Sửa Chữa --</option>');
            if (data && data.length > 0) {
                $.each(data, function(i, item) {
                    $select.append('<option value="' + item.value + '">' + item.text + '</option>');
                });
            }
            // ✅ SỬA: Check trước khi init Select2
            if ($select.hasClass('select2-hidden-accessible')) {
                $select.select2('destroy');
            }
            $select.select2();
            checkComplete();
        })
        .fail(function() {
            checkComplete();
        });

        // Load Employees
        $.ajax({
            url: '/CustomerFeedbackManagement/GetAvailableEmployees',
            type: 'GET'
        })
        .done(function(data) {
            var $select = $('#createFollowUpById');
            $select.empty().append('<option value="">-- Chọn Nhân Viên --</option>');
            if (data && data.length > 0) {
                $.each(data, function(i, item) {
                    $select.append('<option value="' + item.value + '">' + item.text + '</option>');
                });
            }
            // ✅ SỬA: Check trước khi init Select2
            if ($select.hasClass('select2-hidden-accessible')) {
                $select.select2('destroy');
            }
            $select.select2();
            checkComplete();
        })
        .fail(function() {
            checkComplete();
        });

        // Load FeedbackChannels
        $.ajax({
            url: '/CustomerFeedbackManagement/GetAvailableFeedbackChannels',
            type: 'GET'
        })
        .done(function(data) {
            var $select = $('#createFeedbackChannelId');
            $select.empty().append('<option value="">-- Chọn Kênh --</option>');
            if (data && data.length > 0) {
                $.each(data, function(i, item) {
                    $select.append('<option value="' + item.value + '">' + item.text + '</option>');
                });
            }
            // ✅ SỬA: Check trước khi init Select2
            if ($select.hasClass('select2-hidden-accessible')) {
                $select.select2('destroy');
            }
            $select.select2();
            checkComplete();
        })
        .fail(function() {
            checkComplete();
        });
    },

    getRatingText: function(rating) {
        var ratingMap = {
            'Excellent': 'Xuất sắc',
            'Good': 'Tốt',
            'Neutral': 'Bình thường',
            'Poor': 'Kém',
            'VeryPoor': 'Rất kém'
        };
        return ratingMap[rating] || rating || '-';
    },

    getStatusText: function(status) {
        var statusMap = {
            'New': 'Mới',
            'InProgress': 'Đang xử lý',
            'Responded': 'Đã phản hồi',
            'Resolved': 'Đã giải quyết',
            'Closed': 'Đã đóng'
        };
        return statusMap[status] || status || '-';
    }
};

$(document).ready(function() {
    CustomerFeedbackManagement.init();
});

