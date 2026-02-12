using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class LowStockAlertsModel : PageModel
    {
        private readonly MedicineService _service;

        public LowStockAlertsModel(MedicineService service)
        {
            _service = service;
        }

        public List<Medicine> LowStockMedicines { get; set; }

        public void OnGet()
        {
            // ✅ Load medicines from Database
            var allMedicines = _service.GetAllMedicines();

            // ✅ Filter only Low Stock Medicines
            LowStockMedicines = allMedicines
                .Where(m => m.StockUnits <= m.LowStockLimit)
                .ToList();
        }
    }
}
