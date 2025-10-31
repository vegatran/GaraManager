/**
 * Quotation Management Module
 * 
 * Handles all service quotation-related operations
 * CRUD operations for service quotations
 */

// ✅ SỬA: Định nghĩa module trước, sau đó wrap trong document ready
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
                    var displayText = data;
                    
                    // Translate status to Vietnamese
                    switch(data) {
                        case 'Draft': 
                            badgeClass = 'badge-light'; 
                            displayText = 'Nháp';
                            break;
                        case 'Pending': 
                            badgeClass = 'badge-warning'; 
                            displayText = 'Chờ Duyệt';
                            break;
                        case 'Sent': 
                            badgeClass = 'badge-info'; 
                            displayText = 'Đã Gửi';
                            break;
                        case 'Accepted': 
                            badgeClass = 'badge-success'; 
                            displayText = 'Đã Chấp Nhận';
                            break;
                        case 'Approved': 
                            badgeClass = 'badge-success'; 
                            displayText = 'Đã Duyệt';
                            break;
                        case 'Rejected': 
                            badgeClass = 'badge-danger'; 
                            displayText = 'Đã Từ Chối';
                            break;
                        case 'Expired': 
                            badgeClass = 'badge-warning'; 
                            displayText = 'Hết Hạn';
                            break;
                        case 'Converted': 
                            badgeClass = 'badge-primary'; 
                            displayText = 'Đã Chuyển Đổi';
                            break;
                    }
                    return `<span class="badge ${badgeClass}">${displayText}</span>`;
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
                    // ✅ SỬA: Lấy cả status đã dịch và status gốc để check chính xác
                    var status = row.status || row.statusOriginal; // Status đã dịch (tiếng Việt)
                    var statusOriginal = row.statusOriginal || row.status; // Status gốc (tiếng Anh)
                    
                    var buttons = `
                        <div class="btn-group" role="group">
                            <button type="button" class="btn btn-info btn-sm view-quotation" data-id="${row.id}" title="Xem">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button type="button" class="btn btn-primary btn-sm print-quotation" data-id="${row.id}" title="In Báo Giá">
                                <i class="fas fa-print"></i>
                            </button>
                    `;
                    
                    // ✅ 2.1.1: Chỉ hiển thị nút Edit khi chưa được duyệt và chưa có ServiceOrder
                    var hasServiceOrder = row.serviceOrderId !== null && row.serviceOrderId !== undefined && row.serviceOrderId > 0;
                    if (!hasServiceOrder && 
                        (status !== 'Approved' && status !== 'Đã duyệt') && 
                        (statusOriginal !== 'Approved') &&
                        (status !== 'Completed' && status !== 'Hoàn thành')) {
                        buttons += `
                            <button type="button" class="btn btn-warning btn-sm edit-quotation" data-id="${row.id}" title="Sửa">
                                <i class="fas fa-edit"></i>
                            </button>
                        `;
                    }
                    
                    // ✅ SỬA: Hiển thị nút Duyệt/Từ chối - check cả status tiếng Việt và tiếng Anh
                    var canApproveReject = false;
                    
                    // Check theo status đã dịch (tiếng Việt)
                    if (status === 'Draft' || status === 'Nháp' || 
                        status === 'Sent' || status === 'Đã gửi' || status === 'Đã Gửi' ||
                        status === 'Pending' || status === 'Chờ duyệt' || status === 'Chờ Duyệt') {
                        canApproveReject = true;
                    }
                    
                    // Check theo status gốc (tiếng Anh) để đảm bảo
                    if (statusOriginal === 'Draft' || statusOriginal === 'Sent' || statusOriginal === 'Pending') {
                        canApproveReject = true;
                    }
                    
                    if (canApproveReject) {
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
                    
                    // ✅ THÊM: Button cập nhật bảng giá bảo hiểm (chỉ cho xe bảo hiểm)
                    if (row.quotationType === 'Insurance') {
                        var buttonClass = (status === 'Approved' || status === 'Đã duyệt') ? 'btn-success' : 'btn-secondary';
                        var buttonTitle = (status === 'Approved' || status === 'Đã duyệt') ? 'Đã duyệt bảo hiểm' : 'Cập nhật bảng giá bảo hiểm';
                        buttons += `
                            <button type="button" class="btn ${buttonClass} btn-sm insurance-pricing-btn" data-id="${row.id}" title="${buttonTitle}">
                                <i class="fas fa-shield-alt"></i>
                            </button>
                        `;
                    }
                    
                    // ✅ THÊM: Button cập nhật bảng giá công ty (chỉ cho xe công ty)
                    if (row.quotationType === 'Corporate') {
                        var buttonClass = (status === 'Approved' || status === 'Đã duyệt') ? 'btn-success' : 'btn-secondary';
                        var buttonTitle = (status === 'Approved' || status === 'Đã duyệt') ? 'Đã duyệt công ty' : 'Cập nhật bảng giá công ty';
                        buttons += `
                            <button type="button" class="btn ${buttonClass} btn-sm corporate-pricing-btn" data-id="${row.id}" title="${buttonTitle}">
                                <i class="fas fa-building"></i>
                            </button>
                        `;
                    }
                    
                    // ✅ THÊM: Button duyệt báo giá (chỉ cho xe cá nhân đang Pending)
                    if (row.quotationType === 'Personal' && (status === 'Pending' || status === 'Chờ duyệt')) {
                        buttons += `
                            <button type="button" class="btn btn-success btn-sm approve-personal-quotation" data-id="${row.id}" title="Duyệt báo giá">
                                <i class="fas fa-check"></i>
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
        
        // ✅ THÊM: Submit approve form
        $(document).on('click', '#btnSubmitApproveQuotation', function() {
            self.submitApproveQuotation();
        });
        
        // ✅ THÊM: Submit reject form
        $(document).on('click', '#btnSubmitRejectQuotation', function() {
            self.submitRejectQuotation();
        });

        // Delete quotation
        $(document).on('click', '.delete-quotation', function() {
            var id = $(this).data('id');
            self.deleteQuotation(id);
        });

        // Insurance pricing button
        $(document).on('click', '.insurance-pricing-btn', function() {
            var id = $(this).data('id');
            self.showInsurancePricingModal(id);
        });

        // ✅ THÊM: Approve personal quotation button
        $(document).on('click', '.approve-personal-quotation', function() {
            var id = $(this).data('id');
            self.approvePersonalQuotation(id);
        });

        // ✅ THÊM: Corporate pricing button
        $(document).on('click', '.corporate-pricing-btn', function() {
            var id = $(this).data('id');
            self.showCorporatePricingModal(id);
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
            var targetTab = $(e.target).attr('href'); // #edit-parts, #edit-repair, #edit-paint or create-*
            var tabId = targetTab.replace('#', ''); // edit-parts, edit-repair, ...
            currentActiveTab = tabId; // ✅ LƯU tab active hiện tại

            // ✅ SỬA: Re-init typeahead cho các input trong tab vừa hiển thị
            var $container = $(targetTab);
            $container.find('.service-typeahead').each(function() {
                var $input = $(this);
                // Nếu chưa gắn typeahead thì khởi tạo; nếu đã có, trigger refresh bằng cách reattach source
                if (!$input.data('typeahead')) {
                    self.initializeServiceTypeahead($input);
                }
            });
        });
        
        // ✅ THÊM: Event handler cho checkbox - sử dụng currentActiveTab
        $(document).on('change', '.invoice-checkbox', function(e) {
            e.stopPropagation();
            
            var row = $(this).closest('.service-item-row');
            var isChecked = $(this).is(':checked');
            var vatRateInput = row.find('.vat-rate-input');
            var vatAmountInput = row.find('.vat-amount-input');
            
            // ✅ SỬA: Disable/enable VAT input và set value về 0 khi uncheck
            // ✅ THÊM: Kiểm tra xem có phải phụ tùng từ kho không (READ-ONLY VAT)
            var isPartsTab = row.closest('#createPartsItems, #editPartsItems').length > 0;
            var hasPartId = row.find('.service-id-input').val() && row.find('.service-id-input').val() !== '';
            
            // ✅ SỬA: Kiểm tra ServiceType để xác định Parts item
            var serviceTypeInput = row.find('.service-type-input').val();
            var actualIsPartsItem = serviceTypeInput && serviceTypeInput.toLowerCase() === 'parts';
            
            if (isPartsTab && (hasPartId || actualIsPartsItem)) {
                // ✅ THÊM: Đối với phụ tùng từ kho, VAT không được chỉnh sửa
                if (isChecked) {
                    // Enable VAT input nhưng vẫn readonly
                    vatRateInput.prop('disabled', false).prop('readonly', true);
                    vatAmountInput.prop('disabled', false);
                    
                    // ✅ SỬA: Khôi phục VAT rate nếu đã bị set về 0
                    var currentVATRate = parseFloat(vatRateInput.val()) || 0;
                    if (currentVATRate === 0) {
                        // Lấy VAT rate từ data attribute hoặc default 10
                        var restoredVATRate = vatRateInput.data('original-vat-rate') || 10;
                        vatRateInput.val(restoredVATRate);
                    }
                    
                    row.addClass('table-success');
                } else {
                    // Disable VAT input và set về 0
                    // ✅ SỬA: Lưu VAT rate hiện tại vào data attribute để khôi phục sau
                    var currentVATRate = parseFloat(vatRateInput.val()) || 0;
                    if (currentVATRate > 0) {
                        vatRateInput.data('original-vat-rate', currentVATRate);
                    }
                    vatRateInput.prop('disabled', true).val('0');
                    vatAmountInput.prop('disabled', true).val('0 VNĐ');
                    row.removeClass('table-success');
                }
            } else {
                // ✅ SỬA: Logic cho Services (có thể chỉnh sửa VAT)
                if (isChecked) {
                    // Enable VAT input và set default value nếu cần
                    vatRateInput.prop('disabled', false).prop('readonly', false);
                    vatAmountInput.prop('disabled', false);
                    
                    // ✅ SỬA: Khôi phục VAT rate nếu đã bị set về 0
                    var currentVATRate = parseFloat(vatRateInput.val()) || 0;
                    if (currentVATRate === 0) {
                        // Lấy VAT rate từ data attribute hoặc default 10
                        var restoredVATRate = vatRateInput.data('original-vat-rate') || 10;
                        vatRateInput.val(restoredVATRate);
                    }
                    
                    row.addClass('table-success');
                } else {
                    // Disable VAT input và set về 0
                    // ✅ SỬA: Lưu VAT rate hiện tại vào data attribute để khôi phục sau
                    var currentVATRate = parseFloat(vatRateInput.val()) || 0;
                    if (currentVATRate > 0) {
                        vatRateInput.data('original-vat-rate', currentVATRate);
                    }
                    vatRateInput.prop('disabled', true).val('0');
                    vatAmountInput.prop('disabled', true).val('0 VNĐ');
                    row.removeClass('table-success');
                }
            }
            
            // ✅ THÊM: Recalculate totals
            self.calculateItemTotal(row);
            
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
        // ✅ SỬA: Reset toàn bộ form và modal
        $('#createQuotationForm')[0].reset();
        
        // Clear existing service items
        $('#createPartsItems').empty();
        $('#createRepairItems').empty();
        $('#createPaintItems').empty();
        
        // ✅ THÊM: Reset các field quan trọng về giá trị mặc định
        $('#createVehicleInspectionId').val('').trigger('change');
        $('#createCustomerId').val('').trigger('change');
        $('#createVehicleId').val('').trigger('change');
        $('#createQuotationType').val('Personal');
        // ✅ REMOVED: Không còn field VAT ở cấp độ báo giá, VAT được tính tự động theo từng item
        $('#createDiscountAmount').val('0');
        $('#createStatus').val('Draft');
        $('#createDescription').val('');
        $('#createTerms').val('');
        
        // ✅ THÊM: Reset tổng tiền về 0
        $('#createSubTotal').text('0 VNĐ');
        $('#createTaxAmount').text('0 VNĐ');
        $('#createTotalAmount').text('0 VNĐ');
        
        // ✅ XÓA: Không thêm hàng mẫu nữa để modal trống hoàn toàn
        // this.addSampleRow();
        
        // ✅ THÊM: Set default date (30 days from now)
        var defaultDate = new Date();
        defaultDate.setDate(defaultDate.getDate() + 30);
        $('#createValidUntil').val(defaultDate.toISOString().split('T')[0]);
        
        // ✅ THÊM: Force clear any remaining data
        this.clearAllCreateData();
        
        $('#createQuotationModal').modal('show');
    },

    // ✅ THÊM: Function clear hoàn toàn dữ liệu tạo mới
    clearAllCreateData: function() {
        // Clear all service items tables
        $('#createPartsItems').empty();
        $('#createRepairItems').empty();
        $('#createPaintItems').empty();
        
        // Clear all form inputs
        $('#createQuotationForm input[type="text"]').val('');
        $('#createQuotationForm input[type="number"]').val('');
        $('#createQuotationForm textarea').val('');
        $('#createQuotationForm select').val('').trigger('change');
        
        // Reset specific fields to defaults
        $('#createQuotationType').val('Personal');
        // ✅ REMOVED: Không còn field VAT ở cấp độ báo giá, VAT được tính tự động theo từng item
        $('#createDiscountAmount').val('0');
        $('#createStatus').val('Draft');
        
        // Reset totals
        $('#createSubTotal').text('0 VNĐ');
        $('#createTaxAmount').text('0 VNĐ');
        $('#createTotalAmount').text('0 VNĐ');
        
        // Set default date
        var defaultDate = new Date();
        defaultDate.setDate(defaultDate.getDate() + 30);
        $('#createValidUntil').val(defaultDate.toISOString().split('T')[0]);
    },

    // ✅ THÊM: Function thêm hàng mẫu để demo VAT
    addSampleRow: function() {
        var sampleHtml = `
            <tr class="service-item-row">
                <td>
                    <input type="text" class="form-control form-control-sm service-typeahead" 
                           placeholder="Gõ tên phụ tùng..." 
                           name="Items[0].ServiceName"
                           data-service-id=""
                           data-service-type="parts"
                           autocomplete="off"
                           value="Phụ tùng mẫu">
                    <input type="hidden" class="service-id-input" name="Items[0].ServiceId">
                    <input type="hidden" class="service-type-input" name="Items[0].ServiceType" value="parts">
                    <input type="hidden" class="item-category-input" name="Items[0].ItemCategory" value="Material">
                </td>
                <td>
                    <input type="number" class="form-control form-control-sm quantity-input text-center" 
                           name="Items[0].Quantity" value="2" min="1" 
                           placeholder="1" title="Số lượng">
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm unit-price-input text-right" 
                           placeholder="0" readonly title="Đơn giá" value="500,000 VNĐ">
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm subtotal-input text-right" 
                           placeholder="0" readonly title="Thành tiền chưa VAT" value="1,000,000 VNĐ">
                </td>
                <td>
                    <input type="number" class="form-control form-control-sm vat-rate-input text-center" 
                           name="Items[0].VATRate" value="10" min="0" max="100" step="0.1"
                           placeholder="10" title="Tỷ lệ VAT (%)">
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm vat-amount-input text-right" 
                           placeholder="0" readonly title="Tiền VAT" value="100,000 VNĐ">
                </td>
                <td class="text-center">
                    <div class="custom-control custom-checkbox">
                        <input class="custom-control-input invoice-checkbox" type="checkbox" 
                               name="Items[0].HasInvoice" id="invoice_create_0" checked>
                        <label class="custom-control-label" for="invoice_create_0"></label>
                    </div>
                </td>
                <td class="text-center">
                    <button type="button" class="btn btn-sm btn-outline-danger remove-service-item" title="Xóa">
                        <i class="fas fa-times"></i>
                    </button>
                </td>
            </tr>
        `;
        
        $('#createPartsItems').append(sampleHtml);
        
        // Bind events cho hàng mẫu
        this.bindServiceItemEvents('create');
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
                                <input type="text" class="form-control form-control-sm subtotal-input text-right" 
                                       placeholder="0" readonly title="Thành tiền chưa VAT">
                            </td>
                            <td>
                                <input type="number" class="form-control form-control-sm vat-rate-input text-center" 
                                       name="Items[${itemIndex}].VATRate" value="10" min="0" max="100" step="0.1"
                                       placeholder="10" title="Tỷ lệ VAT (%)">
                            </td>
                            <td>
                                <input type="text" class="form-control form-control-sm vat-amount-input text-right" 
                                       placeholder="0" readonly title="Tiền VAT">
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
                    
                    // ✅ THÊM: Disable VAT input cho item mới (checkbox mặc định unchecked)
                    var lastRow = $('#' + containerId + ' .service-item-row').last();
                    lastRow.find('.vat-rate-input').prop('disabled', true).val('0');
                    lastRow.find('.vat-amount-input').prop('disabled', true).val('0 VNĐ');
                    
                    // ✅ THÊM: Nếu là tab Parts, thêm tooltip READ-ONLY
                    if (serviceType === 'parts') {
                        lastRow.find('.vat-rate-input').attr('title', 'VAT sẽ được lấy từ thông tin phụ tùng (Không được chỉnh sửa)');
                    }
                    
                    // Initialize typeahead for new service input (ensure visible in current tab)
                    var $newInput = $('#' + containerId + ' .service-typeahead').last();
                    self.initializeServiceTypeahead($newInput, prefix);
                    // In some cases, plugin needs a micro delay after append before lookup
                    setTimeout(function() {
                        if (!$newInput.data('typeahead')) {
                            self.initializeServiceTypeahead($newInput, prefix);
                        }
                        $newInput.focus();
                        try { $newInput.typeahead('lookup'); } catch(e) { /* no-op */ }
                    }, 0);
                    
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
        var laborOptions = {
            'parts': [
                'Công lắp đặt phụ tùng',
                'Công thay thế phụ tùng',
                'Công kiểm tra và điều chỉnh'
            ],
            'repair': [
                'Công sửa chữa động cơ',
                'Công sửa chữa hệ thống điện',
                'Công sửa chữa thân xe',
                'Công sửa chữa gầm xe',
                'Công sửa chữa hệ thống làm mát',
                'Công sửa chữa hệ thống phanh',
                'Công sửa chữa hệ thống lái',
                'Công kiểm tra và chẩn đoán',
                'Khác'
            ],
            'paint': [
                'Công sơn cánh',
                'Công sơn cửa',
                'Công sơn nắp capo',
                'Công sơn nắp thùng',
                'Công sơn mui xe',
                'Công sơn cản trước',
                'Công sơn cản sau',
                'Công sơn toàn thân xe',
                'Công sơn chi tiết',
                'Khác'
            ]
        };
        
        var options = laborOptions[serviceType] || ['Công lao động', 'Khác'];
        var defaultOption = options[0];
        var optionsHtml = options.map(function(opt, index) {
            var selected = index === 0 ? 'selected' : '';
            return `<option value="${opt}" ${selected}>${opt}</option>`;
        }).join('');
        
        var itemId = 'item_' + Date.now();
        
        // ✅ SỬA: Tính itemIndex dựa trên prefix
        var itemIndex;
        if (prefix === 'create') {
            itemIndex = $('#createPartsItems .service-item-row, #createRepairItems .service-item-row, #createPaintItems .service-item-row').length;
        } else {
            itemIndex = $('#editPartsItems .service-item-row, #editRepairItems .service-item-row, #editPaintItems .service-item-row').length;
        }
        
        return `
            <tr class="service-item-row" data-item-id="${itemId}">
                <td>
                    <input type="hidden" class="service-id-input" name="Items[${itemIndex}].ServiceId" value="">
                    <input type="hidden" class="item-category-input" name="Items[${itemIndex}].ItemCategory" value="Labor">
                    <select class="form-control form-control-sm labor-name-select" 
                            name="Items[${itemIndex}].ServiceName" data-service-type="${serviceType}">
                        ${optionsHtml}
                    </select>
                    <input type="text" class="form-control form-control-sm labor-name-custom d-none mt-1" 
                           placeholder="Nhập tên công..." style="display: none;">
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
                    <input type="text" class="form-control form-control-sm subtotal-input text-right" 
                           name="Items[${itemIndex}].SubTotal" placeholder="0" readonly title="Thành tiền chưa VAT">
                </td>
                <td>
                    <input type="number" class="form-control form-control-sm vat-rate-input text-center" 
                           name="Items[${itemIndex}].VATRate" value="0" min="0" max="100" step="0.1"
                           placeholder="0" title="Tỷ lệ VAT (%)" disabled>
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm vat-amount-input text-right" 
                           placeholder="0" readonly title="Tiền VAT" disabled>
                </td>
                <td class="text-center">
                    <div class="custom-control custom-checkbox">
                        <input class="custom-control-input invoice-checkbox" type="checkbox" 
                               name="Items[${itemIndex}].HasInvoice" id="invoice_labor_${prefix}_${itemIndex}">
                        <label class="custom-control-label" for="invoice_labor_${prefix}_${itemIndex}"></label>
                    </div>
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
        
        // ✅ DEBUG: Log itemData nhận được
        console.log('🔍 DEBUG addServiceItemWithData - itemData:', itemData);
        
        // ✅ CẬP NHẬT: Tính toán VAT từ dữ liệu
        var quantity = itemData.quantity || 1;
        var unitPrice = itemData.unitPrice || 0;
        var serviceType = itemData.serviceType || itemData.ServiceType || '';
        var isPartsItem = serviceType.toLowerCase() === 'parts';
        
        // ✅ SỬA: Lấy VAT rate từ API - ưu tiên PartVATRate nếu là Parts, nếu không thì dùng vatRate từ item
        var partVATRate = itemData.partVATRate !== undefined ? itemData.partVATRate : 
                          (itemData.PartVATRate !== undefined ? itemData.PartVATRate : null);
        var itemVATRate = itemData.vatRate !== undefined ? itemData.vatRate : 
                          (itemData.VATRate !== undefined ? itemData.VATRate : null);
        
        var vatRate;
        if (isPartsItem && partVATRate !== null && partVATRate !== undefined) {
            // Nếu là Parts và có PartVATRate, dùng VAT từ Part (READ-ONLY)
            vatRate = partVATRate;
        } else if (itemVATRate !== null && itemVATRate !== undefined) {
            // Nếu có VATRate từ item, dùng nó (trường hợp này luôn ưu tiên)
            vatRate = itemVATRate;
        } else {
            // Default fallback
            vatRate = 10;
        }
        
        // ✅ SỬA: Đảm bảo vatRate là số hợp lệ
        vatRate = parseFloat(vatRate) || 0;
        
        var hasInvoice = itemData.hasInvoice !== undefined ? itemData.hasInvoice : 
                        (itemData.HasInvoice !== undefined ? itemData.HasInvoice : false);
        var isVATApplicable = itemData.isVATApplicable !== undefined ? itemData.isVATApplicable : 
                             (itemData.IsVATApplicable !== undefined ? itemData.IsVATApplicable : 
                              (hasInvoice || false));
        
        // ✅ DEBUG: Log các giá trị được parse
        console.log('🔍 DEBUG addServiceItemWithData - Parsed values:', {
            quantity: quantity,
            unitPrice: unitPrice,
            vatRate: vatRate,
            hasInvoice: hasInvoice,
            isVATApplicable: isVATApplicable,
            isPartsItem: isPartsItem,
            serviceType: serviceType,
            partVATRate: partVATRate,
            itemDataVATRate: itemData.vatRate || itemData.VATRate
        });
        
        var subtotal = quantity * unitPrice;
        var vatAmount = 0;
        var totalPrice = subtotal;
        
        // ✅ SỬA: Tính VAT dựa trên isVATApplicable - luôn tính nếu có isVATApplicable và vatRate > 0
        if (isVATApplicable && vatRate > 0) {
            vatAmount = subtotal * (vatRate / 100);
            totalPrice = subtotal + vatAmount;
        } else {
            vatAmount = 0;
            totalPrice = subtotal;
        }
        
        var serviceItemHtml = `
            <tr class="service-item-row">
                <td>
                    <input type="text" class="form-control form-control-sm service-typeahead" 
                           placeholder="Gõ tên dịch vụ..." data-service-id="${itemData.serviceId || ''}"
                           value="${itemData.itemName || ''}">
                    <input type="hidden" class="service-id-input" value="${itemData.serviceId || itemData.ServiceId || ''}">
                    <input type="hidden" class="service-type-input" value="${serviceType || ''}">
                    <input type="hidden" class="item-category-input" value="${itemData.itemCategory || itemData.ItemCategory || 'Material'}">
                </td>
                <td>
                    <input type="number" class="form-control form-control-sm quantity-input" 
                           value="${quantity}" min="1" placeholder="1">
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm unit-price-input text-right" 
                           value="${unitPrice.toLocaleString() + ' VNĐ'}" placeholder="0" readonly title="Đơn giá">
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm subtotal-input text-right" 
                           value="${subtotal.toLocaleString() + ' VNĐ'}" placeholder="0" readonly title="Thành tiền chưa VAT">
                </td>
                <td>
                    <input type="number" class="form-control form-control-sm vat-rate-input text-center" 
                           value="${vatRate}" min="0" max="100" step="0.1" 
                           placeholder="10" title="${isPartsItem ? 'VAT từ phụ tùng (Không được chỉnh sửa)' : 'VAT (%)'}"
                           ${isPartsItem && partVATRate !== null ? 'readonly' : ''}>
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm vat-amount-input text-right" 
                           value="${vatAmount.toLocaleString() + ' VNĐ'}" placeholder="0" readonly title="Tiền VAT">
                </td>
                <td class="text-center">
                    <div class="custom-control custom-checkbox">
                        <input class="custom-control-input invoice-checkbox" type="checkbox" 
                               name="Items[${itemIndex}].HasInvoice" id="invoice_${mode}_${itemIndex}"
                               ${(isVATApplicable || itemData.hasInvoice || itemData.HasInvoice || (itemData.notes && itemData.notes.includes('Có hóa đơn'))) ? 'checked' : ''}>
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
        
        // ✅ SỬA: Xử lý VAT input dựa trên isVATApplicable từ API và ServiceType
        var isInvoiceChecked = lastRow.find('.invoice-checkbox').is(':checked');
        var serviceTypeInput = lastRow.find('.service-type-input').val();
        var actualIsPartsItem = isPartsItem || (serviceTypeInput && serviceTypeInput.toLowerCase() === 'parts');
        
        // ✅ SỬA: Ưu tiên isVATApplicable từ API thay vì chỉ dựa vào checkbox
        // Đảm bảo VAT rate và amount được set đúng giá trị từ API
        lastRow.find('.vat-rate-input').val(vatRate);
        // ✅ SỬA: Lưu VAT rate vào data attribute để khôi phục khi check checkbox
        if (vatRate > 0) {
            lastRow.find('.vat-rate-input').data('original-vat-rate', vatRate);
        }
        lastRow.find('.vat-amount-input').val(vatAmount.toLocaleString() + ' VNĐ');
        
        if (!isVATApplicable && !isInvoiceChecked) {
            // Nếu không có VAT và checkbox không check, disable VAT
            lastRow.find('.vat-rate-input').prop('disabled', true).prop('readonly', false).val('0');
            lastRow.find('.vat-amount-input').prop('disabled', true).val('0 VNĐ');
        } else {
            // ✅ SỬA: Nếu có VAT (isVATApplicable = true), đảm bảo VAT được enable
            lastRow.find('.vat-amount-input').prop('disabled', false);
            
            if (actualIsPartsItem) {
                // ✅ SỬA: Đối với phụ tùng (ServiceType = "parts"), VAT không được chỉnh sửa - READ-ONLY
                lastRow.find('.vat-rate-input').prop('disabled', false).prop('readonly', true);
                lastRow.find('.vat-rate-input').addClass('bg-light text-muted');
                lastRow.find('.vat-rate-input').attr('title', `VAT từ phụ tùng: ${vatRate}% (Không được chỉnh sửa)`);
            } else {
                // Đối với Service items, cho phép chỉnh sửa VAT
                lastRow.find('.vat-rate-input').prop('disabled', false).prop('readonly', false);
                lastRow.find('.vat-rate-input').removeClass('bg-light text-muted');
                lastRow.find('.vat-rate-input').attr('title', 'VAT (%)');
            }
        }
        
        // Initialize typeahead for new service input
        var $newInput2 = $('#' + containerId + ' .service-typeahead').last();
        self.initializeServiceTypeahead($newInput2, prefix);
        setTimeout(function() {
            if (!$newInput2.data('typeahead')) {
                self.initializeServiceTypeahead($newInput2, prefix);
            }
            $newInput2.focus();
            try { $newInput2.typeahead('lookup'); } catch(e) { /* no-op */ }
        }, 0);
        
        // Bind change events for new item
        self.bindServiceItemEvents(prefix);
    },

    // ✅ THÊM: Helper function để parse giá trị tiền tệ
    parseCurrencyValue: function(currencyText) {
        if (!currencyText) return 0;
        // Loại bỏ dấu chấm (nghìn), dấu phẩy (thập phân) và ký hiệu tiền tệ ' VNĐ'
        var cleanedText = currencyText.replace(/\./g, '').replace(/,/g, '').replace(' VNĐ', '');
        return parseFloat(cleanedText) || 0;
    },

    // ✅ THÊM: Function tính toán thành tiền cho từng item
    calculateItemTotal: function($row) {
        var quantity = parseFloat($row.find('.quantity-input').val()) || 0;
        
        // ✅ SỬA: Parse đúng giá trị từ input có format "500.000 VNĐ"
        var unitPriceText = $row.find('.unit-price-input').val() || '';
        var unitPrice = this.parseCurrencyValue(unitPriceText);
        
        var vatRate = parseFloat($row.find('.vat-rate-input').val()) || 0;
        var isVATApplicable = $row.find('.invoice-checkbox').is(':checked');

        var itemSubtotal = quantity * unitPrice;
        var vatAmount = 0;
        var itemTotalPrice = itemSubtotal;

        if (isVATApplicable && vatRate > 0) {
            vatAmount = itemSubtotal * (vatRate / 100);
            itemTotalPrice = itemSubtotal + vatAmount;
        }

        // ✅ CẬP NHẬT: Hiển thị subtotal, VAT amount và total
        $row.find('.subtotal-input').val(itemSubtotal.toLocaleString() + ' VNĐ');
        $row.find('.vat-amount-input').val(vatAmount.toLocaleString() + ' VNĐ');
        
        // ✅ THÊM: Cột tổng tiền cuối cùng (nếu cần)
        if ($row.find('.total-input').length > 0) {
            $row.find('.total-input').val(itemTotalPrice.toLocaleString() + ' VNĐ');
        }
        
        this.calculateOverallTotals();
    },

    // ✅ THÊM: Function tính tổng cộng
    calculateOverallTotals: function() {
        var self = this;
        var subtotal = 0;
        var taxAmount = 0;
        var discountAmount = 0;

        // ✅ CẬP NHẬT: Xử lý cả create và edit modal
        var containers = ['#createPartsItems', '#createRepairItems', '#createPaintItems', '#editPartsItems', '#editRepairItems', '#editPaintItems'];
        
        containers.forEach(function(container) {
            $(container + ' tr').each(function() {
            var $row = $(this);
            var quantity = parseFloat($row.find('.quantity-input').val()) || 0;
                
                // ✅ SỬA: Parse đúng giá trị từ input có format "500.000 VNĐ"
                var unitPriceText = $row.find('.unit-price-input').val() || '';
                var unitPrice = self.parseCurrencyValue(unitPriceText);
                
            var vatRate = parseFloat($row.find('.vat-rate-input').val()) || 0;
            var isVATApplicable = $row.find('.invoice-checkbox').is(':checked');

            var itemSubtotal = quantity * unitPrice;
            subtotal += itemSubtotal;

                if (isVATApplicable && vatRate > 0) {
                taxAmount += itemSubtotal * (vatRate / 100);
            }
            });
        });

        // ✅ CẬP NHẬT: Lấy discount từ cả create và edit modal
        var createDiscount = parseFloat($('#createDiscountAmount').val()) || 0;
        var editDiscount = parseFloat($('#editDiscountAmount').val()) || 0;
        discountAmount = createDiscount || editDiscount;

        var totalAmount = subtotal + taxAmount - discountAmount;

        // ✅ CẬP NHẬT: Update display cho cả create và edit modal
        if ($('#createSubTotal').length) $('#createSubTotal').text(subtotal.toLocaleString() + ' VNĐ');
        if ($('#createTaxAmount').length) $('#createTaxAmount').text(taxAmount.toLocaleString() + ' VNĐ');
        if ($('#createTotalAmount').length) $('#createTotalAmount').text(totalAmount.toLocaleString() + ' VNĐ');
        
        if ($('#editSubTotal').length) $('#editSubTotal').text(subtotal.toLocaleString() + ' VNĐ');
        if ($('#editTaxAmount').length) $('#editTaxAmount').text(taxAmount.toLocaleString() + ' VNĐ');
        if ($('#editTotalAmount').length) $('#editTotalAmount').text(totalAmount.toLocaleString() + ' VNĐ');
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

    // ✅ THÊM: Function để search parts với VAT information
    searchPartsWithVAT: function(query, callback) {
        var self = this;
        $.ajax({
            url: '/PartsManagement/SearchParts',
            type: 'GET',
            data: { searchTerm: query },
            success: function(response) {
                if (response && response.success && response.data) {
                    var parts = response.data.map(function(part) {
                        return {
                            value: part.id,
                            text: `${part.partName} (${part.partNumber}) - ${part.sellPrice.toLocaleString()} VNĐ`,
                            partId: part.id,
                            partName: part.partName,
                            partNumber: part.partNumber,
                            sellPrice: part.sellPrice,
                            costPrice: part.costPrice,
                            vatRate: part.vatRate || 10, // ✅ THÊM: VAT rate từ Part
                            isVATApplicable: part.isVATApplicable !== false, // ✅ THÊM: VAT applicability từ Part
                            hasInvoice: part.hasInvoice !== false // ✅ THÊM: Has invoice từ Part
                        };
                    });
                    callback(parts);
                } else {
                    callback([]);
                }
            },
            error: function() {
                callback([]);
            }
        });
    },

    initializeServiceTypeahead: function(input, prefix) {
        var self = this;
        
        // ✅ THÊM: Kiểm tra xem có phải tab Parts không
        var isPartsTab = input.closest('#createPartsItems, #editPartsItems').length > 0;
        
        if (isPartsTab) {
            // ✅ THÊM: Sử dụng search parts với VAT cho tab Parts
            input.typeahead({
                source: function(query, process) {
                    self.searchPartsWithVAT(query, process);
                },
                displayText: function(item) {
                    return item.text;
                },
                afterSelect: function(item) {
                    var row = input.closest('.service-item-row');
                    var price = item.sellPrice;
                    var quantity = parseFloat(row.find('.quantity-input').val()) || 1;
                    var total = price * quantity;
                    
                    // Set values
                    row.find('.service-id-input').val(item.partId);
                    row.find('.unit-price-input').val(price.toLocaleString() + ' VNĐ');
                    row.find('.total-input').val(total.toLocaleString() + ' VNĐ');
                    
                    // ✅ THÊM: Set VAT từ Part (READ-ONLY)
                    var vatRateInput = row.find('.vat-rate-input');
                    var vatAmountInput = row.find('.vat-amount-input');
                    var invoiceCheckbox = row.find('.invoice-checkbox');
                    
                    // Set VAT rate từ Part và disable input (READ-ONLY)
                    vatRateInput.val(item.vatRate).prop('disabled', true).prop('readonly', true);
                    vatAmountInput.prop('disabled', true);
                    
                    // Set checkbox dựa trên Part information
                    invoiceCheckbox.prop('checked', item.hasInvoice);
                    
                    // ✅ THÊM: Add tooltip để hiển thị READ-ONLY
                    vatRateInput.attr('title', `VAT từ phụ tùng: ${item.vatRate}% (Không được chỉnh sửa)`);
                    vatRateInput.addClass('bg-light text-muted');
                    
                    // Set input value
                    input.val(item.partName);
                    
                    // Recalculate totals
                    self.calculateItemTotal(row);
                },
                delay: 300,
            });
        } else {
            // ✅ SỬA: Gọi ServiceManagement/SearchServices thay vì QuotationManagement/SearchServices
            // ServiceManagement là nơi quản lý services, không phải QuotationManagement
        input.typeahead({
            source: function(query, process) {
                $.ajax({
                    url: '/ServiceManagement/SearchServices',
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
                var price = item.price || 0;
                var quantity = parseFloat(row.find('.quantity-input').val()) || 1;
                var subtotal = price * quantity;
                // Set values
                row.find('.service-id-input').val(item.id);
                row.find('.unit-price-input').val(price.toLocaleString() + ' VNĐ');
                row.find('.subtotal-input').val(subtotal.toLocaleString() + ' VNĐ');
                // Enable VAT inputs for non-parts items
                row.find('.vat-rate-input').prop('disabled', false).prop('readonly', false);
                // Recalc totals considering VAT checkbox state
                self.calculateItemTotal(row);
                // Set input value for display
                input.val(item.name);
            },
            delay: 300,
        });
        }
    },

    // ✅ THÊM: Function tính toán thủ công cho labor item
    calculateLaborItemTotal: function(prefix, serviceType) {
        var containerId = prefix + serviceType.charAt(0).toUpperCase() + serviceType.slice(1) + 'Items';
        var lastRow = $('#' + containerId + ' .service-item-row').last();
        
        if (lastRow.length > 0) {
            var priceText = lastRow.find('.unit-price-input').val() || '';
            var price = parseFloat(priceText.replace(/[^\d]/g, '')) || 0;
            var quantity = parseFloat(lastRow.find('.quantity-input').val()) || 1;
            var subtotal = price * quantity;
            
            // Labor items thường không có VAT (disabled), nhưng vẫn tính để đồng nhất với service items
            var hasInvoice = lastRow.find('.invoice-checkbox').is(':checked');
            var vatRate = parseFloat(lastRow.find('.vat-rate-input').val()) || 0;
            var vatAmount = 0;
            
            if (hasInvoice && vatRate > 0) {
                vatAmount = subtotal * (vatRate / 100);
            }
            
            var total = subtotal + vatAmount;
            
            // Cập nhật tất cả các cột để đồng nhất với service items
            lastRow.find('.subtotal-input').val(subtotal.toLocaleString() + ' VNĐ');
            lastRow.find('.vat-amount-input').val(vatAmount.toLocaleString() + ' VNĐ');
            // Labor items không có cột "Total" riêng như service items, chỉ có subtotal
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
        
        // ✅ THÊM: Labor name select change (show/hide custom input)
        $(document).off('change', '#' + prefix + 'PartsItems .labor-name-select, #' + prefix + 'RepairItems .labor-name-select, #' + prefix + 'PaintItems .labor-name-select').on('change', '#' + prefix + 'PartsItems .labor-name-select, #' + prefix + 'RepairItems .labor-name-select, #' + prefix + 'PaintItems .labor-name-select', function() {
            var row = $(this).closest('.service-item-row');
            var select = $(this);
            var customInput = row.find('.labor-name-custom');
            var selectedValue = select.val();
            
            if (selectedValue === 'Khác') {
                customInput.removeClass('d-none').show().focus();
                customInput.val('');
            } else {
                customInput.addClass('d-none').hide().val('');
            }
        });
        
        // ✅ THÊM: Custom labor name input change (sync with select)
        $(document).off('input', '#' + prefix + 'PartsItems .labor-name-custom, #' + prefix + 'RepairItems .labor-name-custom, #' + prefix + 'PaintItems .labor-name-custom').on('input', '#' + prefix + 'PartsItems .labor-name-custom, #' + prefix + 'RepairItems .labor-name-custom, #' + prefix + 'PaintItems .labor-name-custom', function() {
            var row = $(this).closest('.service-item-row');
            var customInput = $(this);
            var select = row.find('.labor-name-select');
            
            // Sync value from custom input to select when submitting
            if (customInput.val().trim() !== '') {
                select.data('custom-value', customInput.val().trim());
            } else {
                select.removeData('custom-value');
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
                        
                        // ✅ 2.1.1: Check nếu Quotation đã có ServiceOrder -> Lock editing
                        if (quotation.serviceOrderId) {
                            GarageApp.showWarning(
                                'Báo giá đã được chuyển thành phiếu sửa chữa. ' +
                                'Không thể chỉnh sửa báo giá. Vui lòng chỉnh sửa trong phiếu sửa chữa thay vì báo giá.',
                                function() {
                                    // Redirect to ServiceOrder nếu cần
                                    window.location.href = '/OrderManagement';
                                }
                            );
                            return;
                        }
                        
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
        
        // ✅ Helper function để lấy service name từ row (hỗ trợ labor dropdown)
        function getServiceNameFromRow(row) {
            // ✅ SỬA: Kiểm tra labor dropdown trước
            var laborSelect = row.find('.labor-name-select');
            if (laborSelect.length > 0) {
                var selectedValue = laborSelect.val();
                // Nếu không có giá trị selected, lấy option đầu tiên (mặc định)
                if (!selectedValue) {
                    var firstOption = laborSelect.find('option:first').val();
                    selectedValue = firstOption || '';
                }
                if (selectedValue === 'Khác') {
                    var customInput = row.find('.labor-name-custom');
                    var customValue = customInput.val() ? customInput.val().trim() : '';
                    // Nếu "Khác" được chọn nhưng chưa nhập giá trị, vẫn trả về "Khác" để lưu
                    return customValue || selectedValue;
                }
                return selectedValue || '';
            }
            // Nếu không phải labor item, lấy từ typeahead
            return row.find('.service-typeahead').val() || '';
        }
        
        // Collect from Parts tab
        $('#createPartsItems .service-item-row').each(function() {
            var row = $(this);
            var serviceId = row.find('.service-id-input').val();
            var serviceName = getServiceNameFromRow(row);
            var quantity = parseInt(row.find('.quantity-input').val()) || 1;
            var unitPriceText = row.find('.unit-price-input').val() || '';
            var unitPrice = parseFloat(unitPriceText.replace(/[^\d]/g, '')) || 0;
            var hasInvoice = row.find('.invoice-checkbox').is(':checked');
            var itemCategory = row.find('.item-category-input').val() || 'Material';
            // ✅ SỬA: Lấy VAT rate từ input (đã được set từ Part data khi chọn part)
            var vatRate = parseFloat(row.find('.vat-rate-input').val()) || 0;
            // ✅ SỬA: VAT chỉ áp dụng nếu checkbox checked VÀ vatRate > 0
            var isVATApplicable = hasInvoice && vatRate > 0;
            
            if (serviceName && serviceName.trim() !== '') {
                items.push({
                    ServiceId: serviceId ? parseInt(serviceId) : null,
                    ItemName: serviceName,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    IsOptional: false,
                    HasInvoice: hasInvoice,
                    IsVATApplicable: isVATApplicable, // ✅ SỬA: Set từ checkbox và VAT rate
                    VATRate: vatRate, // ✅ SỬA: Lấy từ input
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
            var serviceName = getServiceNameFromRow(row);
            var quantity = parseInt(row.find('.quantity-input').val()) || 1;
            var unitPriceText = row.find('.unit-price-input').val() || '';
            var unitPrice = parseFloat(unitPriceText.replace(/[^\d]/g, '')) || 0;
            var hasInvoice = row.find('.invoice-checkbox').is(':checked');
            var itemCategory = row.find('.item-category-input').val() || 'Material';
            // ✅ SỬA: Lấy VAT rate từ input, không mặc định 10
            var vatRate = parseFloat(row.find('.vat-rate-input').val()) || 0;
            // ✅ SỬA: VAT chỉ áp dụng nếu checkbox checked VÀ vatRate > 0
            var isVATApplicable = hasInvoice && vatRate > 0;
            
            if (serviceName && serviceName.trim() !== '') { // ✅ SỬA: Chỉ cần có tên dịch vụ
                items.push({
                    ServiceId: serviceId ? parseInt(serviceId) : null, // ✅ SỬA: Cho phép null
                    ItemName: serviceName,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    IsOptional: false,
                    HasInvoice: hasInvoice, // ✅ THÊM HasInvoice
                    IsVATApplicable: isVATApplicable, // ✅ SỬA: Set từ checkbox và VAT rate
                    VATRate: vatRate, // ✅ SỬA: Lấy từ input
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
            var serviceName = getServiceNameFromRow(row);
            var quantity = parseInt(row.find('.quantity-input').val()) || 1;
            var unitPriceText = row.find('.unit-price-input').val() || '';
            var unitPrice = parseFloat(unitPriceText.replace(/[^\d]/g, '')) || 0;
            var hasInvoice = row.find('.invoice-checkbox').is(':checked');
            var itemCategory = row.find('.item-category-input').val() || 'Material';
            // ✅ SỬA: Lấy VAT rate từ input, không mặc định 10
            var vatRate = parseFloat(row.find('.vat-rate-input').val()) || 0;
            // ✅ SỬA: VAT chỉ áp dụng nếu checkbox checked VÀ vatRate > 0
            var isVATApplicable = hasInvoice && vatRate > 0;
            
            if (serviceName && serviceName.trim() !== '') { // ✅ SỬA: Chỉ cần có tên dịch vụ
                items.push({
                    ServiceId: serviceId ? parseInt(serviceId) : null, // ✅ SỬA: Cho phép null
                    ItemName: serviceName,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    IsOptional: false,
                    HasInvoice: hasInvoice, // ✅ THÊM HasInvoice
                    IsVATApplicable: isVATApplicable, // ✅ SỬA: Set từ checkbox và VAT rate
                    VATRate: vatRate, // ✅ SỬA: Lấy từ input
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
            TaxRate: 0, // ✅ REMOVED: VAT được tính tự động theo từng item (phụ tùng có VAT từ kho, dịch vụ có thể có VAT)
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
        
        // ✅ Helper function để lấy service name từ row (hỗ trợ labor dropdown)
        function getServiceNameFromRow(row) {
            // ✅ SỬA: Kiểm tra labor dropdown trước
            var laborSelect = row.find('.labor-name-select');
            if (laborSelect.length > 0) {
                var selectedValue = laborSelect.val();
                // Nếu không có giá trị selected, lấy option đầu tiên (mặc định)
                if (!selectedValue) {
                    var firstOption = laborSelect.find('option:first').val();
                    selectedValue = firstOption || '';
                }
                if (selectedValue === 'Khác') {
                    var customInput = row.find('.labor-name-custom');
                    var customValue = customInput.val() ? customInput.val().trim() : '';
                    // Nếu "Khác" được chọn nhưng chưa nhập giá trị, vẫn trả về "Khác" để lưu
                    return customValue || selectedValue;
                }
                return selectedValue || '';
            }
            // Nếu không phải labor item, lấy từ typeahead
            return row.find('.service-typeahead').val() || '';
        }
        
        // Collect from Parts tab
        $('#editPartsItems .service-item-row').each(function() {
            var row = $(this);
            var serviceId = row.find('.service-id-input').val();
            var serviceName = getServiceNameFromRow(row);
            var quantity = parseInt(row.find('.quantity-input').val()) || 1;
            var unitPriceText = row.find('.unit-price-input').val() || '';
            var unitPrice = parseFloat(unitPriceText.replace(/[^\d]/g, '')) || 0;
            var hasInvoice = row.find('.invoice-checkbox').is(':checked');
            var itemCategory = row.find('.item-category-input').val() || 'Material';
            // ✅ SỬA: Lấy VAT rate từ input (đã được set từ Part data khi chọn part), không mặc định 10
            var vatRate = parseFloat(row.find('.vat-rate-input').val()) || 0;
            // ✅ SỬA: VAT chỉ áp dụng nếu checkbox checked VÀ vatRate > 0
            var isVATApplicable = hasInvoice && vatRate > 0;
            
            if (serviceName && serviceName.trim() !== '') {
                items.push({
                    ServiceId: serviceId ? parseInt(serviceId) : null,
                    ItemName: serviceName,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    IsOptional: false,
                    HasInvoice: hasInvoice,
                    IsVATApplicable: isVATApplicable, // ✅ SỬA: Set từ checkbox và VAT rate
                    VATRate: vatRate, // ✅ SỬA: Lấy từ input, không mặc định 10
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
            var serviceName = getServiceNameFromRow(row);
            var quantity = parseInt(row.find('.quantity-input').val()) || 1;
            var unitPriceText = row.find('.unit-price-input').val() || '';
            var unitPrice = parseFloat(unitPriceText.replace(/[^\d]/g, '')) || 0;
            var hasInvoice = row.find('.invoice-checkbox').is(':checked');
            var itemCategory = row.find('.item-category-input').val() || 'Material';
            // ✅ SỬA: Lấy VAT rate từ input, không mặc định 10
            var vatRate = parseFloat(row.find('.vat-rate-input').val()) || 0;
            // ✅ SỬA: VAT chỉ áp dụng nếu checkbox checked VÀ vatRate > 0
            var isVATApplicable = hasInvoice && vatRate > 0;
            
            if (serviceName && serviceName.trim() !== '') {
                items.push({
                    ServiceId: serviceId ? parseInt(serviceId) : null,
                    ItemName: serviceName,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    IsOptional: false,
                    HasInvoice: hasInvoice,
                    IsVATApplicable: isVATApplicable, // ✅ SỬA: Set từ checkbox và VAT rate
                    VATRate: vatRate, // ✅ SỬA: Lấy từ input, không mặc định 10
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
            var serviceName = getServiceNameFromRow(row);
            var quantity = parseInt(row.find('.quantity-input').val()) || 1;
            var unitPriceText = row.find('.unit-price-input').val() || '';
            var unitPrice = parseFloat(unitPriceText.replace(/[^\d]/g, '')) || 0;
            var hasInvoice = row.find('.invoice-checkbox').is(':checked');
            var itemCategory = row.find('.item-category-input').val() || 'Material';
            // ✅ SỬA: Lấy VAT rate từ input, không mặc định 10
            var vatRate = parseFloat(row.find('.vat-rate-input').val()) || 0;
            // ✅ SỬA: VAT chỉ áp dụng nếu checkbox checked VÀ vatRate > 0
            var isVATApplicable = hasInvoice && vatRate > 0;
            
            if (serviceName && serviceName.trim() !== '') {
                items.push({
                    ServiceId: serviceId ? parseInt(serviceId) : null,
                    ItemName: serviceName,
                    Quantity: quantity,
                    UnitPrice: unitPrice,
                    IsOptional: false,
                    HasInvoice: hasInvoice,
                    IsVATApplicable: isVATApplicable, // ✅ SỬA: Set từ checkbox và VAT rate
                    VATRate: vatRate, // ✅ SỬA: Lấy từ input, không mặc định 10
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
            TaxRate: 0, // ✅ REMOVED: VAT được tính tự động theo từng item (phụ tùng có VAT từ kho, dịch vụ có thể có VAT)
            DiscountAmount: parseFloat($('#editDiscountAmount').val()) || 0,
            QuotationType: $('#editQuotationType').val() || 'Personal',
            Status: $('#editStatus').val() || 'Draft',
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
        
        // ✅ SỬA: Sử dụng modal form thay vì Swal đơn giản
        // Clear form
        $('#approveCustomerNotes').val('');
        $('#approveCreateServiceOrder').prop('checked', true);
        $('#approveScheduledDate').val('');
        
        // Show modal
        $('#approveQuotationModal').data('quotation-id', id);
        $('#approveQuotationModal').modal('show');
    },
    
    // ✅ THÊM: Xử lý submit approve form
    submitApproveQuotation: function() {
        var self = this;
        var id = $('#approveQuotationModal').data('quotation-id');
        
        if (!id) {
            GarageApp.showError('Không tìm thấy ID báo giá');
            return;
        }
        
        var approveData = {
            CreateServiceOrder: $('#approveCreateServiceOrder').is(':checked'),
            CustomerNotes: $('#approveCustomerNotes').val() || '',
            ScheduledDate: $('#approveScheduledDate').val() ? new Date($('#approveScheduledDate').val()).toISOString() : null
        };
        
        $.ajax({
            url: '/QuotationManagement/ApproveQuotation/' + id,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(approveData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        $('#approveQuotationModal').modal('hide');
                        GarageApp.showSuccess('Duyệt báo giá thành công! ' + (approveData.CreateServiceOrder ? 'Đã tạo phiếu sửa chữa.' : ''));
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
                    GarageApp.showError('Lỗi khi duyệt báo giá: ' + error);
                }
            }
        });
    },

    rejectQuotation: function(id) {
        var self = this;
        
        // ✅ SỬA: Sử dụng modal form thay vì Swal đơn giản
        // Clear form
        $('#rejectReason').val('');
        $('#rejectChargeInspectionFee').prop('checked', false);
        
        // Show modal
        $('#rejectQuotationModal').data('quotation-id', id);
        $('#rejectQuotationModal').modal('show');
    },
    
    // ✅ THÊM: Xử lý submit reject form
    submitRejectQuotation: function() {
        var self = this;
        var id = $('#rejectQuotationModal').data('quotation-id');
        
        if (!id) {
            GarageApp.showError('Không tìm thấy ID báo giá');
            return;
        }
        
        var rejectionReason = $('#rejectReason').val();
        if (!rejectionReason || rejectionReason.trim() === '') {
            GarageApp.showError('Vui lòng nhập lý do từ chối');
            return;
        }
        
        var chargeInspectionFee = $('#rejectChargeInspectionFee').is(':checked');
        
        $.ajax({
            url: '/QuotationManagement/RejectQuotation/' + id,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ 
                reason: rejectionReason,
                chargeInspectionFee: chargeInspectionFee
            }),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        $('#rejectQuotationModal').modal('hide');
                        var message = 'Từ chối báo giá thành công!';
                        if (chargeInspectionFee) {
                            message += ' Đã tính phí kiểm tra.';
                        }
                        GarageApp.showSuccess(message);
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
                    GarageApp.showError('Lỗi khi từ chối báo giá: ' + error);
                }
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
        var quotationType = quotation.quotationType || quotation.QuotationType || 'Personal';
        $('#viewQuotationType').text(quotationType);
        $('#viewQuotationDate').text(quotation.quotationDate || quotation.QuotationDate ? new Date(quotation.quotationDate || quotation.QuotationDate).toLocaleDateString('vi-VN') : new Date().toLocaleDateString('vi-VN'));
        
        $('#viewValidUntil').text(quotation.validUntil || quotation.ValidUntil ? new Date(quotation.validUntil || quotation.ValidUntil).toLocaleDateString('vi-VN') : '');
        $('#viewDescription').text(quotation.description || quotation.Description || '');
        $('#viewTerms').text(quotation.terms || quotation.Terms || '');
        
        // Calculate and populate financial fields
        var items = quotation.items || quotation.Items || [];
        var subtotal = 0;
        var taxAmount = 0;
        var vatRate = 10; // VAT rate 10%
        
        if (items.length > 0) {
            items.forEach(function(item) {
                var unitPrice = item.unitPrice ?? item.UnitPrice ?? 0;
                var quantity = item.quantity ?? item.Quantity ?? 1;
                var itemSubtotal = unitPrice * quantity;
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
        
        if (items.length > 0) {
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
            items.forEach(function(item, index) {
                var unitPrice = item.unitPrice ?? item.UnitPrice ?? 0;
                var quantity = item.quantity ?? item.Quantity ?? 1;
                var totalPrice = item.totalPrice ?? item.TotalPrice ?? (unitPrice * quantity);
                var serviceItemHtml = `
                    <tr>
                        <td><strong>${item.service ? item.service.name : (item.itemName || item.ItemName || 'Dịch vụ')}</strong></td>
                        <td class="text-center"><span class="badge badge-info">${quantity}</span></td>
                        <td class="text-right"><span class="text-muted">${unitPrice ? unitPrice.toLocaleString() + ' VNĐ' : '0 VNĐ'}</span></td>
                        <td class="text-right"><strong class="text-primary">${totalPrice ? totalPrice.toLocaleString() + ' VNĐ' : '0 VNĐ'}</strong></td>
                        <td class="text-center">
                            ${(item.isVATApplicable || item.IsVATApplicable) ? '<span class="badge badge-success">Có</span>' : '<span class="badge badge-secondary">Không</span>'}
                        </td>
                        <td><small class="text-muted">${item.notes || item.Notes || ''}</small></td>
                    </tr>
                `;
                $('#viewServiceItemsBody').append(serviceItemHtml);
            });
        } else {
            $('#viewServiceItems').append('<div class="text-center text-muted p-3">Không có dịch vụ nào</div>');
        }
        
        // Set data-id for print button
        $('.print-quotation').attr('data-id', quotation.id || quotation.Id);
    },

    populateEditModal: function(quotation) {
        var self = this;
        
        // ✅ DEBUG: Log data nhận được từ API
        console.log('🔍 DEBUG populateEditModal - Quotation data:', quotation);
        if (quotation.items && quotation.items.length > 0) {
            console.log('🔍 DEBUG populateEditModal - Items:', quotation.items);
            quotation.items.forEach(function(item, index) {
                console.log(`🔍 DEBUG populateEditModal - Item ${index}:`, {
                    itemName: item.itemName,
                    vatRate: item.vatRate,
                    VATRate: item.VATRate,
                    hasInvoice: item.hasInvoice,
                    HasInvoice: item.HasInvoice
                });
            });
        }
        
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
    },

    // ✅ THÊM: Approve personal quotation
    approvePersonalQuotation: function(quotationId) {
        var self = this;
        
        Swal.fire({
            title: 'Xác nhận duyệt báo giá',
            text: 'Bạn có chắc chắn muốn duyệt báo giá này không?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Có, duyệt báo giá',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/QuotationManagement/UpdateQuotationStatus/' + quotationId,
                    type: 'POST',
                    data: { status: 'Approved' },
                    success: function(response) {
                        if (response.success) {
                            Swal.fire('Thành công', 'Duyệt báo giá thành công', 'success');
                            self.quotationTable.ajax.reload();
                        } else {
                            Swal.fire('Lỗi', response.error || 'Có lỗi xảy ra khi duyệt báo giá', 'error');
                        }
                    },
                    error: function(xhr, status, error) {
                        Swal.fire('Lỗi', 'Có lỗi xảy ra khi duyệt báo giá', 'error');
                    }
                });
            }
        });
    },

    // ✅ THÊM: Show corporate pricing modal
    showCorporatePricingModal: function(quotationId) {
        var self = this;
        
        // Kiểm tra xem đã có bảng giá công ty chưa
        $.ajax({
            url: '/QuotationManagement/GetCorporateApprovedPricing/' + quotationId,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data && response.data.companyName) {
                    // Đã có bảng giá công ty, hiển thị thông báo
                    Swal.fire({
                        title: 'Bảng giá đã được cập nhật',
                        text: 'Bảng giá duyệt của công ty cho xe này đã được cập nhật. Bạn có muốn xem/chỉnh sửa không?',
                        icon: 'info',
                        showCancelButton: true,
                        confirmButtonColor: '#3085d6',
                        cancelButtonColor: '#6c757d',
                        confirmButtonText: 'Có, xem/chỉnh sửa',
                        cancelButtonText: 'Hủy'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            // Pass existing pricing to avoid duplicate API calls
                            self.openCorporatePricingModal(quotationId, response.data);
                        }
                    });
                } else {
                    // Chưa có bảng giá công ty, mở modal bình thường
                    self.openCorporatePricingModal(quotationId, null);
                }
            },
            error: function() {
                // Có lỗi, mở modal bình thường
                self.openCorporatePricingModal(quotationId, null);
            }
        });
    },

    // ✅ THÊM: Mở modal corporate pricing
    openCorporatePricingModal: function(quotationId, existingPricing) {
        var self = this;
        
        // Load quotation data first
        $.ajax({
            url: '/QuotationManagement/GetQuotation/' + quotationId,
            type: 'GET',
            success: function(response) {
                if (response.success) {
                    var quotation = response.data;
                    
                    // Populate modal with quotation data
                    $('#corporatePricingQuotationId').val(quotationId);
                    $('#corporatePricingQuotationNumber').text(quotation.quotationNumber);
                    $('#corporatePricingCustomerName').text(quotation.customerName);
                    $('#corporatePricingVehicleInfo').text(quotation.vehicleInfo);
                    $('#corporatePricingTotalAmount').text(quotation.totalAmount.toLocaleString() + ' VNĐ');
                    
                    // Load existing corporate pricing if available, otherwise load from quotation
                    self.loadCorporatePricing(quotationId, quotation, existingPricing);
                    
                    // Show modal
                    $('#corporatePricingModal').modal('show');
                } else {
                    Swal.fire('Lỗi', response.error || 'Không thể tải thông tin báo giá', 'error');
                }
            },
            error: function() {
                Swal.fire('Lỗi', 'Không thể tải thông tin báo giá', 'error');
            }
        });
    },

    // ✅ THÊM: Load corporate pricing data
    loadCorporatePricing: function(quotationId, quotationData, existingPricing) {
        var self = this;
        
        // If existingPricing is provided, populate directly without extra API call
        if (existingPricing) {
            var pricing = existingPricing;
            // Populate form fields
            $('#corporateCompanyName').val(pricing.companyName);
            $('#corporateTaxCode').val(pricing.taxCode);
            $('#corporateContractNumber').val(pricing.contractNumber);
            $('#corporateApprovalDate').val(pricing.approvalDate ? new Date(pricing.approvalDate).toISOString().split('T')[0] : '');
            $('#corporateApprovedAmount').val(pricing.approvedAmount);
            $('#corporateCustomerCoPayment').val(pricing.customerCoPayment);
            $('#corporateApprovalNotes').val(pricing.approvalNotes);
            if (pricing.approvedItems && pricing.approvedItems.length > 0) {
                self.populateCorporateItemsTable(pricing.approvedItems);
            } else if (quotationData && quotationData.items && quotationData.items.length > 0) {
                self.populateCorporateItemsFromQuotation(quotationData.items);
            }
            return; // done
        }

        $.ajax({
            url: '/QuotationManagement/GetCorporateApprovedPricing/' + quotationId,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var pricing = response.data;
                    
                    // Populate form fields
                    $('#corporateCompanyName').val(pricing.companyName);
                    $('#corporateTaxCode').val(pricing.taxCode);
                    $('#corporateContractNumber').val(pricing.contractNumber);
                    $('#corporateApprovalDate').val(pricing.approvalDate ? new Date(pricing.approvalDate).toISOString().split('T')[0] : '');
                    $('#corporateApprovedAmount').val(pricing.approvedAmount);
                    $('#corporateCustomerCoPayment').val(pricing.customerCoPayment);
                    $('#corporateApprovalNotes').val(pricing.approvalNotes);
                    
                    // Kiểm tra nếu approvedItems empty thì load từ quotation
                    if (pricing.approvedItems && pricing.approvedItems.length > 0) {
                        self.populateCorporateItemsTable(pricing.approvedItems);
                    } else {
                        // Load items từ quotation nếu approvedItems empty
                        if (quotationData && quotationData.items && quotationData.items.length > 0) {
                            self.populateCorporateItemsFromQuotation(quotationData.items);
                        }
                    }
                } else {
                    // Clear form if no data
                    $('#corporatePricingForm')[0].reset();
                    $('#corporateItemsTable tbody').empty();
                    
                    // Tự động load items từ báo giá nếu chưa có dữ liệu corporate pricing
                    if (quotationData && quotationData.items && quotationData.items.length > 0) {
                        self.populateCorporateItemsFromQuotation(quotationData.items);
                    }
                }
            },
            error: function(xhr, status, error) {
                // Clear form on error
                $('#corporatePricingForm')[0].reset();
                $('#corporateItemsTable tbody').empty();
                
                // Load items từ quotation nếu có lỗi API
                if (quotationData && quotationData.items && quotationData.items.length > 0) {
                    self.populateCorporateItemsFromQuotation(quotationData.items);
                }
            }
        });
    },

    // ✅ THÊM: Populate corporate items table
    populateCorporateItemsTable: function(items) {
        var tbody = $('#corporateItemsTable tbody');
        tbody.empty();
        
        if (items && items.length > 0) {
            items.forEach(function(item) {
                var row = `
                    <tr data-quotation-item-id="${item.quotationItemId || 0}">
                        <td>${item.itemName || 'N/A'}</td>
                        <td class="text-center">${item.quantity || 1}</td>
                        <td class="text-right">${(item.originalPrice || 0).toLocaleString()} VNĐ</td>
                        <td>
                            <input type="number" class="form-control form-control-sm approved-price" 
                                   value="${item.approvedPrice || 0}" min="0" step="0.01">
                        </td>
                        <td>
                            <input type="number" class="form-control form-control-sm co-payment" 
                                   value="${item.customerCoPayment || 0}" min="0" step="0.01">
                        </td>
                        <td class="text-center">
                            <div class="custom-control custom-checkbox">
                                <input type="checkbox" class="custom-control-input is-approved" ${item.isApproved ? 'checked' : ''}>
                                <label class="custom-control-label"></label>
                            </div>
                        </td>
                        <td>
                            <input type="text" class="form-control form-control-sm approval-notes" 
                                   value="${item.approvalNotes || ''}" placeholder="Ghi chú duyệt...">
                        </td>
                    </tr>
                `;
                tbody.append(row);
            });
        }
    },

    // ✅ THÊM: Populate corporate items from quotation data
    populateCorporateItemsFromQuotation: function(quotationItems) {
        var tbody = $('#corporateItemsTable tbody');
        tbody.empty();
        
        if (quotationItems && quotationItems.length > 0) {
            quotationItems.forEach(function(item) {
                var row = `
                    <tr data-quotation-item-id="${item.id || 0}">
                        <td>${item.itemName || item.serviceName || 'N/A'}</td>
                        <td class="text-center">${item.quantity || 1}</td>
                        <td class="text-right">${(item.unitPrice || 0).toLocaleString()} VNĐ</td>
                        <td>
                            <input type="number" class="form-control form-control-sm approved-price" 
                                   value="${item.unitPrice || 0}" min="0" step="0.01">
                        </td>
                        <td>
                            <input type="number" class="form-control form-control-sm co-payment" 
                                   value="0" min="0" step="0.01">
                        </td>
                        <td class="text-center">
                            <div class="custom-control custom-checkbox">
                                <input type="checkbox" class="custom-control-input is-approved" checked>
                                <label class="custom-control-label"></label>
                            </div>
                        </td>
                        <td>
                            <input type="text" class="form-control form-control-sm approval-notes" 
                                   placeholder="Ghi chú duyệt...">
                        </td>
                    </tr>
                `;
                tbody.append(row);
            });
        }
    },

    // ✅ THÊM: Save corporate pricing
    saveCorporatePricing: function() {
        var self = this;
        var quotationId = $('#corporatePricingQuotationId').val();
        
        // Sử dụng FormData để upload file
        var formData = new FormData();
        
        // Add basic form data
        formData.append('quotationId', quotationId);
        formData.append('companyName', $('#corporateCompanyName').val());
        formData.append('taxCode', $('#corporateTaxCode').val());
        formData.append('contractNumber', $('#corporateContractNumber').val());
        formData.append('approvalDate', $('#corporateApprovalDate').val());
        formData.append('approvedAmount', $('#corporateApprovedAmount').val());
        formData.append('customerCoPayment', $('#corporateCustomerCoPayment').val());
        formData.append('approvalNotes', $('#corporateApprovalNotes').val());
        
        // Add file if selected
        var fileInput = $('#corporateContractFile')[0];
        if (fileInput.files.length > 0) {
            formData.append('contractFile', fileInput.files[0]);
        }

// Modal-specific behaviors moved from partial views
(function () {
    if (typeof window !== 'undefined') {
        // Approve Quotation modal behaviors
        window.$(document).on('change', '#approveCreateServiceOrder', function () {
            if (window.$(this).is(':checked')) {
                window.$('#approveScheduledDateGroup').slideDown();
            } else {
                window.$('#approveScheduledDateGroup').slideUp();
            }
        });

        // Set default approve date on load
        var todayStr = new Date().toISOString().split('T')[0];
        window.$('#approveScheduledDate').val(todayStr);

        // Reset approve form on modal hidden
        window.$(document).on('hidden.bs.modal', '#approveQuotationModal', function () {
            var form = document.getElementById('approveQuotationForm');
            if (form) form.reset();
            window.$('#approveCreateServiceOrder').prop('checked', true);
            window.$('#approveScheduledDate').val(todayStr);
            window.$('#approveScheduledDateGroup').show();
        });

        // Create Quotation modal defaults
        window.$(document).on('show.bs.modal', '#createQuotationModal', function () {
            var today = new Date();
            var validUntil = new Date(today);
            validUntil.setDate(today.getDate() + 30);
            var validUntilString = validUntil.getFullYear() + '-' + String(validUntil.getMonth() + 1).padStart(2, '0') + '-' + String(validUntil.getDate()).padStart(2, '0');
            window.$('#createValidUntil').val(validUntilString);
            // Initialize AdminLTE card widgets
            if (window.$.fn.CardWidget) {
                window.$('[data-card-widget="collapse"]').CardWidget();
            }
        });

        // Initialize AdminLTE CardWidget inside insurance and corporate pricing modals when shown
        var initCardWidgets = function() {
            if (window.$.fn.CardWidget) {
                window.$('[data-card-widget="collapse"]').CardWidget();
            }
        };
        window.$(document).on('show.bs.modal', '#insurancePricingModal', initCardWidgets);
        window.$(document).on('show.bs.modal', '#corporatePricingModal', initCardWidgets);

        // Edit modal: format money fields with .total-input
        window.$(document).on('blur', '.total-input', function () {
            var val = parseFloat(window.$(this).val()) || 0;
            if (val > 0) {
                window.$(this).val(val.toLocaleString('vi-VN'));
            }
        });
        window.$(document).on('focus', '.total-input', function () {
            var raw = (window.$(this).val() || '').toString().replace(/[^\d]/g, '');
            window.$(this).val(raw);
        });

        // Reject modal: reset on close
        window.$(document).on('hidden.bs.modal', '#rejectQuotationModal', function () {
            var form = document.getElementById('rejectQuotationForm');
            if (form) form.reset();
            window.$('#rejectChargeInspectionFee').prop('checked', false);
        });
    }
})();
        
        // Collect items data
        var approvedItems = [];
        $('#corporateItemsTable tbody tr').each(function(index) {
            var row = $(this);
            var item = {
                quotationItemId: row.data('quotation-item-id') || index + 1,
                itemName: row.find('td:first').text(),
                quantity: parseInt(row.find('td:nth-child(2)').text()),
                originalPrice: parseFloat(row.find('td:nth-child(3)').text().replace(/[^\d]/g, '')),
                approvedPrice: parseFloat(row.find('.approved-price').val()) || 0,
                customerCoPayment: parseFloat(row.find('.co-payment').val()) || 0,
                isApproved: row.find('.is-approved').is(':checked'),
                approvalNotes: row.find('.approval-notes').val()
            };
            approvedItems.push(item);
        });
        
        formData.append('approvedItems', JSON.stringify(approvedItems));
        
        // Send data to server
        $.ajax({
            url: '/QuotationManagement/UpdateCorporateApprovedPricing/' + quotationId,
            type: 'POST',
            processData: false,
            contentType: false,
            data: formData,
            success: function(response) {
                if (response.success) {
                    Swal.fire('Thành công', 'Cập nhật bảng giá duyệt của công ty thành công', 'success');
                    $('#corporatePricingModal').modal('hide');
                    // Refresh quotation table
                    self.quotationTable.ajax.reload();
                } else {
                    Swal.fire('Lỗi', response.error || 'Có lỗi xảy ra khi cập nhật bảng giá duyệt của công ty', 'error');
                }
            },
            error: function(xhr, status, error) {
                Swal.fire('Lỗi', 'Có lỗi xảy ra khi cập nhật bảng giá duyệt của công ty', 'error');
            }
        });
    },

    // Show insurance pricing modal
    showInsurancePricingModal: function(quotationId) {
        var self = this;
        
        // ✅ THÊM: Kiểm tra xem đã có bảng giá bảo hiểm chưa
        $.ajax({
            url: '/QuotationManagement/GetInsuranceApprovedPricing/' + quotationId,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data && response.data.insuranceCompany) {
                    // Đã có bảng giá bảo hiểm, hiển thị thông báo
                    Swal.fire({
                        title: 'Bảng giá đã được cập nhật',
                        text: 'Bảng giá duyệt của bảo hiểm cho xe này đã được cập nhật. Bạn có muốn xem/chỉnh sửa không?',
                        icon: 'info',
                        showCancelButton: true,
                        confirmButtonColor: '#3085d6',
                        cancelButtonColor: '#6c757d',
                        confirmButtonText: 'Có, xem/chỉnh sửa',
                        cancelButtonText: 'Hủy'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            // Pass existing pricing to avoid duplicate API calls
                            self.openInsurancePricingModal(quotationId, response.data);
                        }
                    });
                } else {
                    // Chưa có bảng giá bảo hiểm, mở modal bình thường
                    self.openInsurancePricingModal(quotationId, null);
                }
            },
            error: function() {
                // Có lỗi, mở modal bình thường
                self.openInsurancePricingModal(quotationId, null);
            }
        });
    },

    // ✅ THÊM: Mở modal insurance pricing
    openInsurancePricingModal: function(quotationId, existingPricing) {
        var self = this;
        
        // Load quotation data first
        $.ajax({
            url: '/QuotationManagement/GetQuotation/' + quotationId,
            type: 'GET',
            success: function(response) {
                if (response.success) {
                    var quotation = response.data;
                    
                    // Populate modal with quotation data
                    $('#insurancePricingQuotationId').val(quotationId);
                    $('#insurancePricingQuotationNumber').text(quotation.quotationNumber);
                    $('#insurancePricingCustomerName').text(quotation.customerName);
                    $('#insurancePricingVehicleInfo').text(quotation.vehicleInfo);
                    $('#insurancePricingTotalAmount').text(quotation.totalAmount.toLocaleString() + ' VNĐ');
                    
                    // Load existing insurance pricing if available, otherwise load from quotation
                    self.loadInsurancePricing(quotationId, quotation, existingPricing);
                    
                    // Show modal
                    $('#insurancePricingModal').modal('show');
                } else {
                    Swal.fire('Lỗi', response.error || 'Không thể tải thông tin báo giá', 'error');
                }
            },
            error: function() {
                Swal.fire('Lỗi', 'Không thể tải thông tin báo giá', 'error');
            }
        });
    },

    // Load insurance pricing data
    loadInsurancePricing: function(quotationId, quotationData, existingPricing) {
        var self = this;
        
        // If existingPricing is provided, populate directly without extra API call
        if (existingPricing) {
            var pricing = existingPricing;
            $('#insuranceCompany').val(pricing.insuranceCompany);
            $('#taxCode').val(pricing.taxCode);
            $('#policyNumber').val(pricing.policyNumber);
            $('#approvalDate').val(pricing.approvalDate ? new Date(pricing.approvalDate).toISOString().split('T')[0] : '');
            $('#approvedAmount').val(pricing.approvedAmount);
            $('#customerCoPayment').val(pricing.customerCoPayment);
            $('#approvalNotes').val(pricing.approvalNotes);
            
            // Show current file if exists
            if (pricing.insuranceFilePath) {
                var fileName = pricing.insuranceFilePath.split('/').pop();
                $('#currentFileName').text(fileName);
                $('#downloadCurrentFile').attr('href', pricing.insuranceFilePath);
                // Ensure both container and alert are visible and use flex for row
                $('#currentFileRow').css('display', 'flex');
                $('#currentFileRow .alert').css('display', 'block');
            } else {
                $('#currentFileRow').hide();
                $('#currentFileRow .alert').hide();
            }
            
            if (pricing.approvedItems && pricing.approvedItems.length > 0) {
                self.populateInsuranceItemsTable(pricing.approvedItems);
            } else if (quotationData && quotationData.items && quotationData.items.length > 0) {
                self.populateInsuranceItemsFromQuotation(quotationData.items);
            }
            return; // done
        }

        $.ajax({
            url: '/QuotationManagement/GetInsuranceApprovedPricing/' + quotationId,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var pricing = response.data;
                    
                    // Populate form fields
                    window.$('#insuranceCompany').val(pricing.insuranceCompany);
                    window.$('#taxCode').val(pricing.taxCode);
                    window.$('#policyNumber').val(pricing.policyNumber);
                    window.$('#approvalDate').val(pricing.approvalDate ? new Date(pricing.approvalDate).toISOString().split('T')[0] : '');
                    window.$('#approvedAmount').val(pricing.approvedAmount);
                    window.$('#customerCoPayment').val(pricing.customerCoPayment);
                    window.$('#approvalNotes').val(pricing.approvalNotes);
                    
                    // Show current file if exists (from API)
                    if (pricing.insuranceFilePath) {
                        var fileName = pricing.insuranceFilePath.split('/').pop();
                        window.$('#currentFileName').text(fileName);
                        window.$('#downloadCurrentFile').attr('href', pricing.insuranceFilePath);
                        window.$('#currentFileRow').css('display', 'flex');
                        window.$('#currentFileRow .alert').css('display', 'block');
                    } else {
                        window.$('#currentFileRow').hide();
                        window.$('#currentFileRow .alert').hide();
                    }
                    
                    // Kiểm tra nếu approvedItems empty thì load từ quotation
                    if (pricing.approvedItems && pricing.approvedItems.length > 0) {
                        self.populateInsuranceItemsTable(pricing.approvedItems);
                    } else {
                        // Load items từ quotation nếu approvedItems empty
                        if (quotationData && quotationData.items && quotationData.items.length > 0) {
                            self.populateInsuranceItemsFromQuotation(quotationData.items);
                        }
                    }
                } else {
                    // Clear form if no data
                    window.$('#insurancePricingForm')[0].reset();
                    window.$('#insuranceItemsTable tbody').empty();
                    
                    // Tự động load items từ báo giá nếu chưa có dữ liệu insurance pricing
                    if (quotationData && quotationData.items && quotationData.items.length > 0) {
                        self.populateInsuranceItemsFromQuotation(quotationData.items);
                    }
                }
            },
            error: function(xhr, status, error) {
                // Clear form on error
                window.$('#insurancePricingForm')[0].reset();
                window.$('#insuranceItemsTable tbody').empty();
                
                // Load items từ quotation nếu có lỗi API
                if (quotationData && quotationData.items && quotationData.items.length > 0) {
                    self.populateInsuranceItemsFromQuotation(quotationData.items);
                }
            }
        });
    },

    // Populate insurance items table
    populateInsuranceItemsTable: function(items) {
        var tbody = window.$('#insuranceItemsTable tbody');
        tbody.empty();
        
        if (items && items.length > 0) {
            items.forEach(function(item) {
                var row = `
                    <tr>
                        <td>${item.itemName}</td>
                        <td class="text-center">${item.quantity}</td>
                        <td class="text-right">${item.originalPrice.toLocaleString()} VNĐ</td>
                        <td>
                            <input type="number" class="form-control form-control-sm approved-price" 
                                   value="${item.approvedPrice}" min="0" step="0.01">
                        </td>
                        <td>
                            <input type="number" class="form-control form-control-sm co-payment" 
                                   value="${item.customerCoPayment}" min="0" step="0.01">
                        </td>
                        <td class="text-center">
                            <div class="custom-control custom-checkbox">
                                <input type="checkbox" class="custom-control-input is-approved" 
                                       ${item.isApproved ? 'checked' : ''}>
                                <label class="custom-control-label"></label>
                            </div>
                        </td>
                        <td>
                            <input type="text" class="form-control form-control-sm approval-notes" 
                                   value="${item.approvalNotes || ''}">
                        </td>
                    </tr>
                `;
                tbody.append(row);
            });
        }
    },

    // ✅ THÊM: Populate insurance items from quotation data
    populateInsuranceItemsFromQuotation: function(quotationItems) {
        var tbody = window.$('#insuranceItemsTable tbody');
        tbody.empty();
        
        if (quotationItems && quotationItems.length > 0) {
            quotationItems.forEach(function(item) {
                var row = `
                    <tr data-quotation-item-id="${item.id || 0}">
                        <td>${item.itemName || item.serviceName || 'N/A'}</td>
                        <td class="text-center">${item.quantity || 1}</td>
                        <td class="text-right">${(item.unitPrice || 0).toLocaleString()} VNĐ</td>
                        <td>
                            <input type="number" class="form-control form-control-sm approved-price" 
                                   value="${item.unitPrice || 0}" min="0" step="0.01">
                        </td>
                        <td>
                            <input type="number" class="form-control form-control-sm co-payment" 
                                   value="0" min="0" step="0.01">
                        </td>
                        <td class="text-center">
                            <div class="custom-control custom-checkbox">
                                <input type="checkbox" class="custom-control-input is-approved" checked>
                                <label class="custom-control-label"></label>
                            </div>
                        </td>
                        <td>
                            <input type="text" class="form-control form-control-sm approval-notes" 
                                   placeholder="Ghi chú duyệt...">
                        </td>
                    </tr>
                `;
                tbody.append(row);
            });
        }
    },

    // Save insurance pricing
    saveInsurancePricing: function() {
        var self = this;
        var quotationId = $('#insurancePricingQuotationId').val();
        
        // Sử dụng FormData để upload file
        var formData = new FormData();
        
        // Add basic form data
        formData.append('quotationId', quotationId);
        formData.append('insuranceCompany', $('#insuranceCompany').val());
        formData.append('taxCode', $('#taxCode').val());
        formData.append('policyNumber', $('#policyNumber').val());
        formData.append('approvalDate', $('#approvalDate').val());
        formData.append('approvedAmount', $('#approvedAmount').val());
        formData.append('customerCoPayment', $('#customerCoPayment').val());
        formData.append('approvalNotes', $('#approvalNotes').val());
        
        // Add file if selected
        var fileInput = $('#insuranceFile')[0];
        if (fileInput.files.length > 0) {
            formData.append('insuranceFile', fileInput.files[0]);
        }
        
        // Collect items data
        var approvedItems = [];
        $('#insuranceItemsTable tbody tr').each(function(index) {
            var row = $(this);
            var item = {
                quotationItemId: row.data('quotation-item-id') || index + 1,
                itemName: row.find('td:first').text(),
                quantity: parseInt(row.find('td:nth-child(2)').text()),
                originalPrice: parseFloat(row.find('td:nth-child(3)').text().replace(/[^\d]/g, '')),
                approvedPrice: parseFloat(row.find('.approved-price').val()) || 0,
                customerCoPayment: parseFloat(row.find('.co-payment').val()) || 0,
                isApproved: row.find('.is-approved').is(':checked'),
                approvalNotes: row.find('.approval-notes').val()
            };
            approvedItems.push(item);
        });
        
        formData.append('approvedItems', JSON.stringify(approvedItems));
        
        // Send data to server
        $.ajax({
            url: '/QuotationManagement/UpdateInsuranceApprovedPricing/' + quotationId,
            type: 'POST',
            processData: false,
            contentType: false,
            data: formData,
            success: function(response) {
                if (response.success) {
                    Swal.fire('Thành công', 'Cập nhật bảng giá duyệt của bảo hiểm thành công', 'success');
                    $('#insurancePricingModal').modal('hide');
                    // Refresh quotation table
                    self.quotationTable.ajax.reload();
                } else {
                    Swal.fire('Lỗi', response.error || 'Cập nhật thất bại', 'error');
                }
            },
            error: function() {
                Swal.fire('Lỗi', 'Cập nhật thất bại', 'error');
            }
        });
    }
};

// ✅ SỬA: Wrap khởi tạo trong document ready
$(document).ready(function() {
    // ✅ THÊM: Khởi tạo module và các event handlers
    QuotationManagement.init();
    
    // ✅ THÊM: Khởi tạo CardWidget cho collapse/expand
    $('[data-card-widget="collapse"]').CardWidget();
    
    // ✅ THÊM: Insurance pricing form submit
    $(document).on('submit', '#insurancePricingForm', function(e) {
        e.preventDefault();
        QuotationManagement.saveInsurancePricing();
    });

    // ✅ THÊM: File upload events
    $(document).on('change', '#insuranceFile', function() {
        var file = this.files[0];
        if (file) {
            $('#fileName').text(file.name);
            $('#filePreviewRow').show();
            
            // Update custom file label
            $(this).next('.custom-file-label').text(file.name);
        }
    });

    $(document).on('click', '#clearFileBtn', function() {
        $('#insuranceFile').val('');
        $('#filePreviewRow').hide();
        $('#insuranceFile').next('.custom-file-label').text('Chọn file...');
        // Đừng ẩn file hiện tại khi chỉ clear file mới chọn
        if ($('#downloadCurrentFile').attr('href')) {
            $('#currentFileRow').css('display', 'flex');
            $('#currentFileRow .alert').css('display', 'block');
        }
    });

    // Remove current file
    $(document).on('click', '#removeCurrentFile', function() {
        Swal.fire({
            title: 'Xác nhận',
            text: 'Bạn có chắc muốn xóa file hiện tại?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                // Chỉ ẩn khi thực sự xóa (ở phiên bản sau sẽ gọi API xóa file)
                $('#currentFileRow').hide();
                $('#currentFileName').text('');
                $('#downloadCurrentFile').attr('href', '');
            }
        });
    });

    $(document).on('click', '#viewFileBtn', function() {
        var file = $('#insuranceFile')[0].files[0];
        if (file) {
            var url = URL.createObjectURL(file);
            window.open(url, '_blank');
        }
    });
}); // ✅ Đóng $(document).ready()
