/**
 * API Scope Management - Example using DataTablesUtility
 * 
 * Đây là ví dụ về cách sử dụng DataTablesUtility.initializeTable()
 * cho feature mới - API Scope Management
 */

// Trong Views/ApiScopeManagement/Index.cshtml
$(document).ready(function() {
    // Khởi tạo DataTable sử dụng utility function
    DataTablesUtility.initializeTable('#apiScopesTable', {
        ajaxUrl: '/ApiScopeManagement/GetApiScopes',
        columns: [
            { 
                data: 'name',
                title: 'Scope Name'
            },
            { 
                data: 'displayName',
                title: 'Display Name'
            },
            { 
                data: 'description',
                title: 'Description'
            },
            { 
                data: 'enabled',
                title: 'Status',
                render: function(data) {
                    return data ? 
                        '<span class="badge badge-success">Enabled</span>' : 
                        '<span class="badge badge-danger">Disabled</span>';
                }
            },
            { 
                data: 'required',
                title: 'Required',
                render: function(data) {
                    return data ? 
                        '<span class="badge badge-warning">Yes</span>' : 
                        '<span class="badge badge-secondary">No</span>';
                }
            },
            { 
                data: 'emphasize',
                title: 'Emphasize',
                render: function(data) {
                    return data ? 
                        '<span class="badge badge-info">Yes</span>' : 
                        '<span class="badge badge-light">No</span>';
                }
            },
            { 
                data: 'userClaims',
                title: 'User Claims',
                render: function(data) {
                    if (data && data.length > 0) {
                        return data.map(claim => 
                            `<span class="badge badge-primary mr-1">${claim}</span>`
                        ).join('');
                    }
                    return '<span class="text-muted">None</span>';
                }
            }
        ],
        actionCallbacks: {
            view: 'ApiScopeManagement.showScopeDetails',
            edit: 'ApiScopeManagement.showEditScope',
            delete: 'ApiScopeManagement.deleteScope'
        },
        language: {
            processing: "Loading API Scopes...",
            emptyTable: "No API Scopes found",
            info: "Showing _START_ to _END_ of _TOTAL_ API Scopes",
            infoEmpty: "Showing 0 to 0 of 0 API Scopes",
            infoFiltered: "(filtered from _MAX_ total API Scopes)"
        },
        pageLength: 25
    });
});

/**
 * So sánh với cách cũ (không dùng utility):
 * 
 * CÁCH CŨ (50+ dòng):
 * $('#apiScopesTable').DataTable({
 *     processing: true,
 *     serverSide: false,
 *     ajax: {
 *         url: '/ApiScopeManagement/GetApiScopes',
 *         type: 'GET',
 *         dataSrc: 'data'
 *     },
 *     columns: [
 *         // ... 7 cột như trên
 *         {
 *             data: null,
 *             orderable: false,
 *             render: function(data, type, row) {
 *                 return `
 *                     <div class="action-buttons">
 *                         <button class="btn btn-info btn-sm" onclick="ApiScopeManagement.showScopeDetails('${row.name}')" title="View">
 *                             <i class="fas fa-eye"></i>
 *                         </button>
 *                         <button class="btn btn-warning btn-sm" onclick="ApiScopeManagement.showEditScope('${row.name}')" title="Edit">
 *                             <i class="fas fa-edit"></i>
 *                         </button>
 *                         <button class="btn btn-danger btn-sm" onclick="ApiScopeManagement.deleteScope(${row.id})" title="Delete">
 *                             <i class="fas fa-trash"></i>
 *                         </button>
 *                     </div>
 *                 `;
 *             }
 *         }
 *     ],
 *     responsive: true,
 *     autoWidth: false,
 *     scrollX: true,
 *     scrollCollapse: true,
 *     pageLength: 25,
 *     lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
 *     language: {
 *         "processing": "Loading API Scopes...",
 *         "lengthMenu": "Show _MENU_ entries",
 *         "zeroRecords": "No matching records found",
 *         "info": "Showing _START_ to _END_ of _TOTAL_ entries",
 *         "infoEmpty": "Showing 0 to 0 of 0 entries",
 *         "infoFiltered": "(filtered from _MAX_ total entries)",
 *         "loadingRecords": "Loading...",
 *         "paginate": {
 *             "first": "First",
 *             "last": "Last",
 *             "next": "Next",
 *             "previous": "Previous"
 *         },
 *         "search": "Search:",
 *         "emptyTable": "No API Scopes found"
 *     },
 *     dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
 *          '<"row"<"col-sm-12"tr>>' +
 *          '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
 *     drawCallback: function(settings) {
 *         $('.loading-indicator').hide();
 *     }
 * });
 * 
 * CÁCH MỚI (15 dòng):
 * DataTablesUtility.initializeTable('#apiScopesTable', {
 *     ajaxUrl: '/ApiScopeManagement/GetApiScopes',
 *     columns: [...],
 *     actionCallbacks: {
 *         view: 'ApiScopeManagement.showScopeDetails',
 *         edit: 'ApiScopeManagement.showEditScope',
 *         delete: 'ApiScopeManagement.deleteScope'
 *     },
 *     language: { ... },
 *     pageLength: 25
 * });
 * 
 * KẾT QUẢ:
 * - Từ 50+ dòng xuống còn 15 dòng
 * - Dễ đọc hơn
 * - Nhất quán với các features khác
 * - Dễ maintain và update
 */

/**
 * Ví dụ khác - User Management với ít cột hơn:
 */
/*
DataTablesUtility.initializeTable('#usersTable', {
    ajaxUrl: '/UserManagement/GetUsers',
    columns: [
        { data: 'userName' },
        { data: 'email' },
        { 
            data: 'isActive',
            render: function(data) {
                return data ? 
                    '<span class="badge badge-success">Active</span>' : 
                    '<span class="badge badge-danger">Inactive</span>';
            }
        },
        { 
            data: 'roles',
            render: function(data) {
                return data && data.length > 0 ? 
                    data.join(', ') : 
                    '<span class="text-muted">No roles</span>';
            }
        }
    ],
    actionCallbacks: {
        view: 'UserManagement.showUserDetails',
        edit: 'UserManagement.showEditUser'
        // Không có delete
    },
    language: {
        processing: "Loading users...",
        emptyTable: "No users found"
    }
});
*/

/**
 * Ví dụ cho Read-Only Table (không có actions):
 */
/*
DataTablesUtility.initializeTable('#reportsTable', {
    ajaxUrl: '/Reports/GetData',
    columns: [
        { data: 'reportName' },
        { data: 'generatedDate' },
        { data: 'recordCount' }
    ],
    showActions: false, // Tắt action column
    language: {
        processing: "Loading reports...",
        emptyTable: "No reports available"
    }
});
*/
