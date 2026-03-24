using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class LowStockAlertsModel : PharmacyPageModel
    {
        private readonly MedicineService _service;

        public LowStockAlertsModel(MedicineService service)
        {
            _service = service;
        }

        public List<Medicine> LowStockMedicines { get; set; } = new();

        public string SearchTerm { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }

        public void OnGet(string search, int pageNumber = 1)
        {
            SearchTerm = search;
            CurrentPage = pageNumber;

            // ✅ Already filtered by PharmacyId
            var allMedicines = _service.GetAllMedicines(CurrentPharmacyId);

            // ✅ Low stock filter
            var lowStockList = allMedicines
                .Where(m => m.StockUnits <= m.LowStockLimit)
                .ToList();

            // ✅ Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                lowStockList = lowStockList
                    .Where(m =>
                        (!string.IsNullOrEmpty(m.MedicineName) &&
                         m.MedicineName.ToLower().StartsWith(search))
                        ||
                        (!string.IsNullOrEmpty(m.Category) &&
                         m.Category.ToLower().StartsWith(search))
                    )
                    .ToList();
            }

            // ✅ Pagination
            int pageSize = 8;

            TotalPages = (int)Math.Ceiling(lowStockList.Count / (double)pageSize);

            LowStockMedicines = lowStockList
                .Skip((CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}