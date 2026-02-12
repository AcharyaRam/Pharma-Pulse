using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;

namespace Pharma_Pulse.Pages
{
    public class AddMedicineModel : PageModel
    {
        private readonly MedicineService _service;

        public AddMedicineModel(MedicineService service)
        {
            _service = service;
        }

        [BindProperty]
        public Medicine Medicine { get; set; }

        public string SuccessMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (Medicine.SellType == "Unit")
            {
                Medicine.UnitsPerStrip = 1;
            }

            _service.AddMedicine(Medicine);

            SuccessMessage = "Medicine Added Successfully!";

            return RedirectToPage();
        }
    }
}
