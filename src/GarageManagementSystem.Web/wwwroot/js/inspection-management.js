/**
 * Inspection Management Module
 * 
 * Handles all vehicle inspection-related operations
 * CRUD operations for vehicle inspections
 */

window.InspectionManagement = {
    // DataTable instance
    inspectionTable: null,

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
            { data: 'inspectionNumber', title: 'Số Kiểm Tra', width: '10%' },
            { data: 'vehiclePlate', title: 'Biển Số Xe', width: '12%' },
            { data: 'customerName', title: 'Khách Hàng', width: '12%' },
            { data: 'inspectorName', title: 'Kỹ Thuật Viên', width: '12%' },
            { 
                data: 'inspectionDate', 
                title: 'Ngày Kiểm Tra', 
                width: '12%',
            },
            { 
                data: 'status', 
                title: 'Trạng Thái', 
                width: '8%',
                render: function(data, type, row) {
                    var badgeClass = 'badge-secondary';
                    switch(data) {
                        case 'Pending': badgeClass = 'badge-warning'; break;
                        case 'InProgress': badgeClass = 'badge-info'; break;
                        case 'Completed': badgeClass = 'badge-success'; break;
                        case 'Cancelled': badgeClass = 'badge-danger'; break;
                    }
                    return `<span class="badge ${badgeClass}">${data}</span>`;
                }
            },
            { data: 'currentMileage', title: 'Số Km', width: '8%' },
            { 
                data: 'generalCondition', 
                title: 'Tình Trạng', 
                width: '10%',
                render: function(data, type, row) {
                    return InspectionManagement.translateGeneralCondition(data);
                }
            },
            {
                data: null,
                title: 'Thao Tác',
                width: '10%',
                orderable: false,
                render: function(data, type, row) {
                    var actions = `
                        <div class="btn-group" role="group">
                            <button type="button" class="btn btn-info btn-sm view-inspection" data-id="${row.id}" title="Xem">
                                <i class="fas fa-eye"></i>
                            </button>
                    `;
                    
                    // Kiểm tra trạng thái để hiển thị nút phù hợp
                    var isCompleted = row.status === 'Completed' || 
                                     row.status === 'Hoàn Thành' ||  // Chữ T viết hoa (từ TranslateStatus)
                                     row.status === 'Hoàn thành' || 
                                     row.status === 'completed' || 
                                     row.status === 'hoàn thành' ||
                                     (row.status && row.status.toLowerCase().includes('hoàn thành')) ||
                                     (row.status && row.status.toLowerCase().includes('completed'));
                    
                    var isInProgress = row.status === 'InProgress' || 
                                      row.status === 'Đang Kiểm Tra' ||
                                      row.status === 'Đang xử lý' ||
                                      (row.status && row.status.toLowerCase().includes('đang'));
                    
                    if (!isCompleted) {
                        // Nút Sửa và Xóa cho tất cả trạng thái chưa hoàn thành
                        actions += `
                            <button type="button" class="btn btn-warning btn-sm edit-inspection" data-id="${row.id}" title="Sửa">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button type="button" class="btn btn-danger btn-sm delete-inspection" data-id="${row.id}" title="Xóa">
                                <i class="fas fa-trash"></i>
                            </button>
                        `;
                        
                        // Nút Hoàn Thành chỉ hiển thị khi đang kiểm tra
                        if (isInProgress) {
                            actions += `
                                <button type="button" class="btn btn-success btn-sm complete-inspection" data-id="${row.id}" title="Hoàn Thành Kiểm Tra">
                                    <i class="fas fa-check"></i>
                                </button>
                            `;
                        }
                    }
                    
                    actions += `
                        </div>
                    `;
                    
                    return actions;
                }
            }
        ];
        
        this.inspectionTable = DataTablesUtility.initServerSideTable('#inspectionTable', '/VehicleManagement/GetInspections', columns, {
            order: [[0, 'desc']],
            pageLength: 10,
        });
    },

    bindEvents: function() {
        var self = this;


        // Add inspection button
        $(document).on('click', '[data-target="#createInspectionModal"]', function() {
            self.showCreateModal();
        });

        // View inspection
        $(document).on('click', '.view-inspection', function() {
            var id = $(this).data('id');
            self.viewInspection(id);
        });

        // Edit inspection
        $(document).on('click', '.edit-inspection', function() {
            var id = $(this).data('id');
            self.editInspection(id);
        });

        // Delete inspection
        $(document).on('click', '.delete-inspection', function() {
            var id = $(this).data('id');
            self.deleteInspection(id);
        });

        // Complete inspection
        $(document).on('click', '.complete-inspection', function() {
            var id = $(this).data('id');
            self.completeInspection(id);
        });

        // Create inspection form
        $(document).on('submit', '#createInspectionForm', function(e) {
            e.preventDefault();
            self.createInspection();
        });

        // Update inspection form
        $(document).on('submit', '#editInspectionForm', function(e) {
            e.preventDefault();
            self.updateInspection();
        });

        // Bind CustomerReception change event
        $(document).on('change', '#createCustomerReceptionId', function() {
            self.onReceptionChange();
        });
    },

    loadDropdowns: function() {
        this.loadReceptions();
        this.loadVehicles();
        this.loadCustomers();
        this.loadEmployees();
    },

    loadReceptions: function() {
        var self = this;
        $.ajax({
            url: '/VehicleManagement/GetAvailableReceptions',
            type: 'GET',
            success: function(data) {
                var $select = $('#createCustomerReceptionId');
                
                $select.empty().append('<option value="">Chọn phiếu tiếp đón</option>');
                
                if (data && data.length > 0) {
                    $.each(data, function(index, item) {
                        $select.append(`<option value="${item.value}" 
                            data-customer-id="${item.customerId}" 
                            data-vehicle-id="${item.vehicleId}" 
                            data-customer-name="${item.customerName}" 
                            data-vehicle-info="${item.vehicleInfo}" 
                            data-assigned-technician-id="${item.assignedTechnicianId}">${item.text}</option>`);
                    });
                } else {
                }
                
                $select.select2({
                    placeholder: 'Chọn phiếu tiếp đón',
                    allowClear: true,
                });
            },
            error: function(xhr, status, error) {
                GarageApp.showError('Lỗi khi tải danh sách phiếu tiếp đón');
            }
        });
    },

    onReceptionChange: function() {
        var selectedOption = $('#createCustomerReceptionId option:selected');
        
        if (selectedOption.val()) {
            // Tự động điền thông tin từ CustomerReception
            var customerId = selectedOption.data('customer-id');
            var vehicleId = selectedOption.data('vehicle-id');
            var customerName = selectedOption.data('customer-name');
            var vehicleInfo = selectedOption.data('vehicle-info');
            var assignedTechnicianId = selectedOption.data('assigned-technician-id');
            
            // Điền thông tin xe
            $('#createVehicleId').val(vehicleId).trigger('change');
            
            // Điền thông tin khách hàng
            $('#createCustomerId').val(customerId).trigger('change');
            
            // Điền thông tin kỹ thuật viên
            $('#createInspectorId').val(assignedTechnicianId).trigger('change');
            
            // Hiển thị thông tin đã chọn
            console.log('Selected reception data:', {
                customerId: customerId,
                vehicleId: vehicleId,
                customerName: customerName,
                vehicleInfo: vehicleInfo,
                assignedTechnicianId: assignedTechnicianId,
            });
        } else {
            // Reset các field khi không chọn reception
            $('#createVehicleId').val('').trigger('change');
            $('#createCustomerId').val('').trigger('change');
            $('#createInspectorId').val('').trigger('change');
        }
    },

    loadVehicles: function() {
        var self = this;
        $.ajax({
            url: '/VehicleManagement/GetAvailableVehicles',
            type: 'GET',
            success: function(response) {
                if (response && Array.isArray(response)) {
                    var vehicles = response;
                    var options = '<option value="">Chọn xe</option>';
                    vehicles.forEach(function(vehicle) {
                        options += `<option value="${vehicle.value}" data-customer-id="${vehicle.customerId}" data-customer-name="${vehicle.customerName}">${vehicle.text}</option>`;
                    });
                    $('#createVehicleId').html(options);
                    
                    // Thêm event handler khi chọn xe
                    self.setupVehicleChangeHandler();
                } else {
                }
            },
            error: function(xhr, status, error) {
            }
        });
    },

    setupVehicleChangeHandler: function() {
        var self = this;
        
        // Event handler cho dropdown xe (chỉ cho create modal)
        $('#createVehicleId').on('change', function() {
            var selectedVehicle = $(this).find('option:selected');
            var customerId = selectedVehicle.data('customer-id');
            var customerName = selectedVehicle.data('customer-name');
            
            if (customerId && customerName) {
                // Tự động set khách hàng tương ứng
                $(this).closest('.modal').find('#createCustomerId, #editCustomerId').val(customerId).trigger('change');
                
                // Hiển thị tên khách hàng (disable dropdown)
                var customerDisplay = $(this).closest('.modal').find('#createCustomerId, #editCustomerId');
                customerDisplay.prop('disabled', true);
                customerDisplay.attr('title', 'Khách hàng được tự động chọn theo xe đã chọn');
            } else {
                // Reset về trạng thái ban đầu
                var customerDisplay = $(this).closest('.modal').find('#createCustomerId, #editCustomerId');
                customerDisplay.prop('disabled', false);
                customerDisplay.removeAttr('title');
                customerDisplay.val('');
            }
        });
    },

    loadCustomers: function() {
        $.ajax({
            url: '/VehicleManagement/GetAvailableCustomers',
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

    loadEmployees: function() {
        $.ajax({
            url: '/VehicleManagement/GetAvailableEmployees',
            type: 'GET',
            success: function(response) {
                if (response && Array.isArray(response)) {
                    var employees = response;
                    var options = '<option value="">Chọn kỹ thuật viên</option>';
                    employees.forEach(function(employee) {
                        options += `<option value="${employee.value}">${employee.text}</option>`;
                    });
                    $('#createInspectorId, #editInspectorId').html(options);
                } else {
                }
            },
            error: function(xhr, status, error) {
            }
        });
    },

    showCreateModal: function() {
        $('#createInspectionForm')[0].reset();
        $('#createInspectionModal').modal('show');
        
        // Reset trạng thái dropdown khách hàng
        $('#createCustomerId').prop('disabled', true).val('').trigger('change');
        
        // Đợi modal hiển thị xong rồi mới load data và set default values
        $('#createInspectionModal').on('shown.bs.modal', function() {
            // Set default inspection date to current date only (no time)
            var now = new Date();
            var localDate = now.getFullYear() + '-' + 
                String(now.getMonth() + 1).padStart(2, '0') + '-' + 
                String(now.getDate()).padStart(2, '0');
            $('#createInspectionDate').val(localDate);
            
            InspectionManagement.loadReceptions();
        });
    },

    viewInspection: function(id) {
        var self = this;
        $.ajax({
            url: '/VehicleManagement/GetInspection/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success && response.data) {
                        var inspection = response.data;
                        self.populateViewModal(inspection);
                        $('#viewInspectionModal').modal('show');
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi tải thông tin kiểm tra xe');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin kiểm tra xe');
                }
            }
        });
    },

    editInspection: function(id) {
        var self = this;
        $.ajax({
            url: '/VehicleManagement/GetInspection/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success && response.data) {
                        var inspection = response.data;
                        self.populateEditModal(inspection);
                        $('#editInspectionModal').modal('show');
                        
                        // Reset trạng thái dropdown khách hàng
                        $('#editCustomerId').prop('disabled', true);
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi tải thông tin kiểm tra xe');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin kiểm tra xe');
                }
            }
        });
    },

    createInspection: function() {
        var self = this;
        var selectedVehicle = $('#createVehicleId').find('option:selected');
        var customerId = selectedVehicle.data('customer-id');
        
        var formData = {
            CustomerReceptionId: parseInt($('#createCustomerReceptionId').val()),
            VehicleId: parseInt($('#createVehicleId').val()),
            CustomerId: customerId ? parseInt(customerId) : null,
            InspectorId: parseInt($('#createInspectorId').val()),
            InspectionDate: $('#createInspectionDate').val() || null,
            InspectionType: $('#createInspectionType').val() || null,
            CurrentMileage: parseInt($('#createCurrentMileage').val()) || null,
            FuelLevel: $('#createFuelLevel').val() || null,
            GeneralCondition: $('#createGeneralCondition').val() || null,
            Status: $('#createStatus').val() || null,
            ExteriorCondition: $('#createExteriorCondition').val() || null,
            InteriorCondition: $('#createInteriorCondition').val() || null,
            EngineCondition: $('#createEngineCondition').val() || null,
            BrakeCondition: $('#createBrakeCondition').val() || null,
            SuspensionCondition: $('#createSuspensionCondition').val() || null,
            TireCondition: $('#createTireCondition').val() || null,
            ElectricalCondition: $('#createElectricalCondition').val() || null,
            LightsCondition: $('#createLightsCondition').val() || null,
            CustomerComplaints: $('#createCustomerComplaints').val() || null,
            Recommendations: $('#createRecommendations').val() || null,
            TechnicianNotes: $('#createTechnicianNotes').val() || null,
            Issues: [] // Empty array for now, can be extended later
        };

        // Validate required fields
        if (!formData.VehicleId || !formData.InspectorId) {
            GarageApp.showError('Vui lòng chọn xe và kỹ thuật viên');
            return;
        }

        $.ajax({
            url: '/VehicleManagement/CreateInspection',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Tạo kiểm tra xe thành công!');
                        $('#createInspectionModal').modal('hide');
                        self.inspectionTable.ajax.reload();
                    } else {
                        // Parse error message from server response
                        var errorMessage = 'Lỗi khi tạo kiểm tra xe';
                        try {
                            if (response.error) {
                                var errorText = response.error;
                                
                                // Check if error contains nested JSON (API Error format)
                                if (errorText.includes('API Error: BadRequest - ')) {
                                    var jsonStart = errorText.indexOf('{');
                                    if (jsonStart !== -1) {
                                        var nestedJson = errorText.substring(jsonStart);
                                        var nestedResponse = JSON.parse(nestedJson);
                                        if (nestedResponse.message) {
                                            errorMessage = nestedResponse.message;
                                        } else {
                                            errorMessage = errorText;
                                        }
                                    } else {
                                        errorMessage = errorText;
                                    }
                                } else {
                                    errorMessage = errorText;
                                }
                            }
                        } catch (e) {
                        }
                        GarageApp.showError(errorMessage);
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    // Parse error message from server response
                    var errorMessage = 'Lỗi khi tạo kiểm tra xe';
                    try {
                        if (xhr.responseJSON && xhr.responseJSON.error) {
                            var errorText = xhr.responseJSON.error;
                            
                            // Check if error contains nested JSON (API Error format)
                            if (errorText.includes('API Error: BadRequest - ')) {
                                var jsonStart = errorText.indexOf('{');
                                if (jsonStart !== -1) {
                                    var nestedJson = errorText.substring(jsonStart);
                                    var nestedResponse = JSON.parse(nestedJson);
                                    if (nestedResponse.message) {
                                        errorMessage = nestedResponse.message;
                                    } else {
                                        errorMessage = errorText;
                                    }
                                } else {
                                    errorMessage = errorText;
                                }
                            } else {
                                errorMessage = errorText;
                            }
                        } else if (xhr.responseText) {
                            var response = JSON.parse(xhr.responseText);
                            if (response.error) {
                                errorMessage = response.error;
                            }
                        }
                    } catch (e) {
                    }
                    GarageApp.showError(errorMessage);
                }
            }
        });
    },

    updateInspection: function() {
        var self = this;
        var inspectionId = $('#editId').val();
        var selectedVehicle = $('#editVehicleId').find('option:selected');
        var customerId = selectedVehicle.data('customer-id');
        
        var formData = {
            Id: parseInt(inspectionId),
            VehicleId: parseInt($('#editVehicleId').val()),
            CustomerId: customerId ? parseInt(customerId) : null,
            InspectorId: parseInt($('#editInspectorId').val()),
            InspectionDate: $('#editInspectionDate').val() || null,
            InspectionType: $('#editInspectionType').val() || null,
            CurrentMileage: parseInt($('#editCurrentMileage').val()) || null,
            FuelLevel: $('#editFuelLevel').val() || null,
            GeneralCondition: $('#editGeneralCondition').val() || null,
            ExteriorCondition: $('#editExteriorCondition').val() || null,
            InteriorCondition: $('#editInteriorCondition').val() || null,
            EngineCondition: $('#editEngineCondition').val() || null,
            BrakeCondition: $('#editBrakeCondition').val() || null,
            SuspensionCondition: $('#editSuspensionCondition').val() || null,
            TireCondition: $('#editTireCondition').val() || null,
            ElectricalCondition: $('#editElectricalCondition').val() || null,
            LightsCondition: $('#editLightsCondition').val() || null,
            CustomerComplaints: $('#editCustomerComplaints').val() || null,
            Recommendations: $('#editRecommendations').val() || null,
            TechnicianNotes: $('#editTechnicianNotes').val() || null,
            Status: $('#editStatus').val() || null,
            Issues: [] // Empty array for now, can be extended later
        };

        // Validate required fields
        if (!formData.VehicleId || !formData.InspectorId) {
            GarageApp.showError('Vui lòng chọn xe và kỹ thuật viên');
            return;
        }

        $.ajax({
            url: '/VehicleManagement/UpdateInspection/' + inspectionId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Cập nhật kiểm tra xe thành công!');
                        $('#editInspectionModal').modal('hide');
                        self.inspectionTable.ajax.reload();
                    } else {
                        // Parse error message from server response
                        var errorMessage = 'Lỗi khi cập nhật kiểm tra xe';
                        try {
                            if (response.error) {
                                var errorText = response.error;
                                
                                // Check if error contains nested JSON (API Error format)
                                if (errorText.includes('API Error: BadRequest - ')) {
                                    var jsonStart = errorText.indexOf('{');
                                    if (jsonStart !== -1) {
                                        var nestedJson = errorText.substring(jsonStart);
                                        var nestedResponse = JSON.parse(nestedJson);
                                        if (nestedResponse.message) {
                                            errorMessage = nestedResponse.message;
                                        } else {
                                            errorMessage = errorText;
                                        }
                                    } else {
                                        errorMessage = errorText;
                                    }
                                } else {
                                    errorMessage = errorText;
                                }
                            }
                        } catch (e) {
                        }
                        GarageApp.showError(errorMessage);
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    // Parse error message from server response
                    var errorMessage = 'Lỗi khi cập nhật kiểm tra xe';
                    try {
                        if (xhr.responseJSON && xhr.responseJSON.error) {
                            var errorText = xhr.responseJSON.error;
                            
                            // Check if error contains nested JSON (API Error format)
                            if (errorText.includes('API Error: BadRequest - ')) {
                                var jsonStart = errorText.indexOf('{');
                                if (jsonStart !== -1) {
                                    var nestedJson = errorText.substring(jsonStart);
                                    var nestedResponse = JSON.parse(nestedJson);
                                    if (nestedResponse.message) {
                                        errorMessage = nestedResponse.message;
                                    } else {
                                        errorMessage = errorText;
                                    }
                                } else {
                                    errorMessage = errorText;
                                }
                            } else {
                                errorMessage = errorText;
                            }
                        } else if (xhr.responseText) {
                            var response = JSON.parse(xhr.responseText);
                            if (response.error) {
                                errorMessage = response.error;
                            }
                        }
                    } catch (e) {
                    }
                    GarageApp.showError(errorMessage);
                }
            }
        });
    },

    deleteInspection: function(id) {
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
                    url: '/VehicleManagement/DeleteInspection/' + id,
                    type: 'DELETE',
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Xóa kiểm tra xe thành công!');
                                self.inspectionTable.ajax.reload();
                            } else {
                                GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi xóa kiểm tra xe');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            // Parse error message from server response
                            var errorMessage = 'Lỗi khi xóa kiểm tra xe';
                            try {
                                if (xhr.responseJSON && xhr.responseJSON.error) {
                                    var errorText = xhr.responseJSON.error;
                                    
                                    // Check if error contains nested JSON (API Error format)
                                    if (errorText.includes('API Error: BadRequest - ')) {
                                        var jsonStart = errorText.indexOf('{');
                                        if (jsonStart !== -1) {
                                            var nestedJson = errorText.substring(jsonStart);
                                            var nestedResponse = JSON.parse(nestedJson);
                                            if (nestedResponse.message) {
                                                errorMessage = nestedResponse.message;
                                            } else {
                                                errorMessage = errorText;
                                            }
                                        } else {
                                            errorMessage = errorText;
                                        }
                                    } else {
                                        errorMessage = errorText;
                                    }
                                } else if (xhr.responseText) {
                                    var response = JSON.parse(xhr.responseText);
                                    if (response.error) {
                                        errorMessage = response.error;
                                    }
                                }
                            } catch (e) {
                            }
                            GarageApp.showError(errorMessage);
                        }
                    }
                });
            }
        });
    },

    populateViewModal: function(inspection) {
        $('#viewInspectionNumber').text(inspection.inspectionNumber || '');
        $('#viewInspectionDate').text(inspection.inspectionDate ? new Date(inspection.inspectionDate).toLocaleDateString('vi-VN') : '');
        $('#viewVehicleInfo').text(inspection.vehicle ? `${inspection.vehicle.brand} ${inspection.vehicle.model} - ${inspection.vehicle.licensePlate}` : '');
        $('#viewCustomerName').text(inspection.customer ? inspection.customer.name : '');
        $('#viewInspectorName').text(inspection.inspector ? inspection.inspector.name : '');
        $('#viewInspectionType').text(inspection.inspectionType || '');
        $('#viewCurrentMileage').text(inspection.currentMileage ? inspection.currentMileage.toLocaleString() + ' km' : '');
        $('#viewFuelLevel').text(inspection.fuelLevel || '');
        $('#viewGeneralCondition').text(inspection.generalCondition || '');
        $('#viewExteriorCondition').text(inspection.exteriorCondition || '');
        $('#viewInteriorCondition').text(inspection.interiorCondition || '');
        $('#viewEngineCondition').text(inspection.engineCondition || '');
        $('#viewBrakeCondition').text(inspection.brakeCondition || '');
        $('#viewSuspensionCondition').text(inspection.suspensionCondition || '');
        $('#viewTireCondition').text(inspection.tireCondition || '');
        $('#viewElectricalCondition').text(inspection.electricalCondition || '');
        $('#viewLightsCondition').text(inspection.lightsCondition || '');
        $('#viewStatus').text(inspection.status || '');
        $('#viewCustomerComplaints').text(inspection.customerComplaints || '');
        $('#viewRecommendations').text(inspection.recommendations || '');
        $('#viewTechnicianNotes').text(inspection.technicianNotes || '');
    },

    populateEditModal: function(inspection) {
        $('#editId').val(inspection.id);
        $('#editVehicleId').val(inspection.vehicleId);
        
        // Set khách hàng theo xe (không cần trigger change vì xe đã disabled)
        if (inspection.customerId) {
            $('#editCustomerId').val(inspection.customerId);
        }
        
        $('#editInspectorId').val(inspection.inspectorId).trigger('change');
        $('#editInspectionDate').val(inspection.inspectionDate ? new Date(inspection.inspectionDate).toISOString().split('T')[0] : '');
        $('#editInspectionType').val(inspection.inspectionType || '');
        $('#editCurrentMileage').val(inspection.currentMileage || '');
        $('#editFuelLevel').val(inspection.fuelLevel || '');
        $('#editGeneralCondition').val(inspection.generalCondition || '');
        $('#editExteriorCondition').val(inspection.exteriorCondition || '');
        $('#editInteriorCondition').val(inspection.interiorCondition || '');
        $('#editEngineCondition').val(inspection.engineCondition || '');
        $('#editBrakeCondition').val(inspection.brakeCondition || '');
        $('#editSuspensionCondition').val(inspection.suspensionCondition || '');
        $('#editTireCondition').val(inspection.tireCondition || '');
        $('#editElectricalCondition').val(inspection.electricalCondition || '');
        $('#editLightsCondition').val(inspection.lightsCondition || '');
        $('#editCustomerComplaints').val(inspection.customerComplaints || '');
        $('#editRecommendations').val(inspection.recommendations || '');
        $('#editTechnicianNotes').val(inspection.technicianNotes || '');
        $('#editStatus').val(inspection.status || '');
    },

    // Complete inspection
    completeInspection: function(id) {
        var self = this;
        
        Swal.fire({
            title: 'Xác nhận hoàn thành kiểm tra?',
            text: 'Bạn có chắc chắn muốn đánh dấu kiểm tra xe này là đã hoàn thành?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Hoàn thành',
            cancelButtonText: 'Hủy',
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/VehicleManagement/CompleteInspection/' + id,
                    type: 'POST',
                    success: function(response) {
                        if (response.success) {
                            Swal.fire({
                                title: 'Thành công!',
                                text: 'Đã hoàn thành kiểm tra xe',
                                icon: 'success',
                                timer: 2000,
                                showConfirmButton: false,
                            });
                            self.dataTable.ajax.reload();
                        } else {
                            GarageApp.showError(response.message || 'Có lỗi xảy ra khi hoàn thành kiểm tra');
                        }
                    },
                    error: function(xhr, status, error) {
                        var errorMessage = 'Lỗi khi hoàn thành kiểm tra xe';
                        if (xhr.responseJSON && xhr.responseJSON.message) {
                            errorMessage = xhr.responseJSON.message;
                        }
                        GarageApp.showError(errorMessage);
                    }
                });
            }
        });
    },

    // Translate GeneralCondition từ English sang Vietnamese
    translateGeneralCondition: function(condition) {
        if (!condition) return '';
        
        switch (condition.toLowerCase()) {
            case 'excellent': return 'Xuất sắc';
            case 'good': return 'Tốt';
            case 'fair': return 'Khá';
            case 'poor': return 'Kém';
            default: return condition; // Trả về nguyên gốc nếu không match
        }
    }
};

$(document).ready(function() {
    InspectionManagement.init();
});
