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

        public List<Medicine> Medicines { get; set; } = new();

        // Pagination Variables
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // ✅ Search Term
        public string SearchTerm { get; set; }

        public void OnGet(int pageNumber = 1, string search = "")
        {
            // ✅ Load medicines from Database
            var allMedicines = _service.GetAllMedicines();

            // ✅ Store search term
            SearchTerm = search;

            // ✅ Apply Search Filter
            if (!string.IsNullOrEmpty(search))
            {
                allMedicines = allMedicines
                    .Where(m =>
                        m.MedicineName.StartsWith(search, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
            }

            // Total Pages Calculation
            TotalPages = (int)Math.Ceiling(allMedicines.Count / (double)PageSize);

            // Current Page Set
            CurrentPage = pageNumber;

            // Pagination Apply
            Medicines = allMedicines
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }
    }
}
