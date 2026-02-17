namespace Pharma_Pulse.Models
{
    public class BillDetail
    {
        public int Id { get; set; }

        // Foreign Key
        public int BillId { get; set; }
        public Bill Bill { get; set; }

        public string MedicineName { get; set; }

        // ✅ NEW Fields (Save Invoice Data)
        public string BatchNo { get; set; }

        public DateTime ExpiryDate { get; set; }


        public string HsnSac { get; set; }

        // Existing
        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Total { get; set; }
    }
}
