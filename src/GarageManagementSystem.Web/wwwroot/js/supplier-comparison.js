/**
 * Supplier Comparison Module - Phase 4.2.2
 * 
 * Handles supplier comparison for parts
 */

window.SupplierComparison = {
    currentPartId: null,
    currentQuantity: 1,
    comparisonData: [],
    searchTimeout: null, // ✅ OPTIMIZED: For debouncing search
    cache: new Map(), // ✅ OPTIMIZED: Cache results by partId+quantity

    init: function() {
        this.loadParts();
        this.bindEvents();
    },

    loadParts: function() {
        var self = this;
        
        // Load parts for dropdown using Select2 with AJAX
        $('#partSelect').select2({
            placeholder: '-- Chọn phụ tùng --',
            allowClear: true,
            width: '100%',
            ajax: {
                url: '/PartsManagement/SearchParts',
                dataType: 'json',
                delay: 250,
                data: function(params) {
                    return {
                        searchTerm: params.term || ''
                    };
                },
                processResults: function(data) {
                    // ✅ FIX: SearchParts returns { success: true, data: [...] }
                    if (data && data.success && data.data && Array.isArray(data.data)) {
                        return {
                            results: data.data.map(function(part) {
                                return {
                                    id: part.id || part.partId,
                                    text: `${part.partName || part.text} (${part.partNumber})`,
                                    partNumber: part.partNumber,
                                    partName: part.partName || part.text
                                };
                            })
                        };
                    }
                    return { results: [] };
                },
                cache: true
            },
            minimumInputLength: 1
        });
    },

    bindEvents: function() {
        var self = this;

        // Search button
        $('#btnSearch').on('click', function() {
            self.searchComparison();
        });

        // Get recommendation button
        $('#btnGetRecommendation').on('click', function() {
            var partId = $('#partSelect').val();
            var quantity = parseInt($('#quantityInput').val(), 10) || 1;
            if (partId) {
                window.location.href = `/Procurement/SupplierRecommendation?partId=${partId}&quantity=${quantity}`;
            } else {
                GarageApp.showWarning('Vui lòng chọn phụ tùng.');
            }
        });

        // Create PO button
        $('#btnCreatePO').on('click', function() {
            self.createPO();
        });

        // ✅ OPTIMIZED: Debounce quantity input changes
        $('#quantityInput').on('input', function() {
            clearTimeout(self.searchTimeout);
            // Auto-search after 500ms of no input
            self.searchTimeout = setTimeout(function() {
                if ($('#partSelect').val()) {
                    self.searchComparison();
                }
            }, 500);
        });

        // Enter key on quantity input
        $('#quantityInput').on('keypress', function(e) {
            if (e.which === 13) {
                clearTimeout(self.searchTimeout);
                self.searchComparison();
            }
        });
    },

    searchComparison: function() {
        var self = this;
        var partId = $('#partSelect').val();
        var quantity = parseInt($('#quantityInput').val(), 10) || 1;

        if (!partId) {
            GarageApp.showWarning('Vui lòng chọn phụ tùng.');
            return;
        }

        if (quantity <= 0) {
            GarageApp.showWarning('Số lượng phải lớn hơn 0.');
            return;
        }

        // ✅ OPTIMIZED: Check cache first
        var cacheKey = `${partId}_${quantity}`;
        if (self.cache.has(cacheKey)) {
            var cachedData = self.cache.get(cacheKey);
            self.comparisonData = cachedData;
            self.renderComparison(cachedData);
            $('#comparisonResults').show();
            
            var selectedOption = $('#partSelect').select2('data')[0];
            if (selectedOption) {
                $('#partName').text(selectedOption.text);
                $('#partQuantity').text(quantity);
            }
            return;
        }

        self.currentPartId = partId;
        self.currentQuantity = quantity;

        GarageApp.showLoading('Đang tải dữ liệu so sánh...');

        // ✅ FIX: Store cacheKey in closure to use in callback
        var cacheKeyForCallback = cacheKey;

        $.ajax({
            url: '/Procurement/GetSupplierComparison',
            type: 'GET',
            data: {
                partId: partId,
                quantity: quantity
            }
        })
        .done(function(response) {
            GarageApp.hideLoading();
            
            if (response && response.success && response.data && Array.isArray(response.data) && response.data.length > 0) {
                self.comparisonData = response.data;
                // ✅ OPTIMIZED: Cache the result
                self.cache.set(cacheKeyForCallback, response.data);
                // Limit cache size to 50 entries
                if (self.cache.size > 50) {
                    var firstKey = self.cache.keys().next().value;
                    self.cache.delete(firstKey);
                }
                
                self.renderComparison(response.data);
                $('#comparisonResults').show();
                
                // Get part info from select2
                var selectedOption = $('#partSelect').select2('data')[0];
                if (selectedOption) {
                    $('#partName').text(selectedOption.text);
                    $('#partQuantity').text(quantity);
                }
            } else {
                GarageApp.showWarning(response.errorMessage || 'Không có nhà cung cấp nào cho phụ tùng này.');
                $('#comparisonResults').hide();
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (xhr.status === 401) {
                AuthHandler.handleUnauthorized(xhr, true);
            } else {
                var errorMsg = xhr.responseJSON?.errorMessage || xhr.responseJSON?.message || 'Lỗi khi tải dữ liệu so sánh.';
                GarageApp.showError(errorMsg);
            }
            $('#comparisonResults').hide();
        });
    },

    renderComparison: function(data) {
        var self = this;
        // ✅ SAFETY: Null/empty check
        if (!data || !Array.isArray(data) || data.length === 0) {
            GarageApp.showWarning('Không có dữ liệu để hiển thị.');
            return;
        }
        
        var tbody = $('#comparisonTableBody');
        tbody.empty();

        // Sort by CalculatedScore descending
        var sortedData = data.slice().sort(function(a, b) {
            return (b.calculatedScore || 0) - (a.calculatedScore || 0);
        });

        sortedData.forEach(function(supplier) {
            var row = `
                <tr ${supplier.isPreferred ? 'class="table-warning"' : ''}>
                    <td>
                        <strong>${supplier.supplierName}</strong>
                        ${supplier.isPreferred ? '<span class="badge badge-warning ml-2">Ưu tiên</span>' : ''}
                        <br>
                        <small class="text-muted">${supplier.supplierCode || ''}</small>
                    </td>
                    <td class="text-right">${self.formatCurrency(supplier.unitPrice)}</td>
                    <td class="text-right font-weight-bold">${self.formatCurrency(supplier.totalPrice)}</td>
                    <td class="text-center">
                        ${self.renderRating(supplier.averageRating)}
                        <br>
                        <small class="text-muted">(${supplier.ratingCount} đánh giá)</small>
                    </td>
                    <td class="text-center">
                        <span class="badge ${supplier.onTimeDeliveryRate >= 90 ? 'badge-success' : supplier.onTimeDeliveryRate >= 70 ? 'badge-warning' : 'badge-danger'}">
                            ${supplier.onTimeDeliveryRate.toFixed(1)}%
                        </span>
                    </td>
                    <td class="text-center">
                        <span class="badge ${supplier.defectRate <= 2 ? 'badge-success' : supplier.defectRate <= 5 ? 'badge-warning' : 'badge-danger'}">
                            ${supplier.defectRate.toFixed(1)}%
                        </span>
                    </td>
                    <td class="text-center">${supplier.leadTimeDays} ngày</td>
                    <td class="text-center">
                        <span class="badge badge-info">${supplier.calculatedScore.toFixed(1)}</span>
                    </td>
                    <td>
                        <button class="btn btn-sm btn-success create-po-btn" 
                                data-supplier-id="${supplier.supplierId}"
                                data-unit-price="${supplier.unitPrice}"
                                title="Tạo PO">
                            <i class="fas fa-shopping-cart"></i>
                        </button>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });

        // Bind create PO buttons
        $('.create-po-btn').on('click', function() {
            var supplierId = $(this).data('supplier-id');
            var unitPrice = $(this).data('unit-price');
            self.createPOForSupplier(supplierId, unitPrice);
        });
    },

    renderRating: function(rating) {
        if (!rating || rating === 0) {
            return '<span class="text-muted">Chưa có đánh giá</span>';
        }
        
        var fullStars = Math.floor(rating);
        var hasHalfStar = rating % 1 >= 0.5;
        var emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);
        
        var html = '';
        for (var i = 0; i < fullStars; i++) {
            html += '<i class="fas fa-star text-warning"></i>';
        }
        if (hasHalfStar) {
            html += '<i class="fas fa-star-half-alt text-warning"></i>';
        }
        for (var i = 0; i < emptyStars; i++) {
            html += '<i class="far fa-star text-muted"></i>';
        }
        return html;
    },

    createPO: function() {
        // Create PO for the best supplier (highest score)
        if (this.comparisonData.length === 0) {
            GarageApp.showWarning('Không có dữ liệu để tạo PO.');
            return;
        }

        var bestSupplier = this.comparisonData.sort(function(a, b) {
            return (b.calculatedScore || 0) - (a.calculatedScore || 0);
        })[0];

        this.createPOForSupplier(bestSupplier.supplierId, bestSupplier.unitPrice);
    },

    createPOForSupplier: function(supplierId, unitPrice) {
        if (!this.currentPartId) {
            GarageApp.showWarning('Vui lòng chọn phụ tùng trước.');
            return;
        }

        // Redirect to Purchase Order creation with pre-filled data
        var params = new URLSearchParams({
            partId: this.currentPartId,
            quantity: this.currentQuantity,
            supplierId: supplierId,
            unitPrice: unitPrice
        });
        
        window.location.href = `/PurchaseOrder?${params.toString()}`;
    },

    formatCurrency: function(value) {
        if (!value && value !== 0) return '0';
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(value);
    }
};

// Initialize when document is ready
$(document).ready(function() {
    if ($('#comparisonTable').length > 0) {
        SupplierComparison.init();
    }
});

