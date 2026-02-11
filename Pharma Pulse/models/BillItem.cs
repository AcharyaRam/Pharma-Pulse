namespace Pharma_Pulse.Models
{
    public class BillItem
    {
        public string MedicineName { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string SaleMode { get; set; }


        public decimal Total => Quantity * Price;
    }
}
