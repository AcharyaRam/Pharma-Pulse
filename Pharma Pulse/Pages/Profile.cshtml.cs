using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Pharma_Pulse.Data;
using Pharma_Pulse.Models;

namespace Pharma_Pulse.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly AppDbContext _context;

        public ProfileModel(AppDbContext context)
        {
            _context = context;
        }

        // 🔥 Bind Pharmacy Model
        [BindProperty]
        public Pharmacy Pharmacy { get; set; }

        // ================= LOAD DATA =================
        public async Task OnGetAsync()
        {
            Pharmacy = await _context.Pharmacies.FirstOrDefaultAsync();

            if (Pharmacy == null)
            {
                Pharmacy = new Pharmacy
                {
                    PlanName = "Pro Plan",
                    PlanPrice = 999,
                    PlanValidTill = DateTime.Now.AddMonths(1),
                    IsActive = true
                };

                _context.Pharmacies.Add(Pharmacy);
                await _context.SaveChangesAsync();
            }
        }

        // ================= SAVE PROFILE INFO =================
        public async Task<IActionResult> OnPostSaveProfileAsync()
        {
            var existing = await _context.Pharmacies
                .FirstOrDefaultAsync(p => p.Id == Pharmacy.Id);

            if (existing != null)
            {
                existing.PharmacyName = Pharmacy.PharmacyName;
                existing.OwnerName = Pharmacy.OwnerName;
                existing.MobileNumber = Pharmacy.MobileNumber;
                existing.Email = Pharmacy.Email;
                existing.GSTNumber = Pharmacy.GSTNumber;
                existing.DrugLicenseNo = Pharmacy.DrugLicenseNo;
                existing.Address = Pharmacy.Address;

                await _context.SaveChangesAsync();
            }

            return new JsonResult(new { success = true });
        }

        // ================= UPGRADE PLAN =================
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostUpgradePlanAsync([FromBody] UpgradeRequest request)
        {
            var pharmacy = await _context.Pharmacies.FirstOrDefaultAsync();

            if (pharmacy != null)
            {
                pharmacy.PlanName = request.Plan;
                pharmacy.PlanPrice = request.Price;
                pharmacy.PlanValidTill = DateTime.Now.AddMonths(1);
                pharmacy.IsActive = true;

                await _context.SaveChangesAsync();
            }

            return new JsonResult(new { success = true });
        }
    }

    // 🔥 Request Model for Upgrade
    public class UpgradeRequest
    {
        public string Plan { get; set; }
        public decimal Price { get; set; }
    }
}