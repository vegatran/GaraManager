/**
 * Customer Reception Management JavaScript Module
 * Quản lý tiếp đón khách hàng theo pattern chuẩn của dự án
 */
window.CustomerReceptionManagement = (function() {
    'use strict';

    let receptionTable;
    let customers = [];
    let vehicles = [];
    let technicians = [];

    /**
     * Initialize the module
     */
    function init() {
        initializeDataTable();
        loadDropdownData();
        bindEvents();
    }

    /**
     * Initialize DataTable
     */
    function initializeDataTable() {
        var columns = [
            { data: 'id', title: 'ID', width: '60px' },
            { data: 'receptionNumber', title: 'Số Phiếu' },
            { data: 'customerName', title: 'Khách Hàng' },
            { data: 'vehiclePlate', title: 'Biển Số' },
            { data: 'vehicleInfo', title: 'Xe' },
            { 
                data: 'receptionDate', 
                title: 'Ngày Tiếp Đón',
                render: DataTablesUtility.renderDate
            },
            { data: 'technicianName', title: 'Kỹ Thuật Viên' },
            { 
                data: 'status', 
                title: 'Trạng Thái',
                render: function(data, type, row) {
                    const statusClass = getStatusClass(data);
                    const statusText = translateReceptionStatus(data);
                    return `<span class="badge badge-${statusClass}">${statusText}</span>`;
                }
            },
            { 
                data: 'priority', 
                title: 'Ưu Tiên',
                render: function(data, type, row) {
                    const priorityClass = getPriorityClass(data);
                    return `<span class="badge badge-${priorityClass}">${data}</span>`;
                }
            },
            { data: 'serviceType', title: 'Loại Dịch Vụ' },
            {
                data: null,
                title: 'Thao Tác',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    let actions = `
                        <div class="btn-group" role="group">
                            <button class="btn btn-info btn-sm view-reception" data-id="${row.id}" title="Xem">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn btn-primary btn-sm print-reception" data-id="${row.id}" title="In Biên Bản">
                                <i class="fas fa-print"></i>
                            </button>
                    `;

                    if (row.status !== 3 && row.status !== 4) {
                        actions += `
                            <button class="btn btn-warning btn-sm edit-reception" data-id="${row.id}">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-danger btn-sm delete-reception" data-id="${row.id}">
                                <i class="fas fa-trash"></i>
                            </button>
                        `;
                    }

                    if (row.status === 0 || (row.status === 1 && (!row.technicianName || row.technicianName === 'Chưa phân công'))) {
                        actions += `
                            <button class="btn btn-success btn-sm assign-technician" data-id="${row.id}" title="Phân Công KTV">
                                <i class="fas fa-user-plus"></i>
                            </button>
                        `;
                    }

                    actions += `</div>`;
                    return actions;
                }
            }
        ];
        
        receptionTable = DataTablesUtility.initServerSideTable('#receptionTable', '/CustomerReception/GetReceptions', columns, {
            order: [[0, 'desc']],
            pageLength: 10
        });
    }

    /**
     * Load dropdown data
     */
    function loadDropdownData() {
        loadCustomers();
        loadVehicles();
        loadTechnicians();
    }

    /**
     * Load customers for dropdown
     */
    function loadCustomers() {
        $.ajax({
            url: '/CustomerReception/GetAvailableCustomers',
            type: 'GET',
            success: function(data) {
                customers = data;
                populateCustomerDropdowns(data);
            },
            error: function() {
                GarageApp.showError('Lỗi khi tải danh sách khách hàng');
            }
        });
    }

    /**
     * Load vehicles for dropdown
     */
    function loadVehicles() {
        $.ajax({
            url: '/CustomerReception/GetAvailableVehicles',
            type: 'GET',
            success: function(data) {
                vehicles = data;
                // Không populate ngay, sẽ populate khi chọn khách hàng
                resetVehicleDropdown('#createVehicleSelect');
                resetVehicleDropdown('#editVehicleSelect');
            },
            error: function() {
                GarageApp.showError('Lỗi khi tải danh sách xe');
            }
        });
    }

    /**
     * Filter vehicles by customer ID
     */
    function filterVehiclesByCustomer(customerId, vehicleSelect) {
        const customerVehicles = vehicles.filter(vehicle => vehicle.customerId == customerId);
        populateVehicleDropdown(vehicleSelect, customerVehicles);
    }

    /**
     * Reset vehicle dropdown
     */
    function resetVehicleDropdown(vehicleSelect) {
        const $select = $(vehicleSelect);
        $select.empty().append('<option value="">-- Chọn xe --</option>');
    }

    /**
     * Load technicians for dropdown
     */
    function loadTechnicians() {
        $.ajax({
            url: '/CustomerReception/GetAvailableTechnicians',
            type: 'GET',
            success: function(data) {
                technicians = data;
                populateTechnicianDropdowns(data);
            },
            error: function() {
                GarageApp.showError('Lỗi khi tải danh sách kỹ thuật viên');
            }
        });
    }

    /**
     * Populate customer dropdowns
     */
    function populateCustomerDropdowns(data) {
        const dropdowns = ['#createCustomerSelect', '#editCustomerSelect'];
        dropdowns.forEach(selector => {
            const $dropdown = $(selector);
            $dropdown.empty().append('<option value="">-- Chọn khách hàng --</option>');
            data.forEach(customer => {
                $dropdown.append(`<option value="${customer.value}">${customer.text}</option>`);
            });
        });
    }

    /**
     * Populate single vehicle dropdown
     */
    function populateVehicleDropdown(selector, data) {
        const $dropdown = $(selector);
        $dropdown.empty().append('<option value="">-- Chọn xe --</option>');
        data.forEach(vehicle => {
            $dropdown.append(`<option value="${vehicle.value}" data-customer-id="${vehicle.customerId}">${vehicle.text}</option>`);
        });
    }

    /**
     * Populate vehicle dropdowns
     */
    function populateVehicleDropdowns(data) {
        const dropdowns = ['#createVehicleSelect', '#editVehicleSelect'];
        dropdowns.forEach(selector => {
            populateVehicleDropdown(selector, data);
        });
    }

    /**
     * Populate technician dropdowns
     */
    function populateTechnicianDropdowns(data) {
        const dropdowns = ['#createTechnicianSelect', '#editTechnicianSelect'];
        dropdowns.forEach(selector => {
            const $dropdown = $(selector);
            $dropdown.empty().append('<option value="">-- Chọn kỹ thuật viên --</option>');
            data.forEach(technician => {
                $dropdown.append(`<option value="${technician.value}">${technician.text}</option>`);
            });
        });
    }

    /**
     * Bind events
     */
    function bindEvents() {

        // Add reception button
        $(document).on('click', '#addReceptionBtn', function() {
            showCreateModal();
        });

        // View reception
        $(document).on('click', '.view-reception', function() {
            var id = $(this).data('id');
            viewReception(id);
        });

        // Edit reception
        $(document).on('click', '.edit-reception', function() {
            var id = $(this).data('id');
            editReception(id);
        });

        // Delete reception
        $(document).on('click', '.delete-reception', function() {
            var id = $(this).data('id');
            deleteReception(id);
        });

        // Assign technician
        $(document).on('click', '.assign-technician', function() {
            var id = $(this).data('id');
            assignTechnician(id);
        });

        // Print reception
        $(document).on('click', '.print-reception, #printReceptionBtn', function() {
            var id = $(this).data('id') || $('#viewReceptionModal').data('reception-id');
            if (id) {
                window.open(`/CustomerReception/PrintReception/${id}`, '_blank');
            } else {
                GarageApp.showError('Không tìm thấy ID phiếu tiếp đón');
            }
        });

        // Create form submission
        $(document).on('submit', '#createReceptionForm', function(e) {
            e.preventDefault();
            createReception();
        });

        // Edit form submission
        $(document).on('submit', '#editReceptionForm', function(e) {
            e.preventDefault();
            updateReception();
        });

        // Customer selection change - filter vehicles
        $('#createCustomerSelect, #editCustomerSelect').on('change', function() {
            const selectedCustomer = $(this).find('option:selected');
            const customerId = selectedCustomer.val();
            const isCreateForm = $(this).attr('id').includes('create');
            const vehicleSelect = isCreateForm ? '#createVehicleSelect' : '#editVehicleSelect';
            
            if (customerId) {
                // Filter vehicles by selected customer
                filterVehiclesByCustomer(customerId, vehicleSelect);
            } else {
                // Reset vehicle dropdown
                resetVehicleDropdown(vehicleSelect);
            }
        });

        // Vehicle selection change - auto-select customer
        $('#createVehicleSelect, #editVehicleSelect').on('change', function() {
            const selectedVehicle = $(this).find('option:selected');
            const customerId = selectedVehicle.data('customer-id');
            const isCreateForm = $(this).attr('id').includes('create');
            const customerSelect = isCreateForm ? '#createCustomerSelect' : '#editCustomerSelect';
            
            if (customerId) {
                $(customerSelect).val(customerId);
            }
        });
    }

    /**
     * Create new reception
     */
    function createReception() {
        const formData = {
            customerId: parseInt($('#createCustomerSelect').val()),
            vehicleId: parseInt($('#createVehicleSelect').val()),
            serviceType: $('#createServiceType').val(),
            priority: $('#createPriority').val(),
            assignedTechnicianId: $('#createTechnicianSelect').val() ? parseInt($('#createTechnicianSelect').val()) : null,
            inspectionStartDate: $('#createInspectionStartDate').val() ? $('#createInspectionStartDate').val() : null,
            inspectionCompletedDate: $('#createInspectionCompletedDate').val() ? $('#createInspectionCompletedDate').val() : null,
            customerRequest: $('#createCustomerRequest').val(),
            customerComplaints: $('#createCustomerComplaints').val(),
            insuranceCompany: $('#createInsuranceCompany').val(),
            insurancePolicyNumber: $('#createInsurancePolicy').val(),
            // insuranceClaimDate: DTO không có field này
            emergencyContact: $('#createEmergencyContact').val(),
            emergencyContactName: $('#createEmergencyContactName').val(),
            receptionNotes: $('#createReceptionNotes').val()
        };

        $.ajax({
            url: '/CustomerReception/CreateReception',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess('Tạo phiếu tiếp đón thành công');
                    $('#createReceptionModal').modal('hide');
                    receptionTable.ajax.reload();
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(response));
                }
            },
            error: function() {
                GarageApp.showError('Lỗi khi tạo phiếu tiếp đón');
            }
        });
    }

    /**
     * View reception details
     */
    function viewReception(id) {
        $.ajax({
            url: `/CustomerReception/GetReception/${id}`,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    populateViewModal(response.data);
                    // Store reception ID for print button
                    $('#viewReceptionModal').data('reception-id', id);
                    $('#viewReceptionModal').modal('show');
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi tải thông tin phiếu tiếp đón');
                }
            },
            error: function() {
                GarageApp.showError('Lỗi khi tải thông tin phiếu tiếp đón');
            }
        });
    }

    /**
     * Edit reception
     */
    function editReception(id) {
        // Đảm bảo vehicles đã được load trước khi populate
        if (vehicles.length === 0) {
            loadVehicles();
        }
        
        $.ajax({
            url: `/CustomerReception/GetReception/${id}`,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    // Đợi một chút để đảm bảo vehicles dropdown đã được populate
                    setTimeout(function() {
                        populateEditModal(response.data);
                    }, 100);
                    $('#editReceptionModal').modal('show');
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi tải thông tin phiếu tiếp đón');
                }
            },
            error: function() {
                GarageApp.showError('Lỗi khi tải thông tin phiếu tiếp đón');
            }
        });
    }

    /**
     * Update reception
     */
    function updateReception() {
        const id = parseInt($('#editReceptionId').val());
        const assignedTechnicianId = $('#editTechnicianSelect').val() ? parseInt($('#editTechnicianSelect').val()) : null;
        
        const formData = {
            id: id,
            receptionNumber: $('#editReceptionNumber').val(),
            // ❌ KHÔNG gửi customerId và vehicleId vì đã disable
            // customerId: parseInt($('#editCustomerSelect').val()),
            // vehicleId: parseInt($('#editVehicleSelect').val()),
            serviceType: $('#editServiceType').val(),
            priority: $('#editPriority').val(),
            assignedTechnicianId: assignedTechnicianId,
            // ✅ Tính toán status dựa trên assignedTechnicianId
            status: assignedTechnicianId ? 1 : 0, // ReceptionStatus.Assigned : ReceptionStatus.Pending
            inspectionStartDate: $('#editInspectionStartDate').val() ? $('#editInspectionStartDate').val() : null,
            inspectionCompletedDate: $('#editInspectionCompletedDate').val() ? $('#editInspectionCompletedDate').val() : null,
            customerRequest: $('#editCustomerRequest').val(),
            customerComplaints: $('#editCustomerComplaints').val(),
            insuranceCompany: $('#editInsuranceCompany').val(),
            insurancePolicyNumber: $('#editInsurancePolicy').val(),
            // insuranceClaimDate: DTO không có field này
            emergencyContact: $('#editEmergencyContact').val(),
            emergencyContactName: $('#editEmergencyContactName').val(),
            receptionNotes: $('#editReceptionNotes').val()
        };

        $.ajax({
            url: `/CustomerReception/UpdateReception/${id}`,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess('Cập nhật phiếu tiếp đón thành công');
                    $('#editReceptionModal').modal('hide');
                    receptionTable.ajax.reload();
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi cập nhật phiếu tiếp đón');
                }
            },
            error: function() {
                GarageApp.showError('Lỗi khi cập nhật phiếu tiếp đón');
            }
        });
    }

    /**
     * Delete reception
     */
    function deleteReception(id) {
        Swal.fire({
            title: 'Xác nhận xóa',
            text: 'Bạn có chắc chắn muốn xóa phiếu tiếp đón này?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: `/CustomerReception/DeleteReception/${id}`,
                    type: 'DELETE',
                    success: function(response) {
                        if (response.success) {
                            GarageApp.showSuccess('Xóa phiếu tiếp đón thành công');
                            receptionTable.ajax.reload();
                        } else {
                            GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi xóa phiếu tiếp đón');
                        }
                    },
                    error: function() {
                        GarageApp.showError('Lỗi khi xóa phiếu tiếp đón');
                    }
                });
            }
        });
    }

    /**
     * Assign technician
     */
    function assignTechnician(id) {
        // Load technicians if not already loaded
        if (technicians.length === 0) {
            loadTechniciansForAssign(id);
            return;
        }
        
        showAssignTechnicianModal(id);
    }

    /**
     * Load technicians for assignment
     */
    function loadTechniciansForAssign(receptionId) {
        $.ajax({
            url: '/CustomerReception/GetAvailableTechnicians',
            type: 'GET',
            success: function(data) {
                technicians = data;
                showAssignTechnicianModal(receptionId);
            },
            error: function() {
                GarageApp.showError('Lỗi khi tải danh sách kỹ thuật viên');
            }
        });
    }

    /**
     * Show assign technician modal
     */
    function showAssignTechnicianModal(id) {
        if (technicians.length === 0) {
            GarageApp.showError('Không có kỹ thuật viên nào khả dụng');
            return;
        }

        // Load current reception info to get assigned technician
        $.ajax({
            url: `/CustomerReception/GetReception/${id}`,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    var currentTechnicianId = response.data.assignedTechnicianId;
                    
                    Swal.fire({
                        title: 'Phân Công Kỹ Thuật Viên',
                        input: 'select',
                        inputOptions: technicians.reduce((acc, tech) => {
                            acc[tech.value] = tech.text;
                            return acc;
                        }, {}),
                        inputValue: currentTechnicianId || '', // Pre-select current technician
                        inputPlaceholder: 'Chọn kỹ thuật viên',
                        showCancelButton: true,
                        confirmButtonText: 'Phân Công',
                        cancelButtonText: 'Hủy',
                        inputValidator: (value) => {
                            if (!value) {
                                return 'Vui lòng chọn kỹ thuật viên';
                            }
                        }
                    }).then((result) => {
                        if (result.isConfirmed) {
                            $.ajax({
                                url: `/CustomerReception/AssignTechnician/${id}?technicianId=${result.value}`,
                                type: 'PUT',
                                success: function(response) {
                                    if (response.success) {
                                        GarageApp.showSuccess('Phân công kỹ thuật viên thành công');
                                        receptionTable.ajax.reload();
                                    } else {
                                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi phân công kỹ thuật viên');
                                    }
                                },
                                error: function() {
                                    GarageApp.showError('Lỗi khi phân công kỹ thuật viên');
                                }
                            });
                        }
                    });
                } else {
                    GarageApp.showError('Không thể tải thông tin phiếu tiếp đón');
                }
            },
            error: function() {
                GarageApp.showError('Lỗi khi tải thông tin phiếu tiếp đón');
            }
        });
    }

    /**
     * Populate view modal
     */
    function populateViewModal(data) {
        $('#viewReceptionNumber').text(data.receptionNumber || '-');
        $('#viewCustomerName').text(data.customerName || '-');
        $('#viewVehiclePlate').text(data.vehiclePlate || '-');
        $('#viewVehicleInfo').text(`${data.vehicleMake || ''} ${data.vehicleModel || ''} (${data.vehicleYear || ''})`.trim() || '-');
        $('#viewReceptionDate').text(data.receptionDate ? new Date(data.receptionDate).toLocaleString('vi-VN') : '-');
        $('#viewServiceType').text(translateServiceType(data.serviceType) || '-');
        $('#viewPriority').text(translatePriority(data.priority) || '-');
        $('#viewStatus').text(translateReceptionStatus(data.status) || '-');
        $('#viewTechnicianName').text(data.assignedTechnician?.name || '-');
        $('#viewInspectionStartDate').text(data.inspectionStartDate ? new Date(data.inspectionStartDate).toLocaleString('vi-VN') : '-');
        $('#viewInspectionCompletedDate').text(data.inspectionCompletedDate ? new Date(data.inspectionCompletedDate).toLocaleDateString('vi-VN') : '-');
        $('#viewCustomerRequest').text(data.customerRequest || '-');
        $('#viewCustomerComplaints').text(data.customerComplaints || '-');
        $('#viewReceptionNotes').text(data.receptionNotes || '-');

        // Show/hide insurance section
        if (data.insuranceCompany || data.insurancePolicyNumber) {
            $('#viewInsuranceSection').show();
            $('#viewInsuranceCompany').text(data.insuranceCompany || '-');
            $('#viewInsurancePolicy').text(data.insurancePolicyNumber || '-');
            $('#viewInsuranceClaimDate').text('-'); // DTO không có field này
        } else {
            $('#viewInsuranceSection').hide();
        }

        // Show/hide emergency section
        if (data.emergencyContact || data.emergencyContactName) {
            $('#viewEmergencySection').show();
            $('#viewEmergencyContact').text(data.emergencyContact || '-');
            $('#viewEmergencyContactName').text(data.emergencyContactName || '-');
        } else {
            $('#viewEmergencySection').hide();
        }
    }

    /**
     * Populate edit modal
     */
    function populateEditModal(data) {
        $('#editReceptionId').val(data.id);
        $('#editReceptionNumber').val(data.receptionNumber);
        
        // Populate customer dropdown
        $('#editCustomerSelect').val(data.customerId);
        
        // Populate vehicle dropdown - đảm bảo vehicle options đã có
        if (data.vehicleId) {
            // Kiểm tra xem vehicle có trong dropdown không
            const vehicleOption = $(`#editVehicleSelect option[value="${data.vehicleId}"]`);
            if (vehicleOption.length > 0) {
                $('#editVehicleSelect').val(data.vehicleId);
            } else {
                // Nếu không có, thêm option mới từ data
                const vehicleText = data.vehicle ? 
                    `${data.vehicle.licensePlate} - ${data.vehicle.brand} ${data.vehicle.model}` :
                    `${data.vehicleMake} ${data.vehicleModel} - ${data.vehiclePlate}`;
                
                $('#editVehicleSelect').append(`<option value="${data.vehicleId}" selected>${vehicleText}</option>`);
            }
        }
        
        $('#editServiceType').val(data.serviceType);
        $('#editPriority').val(data.priority);
        $('#editTechnicianSelect').val(data.assignedTechnicianId || '');
        $('#editInspectionStartDate').val(data.inspectionStartDate ? (() => {
            const date = new Date(data.inspectionStartDate);
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');
            return `${year}-${month}-${day}T${hours}:${minutes}`;
        })() : '');
        $('#editInspectionCompletedDate').val(data.inspectionCompletedDate ? (() => {
            const date = new Date(data.inspectionCompletedDate);
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            return `${year}-${month}-${day}`;
        })() : '');
        $('#editCustomerRequest').val(data.customerRequest || '');
        $('#editCustomerComplaints').val(data.customerComplaints || '');
        $('#editInsuranceCompany').val(data.insuranceCompany || '');
        $('#editInsurancePolicy').val(data.insurancePolicyNumber || '');
        $('#editInsuranceClaimDate').val(''); // DTO không có field này
        $('#editEmergencyContact').val(data.emergencyContact || '');
        $('#editEmergencyContactName').val(data.emergencyContactName || '');
        $('#editReceptionNotes').val(data.receptionNotes || '');
    }

    /**
     * Show create modal with reset
     */
    function showCreateModal() {
        $('#createReceptionModal').modal('show');
        
        // Reset form when modal is fully shown
        $('#createReceptionModal').on('shown.bs.modal', function() {
            $('#createReceptionForm')[0].reset();
            
            // Reset dropdowns
            $('#createCustomerSelect').val('').trigger('change');
            $('#createVehicleSelect').val('').trigger('change');
            $('#createTechnicianSelect').val('').trigger('change');
            $('#createPriority').val('Normal');
            
            // Set default dates
            const now = new Date();
            const tomorrow = new Date(now);
            tomorrow.setDate(now.getDate() + 1);
            const nextWeek = new Date(now);
            nextWeek.setDate(now.getDate() + 7);
            
            // Set default inspection start date (tomorrow at 9:00 AM)
            const inspectionStartDate = new Date(tomorrow);
            inspectionStartDate.setHours(9, 0, 0, 0);
            const inspectionStartDateStr = (() => {
                const year = inspectionStartDate.getFullYear();
                const month = String(inspectionStartDate.getMonth() + 1).padStart(2, '0');
                const day = String(inspectionStartDate.getDate()).padStart(2, '0');
                const hours = String(inspectionStartDate.getHours()).padStart(2, '0');
                const minutes = String(inspectionStartDate.getMinutes()).padStart(2, '0');
                return `${year}-${month}-${day}T${hours}:${minutes}`;
            })();
            $('#createInspectionStartDate').val(inspectionStartDateStr);
            
            // Set default completion date (next week)
            const completionDateStr = (() => {
                const year = nextWeek.getFullYear();
                const month = String(nextWeek.getMonth() + 1).padStart(2, '0');
                const day = String(nextWeek.getDate()).padStart(2, '0');
                return `${year}-${month}-${day}`;
            })();
            $('#createInspectionCompletedDate').val(completionDateStr);
            
            // Remove the event listener to prevent multiple bindings
            $('#createReceptionModal').off('shown.bs.modal');
        });
    }


    /**
     * Get status class for badge
     */
    function getStatusClass(status) {
        switch (status) {
            case 0: return 'info';    // ReceptionStatus.Pending
            case 1: return 'warning'; // ReceptionStatus.Assigned
            case 2: return 'primary'; // ReceptionStatus.InProgress
            case 3: return 'success'; // ReceptionStatus.Completed
            case 4: return 'danger';  // ReceptionStatus.Cancelled
            default: return 'secondary';
        }
    }

    /**
     * Get priority class for badge
     */
    function getPriorityClass(priority) {
        switch (priority) {
            case 'Thấp': return 'secondary';
            case 'Bình Thường': return 'info';
            case 'Cao': return 'warning';
            case 'Khẩn Cấp': return 'danger';
            default: return 'secondary';
        }
    }

    /**
     * Translate reception status
     */
    function translateReceptionStatus(status) {
        switch (status) {
            case 0: return 'Chờ Kiểm Tra';    // ReceptionStatus.Pending
            case 1: return 'Đã Phân Công';    // ReceptionStatus.Assigned
            case 2: return 'Đang Kiểm Tra';   // ReceptionStatus.InProgress
            case 3: return 'Đã Hoàn Thành';   // ReceptionStatus.Completed
            case 4: return 'Đã Hủy';          // ReceptionStatus.Cancelled
            default: return status;
        }
    }

    /**
     * Translate priority
     */
    function translatePriority(priority) {
        switch (priority) {
            case 'Low': return 'Thấp';
            case 'Normal': return 'Bình Thường';
            case 'High': return 'Cao';
            case 'Urgent': return 'Khẩn Cấp';
            default: return priority;
        }
    }

    /**
     * Translate service type
     */
    function translateServiceType(serviceType) {
        switch (serviceType) {
            case 'Repair': return 'Sửa Chữa';
            case 'Maintenance': return 'Bảo Dưỡng';
            case 'Warranty': return 'Bảo Hành';
            case 'Bodywork': return 'Sửa Chữa Thân Xe';
            case 'Other': return 'Khác';
            default: return serviceType;
        }
    }

    // Public API
    return {
        init: init,
        showCreateModal: showCreateModal,
        viewReception: viewReception,
        editReception: editReception,
        deleteReception: deleteReception,
        assignTechnician: assignTechnician
    };
})();

// Initialize when document is ready
$(document).ready(function() {
    CustomerReceptionManagement.init();
});
