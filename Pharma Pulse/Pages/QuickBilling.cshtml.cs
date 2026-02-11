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
        // ✅ Inject MedicineService
        private readonly MedicineService _service;

        public QuickBillingModel(MedicineService service)
        {
            _service = service;
        }

        // ===========================
        // CUSTOMER DETAILS
        // ===========================
        [BindProperty]
        public string CustomerName { get; set; }

        [BindProperty]
        public string MobileNumber { get; set; }

        [BindProperty]
        public string DoctorName { get; set; }

        // ===========================
        // MEDICINE DETAILS
        // ===========================
        [BindProperty]
        public string SelectedMedicine { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

        [BindProperty]
        public string SellMode { get; set; }

        public List<Medicine> AllMedicines { get; set; } = new();
        public List<BillItem> BillItems { get; set; } = new();

        public string InvoiceNumber { get; set; }
        public decimal GrandTotal { get; set; }

        // ===========================
        // PAGE LOAD
        // ===========================
        public void OnGet()
        {
            AllMedicines = _service.GetAllMedicines();

            CustomerName = HttpContext.Session.GetString("CustomerName");
            MobileNumber = HttpContext.Session.GetString("MobileNumber");
            DoctorName = HttpContext.Session.GetString("DoctorName");

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
        // ADD ITEM
        // ===========================
        public IActionResult OnPostAddItem()
        {
            AllMedicines = _service.GetAllMedicines();

            // Save Customer Info
            HttpContext.Session.SetString("CustomerName", CustomerName ?? "");
            HttpContext.Session.SetString("MobileNumber", MobileNumber ?? "");
            HttpContext.Session.SetString("DoctorName", DoctorName ?? "");

            // Load Bill Items
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            var med = AllMedicines.FirstOrDefault(m => m.MedicineName == SelectedMedicine);

            if (med == null)
                return RedirectToPage();

            if (string.IsNullOrEmpty(SellMode))
                SellMode = "Tablet";

            // ===========================
            // CALCULATE UNITS TO SELL
            // ===========================
            int unitsToSell = Quantity;

            if (SellMode == "Strip")
            {
                unitsToSell = Quantity * med.UnitsPerStrip;
            }

            // ===========================
            // STOCK CHECK
            // ===========================
            if (med.StockUnits < unitsToSell)
            {
                TempData["Error"] = "Not enough stock available!";
                return RedirectToPage();
            }

            // ===========================
            // REDUCE STOCK
            // ===========================
            med.StockUnits -= unitsToSell;
            _service.UpdateMedicine(med);

            // ===========================
            // ADD TO BILL
            // ===========================
            BillItems.Add(new BillItem
            {
                MedicineName = med.MedicineName,
                Quantity = Quantity,
                SaleMode = SellMode,
                Price = med.SellingPrice
            });

            HttpContext.Session.SetObject("BillItems", BillItems);

            return RedirectToPage();
        }

        // ===========================
        // DELETE ITEM
        // ===========================
        public IActionResult OnPostDeleteItem(string medicineName)
        {
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            var item = BillItems.FirstOrDefault(x => x.MedicineName == medicineName);

            if (item != null)
            {
                var med = _service.GetAllMedicines()
                            .FirstOrDefault(m => m.MedicineName == item.MedicineName);

                if (med != null)
                {
                    int restoreUnits = item.Quantity;

                    if (item.SaleMode == "Strip")
                    {
                        restoreUnits = item.Quantity * med.UnitsPerStrip;
                    }

                    med.StockUnits += restoreUnits;
                    _service.UpdateMedicine(med);
                }

                BillItems.Remove(item);
                HttpContext.Session.SetObject("BillItems", BillItems);
            }

            return RedirectToPage();
        }

        // ===========================
        // PRINT BILL
        // ===========================
        public IActionResult OnPostPrintBill()
        {
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            if (BillItems.Count == 0)
                return RedirectToPage();

            return RedirectToPage("/Bill");
        }

        // ===========================
        // COMPLETE SALE
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
                var med = _service.GetAllMedicines()
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

            // Clear Session
            HttpContext.Session.Remove("BillItems");
            HttpContext.Session.Remove("InvoiceNumber");
            HttpContext.Session.Remove("CustomerName");
            HttpContext.Session.Remove("MobileNumber");
            HttpContext.Session.Remove("DoctorName");

            return RedirectToPage("/SalesSummary");
        }
    }
}
