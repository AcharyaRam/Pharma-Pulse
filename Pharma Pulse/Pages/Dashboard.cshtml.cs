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
        private readonly SalesService _salesService;
        private readonly AppDbContext _context;

        public DashboardModel(
            MedicineService service,
            SalesService salesService,
            AppDbContext context)
        {
            _service = service;
            _salesService = salesService;
            _context = context;
        }

        // ============================
        // ✅ Customer Form Binding
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
        public string TopSellerToday { get; set; }

        // ============================
        // Revenue Chart (15 Days)
        // ============================

        public List<string> Days { get; set; } = new();
        public List<int> Revenue { get; set; } = new();


        public void OnGet()
        {
            Customer = new Customer(); // ✅ Customer Form ke liye initialize
            LoadDashboard();
        }

        // ============================
        // ✅ Save Customer Handler
        // ============================

        public IActionResult OnPostSaveCustomer()
        {
            if (!ModelState.IsValid)
            {
                LoadDashboard(); // Page reload properly
                return Page();
            }

            // Optional Defaults
            Customer.Email ??= "";
            Customer.MedicalNotes ??= "N/A";
            Customer.GSTNumber ??= "";

            // Save Customer in DB
            _context.Customers.Add(Customer);
            _context.SaveChanges();

            TempData["Success"] = "Customer Saved Successfully!";

            return RedirectToPage();
        }


        // ============================
        // Dashboard Load Logic
        // ============================

        private void LoadDashboard()
        {
            var allMedicines = _service.GetAllMedicines();

            TotalMedicineCount = allMedicines.Count;
            LowStockCount = allMedicines.Count(m => m.StockUnits <= m.LowStockLimit);

            ExpiryCount = allMedicines.Count(m =>
                m.ExpiryDate <= DateTime.Now.AddDays(30)
            );

            var sales = _salesService.GetAllSales() ?? new List<Sale>();

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

            TopSellerToday = sales
    .Where(s => s.SaleDate.Date == DateTime.Today)
    .GroupBy(s => s.MedicineName)
    .OrderByDescending(g => g.Sum(x => x.QuantitySold))
    .Select(g => g.Key)
    .FirstOrDefault() ?? "No Sales Yet";

            // ============================
            // 15 Days Revenue Chart Data
            // ============================

            var random = new Random();

            Days.Clear();
            Revenue.Clear();

            for (int i = 14; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                Days.Add(date.ToString("dd MMM"));

                // abhi dummy (later DB se replace karenge)
                Revenue.Add(random.Next(800, 4500));
            }


        }
    }
}
