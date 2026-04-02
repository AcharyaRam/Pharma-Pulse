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
            if (!ModelState.IsValid)
            {
                Medicines = _service.GetAllMedicines(CurrentPharmacyId);
                return Page();
            }

            if (Medicine.ExpiryDate.Date < DateTime.Today)
                Medicine.IsActive = false;
            else
                Medicine.IsActive = Medicine.StockUnits > 0;

            // ✅ FIX: Force UnitsPerStrip = 1 if Unit only
            if (Medicine.SellType == "Unit")
                Medicine.UnitsPerStrip = 1;

            if (Medicine.Id == 0)
            {
                Medicine.PharmacyId = CurrentPharmacyId;
                _service.AddMedicine(Medicine);
                TempData["Success"] = "Medicine Added Successfully!";
            }
            else
            {
                var existing = _service.GetAllMedicines(CurrentPharmacyId)
                    .FirstOrDefault(m => m.Id == Medicine.Id);

                if (existing != null)
                {
                    existing.MedicineName = Medicine.MedicineName;
                    existing.Category = Medicine.Category;
                    existing.BatchNo = Medicine.BatchNo;
                    existing.MfgDate = Medicine.MfgDate;
                    existing.StockUnits = Medicine.StockUnits;
                    existing.LowStockLimit = Medicine.LowStockLimit;
                    existing.BuyingPrice = Medicine.BuyingPrice;
                    existing.SellingPrice = Medicine.SellingPrice;
                    existing.HsnSac = Medicine.HsnSac;
                    existing.SellType = Medicine.SellType;
                    existing.ExpiryDate = Medicine.ExpiryDate;
                    existing.UnitsPerStrip = Medicine.UnitsPerStrip; // ✅ FIX: Save UnitsPerStrip
                    existing.GstPercent = Medicine.GstPercent;
                    existing.SupplierName = Medicine.SupplierName;

                    if (existing.ExpiryDate.Date < DateTime.Today)
                        existing.IsActive = false;
                    else
                        existing.IsActive = existing.StockUnits > 0;

                    _service.UpdateMedicine(existing, CurrentPharmacyId);
                }

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