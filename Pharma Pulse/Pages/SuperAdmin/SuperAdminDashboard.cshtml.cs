using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharma_Pulse.Data;  // ✅ your actual namespace

namespace Pharma_Pulse.Pages.SuperAdmin
{
    public class PharmacyRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string OwnerName { get; set; } = "";
        public string City { get; set; } = "";
        public string Plan { get; set; } = "Basic";
        public string Status { get; set; } = "Active";
        public string StatusClass => Status.ToLower() switch
        {
            "active" => "active",
            "blocked" => "blocked",
            _ => "pending"
        };
        public DateTime JoinedDate { get; set; }
    }

    public class SuperAdminDashboardModel : PageModel
    {
        private readonly AppDbContext _db;  // ✅ fixed
        public SuperAdminDashboardModel(AppDbContext db) => _db = db;  // ✅ fixed

        public int TotalPharmacies { get; set; }
        public int ActivePharmacies { get; set; }
        public int BlockedPharmacies { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueGrowth { get; set; }
        public List<PharmacyRow> RecentPharmacies { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                TotalPharmacies = await _db.Pharmacies.CountAsync();
                ActivePharmacies = await _db.Pharmacies.CountAsync(p => p.IsActive);
                BlockedPharmacies = await _db.Pharmacies.CountAsync(p => !p.IsActive);
                TotalUsers = TotalPharmacies;
                TotalRevenue = await _db.Sales.SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

                var now = DateTime.Now;
                var thisMonthStart = new DateTime(now.Year, now.Month, 1);
                var lastMonthStart = thisMonthStart.AddMonths(-1);

                var thisMonth = await _db.Sales
                    .Where(s => s.SaleDate >= thisMonthStart)
                    .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

                var lastMonth = await _db.Sales
                    .Where(s => s.SaleDate >= lastMonthStart && s.SaleDate < thisMonthStart)
                    .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

                RevenueGrowth = lastMonth > 0
                    ? Math.Round((thisMonth - lastMonth) / lastMonth * 100, 1)
                    : 0;

                RecentPharmacies = await _db.Pharmacies
                    .OrderByDescending(p => p.Id)
                    .Take(10)
                    .Select(p => new PharmacyRow
                    {
                        Id = p.Id,
                        Name = p.PharmacyName,
                        OwnerName = p.OwnerName ?? "N/A",
                        City = p.Address ?? "N/A",
                        Plan = p.PlanName ?? "Basic",
                        Status = p.IsActive ? "Active" : "Blocked",
                        JoinedDate = p.PlanValidTill
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // This will show the real error in browser
                throw new Exception("SuperAdmin Dashboard Error: " + ex.Message, ex);
            }
        }
    }
}