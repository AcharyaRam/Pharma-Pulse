using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class LowStockAlertsModel : PageModel
    {
        public List<Medicine> LowStockMedicines { get; set; }

        public void OnGet()
        {
            // Load same common medicines list
            var allMedicines = MedicineService.GetAllMedicines();

            // Filter only Low Stock Medicines (Stock <= 10)
            LowStockMedicines = allMedicines
                .Where(m => m.Stock <= m.LowStockLimit)
                .ToList();
        }
    }
}
