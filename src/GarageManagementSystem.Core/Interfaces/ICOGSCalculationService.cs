namespace GarageManagementSystem.Core.Interfaces
{
    /// <summary>
    /// Interface cho service tính toán COGS (Cost of Goods Sold)
    /// </summary>
    public interface ICOGSCalculationService
    {
        /// <summary>
        /// Tính COGS cho Service Order theo phương pháp FIFO
        /// </summary>
        /// <param name="serviceOrderId">ID của Service Order</param>
        /// <returns>Kết quả tính toán COGS</returns>
        Task<COGSCalculationResult> CalculateCOGSByFIFOAsync(int serviceOrderId);

        /// <summary>
        /// Tính COGS cho Service Order theo phương pháp Bình quân gia quyền
        /// </summary>
        /// <param name="serviceOrderId">ID của Service Order</param>
        /// <returns>Kết quả tính toán COGS</returns>
        Task<COGSCalculationResult> CalculateCOGSByWeightedAverageAsync(int serviceOrderId);

        /// <summary>
        /// Tính COGS cho Service Order theo phương pháp được chỉ định
        /// </summary>
        /// <param name="serviceOrderId">ID của Service Order</param>
        /// <param name="method">Phương pháp tính ('FIFO' hoặc 'WeightedAverage')</param>
        /// <returns>Kết quả tính toán COGS</returns>
        Task<COGSCalculationResult> CalculateCOGSAsync(int serviceOrderId, string method);

        /// <summary>
        /// Lấy chi tiết breakdown COGS cho Service Order
        /// </summary>
        /// <param name="serviceOrderId">ID của Service Order</param>
        /// <returns>Chi tiết breakdown COGS</returns>
        Task<COGSBreakdownResult> GetCOGSBreakdownAsync(int serviceOrderId);

        /// <summary>
        /// Tính lợi nhuận gộp cho Service Order
        /// </summary>
        /// <param name="serviceOrderId">ID của Service Order</param>
        /// <returns>Lợi nhuận gộp (Gross Profit = Revenue - COGS)</returns>
        Task<GrossProfitResult> CalculateGrossProfitAsync(int serviceOrderId);
    }

    /// <summary>
    /// Kết quả tính toán COGS
    /// </summary>
    public class COGSCalculationResult
    {
        public int ServiceOrderId { get; set; }
        public string CalculationMethod { get; set; } = string.Empty; // 'FIFO' hoặc 'WeightedAverage'
        public decimal TotalCOGS { get; set; }
        public DateTime CalculationDate { get; set; }
        public List<COGSItemDetail> ItemDetails { get; set; } = new();
        public string? BreakdownJson { get; set; } // JSON breakdown để lưu vào DB
    }

    /// <summary>
    /// Chi tiết từng vật tư trong tính toán COGS
    /// </summary>
    public class COGSItemDetail
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
    /// Chi tiết breakdown COGS
    /// </summary>
    public class COGSBreakdownResult
    {
        public int ServiceOrderId { get; set; }
        public string CalculationMethod { get; set; } = string.Empty;
        public decimal TotalCOGS { get; set; }
        public DateTime? CalculationDate { get; set; }
        public List<COGSItemDetail> ItemDetails { get; set; } = new();
    }

    /// <summary>
    /// Kết quả tính lợi nhuận gộp
    /// </summary>
    public class GrossProfitResult
    {
        public int ServiceOrderId { get; set; }
        public decimal TotalRevenue { get; set; } // Tổng doanh thu
        public decimal TotalCOGS { get; set; } // Tổng giá vốn
        public decimal GrossProfit { get; set; } // Lợi nhuận gộp = Revenue - COGS
        public decimal GrossProfitMargin { get; set; } // Tỷ lệ lợi nhuận gộp (%) = (GrossProfit / Revenue) * 100
    }
}

