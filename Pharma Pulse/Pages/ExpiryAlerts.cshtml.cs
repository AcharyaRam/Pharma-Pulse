using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class ExpiryAlertsModel : PageModel
    {
        // ✅ Inject MedicineService
        private readonly MedicineService _service;

        public ExpiryAlertsModel(MedicineService service)
        {
            _service = service;
        }

        public List<Medicine> ExpiryMedicines { get; set; }

        public void OnGet()
        {
            // ✅ Load medicines from Database
            var allMedicines = _service.GetAllMedicines();

            // Expiry within next 30 days
            ExpiryMedicines = allMedicines
                .Where(m => m.ExpiryDate <= DateTime.Now.AddDays(30))
                .ToList();
        }
    }
}
