/**
 * Print Template Management Module
 * 
 * Handles all print template-related operations
 * CRUD operations for print templates
 */

window.PrintTemplateManagement = {
    // DataTable instance
    templateTable: null,

    // Initialize module
    init: function() {
        this.initDataTable();
        this.bindEvents();
    },

    // Initialize DataTable
    initDataTable: function() {
        var self = this;
        
        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable('#templateTable')) {
            $('#templateTable').DataTable().destroy();
        }
        
        this.templateTable = DataTablesVietnamese.init('#templateTable', {
            processing: true,
            serverSide: false,
            ajax: {
                url: '/PrintTemplateManagement/GetTemplates',
                type: 'GET',
                dataSrc: function(json) {
                    if (json.success && json.data) {
                        return json.data;
                    } else {
                        GarageApp.showError(json.message || 'Error loading templates');
                        return [];
                    }
                },
                error: function(xhr, status, error) {
                    if (AuthHandler.isUnauthorized(xhr)) {
                        AuthHandler.handleUnauthorized(xhr, true);
                    } else {
                        GarageApp.showError('Error loading templates');
                    }
                }
            },
            columns: [
                { data: 'id', title: 'ID', width: '60px' },
                { 
                    data: 'templateName', 
                    title: 'Tên Mẫu',
                    render: function(data, type, row) {
                        var html = '<strong>' + data + '</strong>';
                        if (row.notes) {
                            html += '<br><small class="text-muted">' + row.notes + '</small>';
                        }
                        return html;
                    }
                },
                { 
                    data: 'templateType', 
                    title: 'Loại',
                    render: function(data) {
                        return '<span class="badge badge-info">' + data + '</span>';
                    }
                },
                { 
                    data: 'description', 
                    title: 'Mô Tả',
                    render: function(data) {
                        return data || 'Không có mô tả';
                    }
                },
                { 
                    data: 'isActive', 
                    title: 'Trạng Thái',
                    render: function(data) {
                        if (data) {
                            return '<span class="badge badge-success">Hoạt Động</span>';
                        } else {
                            return '<span class="badge badge-secondary">Tạm Dừng</span>';
                        }
                    }
                },
                { 
                    data: 'isDefault', 
                    title: 'Mặc Định',
                    render: function(data, type, row) {
                        if (data) {
                            return '<span class="badge badge-warning"><i class="fas fa-star"></i> Mặc Định</span>';
                        } else {
                            return '<button type="button" class="btn btn-outline-warning btn-sm" onclick="PrintTemplateManagement.setAsDefault(' + row.id + ', \'' + row.templateType + '\')"><i class="fas fa-star"></i> Đặt Mặc Định</button>';
                        }
                    }
                },
                { 
                    data: 'createdAt', 
                    title: 'Ngày Tạo',
                    render: function(data) {
                        return new Date(data).toLocaleDateString('vi-VN') + ' ' + new Date(data).toLocaleTimeString('vi-VN', {hour: '2-digit', minute: '2-digit'});
                    }
                },
                { 
                    data: null, 
                    title: 'Thao Tác',
                    orderable: false,
                    render: function(data, type, row) {
                        var buttons = '<div class="btn-group" role="group">';
                        buttons += '<button type="button" class="btn btn-info btn-sm" onclick="PrintTemplateManagement.viewTemplate(' + row.id + ')" title="Xem"><i class="fas fa-eye"></i></button>';
                        buttons += '<button type="button" class="btn btn-warning btn-sm" onclick="PrintTemplateManagement.editTemplate(' + row.id + ')" title="Sửa"><i class="fas fa-edit"></i></button>';
                        
                        if (!row.isDefault) {
                            buttons += '<button type="button" class="btn btn-danger btn-sm" onclick="PrintTemplateManagement.deleteTemplate(' + row.id + ')" title="Xóa"><i class="fas fa-trash"></i></button>';
                        }
                        
                        buttons += '</div>';
                        return buttons;
                    }
                }
            ]
        });
    },

    // View template details
    viewTemplate: function(templateId) {
        var self = this;
        
        $.ajax({
            url: '/PrintTemplateManagement/View/' + templateId,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    self.populateViewModal(response.data);
                    $('#viewTemplateModal').modal('show');
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Không thể tải thông tin mẫu');
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin mẫu: ' + error);
                }
            }
        });
    },

    // Populate view modal
    populateViewModal: function(template) {
        $('#viewTemplateName').text(template.templateName || '');
        $('#viewTemplateType').text(template.templateType || '');
        $('#viewDescription').text(template.description || '');
        $('#viewCreatedAt').text(new Date(template.createdAt).toLocaleDateString('vi-VN'));
        
        // Status badges
        $('#viewTemplateStatus').html(template.isActive ? 
            '<span class="badge badge-success">Hoạt Động</span>' : 
            '<span class="badge badge-secondary">Tạm Dừng</span>');
        
        $('#viewTemplateDefault').html(template.isDefault ? 
            '<span class="badge badge-warning"><i class="fas fa-star"></i> Mặc Định</span>' : 
            '');
        
        // Company info
        var companyInfo = '';
        if (template.companyInfo) {
            try {
                var company = JSON.parse(template.companyInfo);
                companyInfo = `
                    <p><strong>Tên:</strong> ${company.companyName || ''}</p>
                    <p><strong>Địa chỉ:</strong> ${company.companyAddress || ''}</p>
                    <p><strong>Điện thoại:</strong> ${company.companyPhone || ''}</p>
                    <p><strong>Email:</strong> ${company.companyEmail || ''}</p>
                    <p><strong>Website:</strong> ${company.companyWebsite || ''}</p>
                    <p><strong>Mã số thuế:</strong> ${company.taxCode || ''}</p>
                `;
            } catch (e) {
                companyInfo = '<p class="text-muted">Không có thông tin công ty</p>';
            }
        } else {
            companyInfo = '<p class="text-muted">Không có thông tin công ty</p>';
        }
        $('#viewCompanyInfo').html(companyInfo);
        
        // Preview
        var preview = '';
        if (template.headerHtml) {
            preview += template.headerHtml;
        }
        preview += '<div class="p-3"><h5>Nội dung mẫu sẽ hiển thị ở đây...</h5></div>';
        if (template.footerHtml) {
            preview += template.footerHtml;
        }
        $('#viewPreview').html(preview);
    },

    // Edit template
    editTemplate: function(templateId) {
        var self = this;
        
        $.ajax({
            url: '/PrintTemplateManagement/View/' + templateId,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    self.populateEditModal(response.data);
                    $('#editTemplateModal').modal('show');
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Không thể tải thông tin mẫu');
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tải thông tin mẫu: ' + error);
                }
            }
        });
    },

    // Populate edit modal
    populateEditModal: function(template) {
        $('#editTemplateId').val(template.id);
        $('#editTemplateName').val(template.templateName || '');
        $('#editTemplateType').val(template.templateType || '');
        $('#editDescription').val(template.description || '');
        $('#editIsActive').prop('checked', template.isActive || false);
        $('#editIsDefault').prop('checked', template.isDefault || false);
        $('#editHeaderHtml').val(template.headerHtml || '');
        $('#editFooterHtml').val(template.footerHtml || '');
        $('#editCustomCss').val(template.customCss || '');
        
        // Company info
        if (template.companyInfo) {
            try {
                var company = JSON.parse(template.companyInfo);
                $('#editCompanyName').val(company.companyName || '');
                $('#editCompanyAddress').val(company.companyAddress || '');
                $('#editCompanyPhone').val(company.companyPhone || '');
                $('#editCompanyEmail').val(company.companyEmail || '');
                $('#editCompanyWebsite').val(company.companyWebsite || '');
                $('#editTaxCode').val(company.taxCode || '');
            } catch (e) {
                // Ignore parsing errors
            }
        }
    },

    // Update template
    updateTemplate: function() {
        var self = this;
        var formData = {
            Id: $('#editTemplateId').val(),
            TemplateName: $('#editTemplateName').val(),
            TemplateType: $('#editTemplateType').val(),
            Description: $('#editDescription').val(),
            IsActive: $('#editIsActive').is(':checked'),
            IsDefault: $('#editIsDefault').is(':checked'),
            HeaderHtml: $('#editHeaderHtml').val(),
            FooterHtml: $('#editFooterHtml').val(),
            CustomCss: $('#editCustomCss').val(),
            CompanyInfo: JSON.stringify({
                companyName: $('#editCompanyName').val(),
                companyAddress: $('#editCompanyAddress').val(),
                companyPhone: $('#editCompanyPhone').val(),
                companyEmail: $('#editCompanyEmail').val(),
                companyWebsite: $('#editCompanyWebsite').val(),
                taxCode: $('#editTaxCode').val()
            })
        };

        $.ajax({
            url: '/PrintTemplateManagement/Update/' + formData.Id,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess(GarageApp.parseErrorMessage(response) || 'Cập nhật mẫu thành công!');
                    $('#editTemplateModal').modal('hide');
                    self.templateTable.ajax.reload();
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi cập nhật mẫu');
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi cập nhật mẫu: ' + error);
                }
            }
        });
    },

    // Create new template
    createTemplate: function() {
        var self = this;
        var formData = {
            TemplateName: $('#createTemplateName').val(),
            TemplateType: $('#createTemplateType').val(),
            Description: $('#createDescription').val(),
            IsActive: $('#createIsActive').is(':checked'),
            IsDefault: $('#createIsDefault').is(':checked'),
            HeaderHtml: $('#createHeaderHtml').val(),
            FooterHtml: $('#createFooterHtml').val(),
            CustomCss: $('#createCustomCss').val(),
            CompanyInfo: JSON.stringify({
                companyName: $('#createCompanyName').val(),
                companyAddress: $('#createCompanyAddress').val(),
                companyPhone: $('#createCompanyPhone').val(),
                companyEmail: $('#createCompanyEmail').val(),
                companyWebsite: $('#createCompanyWebsite').val(),
                taxCode: $('#createTaxCode').val()
            })
        };

        $.ajax({
            url: '/PrintTemplateManagement/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    GarageApp.showSuccess(GarageApp.parseErrorMessage(response) || 'Tạo mẫu thành công!');
                    $('#createTemplateModal').modal('hide');
                    self.templateTable.ajax.reload();
                    self.clearCreateForm();
                } else {
                    GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi tạo mẫu');
                }
            },
            error: function(xhr, status, error) {
                if (AuthHandler.isUnauthorized(xhr)) {
                    AuthHandler.handleUnauthorized(xhr, true);
                } else {
                    GarageApp.showError('Lỗi khi tạo mẫu: ' + error);
                }
            }
        });
    },

    // Clear create form
    clearCreateForm: function() {
        $('#createTemplateForm')[0].reset();
        $('#createIsActive').prop('checked', true);
        $('#createIsDefault').prop('checked', false);
    },

    // Bind events
    bindEvents: function() {
        var self = this;
        
        // Bind create default quotation button
        $('#createDefaultQuotationBtn').on('click', function() {
            self.createDefaultQuotation();
        });

        // Create template form submit
        $('#createTemplateForm').on('submit', function(e) {
            e.preventDefault();
            self.createTemplate();
        });

        // Edit template form submit
        $('#editTemplateForm').on('submit', function(e) {
            e.preventDefault();
            self.updateTemplate();
        });
    },

    // Create default quotation template
    createDefaultQuotation: function() {
        var self = this;
        
        Swal.fire({
            title: 'Xác nhận tạo mẫu mặc định?',
            text: "Bạn có chắc chắn muốn tạo mẫu báo giá mặc định không?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Có, tạo ngay!',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/PrintTemplateManagement/CreateDefaultQuotation',
                    type: 'POST',
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function(response) {
                        if (response.success) {
                            GarageApp.showSuccess(GarageApp.parseErrorMessage(response) || 'Tạo mẫu mặc định thành công!');
                            self.templateTable.ajax.reload();
                        } else {
                            GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi tạo mẫu mặc định');
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi tạo mẫu mặc định: ' + error);
                        }
                    }
                });
            }
        });
    },

    // Set template as default
    setAsDefault: function(templateId, templateType) {
        var self = this;
        
        Swal.fire({
            title: 'Xác nhận đặt làm mặc định?',
            text: `Bạn có chắc chắn muốn đặt mẫu in ID ${templateId} làm mặc định cho loại "${templateType}" không?`,
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Có, đặt ngay!',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/PrintTemplateManagement/SetAsDefault/' + templateId,
                    type: 'POST',
                    data: { templateType: templateType },
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function(response) {
                        if (response.success) {
                            GarageApp.showSuccess(GarageApp.parseErrorMessage(response) || 'Đặt mẫu làm mặc định thành công!');
                            self.templateTable.ajax.reload();
                        } else {
                            GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi đặt mặc định');
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi đặt mặc định: ' + error);
                        }
                    }
                });
            }
        });
    },

    // Delete template
    deleteTemplate: function(templateId) {
        var self = this;
        
        Swal.fire({
            title: 'Xác nhận xóa mẫu in?',
            text: "Bạn có chắc chắn muốn xóa mẫu in này không? Hành động này không thể hoàn tác!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Có, xóa ngay!',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/PrintTemplateManagement/Delete/' + templateId,
                    type: 'POST',
                    headers: {
                        'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function(response) {
                        if (response.success) {
                            GarageApp.showSuccess(GarageApp.parseErrorMessage(response) || 'Xóa mẫu thành công!');
                            self.templateTable.ajax.reload();
                        } else {
                            GarageApp.showError(GarageApp.parseErrorMessage(response) || 'Lỗi khi xóa mẫu');
                        }
                    },
                    error: function(xhr, status, error) {
                        if (AuthHandler.isUnauthorized(xhr)) {
                            AuthHandler.handleUnauthorized(xhr, true);
                        } else {
                            GarageApp.showError('Lỗi khi xóa mẫu: ' + error);
                        }
                    }
                });
            }
        });
    },

};

// Initialize when document is ready
$(document).ready(function() {
    PrintTemplateManagement.init();
});