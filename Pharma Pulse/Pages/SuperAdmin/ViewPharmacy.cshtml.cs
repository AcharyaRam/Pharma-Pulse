using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Data;
using Pharma_Pulse.Models;

namespace Pharma_Pulse.Pages.SuperAdmin
{
    public class ViewPharmacyModel : PageModel
    {
        private readonly AppDbContext _db;
        public ViewPharmacyModel(AppDbContext db) => _db = db;

        public Pharmacy Pharmacy { get; set; } = null!;

        public IActionResult OnGet(int id)
        {
            if (HttpContext.Session.GetString("SuperAdmin") != "true")
                return RedirectToPage("/SuperAdmin/SuperAdminLogin");

            Pharmacy = _db.Pharmacies.FirstOrDefault(p => p.Id == id)!;

            if (Pharmacy == null)
                return RedirectToPage("/SuperAdmin/Pharmacies");

            return Page();
        }
    }
}