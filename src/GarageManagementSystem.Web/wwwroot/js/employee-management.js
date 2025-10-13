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
        
        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable('#employeeTable')) {
            $('#employeeTable').DataTable().destroy();
        }
        
        this.employeeTable = DataTablesVietnamese.init('#employeeTable', {
            processing: true,
            serverSide: false,
            ajax: {
                url: '/EmployeeManagement/GetEmployees',
                type: 'GET',
                error: function(xhr, status, error) {
                    if (AuthHandler.isUnauthorized(xhr)) {
                        AuthHandler.handleUnauthorized(xhr, true);
                    } else {
                        GarageApp.showError('Error loading employees');
                    }
                }
            },
            columns: [
                { data: 'id', title: 'ID', width: '60px' },
                { data: 'name', title: 'Tên Nhân Viên' },
                { data: 'position', title: 'Chức Vụ' },
                { data: 'department', title: 'Bộ Phận' },
                { data: 'phone', title: 'Số Điện Thoại' },
                { data: 'email', title: 'Email' },
                { data: 'hireDate', title: 'Ngày Tuyển' },
                { data: 'status', title: 'Trạng Thái' },
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
            ],
            order: [[0, 'desc']],
            pageLength: 10,
            responsive: true
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
                console.log('🔍 Positions API Response:', response);
                
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
                console.error('Error loading positions:', error);
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
                console.log('🔍 Departments API Response:', response);
                
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
                console.error('Error loading departments:', error);
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                }
            }
        });
    },

    // Show create modal
    showCreateModal: function() {
        $('#createEmployeeModal').modal('show');
        $('#createEmployeeForm')[0].reset();
    },

    // View employee
    viewEmployee: function(id) {
        var self = this;
        
        $.ajax({
            url: '/EmployeeManagement/GetEmployee/' + id,
            type: 'GET',
            success: function(response) {
                console.log('🔍 View Employee API Response:', response);
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        console.log('🔍 Employee Data:', response.data);
                        self.populateViewModal(response.data);
                        $('#viewEmployeeModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Error loading employee');
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
                console.log('🔍 Edit Employee API Response:', response);
                if (AuthHandler.validateApiResponse(response)) {
                    if (response.success) {
                        console.log('🔍 Employee Data:', response.data);
                        self.populateEditModal(response.data);
                        $('#editEmployeeModal').modal('show');
                    } else {
                        GarageApp.showError(response.message || 'Error loading employee');
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
                                GarageApp.showError(response.message || 'Error deleting employee');
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
            Position: $('#createPosition').val(),
            Department: $('#createDepartment').val(),
            HireDate: $('#createHireDate').val(),
            Salary: parseFloat($('#createSalary').val()) || 0,
            Status: $('#createStatus').val(),
            Skills: $('#createSkills').val()
        };

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
                        GarageApp.showError(response.message || 'Error creating employee');
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
            PositionId: $('#editPosition').val(),
            DepartmentId: $('#editDepartment').val(),
            HireDate: $('#editHireDate').val() != "" ? $('#editHireDate').val() : null,
            Salary: parseFloat($('#editSalary').val()) || 0,
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
                        GarageApp.showError(response.message || 'Error updating employee');
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
        console.log('🔍 Employee data for edit:', employee);
        
        $('#editId').val(employee.id);
        $('#editName').val(employee.name);
        $('#editEmail').val(employee.email);
        $('#editPhone').val(employee.phone);
        $('#editAddress').val(employee.address);
        
        // Use positionId/departmentId for dropdown values, fallback to names
        $('#editPosition').val(employee.positionId || employee.position);
        $('#editDepartment').val(employee.departmentId || employee.department);
        
        $('#editHireDate').val(employee.hireDate);
        $('#editSalary').val(employee.salary);
        $('#editStatus').val(employee.status);
        $('#editSkills').val(employee.skills);
    }
};

// Initialize when document is ready
$(document).ready(function() {
    EmployeeManagement.init();
});