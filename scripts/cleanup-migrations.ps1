# PowerShell Script để xóa migrations dư thừa
# Chạy với quyền Administrator và backup trước khi thực hiện

param(
    [switch]$WhatIf = $false,
    [switch]$Backup = $true,
    [switch]$Force = $false
)

Write-Host "🗑️ MIGRATION CLEANUP SCRIPT" -ForegroundColor Yellow
Write-Host "================================" -ForegroundColor Yellow

# Kiểm tra quyền
if (-not $Force) {
    $confirm = Read-Host "⚠️ Bạn có chắc chắn muốn xóa migrations? (yes/no)"
    if ($confirm -ne "yes") {
        Write-Host "❌ Hủy bỏ thao tác" -ForegroundColor Red
        exit
    }
}

# Đường dẫn
$migrationsPath = "src/GarageManagementSystem.Infrastructure/Migrations"
$backupPath = "migrations_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"

# Tạo backup nếu được yêu cầu
if ($Backup) {
    Write-Host "📦 Tạo backup migrations..." -ForegroundColor Green
    if (Test-Path $migrationsPath) {
        Copy-Item -Path $migrationsPath -Destination $backupPath -Recurse -Force
        Write-Host "✅ Backup tạo tại: $backupPath" -ForegroundColor Green
    } else {
        Write-Host "❌ Không tìm thấy thư mục migrations" -ForegroundColor Red
        exit 1
    }
}

# Danh sách files cần xóa
$filesToDelete = @(
    "20251012073417_InitialCreate.cs",
    "20251012073417_InitialCreate.Designer.cs",
    "20251014023327_AddMileageToVehicle.cs",
    "20251014023327_AddMileageToVehicle.Designer.cs",
    "20251014052214_AddPrintTemplateTable.cs",
    "20251014052214_AddPrintTemplateTable.Designer.cs",
    "20251014061829_AddPrintTemplatesTable.cs",
    "20251014061829_AddPrintTemplatesTable.Designer.cs",
    "20251014103414_AddHasInvoiceToQuotationItem.cs",
    "20251014103414_AddHasInvoiceToQuotationItem.Designer.cs",
    "20251015012805_AddCustomerReceptionAndWorkflowTracking.cs",
    "20251015012805_AddCustomerReceptionAndWorkflowTracking.Designer.cs",
    "20251015100112_ConvertReceptionStatusToEnum.cs",
    "20251015100112_ConvertReceptionStatusToEnum.Designer.cs",
    "20251015122942_AddPricingModelsToServiceAndQuotationItem.cs",
    "20251015122942_AddPricingModelsToServiceAndQuotationItem.Designer.cs",
    "20251015123726_AddPricingModelSupport.cs",
    "20251015123726_AddPricingModelSupport.Designer.cs",
    "20251016004206_UpdateServicePricingFields.cs",
    "20251016004206_UpdateServicePricingFields.Designer.cs",
    "20251016021540_IncreasePricingBreakdownLength.cs",
    "20251016021540_IncreasePricingBreakdownLength.Designer.cs",
    "20251016031914_AddItemCategoryToQuotationItem.cs",
    "20251016031914_AddItemCategoryToQuotationItem.Designer.cs",
    "20251019153822_AddQuotationAttachmentAndInsurancePricing.cs",
    "20251019153822_AddQuotationAttachmentAndInsurancePricing.Designer.cs",
    "20251020074835_AddCorporateFieldsToQuotation.cs",
    "20251020074835_AddCorporateFieldsToQuotation.Designer.cs",
    "20251022102808_AddVATFieldsToPartAndQuotationItem.cs",
    "20251022102808_AddVATFieldsToPartAndQuotationItem.Designer.cs"
)

# Files cần giữ lại
$filesToKeep = @(
    "GarageDbContextModelSnapshot.cs"
)

Write-Host "🔍 Kiểm tra files..." -ForegroundColor Blue

$deletedCount = 0
$notFoundCount = 0

foreach ($file in $filesToDelete) {
    $filePath = Join-Path $migrationsPath $file
    
    if (Test-Path $filePath) {
        if ($WhatIf) {
            Write-Host "🔍 Sẽ xóa: $file" -ForegroundColor Yellow
        } else {
            try {
                Remove-Item $filePath -Force
                Write-Host "✅ Đã xóa: $file" -ForegroundColor Green
                $deletedCount++
            } catch {
                Write-Host "❌ Lỗi khi xóa: $file - $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "⚠️ Không tìm thấy: $file" -ForegroundColor Yellow
        $notFoundCount++
    }
}

# Kiểm tra files còn lại
Write-Host "`n📋 Files còn lại trong thư mục migrations:" -ForegroundColor Blue
$remainingFiles = Get-ChildItem $migrationsPath -File | Select-Object -ExpandProperty Name
foreach ($file in $remainingFiles) {
    if ($filesToKeep -contains $file) {
        Write-Host "✅ Giữ lại: $file" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Còn lại: $file" -ForegroundColor Yellow
    }
}

# Tóm tắt
Write-Host "`n📊 TÓM TẮT:" -ForegroundColor Cyan
Write-Host "===========" -ForegroundColor Cyan
Write-Host "✅ Files đã xóa: $deletedCount" -ForegroundColor Green
Write-Host "⚠️ Files không tìm thấy: $notFoundCount" -ForegroundColor Yellow
Write-Host "📦 Backup tại: $backupPath" -ForegroundColor Blue

if ($WhatIf) {
    Write-Host "`n🔍 CHẠY VỚI -WhatIf: Không có file nào bị xóa thực sự" -ForegroundColor Yellow
    Write-Host "💡 Để xóa thực sự, chạy lại script không có -WhatIf" -ForegroundColor Yellow
} else {
    Write-Host "`n🎉 HOÀN THÀNH!" -ForegroundColor Green
    Write-Host "💡 Bây giờ bạn có thể sử dụng CONSOLIDATED_DATABASE_SCHEMA.sql" -ForegroundColor Blue
    Write-Host "🔄 Để rollback, restore từ: $backupPath" -ForegroundColor Blue
}

Write-Host "`n⚠️ LƯU Ý:" -ForegroundColor Red
Write-Host "- Backup database trước khi test" -ForegroundColor Red
Write-Host "- Test trên development environment trước" -ForegroundColor Red
Write-Host "- Sync code với team members" -ForegroundColor Red
