using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Helpers;
using Pharma_Pulse.Models;
using Pharma_Pulse.Data;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class BillModel : PageModel
    {
        private readonly AppDbContext _context;

        // ✅ Inject DbContext
        public BillModel(AppDbContext context)
        {
            _context = context;
        }

        // ==========================
        // BILL ITEMS
        // ==========================
        public List<BillItem> BillItems { get; set; }

        // ==========================
        // CUSTOMER INFO
        // ==========================
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public string MobileNumber { get; set; }

        // ==========================
        // GST + TOTALS
        // ==========================
        public decimal GstPercent { get; set; }

        public decimal SubTotal { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal GrandTotal { get; set; }

        // ==========================
        // PAGE LOAD
        // ==========================
        public void OnGet()
        {
            // Load Items from Session
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            // Load Customer Info
            InvoiceNumber = HttpContext.Session.GetString("InvoiceNumber");
            CustomerName = HttpContext.Session.GetString("CustomerName");
            MobileNumber = HttpContext.Session.GetString("MobileNumber");

            // ✅ Load GST Percent from Database
            GstPercent = _context.GstSettings.FirstOrDefault()?.GstPercent ?? 5;

            // Calculate Totals
            SubTotal = BillItems.Sum(x => x.Total);

            // Dynamic GST Calculation
            CGST = SubTotal * (GstPercent / 100) / 2;
            SGST = SubTotal * (GstPercent / 100) / 2;

            GrandTotal = SubTotal + CGST + SGST;
        }

        // ==========================
        // FINISH BILL
        // ==========================
        public IActionResult OnPostFinish()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/SalesSummary");
        }
    }
}
