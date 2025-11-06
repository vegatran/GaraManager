using System.ComponentModel.DataAnnotations;

namespace GarageManagementSystem.Core.Entities
{
    /// <summary>
    /// ✅ 2.4.2: Quality Control - Kiểm tra chất lượng cho Service Order
    /// </summary>
    public class QualityControl : BaseEntity
    {
        [Required]
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// Nhân viên QC thực hiện kiểm tra
        /// </summary>
        public int? QCInspectorId { get; set; }

        /// <summary>
        /// Ngày kiểm tra QC
        /// </summary>
        [Required]
        public DateTime QCDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Kết quả QC: "Pass" (Đạt), "Fail" (Không đạt)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string QCResult { get; set; } = "Pending"; // "Pending", "Pass", "Fail"

        /// <summary>
        /// Ghi chú QC
        /// </summary>
        [StringLength(2000)]
        public string? QCNotes { get; set; }

        /// <summary>
        /// Cần làm lại không
        /// </summary>
        public bool ReworkRequired { get; set; } = false;

        /// <summary>
        /// Ghi chú làm lại
        /// </summary>
        [StringLength(2000)]
        public string? ReworkNotes { get; set; }

        /// <summary>
        /// Ngày hoàn thành QC (khi có kết quả)
        /// </summary>
        public DateTime? QCCompletedDate { get; set; }

        // Navigation properties
        public virtual ServiceOrder ServiceOrder { get; set; } = null!;
        public virtual Employee? QCInspector { get; set; }
        public virtual ICollection<QCChecklistItem> QCChecklistItems { get; set; } = new List<QCChecklistItem>();
    }
}

