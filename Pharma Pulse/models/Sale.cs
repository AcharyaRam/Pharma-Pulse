using System;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Pulse.Models
{
    public class Sale
    {
        public int Id { get; set; }  // Primary Key

        [Required]
        public string MedicineName { get; set; } = string.Empty;

        // 🔑 Multi-tenancy key
        public int PharmacyId { get; set; }

        public int QuantitySold { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public decimal Profit { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;
    }
}