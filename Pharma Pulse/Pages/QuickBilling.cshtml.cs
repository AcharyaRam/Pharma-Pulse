using Microsoft.AspNetCore.Mvc;
using Pharma_Pulse.Helpers;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services;
using Pharma_Pulse.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class QuickBillingModel : PharmacyPageModel
    {
        private readonly MedicineService _service;
        private readonly AppDbContext _context;

        public QuickBillingModel(MedicineService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        public bool IsReviewMode { get; set; } = false;

        public List<Customer> AllCustomers { get; set; } = new();
        public Customer SelectedCustomer { get; set; }

        [BindProperty]
        public int SelectedCustomerId { get; set; }

        [BindProperty]
        public string SelectedMedicine { get; set; }

        [BindProperty]
        public int Quantity { get; set; }

        [BindProperty]
        public string SellMode { get; set; }

        public List<Medicine> AllMedicines { get; set; } = new();
        public List<BillItem> BillItems { get; set; } = new();

        [BindProperty]
        public decimal DiscountPercent { get; set; }

        [BindProperty]
        public decimal SelectedGstPercent { get; set; } = 12;

        [BindProperty]
        public string PaymentMode { get; set; } = "Cash";

        public string InvoiceNumber { get; set; }

        public decimal SubTotal { get; set; }
        public decimal GstAmount { get; set; }
        public decimal GrandTotal { get; set; }

        // ================= ON GET =================
        public void OnGet(int? id)
        {
            if (id.HasValue && id.Value > 0)
                LoadReviewBill(id.Value);
            else
                LoadNewBill();
        }

        // ================= LOAD NEW BILL =================
        private void LoadNewBill()
        {
            IsReviewMode = false;

            AllMedicines = _service.GetAllMedicines(CurrentPharmacyId)
                .Where(m => m.IsActive &&
                            m.StockUnits > 0 &&
                            m.ExpiryDate.Date >= DateTime.Today)
                .ToList();

            AllCustomers = _context.Customers
                .Where(c => c.PharmacyId == CurrentPharmacyId)
                .ToList();

            BillItems = HttpContext.Session.GetObject<List<BillItem>>("BillItems") ?? new();

            SelectedCustomerId = HttpContext.Session.GetInt32("SelectedCustomerId") ?? 0;
            SelectedCustomer = AllCustomers.FirstOrDefault(c => c.CustomerId == SelectedCustomerId);

            DiscountPercent = Convert.ToDecimal(HttpContext.Session.GetString("Discount") ?? "0");
            SelectedGstPercent = Convert.ToDecimal(HttpContext.Session.GetString("GST") ?? "12");
            PaymentMode = HttpContext.Session.GetString("PaymentMode") ?? "Cash";

            InvoiceNumber = HttpContext.Session.GetString("InvoiceNumber");

            if (string.IsNullOrEmpty(InvoiceNumber))
            {
                InvoiceNumber = "INV-" + DateTime.Now.Ticks.ToString().Substring(10);
                HttpContext.Session.SetString("InvoiceNumber", InvoiceNumber);
            }

            CalculateTotals();
        }

        // ================= REVIEW BILL =================
        private void LoadReviewBill(int billId)
        {
            IsReviewMode = true;

            var bill = _context.Bills
                .FirstOrDefault(x => x.Id == billId && x.PharmacyId == CurrentPharmacyId);

            if (bill == null)
            {
                LoadNewBill();
                return;
            }

            InvoiceNumber = bill.InvoiceNumber;
            PaymentMode = bill.PaymentMode;
            DiscountPercent = bill.DiscountPercent;

            SubTotal = bill.SubTotal;
            GrandTotal = bill.GrandTotal;

            decimal totalGst = bill.CGST + bill.SGST;
            GstAmount = totalGst;

            decimal discountAmount = SubTotal * (bill.DiscountPercent / 100);
            decimal afterDiscount = SubTotal - discountAmount;

            if (afterDiscount > 0)
                SelectedGstPercent = Math.Round((totalGst * 100) / afterDiscount);
            else
                SelectedGstPercent = 0;

            SelectedCustomer = _context.Customers
                .FirstOrDefault(c => c.MobileNumber == bill.MobileNumber
                                  && c.PharmacyId == CurrentPharmacyId);

            BillItems = _context.BillDetails
                .Where(x => x.BillId == bill.Id && x.PharmacyId == CurrentPharmacyId)
                .Select(x => new BillItem
                {
                    MedicineName = x.MedicineName,
                    BatchNo = x.BatchNo,
                    MfgDate = x.MfgDate,
                    ExpiryDate = x.ExpiryDate,
                    HsnSac = x.HsnSac,
                    Quantity = x.Quantity,
                    Price = x.Price,
                    SaleMode = x.SaleMode
                }).ToList();
        }

        // ================= SELECT CUSTOMER =================
        public IActionResult OnPostSelectCustomer()
        {
            HttpContext.Session.SetInt32("SelectedCustomerId", SelectedCustomerId);
            return RedirectToPage();
        }

        // ================= CALCULATE TOTALS =================
        private void CalculateTotals()
        {
            SubTotal = BillItems.Sum(x => x.Total);

            decimal discountAmount = SubTotal * (DiscountPercent / 100);
            decimal afterDiscount = SubTotal - discountAmount;

            GstAmount = afterDiscount * (SelectedGstPercent / 100);
            GrandTotal = afterDiscount + GstAmount;
        }

        // ================= ADD ITEM =================
        public IActionResult OnPostAddItem()
        {
            if (IsReviewMode)
                return RedirectToPage();

            LoadNewBill();

            var med = _service.GetAllMedicines(CurrentPharmacyId)
                .FirstOrDefault(m => m.MedicineName == SelectedMedicine);

            if (med == null || Quantity <= 0)
                return RedirectToPage();

            var existingItem = BillItems
                .FirstOrDefault(x => x.MedicineName == med.MedicineName && x.SaleMode == SellMode);

            int unitsAlreadyInBill = BillItems
                .Where(x => x.MedicineName == med.MedicineName)
                .Sum(x => x.SaleMode == "Strip"
                    ? x.Quantity * med.UnitsPerStrip
                    : x.Quantity);

            int newUnits = SellMode == "Strip"
                ? Quantity * med.UnitsPerStrip
                : Quantity;

            int totalUnitsNeeded = unitsAlreadyInBill + newUnits;

            if (totalUnitsNeeded > med.StockUnits)
            {
                int availableUnits = med.StockUnits - unitsAlreadyInBill;
                TempData["StockError"] = $"Only {availableUnits} units left in stock!";
                return RedirectToPage();
            }

            decimal itemPrice = SellMode == "Strip"
                ? med.SellingPrice * med.UnitsPerStrip
                : med.SellingPrice;

            if (existingItem != null)
                existingItem.Quantity += Quantity;
            else
            {
                BillItems.Add(new BillItem
                {
                    MedicineName = med.MedicineName,
                    BatchNo = med.BatchNo,
                    MfgDate = med.MfgDate,
                    ExpiryDate = med.ExpiryDate,
                    HsnSac = med.HsnSac,
                    Quantity = Quantity,
                    SaleMode = SellMode,
                    Price = itemPrice
                });
            }

            HttpContext.Session.SetObject("BillItems", BillItems);
            return RedirectToPage();
        }

        // ================= UPDATE BILL =================
        // ✅ FIX: Was missing — caused Update Bill to wipe all session data on submit
        public IActionResult OnPostUpdateBill()
        {
            HttpContext.Session.SetString("Discount", DiscountPercent.ToString());
            HttpContext.Session.SetString("GST", SelectedGstPercent.ToString());
            HttpContext.Session.SetString("PaymentMode", PaymentMode ?? "Cash");

            return RedirectToPage();
        }

        // ================= COMPLETE SALE =================
        public JsonResult OnPostCompleteSale()
        {
            LoadNewBill();

            if (SelectedCustomer == null || !BillItems.Any())
                return new JsonResult(new { success = false, message = "Select customer & add items!" });

            Bill bill = new Bill
            {
                PharmacyId = CurrentPharmacyId,
                InvoiceNumber = InvoiceNumber,
                CustomerName = SelectedCustomer.FirstName,
                MobileNumber = SelectedCustomer.MobileNumber,
                SubTotal = SubTotal,
                DiscountPercent = DiscountPercent,
                GstPercent = SelectedGstPercent,
                CGST = GstAmount / 2,
                SGST = GstAmount / 2,
                GrandTotal = GrandTotal,
                PaymentMode = PaymentMode,
                BillDate = DateTime.Now
            };

            _context.Bills.Add(bill);
            _context.SaveChanges();

            foreach (var item in BillItems)
            {
                _context.BillDetails.Add(new BillDetail
                {
                    PharmacyId = CurrentPharmacyId,
                    BillId = bill.Id,
                    MedicineName = item.MedicineName,
                    BatchNo = item.BatchNo,
                    MfgDate = item.MfgDate,
                    ExpiryDate = item.ExpiryDate,
                    HsnSac = item.HsnSac,
                    Quantity = item.Quantity,
                    SaleMode = item.SaleMode,
                    Price = item.Price,
                    Total = item.Total
                });

                var medicine = _context.Medicines
                    .FirstOrDefault(m => m.MedicineName == item.MedicineName
                                      && m.PharmacyId == CurrentPharmacyId);

                if (medicine != null)
                {
                    int unitsToDeduct = item.SaleMode == "Strip"
                        ? item.Quantity * medicine.UnitsPerStrip
                        : item.Quantity;

                    medicine.StockUnits -= unitsToDeduct;
                }
            }

            _context.SaveChanges();

            // ✅ FIX: Remove only billing keys — Session.Clear() was logging the user out
            HttpContext.Session.Remove("BillItems");
            HttpContext.Session.Remove("SelectedCustomerId");
            HttpContext.Session.Remove("Discount");
            HttpContext.Session.Remove("GST");
            HttpContext.Session.Remove("PaymentMode");
            HttpContext.Session.Remove("InvoiceNumber");

            return new JsonResult(new { success = true, message = "Sale Saved Successfully!" });
        }

        // ================= CLEAR BILL =================
        public JsonResult OnPostClearBill()
        {
            // ✅ FIX: Remove only billing keys — Session.Clear() was logging the user out
            HttpContext.Session.Remove("BillItems");
            HttpContext.Session.Remove("SelectedCustomerId");
            HttpContext.Session.Remove("Discount");
            HttpContext.Session.Remove("GST");
            HttpContext.Session.Remove("PaymentMode");
            HttpContext.Session.Remove("InvoiceNumber");

            return new JsonResult(new { success = true });
        }
    }
}