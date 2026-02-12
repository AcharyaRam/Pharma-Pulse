using System;

namespace Pharma_Pulse.Models
{
    public class Sale
    {
        public int Id { get; set; }  // ✅ Primary Key

        public string MedicineName { get; set; }

        public int QuantitySold { get; set; }

        public DateTime SaleDate { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal Profit { get; set; }

        public string InvoiceNumber { get; set; }
    }
}
