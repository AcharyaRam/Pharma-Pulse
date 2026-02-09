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

        // ===========================
        // ✅ GET PAGE LOAD
        // ===========================
        public void OnGet()
        {
            AllMedicines = MedicineService.GetAllMedicines();

            // ✅ Load Customer Info from Session
            CustomerName = HttpContext.Session.GetString("CustomerName");
            MobileNumber = HttpContext.Session.GetString("MobileNumber");

            // ✅ Load Bill Items
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            GrandTotal = BillItems.Sum(x => x.Total);

            // ✅ Invoice Number Fix
            InvoiceNumber = HttpContext.Session.GetString("InvoiceNumber");

            if (string.IsNullOrEmpty(InvoiceNumber))
            {
                InvoiceNumber = "INV-" + DateTime.Now.Ticks.ToString().Substring(10);
                HttpContext.Session.SetString("InvoiceNumber", InvoiceNumber);
            }
        }

        // ===========================
        // ✅ ADD ITEM BUTTON
        // ===========================
        public IActionResult OnPostAddItem()
        {
            AllMedicines = MedicineService.GetAllMedicines();

            // ✅ Save Customer Info only if not null
            if (!string.IsNullOrEmpty(CustomerName))
                HttpContext.Session.SetString("CustomerName", CustomerName);

            if (!string.IsNullOrEmpty(MobileNumber))
                HttpContext.Session.SetString("MobileNumber", MobileNumber);

            // Load bill items
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            // Invoice Fix
            InvoiceNumber = HttpContext.Session.GetString("InvoiceNumber");

            if (string.IsNullOrEmpty(InvoiceNumber))
            {
                InvoiceNumber = "INV-" + DateTime.Now.Ticks.ToString().Substring(10);
                HttpContext.Session.SetString("InvoiceNumber", InvoiceNumber);
            }

            var med = AllMedicines.FirstOrDefault(m => m.MedicineName == SelectedMedicine);

            if (med == null)
                return RedirectToPage();

            // ✅ Same medicine quantity increase
            var existingItem = BillItems.FirstOrDefault(x => x.MedicineName == med.MedicineName);

            if (existingItem != null)
                existingItem.Quantity += Quantity;
            else
                BillItems.Add(new BillItem
                {
                    MedicineName = med.MedicineName,
                    Quantity = Quantity,
                    Price = med.SellingPrice
                });

            HttpContext.Session.SetObject("BillItems", BillItems);

            return RedirectToPage();
        }


        // ===========================
        // ✅ COMPLETE SALE BUTTON
        // ===========================
        public IActionResult OnPostCompleteSale()
        {
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            InvoiceNumber = HttpContext.Session.GetString("InvoiceNumber");

            if (BillItems.Count == 0)
                return RedirectToPage();

            foreach (var item in BillItems)
            {
                var med = MedicineService.GetAllMedicines()
                            .FirstOrDefault(m => m.MedicineName == item.MedicineName);

                if (med == null) continue;

                decimal profit = (med.SellingPrice - med.BuyingPrice) * item.Quantity;

                // ✅ Save Sale Record
                SalesService.AddSale(new Sale
                {
                    InvoiceNumber = InvoiceNumber,
                    MedicineName = item.MedicineName,
                    QuantitySold = item.Quantity,
                    SaleDate = DateTime.Now,
                    TotalAmount = item.Total,
                    Profit = profit
                });
            }

            // ✅ Clear Everything After Sale
            HttpContext.Session.Remove("BillItems");
            HttpContext.Session.Remove("InvoiceNumber");
            HttpContext.Session.Remove("CustomerName");
            HttpContext.Session.Remove("MobileNumber");

            return RedirectToPage("/SalesSummary");
        }
    }
}
