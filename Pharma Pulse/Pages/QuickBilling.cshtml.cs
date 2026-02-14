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
        private readonly AppDbContext _context;

        public QuickBillingModel(MedicineService service, AppDbContext context)
        {
            _service = service;
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
        public string SellMode { get; set; }

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
        public decimal IGST { get; set; }

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
            decimal gstPercent = _context.GstSettings.FirstOrDefault()?.GstPercent ?? 12;

            SubTotal = BillItems.Sum(x => x.Total);

            bool isInterStateSale = false; // future me customer state se check kar sakte

            if (isInterStateSale)
            {
                // ✅ IGST Full
                IGST = SubTotal * (gstPercent / 100);
                CGST = 0;
                SGST = 0;
            }
            else
            {
                // ✅ CGST + SGST Split
                CGST = SubTotal * (gstPercent / 100) / 2;
                SGST = SubTotal * (gstPercent / 100) / 2;
                IGST = 0;
            }

            decimal discountAmount = SubTotal * (DiscountPercent / 100);

            GrandTotal = (SubTotal - discountAmount) + CGST + SGST + IGST;
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
        // ADD ITEM (NO STOCK REDUCE HERE)
        // =======================
        public IActionResult OnPostAddItem()
        {
            LoadData();

            var med = AllMedicines.FirstOrDefault(m => m.MedicineName == SelectedMedicine);
            if (med == null) return RedirectToPage();

            // ❌ Block Expired Medicine
            if (med.ExpiryDate < DateTime.Today)
            {
                TempData["Error"] = "Medicine Expired!";
                return RedirectToPage();
            }

            // SellMode Auto Fix
            if (med.SellType == "Unit")
                SellMode = "Tablet";
            else if (med.SellType == "Pack")
                SellMode = "Strip";
            else if (med.SellType == "Both" && string.IsNullOrEmpty(SellMode))
                SellMode = "Tablet";

            // Price Fix
            decimal finalPrice = med.SellingPrice;

            if (SellMode == "Strip")
                finalPrice = med.SellingPrice * med.UnitsPerStrip;

            // Add Bill Item with Batch + Expiry + HSN
            BillItems.Add(new BillItem
            {
                MedicineName = med.MedicineName,

                BatchNo = med.BatchNo,
                ExpiryDate = med.ExpiryDate,
                HsnSac = med.HsnSac,

                Quantity = Quantity,
                SaleMode = SellMode,
                Price = finalPrice
            });

            HttpContext.Session.SetObject("BillItems", BillItems);

            return RedirectToPage();
        }

        // =======================
        // DELETE ITEM (NO STOCK RESTORE)
        // =======================
        public IActionResult OnPostDeleteItem(string medicineName)
        {
            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems") ?? new();

            var item = BillItems.FirstOrDefault(x => x.MedicineName == medicineName);

            if (item != null)
            {
                BillItems.Remove(item);
                HttpContext.Session.SetObject("BillItems", BillItems);
            }

            return RedirectToPage();
        }

        // =======================
        // COMPLETE SALE (STOCK REDUCE HERE)
        // =======================
        public IActionResult OnPostCompleteSale()
        {
            LoadData();

            if (SelectedCustomer == null)
            {
                TempData["Error"] = "Select customer first!";
                return RedirectToPage();
            }

            // ✅ Reduce Stock Now
            foreach (var item in BillItems)
            {
                var med = _service.GetAllMedicines()
                    .FirstOrDefault(m => m.MedicineName == item.MedicineName);

                if (med != null)
                {
                    int unitsToSell = item.Quantity;

                    if (item.SaleMode == "Strip")
                        unitsToSell *= med.UnitsPerStrip;

                    if (med.StockUnits < unitsToSell)
                    {
                        TempData["Error"] = "Not enough stock!";
                        return RedirectToPage();
                    }

                    med.StockUnits -= unitsToSell;
                    _service.UpdateMedicine(med);
                }
            }

            // ✅ Save Bill
            Bill bill = new Bill
            {
                InvoiceNumber = InvoiceNumber,
                CustomerName = SelectedCustomer.CustomerName,
                MobileNumber = SelectedCustomer.MobileNumber,

                SubTotal = SubTotal,
                CGST = CGST,
                SGST = SGST,
                GrandTotal = GrandTotal,

                DiscountPercent = DiscountPercent,
                PaymentMode = PaymentMode,

                BillDate = DateTime.Now
            };


            _context.Bills.Add(bill);
            _context.SaveChanges();

            // ✅ Save Bill Details with Batch + Expiry + HSN
            foreach (var item in BillItems)
            {
                _context.BillDetails.Add(new BillDetail
                {
                    BillId = bill.Id,

                    MedicineName = item.MedicineName,
                    BatchNo = item.BatchNo,
                    ExpiryDate = item.ExpiryDate,
                    HsnSac = item.HsnSac,

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
