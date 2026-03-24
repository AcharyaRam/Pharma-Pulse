using Microsoft.AspNetCore.Mvc;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using Pharma_Pulse.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class DashboardModel : PharmacyPageModel
    {
        private readonly MedicineService _service;
        private readonly AppDbContext _context;

        public DashboardModel(MedicineService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        [BindProperty]
        public Customer Customer { get; set; } = new Customer();

        public int TotalMedicineCount { get; set; }
        public int LowStockCount { get; set; }
        public int ExpiryCount { get; set; }
        public decimal SalesToday { get; set; }
        public int TotalBillsToday { get; set; }
        public decimal ProfitToday { get; set; }

        public List<(int Rank, string Name)> Top3MedicinesToday { get; set; } = new();

        public List<string> Days { get; set; } = new();
        public List<decimal> Revenue { get; set; } = new();

        public void OnGet()
        {
            Customer = new Customer();
            LoadDashboard();
        }

        // ================= SAVE CUSTOMER =================
        public IActionResult OnPostSaveCustomer()
        {
            if (!ModelState.IsValid)
            {
                LoadDashboard();
                return Page();
            }

            Customer.Email ??= "";
            Customer.MedicalNotes ??= "N/A";
            Customer.GSTNumber ??= "";

            // 🔥 MOST IMPORTANT LINE
            Customer.PharmacyId = CurrentPharmacyId;

            _context.Customers.Add(Customer);
            _context.SaveChanges();

            TempData["Success"] = "Customer Saved Successfully!";
            return RedirectToPage();
        }

        // ================= DASHBOARD =================
        private void LoadDashboard()
        {
            var allMedicines = _service.GetAllMedicines(CurrentPharmacyId);

            TotalMedicineCount = allMedicines.Count;
            LowStockCount = allMedicines.Count(m => m.StockUnits <= m.LowStockLimit);
            ExpiryCount = allMedicines.Count(m => m.ExpiryDate <= DateTime.Now.AddDays(30));

            // ✅ FILTER BILLS BY PHARMACY
            var todayQuery = _context.Bills
                .Where(b => b.PharmacyId == CurrentPharmacyId &&
                            b.BillDate.Date == DateTime.Today);

            SalesToday = todayQuery.Sum(b => b.GrandTotal);
            TotalBillsToday = todayQuery.Count();

            // ================= PROFIT =================
            ProfitToday = 0;

            var todayBillDetails = _context.BillDetails
                .Where(d => d.PharmacyId == CurrentPharmacyId &&
                            d.Bill.BillDate.Date == DateTime.Today)
                .ToList();

            foreach (var item in todayBillDetails)
            {
                var bill = _context.Bills
                    .FirstOrDefault(b => b.Id == item.BillId && b.PharmacyId == CurrentPharmacyId);

                var med = _context.Medicines
                    .FirstOrDefault(m => m.MedicineName == item.MedicineName &&
                                         m.PharmacyId == CurrentPharmacyId);

                if (bill == null || med == null) continue;

                decimal discountMultiplier = 1 - (bill.DiscountPercent / 100);
                decimal actualSellingTotal = item.Total * discountMultiplier;

                int actualUnits = item.SaleMode == "Strip"
                    ? item.Quantity * med.UnitsPerStrip
                    : item.Quantity;

                decimal costTotal = med.BuyingPrice * actualUnits;

                ProfitToday += actualSellingTotal - costTotal;
            }

            // ================= TOP 3 MEDICINES =================
            var topData = _context.BillDetails
                .Where(d => d.PharmacyId == CurrentPharmacyId &&
                            d.Bill.BillDate.Date == DateTime.Today)
                .GroupBy(d => d.MedicineName)
                .Select(g => new
                {
                    Name = g.Key,
                    Qty = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Qty)
                .Take(3)
                .ToList();

            Top3MedicinesToday.Clear();

            int rank = 1;
            foreach (var item in topData)
            {
                Top3MedicinesToday.Add((rank, item.Name));
                rank++;
            }

            if (Top3MedicinesToday.Count == 0)
                Top3MedicinesToday.Add((1, "No Sales Today"));

            // ================= REVENUE =================
            Days.Clear();
            Revenue.Clear();

            var startDate = DateTime.Today.AddDays(-14);

            var revenueData = _context.Bills
                .Where(b => b.PharmacyId == CurrentPharmacyId &&
                            b.BillDate.Date >= startDate &&
                            b.BillDate.Date <= DateTime.Today)
                .GroupBy(b => b.BillDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.GrandTotal)
                })
                .ToList();

            for (int i = 0; i < 15; i++)
            {
                var day = startDate.AddDays(i);
                var found = revenueData.FirstOrDefault(x => x.Date == day);

                Days.Add(day.ToString("dd MMM"));
                Revenue.Add(found != null ? found.Total : 0);
            }
        }
    }
}