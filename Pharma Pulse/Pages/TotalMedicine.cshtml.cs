using Microsoft.AspNetCore.Mvc;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class TotalMedicineModel : PharmacyPageModel
    {
        private readonly MedicineService _service;

        public TotalMedicineModel(MedicineService service)
        {
            _service = service;
        }

        public List<Medicine> Medicines { get; set; } = new();

        [BindProperty]
        public Medicine Medicine { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ExpiryFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StockFilter { get; set; }

        public void OnGet(int pageNumber = 1, string search = "")
        {
            var allMedicines = _service.GetAllMedicines(CurrentPharmacyId);

            // ✅ Sirf IsActive set karo, UpdateMedicine call NAHI
            foreach (var med in allMedicines)
            {
                if (med.ExpiryDate.Date < DateTime.Today)
                    med.IsActive = false;
                else
                    med.IsActive = med.StockUnits > 0;
            }

            SearchTerm = search;

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                allMedicines = allMedicines
                    .Where(m =>
                        (!string.IsNullOrEmpty(m.MedicineName) &&
                         m.MedicineName.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                        ||
                        (!string.IsNullOrEmpty(m.SupplierName) &&
                         m.SupplierName.StartsWith(search, StringComparison.OrdinalIgnoreCase))
                    )
                    .ToList();
            }

            if (ExpiryFilter.HasValue)
            {
                var cutoff = DateTime.Today.AddDays(ExpiryFilter.Value);
                allMedicines = allMedicines
                    .Where(m => m.ExpiryDate.Date <= cutoff && m.ExpiryDate.Date >= DateTime.Today)
                    .ToList();
            }

            if (!string.IsNullOrEmpty(StockFilter))
            {
                if (StockFilter == "low")
                    allMedicines = allMedicines
                        .Where(m => m.StockUnits > 0 && m.StockUnits <= m.LowStockLimit)
                        .ToList();
                else if (StockFilter == "out")
                    allMedicines = allMedicines
                        .Where(m => m.StockUnits == 0)
                        .ToList();
            }

            TotalPages = (int)Math.Ceiling(allMedicines.Count / (double)PageSize);
            CurrentPage = pageNumber;

            Medicines = allMedicines
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public IActionResult OnPostSaveMedicine()
        {
            foreach (var error in ModelState)
            {
                if (error.Value.Errors.Count > 0)
                {
                    TempData["Error"] = $"Field: {error.Key} — Error: {error.Value.Errors[0].ErrorMessage}";
                    Medicines = _service.GetAllMedicines(CurrentPharmacyId);
                    return Page();
                }
            }

            if (Medicine.SellType == "Unit")
                Medicine.UnitsPerStrip = 1;

            if (Medicine.ExpiryDate.Date < DateTime.Today)
                Medicine.IsActive = false;
            else
                Medicine.IsActive = Medicine.StockUnits > 0;

            if (Medicine.Id == 0)
            {
                // ✅ ADD
                Medicine.PharmacyId = CurrentPharmacyId;
                _service.AddMedicine(Medicine);
                TempData["Success"] = "Medicine Added Successfully!";
            }
            else
            {
                // ✅ FINAL FIX: GetMedicineById nahi, seedha SQL update
                Medicine.PharmacyId = CurrentPharmacyId;
                _service.UpdateMedicine(Medicine, CurrentPharmacyId);
                TempData["Success"] = "Medicine Updated Successfully!";
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDeleteMedicine(int id)
        {
            _service.DeleteMedicine(id, CurrentPharmacyId);
            TempData["Success"] = "Medicine Deleted Successfully!";
            return RedirectToPage();
        }
    }
}