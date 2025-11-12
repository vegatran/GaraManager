using System;
using System.Collections.Generic;

namespace GarageManagementSystem.Shared.DTOs
{
    public class ServiceOrderCogsReportItemDto
    {
        public int ServiceOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossMargin { get; set; }
        public string? CogsCalculationMethod { get; set; }
        public DateTime? CogsCalculationDate { get; set; }
    }

    public class ServiceOrderCogsReportDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCogs { get; set; }
        public decimal TotalGrossProfit { get; set; }
        public decimal AverageGrossMargin { get; set; }
        public int TotalOrders { get; set; }
        public List<ServiceOrderCogsReportItemDto> Orders { get; set; } = new();
    }
}

