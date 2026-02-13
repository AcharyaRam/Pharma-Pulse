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
        private readonly MedicineService _service;
        private readonly SalesService _salesService;
        private readonly AppDbContext _context;

        public QuickBillingModel(MedicineService service, SalesService salesService, AppDbContext context)
        {
            _service = service;
            _salesService = salesService;
            _context = context;
        }

        // =======================
        // CUSTOMER
        // =======================
        public List<Customer> AllCustomers { get; set; } = new();
        public Customer SelectedCustomer { get; set; }

        [BindProperty]
        public int SelectedCustomerId { get; set; }

        // =======================
        // MEDICINE
        // =======================
        [BindProperty]
        public string SelectedMedicine { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

        [BindProperty]
        public string SellMode { get; set; } // Tablet / Strip

        public List<Medicine> AllMedicines { get; set; } = new();
        public List<BillItem> BillItems { get; set; } = new();

        // =======================
        // BILL EXTRA
        // =======================
        [BindProperty]
        public decimal DiscountPercent { get; set; }

        [BindProperty]
        public string PaymentMode { get; set; } = "Cash";

        public string InvoiceNumber { get; set; }

        // =======================
        // TOTALS
        // =======================
        public decimal SubTotal { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal GrandTotal { get; set; }

        // =======================
        // PAGE LOAD
        // =======================
        public void OnGet()
        {
            LoadData();
        }

        private void LoadData()
        {
            AllMedicines = _service.GetAllMedicines();
            AllCustomers = _context.Customers.ToList();

            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems") ?? new();

            SelectedCustomerId = HttpContext.Session.GetInt32("SelectedCustomerId") ?? 0;
            SelectedCustomer = AllCustomers.FirstOrDefault(c => c.CustomerId == SelectedCustomerId);

            DiscountPercent = Convert.ToDecimal(HttpContext.Session.GetString("Discount") ?? "0");
            PaymentMode = HttpContext.Session.GetString("PaymentMode") ?? "Cash";

            InvoiceNumber = HttpContext.Session.GetString("InvoiceNumber");

            if (string.IsNullOrEmpty(InvoiceNumber))
            {
                InvoiceNumber = "INV-" + DateTime.Now.Ticks.ToString().Substring(10);
                HttpContext.Session.SetString("InvoiceNumber", InvoiceNumber);
            }

            CalculateTotals();
        }

        // =======================
        // TOTAL CALCULATION
        // =======================
        private void CalculateTotals()
        {
            decimal gstPercent = _context.GstSettings.FirstOrDefault()?.GstPercent ?? 5;

            SubTotal = BillItems.Sum(x => x.Total);

            CGST = SubTotal * (gstPercent / 100) / 2;
            SGST = SubTotal * (gstPercent / 100) / 2;

            decimal discountAmount = SubTotal * (DiscountPercent / 100);

            GrandTotal = (SubTotal - discountAmount) + CGST + SGST;
        }

        // =======================
        // CUSTOMER SELECT
        // =======================
        public IActionResult OnPostSelectCustomer()
        {
            HttpContext.Session.SetInt32("SelectedCustomerId", SelectedCustomerId);
            return RedirectToPage();
        }

        // =======================
        // APPLY DISCOUNT + PAYMENT
        // =======================
        public IActionResult OnPostApplyDiscount()
        {
            HttpContext.Session.SetString("Discount", DiscountPercent.ToString());
            HttpContext.Session.SetString("PaymentMode", PaymentMode);

            return RedirectToPage();
        }

        // =======================
        // ADD ITEM (UNIT + STRIP FIX)
        // =======================
        public IActionResult OnPostAddItem()
        {
            LoadData();

            var med = AllMedicines.FirstOrDefault(m => m.MedicineName == SelectedMedicine);
            if (med == null) return RedirectToPage();

            // ✅ SellMode Fix
            if (med.SellType == "Unit")
                SellMode = "Tablet";

            else if (med.SellType == "Pack")
                SellMode = "Strip";

            else if (med.SellType == "Both" && string.IsNullOrEmpty(SellMode))
                SellMode = "Tablet";

            // ✅ Stock Units Calculation
            int unitsToSell = Quantity;

            if (SellMode == "Strip")
                unitsToSell = Quantity * med.UnitsPerStrip;

            // ✅ Stock Check
            if (med.StockUnits < unitsToSell)
            {
                TempData["Error"] = "Not enough stock!";
                return RedirectToPage();
            }

            // ✅ Reduce Stock
            med.StockUnits -= unitsToSell;
            _service.UpdateMedicine(med);

            // ✅ Price Fix Based on Mode
            decimal finalPrice = med.SellingPrice;

            if (SellMode == "Strip")
                finalPrice = med.SellingPrice * med.UnitsPerStrip;

            // ✅ Add Bill Item
            BillItems.Add(new BillItem
            {
                MedicineName = med.MedicineName,
                Quantity = Quantity,
                SaleMode = SellMode,
                Price = finalPrice
            });

            HttpContext.Session.SetObject("BillItems", BillItems);

            return RedirectToPage();
        }

        // =======================
        // DELETE ITEM + RESTORE STOCK
        // =======================
        public IActionResult OnPostDeleteItem(string medicineName)
        {
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems") ?? new();

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

        // =======================
        // COMPLETE SALE
        // =======================
        public IActionResult OnPostCompleteSale()
        {
            LoadData();

            if (SelectedCustomer == null)
            {
                TempData["Error"] = "Select customer first!";
                return RedirectToPage();
            }

            Bill bill = new Bill
            {
                InvoiceNumber = InvoiceNumber,
                CustomerName = SelectedCustomer.CustomerName,
                MobileNumber = SelectedCustomer.MobileNumber,

                SubTotal = SubTotal,
                CGST = CGST,
                SGST = SGST,
                GrandTotal = GrandTotal,

                BillDate = DateTime.Now
            };

            _context.Bills.Add(bill);
            _context.SaveChanges();

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

            HttpContext.Session.Clear();

            return RedirectToPage("/SalesSummary");
        }
    }
}
