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

        [BindProperty]
        public int ScrollPos { get; set; }


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
        public decimal SelectedGstPercent { get; set; } = 12;

        [BindProperty]
        public string PaymentMode { get; set; } = "Cash";

        public string InvoiceNumber { get; set; }

        // =======================
        // TOTALS
        // =======================
        public decimal SubTotal { get; set; }
        public decimal GstAmount { get; set; }   // ⭐ NEW PROPERTY
        public decimal GrandTotal { get; set; }

        // =======================
        // PAGE LOAD
        // =======================
        public void OnGet()
        {
            LoadData();
        }

        // =======================
        // LOAD DATA
        // =======================
        private void LoadData()
        {
            AllMedicines = _service.GetAllMedicines()
                .Where(m => m.IsActive)
                .ToList();

            AllCustomers = _context.Customers.ToList();

            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems") ?? new();

            SelectedCustomerId = HttpContext.Session.GetInt32("SelectedCustomerId") ?? 0;
            SelectedCustomer = AllCustomers.FirstOrDefault(c => c.CustomerId == SelectedCustomerId);

            DiscountPercent = Convert.ToDecimal(HttpContext.Session.GetString("Discount") ?? "0");
            PaymentMode = HttpContext.Session.GetString("PaymentMode") ?? "Cash";

            var gstSession = HttpContext.Session.GetString("GST");
            SelectedGstPercent = gstSession == null ? 12 : Convert.ToDecimal(gstSession);

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
            SubTotal = BillItems.Sum(x => x.Total);

            decimal discountAmount = SubTotal * (DiscountPercent / 100);
            decimal afterDiscount = SubTotal - discountAmount;

            // ⭐ GST CALCULATION STORED
            GstAmount = afterDiscount * (SelectedGstPercent / 100);

            GrandTotal = afterDiscount + GstAmount;
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
        // ADD ITEM
        // =======================
        public IActionResult OnPostAddItem()
        {
            LoadData();

            var med = AllMedicines.FirstOrDefault(m => m.MedicineName == SelectedMedicine);
            if (med == null) return RedirectToPage();

            if (!med.IsActive)
            {
                TempData["Error"] = "Medicine is Deactivated!";
                return RedirectToPage();
            }

            if (med.ExpiryDate < DateTime.Today)
            {
                TempData["Error"] = "Medicine Expired!";
                return RedirectToPage();
            }

            if (med.SellType == "Unit") SellMode = "Unit";
            else if (med.SellType == "Pack") SellMode = "Strip";
            else if (med.SellType == "Both" && string.IsNullOrEmpty(SellMode)) SellMode = "Unit";

            decimal finalPrice = med.SellingPrice;
            if (SellMode == "Strip") finalPrice *= med.UnitsPerStrip;

            BillItems.Add(new BillItem
            {
                MedicineName = med.MedicineName,
                BatchNo = med.BatchNo,
                ExpiryDate = med.ExpiryDate,
                MfgDate = med.MfgDate,
                HsnSac = med.HsnSac,
                Quantity = Quantity,
                SaleMode = SellMode,
                Price = finalPrice
            });

            HttpContext.Session.SetObject("BillItems", BillItems);
            return RedirectToPage();
        }

        // =======================
        // DELETE ITEM
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
        // UPDATE BILL
        // =======================
        public IActionResult OnPostUpdateBill()
        {
            HttpContext.Session.SetString("Discount", DiscountPercent.ToString());
            HttpContext.Session.SetString("GST", SelectedGstPercent.ToString());
            HttpContext.Session.SetString("PaymentMode", PaymentMode);

            LoadData();
            TempData["ScrollPos"] = ScrollPos;
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

            foreach (var item in BillItems)
            {
                var med = _service.GetAllMedicines()
                    .FirstOrDefault(m => m.MedicineName == item.MedicineName);

                if (med != null)
                {
                    int unitsToSell = item.Quantity;
                    if (item.SaleMode == "Strip") unitsToSell *= med.UnitsPerStrip;

                    if (med.StockUnits < unitsToSell)
                    {
                        TempData["Error"] = "Not enough stock!";
                        return RedirectToPage();
                    }

                    med.StockUnits -= unitsToSell;
                    _service.UpdateMedicine(med);
                }
            }

            Bill bill = new Bill
            {
                InvoiceNumber = InvoiceNumber,
                CustomerName = SelectedCustomer.FirstName + " " + (SelectedCustomer.MiddleName ?? "") + " " + SelectedCustomer.Surname,
                MobileNumber = SelectedCustomer.MobileNumber,
                SubTotal = SubTotal,
                GrandTotal = GrandTotal,
                DiscountPercent = DiscountPercent,
                PaymentMode = PaymentMode,
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
