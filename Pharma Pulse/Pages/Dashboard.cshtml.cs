using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class DashboardModel : PageModel
    {
        public List<Medicine> Medicines { get; set; }

        // ✅ Total medicines count for card
        public int TotalMedicineCount { get; set; }

        // low stock medicine count

        public int LowStockCount { get; set; }

        //lowest stock among all medicine
        public Medicine? LowestStockMedicine { get; set; }

        // expiry count 
        public int ExpiryCount { get; set; }

        //nearest expiry medicine
        public Medicine? NearestExpiryMedicine { get; set; }
        public void OnGet()
        {
            // Load full list once
            var allMedicines = MedicineService.GetAllMedicines();

            // First one to get expire 
            // Expiry medicines count (next 30 days)
            ExpiryCount = allMedicines.Count(m =>
                m.ExpiryDate <= DateTime.Now.AddDays(30)
            );

            // Find medicine which will expire first
            NearestExpiryMedicine = allMedicines
                .OrderBy(m => m.ExpiryDate)
                .FirstOrDefault();


            // ✅ Total count for card
            TotalMedicineCount = allMedicines.Count;

            // Show only top 10 medicines in dashboard table
            Medicines = allMedicines.Take(10).ToList();

            //show low stock medicine on dashboard
            LowStockCount = allMedicines.Count(m => m.Stock <= 10);

            // ✅ Find medicine with lowest stock
            LowestStockMedicine = allMedicines.OrderBy(m => m.Stock).FirstOrDefault();

            // expiry count 
            ExpiryCount = allMedicines.Count(m => m.ExpiryDate <= DateTime.Now.AddDays(30));

        }
    }
}
