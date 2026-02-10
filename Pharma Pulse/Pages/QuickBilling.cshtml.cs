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
        // ✅ PAGE LOAD
        // ===========================
        public void OnGet()
        {
            AllMedicines = MedicineService.GetAllMedicines();

            CustomerName = HttpContext.Session.GetString("CustomerName");
            MobileNumber = HttpContext.Session.GetString("MobileNumber");

            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            GrandTotal = BillItems.Sum(x => x.Total);

            InvoiceNumber = HttpContext.Session.GetString("InvoiceNumber");

            if (string.IsNullOrEmpty(InvoiceNumber))
            {
                InvoiceNumber = "INV-" + DateTime.Now.Ticks.ToString().Substring(10);
                HttpContext.Session.SetString("InvoiceNumber", InvoiceNumber);
            }
        }


        // ===========================
        // ✅ ADD ITEM
        // ===========================
        public IActionResult OnPostAddItem()
        {
            AllMedicines = MedicineService.GetAllMedicines();

            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            var med = AllMedicines.FirstOrDefault(m => m.MedicineName == SelectedMedicine);

            if (med == null)
                return RedirectToPage();

            // ❌ Stock check
            if (med.Stock < Quantity)
            {
                ModelState.AddModelError("", "Not enough stock available.");
                return RedirectToPage();
            }

            // ✅ Reduce stock immediately
            med.Stock -= Quantity;
            MedicineService.UpdateMedicine(med);

            // Add to bill
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
        // ✅ DELETE ITEM
        // ===========================
        public IActionResult OnPostDeleteItem(string medicineName)
        {
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            var item = BillItems.FirstOrDefault(x => x.MedicineName == medicineName);

            if (item != null)
            {
                // ✅ Stock wapas add karo
                var med = MedicineService.GetAllMedicines()
                            .FirstOrDefault(m => m.MedicineName == item.MedicineName);

                if (med != null)
                {
                    med.Stock += item.Quantity;
                    MedicineService.UpdateMedicine(med);
                }

                BillItems.Remove(item);
                HttpContext.Session.SetObject("BillItems", BillItems);
            }

            return RedirectToPage();
        }




        // ===========================
        // ✅ PRINT BILL
        // ===========================
        public IActionResult OnPostPrintBill()
        {
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            if (BillItems.Count == 0)
                return RedirectToPage();

            if (!string.IsNullOrEmpty(CustomerName))
                HttpContext.Session.SetString("CustomerName", CustomerName);

            if (!string.IsNullOrEmpty(MobileNumber))
                HttpContext.Session.SetString("MobileNumber", MobileNumber);

            return RedirectToPage("/Bill");
        }

        // ===========================
        // ✅ COMPLETE SALE
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

            HttpContext.Session.Remove("BillItems");
            HttpContext.Session.Remove("InvoiceNumber");
            HttpContext.Session.Remove("CustomerName");
            HttpContext.Session.Remove("MobileNumber");

            return RedirectToPage("/SalesSummary");
        }
    }
}
