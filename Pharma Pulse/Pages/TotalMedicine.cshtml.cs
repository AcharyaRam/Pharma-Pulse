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

        // Medicine List
        public List<Medicine> Medicines { get; set; } = new();

        [BindProperty]
        public Medicine Medicine { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }

        // Search
        public string SearchTerm { get; set; }

        // ============================
        // GET : Load Medicines
        // ============================
        public void OnGet(int pageNumber = 1, string search = "")
        {
            // ✅ FILTER BY PHARMACY
            var allMedicines = _service.GetAllMedicines(CurrentPharmacyId);

            // 🔥 AUTO STATUS LOGIC
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
                        !string.IsNullOrEmpty(m.MedicineName) &&
                        m.MedicineName.StartsWith(search, StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
            }

            TotalPages = (int)Math.Ceiling(allMedicines.Count / (double)PageSize);
            CurrentPage = pageNumber;

            Medicines = allMedicines
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        // ============================
        // ADD / UPDATE MEDICINE
        // ============================
        public IActionResult OnPostSaveMedicine()
        {
            if (!ModelState.IsValid)
            {
                Medicines = _service.GetAllMedicines(CurrentPharmacyId);
                return Page();
            }

            // 🔥 AUTO STATUS
            if (Medicine.ExpiryDate.Date < DateTime.Today)
                Medicine.IsActive = false;
            else
                Medicine.IsActive = Medicine.StockUnits > 0;

            if (Medicine.Id == 0)
            {
                // ✅ SET PHARMACY ID
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

                    // 🔥 AUTO STATUS
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

        // ============================
        // DELETE MEDICINE
        // ============================
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