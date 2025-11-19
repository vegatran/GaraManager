using System.Text.RegularExpressions;

namespace GarageManagementSystem.Core.Helpers
{
    /// <summary>
    /// ✅ 4.3.2.3: Helper class để parse PaymentTerms thành số ngày credit
    /// </summary>
    public static class PaymentTermsHelper
    {
        /// <summary>
        /// Parse PaymentTerms để lấy số ngày credit (linh hoạt hỗ trợ nhiều format)
        /// </summary>
        /// <param name="paymentTerms">PaymentTerms string (ví dụ: "Net 30", "35", "45 days", "COD", "Prepaid")</param>
        /// <returns>Số ngày credit: "Net 30" → 30, "35" → 35, "45 days" → 45, COD → 0, Prepaid → -1, null → 30 (default)</returns>
        public static int ParsePaymentTermsDays(string? paymentTerms)
        {
            if (string.IsNullOrEmpty(paymentTerms))
                return 30; // Default 30 days

            var trimmed = paymentTerms.Trim();
            var lower = trimmed.ToLowerInvariant();
            
            // ✅ Handle special cases
            if (lower.Contains("cod") || lower.Contains("cash on delivery"))
                return 0; // COD - không có credit terms (thanh toán khi nhận hàng)
            
            if (lower.Contains("prepaid") || lower.Contains("paid in advance"))
                return -1; // Prepaid - đã trả trước (sẽ handle riêng nếu cần)
            
            // ✅ Nếu chỉ là số thuần túy: "30", "35", "45" → parse trực tiếp
            if (int.TryParse(trimmed, out int directDays) && directDays > 0)
            {
                return directDays;
            }
            
            // ✅ Pattern linh hoạt: "Net 30", "Net 35", "30 Days", "35 days", "45 ngày", "30 days net", etc.
            // Tìm số đầu tiên trong chuỗi
            var match = Regex.Match(trimmed, @"(\d+)", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Value, out int days))
            {
                // Hỗ trợ số ngày bất kỳ: 30, 35, 45, 60, 90, etc.
                return days > 0 ? days : 30;
            }

            return 30; // Default 30 days if can't parse
        }
        
        /// <summary>
        /// Parse PaymentTerms hoặc dùng CreditDays trực tiếp (ưu tiên CreditDays nếu có)
        /// </summary>
        /// <param name="paymentTerms">PaymentTerms string</param>
        /// <param name="creditDays">CreditDays trực tiếp (nếu có)</param>
        /// <returns>Số ngày credit: creditDays (nếu có) > ParsePaymentTermsDays(paymentTerms) > 30 (default)</returns>
        public static int GetCreditDays(string? paymentTerms, int? creditDays)
        {
            // ✅ Ưu tiên CreditDays trực tiếp nếu có
            if (creditDays.HasValue && creditDays.Value >= 0)
                return creditDays.Value;
            
            // ✅ Nếu CreditDays = -1 hoặc không có, parse từ PaymentTerms
            if (creditDays.HasValue && creditDays.Value == -1)
                return -1; // Prepaid
            
            return ParsePaymentTermsDays(paymentTerms);
        }
    }
}

