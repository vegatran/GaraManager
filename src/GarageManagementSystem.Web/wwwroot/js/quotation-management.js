/**
 * Quotation Management Module
 * 
 * Handles all service quotation-related operations
 * CRUD operations for service quotations
 */

window.QuotationManagement = {
    // DataTable instance
    quotationTable: null,

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.loadDropdowns();
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        this.quotationTable = DataTablesVietnamese.init('#quotationTable', {
            ajax: {
                url: '/QuotationManagement/GetQuotations',
                type: 'GET',
                error: function(xhr, status, error) {
                    if (AuthHandler.isUnauthorized(xhr)) {
                        AuthHandler.handleUnauthorized(xhr, true);
                    } else {
                        console.error('Error loading quotations:', error);
                        GarageApp.showError('Lỗi khi tải danh sách báo giá');
                    }
                }
            },
            columns: [
                { data: 'id', title: 'ID', width: '5%' },
                { data: 'quotationNumber', title: 'Số Báo Giá', width: '12%' },
                { data: 'vehicleInfo', title: 'Thông Tin Xe', width: '20%' },
                { data: 'customerName', title: 'Khách Hàng', width: '15%' },
                { 
                    data: 'totalAmount', 
                    title: 'Tổng Tiền', 
                    width: '12%',
                    render: function(data, type, row) {
                        return data ? data + ' VNĐ' : '0 VNĐ';
                    }
                },
                { 
                    data: 'status', 
                    title: 'Trạng Thái', 
                    width: '10%',
                    render: function(data, type, row) {
                        var badgeClass = 'badge-secondary';
                        switch(data) {
                            case 'Draft': badgeClass = 'badge-light'; break;
                            case 'Sent': badgeClass = 'badge-info'; break;
                            case 'Approved': badgeClass = 'badge-success'; break;
                            case 'Rejected': badgeClass = 'badge-danger'; break;
                            case 'Expired': badgeClass = 'badge-warning'; break;
                        }
                        return `<span class="badge ${badgeClass}">${data}</span>`;
                    }
                },
                { data: 'validUntil', title: 'Có Hiệu Lực Đến', width: '12%' },
                {
                    data: null,
                    title: 'Thao Tác',
                    width: '14%',
                    orderable: false,
                    render: function(data, type, row) {
                        var status = row.status;
                        var buttons = `
                            <div class="btn-group" role="group">
                                <button type="button" class="btn btn-info btn-sm view-quotation" data-id="${row.id}" title="Xem">
                                    <i class="fas fa-eye"></i>
                                </button>
                        `;
                        
                        // Chỉ hiển thị nút Edit và Delete khi chưa được duyệt
                        if (status !== 'Approved' && status !== 'Đã duyệt' && status !== 'Completed' && status !== 'Hoàn thành') {
                            buttons += `
                                <button type="button" class="btn btn-warning btn-sm edit-quotation" data-id="${row.id}" title="Sửa">
                                    <i class="fas fa-edit"></i>
                                </button>
                            `;
                        }
                        
                        if (status === 'Draft' || status === 'Sent') {
                            buttons += `
                                <button type="button" class="btn btn-success btn-sm approve-quotation" data-id="${row.id}" title="Duyệt">
                                    <i class="fas fa-check"></i>
                                </button>
                                <button type="button" class="btn btn-danger btn-sm reject-quotation" data-id="${row.id}" title="Từ chối">
                                    <i class="fas fa-times"></i>
                                </button>
                            `;
                        }
                        
                        // Chỉ hiển thị nút Delete khi chưa được duyệt
                        if (status !== 'Approved' && status !== 'Đã duyệt' && status !== 'Completed' && status !== 'Hoàn thành') {
                            buttons += `
                                <button type="button" class="btn btn-danger btn-sm delete-quotation" data-id="${row.id}" title="Xóa">
                                    <i class="fas fa-trash"></i>
                                </button>
                            `;
                        }
                        
                        buttons += `
                            </div>
                        `;
                        
                        return buttons;
                    }
                }
            ],
            order: [[0, 'desc']],
            pageLength: 25,
            dom: 'rtip'  // Chỉ hiển thị table, paging, info, processing (không có search box)
        });
    },

    bindEvents: function() {
        var self = this;

        // Search functionality
        $('#searchInput').on('keyup', function() {
            self.quotationTable.search(this.value).draw();
        });

        // Add quotation button
        $(document).on('click', '[data-target="#createQuotationModal"]', function() {
            self.showCreateModal();
        });

        // View quotation
        $(document).on('click', '.view-quotation', function() {
            var id = $(this).data('id');
            self.viewQuotation(id);
        });

        // Edit quotation
        $(document).on('click', '.edit-quotation', function() {
            var id = $(this).data('id');
            self.editQuotation(id);
        });

        // Approve quotation
        $(document).on('click', '.approve-quotation', function() {
            var id = $(this).data('id');
            self.approveQuotation(id);
        });

        // Reject quotation
        $(document).on('click', '.reject-quotation', function() {
            var id = $(this).data('id');
            self.rejectQuotation(id);
        });

        // Delete quotation
        $(document).on('click', '.delete-quotation', function() {
            var id = $(this).data('id');
            self.deleteQuotation(id);
        });

        // Create quotation form
        $(document).on('submit', '#createQuotationForm', function(e) {
            e.preventDefault();
            self.createQuotation();
        });

        // Update quotation form
        $(document).on('submit', '#editQuotationForm', function(e) {
            e.preventDefault();
            self.updateQuotation();
        });

        // Add service item button
        $(document).on('click', '#addCreateServiceItem', function() {
            self.addServiceItem('create');
        });

        // Add service item button for edit
        $(document).on('click', '#addEditServiceItem', function() {
            self.addServiceItem('edit');
        });

        // Remove service item
        $(document).on('click', '.remove-service-item', function() {
            $(this).closest('.service-item-row').remove();
        });
    },

    loadDropdowns: function() {
        this.loadVehicles();
        this.loadCustomers();
        this.loadServices();
    },

    loadVehicles: function() {
        $.ajax({
            url: '/QuotationManagement/GetAvailableVehicles',
            type: 'GET',
            success: function(response) {
                if (response && Array.isArray(response)) {
                    var vehicles = response;
                    var options = '<option value="">Chọn xe</option>';
                    vehicles.forEach(function(vehicle) {
                        options += `<option value="${vehicle.value}">${vehicle.text}</option>`;
                    });
                    $('#createVehicleId, #editVehicleId').html(options);
                } else {
                    console.error('Invalid vehicle data format:', response);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading vehicles:', error);
            }
        });
    },

    loadCustomers: function() {
        $.ajax({
            url: '/QuotationManagement/GetAvailableCustomers',
            type: 'GET',
            success: function(response) {
                if (response && Array.isArray(response)) {
                    var customers = response;
                    var options = '<option value="">Chọn khách hàng</option>';
                    customers.forEach(function(customer) {
                        options += `<option value="${customer.value}">${customer.text}</option>`;
                    });
                    $('#createCustomerId, #editCustomerId').html(options);
                } else {
                    console.error('Invalid customer data format:', response);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading customers:', error);
            }
        });
    },

    loadServices: function() {
        $.ajax({
            url: '/QuotationManagement/GetAvailableServices',
            type: 'GET',
            success: function(response) {
                if (response && Array.isArray(response)) {
                    var services = response;
                    var options = '<option value="">Chọn dịch vụ</option>';
                    services.forEach(function(service) {
                        options += `<option value="${service.value}">${service.text}</option>`;
                    });
                    $('#createServiceId, #editServiceId').html(options);
                } else {
                    console.error('Invalid service data format:', response);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading services:', error);
            }
        });
    },

    showCreateModal: function() {
        $('#createQuotationForm')[0].reset();
        $('#createServiceItems').empty(); // Clear existing service items
        this.addServiceItem('create'); // Add first service item
        $('#createQuotationModal').modal('show');
    },

    addServiceItem: function(mode) {
        var self = this;
        var prefix = mode === 'create' ? 'create' : 'edit';
        var containerId = prefix === 'create' ? 'createServiceItems' : 'editServiceItems';
        
        // Load services if not already loaded
        $.ajax({
            url: '/QuotationManagement/GetAvailableServices',
            type: 'GET',
            success: function(response) {
                if (response && Array.isArray(response)) {
                    var services = response;
                    var serviceOptions = '<option value="">Chọn dịch vụ</option>';
                    services.forEach(function(service) {
                        serviceOptions += `<option value="${service.value}" data-price="${service.price || 0}">${service.text} - ${service.price ? service.price.toLocaleString() + ' VNĐ' : '0 VNĐ'}</option>`;
                    });
                    
                    var itemIndex = $('#' + containerId + ' .service-item-row').length;
                    var serviceItemHtml = `
                        <div class="service-item-row bg-light p-2 mb-1 rounded border">
                            <div class="row align-items-center">
                                <div class="col-md-4">
                                    <div class="input-group input-group-sm">
                                        <input type="text" class="form-control service-typeahead" 
                                               placeholder="Gõ tên dịch vụ..." 
                                               name="Items[${itemIndex}].ServiceName"
                                               data-service-id=""
                                               autocomplete="off">
                                        <input type="hidden" class="service-id-input" name="Items[${itemIndex}].ServiceId">
                                    </div>
                                </div>
                                <div class="col-md-2">
                                    <input type="number" class="form-control form-control-sm quantity-input text-center" 
                                           name="Items[${itemIndex}].Quantity" value="1" min="1" 
                                           placeholder="SL" title="Số lượng">
                                </div>
                                <div class="col-md-2">
                                    <input type="text" class="form-control form-control-sm price-input text-right" 
                                           placeholder="0" readonly title="Đơn giá">
                                </div>
                                <div class="col-md-2">
                                    <input type="text" class="form-control form-control-sm total-input text-right" 
                                           placeholder="0" readonly title="Thành tiền">
                                </div>
                                <div class="col-md-2">
                                    <button type="button" class="btn btn-sm btn-outline-danger remove-service-item" title="Xóa">
                                        <i class="fas fa-times"></i>
                                    </button>
                                </div>
                            </div>
                        </div>
                    `;
                    
                    $('#' + containerId).append(serviceItemHtml);
                    
                    // Initialize typeahead for new service input
                    self.initializeServiceTypeahead($('#' + containerId + ' .service-typeahead').last(), prefix);
                    
                    // Bind change events for new item
                    self.bindServiceItemEvents(prefix);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading services:', error);
                GarageApp.showError('Lỗi khi tải danh sách dịch vụ');
            }
        });
    },

    initializeServiceTypeahead: function(input, prefix) {
        var self = this;
        
        input.typeahead({
            source: function(query, process) {
                $.ajax({
                    url: '/QuotationManagement/SearchServices',
                    type: 'GET',
                    data: { q: query },
                    success: function(response) {
                        if (response && Array.isArray(response)) {
                            var services = response.map(function(service) {
                                return {
                                    id: service.value,
                                    name: service.text,
                                    price: service.price || 0
                                };
                            });
                            process(services);
                        } else {
                            process([]);
                        }
                    },
                    error: function(xhr, status, error) {
                        console.error('Error searching services:', error);
                        process([]);
                    }
                });
            },
            displayText: function(item) {
                return item.name + ' - ' + item.price.toLocaleString() + ' VNĐ';
            },
            afterSelect: function(item) {
                var row = input.closest('.service-item-row');
                var price = item.price;
                var quantity = parseFloat(row.find('.quantity-input').val()) || 1;
                var total = price * quantity;
                
                // Set values
                row.find('.service-id-input').val(item.id);
                row.find('.price-input').val(price.toLocaleString() + ' VNĐ');
                row.find('.total-input').val(total.toLocaleString() + ' VNĐ');
                
                // Set input value
                input.val(item.name);
            },
            delay: 300
        });
    },

    bindServiceItemEvents: function(prefix) {
        var self = this;
        
        // Quantity change
        $(document).off('change', '.' + prefix + 'ServiceItems .quantity-input').on('change', '.' + prefix + 'ServiceItems .quantity-input', function() {
            var row = $(this).closest('.service-item-row');
            var priceText = row.find('.price-input').val();
            var price = parseFloat(priceText.replace(/[^\d]/g, '')) || 0;
            var quantity = parseFloat($(this).val()) || 1;
            var total = price * quantity;
            
            row.find('.total-input').val(total.toLocaleString() + ' VNĐ');
        });
        
        // Clear typeahead when input is cleared
        $(document).off('input', '.' + prefix + 'ServiceItems .service-typeahead').on('input', '.' + prefix + 'ServiceItems .service-typeahead', function() {
            if ($(this).val().trim() === '') {
                var row = $(this).closest('.service-item-row');
                row.find('.service-id-input').val('');
                row.find('.price-input').val('');
                row.find('.total-input').val('');
            }
        });
    },

    viewQuotation: function(id) {
        var self = this;
        $.ajax({
            url: '/QuotationManagement/GetQuotation/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success && response.data) {
                        var quotation = response.data;
                        self.populateViewModal(quotation);
                        $('#viewQuotationModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Lỗi khi tải thông tin báo giá');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin báo giá');
                }
            }
        });
    },

    editQuotation: function(id) {
        var self = this;
        $.ajax({
            url: '/QuotationManagement/GetQuotation/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success && response.data) {
                        var quotation = response.data;
                        self.populateEditModal(quotation);
                        $('#editQuotationModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Lỗi khi tải thông tin báo giá');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin báo giá');
                }
            }
        });
    },

    createQuotation: function() {
        var self = this;
        var formData = {
            VehicleId: parseInt($('#createVehicleId').val()),
            CustomerId: parseInt($('#createCustomerId').val()),
            ServiceId: parseInt($('#createServiceId').val()),
            Description: $('#createDescription').val() || null,
            ValidUntil: $('#createValidUntil').val() || null,
            Status: 'Draft'
        };

        // Validate required fields
        if (!formData.VehicleId || !formData.CustomerId || !formData.ServiceId) {
            GarageApp.showError('Vui lòng điền đầy đủ thông tin bắt buộc');
            return;
        }

        $.ajax({
            url: '/QuotationManagement/CreateQuotation',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Tạo báo giá thành công!');
                        $('#createQuotationModal').modal('hide');
                        self.quotationTable.ajax.reload();
                    } else {
                        GarageApp.showError(response.message || 'Lỗi khi tạo báo giá');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tạo báo giá');
                }
            }
        });
    },

    updateQuotation: function() {
        var self = this;
        var quotationId = $('#editId').val();
        var formData = {
            Id: parseInt(quotationId),
            VehicleId: parseInt($('#editVehicleId').val()),
            CustomerId: parseInt($('#editCustomerId').val()),
            ServiceId: parseInt($('#editServiceId').val()),
            Description: $('#editDescription').val() || null,
            ValidUntil: $('#editValidUntil').val() || null
        };

        // Validate required fields
        if (!formData.VehicleId || !formData.CustomerId || !formData.ServiceId) {
            GarageApp.showError('Vui lòng điền đầy đủ thông tin bắt buộc');
            return;
        }

        $.ajax({
            url: '/QuotationManagement/UpdateQuotation/' + quotationId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Cập nhật báo giá thành công!');
                        $('#editQuotationModal').modal('hide');
                        self.quotationTable.ajax.reload();
                    } else {
                        GarageApp.showError(response.message || 'Lỗi khi cập nhật báo giá');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi cập nhật báo giá');
                }
            }
        });
    },

    approveQuotation: function(id) {
        var self = this;
        
        Swal.fire({
            title: 'Duyệt báo giá?',
            text: "Bạn có chắc chắn muốn duyệt báo giá này?",
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Có, duyệt!',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/QuotationManagement/ApproveQuotation/' + id,
                    type: 'POST',
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Duyệt báo giá thành công!');
                                self.quotationTable.ajax.reload();
                            } else {
                                GarageApp.showError(response.message || 'Lỗi khi duyệt báo giá');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi duyệt báo giá');
                        }
                    }
                });
            }
        });
    },

    rejectQuotation: function(id) {
        var self = this;
        
        Swal.fire({
            title: 'Từ chối báo giá?',
            text: "Bạn có chắc chắn muốn từ chối báo giá này?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Có, từ chối!',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/QuotationManagement/RejectQuotation/' + id,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ reason: 'Từ chối bởi quản lý' }),
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Từ chối báo giá thành công!');
                                self.quotationTable.ajax.reload();
                            } else {
                                GarageApp.showError(response.message || 'Lỗi khi từ chối báo giá');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi từ chối báo giá');
                        }
                    }
                });
            }
        });
    },

    deleteQuotation: function(id) {
        var self = this;
        
        Swal.fire({
            title: 'Bạn có chắc chắn?',
            text: "Bạn sẽ không thể hoàn tác hành động này!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Có, xóa!',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/QuotationManagement/DeleteQuotation/' + id,
                    type: 'DELETE',
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Xóa báo giá thành công!');
                                self.quotationTable.ajax.reload();
                            } else {
                                GarageApp.showError(response.message || 'Lỗi khi xóa báo giá');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi xóa báo giá');
                        }
                    }
                });
            }
        });
    },

    populateViewModal: function(quotation) {
        $('#viewQuotationNumber').text(quotation.quotationNumber || '');
        $('#viewVehicleInfo').text(quotation.vehicle ? `${quotation.vehicle.brand} ${quotation.vehicle.model} - ${quotation.vehicle.licensePlate}` : '');
        $('#viewCustomerName').text(quotation.customer ? quotation.customer.name : '');
        $('#viewTotalAmount').text(quotation.totalAmount ? quotation.totalAmount.toLocaleString() + ' VNĐ' : '0 VNĐ');
        $('#viewStatus').text(quotation.status || '');
        $('#viewValidUntil').text(quotation.validUntil ? new Date(quotation.validUntil).toLocaleDateString('vi-VN') : '');
        $('#viewDescription').text(quotation.description || '');
    },

    populateEditModal: function(quotation) {
        $('#editId').val(quotation.id);
        $('#editVehicleId').val(quotation.vehicleId).trigger('change');
        $('#editCustomerId').val(quotation.customerId).trigger('change');
        $('#editServiceId').val(quotation.serviceId).trigger('change');
        $('#editDescription').val(quotation.description || '');
        $('#editValidUntil').val(quotation.validUntil ? new Date(quotation.validUntil).toISOString().split('T')[0] : '');
    }
};

$(document).ready(function() {
    QuotationManagement.init();
});
