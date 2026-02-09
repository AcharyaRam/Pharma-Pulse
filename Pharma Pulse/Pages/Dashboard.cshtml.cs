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
        public List<Medicine> Medicines { get; set; }

        // ✅ Total medicines count for card
        public int TotalMedicineCount { get; set; }

        // ✅ Low stock medicine count
        public int LowStockCount { get; set; }

        // ✅ Lowest stock medicine
        public Medicine? LowestStockMedicine { get; set; }

        // ✅ Expiry count
        public int ExpiryCount { get; set; }

        // ✅ Nearest expiry medicine
        public Medicine? NearestExpiryMedicine { get; set; }

        // ✅ Total sales today
        public decimal SalesToday { get; set; }

        // ✅ Total bills + Top seller
        public int TotalBillsToday { get; set; }
        public string TopSellerToday { get; set; }

        public void OnGet()
        {
            // ✅ Load Medicines
            var allMedicines = MedicineService.GetAllMedicines();

            TotalMedicineCount = allMedicines.Count;

            // Show only top 10 medicines in dashboard table
            Medicines = allMedicines.Take(10).ToList();

            // ✅ Low Stock Count (based on medicine LowStockLimit)
            LowStockCount = allMedicines.Count(m => m.Stock <= m.LowStockLimit);

            // ✅ Lowest Stock Medicine
            LowestStockMedicine = allMedicines
                .OrderBy(m => m.Stock)
                .FirstOrDefault();

            // ✅ Expiry Count (next 30 days)
            ExpiryCount = allMedicines.Count(m =>
                m.ExpiryDate <= DateTime.Now.AddDays(30)
            );

            // ✅ Nearest Expiry Medicine
            NearestExpiryMedicine = allMedicines
                .OrderBy(m => m.ExpiryDate)
                .FirstOrDefault();

            // ====================================================
            // ✅ Load Sales Only ONCE
            // ====================================================
            var sales = SalesService.GetAllSales() ?? new List<Sale>();

            // ✅ Today's Sales Total
            SalesToday = sales
                .Where(s => s.SaleDate.Date == DateTime.Today)
                .Sum(s => s.TotalAmount);

            // ✅ Total Bills Today (unique invoices)
            TotalBillsToday = sales
                .Where(s => s.SaleDate.Date == DateTime.Today)
                .Select(s => s.InvoiceNumber)
                .Distinct()
                .Count();

            // ✅ Top Seller Medicine Today
            TopSellerToday = sales
                .Where(s => s.SaleDate.Date == DateTime.Today)
                .GroupBy(s => s.MedicineName)
                .OrderByDescending(g => g.Sum(x => x.QuantitySold))
                .Select(g => g.Key)
                .FirstOrDefault() ?? "No Sales Yet";
        }
    }
}
