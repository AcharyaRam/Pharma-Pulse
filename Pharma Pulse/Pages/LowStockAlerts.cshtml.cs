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

        public List<Medicine> LowStockMedicines { get; set; } = new();

        public LowStockAlertsModel(MedicineService service)
        {
            _service = service;
        }


        public void OnGet()
        {
            // Load same common medicines list
            var allMedicines = _service.GetAllMedicines();


            // Filter only Low Stock Medicines (Stock <= 10)
            LowStockMedicines = allMedicines
                .Where(m => m.StockUnits <= m.LowStockLimit)
                .ToList();
        }
    }
}
