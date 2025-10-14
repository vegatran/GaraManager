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
        
        this.inspectionTable = DataTablesVietnamese.init('#inspectionTable', {
            ajax: {
                url: '/InspectionManagement/GetInspections',
                type: 'GET',
                error: function(xhr, status, error) {
                    if (AuthHandler.isUnauthorized(xhr)) {
                        AuthHandler.handleUnauthorized(xhr, true);
                    } else {
                        console.error('Error loading inspections:', error);
                        GarageApp.showError('Lỗi khi tải danh sách kiểm tra xe');
                    }
                }
            },
            columns: [
                { data: 'id', title: 'ID', width: '5%' },
                { data: 'inspectionNumber', title: 'Số Kiểm Tra', width: '10%' },
                { data: 'vehicleInfo', title: 'Thông Tin Xe', width: '15%' },
                { data: 'customerName', title: 'Khách Hàng', width: '12%' },
                { data: 'inspectorName', title: 'Kỹ Thuật Viên', width: '12%' },
                { data: 'inspectionDate', title: 'Ngày Kiểm Tra', width: '10%' },
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
                { data: 'generalCondition', title: 'Tình Trạng', width: '10%' },
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
                        
                        // Chỉ hiển thị nút Edit và Delete khi trạng thái chưa hoàn thành
                        var isCompleted = row.status === 'Completed' || 
                                         row.status === 'Hoàn Thành' ||  // Chữ T viết hoa (từ TranslateStatus)
                                         row.status === 'Hoàn thành' || 
                                         row.status === 'completed' || 
                                         row.status === 'hoàn thành' ||
                                         (row.status && row.status.toLowerCase().includes('hoàn thành')) ||
                                         (row.status && row.status.toLowerCase().includes('completed'));
                        
                        if (!isCompleted) {
                            actions += `
                                <button type="button" class="btn btn-warning btn-sm edit-inspection" data-id="${row.id}" title="Sửa">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button type="button" class="btn btn-danger btn-sm delete-inspection" data-id="${row.id}" title="Xóa">
                                    <i class="fas fa-trash"></i>
                                </button>
                            `;
                        }
                        
                        actions += `
                            </div>
                        `;
                        
                        return actions;
                    }
                }
            ],
            order: [[0, 'desc']],
            pageLength: 25
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
    },

    loadDropdowns: function() {
        this.loadVehicles();
        this.loadCustomers();
        this.loadEmployees();
    },

    loadVehicles: function() {
        var self = this;
        $.ajax({
            url: '/InspectionManagement/GetAvailableVehicles',
            type: 'GET',
            success: function(response) {
                if (response && Array.isArray(response)) {
                    var vehicles = response;
                    var options = '<option value="">Chọn xe</option>';
                    vehicles.forEach(function(vehicle) {
                        options += `<option value="${vehicle.value}" data-customer-id="${vehicle.customerId}" data-customer-name="${vehicle.customerName}">${vehicle.text}</option>`;
                    });
                    $('#createVehicleId, #editVehicleId').html(options);
                    
                    // Thêm event handler khi chọn xe
                    self.setupVehicleChangeHandler();
                } else {
                    console.error('Invalid vehicle data format:', response);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading vehicles:', error);
            }
        });
    },

    setupVehicleChangeHandler: function() {
        var self = this;
        
        // Event handler cho dropdown xe
        $('#createVehicleId, #editVehicleId').on('change', function() {
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
            url: '/InspectionManagement/GetAvailableCustomers',
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

    loadEmployees: function() {
        $.ajax({
            url: '/InspectionManagement/GetAvailableEmployees',
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
                    console.error('Invalid employee data format:', response);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading employees:', error);
            }
        });
    },

    showCreateModal: function() {
        $('#createInspectionForm')[0].reset();
        $('#createInspectionModal').modal('show');
        
        // Reset trạng thái dropdown khách hàng
        $('#createCustomerId').prop('disabled', true).val('').trigger('change');
    },

    viewInspection: function(id) {
        var self = this;
        $.ajax({
            url: '/InspectionManagement/GetInspection/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success && response.data) {
                        var inspection = response.data;
                        self.populateViewModal(inspection);
                        $('#viewInspectionModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Lỗi khi tải thông tin kiểm tra xe');
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
            url: '/InspectionManagement/GetInspection/' + id,
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
                        GarageApp.showError(response.message || 'Lỗi khi tải thông tin kiểm tra xe');
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
            url: '/InspectionManagement/CreateInspection',
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
                        GarageApp.showError(response.message || 'Lỗi khi tạo kiểm tra xe');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tạo kiểm tra xe');
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
            url: '/InspectionManagement/UpdateInspection/' + inspectionId,
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
                        GarageApp.showError(response.message || 'Lỗi khi cập nhật kiểm tra xe');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi cập nhật kiểm tra xe');
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
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/InspectionManagement/DeleteInspection/' + id,
                    type: 'DELETE',
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Xóa kiểm tra xe thành công!');
                                self.inspectionTable.ajax.reload();
                            } else {
                                GarageApp.showError(response.message || 'Lỗi khi xóa kiểm tra xe');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi xóa kiểm tra xe');
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
        $('#editVehicleId').val(inspection.vehicleId).trigger('change');
        
        // Khách hàng sẽ được tự động set theo xe trong setupVehicleChangeHandler
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
    }
};

$(document).ready(function() {
    InspectionManagement.init();
});
