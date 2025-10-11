# JavaScript Module Pattern - Garage Management System

## Overview

This project uses a **Module Pattern** for organizing JavaScript code. This pattern prevents naming conflicts, improves code organization, and makes the application more maintainable.

## Architecture

### Utility Files (Load First)
These files provide common functionality used across all modules:

1. **site.js** - Core utilities and notification system
   - `GarageApp` namespace with showSuccess, showError, showWarning, showInfo, confirm
   - Format utilities (currency, date, datetime)

2. **ajax-utility.js** - AJAX request handling
   - `AjaxUtility` namespace
   - Standardized GET, POST, PUT, DELETE methods
   - Error handling and CSRF token management
   - Form submission and delete confirmation helpers

3. **datatables-utility.js** - DataTables configuration
   - `DataTablesUtility` namespace
   - Default configurations for consistent tables
   - Refresh and redraw utilities

4. **modal-select2.js** - Modal Select2 management
   - `ModalSelect2` namespace
   - Lifecycle management for Select2 in modals
   - Prevents conflicts and memory leaks

### Module Files
Each feature has its own module file following a consistent pattern:

- **customer-management.js** - Customer CRUD operations
- **vehicle-management.js** - Vehicle CRUD operations
- **service-management.js** - Service CRUD operations
- **order-management.js** - Service Order CRUD operations (with dynamic items)
- **dashboard.js** - Dashboard charts and statistics

## Module Pattern Structure

Each module follows this structure:

```javascript
/**
 * Module Name JavaScript Module
 * 
 * Description of module functionality
 */

window.ModuleName = {
    // Module properties
    someProperty: null,

    // Initialize module
    init: function() {
        this.initDataTable();
        this.loadApiData(); // if needed
        this.bindEvents();
    },

    // Initialize DataTable
    initDataTable: function() {
        if ($.fn.DataTable.isDataTable('#tableId')) {
            $('#tableId').DataTable().destroy();
        }
        
        $("#tableId").DataTable({
            // configuration
        });
    },

    // Load data from API (if needed)
    loadApiData: function() {
        AjaxUtility.get('/Controller/Action', 
            function(response) {
                // handle success
            }.bind(this)
        );
    },

    // Bind event handlers
    bindEvents: function() {
        var self = this;

        // Form submissions
        $('#createForm').on('submit', function(e) {
            e.preventDefault();
            self.handleCreate();
        });

        // Button clicks (delegated events)
        $(document).on('click', '.view-btn', function() {
            var id = $(this).data('id');
            self.showDetails(id);
        });
    },

    // CRUD methods
    handleCreate: function() { /* ... */ },
    showDetails: function(id) { /* ... */ },
    showEdit: function(id) { /* ... */ },
    handleEdit: function() { /* ... */ },
    delete: function(id) { /* ... */ }
};

// Initialize when document is ready
$(document).ready(function() {
    ModuleName.init();
});
```

## Benefits

1. **No Naming Conflicts** - Each module has its own namespace
2. **Consistent Structure** - All modules follow the same pattern
3. **Reusable Code** - Utility functions are shared across modules
4. **Easy to Maintain** - Clear separation of concerns
5. **Testable** - Modules can be tested independently

## Usage in Views

1. Include utility files in `_Layout.cshtml` (before @RenderSection):
```html
<script src="~/js/site.js"></script>
<script src="~/js/ajax-utility.js"></script>
<script src="~/js/datatables-utility.js"></script>
<script src="~/js/modal-select2.js"></script>
```

2. Include module file in specific view:
```html
@section Scripts {
    <script src="~/js/vehicle-management.js"></script>
}
```

## Common Patterns

### Making AJAX Calls
```javascript
AjaxUtility.get('/Controller/Action', 
    function(response) {
        if (response.success) {
            GarageApp.showSuccess('Success message');
        } else {
            GarageApp.showError(response.message);
        }
    }.bind(this)
);
```

### Refreshing DataTables
```javascript
DataTablesUtility.refresh('#tableId');
```

### Confirm Delete
```javascript
GarageApp.confirm('Do you want to delete this?', function(result) {
    if (result.isConfirmed) {
        AjaxUtility.deleteRecord('/Controller/Delete?id=' + id, {
            success: function(response) {
                GarageApp.showSuccess('Deleted successfully');
                DataTablesUtility.refresh('#tableId');
            }
        });
    }
});
```

### Managing Select2 in Modals
```javascript
$('#modalId').on('shown.bs.modal', function () {
    $(this).find('.select2').select2({
        width: '100%',
        dropdownParent: $(this)
    });
});

$('#modalId').on('hide.bs.modal', function () {
    $(this).find('.select2').each(function() {
        if ($(this).hasClass('select2-hidden-accessible')) {
            $(this).select2('destroy');
        }
    });
});
```

## Best Practices

1. **Always use `var self = this;`** in bindEvents() to maintain context
2. **Use `.bind(this)`** for AJAX callbacks to maintain module context
3. **Destroy DataTables** before reinitializing to prevent conflicts
4. **Destroy Select2** in modals before hiding to prevent memory leaks
5. **Use delegated events** for dynamic elements: `$(document).on('click', '.btn', handler)`
6. **Check for element existence** before initializing plugins
7. **Use consistent naming**: create/edit/delete for CRUD operations

## Naming Conventions

- **Module Names**: PascalCase (e.g., `VehicleManagement`)
- **Method Names**: camelCase (e.g., `initDataTable`, `handleCreate`)
- **Form IDs**: action + Entity + Form (e.g., `createVehicleForm`, `editVehicleForm`)
- **Modal IDs**: action + Entity + Modal (e.g., `createVehicleModal`)
- **Table IDs**: entity + Table (e.g., `vehicleTable`)
- **Button Classes**: action + -btn (e.g., `view-btn`, `edit-btn`, `delete-btn`)

## Error Handling

All AJAX calls should handle both success and error cases:

```javascript
AjaxUtility.post(url, data,
    function(response) {
        if (response.success) {
            // Handle success
        } else {
            GarageApp.showError(response.message || 'Operation failed');
        }
    },
    function(xhr, status, error) {
        // Handle error
        var message = 'An error occurred';
        if (xhr.responseJSON && xhr.responseJSON.message) {
            message = xhr.responseJSON.message;
        }
        GarageApp.showError(message);
    }
);
```

## Performance Considerations

1. **Initialize modules only once** - Check if already initialized
2. **Destroy and recreate** DataTables and Select2 to prevent memory leaks
3. **Use delegated events** for dynamic content to avoid rebinding
4. **Cache jQuery selectors** when used multiple times
5. **Minimize DOM manipulations** - batch updates when possible

## Troubleshooting

### DataTable not loading
- Check if table ID is correct
- Verify AJAX endpoint returns correct format: `{ "data": [...] }`
- Check browser console for errors

### Select2 not working in modal
- Ensure `dropdownParent` is set to the modal
- Verify Select2 is destroyed on modal hide
- Check if Select2 library is loaded

### AJAX calls failing
- Verify CSRF token is present in form
- Check endpoint URL is correct
- Verify request/response data format
- Check browser network tab for details

## Related Documentation

- [DataTables Documentation](https://datatables.net/)
- [Select2 Documentation](https://select2.org/)
- [SweetAlert2 Documentation](https://sweetalert2.github.io/)
- [Chart.js Documentation](https://www.chartjs.org/)

