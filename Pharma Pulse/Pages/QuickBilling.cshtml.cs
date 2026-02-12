using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Helpers;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using Pharma_Pulse.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class QuickBillingModel : PageModel
    {
        // ✅ Inject MedicineService
        private readonly MedicineService _service;

        // ✅ Inject SalesService (Database)
        private readonly SalesService _salesService;

        // ✅ Inject DbContext (Bill Save)
        private readonly AppDbContext _context;

        public QuickBillingModel(
            MedicineService service,
            SalesService salesService,
            AppDbContext context)
        {
            _service = service;
            _salesService = salesService;
            _context = context;
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

            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            var med = AllMedicines.FirstOrDefault(m => m.MedicineName == SelectedMedicine);

            if (med == null)
                return RedirectToPage();

            // ===========================
            // ✅ FIX SELL MODE BASED ON SELLTYPE
            // ===========================
            if (med.SellType == "Unit")
            {
                SellMode = "Tablet"; // Fixed mode
            }
            else if (med.SellType == "Pack")
            {
                SellMode = "Strip";  // Fixed mode
            }
            else if (med.SellType == "Both")
            {
                if (string.IsNullOrEmpty(SellMode))
                    SellMode = "Tablet"; // Default for Both
            }

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
            // ADD ITEM TO SESSION
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
                        restoreUnits = item.Quantity * med.UnitsPerStrip;

                    med.StockUnits += restoreUnits;
                    _service.UpdateMedicine(med);
                }

                BillItems.Remove(item);
                HttpContext.Session.SetObject("BillItems", BillItems);
            }

            return RedirectToPage();
        }

        // ===========================
        // COMPLETE SALE + BILL SAVE
        // ===========================
        public IActionResult OnPostCompleteSale()
        {
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems")
                        ?? new List<BillItem>();

            InvoiceNumber = HttpContext.Session.GetString("InvoiceNumber");

            if (BillItems.Count == 0)
                return RedirectToPage();

            // GST Load
            decimal gstPercent =
                _context.GstSettings.FirstOrDefault()?.GstPercent ?? 5;

            decimal subTotal = BillItems.Sum(x => x.Total);

            decimal cgst = subTotal * (gstPercent / 100) / 2;
            decimal sgst = subTotal * (gstPercent / 100) / 2;

            decimal grandTotal = subTotal + cgst + sgst;

            // SAVE BILL HEADER
            Bill bill = new Bill
            {
                InvoiceNumber = InvoiceNumber,
                CustomerName = HttpContext.Session.GetString("CustomerName"),
                MobileNumber = HttpContext.Session.GetString("MobileNumber"),
                DoctorName = HttpContext.Session.GetString("DoctorName"),

                SubTotal = subTotal,
                GstPercent = gstPercent,
                CGST = cgst,
                SGST = sgst,
                GrandTotal = grandTotal,
                BillDate = DateTime.Now
            };

            _context.Bills.Add(bill);
            _context.SaveChanges();

            // SAVE BILL DETAILS
            foreach (var item in BillItems)
            {
                _context.BillDetails.Add(new BillDetail
                {
                    BillId = bill.Id,
                    MedicineName = item.MedicineName,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Total = item.Total
                });
            }

            _context.SaveChanges();

            // SAVE SALES REPORT
            foreach (var item in BillItems)
            {
                var med = _service.GetAllMedicines()
                    .FirstOrDefault(m => m.MedicineName == item.MedicineName);

                if (med == null) continue;

                decimal profit =
                    (med.SellingPrice - med.BuyingPrice) * item.Quantity;

                _salesService.AddSale(new Sale
                {
                    InvoiceNumber = InvoiceNumber,
                    MedicineName = item.MedicineName,
                    QuantitySold = item.Quantity,
                    SaleDate = DateTime.Now,
                    TotalAmount = item.Total,
                    Profit = profit
                });
            }

            // CLEAR SESSION
            HttpContext.Session.Clear();

            return RedirectToPage("/SalesSummary");
        }
    }
}
