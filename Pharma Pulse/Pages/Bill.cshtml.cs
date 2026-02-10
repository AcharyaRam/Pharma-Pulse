using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Helpers;
using Pharma_Pulse.Models;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class BillModel : PageModel
    {
        public List<BillItem> BillItems { get; set; }

        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public string MobileNumber { get; set; }

        public decimal SubTotal { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal GrandTotal { get; set; }

        public void OnGet()
        {
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            InvoiceNumber = HttpContext.Session.GetString("InvoiceNumber");
            CustomerName = HttpContext.Session.GetString("CustomerName");
            MobileNumber = HttpContext.Session.GetString("MobileNumber");

            SubTotal = BillItems.Sum(x => x.Total);
            CGST = SubTotal * 0.025m;
            SGST = SubTotal * 0.025m;
            GrandTotal = SubTotal + CGST + SGST;
        }

        public IActionResult OnPostFinish()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/SalesSummary");
        }
    }
}
