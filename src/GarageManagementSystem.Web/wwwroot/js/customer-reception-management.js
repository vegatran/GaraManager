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
        // Sử dụng DataTablesUtility với style chung
        var columns = [
            { data: 'id', visible: false },
            { data: 'receptionNumber' },
            { data: 'customerName' },
            { data: 'vehiclePlate' },
            { data: 'vehicleInfo' },
            { 
                data: 'receptionDate',
                render: DataTablesUtility.renderDate
            },
            { data: 'technicianName' },
            { 
                data: 'status',
                render: function(data, type, row) {
                    const statusClass = getStatusClass(data);
                    const statusText = translateReceptionStatus(data);
                    return `<span class="badge badge-${statusClass}">${statusText}</span>`;
                }
            },
            { 
                data: 'priority',
                render: function(data, type, row) {
                    const priorityClass = getPriorityClass(data);
                    return `<span class="badge badge-${priorityClass}">${data}</span>`;
                }
            },
            { data: 'serviceType' },
            {
                data: null,
                orderable: false,
                render: function(data, type, row) {
                    let actions = `
                        <button class="btn btn-info btn-sm" onclick="CustomerReceptionManagement.viewReception(${row.id})" title="Xem">
                            <i class="fas fa-eye"></i>
                        </button>
                    `;

                    // Chỉ cho phép sửa nếu chưa hoàn thành
                    if (row.status !== 3 && row.status !== 4) { // ReceptionStatus.Completed && ReceptionStatus.Cancelled
                        actions += `
                            <button class="btn btn-warning btn-sm" onclick="CustomerReceptionManagement.editReception(${row.id})" title="Sửa">
                                <i class="fas fa-edit"></i>
                            </button>
                        `;
                    }

                    // Chỉ cho phép xóa nếu chưa hoàn thành
                    if (row.status !== 3 && row.status !== 4) { // ReceptionStatus.Completed && ReceptionStatus.Cancelled
                        actions += `
                            <button class="btn btn-danger btn-sm" onclick="CustomerReceptionManagement.deleteReception(${row.id})" title="Xóa">
                                <i class="fas fa-trash"></i>
                            </button>
                        `;
                    }

                    // Nút phân công kỹ thuật viên - chỉ hiển thị khi chưa có kỹ thuật viên
                    if (row.status === 0 || (row.status === 1 && (!row.technicianName || row.technicianName === 'Chưa phân công'))) {
                        actions += `
                            <button class="btn btn-success btn-sm" onclick="CustomerReceptionManagement.assignTechnician(${row.id})" title="Phân Công">
                                <i class="fas fa-user-plus"></i>
                            </button>
                        `;
                    }

                    return actions;
                }
            }
        ];
        
        receptionTable = DataTablesUtility.initAjaxTable('#receptionTable', '/CustomerReception/GetReceptions', columns, {
            dataSrc: function(json) {
                if (json.success && json.data) {
                    return json.data;
                } else {
                    GarageApp.showError(json.error || 'Lỗi khi tải dữ liệu');
                    return [];
                }
            },
            dom: 'Brtip',  // Ẩn search input (f) và length menu (l), chỉ hiển thị buttons, table, paging, info
            buttons: [
                'copy', 'csv', 'excel', 'pdf', 'print'
            ],
            responsive: true,
            pageLength: 25,
            order: [[1, 'desc']] // Sort by reception number descending
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
        // Search functionality
        $('#searchInput').on('keyup', function() {
            receptionTable.search(this.value).draw();
        });

        // Create form submission
        $('#createReceptionForm').on('submit', function(e) {
            e.preventDefault();
            createReception();
        });

        // Edit form submission
        $('#editReceptionForm').on('submit', function(e) {
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
            inspectionStartDate: $('#createInspectionStartDate').val() ? new Date($('#createInspectionStartDate').val()) : null,
            inspectionCompletedDate: $('#createInspectionCompletedDate').val() ? new Date($('#createInspectionCompletedDate').val()) : null,
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
                    clearCreateForm();
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
        $.ajax({
            url: `/CustomerReception/GetReception/${id}`,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    populateEditModal(response.data);
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
            inspectionStartDate: $('#editInspectionStartDate').val() ? new Date($('#editInspectionStartDate').val()) : null,
            inspectionCompletedDate: $('#editInspectionCompletedDate').val() ? new Date($('#editInspectionCompletedDate').val()) : null,
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
        
        // ✅ Chỉ set giá trị hiển thị cho customer và vehicle (đã disable)
        // KHÔNG trigger change vì field đã disabled
        $('#editCustomerSelect').val(data.customerId);
        $('#editVehicleSelect').val(data.vehicleId);
        
        $('#editServiceType').val(data.serviceType);
        $('#editPriority').val(data.priority);
        $('#editTechnicianSelect').val(data.assignedTechnicianId || '');
        $('#editInspectionStartDate').val(data.inspectionStartDate ? new Date(data.inspectionStartDate).toISOString().slice(0, 16) : '');
        $('#editInspectionCompletedDate').val(data.inspectionCompletedDate ? new Date(data.inspectionCompletedDate).toISOString().slice(0, 10) : '');
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
     * Clear create form
     */
    function clearCreateForm() {
        $('#createReceptionForm')[0].reset();
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
