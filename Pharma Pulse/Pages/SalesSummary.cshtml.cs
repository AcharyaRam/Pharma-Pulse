using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharma_Pulse.Data;
using Pharma_Pulse.Models;
using Pharma_Pulse.Services.Pdf;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pharma_Pulse.Pages
{
    public class SalesSummaryModel : PageModel
    {
        private readonly AppDbContext _context;

        public SalesSummaryModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Bill> AllBills { get; set; } = new();

        public decimal TotalSales { get; set; }
        public decimal TotalGST { get; set; }
        public decimal TotalProfit { get; set; }

        [BindProperty(SupportsGet = true)]
        public string ReportType { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        // ================= GET =================
        public void OnGet()
        {
            LoadReport();
        }

        // ================= PRINT BILL =================
        public ContentResult OnGetPrintBill(int id)
        {
            var bill = _context.Bills
                .Include(b => b.BillDetails)
                .FirstOrDefault(x => x.Id == id);

            if (bill == null)
                return Content("Invoice Not Found");

            var sb = new StringBuilder();

            sb.Append($@"
<html>
<head>
<style>
body {{
    font-family: 'Courier New', monospace;
    width: 800px;
    margin: auto;
    font-size: 14px;
}}

.center {{
    text-align: center;
}}

.right {{
    text-align: right;
}}

.dashed {{
    border-top: 1px dashed #000;
    margin: 8px 0;
}}
</style>
</head>
<body>

<div class='center'>
    <h3>PHARMA PULSE MEDICAL</h3>
    Near City Hospital, Vadodara, Gujarat<br/>
    GSTIN: 24ABCDE1234F1Z5 | DL No: GJ-122345<br/>
    Phone: 9999999999
</div>

<br/>

<div style='display:flex; justify-content:space-between;'>
<div>
Invoice No: {bill.InvoiceNumber}<br/>
Date: {bill.BillDate:dd-MM-yyyy hh:mm tt}
</div>
<div class='right'>
Customer: {bill.CustomerName}<br/>
Mobile: {bill.MobileNumber}
</div>
</div>

<div class='dashed'></div>

<table width='100%' style='border-collapse:collapse;'>
<tr>
    <th align='left'>#</th>
    <th align='left'>Medicine</th>
    <th align='left'>Batch</th>
    <th align='left'>Expiry</th>
    <th align='right'>Qty</th>
    <th align='right'>MRP</th>
    <th align='right'>Total</th>
</tr>
<tr><td colspan='7'><div class='dashed'></div></td></tr>
");

            int i = 1;
            foreach (var d in bill.BillDetails)
            {
                sb.Append($@"
<tr>
    <td>{i}</td>
    <td>{d.MedicineName}</td>
    <td>{d.BatchNo}</td>
    <td>{d.ExpiryDate:MM-yy}</td>
    <td align='right'>{d.Quantity}</td>
    <td align='right'>{d.Price:0.00}</td>
    <td align='right'>{d.Total:0.00}</td>
</tr>
");
                i++;
            }

            decimal gst = bill.CGST + bill.SGST;

            sb.Append($@"
<tr><td colspan='7'><div class='dashed'></div></td></tr>
</table>

<div class='right'>
Subtotal : ₹{bill.SubTotal:0.00}<br/><br/>
Discount : {bill.DiscountPercent:0.00} %<br/><br/>
GST (12%) : ₹{gst:0.00}
</div>

<div class='dashed'></div>

<div class='right' style='font-size:18px; font-weight:bold;'>
Net Amount : ₹{bill.GrandTotal:0.00}
</div>

<br/><br/>

<div class='right'>
Pharmacist Signature
</div>

<br/><br/>

<div class='center'>
*** Thank You Visit Again ***
</div>

</body>
</html>
");

            return Content(sb.ToString(), "text/html");
        }
        // ================= PDF DOWNLOAD =================
        public IActionResult OnGetExportPdf()
        {
            LoadReport();

            var document = new SalesSummaryPdf(AllBills, TotalSales, TotalGST, TotalProfit);
            byte[] pdf = document.GeneratePdf();

            return File(pdf, "application/pdf", $"SalesReport_{DateTime.Now:ddMMyyyy}.pdf");
        }

        // ================= MAIN LOGIC =================
        private void LoadReport()
        {
            if (string.IsNullOrEmpty(ReportType))
                ReportType = "Daily";

            var billsQuery = _context.Bills
                .Include(b => b.BillDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                string search = SearchTerm.Trim().ToLower();

                billsQuery = billsQuery.Where(b =>
                    (!string.IsNullOrEmpty(b.CustomerName) && b.CustomerName.ToLower().StartsWith(search)) ||
                    (!string.IsNullOrEmpty(b.MobileNumber) && b.MobileNumber.StartsWith(search))
                );
            }

            if (ReportType == "Daily")
            {
                billsQuery = billsQuery.Where(b => b.BillDate.Date == DateTime.Today);
            }
            else if (ReportType == "Monthly")
            {
                billsQuery = billsQuery.Where(b =>
                    b.BillDate.Month == DateTime.Now.Month &&
                    b.BillDate.Year == DateTime.Now.Year);
            }
            else if (ReportType == "Yearly")
            {
                billsQuery = billsQuery.Where(b => b.BillDate.Year == DateTime.Now.Year);
            }
            else if (ReportType == "Custom")
            {
                if (StartDate.HasValue && EndDate.HasValue)
                {
                    billsQuery = billsQuery.Where(b =>
                        b.BillDate.Date >= StartDate.Value.Date &&
                        b.BillDate.Date <= EndDate.Value.Date);
                }
            }

            AllBills = billsQuery
                .OrderByDescending(b => b.BillDate)
                .ToList();

            TotalSales = AllBills.Sum(b => b.GrandTotal);
            TotalGST = AllBills.Sum(b => b.CGST + b.SGST);

            TotalProfit = 0;

            foreach (var bill in AllBills)
            {
                decimal discountMultiplier = 1 - (bill.DiscountPercent / 100);

                foreach (var item in bill.BillDetails)
                {
                    var med = _context.Medicines
                        .FirstOrDefault(m => m.MedicineName == item.MedicineName);

                    if (med == null) continue;

                    // ✅ Actual selling total after discount
                    decimal actualSellingTotal = item.Total * discountMultiplier;

                    // ✅ Cost = buying price × actual units (strips × unitsPerStrip)
                    int actualUnits = item.SaleMode == "Strip"
                        ? item.Quantity * med.UnitsPerStrip
                        : item.Quantity;

                    decimal costTotal = med.BuyingPrice * actualUnits;

                    TotalProfit += actualSellingTotal - costTotal;
                }
            }
        }
    }
}