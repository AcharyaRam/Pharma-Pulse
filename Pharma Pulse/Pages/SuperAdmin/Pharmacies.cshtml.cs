using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Data;
using Pharma_Pulse.Models;

namespace Pharma_Pulse.Pages.SuperAdmin
{
    public class PharmaciesModel : PageModel
    {
        private readonly AppDbContext _db;
        public PharmaciesModel(AppDbContext db) => _db = db;

        public List<Pharmacy> Pharmacies { get; set; } = new();

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("SuperAdmin") != "true")
                return RedirectToPage("/SuperAdmin/SuperAdminLogin");

            Pharmacies = _db.Pharmacies
                            .OrderByDescending(p => p.CreatedAt)
                            .ToList();
            return Page();
        }

        // Toggle Active / Blocked
        public IActionResult OnGetToggle(int id)
        {
            if (HttpContext.Session.GetString("SuperAdmin") != "true")
                return RedirectToPage("/SuperAdmin/SuperAdminLogin");

            var pharmacy = _db.Pharmacies.Find(id);
            if (pharmacy != null)
            {
                pharmacy.IsActive = !pharmacy.IsActive;
                _db.SaveChanges();
                TempData["Success"] = $"'{pharmacy.PharmacyName}' has been {(pharmacy.IsActive ? "activated" : "blocked")}.";
            }
            else
            {
                TempData["Error"] = "Pharmacy not found.";
            }

            return RedirectToPage();
        }

        // Delete pharmacy
        public IActionResult OnGetDelete(int id)
        {
            if (HttpContext.Session.GetString("SuperAdmin") != "true")
                return RedirectToPage("/SuperAdmin/SuperAdminLogin");

            var pharmacy = _db.Pharmacies.Find(id);
            if (pharmacy != null)
            {
                _db.Pharmacies.Remove(pharmacy);
                _db.SaveChanges();
                TempData["Success"] = $"'{pharmacy.PharmacyName}' has been deleted.";
            }
            else
            {
                TempData["Error"] = "Pharmacy not found.";
            }

            return RedirectToPage();
        }
    }
}