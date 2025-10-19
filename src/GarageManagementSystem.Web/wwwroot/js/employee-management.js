/**
 * Employee Management Module
 * 
 * Handles all employee-related operations
 * CRUD operations for employees
 */

window.EmployeeManagement = {
    // DataTable instance
    employeeTable: null,

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
        this.loadDropdownData();
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        // Sử dụng DataTablesUtility với style chung
        var columns = [
            { data: 'id', title: 'ID', width: '60px' },
            { data: 'name', title: 'Tên Nhân Viên' },
            { data: 'position', title: 'Chức Vụ' },
            { data: 'department', title: 'Bộ Phận' },
            { data: 'phone', title: 'Số Điện Thoại' },
            { data: 'email', title: 'Email' },
            { 
                data: 'hireDate', 
                title: 'Ngày Tuyển',
                render: DataTablesUtility.renderDate
            },
            { 
                data: 'status', 
                title: 'Trạng Thái',
                render: DataTablesUtility.renderStatus
            },
            {
                data: null,
                title: 'Thao Tác',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    return `
                        <button class="btn btn-info btn-sm view-employee" data-id="${row.id}">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-warning btn-sm edit-employee" data-id="${row.id}">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn btn-danger btn-sm delete-employee" data-id="${row.id}">
                            <i class="fas fa-trash"></i>
                        </button>
                    `;
                }
            }
        ];
        
        this.employeeTable = DataTablesUtility.initServerSideTable('#employeeTable', '/EmployeeManagement/GetEmployees', columns, {
            order: [[0, 'desc']],
            pageLength: 10
        });
    },

    // Bind events
    bindEvents: function() {
        var self = this;


        // Add employee button
        $(document).on('click', '#addEmployeeBtn', function() {
            self.showCreateModal();
        });

        // View employee
        $(document).on('click', '.view-employee', function() {
            var id = $(this).data('id');
            self.viewEmployee(id);
        });

        // Edit employee
        $(document).on('click', '.edit-employee', function() {
            var id = $(this).data('id');
            self.editEmployee(id);
        });

        // Delete employee
        $(document).on('click', '.delete-employee', function() {
            var id = $(this).data('id');
            self.deleteEmployee(id);
        });

        // Create employee form
        $(document).on('submit', '#createEmployeeForm', function(e) {
            e.preventDefault();
            self.createEmployee();
        });

        // Update employee form
        $(document).on('submit', '#editEmployeeForm', function(e) {
            e.preventDefault();
            self.updateEmployee();
        });
    },

    // Load dropdown data for positions and departments
    loadDropdownData: function() {
        this.loadPositions();
        this.loadDepartments();
    },

    // Load positions for dropdowns
    loadPositions: function() {
        var self = this;
        
        $.ajax({
            url: '/EmployeeManagement/GetAvailablePositions',
            type: 'GET',
            success: function(response) {
                
                // Clear existing options
                $('#editPosition, #createPosition').empty();
                $('#editPosition, #createPosition').append('<option value="">-- Chọn Chức Vụ --</option>');
                
                if (response && response.length > 0) {
                    response.forEach(function(position) {
                        var value = position.value || position.name || position.text;
                        var text = position.text || position.name || position.value;
                        $('#editPosition, #createPosition').append(
                            '<option value="' + value + '">' + text + '</option>'
                        );
                    });
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                }
            }
        });
    },

    // Load departments for dropdowns
    loadDepartments: function() {
        var self = this;
        
        $.ajax({
            url: '/EmployeeManagement/GetAvailableDepartments',
            type: 'GET',
            success: function(response) {
                
                // Clear existing options
                $('#editDepartment, #createDepartment').empty();
                $('#editDepartment, #createDepartment').append('<option value="">-- Chọn Bộ Phận --</option>');
                
                if (response && response.length > 0) {
                    response.forEach(function(department) {
                        var value = department.value || department.name || department.text;
                        var text = department.text || department.name || department.value;
                        $('#editDepartment, #createDepartment').append(
                            '<option value="' + value + '">' + text + '</option>'
                        );
                    });
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                }
            }
        });
    },

    // Show create modal
    showCreateModal: function() {
        $('#createEmployeeModal').modal('show');
        
        // Reset form when modal is fully shown
        $('#createEmployeeModal').on('shown.bs.modal', function() {
            // Reset form fields
            $('#createEmployeeForm')[0].reset();
            
            // Set default hire date to today
            var today = new Date();
            var todayString = today.getFullYear() + '-' + 
                             String(today.getMonth() + 1).padStart(2, '0') + '-' + 
                             String(today.getDate()).padStart(2, '0');
            $('#createHireDate').val(todayString);
            
            // Reset dropdowns to default
            $('#createPosition').val('').trigger('change');
            $('#createDepartment').val('').trigger('change');
            $('#createStatus').val('Active');
            
            // Remove the event listener to prevent multiple bindings
            $('#createEmployeeModal').off('shown.bs.modal');
        });
    },

    // View employee
    viewEmployee: function(id) {
        var self = this;
        
        $.ajax({
            url: '/EmployeeManagement/GetEmployee/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        self.populateViewModal(response.data);
                        $('#viewEmployeeModal').modal('show');
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error loading employee');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error loading employee');
                }
            }
        });
    },

    // Edit employee
    editEmployee: function(id) {
        var self = this;
        
        $.ajax({
            url: '/EmployeeManagement/GetEmployee/' + id,
            type: 'GET',
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        
                        // Store employee data and show modal
                        var employeeData = response.data;
                        $('#editEmployeeModal').modal('show');
                        
                        // Populate form when modal is fully shown
                        $('#editEmployeeModal').on('shown.bs.modal', function() {
                            // Reset form first
                            $('#editEmployeeForm')[0].reset();
                            self.populateEditModal(employeeData);
                            // Remove the event listener to prevent multiple bindings
                            $('#editEmployeeModal').off('shown.bs.modal');
                        });
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error loading employee');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error loading employee');
                }
            }
        });
    },

    // Delete employee
    deleteEmployee: function(id) {
        var self = this;
        
        Swal.fire({
            title: 'Are you sure?',
            text: "You won't be able to revert this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/EmployeeManagement/DeleteEmployee/' + id,
                    type: 'DELETE',
                    success: function(response) {
                        if (AuthHandler.validateApiResponse(response)) {
                            if (response.success) {
                                GarageApp.showSuccess('Employee deleted successfully!');
                                self.employeeTable.ajax.reload();
                            } else {
                                GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error deleting employee');
                            }
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Error deleting employee');
                        }
                    }
                });
            }
        });
    },

    // Create employee
    createEmployee: function() {
        var self = this;
        var formData = {
            Name: $('#createName').val(),
            Email: $('#createEmail').val(),
            Phone: $('#createPhone').val(),
            Address: $('#createAddress').val(),
            PositionId: parseInt($('#createPosition').val()) || null,
            DepartmentId: parseInt($('#createDepartment').val()) || null,
            HireDate: $('#createHireDate').val() || null,
            Salary: parseFloat($('#createSalary').val()) || null,
            Status: $('#createStatus').val() || 'Active',
            Skills: $('#createSkills').val()
        };

        // Debug: Log form data

        $.ajax({
            url: '/EmployeeManagement/CreateEmployee',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Employee created successfully!');
                        $('#createEmployeeModal').modal('hide');
                        self.employeeTable.ajax.reload();
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error creating employee');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error creating employee');
                }
            }
        });
    },

    // Update employee
    updateEmployee: function() {
        var self = this;
        var employeeId = $('#editId').val();
        var formData = {
            Id: parseInt(employeeId),
            Name: $('#editName').val(),
            Email: $('#editEmail').val(),
            Phone: $('#editPhone').val(),
            Address: $('#editAddress').val(),
            PositionId: parseInt($('#editPosition').val()) || null,
            DepartmentId: parseInt($('#editDepartment').val()) || null,
            HireDate: $('#editHireDate').val() != "" ? $('#editHireDate').val() : null,
            Salary: parseFloat($('#editSalary').val()) || null,
            Status: $('#editStatus').val(),
            Skills: $('#editSkills').val()
        };

        $.ajax({
            url: '/EmployeeManagement/UpdateEmployee/' + employeeId,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function(response) {
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        GarageApp.showSuccess('Employee updated successfully!');
                        $('#editEmployeeModal').modal('hide');
                        self.employeeTable.ajax.reload();
                    } else {
                        GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Error updating employee');
                    }
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Error updating employee');
                }
            }
        });
    },

    // Populate view modal
    populateViewModal: function(employee) {
        $('#viewName').text(employee.name || '');
        $('#viewEmail').text(employee.email || '');
        $('#viewPhone').text(employee.phone || '');
        $('#viewAddress').text(employee.address || '');
        $('#viewPosition').text(employee.positionName || employee.position || '');
        $('#viewDepartment').text(employee.departmentName || employee.department || '');
        $('#viewHireDate').text(employee.hireDate || '');
        $('#viewSalary').text(employee.salary || '');
        $('#viewStatus').text(employee.status || '');
        $('#viewSkills').text(employee.skills || '');
    },

    // Populate edit modal
    populateEditModal: function(employee) {
        
        // Reset form first
        $('#editEmployeeForm')[0].reset();
        
        $('#editId').val(employee.id);
        $('#editName').val(employee.name);
        $('#editEmail').val(employee.email);
        $('#editPhone').val(employee.phone);
        $('#editAddress').val(employee.address);
        
        // Use positionId/departmentId for dropdown values, fallback to names
        $('#editPosition').val(employee.positionId || employee.position).trigger('change');
        $('#editDepartment').val(employee.departmentId || employee.department).trigger('change');
        
        // Format hire date for HTML input type="date"
        if (employee.hireDate) {
            // If hireDate is already in YYYY-MM-DD format, use it directly
            if (employee.hireDate.match(/^\d{4}-\d{2}-\d{2}$/)) {
                $('#editHireDate').val(employee.hireDate);
            } else {
                // Convert from other formats to YYYY-MM-DD
                var date = new Date(employee.hireDate);
                var formattedDate = date.getFullYear() + '-' + 
                                   String(date.getMonth() + 1).padStart(2, '0') + '-' + 
                                   String(date.getDate()).padStart(2, '0');
                $('#editHireDate').val(formattedDate);
            }
        }
        
        $('#editSalary').val(employee.salary);
        $('#editStatus').val(employee.status || 'Active');
        $('#editSkills').val(employee.skills);
    }
};

// Initialize when document is ready
$(document).ready(function() {
    EmployeeManagement.init();
});