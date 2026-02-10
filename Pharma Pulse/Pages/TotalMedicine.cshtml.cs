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
        public List<Medicine> Medicines { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // ✅ Search Term
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        // Total Count after filtering
        public int TotalCount { get; set; }

        public void OnGet(int pageNumber = 1)
        {
            var allMedicines = MedicineService.GetAllMedicines();

            // ✅ Apply Search Filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                allMedicines = allMedicines
                    .Where(m =>
                        m.MedicineName.StartsWith(SearchTerm, StringComparison.OrdinalIgnoreCase)
                     )
                     .ToList();

            }

            // Total Count after filter
            TotalCount = allMedicines.Count;

            // Pagination Pages
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            CurrentPage = pageNumber;

            // Pagination Apply
            Medicines = allMedicines
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }
    }
}
