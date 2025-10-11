using System;
using System.Collections.Generic;

namespace GarageManagementSystem.IdentityServer.Models;

public partial class SoftDeleteRecord
{
    public int Id { get; set; }

    public string EntityType { get; set; } = null!;

    public string EntityName { get; set; } = null!;

    public DateTime DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public string? Reason { get; set; }
}
