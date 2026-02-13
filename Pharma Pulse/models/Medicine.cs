using System;

namespace Pharma_Pulse.Models
{
    public class Medicine
    {
        // ✅ Auto Primary Key (Yehi SR No ka kaam karega)
        public int Id { get; set; }

        // ✅ Basic Info
        public string MedicineName { get; set; }
        public string Category { get; set; }

        // ✅ New Required Fields
        public string BatchNo { get; set; }        // Batch Number
        public DateTime MfgDate { get; set; }      // Manufacturing Date
        public string HsnSac { get; set; } = "3004";        // HSN/SAC Code

        // ✅ Expiry Date (Already Existing)
        public DateTime ExpiryDate { get; set; }

        // ✅ Stock Management (Important - Don't Remove)
        public int StockUnits { get; set; }        // Total Stock Units Available

        public int UnitsPerStrip { get; set; }     // Units per Strip/Pack

        // ✅ Selling Mode
        public string SellType { get; set; } = "Tablet";

        // ✅ Low Stock Alert
        public int LowStockLimit { get; set; }

        // ✅ Pricing
        public decimal BuyingPrice { get; set; }
        public decimal SellingPrice { get; set; }
    }
}
