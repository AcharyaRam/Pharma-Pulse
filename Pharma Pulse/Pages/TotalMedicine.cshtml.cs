using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class TotalMedicineModel : PageModel
    {
        // ✅ Inject MedicineService
        private readonly MedicineService _service;

        public TotalMedicineModel(MedicineService service)
        {
            _service = service;
        }

        // ✅ Medicine List
        public List<Medicine> Medicines { get; set; } = new();

        // ✅ Bind Property for Popup Form
        [BindProperty]
        public Medicine Medicine { get; set; }

        // ✅ Pagination Variables
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // ✅ Search Term
        public string SearchTerm { get; set; }

        // ================================
        // ✅ GET : Load Medicines List
        // ================================
        public void OnGet(int pageNumber = 1, string search = "")
        {
            var allMedicines = _service.GetAllMedicines();

            SearchTerm = search;

            // ✅ Apply Search Filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                allMedicines = allMedicines
                    .Where(m =>
                        !string.IsNullOrEmpty(m.MedicineName) &&
                        m.MedicineName.StartsWith(search, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
            }

            // ✅ Total Pages Calculation
            TotalPages = (int)Math.Ceiling(allMedicines.Count / (double)PageSize);

            // ✅ Current Page Set
            CurrentPage = pageNumber;

            // ✅ Pagination Apply
            Medicines = allMedicines
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        // ================================
        // ✅ POST : SAVE MEDICINE (Popup)
        // ================================
        public IActionResult OnPostSaveMedicine()
        {
            if (!ModelState.IsValid)
            {
                // Reload medicines list if validation fails
                Medicines = _service.GetAllMedicines();
                return Page();
            }

            // ✅ Default Active
            Medicine.IsActive = true;

            // ✅ Save into Database using Service
            _service.AddMedicine(Medicine);

            TempData["Success"] = "Medicine Added Successfully!";

            return RedirectToPage();
        }

        // ================================
        // ✅ POST : Toggle Active/Deactive
        // ================================
        public IActionResult OnPostToggleStatus(int id)
        {
            var medicine = _service.GetAllMedicines()
                .FirstOrDefault(m => m.Id == id);

            if (medicine != null)
            {
                medicine.IsActive = !medicine.IsActive;

                _service.UpdateMedicine(medicine);
            }

            return RedirectToPage();
        }
    }
}
