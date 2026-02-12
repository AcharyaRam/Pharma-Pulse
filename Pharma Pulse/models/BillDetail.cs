namespace Pharma_Pulse.Models
{
    public class BillDetail
    {
        public int Id { get; set; }

        // Foreign Key
        public int BillId { get; set; }
        public Bill Bill { get; set; }

        public string MedicineName { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Total { get; set; }
    }
}
