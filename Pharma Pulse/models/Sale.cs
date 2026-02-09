using System;

namespace Pharma_Pulse.Models
{
    public class Sale
    {
        public string MedicineName { get; set; }

        public int QuantitySold { get; set; }

        public DateTime SaleDate { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal Profit { get; set; }
    }
}
