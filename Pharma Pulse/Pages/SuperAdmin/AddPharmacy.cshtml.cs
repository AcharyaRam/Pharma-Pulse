using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Data;
using Pharma_Pulse.Models;

namespace Pharma_Pulse.Pages.SuperAdmin
{
    public class AddPharmacyModel : PageModel
    {
        private readonly AppDbContext _db;
        public AddPharmacyModel(AppDbContext db) => _db = db;

        [BindProperty]
        public PharmacyInputModel Input { get; set; } = new();

        public class PharmacyInputModel
        {
            public string PharmacyName { get; set; } = string.Empty;
            public string OwnerName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string MobileNumber { get; set; } = string.Empty;
            public string GSTNumber { get; set; } = string.Empty;
            public string DrugLicenseNo { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string PlanName { get; set; } = "Basic";
            public decimal PlanPrice { get; set; } = 0;
            public DateTime PlanValidTill { get; set; } = DateTime.Now.AddMonths(1);
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetString("SuperAdmin") != "true")
                return RedirectToPage("/SuperAdmin/SuperAdminLogin");
            return Page();
        }

        public IActionResult OnPost()
        {
            if (HttpContext.Session.GetString("SuperAdmin") != "true")
                return RedirectToPage("/SuperAdmin/SuperAdminLogin");

            if (_db.Pharmacies.Any(p => p.Username == Input.Username))
            {
                TempData["Error"] = "Username already exists. Please choose a different one.";
                return Page();
            }

            var pharmacy = new Pharmacy
            {
                PharmacyName = Input.PharmacyName,
                OwnerName = Input.OwnerName,
                Email = Input.Email,
                MobileNumber = Input.MobileNumber,
                GSTNumber = Input.GSTNumber,
                DrugLicenseNo = Input.DrugLicenseNo,
                Address = Input.Address,
                PlanName = Input.PlanName,
                PlanPrice = Input.PlanPrice,
                PlanValidTill = Input.PlanValidTill,
                Username = Input.Username,
                PlainPassword = Input.Password,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Pharmacies.Add(pharmacy);
            _db.SaveChanges();

            TempData["Success"] = $"Pharmacy '{pharmacy.PharmacyName}' created successfully!";
            return RedirectToPage("/SuperAdmin/Pharmacies");
        }
    }
}