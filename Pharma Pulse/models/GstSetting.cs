using System;

namespace Pharma_Pulse.Models
{
    public class GstSetting
    {
        public int Id { get; set; }

        public decimal GstPercent { get; set; }

        public DateTime UpdatedOn { get; set; } = DateTime.Now;
    }
}
