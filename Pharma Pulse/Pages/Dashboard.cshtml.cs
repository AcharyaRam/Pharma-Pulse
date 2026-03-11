using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using Pharma_Pulse.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly MedicineService _service;
        private readonly AppDbContext _context;


        public DashboardModel(MedicineService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        // ============================
        // Customer Form
        // ============================
        [BindProperty]
        public Customer Customer { get; set; } = new Customer();

        // ============================
        // Dashboard Stats
        // ============================
        public int TotalMedicineCount { get; set; }
        public int LowStockCount { get; set; }
        public int ExpiryCount { get; set; }
        public decimal SalesToday { get; set; }
        public int TotalBillsToday { get; set; }
        public decimal ProfitToday { get; set; }

        // ⭐ FINAL FEATURE
        public List<(int Rank, string Name)> Top3MedicinesToday { get; set; } = new();

        // ============================
        // Revenue Chart (REAL DATA)
        // ============================
        public List<string> Days { get; set; } = new();
        public List<decimal> Revenue { get; set; } = new();

        public void OnGet()
        {
            Customer = new Customer();
            LoadDashboard();
        }

        // ============================
        // Save Customer
        // ============================
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

            _context.Customers.Add(Customer);
            _context.SaveChanges();

            TempData["Success"] = "Customer Saved Successfully!";
            return RedirectToPage();
        }

        // ============================
        // Dashboard Load
        // ============================
        private void LoadDashboard()
        {
            var allMedicines = _service.GetAllMedicines();

            TotalMedicineCount = allMedicines.Count;
            LowStockCount = allMedicines.Count(m => m.StockUnits <= m.LowStockLimit);
            ExpiryCount = allMedicines.Count(m => m.ExpiryDate <= DateTime.Now.AddDays(30));

            // ============================
            // TODAY SALES (Optimized)
            // ============================
            var todayQuery = _context.Bills
                .Where(b => b.BillDate.Date == DateTime.Today);

            SalesToday = todayQuery.Sum(b => b.GrandTotal);
            TotalBillsToday = todayQuery.Count();

            // ============================
            // TODAY PROFIT (Correct Way)
            // ============================
            ProfitToday = _context.BillDetails
                .Where(d => d.Bill.BillDate.Date == DateTime.Today)
                .Sum(d => (d.SellingPrice - d.PurchasePrice) * d.Quantity);

            // ============================
            // TOP 3 MEDICINES TODAY
            // ============================
            var topData = _context.BillDetails
                .Where(d => d.Bill.BillDate.Date == DateTime.Today)
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

            // ============================
            // LAST 15 DAYS REVENUE
            // ============================
            Days.Clear();
            Revenue.Clear();

            var startDate = DateTime.Today.AddDays(-14);

            var revenueData = _context.Bills
                .Where(b => b.BillDate.Date >= startDate &&
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
