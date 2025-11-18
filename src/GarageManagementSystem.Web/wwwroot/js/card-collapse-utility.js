/**
 * Card Collapse Utility
 * 
 * Automatically adds Bootstrap collapse functionality to cards inside modals only
 * Uses Bootstrap's built-in collapse component with data-toggle="collapse"
 */

(function() {
    'use strict';

    /**
     * Initialize card collapse functionality for cards inside modals only
     */
    function initCardCollapseInModals() {
        // Find all modals
        var modals = document.querySelectorAll('.modal');
        
        modals.forEach(function(modal) {
            // Find cards inside this modal that have both card-header and card-body
            var cards = modal.querySelectorAll('.card');
            
            cards.forEach(function(card, index) {
                // Skip if already initialized
                if (card.classList.contains('card-collapse-initialized')) {
                    return;
                }
                
                var cardHeader = card.querySelector('.card-header');
                var cardBody = card.querySelector('.card-body');
                
                if (!cardHeader || !cardBody) {
                    return;
                }
                
                // Skip if already has collapse toggle
                if (cardHeader.querySelector('[data-toggle="collapse"]')) {
                    return;
                }
                
                // Generate unique ID for collapse (use modal ID if available, otherwise use index)
                var modalId = modal.id || 'modal_' + index;
                var collapseId = 'cardCollapse_' + modalId + '_' + index;
                
                // Check if ID already exists, if so, skip to avoid conflicts
                if (document.getElementById(collapseId)) {
                    return;
                }
                
                // Add collapse classes and ID to card-body (Bootstrap default)
                // Don't add 'show' class initially - let Bootstrap handle it
                cardBody.id = collapseId;
                if (!cardBody.classList.contains('collapse')) {
                    cardBody.classList.add('collapse');
                }
                // Ensure it starts expanded
                if (!cardBody.classList.contains('show')) {
                    cardBody.classList.add('show');
                }
                
                // Create toggle button using Bootstrap's standard collapse pattern
                var toggleBtn = document.createElement('button');
                toggleBtn.type = 'button';
                toggleBtn.className = 'btn btn-sm btn-link ml-auto card-collapse-btn';
                toggleBtn.setAttribute('data-toggle', 'collapse');
                toggleBtn.setAttribute('data-target', '#' + collapseId);
                toggleBtn.setAttribute('aria-expanded', 'true');
                toggleBtn.setAttribute('aria-controls', collapseId);
                
                // Use appropriate color based on header background
                var headerClasses = cardHeader.className;
                if (headerClasses.includes('bg-') && !headerClasses.includes('bg-light') && !headerClasses.includes('bg-white')) {
                    toggleBtn.classList.add('text-white');
                } else {
                    toggleBtn.classList.add('text-muted');
                }
                
                toggleBtn.innerHTML = '<i class="fas fa-chevron-up"></i>';
                toggleBtn.style.cssText = 'padding: 0; border: none; background: transparent; font-size: 0.875rem;';
                
                // Add toggle button to card-header
                if (cardHeader.classList.contains('d-flex')) {
                    cardHeader.appendChild(toggleBtn);
                } else {
                    var headerContent = cardHeader.innerHTML;
                    cardHeader.innerHTML = '';
                    cardHeader.classList.add('d-flex', 'justify-content-between', 'align-items-center');
                    
                    var contentWrapper = document.createElement('div');
                    contentWrapper.className = 'flex-grow-1';
                    contentWrapper.innerHTML = headerContent;
                    cardHeader.appendChild(contentWrapper);
                    cardHeader.appendChild(toggleBtn);
                }
                
                // Store references for cleanup
                var $cardBody = $(cardBody);
                var $toggleBtn = $(toggleBtn);
                
                // Update icon when collapse state changes (Bootstrap events using jQuery)
                // Use namespace to allow proper cleanup
                // Use CSS transform for smooth rotation instead of changing icon classes
                $cardBody.off('show.bs.collapse.cardCollapse hide.bs.collapse.cardCollapse shown.bs.collapse.cardCollapse hidden.bs.collapse.cardCollapse');
                
                // Update aria-expanded immediately when collapse starts for smooth CSS transition
                $cardBody.on('show.bs.collapse.cardCollapse', function() {
                    $toggleBtn.attr('aria-expanded', 'true');
                });
                
                $cardBody.on('hide.bs.collapse.cardCollapse', function() {
                    $toggleBtn.attr('aria-expanded', 'false');
                });
                
                // Ensure state is correct after animation completes
                $cardBody.on('shown.bs.collapse.cardCollapse', function() {
                    $toggleBtn.attr('aria-expanded', 'true');
                });
                
                $cardBody.on('hidden.bs.collapse.cardCollapse', function() {
                    $toggleBtn.attr('aria-expanded', 'false');
                });
                
                // Mark as initialized
                card.classList.add('card-collapse-initialized');
            });
        });
    }
    
    /**
     * Clean up card collapse when modal is hidden
     */
    function cleanupCardCollapseInModal(modal) {
        var cards = modal.querySelectorAll('.card.card-collapse-initialized');
        cards.forEach(function(card) {
            var cardBody = card.querySelector('.card-body');
            var toggleBtn = card.querySelector('.card-collapse-btn');
            
            if (cardBody && toggleBtn) {
                // Remove event listeners using namespace
                $(cardBody).off('show.bs.collapse.cardCollapse hide.bs.collapse.cardCollapse shown.bs.collapse.cardCollapse hidden.bs.collapse.cardCollapse');
                
                // Remove collapse classes but keep the ID for next time
                // Actually, let's keep it initialized to avoid re-adding buttons
                // Just ensure the collapse state is reset
                if (!$(cardBody).hasClass('show')) {
                    $(cardBody).addClass('show');
                    var icon = $(toggleBtn).find('i');
                    if (icon.length) {
                        icon.removeClass('fa-chevron-down').addClass('fa-chevron-up');
                    }
                    $(toggleBtn).attr('aria-expanded', 'true');
                }
            }
        });
    }

    // Initialize when modals are shown (Bootstrap event)
    $(document).on('shown.bs.modal', function(e) {
        // Initialize collapse for cards in the shown modal
        var modal = e.target;
        setTimeout(function() {
            initCardCollapseInModals();
        }, 100);
    });

    // Clean up when modal is hidden
    $(document).on('hidden.bs.modal', function(e) {
        var modal = e.target;
        cleanupCardCollapseInModal(modal);
    });

    // Also initialize for any modals already in DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            setTimeout(initCardCollapseInModals, 100);
        });
    } else {
        setTimeout(initCardCollapseInModals, 100);
    }

    // Export function for manual initialization if needed
    window.CardCollapseUtility = {
        init: initCardCollapseInModals
    };

})();

