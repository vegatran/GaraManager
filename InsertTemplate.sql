INSERT INTO GarageManagementDB.PrintTemplates (
    TemplateName,
    TemplateType,
    Description,
    CompanyInfo,
    IsDefault,
    IsActive,
    DisplayOrder,
    Notes,
    CreatedAt,
    CreatedBy,
    IsDeleted
) VALUES (
    'Mẫu Báo Giá Mặc Định',
    'Quotation',
    'Mẫu báo giá chuẩn cho gara ô tô Thuận Phát',
    '{"companyName":"GARAGE Ô TÔ THUẬN PHÁT","companyAddress":"313 Võ Văn Vân, Vĩnh Lộc B, H. Bình Chánh, TP.HCM","companyPhone":"032.7007.985 (Mr. Bằng)","companyEmail":"garage@thuanphat.com","companyWebsite":"www.thuanphat.com","taxCode":"0123456789"}',
    1,
    1,
    1,
    'Mẫu in báo giá chuẩn của gara',
    NOW(),
    'System',
    0
);

SELECT 'Đã tạo mẫu báo giá mặc định thành công!' AS Message;
SELECT * FROM GarageManagementDB.PrintTemplates WHERE TemplateType = 'Quotation';

