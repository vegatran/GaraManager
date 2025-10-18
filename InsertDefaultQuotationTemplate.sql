-- ============================================
-- Script: Insert Default Quotation Template
-- Description: Tạo mẫu báo giá mặc định cho hệ thống
-- ============================================

USE GarageManagementDB;

-- Xóa mẫu cũ nếu có (để tránh duplicate)
DELETE FROM PrintTemplates WHERE TemplateType = 'Quotation' AND TemplateName = 'Mẫu Báo Giá Mặc Định';

-- Insert mẫu báo giá mặc định
INSERT INTO PrintTemplates (
    TemplateName,
    TemplateType,
    Description,
    HeaderHtml,
    FooterHtml,
    CompanyInfo,
    CustomCss,
    IsDefault,
    IsActive,
    DisplayOrder,
    LogoFileName,
    LogoPath,
    Notes,
    CreatedAt,
    CreatedBy,
    IsDeleted
) VALUES (
    'Mẫu Báo Giá Mặc Định',
    'Quotation',
    'Mẫu báo giá chuẩn cho gara ô tô Thuận Phát',
    '<div class="header">
        <div class="logo-section">
            <div class="logo">GARAGE<br>DỊCH VỤ SỬA CHỮA<br>Ô TÔ THUẬN PHÁT</div>
        </div>
        <div class="company-info">
            <div class="company-name">GARAGE Ô TÔ THUẬN PHÁT</div>
        </div>
    </div>',
    '<div class="footer">
        <p>Xin cảm ơn quý khách đã tin tưởng dịch vụ của chúng tôi!</p>
        <p>Báo giá có hiệu lực trong vòng 7 ngày kể từ ngày phát hành.</p>
    </div>',
    '{
        "companyName": "GARAGE Ô TÔ THUẬN PHÁT",
        "companyAddress": "313 Võ Văn Vân, Vĩnh Lộc B, H. Bình Chánh, TP.HCM",
        "companyPhone": "032.7007.985 (Mr. Bằng)",
        "companyEmail": "garage@thuanphat.com",
        "companyWebsite": "www.thuanphat.com",
        "taxCode": "0123456789"
    }',
    '@media print {
        body { font-size: 11px; }
        .print-container { margin: 0; padding: 5mm; max-width: none; }
        .no-print { display: none !important; }
        @page { size: A4; margin: 10mm; }
    }',
    1, -- IsDefault
    1, -- IsActive
    1, -- DisplayOrder
    NULL, -- LogoFileName
    NULL, -- LogoPath
    'Mẫu in báo giá chuẩn của gara. Được tạo tự động khi khởi tạo hệ thống.',
    NOW(),
    'System',
    0 -- IsDeleted
);

-- Kiểm tra kết quả
SELECT 
    Id,
    TemplateName,
    TemplateType,
    IsDefault,
    IsActive,
    CreatedAt,
    CreatedBy
FROM PrintTemplates
WHERE TemplateType = 'Quotation'
ORDER BY CreatedAt DESC;

-- Thông báo thành công
SELECT 'Đã tạo mẫu báo giá mặc định thành công!' AS Message;

