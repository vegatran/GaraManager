using System;
using System.Collections.Generic;
using GarageManagementSystem.IdentityServer.Models;
using Microsoft.EntityFrameworkCore;

namespace GarageManagementSystem.IdentityServer.Data;

public partial class GaraManagementContext : DbContext
{
    public GaraManagementContext()
    {
    }

    public GaraManagementContext(DbContextOptions<GaraManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SoftDeleteRecord> SoftDeleteRecords { get; set; }
    public virtual DbSet<Claim> Claims { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string will be configured via DI in Program.cs
        // This method is only called if no configuration is provided via DI
        if (!optionsBuilder.IsConfigured)
        {
            // Fallback - should not be used in production
            // Connection string should come from appsettings.json via DI
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SoftDeleteRecord>(entity =>
        {
            entity.Property(e => e.DeletedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
