namespace GarageManagementSystem.Shared.DTOs
{
    /// <summary>
    /// ✅ 3.1: DTO cho tính toán COGS
    /// </summary>
    public class COGSCalculationDto : BaseDto
    {
        public int ServiceOrderId { get; set; }
        public string CalculationMethod { get; set; } = string.Empty; // 'FIFO' hoặc 'WeightedAverage'
        public decimal TotalCOGS { get; set; }
        public DateTime CalculationDate { get; set; }
        public List<COGSItemDetailDto> ItemDetails { get; set; } = new();
    }

    /// <summary>
    /// ✅ 3.1: DTO cho chi tiết từng vật tư trong tính toán COGS
    /// </summary>
    public class COGSItemDetailDto
    {
        public int PartId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public int QuantityUsed { get; set; }
        public decimal UnitCost { get; set; } // Giá vốn đơn vị
        public decimal TotalCost { get; set; } // Tổng giá vốn
        public string? BatchNumber { get; set; } // Mã lô hàng (cho FIFO)
        public DateTime? BatchReceiveDate { get; set; } // Ngày nhập lô hàng (cho FIFO)
        public string CalculationMethod { get; set; } = string.Empty; // Phương pháp tính cho item này
    }

    /// <summary>
    /// ✅ 3.1: DTO cho breakdown COGS
    /// </summary>
    public class COGSBreakdownDto : BaseDto
    {
        public int ServiceOrderId { get; set; }
        public string CalculationMethod { get; set; } = string.Empty;
        public decimal TotalCOGS { get; set; }
        public DateTime? CalculationDate { get; set; }
        public List<COGSItemDetailDto> ItemDetails { get; set; } = new();
    }

    /// <summary>
    /// ✅ 3.1: DTO cho phương pháp tính COGS
    /// </summary>
    public class COGSMethodDto
    {
        public string Method { get; set; } = string.Empty; // 'FIFO' hoặc 'WeightedAverage'
    }

    /// <summary>
    /// ✅ 3.1: DTO cho kết quả tính lợi nhuận gộp
    /// </summary>
    public class GrossProfitDto
    {
        public int ServiceOrderId { get; set; }
        public decimal TotalRevenue { get; set; } // Tổng doanh thu
        public decimal TotalCOGS { get; set; } // Tổng giá vốn
        public decimal GrossProfit { get; set; } // Lợi nhuận gộp = Revenue - COGS
        public decimal GrossProfitMargin { get; set; } // Tỷ lệ lợi nhuận gộp (%) = (GrossProfit / Revenue) * 100
    }
}

