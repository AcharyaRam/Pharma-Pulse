using System;

namespace Pharma_Pulse.Models
{
    public class Medicine
    {
        public string MedicineName { get; set; }
        public string Category { get; set; }
        public int Stock { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
