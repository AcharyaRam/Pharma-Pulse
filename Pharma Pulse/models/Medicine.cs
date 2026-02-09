using System;

namespace Pharma_Pulse.Models
{
    public class Medicine
    {
        public string MedicineName { get; set; }
        public string Category { get; set; }
        public int Stock { get; set; }
        public DateTime ExpiryDate { get; set; }

        // ✅ Low stock threshold (har medicine ka alag hoga)
        public int LowStockLimit { get; set; }

        // ✅ Buying Price (Purchase cost)
        public decimal BuyingPrice { get; set; }

        // ✅ Selling Price (Customer selling rate)
        public decimal SellingPrice { get; set; }
    }
}
