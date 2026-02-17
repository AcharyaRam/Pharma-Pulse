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
        public decimal TotalProfit { get; set; }

        // ✅ Report Filter Dropdown
        [BindProperty(SupportsGet = true)]
        public string ReportType { get; set; }

        // ✅ Search Term
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        // ✅ Custom Date Range
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public void OnGet()
        {
            if (string.IsNullOrEmpty(ReportType))
                ReportType = "Daily";

            // ✅ Load Bills with Details
            var billsQuery = _context.Bills
                .Include(b => b.BillDetails)
                .AsQueryable();

            // ============================
            // ✅ SEARCH FILTER (Name/Mobile)
            // ============================
            
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                string search = SearchTerm.Trim().ToLower();

                billsQuery = billsQuery.Where(b =>
                    (
                        !string.IsNullOrEmpty(b.CustomerName) &&
                        b.CustomerName.ToLower().StartsWith(search)
                    )
                    ||
                    (
                        !string.IsNullOrEmpty(b.MobileNumber) &&
                        b.MobileNumber.StartsWith(search)
                    )
                );
            }




            // ============================
            // ✅ REPORT TYPE FILTER
            // ============================
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
            else if (ReportType == "Custom")
            {
                if (StartDate.HasValue && EndDate.HasValue)
                {
                    billsQuery = billsQuery.Where(b =>
                        b.BillDate.Date >= StartDate.Value.Date &&
                        b.BillDate.Date <= EndDate.Value.Date
                    );
                }
            }

            // ✅ Store Bills
            AllBills = billsQuery
                .OrderByDescending(b => b.BillDate)
                .ToList();

            // ============================
            // ✅ TOTALS
            // ============================
            TotalSales = AllBills.Sum(b => b.GrandTotal);
            TotalGST = AllBills.Sum(b => b.CGST + b.SGST);

            // ============================
            // ✅ PROFIT CALCULATION
            // ============================
            TotalProfit = 0;

            foreach (var bill in AllBills)
            {
                foreach (var item in bill.BillDetails)
                {
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
