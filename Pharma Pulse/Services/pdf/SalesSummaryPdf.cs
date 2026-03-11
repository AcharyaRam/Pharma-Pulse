using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Pharma_Pulse.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pharma_Pulse.Services.Pdf
{
    public class SalesSummaryPdf : IDocument
    {
        private readonly List<Bill> _bills;
        private readonly decimal _totalSales;
        private readonly decimal _totalGst;
        private readonly decimal _totalProfit;
        private readonly string _webRoot;
        public SalesSummaryPdf(List<Bill> bills, decimal totalSales, decimal totalGst, decimal totalProfit)
        {
            _bills = bills ?? new List<Bill>();
            _totalSales = totalSales;
            _totalGst = totalGst;
            _totalProfit = totalProfit;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeTable);
                page.Footer().Element(ComposeFooter);
            });
        }

        // ---------------- HEADER ----------------
        void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot/images/pulse.png");

                    if (File.Exists(logoPath))
                        row.ConstantItem(60).Height(60).Image(logoPath);

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("PHARMA PULSE MEDICAL").FontSize(16).Bold();
                        c.Item().Text("Sales Summary Report").FontSize(12);
                        c.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy hh:mm tt}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });

                col.Item().PaddingTop(5).LineHorizontal(1);
            });
        }

        // ---------------- TABLE ----------------
        void ComposeTable(IContainer container)
        {
            container.PaddingTop(10).Table(table =>
            {
                // FIXED COLUMN WIDTH (NO OVERLAP)
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(45);     // Bill
                    columns.RelativeColumn(3);      // Customer
                    columns.RelativeColumn(2);      // Mobile
                    columns.RelativeColumn(1.2f);   // GST
                    columns.RelativeColumn(1.5f);   // Total
                    columns.RelativeColumn(1.8f);   // Date
                });

                // HEADER
                table.Header(header =>
                {
                    StyleHeader(header.Cell(), "Bill");
                    StyleHeader(header.Cell(), "Customer");
                    StyleHeader(header.Cell(), "Mobile");
                    StyleHeader(header.Cell().AlignRight(), "GST");
                    StyleHeader(header.Cell().AlignRight(), "Total");
                    StyleHeader(header.Cell().AlignRight(), "Date");
                });

                int i = 0;

                foreach (var b in _bills)
                {
                    var bg = (i % 2 == 0) ? Colors.White : Colors.Grey.Lighten4;

                    table.Cell().Background(bg).Padding(3).Text(b.Id.ToString());
                    table.Cell().Background(bg).Padding(3).Text(b.CustomerName ?? "-");
                    table.Cell().Background(bg).Padding(3).Text(b.MobileNumber ?? "-");
                    table.Cell().Background(bg).Padding(3).AlignRight().Text($"₹ {(b.CGST + b.SGST):0.00}");
                    table.Cell().Background(bg).Padding(3).AlignRight().Text($"₹ {b.GrandTotal:0.00}");
                    table.Cell().Background(bg).Padding(3).AlignRight().Text(b.BillDate.ToString("dd MMM yyyy"));

                    i++;
                }
            });
        }

        // ---------------- FOOTER ----------------
        void ComposeFooter(IContainer container)
        {
            container.AlignRight().Column(col =>
            {
                col.Item().LineHorizontal(1);
                col.Item().Text($"Total Sales : ₹ {_totalSales:0.00}");
                col.Item().Text($"Total GST   : ₹ {_totalGst:0.00}");
                col.Item().Text($"Total Profit: ₹ {_totalProfit:0.00}").Bold();
            });
        }

        // HEADER STYLE
        static void StyleHeader(IContainer container, string text)
        {
            container
                .Background(Colors.Grey.Lighten2)
                .PaddingVertical(6)
                .PaddingHorizontal(4)
                .Text(text)
                .SemiBold();
        }
    }
}