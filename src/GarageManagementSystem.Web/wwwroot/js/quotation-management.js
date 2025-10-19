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
        
        // Sử dụng DataTablesUtility với server-side pagination
        var columns = [
            { data: 'id', title: 'ID', width: '5%' },
            { data: 'quotationNumber', title: 'Số Báo Giá', width: '12%' },
            { data: 'vehicleInfo', title: 'Thông Tin Xe', width: '20%' },
            { data: 'customerName', title: 'Khách Hàng', width: '15%' },
            { 
                data: 'totalAmount', 
                title: 'Tổng Tiền', 
                width: '12%',
                render: DataTablesUtility.renderCurrency,
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
            { 
                data: 'validUntil', 
                title: 'Có Hiệu Lực Đến', 
                width: '12%',
                render: DataTablesUtility.renderDate,
            },
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
                            <button type="button" class="btn btn-primary btn-sm print-quotation" data-id="${row.id}" title="In Báo Giá">
                                <i class="fas fa-print"></i>
                            </button>
                    `;
                    
                    // Chỉ hiển thị nút Edit khi chưa được duyệt
                    if (status !== 'Approved' && status !== 'Đã duyệt' && status !== 'Completed' && status !== 'Hoàn thành') {
                        buttons += `
                            <button type="button" class="btn btn-warning btn-sm edit-quotation" data-id="${row.id}" title="Sửa">
                                <i class="fas fa-edit"></i>
                            </button>
                        `;
                    }
                    
                    if (status === 'Draft' || status === 'Nháp' || status === 'Sent' || status === 'Đã gửi') {
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
        ];
        
        this.quotationTable = DataTablesUtility.initServerSideTable('#quotationTable', '/QuotationManagement/GetQuotations', columns, {
            order: [[0, 'desc']],
            pageLength: 10,
        });
    },

    bindEvents: function() {
        var self = this;

        // Add quotation button
        $(document).on('click', '#addQuotationBtn', function() {
            self.showCreateModal();
        });

        // View quotation
        $(document).on('click', '.view-quotation', function() {
            var id = $(this).data('id');
            self.viewQuotation(id);
        });

        // ✅ THÊM: Attachment Management Events
        // Upload Attachment button
        $(document).on('click', '#uploadAttachmentBtn', function() {
            $('#uploadAttachmentModal').modal('show');
        });

        // Save Attachment button
        $(document).on('click', '#saveAttachmentBtn', function() {
            self.uploadAttachment();
        });

        // Download Attachment button
        $(document).on('click', '.download-attachment', function() {
            var attachmentId = $(this).data('id');
            self.downloadAttachment(attachmentId);
        });

        // Delete Attachment button
        $(document).on('click', '.delete-attachment', function() {
            var attachmentId = $(this).data('id');
            self.deleteAttachment(attachmentId);
        });

        // Print quotation
        $(document).on('click', '.print-quotation', function() {
            var id = $(this).data('id');
            self.printQuotation(id);
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

        // Bind VehicleInspection change event
        $(document).on('change', '#createVehicleInspectionId', function() {
            self.onInspectionChange();
        });

        // Add service item buttons for different types
        $(document).on('click', '#addCreatePartsItem', function() {
            self.addServiceItem('create', 'parts');
        });

        $(document).on('click', '#addCreateRepairItem', function() {
            self.addServiceItem('create', 'repair');
        });

        $(document).on('click', '#addCreatePaintItem', function() {
            self.addServiceItem('create', 'paint');
        });

        // ✅ THÊM: Handlers cho nút "Thêm Tiền Công"
        $(document).on('click', '#addCreatePartsLabor', function() {
            self.addLaborItem('create', 'parts');
        });

        $(document).on('click', '#addCreateRepairLabor', function() {
            self.addLaborItem('create', 'repair');
        });

        $(document).on('click', '#addCreatePaintLabor', function() {
            self.addLaborItem('create', 'paint');
        });

        // Add service item buttons for edit modal
        $(document).on('click', '#addEditPartsItem', function() {
            self.addServiceItem('edit', 'parts');
        });

        $(document).on('click', '#addEditRepairItem', function() {
            self.addServiceItem('edit', 'repair');
        });

        $(document).on('click', '#addEditPaintItem', function() {
            self.addServiceItem('edit', 'paint');
        });

        // ✅ THÊM: Handlers cho nút "Thêm Tiền Công" trong Edit Modal
        $(document).on('click', '#addEditPartsLabor', function() {
            self.addLaborItem('edit', 'parts');
        });

        $(document).on('click', '#addEditRepairLabor', function() {
            self.addLaborItem('edit', 'repair');
        });

        $(document).on('click', '#addEditPaintLabor', function() {
            self.addLaborItem('edit', 'paint');
        });

        // Remove service item
        $(document).on('click', '.remove-service-item', function() {
            $(this).closest('.service-item-row').remove();
        });

        // ✅ THÊM: Event handler cho tab activation - lưu tab active hiện tại
        var currentActiveTab = 'edit-parts'; // Default tab
        
        $(document).on('shown.bs.tab', 'a[data-toggle="tab"]', function(e) {
            var targetTab = $(e.target).attr('href'); // #edit-parts, #edit-repair, #edit-paint
            var tabId = targetTab.replace('#', ''); // edit-parts, edit-repair, edit-paint
            
            currentActiveTab = tabId; // ✅ LƯU tab active hiện tại
        });
        
        // ✅ THÊM: Event handler cho checkbox - sử dụng currentActiveTab
        $(document).on('change', '.invoice-checkbox', function(e) {
            e.stopPropagation();
            
            var row = $(this).closest('.service-item-row');
            var isChecked = $(this).is(':checked');
            
            
            // ✅ XỬ LÝ THEO TAB ACTIVE HIỆN TẠI
            if (currentActiveTab === 'edit-parts') {
                // Logic xử lý cho Parts tab
            } else if (currentActiveTab === 'edit-repair') {
                // Logic xử lý cho Repair tab
            } else if (currentActiveTab === 'edit-paint') {
                // Logic xử lý cho Paint tab
            }
            
            // ✅ THÊM: Visual feedback - highlight row khi checkbox thay đổi
            if (isChecked) {
                row.addClass('table-success');
            } else {
                row.removeClass('table-success');
            }
            
            // ✅ THÊM: Recalculate total khi checkbox thay đổi
            var priceText = row.find('.unit-price-input').val() || '';
            var price = parseFloat(priceText.replace(/[^\d]/g, '')) || 0;
            var quantity = parseFloat(row.find('.quantity-input').val()) || 1;
            var vatRate = 10; // Default VAT rate
            
            var total = self.calculateTotalWithVAT(price, quantity, isChecked, vatRate);
            row.find('.total-input').val(total.toLocaleString() + ' VNĐ');
            
            // ✅ FORCE UPDATE: Đảm bảo checkbox UI hiển thị đúng state
            var checkbox = $(this);
            checkbox.prop('checked', isChecked);
            
            // ✅ DEBUG: Xem có attribute nào can thiệp không
            
            // Có thể thêm logic xử lý khi checkbox thay đổi ở đây
            // Ví dụ: update VAT calculation, etc.
        });
    },

    loadDropdowns: function() {
        this.loadInspections();
        this.loadVehicles();
        this.loadCustomers();
        this.loadServices();
    },

    loadInspections: function() {
        var self = this;
        $.ajax({
            url: '/QuotationManagement/GetAvailableInspections',
            type: 'GET',
            success: function(data) {
                var $select = $('#createVehicleInspectionId');
                $select.empty().append('<option value="">Chọn kiểm tra xe</option>');
                
                if (data && data.length > 0) {
                    $.each(data, function(index, item) {
                        $select.append(`<option value="${item.value}" 
                            data-vehicle-id="${item.vehicleId}" 
                            data-customer-id="${item.customerId}" 
                            data-vehicle-info="${item.vehicleInfo}" 
                            data-customer-name="${item.customerName}" 
                            data-inspection-date="${item.inspectionDate}">${item.text}</option>`);
                    });
                }
                
                $select.select2({
                    placeholder: 'Chọn kiểm tra xe',
                    allowClear: true,
                });
            },
            error: function(xhr, status, error) {
                GarageApp.showError('Lỗi khi tải danh sách kiểm tra xe');
            }
        });
    },

    onInspectionChange: function() {
        var selectedOption = $('#createVehicleInspectionId option:selected');
        
        if (selectedOption.val()) {
            // Tự động điền thông tin từ VehicleInspection
            var vehicleId = selectedOption.data('vehicle-id');
            var customerId = selectedOption.data('customer-id');
            var vehicleInfo = selectedOption.data('vehicle-info');
            var customerName = selectedOption.data('customer-name');
            var inspectionDate = selectedOption.data('inspection-date');
            
            // Điền thông tin xe
            $('#createVehicleId').val(vehicleId).trigger('change');
            
            // Điền thông tin khách hàng
            $('#createCustomerId').val(customerId).trigger('change');
            
            // Hiển thị thông tin đã chọn
        } else {
            // Reset các field khi không chọn inspection
            $('#createVehicleId').val('').trigger('change');
            $('#createCustomerId').val('').trigger('change');
        }
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
                }
            },
            error: function(xhr, status, error) {
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
                }
            },
            error: function(xhr, status, error) {
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
                }
            },
            error: function(xhr, status, error) {
            }
        });
    },

    showCreateModal: function() {
        $('#createQuotationForm')[0].reset();
        $('#createPartsItems').empty(); // Clear existing parts items
        $('#createRepairItems').empty(); // Clear existing repair items
        $('#createPaintItems').empty(); // Clear existing paint items
        $('#createQuotationModal').modal('show');
    },

    addServiceItem: function(mode, serviceType) {
        var self = this;
        var prefix = mode === 'create' ? 'create' : 'edit';
        var containerId;
        
        if (mode === 'create') {
            switch(serviceType) {
                case 'parts': containerId = 'createPartsItems'; break;
                case 'repair': containerId = 'createRepairItems'; break;
                case 'paint': containerId = 'createPaintItems'; break;
                default: containerId = 'createServiceItems'; break;
            }
        } else {
            switch(serviceType) {
                case 'parts': containerId = 'editPartsItems'; break;
                case 'repair': containerId = 'editRepairItems'; break;
                case 'paint': containerId = 'editPaintItems'; break;
                default: containerId = 'editServiceItems'; break;
            }
        }
        
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
                    
                    // ✅ SỬA: Tính itemIndex global cho tất cả tabs
                    var itemIndex = $('#editPartsItems .service-item-row, #editRepairItems .service-item-row, #editPaintItems .service-item-row').length;
                    var placeholder = "Gõ tên dịch vụ...";
                    var serviceTypeClass = "";
                    
                    // Set placeholder and styling based on service type
                    switch(serviceType) {
                        case 'parts':
                            placeholder = "Gõ tên phụ tùng...";
                            serviceTypeClass = "border-left-primary";
                            break;
                        case 'repair':
                            placeholder = "Gõ tên dịch vụ sửa chữa...";
                            serviceTypeClass = "border-left-warning";
                            break;
                        case 'paint':
                            placeholder = "Gõ tên dịch vụ sơn...";
                            serviceTypeClass = "border-left-info";
                            break;
                    }
                    
                    var serviceItemHtml = `
                        <tr class="service-item-row">
                            <td>
                                <input type="text" class="form-control form-control-sm service-typeahead" 
                                       placeholder="${placeholder}" 
                                       name="Items[${itemIndex}].ServiceName"
                                       data-service-id=""
                                       data-service-type="${serviceType}"
                                       autocomplete="off">
                                <input type="hidden" class="service-id-input" name="Items[${itemIndex}].ServiceId">
                                <input type="hidden" class="service-type-input" name="Items[${itemIndex}].ServiceType" value="${serviceType}">
                                <input type="hidden" class="item-category-input" name="Items[${itemIndex}].ItemCategory" value="Material">
                            </td>
                            <td>
                                <input type="number" class="form-control form-control-sm quantity-input text-center" 
                                       name="Items[${itemIndex}].Quantity" value="1" min="1" 
                                       placeholder="1" title="Số lượng">
                            </td>
                            <td>
                                <input type="text" class="form-control form-control-sm unit-price-input text-right" 
                                       placeholder="0" readonly title="Đơn giá">
                            </td>
                            <td>
                                <input type="text" class="form-control form-control-sm total-input text-right" 
                                       placeholder="0" readonly title="Thành tiền">
                            </td>
                            <td class="text-center">
                                <div class="custom-control custom-checkbox">
                                    <input class="custom-control-input invoice-checkbox" type="checkbox" 
                                           name="Items[${itemIndex}].HasInvoice" id="invoice_${mode}_${itemIndex}">
                                    <label class="custom-control-label" for="invoice_${mode}_${itemIndex}"></label>
                                </div>
                            </td>
                            <td class="text-center">
                                <button type="button" class="btn btn-sm btn-outline-danger remove-service-item" title="Xóa">
                                    <i class="fas fa-times"></i>
                                </button>
                            </td>
                        </tr>
                    `;
                    
                    $('#' + containerId).append(serviceItemHtml);
                    
                    // Initialize typeahead for new service input
                    self.initializeServiceTypeahead($('#' + containerId + ' .service-typeahead').last(), prefix);
                    
                    // Bind change events for new item
                    self.bindServiceItemEvents(prefix);
                }
            },
            error: function(xhr, status, error) {
                GarageApp.showError('Lỗi khi tải danh sách dịch vụ');
            }
        });
    },

    // ✅ THÊM: Function tính thành tiền bao gồm VAT
    calculateTotalWithVAT: function(unitPrice, quantity, isVATApplicable, vatRate) {
        var subtotal = unitPrice * quantity;
        if (isVATApplicable && vatRate > 0) {
            var vatAmount = subtotal * (vatRate / 100);
            return subtotal + vatAmount;
        }
        return subtotal;
    },

    // ✅ THÊM: Function tính lại tất cả "Thành tiền" bao gồm VAT
    recalculateAllTotalsWithVAT: function(mode, vatRate) {
        var self = this;
        var prefix = mode === 'create' ? 'create' : 'edit';
        var containers = [prefix + 'PartsItems', prefix + 'RepairItems', prefix + 'PaintItems'];
        
        containers.forEach(function(containerId) {
            $('#' + containerId + ' .service-item-row').each(function() {
                var row = $(this);
                var priceText = row.find('.unit-price-input').val() || '';
                var price = parseFloat(priceText.replace(/[^\d]/g, '')) || 0;
                var quantity = parseFloat(row.find('.quantity-input').val()) || 1;
                var isVATApplicable = row.find('.invoice-checkbox').is(':checked');
                
                if (price > 0 && quantity > 0) {
                    var total = self.calculateTotalWithVAT(price, quantity, isVATApplicable, vatRate);
                    row.find('.total-input').val(total.toLocaleString() + ' VNĐ');
                }
            });
        });
    },

    // ✅ THÊM: Function để thêm tiền công lao động
    addLaborItem: function(mode, serviceType) {
        var self = this;
        var prefix = mode === 'create' ? 'create' : 'edit';
        var containerId;
        
        if (mode === 'create') {
            switch(serviceType) {
                case 'parts': containerId = 'createPartsItems'; break;
                case 'repair': containerId = 'createRepairItems'; break;
                case 'paint': containerId = 'createPaintItems'; break;
                default: containerId = 'createServiceItems'; break;
            }
        } else {
            switch(serviceType) {
                case 'parts': containerId = 'editPartsItems'; break;
                case 'repair': containerId = 'editRepairItems'; break;
                case 'paint': containerId = 'editPaintItems'; break;
                default: containerId = 'editServiceItems'; break;
            }
        }

        // Tạo labor item với ItemCategory = 'Labor'
        var laborItemHtml = self.createLaborItemHtml(prefix, serviceType);
        $('#' + containerId).append(laborItemHtml);
        
        // Bind events cho labor item mới
        self.bindServiceItemEvents(prefix);
        
        // ✅ THÊM: Tính toán thủ công cho labor item mới
        self.calculateLaborItemTotal(prefix, serviceType);
    },

    // ✅ THÊM: Tạo HTML cho labor item
    createLaborItemHtml: function(prefix, serviceType) {
        var laborNames = {
            'parts': 'Công lắp đặt phụ tùng',
            'repair': 'Công sửa chữa động cơ', 
            'paint': 'Công sơn toàn thân xe'
        };
        
        var laborName = laborNames[serviceType] || 'Công lao động';
        var itemId = 'item_' + Date.now();
        
        // ✅ SỬA: Tính itemIndex global cho tất cả tabs
        var itemIndex = $('#editPartsItems .service-item-row, #editRepairItems .service-item-row, #editPaintItems .service-item-row').length;
        
        return `
            <tr class="service-item-row" data-item-id="${itemId}">
                <td>
                    <input type="hidden" class="service-id-input" name="Items[${itemIndex}].ServiceId" value="">
                    <input type="hidden" class="item-category-input" name="Items[${itemIndex}].ItemCategory" value="Labor">
                    <input type="text" class="form-control form-control-sm service-typeahead" 
                           name="Items[${itemIndex}].ServiceName" value="${laborName}" readonly>
                </td>
                <td>
                    <input type="number" class="form-control form-control-sm quantity-input" 
                           name="Items[${itemIndex}].Quantity" value="1" min="1" max="999">
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm unit-price-input" 
                           name="Items[${itemIndex}].UnitPrice" placeholder="0" value="">
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm total-input text-right" 
                           name="Items[${itemIndex}].TotalPrice" placeholder="0" readonly title="Thành tiền">
                </td>
                <td>
                    <span class="badge badge-secondary">Không</span>
                </td>
                <td class="text-center">
                    <button type="button" class="btn btn-sm btn-outline-danger remove-service-item" title="Xóa">
                        <i class="fas fa-times"></i>
                    </button>
                </td>
            </tr>
        `;
    },

    addServiceItemWithData: function(mode, itemData) {
        var self = this;
        var prefix = mode === 'create' ? 'create' : 'edit';
        var containerId;
        
        // Determine container based on service type
        if (mode === 'create') {
            switch(itemData.serviceType) {
                case 'parts': containerId = 'createPartsItems'; break;
                case 'repair': containerId = 'createRepairItems'; break;
                case 'paint': containerId = 'createPaintItems'; break;
                default: containerId = 'createServiceItems'; break;
            }
        } else {
            switch(itemData.serviceType) {
                case 'parts': containerId = 'editPartsItems'; break;
                case 'repair': containerId = 'editRepairItems'; break;
                case 'paint': containerId = 'editPaintItems'; break;
                default: containerId = 'editServiceItems'; break;
            }
        }
        
        // ✅ SỬA: Tính itemIndex global cho tất cả tabs
        var itemIndex = $('#editPartsItems .service-item-row, #editRepairItems .service-item-row, #editPaintItems .service-item-row').length;
        var serviceItemHtml = `
            <tr class="service-item-row">
                <td>
                    <input type="text" class="form-control form-control-sm service-typeahead" 
                           placeholder="Gõ tên dịch vụ..." data-service-id="${itemData.serviceId || ''}"
                           value="${itemData.itemName || ''}">
                    <input type="hidden" class="service-id-input" value="${itemData.serviceId || ''}">
                    <input type="hidden" class="item-category-input" value="${itemData.itemCategory || itemData.ItemCategory || 'Material'}">
                </td>
                <td>
                    <input type="number" class="form-control form-control-sm quantity-input" 
                           value="${itemData.quantity || 1}" min="1" placeholder="1">
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm unit-price-input text-right" 
                           value="${itemData.unitPrice || 0}" placeholder="0" readonly title="Đơn giá">
                </td>
                <td>
                    <input type="number" class="form-control form-control-sm vat-rate-input text-right" 
                           value="${itemData.vatRate || itemData.VATRate || 10}" min="0" max="100" step="0.1" 
                           placeholder="10" title="VAT (%)">
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm total-input text-right" 
                           value="${itemData.totalPrice || 0}" placeholder="0" readonly title="Thành tiền">
                </td>
                <td class="text-center">
                    <div class="custom-control custom-checkbox">
                        <input class="custom-control-input invoice-checkbox" type="checkbox" 
                               name="Items[${itemIndex}].HasInvoice" id="invoice_${mode}_${itemIndex}"
                               ${(itemData.hasInvoice || itemData.HasInvoice || (itemData.notes && itemData.notes.includes('Có hóa đơn'))) ? 'checked' : ''}>
                        <label class="custom-control-label" for="invoice_${mode}_${itemIndex}"></label>
                    </div>
                </td>
                <td class="text-center">
                    <button type="button" class="btn btn-sm btn-outline-danger remove-service-item" title="Xóa">
                        <i class="fas fa-times"></i>
                    </button>
                </td>
            </tr>
        `;
        
        $('#' + containerId).append(serviceItemHtml);
        
        // Set the service name for typeahead display
        var lastRow = $('#' + containerId + ' .service-item-row').last();
        if (itemData.service && itemData.service.name) {
            lastRow.find('.service-typeahead').val(itemData.service.name);
        }
        
        // Initialize typeahead for new service input
        self.initializeServiceTypeahead($('#' + containerId + ' .service-typeahead').last(), prefix);
        
        // Bind change events for new item
        self.bindServiceItemEvents(prefix);
    },

    // ✅ THÊM: Function tính toán thành tiền cho từng item
    calculateItemTotal: function($row) {
        var quantity = parseFloat($row.find('.quantity-input').val()) || 0;
        var unitPrice = parseFloat($row.find('.unit-price-input').val()) || 0;
        var vatRate = parseFloat($row.find('.vat-rate-input').val()) || 0;
        var isVATApplicable = $row.find('.invoice-checkbox').is(':checked');

        var itemSubtotal = quantity * unitPrice;
        var itemTotalPrice = itemSubtotal;

        if (isVATApplicable) {
            itemTotalPrice += itemSubtotal * (vatRate / 100);
        }

        // ✅ SỬA: Lưu giá trị thô, hiển thị format
        $row.find('.total-input').val(itemTotalPrice.toFixed(0));
        this.formatTotalInputForDisplay($row.find('.total-input')); // ✅ THÊM: Format hiển thị ngay lập tức
        this.calculateOverallTotals();
    },

    // ✅ THÊM: Function tính tổng cộng
    calculateOverallTotals: function() {
        var subtotal = 0;
        var taxAmount = 0;
        var discountAmount = parseFloat($('#editDiscountAmount').val()) || 0;

        $('#editPartsItems tr, #editRepairItems tr, #editPaintItems tr').each(function() {
            var $row = $(this);
            var quantity = parseFloat($row.find('.quantity-input').val()) || 0;
            var unitPrice = parseFloat($row.find('.unit-price-input').val()) || 0;
            var vatRate = parseFloat($row.find('.vat-rate-input').val()) || 0;
            var isVATApplicable = $row.find('.invoice-checkbox').is(':checked');

            var itemSubtotal = quantity * unitPrice;
            subtotal += itemSubtotal;

            if (isVATApplicable) {
                taxAmount += itemSubtotal * (vatRate / 100);
            }
        });

        var totalAmount = subtotal + taxAmount - discountAmount;

        // Update display (nếu có các element này)
        if ($('#editSubTotal').length) $('#editSubTotal').text(subtotal.toLocaleString());
        if ($('#editTaxAmount').length) $('#editTaxAmount').text(taxAmount.toLocaleString());
        if ($('#editTotalAmount').length) $('#editTotalAmount').text(totalAmount.toLocaleString());
    },

    // ✅ THÊM: Function để format hiển thị số tiền trong input
    formatTotalInputForDisplay: function($input) {
        var value = parseFloat($input.val()) || 0;
        if (value > 0) {
            $input.val(value.toLocaleString('vi-VN') + ' VNĐ');
        } else {
            $input.val('0 VNĐ');
        }
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
                                    price: service.price || 0,
                                };
                            });
                            process(services);
                        } else {
                            process([]);
                        }
                    },
                    error: function(xhr, status, error) {
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
                row.find('.unit-price-input').val(price.toLocaleString() + ' VNĐ');
                row.find('.total-input').val(total.toLocaleString() + ' VNĐ');
                
                // Set input value
                input.val(item.name);
            },
            delay: 300,
        });
    },

    // ✅ THÊM: Function tính toán thủ công cho labor item
    calculateLaborItemTotal: function(prefix, serviceType) {
        var containerId = prefix + serviceType.charAt(0).toUpperCase() + serviceType.slice(1) + 'Items';
        var lastRow = $('#' + containerId + ' .service-item-row').last();
        
        if (lastRow.length > 0) {
            var priceText = lastRow.find('.unit-price-input').val() || '';
            var price = parseFloat(priceText.replace(/[^\d]/g, '')) || 0;
            var quantity = parseFloat(lastRow.find('.quantity-input').val()) || 1;
            var total = price * quantity;
            
            lastRow.find('.total-input').val(total.toLocaleString() + ' VNĐ');
        }
    },

    bindServiceItemEvents: function(prefix) {
        var self = this;
        
        // Quantity change
        $(document).off('change', '#' + prefix + 'PartsItems .quantity-input, #' + prefix + 'RepairItems .quantity-input, #' + prefix + 'PaintItems .quantity-input').on('change', '#' + prefix + 'PartsItems .quantity-input, #' + prefix + 'RepairItems .quantity-input, #' + prefix + 'PaintItems .quantity-input', function() {
            var row = $(this).closest('.service-item-row');
            self.calculateItemTotal(row);
        });

        // Price change (for labor items)
        $(document).off('input', '#' + prefix + 'PartsItems .unit-price-input, #' + prefix + 'RepairItems .unit-price-input, #' + prefix + 'PaintItems .unit-price-input').on('input', '#' + prefix + 'PartsItems .unit-price-input, #' + prefix + 'RepairItems .unit-price-input, #' + prefix + 'PaintItems .unit-price-input', function() {
            var row = $(this).closest('.service-item-row');
            self.calculateItemTotal(row);
        });

        // ✅ THÊM: VAT rate change
        $(document).off('change input', '#' + prefix + 'PartsItems .vat-rate-input, #' + prefix + 'RepairItems .vat-rate-input, #' + prefix + 'PaintItems .vat-rate-input').on('change input', '#' + prefix + 'PartsItems .vat-rate-input, #' + prefix + 'RepairItems .vat-rate-input, #' + prefix + 'PaintItems .vat-rate-input', function() {
            var row = $(this).closest('.service-item-row');
            self.calculateItemTotal(row);
        });

        // ✅ THÊM: Invoice checkbox change
        $(document).off('change', '#' + prefix + 'PartsItems .invoice-checkbox, #' + prefix + 'RepairItems .invoice-checkbox, #' + prefix + 'PaintItems .invoice-checkbox').on('change', '#' + prefix + 'PartsItems .invoice-checkbox, #' + prefix + 'RepairItems .invoice-checkbox, #' + prefix + 'PaintItems .invoice-checkbox', function() {
            var row = $(this).closest('.service-item-row');
            self.calculateItemTotal(row);
        });
        
        // Clear typeahead when input is cleared
        $(document).off('input', '.' + prefix + 'ServiceItems .service-typeahead').on('input', '.' + prefix + 'ServiceItems .service-typeahead', function() {
            if ($(this).val().trim() === '') {
                var row = $(this).closest('.service-item-row');
                row.find('.service-id-input').val('');
                row.find('.unit-price-input').val('');
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
                        // ✅ THÊM: Load attachments khi mở view modal
                        self.loadAttachments(id);
                        $('#viewQuotationModal').modal('show');
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi tải thông tin báo giá');
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
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi tải thông tin báo giá');
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
        
        // Collect all service items from all tabs
        var items = [];
        
        // Collect from Parts tab
        $('#createPartsItems .service-item-row').each(function() {
            var row = $(this);
            var serviceId = row.find('.service-id-input').val();
            var serviceName = row.find('.service-typeahead').val();
            var quantity = parseInt(row.find('.quantity-input').val()) || 1;
            var unitPriceText = row.find('.unit-price-input').val() || '';
            var unitPrice = parseFloat(unitPriceText.replace(/[^\d]/g, '')) || 0;
            var hasInvoice = row.find('.invoice-checkbox').is(':checked');
            var itemCategory = row.find('.item-category-input').val() || 'Material';
            
            if (serviceName && serviceName.trim() !== '') {
                items.push({
                    ServiceId: serviceId ? parseInt(serviceId) : null,
                    ItemName: serviceName,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    IsOptional: false,
                    HasInvoice: hasInvoice,
                    Notes: hasInvoice ? 'Có hóa đơn' : 'Không có hóa đơn',
                    ServiceType: 'parts',
                    ItemCategory: itemCategory,  // ✅ THÊM ItemCategory
                });
            }
        });
        
        // Collect from Repair tab
        $('#createRepairItems .service-item-row').each(function() {
            var row = $(this);
            var serviceId = row.find('.service-id-input').val();
            var serviceName = row.find('.service-typeahead').val();
            var quantity = parseInt(row.find('.quantity-input').val()) || 1;
            var unitPriceText = row.find('.unit-price-input').val() || '';
            var unitPrice = parseFloat(unitPriceText.replace(/[^\d]/g, '')) || 0;
            var hasInvoice = row.find('.invoice-checkbox').is(':checked');
            var itemCategory = row.find('.item-category-input').val() || 'Material';
            
            if (serviceName && serviceName.trim() !== '') { // ✅ SỬA: Chỉ cần có tên dịch vụ
                items.push({
                    ServiceId: serviceId ? parseInt(serviceId) : null, // ✅ SỬA: Cho phép null
                    ItemName: serviceName,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    IsOptional: false,
                    HasInvoice: hasInvoice, // ✅ THÊM HasInvoice
                    Notes: 'Giá có thể thay đổi tùy theo mức độ hư hại',
                    ServiceType: 'repair',
                    ItemCategory: itemCategory,  // ✅ THÊM ItemCategory
                });
            }
        });
        
        // Collect from Paint tab
        $('#createPaintItems .service-item-row').each(function() {
            var row = $(this);
            var serviceId = row.find('.service-id-input').val();
            var serviceName = row.find('.service-typeahead').val();
            var quantity = parseInt(row.find('.quantity-input').val()) || 1;
            var unitPriceText = row.find('.unit-price-input').val() || '';
            var unitPrice = parseFloat(unitPriceText.replace(/[^\d]/g, '')) || 0;
            var hasInvoice = row.find('.invoice-checkbox').is(':checked');
            var itemCategory = row.find('.item-category-input').val() || 'Material';
            
            if (serviceName && serviceName.trim() !== '') { // ✅ SỬA: Chỉ cần có tên dịch vụ
                items.push({
                    ServiceId: serviceId ? parseInt(serviceId) : null, // ✅ SỬA: Cho phép null
                    ItemName: serviceName,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    IsOptional: false,
                    HasInvoice: hasInvoice, // ✅ THÊM HasInvoice
                    Notes: 'Giá có thể thay đổi tùy theo kích thước vùng bị trầy xước',
                    ServiceType: 'paint',
                    ItemCategory: itemCategory,  // ✅ THÊM ItemCategory
                });
            }
        });
        
        var formData = {
            VehicleInspectionId: parseInt($('#createVehicleInspectionId').val()),
            VehicleId: parseInt($('#createVehicleId').val()),
            CustomerId: parseInt($('#createCustomerId').val()),
            Description: $('#createDescription').val() || null,
            ValidUntil: $('#createValidUntil').val() || null,
            TaxRate: parseFloat($('#createTaxRate').val()) || 0,
            DiscountAmount: parseFloat($('#createDiscountAmount').val()) || 0,
            Items: items,
        };

        // Validate required fields
        if (!formData.VehicleInspectionId || !formData.VehicleId || !formData.CustomerId || items.length === 0) {
            GarageApp.showError('Vui lòng điền đầy đủ thông tin bắt buộc và chọn ít nhất một dịch vụ');
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
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi tạo báo giá');
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
        
        // Collect all service items from all tabs
        var items = [];
        
        // Collect from Parts tab
        $('#editPartsItems .service-item-row').each(function() {
            var row = $(this);
            var serviceId = row.find('.service-id-input').val();
            var serviceName = row.find('.service-typeahead').val();
            var quantity = parseInt(row.find('.quantity-input').val()) || 1;
            var unitPriceText = row.find('.unit-price-input').val() || '';
            var unitPrice = parseFloat(unitPriceText.replace(/[^\d]/g, '')) || 0;
            var hasInvoice = row.find('.invoice-checkbox').is(':checked');
            var itemCategory = row.find('.item-category-input').val() || 'Material';
            var vatRate = parseFloat(row.find('.vat-rate-input').val()) || 10; // ✅ THÊM: Lấy VAT rate từ input
            
            if (serviceName && serviceName.trim() !== '') {
                items.push({
                    ServiceId: serviceId ? parseInt(serviceId) : null,
                    ItemName: serviceName,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    IsOptional: false,
                    HasInvoice: hasInvoice,
                    IsVATApplicable: hasInvoice, // ✅ THÊM: VAT áp dụng nếu có hóa đơn
                    VATRate: vatRate, // ✅ THÊM: VAT rate
                    Notes: hasInvoice ? 'Có hóa đơn' : 'Không có hóa đơn',
                    ServiceType: 'parts',
                    ItemCategory: itemCategory,
                });
            }
        });
        
        // Collect from Repair tab
        $('#editRepairItems .service-item-row').each(function() {
            var row = $(this);
            var serviceId = row.find('.service-id-input').val();
            var serviceName = row.find('.service-typeahead').val();
            var quantity = parseInt(row.find('.quantity-input').val()) || 1;
            var unitPriceText = row.find('.unit-price-input').val() || '';
            var unitPrice = parseFloat(unitPriceText.replace(/[^\d]/g, '')) || 0;
            var hasInvoice = row.find('.invoice-checkbox').is(':checked');
            var itemCategory = row.find('.item-category-input').val() || 'Material';
            var vatRate = parseFloat(row.find('.vat-rate-input').val()) || 10; // ✅ THÊM: Lấy VAT rate từ input
            
            if (serviceName && serviceName.trim() !== '') {
                items.push({
                    ServiceId: serviceId ? parseInt(serviceId) : null,
                    ItemName: serviceName,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    IsOptional: false,
                    HasInvoice: hasInvoice,
                    IsVATApplicable: hasInvoice, // ✅ THÊM: VAT áp dụng nếu có hóa đơn
                    VATRate: vatRate, // ✅ THÊM: VAT rate
                    Notes: 'Giá có thể thay đổi tùy theo mức độ hư hại',
                    ServiceType: 'repair',
                    ItemCategory: itemCategory,
                });
            }
        });
        
        // Collect from Paint tab
        $('#editPaintItems .service-item-row').each(function() {
            var row = $(this);
            var serviceId = row.find('.service-id-input').val();
            var serviceName = row.find('.service-typeahead').val();
            var quantity = parseInt(row.find('.quantity-input').val()) || 1;
            var unitPriceText = row.find('.unit-price-input').val() || '';
            var unitPrice = parseFloat(unitPriceText.replace(/[^\d]/g, '')) || 0;
            var hasInvoice = row.find('.invoice-checkbox').is(':checked');
            var itemCategory = row.find('.item-category-input').val() || 'Material';
            var vatRate = parseFloat(row.find('.vat-rate-input').val()) || 10; // ✅ THÊM: Lấy VAT rate từ input
            
            if (serviceName && serviceName.trim() !== '') {
                items.push({
                    ServiceId: serviceId ? parseInt(serviceId) : null,
                    ItemName: serviceName,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    IsOptional: false,
                    HasInvoice: hasInvoice,
                    IsVATApplicable: hasInvoice, // ✅ THÊM: VAT áp dụng nếu có hóa đơn
                    VATRate: vatRate, // ✅ THÊM: VAT rate
                    Notes: 'Giá có thể thay đổi tùy theo kích thước vùng bị trầy xước',
                    ServiceType: 'paint',
                    ItemCategory: itemCategory,
                });
            }
        });
        
        var formData = {
            Id: parseInt(quotationId),
            Description: $('#editDescription').val() || null,
            Terms: $('#editTerms').val() || null,
            ValidUntil: $('#editValidUntil').val() || null,
            TaxRate: parseFloat($('#editTaxRate').val()) || 0,
            DiscountAmount: parseFloat($('#editDiscountAmount').val()) || 0,
            Items: items,
        };
        
        // ✅ DEBUG: Log items count
        items.forEach(function(item, index) {
        });
        
        // ✅ DEBUG: Log toàn bộ formData

        // Validate required fields
        if (items.length === 0) {
            GarageApp.showError('Vui lòng thêm ít nhất một dịch vụ');
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
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi cập nhật báo giá');
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
            cancelButtonText: 'Hủy',
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
                                GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi duyệt báo giá');
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
            cancelButtonText: 'Hủy',
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
                                GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi từ chối báo giá');
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
            cancelButtonText: 'Hủy',
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
                                GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi xóa báo giá');
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

    printQuotation: function(id) {
        // Mở trang in báo giá trong tab mới
        var printUrl = '/QuotationManagement/PrintQuotation/' + id;
        window.open(printUrl, '_blank', 'width=800,height=600,scrollbars=yes,resizable=yes');
    },

    populateViewModal: function(quotation) {
        var self = this;
        
        // ✅ SỬA: Handle both camelCase and PascalCase from API
        $('#viewQuotationNumber').text(quotation.quotationNumber || quotation.QuotationNumber || '');
        $('#viewVehicle').text(quotation.vehicleInfo || quotation.VehicleInfo || '');
        $('#viewCustomer').text(quotation.customerName || quotation.CustomerName || '');
        $('#viewStatus').text(quotation.status || quotation.Status || '');
        
        // ✅ SỬA: Thêm các trường bị thiếu
        $('#viewQuotationType').text(quotation.quotationType || quotation.QuotationType || 'Personal');
        $('#viewQuotationDate').text(quotation.quotationDate || quotation.QuotationDate ? new Date(quotation.quotationDate || quotation.QuotationDate).toLocaleDateString('vi-VN') : new Date().toLocaleDateString('vi-VN'));
        
        $('#viewValidUntil').text(quotation.validUntil || quotation.ValidUntil ? new Date(quotation.validUntil || quotation.ValidUntil).toLocaleDateString('vi-VN') : '');
        $('#viewDescription').text(quotation.description || quotation.Description || '');
        $('#viewTerms').text(quotation.terms || quotation.Terms || '');
        
        // Calculate and populate financial fields
        var subtotal = 0;
        var taxAmount = 0;
        var vatRate = 10; // VAT rate 10%
        
        if (quotation.items && quotation.items.length > 0) {
            quotation.items.forEach(function(item) {
                var itemSubtotal = (item.unitPrice || 0) * (item.quantity || 1);
                subtotal += itemSubtotal;
                
                // ✅ SỬA: Tính thuế VAT cho items có hóa đơn
                if (item.isVATApplicable || item.IsVATApplicable) {
                    taxAmount += itemSubtotal * vatRate / 100;
                }
            });
        }
        
        var taxRate = taxAmount > 0 ? vatRate : 0; // Hiển thị 10% nếu có thuế
        var discountAmount = quotation.discountAmount || quotation.DiscountAmount || 0;
        
        // ✅ SỬA: Tính lại TotalAmount từ SubTotal để đảm bảo tính toán đúng
        var totalAmount = subtotal + taxAmount - discountAmount;
        
        $('#viewSubTotal').text(subtotal.toLocaleString());
        $('#viewTaxRate').text(taxRate);
        $('#viewTaxAmount').text(taxAmount.toLocaleString());
        $('#viewDiscountAmount').text(discountAmount.toLocaleString());
        $('#viewTotalAmount').text(totalAmount.toLocaleString());
        
        // Populate service items
        $('#viewServiceItems').empty();
        
        if (quotation.items && quotation.items.length > 0) {
            // Add table header
            var headerHtml = `
                <table class="table table-sm table-bordered">
                    <thead class="thead-light">
                        <tr>
                            <th width="25%">Tên Dịch Vụ</th>
                            <th width="10%">Số Lượng</th>
                            <th width="15%">Đơn Giá (VNĐ)</th>
                            <th width="15%">Thành Tiền (VNĐ)</th>
                            <th width="10%">Có Hóa Đơn</th>
                            <th width="15%">Ghi Chú</th>
                        </tr>
                    </thead>
                    <tbody id="viewServiceItemsBody">
                    </tbody>
                </table>
            `;
            $('#viewServiceItems').append(headerHtml);
            
            // Add service items
            quotation.items.forEach(function(item, index) {
                var serviceItemHtml = `
                    <tr>
                        <td><strong>${item.service ? item.service.name : item.itemName || 'Dịch vụ'}</strong></td>
                        <td class="text-center"><span class="badge badge-info">${item.quantity || 1}</span></td>
                        <td class="text-right"><span class="text-muted">${item.unitPrice ? item.unitPrice.toLocaleString() + ' VNĐ' : '0 VNĐ'}</span></td>
                        <td class="text-right"><strong class="text-primary">${item.totalPrice ? item.totalPrice.toLocaleString() + ' VNĐ' : '0 VNĐ'}</strong></td>
                        <td class="text-center">
                            ${(item.isVATApplicable || item.IsVATApplicable) ? '<span class="badge badge-success">Có</span>' : '<span class="badge badge-secondary">Không</span>'}
                        </td>
                        <td><small class="text-muted">${item.notes || ''}</small></td>
                    </tr>
                `;
                $('#viewServiceItemsBody').append(serviceItemHtml);
            });
        } else {
            $('#viewServiceItems').append('<div class="text-center text-muted p-3">Không có dịch vụ nào</div>');
        }
        
        // Set data-id for print button
        $('.print-quotation').attr('data-id', quotation.id);
    },

    populateEditModal: function(quotation) {
        var self = this;
        
        // Clear existing service items from all tabs
        $('#editPartsItems').empty();
        $('#editRepairItems').empty();
        $('#editPaintItems').empty();
        
        // Populate basic fields
        $('#editId').val(quotation.id);
        $('#editVehicleId').val(quotation.vehicleId || quotation.VehicleId).trigger('change');
        $('#editCustomerId').val(quotation.customerId || quotation.CustomerId).trigger('change');
        $('#editDescription').val(quotation.description || quotation.Description || '');
        $('#editValidUntil').val(quotation.validUntil || quotation.ValidUntil ? new Date(quotation.validUntil || quotation.ValidUntil).toISOString().split('T')[0] : '');
        // ✅ XÓA: Không còn field VAT chung, mỗi item có VAT riêng
        $('#editDiscountAmount').val(quotation.discountAmount || quotation.DiscountAmount || 0);
        
        // Load service items if they exist
        if (quotation.items && quotation.items.length > 0) {
            quotation.items.forEach(function(item, index) {
                // Add a new service item row with data to the appropriate tab
                self.addServiceItemWithData('edit', item);
            });
            
            // ✅ THÊM: Tính lại "Thành tiền" bao gồm VAT cho tất cả items sau khi load
            setTimeout(function() {
                self.recalculateAllTotalsWithVAT('edit', quotation.taxRate || 10);
            }, 100); // Delay để đảm bảo DOM đã được render
        }
    },

    // ✅ THÊM: File Attachment Management
    currentQuotationId: null,

    // Load attachments for a quotation
    loadAttachments: function(quotationId) {
        var self = this;
        self.currentQuotationId = quotationId;
        
        $.ajax({
            url: '/QuotationManagement/GetAttachments/' + quotationId,
            type: 'GET',
            success: function(response) {
                if (response.success) {
                    self.displayAttachments(response.data);
                } else {
                    $('#attachmentsList').html('<div class="alert alert-warning">Không thể tải danh sách file đính kèm</div>');
                }
            },
            error: function() {
                $('#attachmentsList').html('<div class="alert alert-danger">Lỗi khi tải danh sách file đính kèm</div>');
            }
        });
    },

    // Display attachments list
    displayAttachments: function(attachments) {
        var html = '';
        
        if (attachments && attachments.length > 0) {
            html += '<div class="table-responsive">';
            html += '<table class="table table-sm table-hover">';
            html += '<thead class="thead-light">';
            html += '<tr><th>Tên File</th><th>Loại</th><th>Kích Thước</th><th>Ngày Upload</th><th>Thao Tác</th></tr>';
            html += '</thead><tbody>';
            
            attachments.forEach(function(attachment) {
                var fileSize = self.formatFileSize(attachment.fileSize);
                var uploadDate = new Date(attachment.uploadedDate).toLocaleDateString('vi-VN');
                var badgeClass = attachment.isInsuranceDocument ? 'badge-warning' : 'badge-secondary';
                var badgeText = attachment.isInsuranceDocument ? 'Bảo Hiểm' : 'Thường';
                
                html += '<tr>';
                html += '<td><i class="fas fa-file mr-2"></i>' + attachment.fileName + '</td>';
                html += '<td><span class="badge ' + badgeClass + '">' + badgeText + '</span></td>';
                html += '<td>' + fileSize + '</td>';
                html += '<td>' + uploadDate + '</td>';
                html += '<td>';
                html += '<button class="btn btn-sm btn-info mr-1 download-attachment" data-id="' + attachment.id + '" title="Download">';
                html += '<i class="fas fa-download"></i></button>';
                html += '<button class="btn btn-sm btn-danger delete-attachment" data-id="' + attachment.id + '" title="Xóa">';
                html += '<i class="fas fa-trash"></i></button>';
                html += '</td>';
                html += '</tr>';
            });
            
            html += '</tbody></table></div>';
        } else {
            html = '<div class="alert alert-info"><i class="fas fa-info-circle mr-2"></i>Chưa có file đính kèm nào</div>';
        }
        
        $('#attachmentsList').html(html);
    },

    // Format file size
    formatFileSize: function(bytes) {
        if (bytes === 0) return '0 Bytes';
        var k = 1024;
        var sizes = ['Bytes', 'KB', 'MB', 'GB'];
        var i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    },

    // Upload attachment
    uploadAttachment: function() {
        var self = this;
        var formData = new FormData();
        var file = $('#attachmentFile')[0].files[0];
        
        if (!file) {
            Swal.fire('Lỗi', 'Vui lòng chọn file', 'error');
            return;
        }
        
        formData.append('quotationId', self.currentQuotationId);
        formData.append('file', file);
        formData.append('attachmentType', $('#attachmentType').val());
        formData.append('description', $('#attachmentDescription').val());
        formData.append('isInsuranceDocument', $('#isInsuranceDocument').is(':checked'));
        
        $.ajax({
            url: '/QuotationManagement/UploadAttachment',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                if (response.success) {
                    Swal.fire('Thành công', 'Upload file thành công', 'success');
                    $('#uploadAttachmentModal').modal('hide');
                    self.loadAttachments(self.currentQuotationId);
                    $('#uploadAttachmentForm')[0].reset();
                } else {
                    Swal.fire('Lỗi', response.message || 'Upload file thất bại', 'error');
                }
            },
            error: function() {
                Swal.fire('Lỗi', 'Upload file thất bại', 'error');
            }
        });
    },

    // Delete attachment
    deleteAttachment: function(attachmentId) {
        var self = this;
        
        Swal.fire({
            title: 'Xác nhận xóa',
            text: 'Bạn có chắc chắn muốn xóa file này?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/QuotationManagement/DeleteAttachment/' + attachmentId,
                    type: 'DELETE',
                    success: function(response) {
                        if (response.success) {
                            Swal.fire('Thành công', 'Xóa file thành công', 'success');
                            self.loadAttachments(self.currentQuotationId);
                        } else {
                            Swal.fire('Lỗi', response.message || 'Xóa file thất bại', 'error');
                        }
                    },
                    error: function() {
                        Swal.fire('Lỗi', 'Xóa file thất bại', 'error');
                    }
                });
            }
        });
    },

    // Download attachment
    downloadAttachment: function(attachmentId) {
        window.open('/api/quotationattachments/' + attachmentId + '/download', '_blank');
    }
};

$(document).ready(function() {
    QuotationManagement.init();
});
