using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Helpers;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class QuickBillingModel : PageModel
    {
        [BindProperty]
        public string CustomerName { get; set; }

        [BindProperty]
        public string MobileNumber { get; set; }

        [BindProperty]
        public string SelectedMedicine { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

        public List<Medicine> AllMedicines { get; set; }

        public List<BillItem> BillItems { get; set; }

        public string InvoiceNumber { get; set; }

        public decimal GrandTotal { get; set; }

        public void OnGet()
        {
            AllMedicines = MedicineService.GetAllMedicines();

            // Load bill items from session
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            GrandTotal = BillItems.Sum(x => x.Total);

            InvoiceNumber = "INV-" + DateTime.Now.Ticks.ToString().Substring(10);
        }

        public IActionResult OnPostCompleteSale()
        {
            // Load medicines again
            AllMedicines = MedicineService.GetAllMedicines();

            // Load bill items from session
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            // If no items, return
            if (BillItems.Count == 0)
                return RedirectToPage();

            // ✅ Convert BillItems into Sale records
            foreach (var item in BillItems)
            {
                var med = AllMedicines.FirstOrDefault(m => m.MedicineName == item.MedicineName);

                if (med == null) continue;

                Sale sale = new Sale
                {
                    
                    MedicineName = item.MedicineName,
                    QuantitySold = item.Quantity,
                    SaleDate = DateTime.Now,
                    TotalAmount = item.Total,
                    Profit = (med.SellingPrice - med.BuyingPrice) * item.Quantity
                };

                // ✅ Store into SalesService
                SalesService.AddSale(sale);
            }

            // ✅ Clear Bill after sale complete
            HttpContext.Session.Remove("BillItems");

            // Redirect to Sales Summary
            return RedirectToPage("/SalesSummary");
        }


        public void OnPostAddItem()
        {
            AllMedicines = MedicineService.GetAllMedicines();

            // Load existing list from session
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            var med = AllMedicines.FirstOrDefault(m => m.MedicineName == SelectedMedicine);

            if (med == null) return;

            // Add new item
            BillItems.Add(new BillItem
            {
                MedicineName = med.MedicineName,
                Quantity = Quantity,
                Price = med.SellingPrice
            });

            // Save back into session
            HttpContext.Session.SetObject("BillItems", BillItems);

            GrandTotal = BillItems.Sum(x => x.Total);

            InvoiceNumber = "INV-" + DateTime.Now.Ticks.ToString().Substring(10);
        }
    }
}
