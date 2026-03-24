namespace Pharma_Pulse.Models
{
    public class BillItem
    {
        public string MedicineName { get; set; }

        // ✅ NEW Fields (Invoice Important)
        public string BatchNo { get; set; }

        public int PharmacyId { get; set; }

        public DateTime ExpiryDate { get; set; }

        public DateTime MfgDate { get; set; }
        public string HsnSac { get; set; }

        // Existing
        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string SaleMode { get; set; }

        // Auto Total
        public decimal Total => Quantity * Price;
    }
}
