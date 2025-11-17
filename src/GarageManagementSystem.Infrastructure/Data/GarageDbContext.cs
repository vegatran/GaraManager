using GarageManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using OfficeOpenXml;

namespace GarageManagementSystem.Infrastructure.Data
{
    public class GarageDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        static GarageDbContext()
        {
            // Set EPPlus license globally for version 8+ - Use new License property
            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");
        }

        public GarageDbContext(DbContextOptions<GarageDbContext> options) : base(options)
        {
        }

        public GarageDbContext(DbContextOptions<GarageDbContext> options, IHttpContextAccessor httpContextAccessor) 
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceOrder> ServiceOrders { get; set; }
        public DbSet<ServiceOrderItem> ServiceOrderItems { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<VehicleInspection> VehicleInspections { get; set; }
        public DbSet<InspectionIssue> InspectionIssues { get; set; }
        public DbSet<InspectionPhoto> InspectionPhotos { get; set; }
        
        // ✅ 2.3.2: Additional Issues (Phát sinh trong quá trình sửa chữa)
        public DbSet<AdditionalIssue> AdditionalIssues { get; set; }
        public DbSet<AdditionalIssuePhoto> AdditionalIssuePhotos { get; set; }
        
        // ✅ 2.4.2: Quality Control (Kiểm tra chất lượng)
        public DbSet<QualityControl> QualityControls { get; set; }
        public DbSet<QCChecklistItem> QCChecklistItems { get; set; }
        
        // ✅ OPTIMIZED: QC Checklist Template (Template mẫu cho QC Checklist)
        public DbSet<QCChecklistTemplate> QCChecklistTemplates { get; set; }
        public DbSet<QCChecklistTemplateItem> QCChecklistTemplateItems { get; set; }
        
        public DbSet<ServiceQuotation> ServiceQuotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }
        public DbSet<QuotationAttachment> QuotationAttachments { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        public DbSet<ServiceOrderPart> ServiceOrderParts { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<InsuranceInvoice> InsuranceInvoices { get; set; }
        public DbSet<InsuranceInvoiceItem> InsuranceInvoiceItems { get; set; }
        
        // Phase 1 - New entities
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<InsuranceClaim> InsuranceClaims { get; set; }
        public DbSet<InsuranceClaimDocument> InsuranceClaimDocuments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Warranty> Warranties { get; set; }
        public DbSet<WarrantyItem> WarrantyItems { get; set; }
        public DbSet<WarrantyClaim> WarrantyClaims { get; set; }
        public DbSet<ServiceFeeType> ServiceFeeTypes { get; set; }
        public DbSet<ServiceOrderFee> ServiceOrderFees { get; set; }
        public DbSet<CustomerFeedback> CustomerFeedbacks { get; set; }
        public DbSet<CustomerFeedbackAttachment> CustomerFeedbackAttachments { get; set; }
        public DbSet<FeedbackChannel> FeedbackChannels { get; set; }
        public DbSet<PartUnit> PartUnits { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<WarehouseZone> WarehouseZones { get; set; }
        public DbSet<WarehouseBin> WarehouseBins { get; set; }
        
        // Advanced features
        public DbSet<PartGroup> PartGroups { get; set; }
        public DbSet<PartGroupCompatibility> PartGroupCompatibilities { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<LaborCategory> LaborCategories { get; set; }
        public DbSet<LaborItem> LaborItems { get; set; }
        public DbSet<ServiceOrderLabor> ServiceOrderLabors { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<PartInventoryBatch> PartInventoryBatches { get; set; }
        public DbSet<MaterialRequest> MaterialRequests { get; set; }
        public DbSet<MaterialRequestItem> MaterialRequestItems { get; set; }
        
        // Print Templates
        public DbSet<PrintTemplate> PrintTemplates { get; set; }
        public DbSet<PartBatchUsage> PartBatchUsages { get; set; }
        public DbSet<VehicleBrand> VehicleBrands { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<EngineSpecification> EngineSpecifications { get; set; }
        public DbSet<VehicleInsurance> VehicleInsurances { get; set; }
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
        public DbSet<FinancialTransactionAttachment> FinancialTransactionAttachments { get; set; }
        public DbSet<PartSupplier> PartSuppliers { get; set; }
        
        // Customer Reception for new workflow
        public DbSet<CustomerReception> CustomerReceptions { get; set; }
        
        // ✅ Phase 4.1 - Sprint 2: Periodic Inventory Checks
        public DbSet<InventoryCheck> InventoryChecks { get; set; }
        public DbSet<InventoryCheckItem> InventoryCheckItems { get; set; }
        public DbSet<InventoryAdjustment> InventoryAdjustments { get; set; }
        public DbSet<InventoryAdjustmentItem> InventoryAdjustmentItems { get; set; }
        // ✅ Phase 4.1 - Advanced Features: Comments/Notes Timeline
        public DbSet<InventoryCheckComment> InventoryCheckComments { get; set; }
        public DbSet<InventoryAdjustmentComment> InventoryAdjustmentComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Customer Reception
            ConfigureCustomerReception(modelBuilder);

            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Gender).HasMaxLength(20);
                
                // Unique index chỉ áp dụng cho bản ghi chưa xóa (IsDeleted = false)
                entity.HasIndex(e => e.Phone)
                    .IsUnique();
                    
                entity.HasIndex(e => e.Email)
                    .IsUnique();
            });

            // Vehicle configuration
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LicensePlate).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Brand).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Year).HasMaxLength(20);
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.VIN).HasMaxLength(17);
                entity.Property(e => e.EngineNumber).HasMaxLength(50);
                
                // Vehicle Type and Insurance/Company fields
                entity.Property(e => e.VehicleType).HasMaxLength(20).HasDefaultValue("Personal");
                
                // Insurance fields
                entity.Property(e => e.InsuranceCompany).HasMaxLength(100);
                entity.Property(e => e.PolicyNumber).HasMaxLength(50);
                entity.Property(e => e.CoverageType).HasMaxLength(50);
                entity.Property(e => e.ClaimNumber).HasMaxLength(50);
                entity.Property(e => e.AdjusterName).HasMaxLength(100);
                entity.Property(e => e.AdjusterPhone).HasMaxLength(20);
                
                // Company fields
                entity.Property(e => e.CompanyName).HasMaxLength(200);
                entity.Property(e => e.TaxCode).HasMaxLength(20);
                entity.Property(e => e.ContactPerson).HasMaxLength(100);
                entity.Property(e => e.ContactPhone).HasMaxLength(20);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.CostCenter).HasMaxLength(50);
                
                // Shadow property for computed unique index (chỉ enforce với bản ghi chưa xóa)
                entity.Property<string>("LicensePlateUnique")
                    .HasMaxLength(20)
                    .HasColumnType("varchar(20)")
                    .HasComputedColumnSql("CASE WHEN (`IsDeleted` = 0) THEN `LicensePlate` ELSE NULL END", stored: true);

                entity.HasIndex("LicensePlateUnique")
                    .IsUnique();
                    
                entity.HasIndex(e => e.VIN)
                    .IsUnique();

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Vehicles)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Service configuration
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category).HasMaxLength(50);
            });

            // ServiceOrder configuration
            modelBuilder.Entity<ServiceOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FinalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentStatus).HasMaxLength(50);
                entity.Property(e => e.HandoverLocation).HasMaxLength(200);
                entity.Property(e => e.WarrantyCode).HasMaxLength(50);
                entity.Property(e => e.WarrantyExpiryDate).HasColumnType("datetime(6)");
                
                // Unique index cho OrderNumber (chỉ áp dụng cho bản ghi chưa xóa)
                entity.HasIndex(e => e.OrderNumber)
                    .IsUnique();

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.ServiceOrders)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Vehicle)
                    .WithMany(v => v.ServiceOrders)
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Warranty configuration
            modelBuilder.Entity<Warranty>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WarrantyCode).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.WarrantyCode).IsUnique();
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.HandoverBy).HasMaxLength(50);
                entity.Property(e => e.HandoverLocation).HasMaxLength(200);

                entity.HasOne(e => e.ServiceOrder)
                    .WithMany(o => o.Warranties)
                    .HasForeignKey(e => e.ServiceOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Warranties)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Vehicle)
                    .WithMany(v => v.Warranties)
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<WarrantyItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PartName).HasMaxLength(200);
                entity.Property(e => e.PartNumber).HasMaxLength(100);
                entity.Property(e => e.SerialNumber).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.Warranty)
                    .WithMany(w => w.Items)
                    .HasForeignKey(e => e.WarrantyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Part)
                    .WithMany(p => p.WarrantyItems)
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ServiceOrderPart)
                    .WithMany(p => p.WarrantyItems)
                    .HasForeignKey(e => e.ServiceOrderPartId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<WarrantyClaim>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClaimNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.ClaimNumber).IsUnique();
                entity.Property(e => e.IssueDescription).HasMaxLength(1000);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Resolution).HasMaxLength(1000);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.Warranty)
                    .WithMany(w => w.Claims)
                    .HasForeignKey(e => e.WarrantyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ServiceOrder)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Vehicle)
                    .WithMany()
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ServiceFeeType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                var seedCreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Local);

                entity.HasData(
                    new ServiceFeeType { Id = 1, Code = "LABOR", Name = "Công sửa chữa", Description = "Tiền công kỹ thuật viên", DisplayOrder = 1, IsSystem = true, CreatedAt = seedCreatedAt },
                    new ServiceFeeType { Id = 2, Code = "PARTS", Name = "Phụ tùng", Description = "Chi phí phụ tùng, vật tư", DisplayOrder = 2, IsSystem = true, CreatedAt = seedCreatedAt },
                    new ServiceFeeType { Id = 3, Code = "CONSUMABLE", Name = "Vật tư tiêu hao", Description = "Dầu nhớt, dung dịch, vật tư tiêu hao", DisplayOrder = 3, IsSystem = true, CreatedAt = seedCreatedAt },
                    new ServiceFeeType { Id = 4, Code = "SUBLET", Name = "Dịch vụ ngoài", Description = "Thuê ngoài/đối tác thực hiện", DisplayOrder = 4, IsSystem = true, CreatedAt = seedCreatedAt },
                    new ServiceFeeType { Id = 5, Code = "OTHER", Name = "Phí khác", Description = "Các khoản phí khác", DisplayOrder = 5, IsSystem = true, CreatedAt = seedCreatedAt }
                );
            });

            modelBuilder.Entity<ServiceOrderFee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VatAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ReferenceSource).HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.ServiceOrder)
                    .WithMany(o => o.ServiceOrderFees)
                    .HasForeignKey(e => e.ServiceOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ServiceFeeType)
                    .WithMany(t => t.ServiceOrderFees)
                    .HasForeignKey(e => e.ServiceFeeTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<FeedbackChannel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                var channelSeedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Local);

                entity.HasData(
                    new FeedbackChannel { Id = 1, Code = "HOTLINE", Name = "Hotline", Description = "Gọi hotline/điện thoại", DisplayOrder = 1, IsSystem = true, IsActive = true, CreatedAt = channelSeedDate },
                    new FeedbackChannel { Id = 2, Code = "IN_PERSON", Name = "Tại gara", Description = "Khách phản hồi trực tiếp tại gara", DisplayOrder = 2, IsSystem = true, IsActive = true, CreatedAt = channelSeedDate },
                    new FeedbackChannel { Id = 3, Code = "EMAIL", Name = "Email", Description = "Phản hồi qua email", DisplayOrder = 3, IsSystem = true, IsActive = true, CreatedAt = channelSeedDate },
                    new FeedbackChannel { Id = 4, Code = "APP", Name = "Ứng dụng di động", Description = "Phản hồi qua app", DisplayOrder = 4, IsSystem = true, IsActive = true, CreatedAt = channelSeedDate },
                    new FeedbackChannel { Id = 5, Code = "SOCIAL", Name = "Mạng xã hội", Description = "Phản hồi Facebook/Zalo...", DisplayOrder = 5, IsSystem = true, IsActive = true, CreatedAt = channelSeedDate },
                    new FeedbackChannel { Id = 6, Code = "SURVEY", Name = "Survey hậu mãi", Description = "Khảo sát hậu mãi", DisplayOrder = 6, IsSystem = true, IsActive = true, CreatedAt = channelSeedDate }
                );
            });

            modelBuilder.Entity<CustomerFeedback>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Source).HasMaxLength(100);
                entity.Property(e => e.Rating).HasMaxLength(50);
                entity.Property(e => e.Topic).HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.ActionTaken).HasMaxLength(1000);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.CustomerFeedbacks)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.ServiceOrder)
                    .WithMany(o => o.CustomerFeedbacks)
                    .HasForeignKey(e => e.ServiceOrderId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.FollowUpBy)
                    .WithMany()
                    .HasForeignKey(e => e.FollowUpById)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.FeedbackChannel)
                    .WithMany(c => c.Feedbacks)
                    .HasForeignKey(e => e.FeedbackChannelId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.CustomerId, e.ServiceOrderId, e.Status });
                entity.HasIndex(e => e.FollowUpDate);
                entity.HasIndex(e => e.Source);
            });

            modelBuilder.Entity<CustomerFeedbackAttachment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).HasMaxLength(200);
                entity.Property(e => e.FilePath).HasMaxLength(500);
                entity.Property(e => e.ContentType).HasMaxLength(100);

                entity.HasOne(e => e.CustomerFeedback)
                    .WithMany(f => f.Attachments)
                    .HasForeignKey(e => e.CustomerFeedbackId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ServiceOrderItem configuration
            modelBuilder.Entity<ServiceOrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.ServiceOrder)
                    .WithMany(so => so.ServiceOrderItems)
                    .HasForeignKey(e => e.ServiceOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Service)
                    .WithMany(s => s.ServiceOrderItems)
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // MaterialRequest configuration
            modelBuilder.Entity<MaterialRequest>(entity =>
            {
                entity.HasIndex(e => e.MRNumber).IsUnique();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.HasOne(e => e.ServiceOrder)
                      .WithMany()
                      .HasForeignKey(e => e.ServiceOrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MaterialRequestItem>(entity =>
            {
                entity.HasOne(i => i.MaterialRequest)
                      .WithMany(mr => mr.Items)
                      .HasForeignKey(i => i.MaterialRequestId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(i => i.Part)
                      .WithMany()
                      .HasForeignKey(i => i.PartId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Department configuration
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.IsActive).IsRequired();
            });

            // Position configuration
            modelBuilder.Entity<Position>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.IsActive).IsRequired();
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Position).HasMaxLength(50);
                entity.Property(e => e.Department).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Skills).HasMaxLength(1000);
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");

                // Foreign key relationships
                entity.HasOne(e => e.DepartmentNavigation)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.PositionNavigation)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(e => e.PositionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // VehicleInspection configuration
            modelBuilder.Entity<VehicleInspection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InspectionNumber).IsRequired().HasMaxLength(50);
                
                // Unique index cho InspectionNumber (chỉ áp dụng cho bản ghi chưa xóa)
                entity.HasIndex(e => e.InspectionNumber)
                    .IsUnique();
                entity.Property(e => e.InspectionType).HasMaxLength(50);
                entity.Property(e => e.FuelLevel).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(20);

                entity.HasOne(e => e.Vehicle)
                    .WithMany(v => v.Inspections)
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.VehicleInspections)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Inspector)
                    .WithMany(emp => emp.PerformedInspections)
                    .HasForeignKey(e => e.InspectorId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // InspectionIssue configuration
            modelBuilder.Entity<InspectionIssue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IssueName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Severity).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.EstimatedCost).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.VehicleInspection)
                    .WithMany(vi => vi.Issues)
                    .HasForeignKey(e => e.VehicleInspectionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.SuggestedService)
                    .WithMany(s => s.RelatedInspectionIssues)
                    .HasForeignKey(e => e.SuggestedServiceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // InspectionPhoto configuration
            modelBuilder.Entity<InspectionPhoto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FileName).HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(100);

                entity.HasOne(e => e.VehicleInspection)
                    .WithMany(vi => vi.Photos)
                    .HasForeignKey(e => e.VehicleInspectionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.InspectionIssue)
                    .WithMany()
                    .HasForeignKey(e => e.InspectionIssueId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ✅ 2.3.2: AdditionalIssue configuration
            modelBuilder.Entity<AdditionalIssue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IssueName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Severity).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.TechnicianNotes).HasMaxLength(1000);

                entity.HasOne(e => e.ServiceOrder)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ServiceOrderItem)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceOrderItemId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.ReportedByEmployee)
                    .WithMany()
                    .HasForeignKey(e => e.ReportedByEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AdditionalQuotation)
                    .WithMany()
                    .HasForeignKey(e => e.AdditionalQuotationId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.AdditionalServiceOrder)
                    .WithMany()
                    .HasForeignKey(e => e.AdditionalServiceOrderId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ✅ 2.3.2: AdditionalIssuePhoto configuration
            modelBuilder.Entity<AdditionalIssuePhoto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FileName).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.AdditionalIssue)
                    .WithMany()
                    .HasForeignKey(e => e.AdditionalIssueId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ✅ 2.4.2: QualityControl configuration
            modelBuilder.Entity<QualityControl>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QCResult).IsRequired().HasMaxLength(20);
                entity.Property(e => e.QCNotes).HasMaxLength(2000);
                entity.Property(e => e.ReworkNotes).HasMaxLength(2000);

                entity.HasOne(e => e.ServiceOrder)
                    .WithMany(so => so.QualityControls)
                    .HasForeignKey(e => e.ServiceOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.QCInspector)
                    .WithMany()
                    .HasForeignKey(e => e.QCInspectorId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ✅ 2.4.2: QCChecklistItem configuration
            modelBuilder.Entity<QCChecklistItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ChecklistItemName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Result).HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasOne(e => e.QualityControl)
                    .WithMany(qc => qc.QCChecklistItems)
                    .HasForeignKey(e => e.QualityControlId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ✅ OPTIMIZED: QCChecklistTemplate configuration
            modelBuilder.Entity<QCChecklistTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TemplateName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.VehicleType).HasMaxLength(50);
                entity.Property(e => e.ServiceType).HasMaxLength(50);

                // Index for faster lookup
                entity.HasIndex(e => new { e.IsDefault, e.IsActive });
                entity.HasIndex(e => new { e.VehicleType, e.ServiceType, e.IsActive });
            });

            // ✅ OPTIMIZED: QCChecklistTemplateItem configuration
            modelBuilder.Entity<QCChecklistTemplateItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ChecklistItemName).IsRequired().HasMaxLength(200);

                entity.HasOne(e => e.Template)
                    .WithMany(t => t.TemplateItems)
                    .HasForeignKey(e => e.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index for ordering
                entity.HasIndex(e => new { e.TemplateId, e.DisplayOrder });
            });

            // ServiceQuotation configuration
            modelBuilder.Entity<ServiceQuotation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuotationNumber).IsRequired().HasMaxLength(50);
                
                // Unique index cho QuotationNumber (chỉ áp dụng cho bản ghi chưa xóa)
                entity.HasIndex(e => e.QuotationNumber)
                    .IsUnique();
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                
                // Quotation Type and Insurance/Company fields
                entity.Property(e => e.QuotationType).HasMaxLength(20).HasDefaultValue("Personal");
                
                // Insurance specific fields
                entity.Property(e => e.MaxInsuranceAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Deductible).HasColumnType("decimal(18,2)");
                entity.Property(e => e.InsuranceApprovedAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.InsuranceApprovalNotes).HasMaxLength(1000);
                entity.Property(e => e.InsuranceAdjusterContact).HasMaxLength(200);
                
                // Company specific fields
                entity.Property(e => e.PONumber).HasMaxLength(50);
                entity.Property(e => e.PaymentTerms).HasMaxLength(20).HasDefaultValue("Cash");
                entity.Property(e => e.CompanyApprovedBy).HasMaxLength(100);
                entity.Property(e => e.CompanyApprovalNotes).HasMaxLength(1000);
                entity.Property(e => e.CompanyContactPerson).HasMaxLength(100);

                entity.HasOne(e => e.VehicleInspection)
                    .WithOne(vi => vi.Quotation)
                    .HasForeignKey<ServiceQuotation>(e => e.VehicleInspectionId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.ServiceQuotations)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Vehicle)
                    .WithMany(v => v.Quotations)
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PreparedBy)
                    .WithMany(emp => emp.PreparedQuotations)
                    .HasForeignKey(e => e.PreparedById)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.ServiceOrder)
                    .WithOne(so => so.ServiceQuotation)
                    .HasForeignKey<ServiceOrder>(so => so.ServiceQuotationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // QuotationItem configuration
            modelBuilder.Entity<QuotationItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ItemName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.ServiceQuotation)
                    .WithMany(sq => sq.Items)
                    .HasForeignKey(e => e.ServiceQuotationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Service)
                    .WithMany(s => s.QuotationItems)
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.InspectionIssue)
                    .WithMany()
                    .HasForeignKey(e => e.InspectionIssueId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ServiceOrder relationships with new entities
            modelBuilder.Entity<ServiceOrder>(entity =>
            {
                entity.Property(e => e.ServiceTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PartsTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AmountPaid).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AmountRemaining).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.VehicleInspection)
                    .WithMany()
                    .HasForeignKey(e => e.VehicleInspectionId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.PrimaryTechnician)
                    .WithMany(emp => emp.AssignedServiceOrders)
                    .HasForeignKey(e => e.PrimaryTechnicianId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Part configuration
            modelBuilder.Entity<Part>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PartNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PartName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CostPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SellPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Sku).HasMaxLength(100);
                entity.Property(e => e.Barcode).HasMaxLength(150);
                entity.Property(e => e.DefaultUnit).HasMaxLength(20);
                
                // Unique index cho PartNumber (chỉ áp dụng cho bản ghi chưa xóa)
                entity.HasIndex(e => e.PartNumber)
                    .IsUnique();
                    // Note: MySQL filtered index syntax is different, handled at database level

                entity.HasIndex(e => e.Sku)
                    .IsUnique();

                entity.HasIndex(e => e.Barcode)
                    .IsUnique();
            });

            modelBuilder.Entity<PartUnit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ConversionRate).HasColumnType("decimal(18,4)");
                entity.Property(e => e.Barcode).HasMaxLength(150);
                entity.Property(e => e.Notes).HasMaxLength(200);

                entity.HasIndex(e => new { e.PartId, e.UnitName })
                    .IsUnique();

                entity.HasOne(e => e.Part)
                    .WithMany(p => p.PartUnits)
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Barcode)
                    .IsUnique();
            });

            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(300);
                entity.Property(e => e.Address).HasMaxLength(300);
                entity.Property(e => e.ManagerName).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(50);

                entity.HasIndex(e => e.Code)
                    .IsUnique();
            });

            modelBuilder.Entity<WarehouseZone>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(300);

                entity.HasIndex(e => new { e.WarehouseId, e.Code })
                    .IsUnique();

                entity.HasOne(e => e.Warehouse)
                    .WithMany(w => w.Zones)
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WarehouseBin>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(300);
                entity.Property(e => e.Capacity).HasColumnType("decimal(18,2)");

                entity.HasIndex(e => new { e.WarehouseId, e.Code })
                    .IsUnique();

                entity.HasOne(e => e.Warehouse)
                    .WithMany(w => w.Bins)
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.WarehouseZone)
                    .WithMany(z => z.Bins)
                    .HasForeignKey(e => e.WarehouseZoneId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<PartInventoryBatch>(entity =>
            {
                entity.HasOne(e => e.Warehouse)
                    .WithMany(w => w.InventoryBatches)
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.WarehouseZone)
                    .WithMany(z => z.InventoryBatches)
                    .HasForeignKey(e => e.WarehouseZoneId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.WarehouseBin)
                    .WithMany(b => b.InventoryBatches)
                    .HasForeignKey(e => e.WarehouseBinId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Supplier configuration
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SupplierCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SupplierName).IsRequired().HasMaxLength(200);
                
                // Unique index cho SupplierCode (chỉ áp dụng cho bản ghi chưa xóa)
                entity.HasIndex(e => e.SupplierCode)
                    .IsUnique();
                    // Note: MySQL filtered index syntax is different, handled at database level
            });

            // StockTransaction configuration
            modelBuilder.Entity<StockTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TransactionNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                
                // Unique index cho TransactionNumber (chỉ áp dụng cho bản ghi chưa xóa)
                entity.HasIndex(e => e.TransactionNumber)
                    .IsUnique();

                entity.HasOne(e => e.Part)
                    .WithMany(p => p.StockTransactions)
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Supplier)
                    .WithMany(s => s.StockTransactions)
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.ProcessedBy)
                    .WithMany(emp => emp.ProcessedStockTransactions)
                    .HasForeignKey(e => e.ProcessedById)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ServiceOrderPart configuration
            modelBuilder.Entity<ServiceOrderPart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.ServiceOrder)
                    .WithMany(so => so.ServiceOrderParts)
                    .HasForeignKey(e => e.ServiceOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Part)
                    .WithMany(p => p.ServiceOrderParts)
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ServiceOrderItem)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceOrderItemId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // PaymentTransaction configuration
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReceiptNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                
                // Unique index cho ReceiptNumber (chỉ áp dụng cho bản ghi chưa xóa)
                entity.HasIndex(e => e.ReceiptNumber)
                    .IsUnique();

                entity.HasOne(e => e.ServiceOrder)
                    .WithMany(so => so.PaymentTransactions)
                    .HasForeignKey(e => e.ServiceOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ReceivedBy)
                    .WithMany(emp => emp.ReceivedPayments)
                    .HasForeignKey(e => e.ReceivedById)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Appointment configuration
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AppointmentNumber).IsRequired().HasMaxLength(50);
                
                // Unique index cho AppointmentNumber (chỉ áp dụng cho bản ghi chưa xóa)
                entity.HasIndex(e => e.AppointmentNumber)
                    .IsUnique();

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Appointments)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Vehicle)
                    .WithMany(v => v.Appointments)
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.AssignedTo)
                    .WithMany(emp => emp.AssignedAppointments)
                    .HasForeignKey(e => e.AssignedToId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // QuotationItem - add Part relationship
            modelBuilder.Entity<QuotationItem>(entity =>
            {
                entity.HasOne(e => e.Part)
                    .WithMany(p => p.QuotationItems)
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Services
            modelBuilder.Entity<Service>().HasData(
                new Service
                {
                    Id = 1,
                    Name = "Thay dầu động cơ",
                    Description = "Thay dầu động cơ và lọc dầu",
                    Price = 200000,
                    Duration = 30,
                    Category = "Bảo dưỡng",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Id = 2,
                    Name = "Kiểm tra phanh",
                    Description = "Kiểm tra và bảo dưỡng hệ thống phanh",
                    Price = 150000,
                    Duration = 45,
                    Category = "An toàn",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Id = 3,
                    Name = "Thay lốp",
                    Description = "Thay lốp xe và cân bằng",
                    Price = 300000,
                    Duration = 60,
                    Category = "Lốp xe",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Id = 4,
                    Name = "Sửa chữa động cơ",
                    Description = "Chẩn đoán và sửa chữa động cơ",
                    Price = 500000,
                    Duration = 120,
                    Category = "Sửa chữa",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );

            // Seed Departments
            modelBuilder.Entity<Department>().HasData(
                new Department
                {
                    Id = 1,
                    Name = "Dịch Vụ",
                    Description = "Bộ phận dịch vụ sửa chữa và bảo dưỡng xe",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Department
                {
                    Id = 2,
                    Name = "Phụ Tùng",
                    Description = "Bộ phận quản lý phụ tùng và linh kiện",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Department
                {
                    Id = 3,
                    Name = "Hành Chính",
                    Description = "Bộ phận hành chính và quản lý",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Department
                {
                    Id = 4,
                    Name = "Kế Toán",
                    Description = "Bộ phận kế toán và tài chính",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Department
                {
                    Id = 5,
                    Name = "Chăm Sóc Khách Hàng",
                    Description = "Bộ phận chăm sóc và hỗ trợ khách hàng",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Department
                {
                    Id = 6,
                    Name = "Quản Lý",
                    Description = "Bộ phận quản lý và điều hành",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );

            // Seed Positions
            modelBuilder.Entity<Position>().HasData(
                new Position
                {
                    Id = 1,
                    Name = "Kỹ Thuật Viên",
                    Description = "Thực hiện sửa chữa và bảo dưỡng xe",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Position
                {
                    Id = 2,
                    Name = "Kỹ Thuật Viên Cao Cấp",
                    Description = "Kỹ thuật viên có kinh nghiệm cao",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Position
                {
                    Id = 3,
                    Name = "Chuyên Viên Phụ Tùng",
                    Description = "Quản lý và tư vấn phụ tùng",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Position
                {
                    Id = 4,
                    Name = "Tư Vấn Dịch Vụ",
                    Description = "Tư vấn và hỗ trợ khách hàng",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Position
                {
                    Id = 5,
                    Name = "Lễ Tân",
                    Description = "Tiếp đón và hỗ trợ khách hàng",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Position
                {
                    Id = 6,
                    Name = "Kế Toán",
                    Description = "Xử lý công việc kế toán",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Position
                {
                    Id = 7,
                    Name = "Quản Lý",
                    Description = "Quản lý và điều hành",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Position
                {
                    Id = 8,
                    Name = "Trợ Lý",
                    Description = "Hỗ trợ công việc quản lý",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Position
                {
                    Id = 9,
                    Name = "Giám Sát",
                    Description = "Giám sát hoạt động sửa chữa",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );

            // Seed Employees
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    Name = "Nguyễn Văn A",
                    Phone = "0123456789",
                    Email = "nguyenvana@garage.com",
                    Position = "Thợ sửa chữa",
                    Department = "Kỹ thuật",
                    HireDate = DateTime.Now.AddYears(-2),
                    Salary = 8000000,
                    Status = "Active",
                    Skills = "Sửa chữa động cơ, Thay dầu, Kiểm tra phanh",
                    CreatedAt = DateTime.Now
                },
                new Employee
                {
                    Id = 2,
                    Name = "Trần Thị B",
                    Phone = "0987654321",
                    Email = "tranthib@garage.com",
                    Position = "Thợ lốp",
                    Department = "Kỹ thuật",
                    HireDate = DateTime.Now.AddYears(-1),
                    Salary = 7000000,
                    Status = "Active",
                    Skills = "Thay lốp, Cân bằng, Sửa chữa lốp",
                    CreatedAt = DateTime.Now
                }
            );

            // InsuranceInvoice Configuration
            modelBuilder.Entity<InsuranceInvoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InsuranceCompany).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PolicyNumber).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ClaimNumber).HasMaxLength(100).IsRequired();
                entity.Property(e => e.AccidentLocation).HasMaxLength(500).IsRequired();
                entity.Property(e => e.LicensePlate).HasMaxLength(20).IsRequired();
                entity.Property(e => e.VehicleModel).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasOne(e => e.ServiceOrder)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceOrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // InsuranceInvoiceItem Configuration
            modelBuilder.Entity<InsuranceInvoiceItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ItemName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.ItemType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.InsuranceInvoice)
                    .WithMany(ii => ii.Items)
                    .HasForeignKey(e => e.InsuranceInvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Invoice Configuration - Self-referencing relationships
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // ReplacesInvoice relationship
                entity.HasOne(e => e.ReplacesInvoice)
                    .WithMany(i => i.ReplacedByInvoices)
                    .HasForeignKey(e => e.ReplacesInvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // AdjustmentForInvoice relationship
                entity.HasOne(e => e.AdjustmentForInvoice)
                    .WithMany(i => i.AdjustmentInvoices)
                    .HasForeignKey(e => e.AdjustmentForInvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // InsuranceClaim Configuration
            modelBuilder.Entity<InsuranceClaim>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // InsuranceClaim -> Invoice relationship (many-to-one)
                entity.HasOne(e => e.Invoice)
                    .WithMany()
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // VehicleInspection Configuration - Use TEXT for long string fields
            modelBuilder.Entity<VehicleInspection>(entity =>
            {
                entity.Property(e => e.GeneralCondition).HasColumnType("TEXT");
                entity.Property(e => e.ExteriorCondition).HasColumnType("TEXT");
                entity.Property(e => e.InteriorCondition).HasColumnType("TEXT");
                entity.Property(e => e.EngineCondition).HasColumnType("TEXT");
                entity.Property(e => e.BrakeCondition).HasColumnType("TEXT");
                entity.Property(e => e.SuspensionCondition).HasColumnType("TEXT");
                entity.Property(e => e.TireCondition).HasColumnType("TEXT");
                entity.Property(e => e.Findings).HasColumnType("TEXT");
                entity.Property(e => e.CustomerComplaints).HasColumnType("TEXT");
                entity.Property(e => e.Recommendations).HasColumnType("TEXT");
                entity.Property(e => e.TechnicianNotes).HasColumnType("TEXT");
                entity.Property(e => e.Notes).HasColumnType("TEXT");
            });

            // ServiceQuotation Configuration - Use TEXT for long fields
            modelBuilder.Entity<ServiceQuotation>(entity =>
            {
                entity.Property(e => e.Description).HasColumnType("TEXT");
                entity.Property(e => e.Terms).HasColumnType("TEXT");
                entity.Property(e => e.CustomerNotes).HasColumnType("TEXT");
                entity.Property(e => e.RejectionReason).HasColumnType("TEXT");
                entity.Property(e => e.Notes).HasColumnType("TEXT");
                
                // ✅ 2.3.3: Configure relationship to parent ServiceOrder (for additional quotations)
                entity.HasOne(e => e.RelatedToServiceOrder)
                    .WithMany()
                    .HasForeignKey(e => e.RelatedToServiceOrderId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ServiceOrder Configuration - Use TEXT for long fields
            modelBuilder.Entity<ServiceOrder>(entity =>
            {
                entity.Property(e => e.Notes).HasColumnType("TEXT");
                entity.Property(e => e.Description).HasColumnType("TEXT");
                
                // ✅ 2.3.3: Configure self-referencing relationship (Parent -> Additional Orders)
                entity.HasOne(e => e.ParentServiceOrder)
                    .WithMany(so => so.AdditionalServiceOrders)
                    .HasForeignKey(e => e.ParentServiceOrderId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
            });

            // InvoiceItem Configuration - Use TEXT for long fields
            modelBuilder.Entity<InvoiceItem>(entity =>
            {
                entity.Property(e => e.Description).HasColumnType("TEXT");
            });
        }

        /// <summary>
        /// Override SaveChanges to automatically set audit fields (synchronous version)
        /// </summary>
        public override int SaveChanges()
        {
            ApplyAuditInformation();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically set audit fields (asynchronous version)
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Apply audit information to all tracked entities
        /// </summary>
        private void ApplyAuditInformation()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified ||
                    e.State == EntityState.Deleted));

            var currentUser = GetCurrentUser();
            var currentTime = DateTime.Now;

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = currentTime;
                        entity.CreatedBy = currentUser;
                        entity.UpdatedAt = currentTime;
                        entity.UpdatedBy = currentUser;
                        entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        // Don't update CreatedAt and CreatedBy
                        entity.UpdatedAt = currentTime;
                        entity.UpdatedBy = currentUser;
                        break;

                    case EntityState.Deleted:
                        // Soft delete
                        entry.State = EntityState.Modified;
                        entity.IsDeleted = true;
                        entity.DeletedAt = currentTime;
                        entity.DeletedBy = currentUser;
                        entity.UpdatedAt = currentTime;
                        entity.UpdatedBy = currentUser;
                        break;
                }
            }
        }

        /// <summary>
        /// Get current user from HttpContext
        /// </summary>
        private string? GetCurrentUser()
        {
            if (_httpContextAccessor?.HttpContext == null)
                return "System";

            var user = _httpContextAccessor.HttpContext.User;
            
            if (user?.Identity?.IsAuthenticated == true)
            {
                // Try to get user ID or name from claims
                return user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? user.FindFirst(ClaimTypes.Name)?.Value 
                    ?? user.FindFirst("sub")?.Value 
                    ?? "Anonymous";
            }

            return "Anonymous";
        }

        // Customer Reception configuration
        private void ConfigureCustomerReception(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerReception>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReceptionNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CustomerRequest).HasMaxLength(1000);
                entity.Property(e => e.CustomerComplaints).HasMaxLength(1000);
                entity.Property(e => e.ReceptionNotes).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.InsuranceCompany).HasMaxLength(100);
                entity.Property(e => e.InsurancePolicyNumber).HasMaxLength(50);
                entity.Property(e => e.EmergencyContact).HasMaxLength(20);
                entity.Property(e => e.EmergencyContactName).HasMaxLength(200);
                
                // Cached information
                entity.Property(e => e.CustomerName).HasMaxLength(200);
                entity.Property(e => e.CustomerPhone).HasMaxLength(20);
                entity.Property(e => e.VehiclePlate).HasMaxLength(20);
                entity.Property(e => e.VehicleMake).HasMaxLength(50);
                entity.Property(e => e.VehicleModel).HasMaxLength(50);

                // Unique index cho ReceptionNumber
                entity.HasIndex(e => e.ReceptionNumber)
                    .IsUnique();

                // Foreign key relationships
                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Vehicle)
                    .WithMany()
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssignedTechnician)
                    .WithMany()
                    .HasForeignKey(e => e.AssignedTechnicianId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.VehicleInspection)
                    .WithOne(i => i.CustomerReception)
                    .HasForeignKey<VehicleInspection>(i => i.CustomerReceptionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ✅ Phase 4.1 - Sprint 2: InventoryCheck configuration
            // Inventory Adjustment configuration
            modelBuilder.Entity<InventoryAdjustment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AdjustmentNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.AdjustmentNumber).IsUnique();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.Reason).HasMaxLength(1000);
                entity.Property(e => e.RejectionReason).HasMaxLength(1000);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.AdjustmentDate).IsRequired();

                entity.HasOne(e => e.InventoryCheck)
                    .WithMany()
                    .HasForeignKey(e => e.InventoryCheckId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Warehouse)
                    .WithMany()
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.WarehouseZone)
                    .WithMany()
                    .HasForeignKey(e => e.WarehouseZoneId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.WarehouseBin)
                    .WithMany()
                    .HasForeignKey(e => e.WarehouseBinId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.ApprovedByEmployee)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedByEmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<InventoryAdjustmentItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.QuantityChange).IsRequired();
                entity.Property(e => e.SystemQuantityBefore).IsRequired();
                entity.Property(e => e.SystemQuantityAfter).IsRequired();

                entity.HasOne(e => e.InventoryAdjustment)
                    .WithMany(a => a.Items)
                    .HasForeignKey(e => e.InventoryAdjustmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Part)
                    .WithMany()
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.InventoryCheckItem)
                    .WithOne(ici => ici.InventoryAdjustmentItem)
                    .HasForeignKey<InventoryAdjustmentItem>(e => e.InventoryCheckItemId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<InventoryCheck>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(500);

                // ✅ NOTE: Unique index for Code - không filter IsDeleted vì có thể reuse code sau khi soft delete
                // Nếu cần filter IsDeleted, cần sử dụng unique constraint với filter
                entity.HasIndex(e => e.Code).IsUnique();

                // Foreign keys
                entity.HasOne(e => e.Warehouse)
                    .WithMany()
                    .HasForeignKey(e => e.WarehouseId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.WarehouseZone)
                    .WithMany()
                    .HasForeignKey(e => e.WarehouseZoneId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.WarehouseBin)
                    .WithMany()
                    .HasForeignKey(e => e.WarehouseBinId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.StartedByEmployee)
                    .WithMany()
                    .HasForeignKey(e => e.StartedByEmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.CompletedByEmployee)
                    .WithMany()
                    .HasForeignKey(e => e.CompletedByEmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ✅ Phase 4.1 - Sprint 2: InventoryCheckItem configuration
            modelBuilder.Entity<InventoryCheckItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Notes).HasMaxLength(500);

                // Foreign keys
                entity.HasOne(e => e.InventoryCheck)
                    .WithMany(ic => ic.Items)
                    .HasForeignKey(e => e.InventoryCheckId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Part)
                    .WithMany()
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.InventoryAdjustmentItem)
                    .WithOne(iai => iai.InventoryCheckItem)
                    .HasForeignKey<InventoryCheckItem>(e => e.InventoryAdjustmentItemId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Index for faster lookup
                entity.HasIndex(e => e.InventoryCheckId);
                entity.HasIndex(e => e.PartId);
                entity.HasIndex(e => e.IsDiscrepancy);
            });

            // ✅ Phase 4.1 - Advanced Features: Comments/Notes Timeline configuration
            modelBuilder.Entity<InventoryCheckComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CommentText).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.CreatedByUserName).HasMaxLength(100);

                entity.HasOne(e => e.InventoryCheck)
                    .WithMany(ic => ic.Comments)
                    .HasForeignKey(e => e.InventoryCheckId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedByEmployee)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByEmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.InventoryCheckId);
                entity.HasIndex(e => e.CreatedAt);
            });

            modelBuilder.Entity<InventoryAdjustmentComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CommentText).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.CreatedByUserName).HasMaxLength(100);

                entity.HasOne(e => e.InventoryAdjustment)
                    .WithMany(ia => ia.Comments)
                    .HasForeignKey(e => e.InventoryAdjustmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedByEmployee)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByEmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.InventoryAdjustmentId);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
