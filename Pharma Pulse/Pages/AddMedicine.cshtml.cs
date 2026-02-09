using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;

namespace Pharma_Pulse.Pages
{
    public class AddMedicineModel : PageModel
    {
        [BindProperty]
        public Medicine Medicine { get; set; }

        public string SuccessMessage { get; set; }

        public void OnGet()
        {
            Medicine = new Medicine();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            // ✅ Add medicine into MedicineService list (temporary)
            MedicineService.AddMedicine(Medicine);

            SuccessMessage = "Medicine Added Successfully!";

            return Page();
        }
    }
}
