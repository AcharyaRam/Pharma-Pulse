using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class SalesSummaryModel : PageModel
    {
        // ✅ Inject SalesService (Database)
        private readonly SalesService _salesService;

        public SalesSummaryModel(SalesService salesService)
        {
            _salesService = salesService;
        }

        // ✅ Filtered Sales List (Daily/Monthly/Yearly)
        public List<Sale> FilteredSales { get; set; } = new List<Sale>();

        // ✅ Totals
        public decimal TotalSales { get; set; }
        public decimal TotalProfit { get; set; }

        // ✅ Dropdown Selected Report Type
        [BindProperty(SupportsGet = true)]
        public string ReportType { get; set; }

        public void OnGet()
        {
            // ✅ Default Report Type
            if (string.IsNullOrEmpty(ReportType))
                ReportType = "Daily";

            // ✅ Load All Sales from Database
            var sales = _salesService.GetAllSales();

            // ✅ Apply Filter Based on ReportType
            if (ReportType == "Daily")
            {
                sales = sales
                    .Where(s => s.SaleDate.Date == DateTime.Today)
                    .ToList();
            }
            else if (ReportType == "Monthly")
            {
                sales = sales
                    .Where(s =>
                        s.SaleDate.Month == DateTime.Now.Month &&
                        s.SaleDate.Year == DateTime.Now.Year
                    )
                    .ToList();
            }
            else if (ReportType == "Yearly")
            {
                sales = sales
                    .Where(s => s.SaleDate.Year == DateTime.Now.Year)
                    .ToList();
            }

            // ✅ Store Filtered List for Table Display
            FilteredSales = sales;

            // ✅ Calculate Totals
            TotalSales = FilteredSales.Sum(s => s.TotalAmount);
            TotalProfit = FilteredSales.Sum(s => s.Profit);
        }
    }
}
