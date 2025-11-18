/**
 * Supplier Recommendation Module - Phase 4.2.2
 * 
 * Handles supplier recommendation for parts
 */

window.SupplierRecommendation = {
    currentPartId: null,
    currentQuantity: 1,
    recommendationData: null,
    searchTimeout: null, // ✅ OPTIMIZED: For debouncing search
    cache: new Map(), // ✅ OPTIMIZED: Cache results by partId+quantity

    init: function() {
        this.loadParts();
        this.bindEvents();
        this.checkUrlParams();
    },

    checkUrlParams: function() {
        // Check if partId and quantity are in URL params
        var urlParams = new URLSearchParams(window.location.search);
        var partId = urlParams.get('partId');
        var quantity = urlParams.get('quantity') || '1';

        if (partId) {
            $('#partSelect').val(partId).trigger('change');
            $('#quantityInput').val(quantity);
            // Auto-load recommendation
            setTimeout(() => {
                this.getRecommendation();
            }, 500); // Wait for select2 to load
        }
    },

    loadParts: function() {
        var self = this;
        
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

        // Get recommendation button
        $('#btnGetRecommendation').on('click', function() {
            self.getRecommendation();
        });

        // View comparison button
        $('#btnViewComparison').on('click', function() {
            var partId = $('#partSelect').val();
            var quantity = parseInt($('#quantityInput').val(), 10) || 1;
            if (partId) {
                window.location.href = `/Procurement/SupplierComparison?partId=${partId}&quantity=${quantity}`;
            } else {
                GarageApp.showWarning('Vui lòng chọn phụ tùng.');
            }
        });

        // Create PO from recommendation button
        $('#btnCreatePOFromRecommendation').on('click', function() {
            self.createPOFromRecommendation();
        });

        // ✅ OPTIMIZED: Debounce quantity input changes
        $('#quantityInput').on('input', function() {
            clearTimeout(self.searchTimeout);
            // Auto-search after 500ms of no input
            self.searchTimeout = setTimeout(function() {
                if ($('#partSelect').val()) {
                    self.getRecommendation();
                }
            }, 500);
        });

        // Enter key on quantity input
        $('#quantityInput').on('keypress', function(e) {
            if (e.which === 13) {
                clearTimeout(self.searchTimeout);
                self.getRecommendation();
            }
        });
    },

    getRecommendation: function() {
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
            self.recommendationData = cachedData;
            self.renderRecommendation(cachedData);
            $('#recommendationResults').show();
            return;
        }

        self.currentPartId = partId;
        self.currentQuantity = quantity;

        GarageApp.showLoading('Đang tải đề xuất nhà cung cấp...');

        // ✅ FIX: Store cacheKey in closure to use in callback
        var cacheKeyForCallback = cacheKey;

        $.ajax({
            url: '/Procurement/GetSupplierRecommendation',
            type: 'GET',
            data: {
                partId: partId,
                quantity: quantity
            }
        })
        .done(function(response) {
            GarageApp.hideLoading();
            
            if (response && response.success && response.data) {
                self.recommendationData = response.data;
                // ✅ OPTIMIZED: Cache the result
                self.cache.set(cacheKeyForCallback, response.data);
                // Limit cache size to 50 entries
                if (self.cache.size > 50) {
                    var firstKey = self.cache.keys().next().value;
                    self.cache.delete(firstKey);
                }
                
                self.renderRecommendation(response.data);
                $('#recommendationResults').show();
            } else {
                GarageApp.showWarning(response.errorMessage || 'Không có nhà cung cấp nào cho phụ tùng này.');
                $('#recommendationResults').hide();
            }
        })
        .fail(function(xhr) {
            GarageApp.hideLoading();
            if (xhr.status === 401) {
                AuthHandler.handleUnauthorized(xhr, true);
            } else {
                var errorMsg = xhr.responseJSON?.errorMessage || xhr.responseJSON?.message || 'Lỗi khi tải đề xuất.';
                GarageApp.showError(errorMsg);
            }
            $('#recommendationResults').hide();
        });
    },

    renderRecommendation: function(data) {
        var self = this;
        // ✅ SAFETY: Null check for data and recommendedSupplier
        if (!data || !data.recommendedSupplier) {
            GarageApp.showError('Dữ liệu không hợp lệ.');
            return;
        }
        var recommended = data.recommendedSupplier;
        var allSuppliers = data.allSuppliers || [];

        // Render recommended supplier info
        $('#recommendedSupplierName').text(recommended.supplierName);
        $('#recommendedSupplierCode').text(recommended.supplierCode || '-');
        $('#recommendedContact').text(recommended.contactPerson || '-');
        $('#recommendedEmail').text(recommended.email || '-');
        $('#recommendedPhone').text(recommended.phone || '-');
        $('#recommendedUnitPrice').text(self.formatCurrency(recommended.unitPrice));
        $('#recommendedTotalPrice').text(self.formatCurrency(recommended.totalPrice));
        $('#recommendedLeadTime').text(recommended.leadTimeDays);
        $('#recommendedScore').text(recommended.calculatedScore.toFixed(1));
        $('#recommendationReason').text(data.recommendationReason || 'Nhà cung cấp được đề xuất dựa trên điểm số tổng thể');

        // Render rating
        $('#recommendedRating').html(self.renderRating(recommended.averageRating));
        $('#recommendedRatingCount').text(recommended.ratingCount || 0);

        // Render performance
        $('#recommendedOnTime').text(recommended.onTimeDeliveryRate.toFixed(1) + '%');
        $('#recommendedDefectRate').text(recommended.defectRate.toFixed(1) + '%');

        // Render all suppliers comparison
        var tbody = $('#allSuppliersTableBody');
        tbody.empty();

        // Sort by CalculatedScore descending
        var sortedSuppliers = allSuppliers.sort(function(a, b) {
            return (b.calculatedScore || 0) - (a.calculatedScore || 0);
        });

        sortedSuppliers.forEach(function(supplier) {
            var isRecommended = supplier.supplierId === recommended.supplierId;
            var priceDiff = supplier.unitPrice - data.averagePrice;
            var priceDiffPercent = data.averagePrice > 0 ? (priceDiff / data.averagePrice) * 100 : 0;
            
            var row = `
                <tr ${isRecommended ? 'class="table-success"' : ''}>
                    <td>
                        <strong>${supplier.supplierName}</strong>
                        ${isRecommended ? '<span class="badge badge-success ml-2">Đề xuất</span>' : ''}
                        ${supplier.isPreferred ? '<span class="badge badge-warning ml-2">Ưu tiên</span>' : ''}
                    </td>
                    <td class="text-right">${self.formatCurrency(supplier.unitPrice)}</td>
                    <td class="text-right font-weight-bold">${self.formatCurrency(supplier.totalPrice)}</td>
                    <td class="text-right">
                        ${priceDiffPercent > 0 
                            ? `<span class="text-danger">+${priceDiffPercent.toFixed(1)}%</span>` 
                            : priceDiffPercent < 0 
                            ? `<span class="text-success">${priceDiffPercent.toFixed(1)}%</span>` 
                            : '<span class="text-muted">0%</span>'}
                    </td>
                    <td class="text-center">${self.renderRating(supplier.averageRating)}</td>
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

    createPOFromRecommendation: function() {
        if (!this.recommendationData || !this.recommendationData.recommendedSupplier) {
            GarageApp.showWarning('Không có dữ liệu để tạo PO.');
            return;
        }

        var recommended = this.recommendationData.recommendedSupplier;
        this.createPOForSupplier(recommended.supplierId, recommended.unitPrice);
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
    if ($('#recommendationResults').length > 0) {
        SupplierRecommendation.init();
    }
});

