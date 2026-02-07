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
        public List<Medicine> ExpiryMedicines { get; set; }

        public void OnGet()
        {
            var allMedicines = MedicineService.GetAllMedicines();

            // Expiry within next 30 days
            ExpiryMedicines = allMedicines
                .Where(m => m.ExpiryDate <= DateTime.Now.AddDays(30))
                .ToList();
        }
    }
}
