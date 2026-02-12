using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly MedicineService _service;

        public DashboardModel(MedicineService service)
        {
            _service = service;
        }

        public List<Medicine> Medicines { get; set; }

        public int TotalMedicineCount { get; set; }
        public int LowStockCount { get; set; }

        public Medicine? LowestStockMedicine { get; set; }

        public int ExpiryCount { get; set; }
        public Medicine? NearestExpiryMedicine { get; set; }

        public decimal SalesToday { get; set; }

        public int TotalBillsToday { get; set; }
        public string TopSellerToday { get; set; }

        public void OnGet()
        {
            // ✅ Load Medicines from Database
            var allMedicines = _service.GetAllMedicines();

            TotalMedicineCount = allMedicines.Count;

            Medicines = allMedicines.Take(10).ToList();

            LowStockCount = allMedicines.Count(m => m.StockUnits <= m.LowStockLimit);

            LowestStockMedicine = allMedicines
                .OrderBy(m => m.StockUnits)
                .FirstOrDefault();

            ExpiryCount = allMedicines.Count(m =>
                m.ExpiryDate <= DateTime.Now.AddDays(30)
            );

            NearestExpiryMedicine = allMedicines
                .OrderBy(m => m.ExpiryDate)
                .FirstOrDefault();

            // ============================
            // Sales Section (unchanged)
            // ============================
            var sales = SalesService.GetAllSales() ?? new List<Sale>();

            SalesToday = sales
                .Where(s => s.SaleDate.Date == DateTime.Today)
                .Sum(s => s.TotalAmount);

            TotalBillsToday = sales
                .Where(s => s.SaleDate.Date == DateTime.Today)
                .Select(s => s.InvoiceNumber)
                .Distinct()
                .Count();

            TopSellerToday = sales
                .Where(s => s.SaleDate.Date == DateTime.Today)
                .GroupBy(s => s.MedicineName)
                .OrderByDescending(g => g.Sum(x => x.QuantitySold))
                .Select(g => g.Key)
                .FirstOrDefault() ?? "No Sales Yet";
        }
    }
}
