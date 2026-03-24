using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class ExpiryAlertsModel : PharmacyPageModel
    {
        private readonly MedicineService _service;

        public ExpiryAlertsModel(MedicineService service)
        {
            _service = service;
        }

        public List<Medicine> ExpiryMedicines { get; set; }

        // ✅ Search Term
        public string SearchTerm { get; set; }

        // ✅ Pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }

        public void OnGet(string search, int pageNumber = 1)
        {
            SearchTerm = search;
            CurrentPage = pageNumber;

            // ✅ Load All Medicines
            var allMedicines = _service.GetAllMedicines(CurrentPharmacyId);

            // ✅ Step 1: Filter Only Expired + Expiring Soon (Next 2 Months)
            var expiryList = allMedicines
                .Where(m => m.ExpiryDate <= DateTime.Now.AddMonths(1))
                .ToList();

            // ✅ Step 2: Search Filter (StartsWith like LowStock)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                expiryList = expiryList
                    .Where(m =>
                        (
                            !string.IsNullOrEmpty(m.MedicineName) &&
                            m.MedicineName.ToLower().StartsWith(search)
                        )
                        ||
                        (
                            !string.IsNullOrEmpty(m.Category) &&
                            m.Category.ToLower().StartsWith(search)
                        )
                    )
                    .ToList();
            }

            // ✅ Step 3: Pagination
            int pageSize = 8;

            TotalPages = (int)Math.Ceiling(expiryList.Count / (double)pageSize);

            ExpiryMedicines = expiryList
                .Skip((CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}
