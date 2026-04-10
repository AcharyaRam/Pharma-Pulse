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
        public string StockFilter { get; set; }

        public void OnGet(int pageNumber = 1, string search = "")
        {
            // ✅ DEBUG
            var sessionId = HttpContext.Session.GetInt32("PharmacyId");
            System.Diagnostics.Debug.WriteLine($"=== SESSION PharmacyId = {sessionId} ===");
            System.Diagnostics.Debug.WriteLine($"=== CurrentPharmacyId = {CurrentPharmacyId} ===");

            var allMedicines = _service.GetAllMedicines(CurrentPharmacyId);

            foreach (var med in allMedicines)
            {
                if (med.ExpiryDate.Date < DateTime.Today)
                    med.IsActive = false;
                else
                    med.IsActive = med.StockUnits > 0;

                _service.UpdateMedicine(med, CurrentPharmacyId);
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
                         m.SupplierName.StartsWith(search, StringComparison.OrdinalIgnoreCase)) // ✅ NEW
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
            // Stock Filter
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

            System.Diagnostics.Debug.WriteLine("=== OnPostSaveMedicine CALLED ==="); // ✅ TOP PAR

            // ✅ Remove fields set by code, not form
            ModelState.Remove("Medicine.PharmacyId");
            ModelState.Remove("Medicine.IsActive");
            ModelState.Remove("StockFilter");

            // ✅ DEBUG - add kar
            foreach (var error in ModelState)
            {
                foreach (var err in error.Value.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"=== MODELSTATE ERROR: {error.Key} = {err.ErrorMessage} ===");
                }
            }
            System.Diagnostics.Debug.WriteLine($"=== IsValid = {ModelState.IsValid} ===");
            System.Diagnostics.Debug.WriteLine($"=== Medicine.Id = {Medicine?.Id} ===");
            System.Diagnostics.Debug.WriteLine($"=== CurrentPharmacyId = {CurrentPharmacyId} ===");

            if (!ModelState.IsValid)
            {
                Medicines = _service.GetAllMedicines(CurrentPharmacyId);
                TotalPages = (int)Math.Ceiling(Medicines.Count / (double)PageSize);
                CurrentPage = 1;
                return Page();
            }

            // ✅ IsActive set karo
            Medicine.IsActive = Medicine.ExpiryDate.Date < DateTime.Today
                ? false
                : Medicine.StockUnits > 0;

            // ✅ Unit mode mein UnitsPerStrip = 1
            if (Medicine.SellType == "Unit")
                Medicine.UnitsPerStrip = 1;

            if (Medicine.Id == 0)
            {
                // ✅ ADD Medicine
                Medicine.PharmacyId = CurrentPharmacyId;
                _service.AddMedicine(Medicine);
                TempData["Success"] = "Medicine Added Successfully!";
            }
            else
            {
                // ✅ UPDATE Medicine — directly pass karo, service mein sab handle hai
                Medicine.PharmacyId = CurrentPharmacyId;
                _service.UpdateMedicine(Medicine, CurrentPharmacyId);
                TempData["Success"] = "Medicine Updated Successfully!";
            }

            return RedirectToPage();
        }
        public IActionResult OnPostDeleteMedicine(int id)
        {
            var medicine = _service.GetAllMedicines(CurrentPharmacyId)
                .FirstOrDefault(m => m.Id == id);

            if (medicine != null)
            {
                _service.DeleteMedicine(id, CurrentPharmacyId);
                TempData["Success"] = "Medicine Deleted Successfully!";
            }
            else
            {
                TempData["Error"] = "Medicine not found!";
            }

            return RedirectToPage();
        }
    }
}