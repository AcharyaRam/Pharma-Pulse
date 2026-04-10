using Pharma_Pulse.Data;
using System;
using System.Linq;

namespace Pharma_Pulse.Services
{
    public class ChatService
    {
        private readonly AppDbContext _context;

        public ChatService(AppDbContext context)
        {
            _context = context;
        }

        public string GetReply(string message, int pharmacyId)
        {
            var query = message.ToLower();

            // STOCK
            if (query.Contains("stock"))
            {
                var med = _context.Medicines
                    .Where(m => m.PharmacyId == pharmacyId)
                    .ToList()
                    .FirstOrDefault(m => query.Contains(m.MedicineName.ToLower()));

                if (med != null)
                    return $"{med.MedicineName} stock: {med.StockUnits}";

                return "Medicine not found";
            }

            // EXPIRY
            if (query.Contains("expiry"))
            {
                var count = _context.Medicines
                    .Count(m => m.PharmacyId == pharmacyId &&
                                m.ExpiryDate <= DateTime.Now.AddDays(30));

                return $"{count} medicines expiring soon";
            }

            // SALES
            if (query.Contains("sales"))
            {
                var total = _context.Bills
                    .Where(b => b.PharmacyId == pharmacyId &&
                                b.BillDate.Date == DateTime.Today)
                    .Sum(b => b.GrandTotal);

                return $"Today's sales: ₹{total}";
            }

            // TOP
            if (query.Contains("top"))
            {
                var top = _context.BillDetails
                    .Where(d => d.PharmacyId == pharmacyId &&
                                d.Bill.BillDate.Date == DateTime.Today)
                    .GroupBy(d => d.MedicineName)
                    .Select(g => new {
                        Name = g.Key,
                        Qty = g.Sum(x => x.Quantity)
                    })
                    .OrderByDescending(x => x.Qty)
                    .FirstOrDefault();

                if (top != null)
                    return $"Top medicine: {top.Name}";
            }

            return "Samajh nahi aaya 😅";
        }
    }
}