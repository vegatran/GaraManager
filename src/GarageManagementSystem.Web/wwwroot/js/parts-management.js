// Parts Management Module
window.PartsManagement = {
    partTable: null,
    currentEditData: null, // ✅ THÊM: Store data for edit modal

    init: function() {
        this.initDataTable();
        this.bindEvents();
    },

    initDataTable: function() {
        var self = this;
        
        // Sử dụng DataTablesUtility với style chung
        var columns = [
            { data: 'id', title: 'ID', width: '5%' },
            { data: 'partNumber', title: 'Mã Phụ Tùng', width: '10%' },
            { data: 'partName', title: 'Tên Phụ Tùng', width: '15%' },
            { data: 'category', title: 'Danh Mục', width: '10%' },
            { data: 'brand', title: 'Thương Hiệu', width: '10%' },
            { 
                data: 'sellPrice', 
                title: 'Giá Bán', 
                width: '10%',
                render: DataTablesUtility.renderCurrency
            },
            { data: 'quantityInStock', title: 'Tồn Kho', width: '8%' },
            { data: 'minimumStock', title: 'Tồn TT', width: '8%' },
            { data: 'unit', title: 'Đơn Vị', width: '7%' },
            { 
                data: 'isActive', 
                title: 'Trạng Thái', 
                width: '8%',
                render: function(data, type, row) {
                    return data ? '<span class="badge badge-success">Hoạt động</span>' : '<span class="badge badge-danger">Tạm dừng</span>';
                }
            },
            {
                data: null,
                title: 'Thao Tác',
                width: '9%',
                orderable: false,
                render: function(data, type, row) {
                    return `
                        <button class="btn btn-info btn-sm view-part" data-id="${row.id}" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-warning btn-sm edit-part" data-id="${row.id}" title="Sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn btn-danger btn-sm delete-part" data-id="${row.id}" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    `;
                }
            }
        ];
        
        this.partTable = DataTablesUtility.initServerSideTable('#partTable', '/PartsManagement/GetParts', columns, {
            order: [[0, 'desc']],
            pageLength: 10
        });
    },

    bindEvents: function() {
        var self = this;


        // Create part
        $('#createPartForm').on('submit', function(e) {
            e.preventDefault();
            self.createPart();
        });

        // Edit part
        $('#editPartForm').on('submit', function(e) {
            e.preventDefault();
            self.updatePart();
        });

        // View part
        $(document).on('click', '.view-part', function() {
            var partId = $(this).data('id');
            self.viewPart(partId);
        });

        // Edit part button
        $(document).on('click', '.edit-part', function() {
            var partId = $(this).data('id');
            self.loadPartForEdit(partId);
        });

        // ✅ THÊM: Populate edit modal when shown
        $('#editPartModal').on('shown.bs.modal', function() {
            if (self.currentEditData) {
                // Đảm bảo InputMask được khởi tạo trước khi populate
                self.initPriceInputMask();
                self.populateEditModal(self.currentEditData);
                self.currentEditData = null; // Clear after use
            }
        });

        // Delete part
        $(document).on('click', '.delete-part', function() {
            var partId = $(this).data('id');
            self.deletePart(partId);
        });

        // ✅ THÊM: Preset handlers
        $('#createApplyPreset').on('click', function() {
            var presetKey = $('#createPresetSelect').val();
            if (presetKey && window.PartClassificationPresets) {
                PartClassificationPresets.applyPreset(presetKey, 'create');
            }
        });

        $('#editApplyPreset').on('click', function() {
            var presetKey = $('#editPresetSelect').val();
            if (presetKey && window.PartClassificationPresets) {
                PartClassificationPresets.applyPreset(presetKey, 'edit');
            }
        });

        // ✅ THÊM: Auto-update summary khi thay đổi classification fields
        $('#createHasInvoice, #createCanUseForCompany, #createCanUseForInsurance, #createCanUseForIndividual, #createIsOEM').on('change', function() {
            if (window.PartClassificationPresets) {
                PartClassificationPresets.updateSummary('create');
            }
        });

        $('#createWarrantyMonths, #createSourceType').on('change', function() {
            if (window.PartClassificationPresets) {
                PartClassificationPresets.updateSummary('create');
            }
        });

        // Same for edit modal
        $('#editHasInvoice, #editCanUseForCompany, #editCanUseForInsurance, #editCanUseForIndividual, #editIsOEM').on('change', function() {
            if (window.PartClassificationPresets) {
                PartClassificationPresets.updateSummary('edit');
            }
        });

        $('#editWarrantyMonths, #editSourceType').on('change', function() {
            if (window.PartClassificationPresets) {
                PartClassificationPresets.updateSummary('edit');
            }
        });

        // ✅ THÊM: Format tiền tệ khi nhập vào các trường giá bằng InputMask
        // Áp dụng inputmask cho các trường giá
        this.initPriceInputMask();

    },

    // ✅ THÊM: Initialize InputMask cho các trường giá
    initPriceInputMask: function() {
        // Cấu hình inputmask cho tiền tệ Việt Nam
        var maskOptions = {
            alias: 'numeric',
            groupSeparator: ',',
            digits: 0,
            digitsOptional: false,
            placeholder: '0',
            autoGroup: true,
            rightAlign: false,
            autoUnmask: true, // Tự động remove mask khi lấy giá trị
            removeMaskOnSubmit: true
        };

        // Áp dụng cho create modal (chỉ nếu chưa có)
        if (!$('#createCostPrice').data('inputmask')) {
            $('#createCostPrice, #createSellPrice').inputmask(maskOptions);
        }
        
        // Áp dụng cho edit modal (chỉ nếu chưa có)
        if (!$('#editCostPrice').data('inputmask')) {
            $('#editCostPrice, #editSellPrice').inputmask(maskOptions);
        }
    },

    createPart: function() {
        var self = this;
        
        // ✅ Validate classification trước khi submit
        if (window.PartClassificationPresets) {
            const validation = PartClassificationPresets.validateClassification('create');
            if (validation.errors.length > 0) {
                Swal.fire('Lỗi', validation.errors.join('<br>'), 'error');
                return;
            }
            if (validation.warnings.length > 0) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Cảnh báo',
                    html: validation.warnings.join('<br>'),
                    showCancelButton: true,
                    confirmButtonText: 'Tiếp tục',
                    cancelButtonText: 'Hủy'
                }).then((result) => {
                    if (result.isConfirmed) {
                        self.submitCreatePart();
                    }
                });
                return;
            }
        }
        
        self.submitCreatePart();
    },

    submitCreatePart: function() {
        var formData = {
            PartNumber: $('#createPartNumber').val(),
            PartName: $('#createPartName').val(),
            Description: $('#createDescription').val(),
            Category: $('#createCategory').val(),
            Brand: $('#createBrand').val(),
            CostPrice: parseFloat($('#createCostPrice').val()) || 0,
            SellPrice: parseFloat($('#createSellPrice').val()) || 0,
            QuantityInStock: parseInt($('#createQuantityInStock').val()) || 0,
            MinimumStock: parseInt($('#createMinimumStock').val()) || 0,
            ReorderLevel: parseInt($('#createReorderLevel').val()) || null,
            Unit: $('#createUnit').val(),
            Location: $('#createLocation').val(),
            CompatibleVehicles: $('#createCompatibleVehicles').val(),
            IsActive: $('#createIsActive').is(':checked'),
            
            // ✅ THÊM: Classification fields
            SourceType: $('#createSourceType').val() || 'Purchased',
            InvoiceType: $('#createInvoiceType').val() || 'WithInvoice',
            HasInvoice: $('#createHasInvoice').is(':checked'),
            Condition: $('#createCondition').val() || 'New',
            SourceReference: $('#createSourceReference').val(),
            CanUseForCompany: $('#createCanUseForCompany').is(':checked'),
            CanUseForInsurance: $('#createCanUseForInsurance').is(':checked'),
            CanUseForIndividual: $('#createCanUseForIndividual').is(':checked'),
            WarrantyMonths: $('#createWarrantyMonths').val() ? parseInt($('#createWarrantyMonths').val()) : 0,
            IsOEM: $('#createIsOEM').is(':checked'),
            
            // ✅ THÊM: Thông tin thuế VAT
            VATRate: parseFloat($('#createVATRate').val()) || 10,
            IsVATApplicable: $('#createIsVATApplicable').is(':checked'),
            
            // ✅ THÊM: Technical fields
            OEMNumber: $('#createOEMNumber').val(),
            AftermarketNumber: $('#createAftermarketNumber').val(),
            Manufacturer: $('#createManufacturer').val(),
            Dimensions: $('#createDimensions').val(),
            Weight: parseFloat($('#createWeight').val()) || null,
            Material: $('#createMaterial').val(),
            Color: $('#createColor').val()
        };

        // Validate required fields
        if (!formData.PartNumber || !formData.PartName || formData.SellPrice <= 0) {
            GarageApp.showError('Vui lòng điền đầy đủ thông tin bắt buộc');
            return;
        }

        $.ajax({
            url: '/PartsManagement/CreatePart',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess('Thêm phụ tùng thành công');
                    $('#createPartModal').modal('hide');
                    $('#createPartForm')[0].reset();
                    window.PartsManagement.partTable.ajax.reload();
                } else {
                     GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Không thể thêm phụ tùng');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else if (xhr.status === 400) {
                    // Handle validation errors
                    try {
                        var errorResponse = JSON.parse(xhr.responseText);
                        if (errorResponse.errors) {
                            self.displayValidationErrors(errorResponse.errors);
                        } else {
                            GarageApp.showError(errorResponse.message || 'Dữ liệu không hợp lệ');
                        }
                    } catch (e) {
                        GarageApp.showError('Dữ liệu không hợp lệ');
                    }
                } else {
                    GarageApp.showError('Lỗi khi thêm phụ tùng');
                }
            }
        });
    },

    // ✅ THÊM: Function hiển thị validation errors
    displayValidationErrors: function(errors) {
        // Clear previous errors
        $('.is-invalid').removeClass('is-invalid');
        $('.invalid-feedback').remove();
        
        // Display field-specific errors
        for (var field in errors) {
            var fieldElement = $(`#create${field}`);
            if (fieldElement.length > 0) {
                fieldElement.addClass('is-invalid');
                fieldElement.after(`<div class="invalid-feedback">${errors[field].join(', ')}</div>`);
            }
        }
        
        // Show general error message
        GarageApp.showError('Vui lòng kiểm tra lại thông tin đã nhập');
    },

    loadPartForEdit: function(partId) {
        var self = this;
        $.ajax({
            url: '/PartsManagement/GetPart/' + partId,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    // Store data and show modal
                    self.currentEditData = response.data;
                    $('#editPartModal').modal('show');
                } else {
                    GarageApp.showError('Không thể tải thông tin phụ tùng');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin phụ tùng');
                }
            }
        });
    },

    populateEditModal: function(part) {
        // Basic fields
        $('#editId').val(part.id);
        $('#editPartNumber').val(part.partNumber);
        $('#editPartName').val(part.partName);
        $('#editDescription').val(part.description);
        $('#editCategory').val(part.category);
        $('#editBrand').val(part.brand);
        
        // Set giá trị cho các trường giá (dùng unmasked value)
        if (part.costPrice) {
            $('#editCostPrice').val(part.costPrice);
        }
        if (part.sellPrice) {
            $('#editSellPrice').val(part.sellPrice);
        }
        $('#editQuantityInStock').val(part.quantityInStock);
        $('#editMinimumStock').val(part.minimumStock);
        $('#editReorderLevel').val(part.reorderLevel);
        $('#editUnit').val(part.unit);
        $('#editLocation').val(part.location);
        $('#editCompatibleVehicles').val(part.compatibleVehicles);
        $('#editIsActive').prop('checked', part.isActive);
        
        // ✅ THÊM: Classification fields
        $('#editSourceType').val(part.sourceType || 'Purchased');
        $('#editInvoiceType').val(part.invoiceType || 'WithInvoice');
        $('#editHasInvoice').prop('checked', part.hasInvoice !== false);
        $('#editCondition').val(part.condition || 'New');
        $('#editSourceReference').val(part.sourceReference || '');
        $('#editCanUseForCompany').prop('checked', part.canUseForCompany || false);
        $('#editCanUseForInsurance').prop('checked', part.canUseForInsurance || false);
        $('#editCanUseForIndividual').prop('checked', part.canUseForIndividual !== false);
        $('#editWarrantyMonths').val(part.warrantyMonths !== undefined ? part.warrantyMonths : 0);
        $('#editIsOEM').prop('checked', part.isOEM || false);
        
        // ✅ THÊM: Technical fields
        $('#editOEMNumber').val(part.oemNumber || '');
        $('#editAftermarketNumber').val(part.aftermarketNumber || '');
        $('#editManufacturer').val(part.manufacturer || '');
        $('#editDimensions').val(part.dimensions || '');
        $('#editWeight').val(part.weight || '');
        $('#editMaterial').val(part.material || '');
        $('#editColor').val(part.color || '');
        
        // ✅ Update summary
        if (window.PartClassificationPresets) {
            PartClassificationPresets.updateSummary('edit');
        }
    },

    updatePart: function() {
        var self = this;
        var partId = $('#editId').val();
        
        // ✅ Validate classification trước khi submit
        if (window.PartClassificationPresets) {
            const validation = PartClassificationPresets.validateClassification('edit');
            if (validation.errors.length > 0) {
                Swal.fire('Lỗi', validation.errors.join('<br>'), 'error');
                return;
            }
            if (validation.warnings.length > 0) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Cảnh báo',
                    html: validation.warnings.join('<br>'),
                    showCancelButton: true,
                    confirmButtonText: 'Tiếp tục',
                    cancelButtonText: 'Hủy'
                }).then((result) => {
                    if (result.isConfirmed) {
                        self.submitUpdatePart(partId);
                    }
                });
                return;
            }
        }
        
        self.submitUpdatePart(partId);
    },

    submitUpdatePart: function(partId) {
        // ✅ DEBUG: Log input values
        console.log('DEBUG: editCostPrice value:', $('#editCostPrice').val());
        console.log('DEBUG: editSellPrice value:', $('#editSellPrice').val());
        
        var formData = {
            Id: parseInt(partId),
            PartNumber: $('#editPartNumber').val(),
            PartName: $('#editPartName').val(),
            Description: $('#editDescription').val(),
            Category: $('#editCategory').val(),
            Brand: $('#editBrand').val(),
            CostPrice: parseFloat($('#editCostPrice').val()) || 0,
            SellPrice: parseFloat($('#editSellPrice').val()) || 0,
            QuantityInStock: parseInt($('#editQuantityInStock').val()) || 0,
            MinimumStock: parseInt($('#editMinimumStock').val()) || 0,
            ReorderLevel: parseInt($('#editReorderLevel').val()) || null,
            Unit: $('#editUnit').val(),
            Location: $('#editLocation').val(),
            CompatibleVehicles: $('#editCompatibleVehicles').val(),
            IsActive: $('#editIsActive').is(':checked'),
            
            // ✅ THÊM: Classification fields
            SourceType: $('#editSourceType').val() || 'Purchased',
            InvoiceType: $('#editInvoiceType').val() || 'WithInvoice',
            HasInvoice: $('#editHasInvoice').is(':checked'),
            Condition: $('#editCondition').val() || 'New',
            SourceReference: $('#editSourceReference').val(),
            CanUseForCompany: $('#editCanUseForCompany').is(':checked'),
            CanUseForInsurance: $('#editCanUseForInsurance').is(':checked'),
            CanUseForIndividual: $('#editCanUseForIndividual').is(':checked'),
            WarrantyMonths: $('#editWarrantyMonths').val() ? parseInt($('#editWarrantyMonths').val()) : 0,
            IsOEM: $('#editIsOEM').is(':checked'),
            
            // ✅ THÊM: Thông tin thuế VAT
            VATRate: parseFloat($('#editVATRate').val()) || 10,
            IsVATApplicable: $('#editIsVATApplicable').is(':checked'),
            
            // ✅ THÊM: Technical fields
            OEMNumber: $('#editOEMNumber').val(),
            AftermarketNumber: $('#editAftermarketNumber').val(),
            Manufacturer: $('#editManufacturer').val(),
            Dimensions: $('#editDimensions').val(),
            Weight: parseFloat($('#editWeight').val()) || null,
            Material: $('#editMaterial').val(),
            Color: $('#editColor').val()
        };

        // ✅ DEBUG: Log formData
        console.log('DEBUG: formData being sent:', formData);
        console.log('DEBUG: CostPrice in formData:', formData.CostPrice);

        $.ajax({
            url: '/PartsManagement/UpdatePart/' + partId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess('Cập nhật phụ tùng thành công');
                    $('#editPartModal').modal('hide');
                    window.PartsManagement.partTable.ajax.reload();
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Không thể cập nhật phụ tùng');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi cập nhật phụ tùng');
                }
            }
        });
    },

    viewPart: function(partId) {
        $.ajax({
            url: '/PartsManagement/GetPart/' + partId,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    window.PartsManagement.populateViewModal(response.data);
                    $('#viewPartModal').modal('show');
                } else {
                    GarageApp.showError('Không thể tải thông tin phụ tùng');
                }
            },
            error: function(xhr) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin phụ tùng');
                }
            }
        });
    },

    populateViewModal: function(part) {
        // Basic fields
        $('#viewPartNumber').text(part.partNumber);
        $('#viewPartName').text(part.partName);
        $('#viewDescription').text(part.description || 'N/A');
        $('#viewCategory').text(part.category || 'N/A');
        $('#viewBrand').text(part.brand || 'N/A');
        $('#viewCostPrice').text(part.costPrice ? part.costPrice.toLocaleString() + ' VNĐ' : 'N/A');
        $('#viewSellPrice').text(part.sellPrice ? part.sellPrice.toLocaleString() + ' VNĐ' : 'N/A');
        $('#viewQuantityInStock').text(part.quantityInStock);
        $('#viewMinimumStock').text(part.minimumStock);
        $('#viewUnit').text(part.unit || 'N/A');
        $('#viewLocation').text(part.location || 'N/A');
        
        // ✅ THÊM: Classification fields
        const sourceTypeMap = {
            'Purchased': 'Mua mới',
            'Used': 'Tháo từ xe cũ',
            'Refurbished': 'Tái chế',
            'Salvage': 'Phế liệu'
        };
        $('#viewSourceType').text(sourceTypeMap[part.sourceType] || part.sourceType || 'N/A');
        
        const conditionMap = {
            'New': 'Mới 100%',
            'Used': 'Đã qua sử dụng',
            'Refurbished': 'Tái chế',
            'AsIs': 'Nguyên trạng'
        };
        $('#viewCondition').text(conditionMap[part.condition] || part.condition || 'N/A');
        
        $('#viewSourceReference').text(part.sourceReference || 'N/A');
        
        const invoiceTypeMap = {
            'WithInvoice': 'Có hóa đơn VAT',
            'WithoutInvoice': 'Không hóa đơn',
            'Internal': 'Nội bộ'
        };
        $('#viewInvoiceType').text(invoiceTypeMap[part.invoiceType] || part.invoiceType || 'N/A');
        
        const hasInvoiceHtml = part.hasInvoice ? 
            '<span class="badge badge-success"><i class="fas fa-check"></i> Có hóa đơn VAT</span>' :
            '<span class="badge badge-warning"><i class="fas fa-times"></i> Không có hóa đơn</span>';
        $('#viewHasInvoice').html(hasInvoiceHtml);
        
        // Đối tượng sử dụng
        let usageTypesHtml = '';
        if (part.canUseForIndividual) {
            usageTypesHtml += '<span class="badge badge-info mr-2"><i class="fas fa-user"></i> Cá nhân</span>';
        }
        if (part.canUseForCompany) {
            usageTypesHtml += '<span class="badge badge-primary mr-2"><i class="fas fa-building"></i> Công ty</span>';
        }
        if (part.canUseForInsurance) {
            usageTypesHtml += '<span class="badge badge-primary mr-2"><i class="fas fa-shield-alt"></i> Bảo hiểm</span>';
        }
        $('#viewUsageTypes').html(usageTypesHtml || '<span class="text-muted">Chưa cấu hình</span>');
        
        // Bảo hành
        const warrantyText = (part.warrantyMonths && part.warrantyMonths > 0) ? 
            part.warrantyMonths + ' tháng' : 'Không bảo hành';
        $('#viewWarrantyMonths').text(warrantyText);
        
        const oemHtml = part.isOEM ? 
            '<span class="badge badge-success"><i class="fas fa-star"></i> Chính hãng OEM</span>' :
            '<span class="badge badge-secondary">Aftermarket</span>';
        $('#viewIsOEM').html(oemHtml);
        
        // ✅ THÊM: Technical fields
        $('#viewOEMNumber').text(part.oemNumber || 'N/A');
        $('#viewAftermarketNumber').text(part.aftermarketNumber || 'N/A');
        $('#viewManufacturer').text(part.manufacturer || 'N/A');
        $('#viewDimensions').text(part.dimensions || 'N/A');
        $('#viewWeight').text(part.weight ? part.weight + ' kg' : 'N/A');
        $('#viewMaterial').text(part.material || 'N/A');
        $('#viewColor').text(part.color || 'N/A');
        $('#viewCompatibleVehicles').text(part.compatibleVehicles || 'N/A');
        
        // ✅ THÊM: Classification summary badges
        this.updateViewBadges(part);
    },

    // ✅ THÊM: Update classification badges trong view modal
    updateViewBadges: function(part) {
        let badgesHtml = '<div class="d-flex flex-wrap">';
        
        // Invoice badge
        if (part.hasInvoice) {
            badgesHtml += '<span class="badge badge-success mr-2 mb-2">';
            badgesHtml += '<i class="fas fa-file-invoice"></i> Có hóa đơn VAT</span>';
        } else {
            badgesHtml += '<span class="badge badge-warning mr-2 mb-2">';
            badgesHtml += '<i class="fas fa-exclamation-triangle"></i> Không hóa đơn</span>';
        }
        
        // Source type badge
        const sourceTypeMap = {
            'Purchased': { label: 'Mua mới', class: 'badge-info', icon: 'fa-shopping-cart' },
            'Used': { label: 'Tháo xe', class: 'badge-secondary', icon: 'fa-recycle' },
            'Refurbished': { label: 'Tái chế', class: 'badge-warning', icon: 'fa-sync' },
            'Salvage': { label: 'Phế liệu', class: 'badge-dark', icon: 'fa-trash' }
        };
        const sourceInfo = sourceTypeMap[part.sourceType] || { label: part.sourceType, class: 'badge-secondary', icon: 'fa-box' };
        badgesHtml += `<span class="badge ${sourceInfo.class} mr-2 mb-2">`;
        badgesHtml += `<i class="fas ${sourceInfo.icon}"></i> ${sourceInfo.label}</span>`;
        
        // Condition badge
        const conditionMap = {
            'New': { label: 'Mới 100%', class: 'badge-success', icon: 'fa-star' },
            'Used': { label: 'Đã dùng', class: 'badge-warning', icon: 'fa-box-open' },
            'Refurbished': { label: 'Tái chế', class: 'badge-info', icon: 'fa-sync-alt' },
            'AsIs': { label: 'Nguyên trạng', class: 'badge-secondary', icon: 'fa-box' }
        };
        const conditionInfo = conditionMap[part.condition] || { label: part.condition, class: 'badge-secondary', icon: 'fa-box' };
        badgesHtml += `<span class="badge ${conditionInfo.class} mr-2 mb-2">`;
        badgesHtml += `<i class="fas ${conditionInfo.icon}"></i> ${conditionInfo.label}</span>`;
        
        // OEM badge
        if (part.isOEM) {
            badgesHtml += '<span class="badge badge-warning mr-2 mb-2">';
            badgesHtml += '<i class="fas fa-certificate"></i> Chính hãng OEM</span>';
        }
        
        // Warranty badge
        if (part.warrantyMonths && part.warrantyMonths > 0) {
            badgesHtml += `<span class="badge badge-primary mr-2 mb-2">`;
            badgesHtml += `<i class="fas fa-shield-alt"></i> BH: ${part.warrantyMonths} tháng</span>`;
        }
        
        // Usage types
        if (part.canUseForCompany) {
            badgesHtml += '<span class="badge badge-primary mr-2 mb-2">';
            badgesHtml += '<i class="fas fa-building"></i> Công ty</span>';
        }
        if (part.canUseForInsurance) {
            badgesHtml += '<span class="badge badge-primary mr-2 mb-2">';
            badgesHtml += '<i class="fas fa-shield-alt"></i> Bảo hiểm</span>';
        }
        
        badgesHtml += '</div>';
        $('#viewClassificationBadges').html(badgesHtml);
    },

    deletePart: function(partId) {
        Swal.fire({
            title: 'Xác nhận xóa?',
            text: 'Bạn có chắc chắn muốn xóa phụ tùng này?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/PartsManagement/DeletePart/' + partId,
                    type: 'DELETE',
                    success: function(response) {
                        if (response.success) {
                            GarageApp.showSuccess('Xóa phụ tùng thành công');
                            window.PartsManagement.partTable.ajax.reload();
                        } else {
                            GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Không thể xóa phụ tùng');
                        }
                    },
                    error: function(xhr) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi xóa phụ tùng');
                        }
                    }
                });
            }
        });
    }
};

// Initialize when document is ready
$(document).ready(function() {
    if ($('#partTable').length) {
        PartsManagement.init();
    }
});

