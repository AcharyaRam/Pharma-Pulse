using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Data;
using Pharma_Pulse.Models;

namespace Pharma_Pulse.Pages.SuperAdmin
{
    public class EditPharmacyModel : PageModel
    {
        private readonly AppDbContext _db;
        public EditPharmacyModel(AppDbContext db) => _db = db;

        private static readonly Dictionary<string, decimal> PlanPrices = new()
        {
            { "Basic",    999m  },
            { "Standard", 2499m },
            { "Premium",  4999m }
        };

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
            public string PlanName { get; set; } = string.Empty;
            public decimal PlanPrice { get; set; } = 0;
            public decimal DiscountPercent { get; set; } = 0;
            public decimal NetPrice { get; set; } = 0;
            public DateTime PlanValidTill { get; set; } = DateTime.Now.AddMonths(1);
            public string PaymentMode { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty; // blank = keep current

            public string LicenseType { get; set; } = string.Empty;
            public string StateCode { get; set; } = string.Empty;
            public string PANNumber { get; set; } = string.Empty;
            public string? FSSAINumber { get; set; }
        }

        // GET — load existing pharmacy data into form
        public IActionResult OnGet(int id)
        {
            if (HttpContext.Session.GetString("SuperAdmin") != "true")
                return RedirectToPage("/SuperAdmin/SuperAdminLogin");

            var pharmacy = _db.Pharmacies.Find(id);
            if (pharmacy == null)
                return RedirectToPage("/SuperAdmin/Pharmacies");

            // Populate form fields from DB
            Input = new PharmacyInputModel
            {
                PharmacyName = pharmacy.PharmacyName,
                OwnerName = pharmacy.OwnerName,
                Email = pharmacy.Email,
                MobileNumber = pharmacy.MobileNumber,
                GSTNumber = pharmacy.GSTNumber,
                DrugLicenseNo = pharmacy.DrugLicenseNo,
                Address = pharmacy.Address,
                PlanName = pharmacy.PlanName,
                PlanPrice = pharmacy.PlanPrice,
                DiscountPercent = pharmacy.DiscountPercent,
                NetPrice = pharmacy.NetPrice,
                PlanValidTill = pharmacy.PlanValidTill,
                PaymentMode = pharmacy.PaymentMode,
                Username = pharmacy.Username,
                Password = string.Empty , // never pre-fill password
                // ✅ Yeh add karo
                LicenseType = pharmacy.LicenseType,
                StateCode = pharmacy.StateCode,
                PANNumber = pharmacy.PANNumber,
                FSSAINumber = pharmacy.FSSAINumber
            };

            return Page();
        }

        // POST — save updated data
        public IActionResult OnPost(int id)
        {
            if (HttpContext.Session.GetString("SuperAdmin") != "true")
                return RedirectToPage("/SuperAdmin/SuperAdminLogin");

            var pharmacy = _db.Pharmacies.Find(id);
            if (pharmacy == null)
                return RedirectToPage("/SuperAdmin/Pharmacies");

            // Username conflict check (exclude self)
            if (_db.Pharmacies.Any(p => p.Username == Input.Username && p.Id != id))
            {
                TempData["Error"] = "Username already exists. Please choose a different one.";
                return Page();
            }

            // Server-side plan price calculation (never trust JS value)
            if (!PlanPrices.TryGetValue(Input.PlanName, out decimal basePrice))
            {
                TempData["Error"] = "Invalid plan selected.";
                return Page();
            }
            Input.PlanPrice = basePrice;
            Input.NetPrice = Math.Round(basePrice - (basePrice * Input.DiscountPercent / 100), 2);

            // Update fields
            pharmacy.PharmacyName = Input.PharmacyName;
            pharmacy.OwnerName = Input.OwnerName;
            pharmacy.Email = Input.Email;
            pharmacy.MobileNumber = Input.MobileNumber;
            pharmacy.GSTNumber = Input.GSTNumber;
            pharmacy.DrugLicenseNo = Input.DrugLicenseNo;
            pharmacy.Address = Input.Address;
            pharmacy.PlanName = Input.PlanName;
            pharmacy.PlanPrice = Input.PlanPrice;
            pharmacy.DiscountPercent = Input.DiscountPercent;
            pharmacy.NetPrice = Input.NetPrice;
            pharmacy.PlanValidTill = Input.PlanValidTill;
            pharmacy.PaymentMode = Input.PaymentMode;
            pharmacy.Username = Input.Username;
            pharmacy.LicenseType = Input.LicenseType;
            pharmacy.StateCode = Input.StateCode;
            pharmacy.PANNumber = Input.PANNumber;
            pharmacy.FSSAINumber = Input.FSSAINumber;

            // Password — only update if user typed a new one
            if (!string.IsNullOrWhiteSpace(Input.Password))
            {
                pharmacy.PlainPassword = Input.Password;
                pharmacy.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password);
            }

            _db.SaveChanges();

            TempData["Success"] = $"Pharmacy '{pharmacy.PharmacyName}' updated successfully!";
            return RedirectToPage("/SuperAdmin/Pharmacies");
        }
    }
}
