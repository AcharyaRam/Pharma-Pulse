using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using System.Collections.Generic;

namespace Pharma_Pulse.Pages
{
    public class TotalMedicineModel : PageModel
    {
        public List<Medicine> Medicines { get; set; }

        public void OnGet()
        {
            // Load medicines from common service
            Medicines = MedicineService.GetAllMedicines();
        }
    }
}
