using System;
using System.Collections.Generic;

namespace Pharma_Pulse.Models
{
    public class Bill
    {
        public int Id { get; set; }

        public string InvoiceNumber { get; set; }

        public string CustomerName { get; set; }

        public string MobileNumber { get; set; }

        public string DoctorName { get; set; }

        public decimal SubTotal { get; set; }

        public decimal GstPercent { get; set; }

        public decimal CGST { get; set; }

        public decimal SGST { get; set; }

        public decimal GrandTotal { get; set; }

        public DateTime BillDate { get; set; } = DateTime.Now;

        // Navigation
        public List<BillDetail> BillDetails { get; set; }
    }
}
