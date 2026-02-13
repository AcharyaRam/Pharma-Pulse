using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharma_Pulse.Data;
using Pharma_Pulse.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class SalesSummaryModel : PageModel
    {
        private readonly AppDbContext _context;

        public SalesSummaryModel(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Bills List
        public List<Bill> AllBills { get; set; } = new();

        // ✅ Totals
        public decimal TotalSales { get; set; }
        public decimal TotalGST { get; set; }

        // ✅ NEW Total Profit
        public decimal TotalProfit { get; set; }

        // ✅ Report Filter Dropdown
        [BindProperty(SupportsGet = true)]
        public string ReportType { get; set; }

        public void OnGet()
        {
            if (string.IsNullOrEmpty(ReportType))
                ReportType = "Daily";

            // ✅ Load Bills with BillDetails
            var billsQuery = _context.Bills
                .Include(b => b.BillDetails)
                .AsQueryable();

            // ✅ Filter Bills
            if (ReportType == "Daily")
            {
                billsQuery = billsQuery
                    .Where(b => b.BillDate.Date == DateTime.Today);
            }
            else if (ReportType == "Monthly")
            {
                billsQuery = billsQuery
                    .Where(b =>
                        b.BillDate.Month == DateTime.Now.Month &&
                        b.BillDate.Year == DateTime.Now.Year
                    );
            }
            else if (ReportType == "Yearly")
            {
                billsQuery = billsQuery
                    .Where(b => b.BillDate.Year == DateTime.Now.Year);
            }

            // ✅ Store Bills
            AllBills = billsQuery
                .OrderByDescending(b => b.BillDate)
                .ToList();

            // ✅ Totals
            TotalSales = AllBills.Sum(b => b.GrandTotal);

            TotalGST = AllBills.Sum(b => b.CGST + b.SGST);

            // ===========================================
            // ✅ PROFIT CALCULATION (Main Fix)
            // ===========================================

            TotalProfit = 0;

            foreach (var bill in AllBills)
            {
                foreach (var item in bill.BillDetails)
                {
                    // Find Medicine Buying Price
                    var med = _context.Medicines
                        .FirstOrDefault(m => m.MedicineName == item.MedicineName);

                    if (med == null) continue;

                    decimal profitPerUnit = med.SellingPrice - med.BuyingPrice;

                    decimal itemProfit = profitPerUnit * item.Quantity;

                    TotalProfit += itemProfit;
                }
            }
        }
    }
}
