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

        // Hardcoded plan prices — change here if needed
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

            // Plan
            public string PlanName { get; set; } = string.Empty;
            public decimal PlanPrice { get; set; } = 0;   // set by server, not user
            public decimal DiscountPercent { get; set; } = 0;
            public decimal NetPrice { get; set; } = 0;   // calculated by server
            public DateTime PlanValidTill { get; set; } = DateTime.Now.AddMonths(1);

            // Payment
            public string PaymentMode { get; set; } = string.Empty;

            // Login
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

            // Server-side: override price from hardcoded dictionary (never trust JS value)
            if (!PlanPrices.TryGetValue(Input.PlanName, out decimal basePrice))
            {
                TempData["Error"] = "Invalid plan selected.";
                return Page();
            }
            Input.PlanPrice = basePrice;
            Input.NetPrice = Math.Round(basePrice - (basePrice * Input.DiscountPercent / 100), 2);

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
                DiscountPercent = Input.DiscountPercent,
                NetPrice = Input.NetPrice,
                PlanValidTill = Input.PlanValidTill,
                PaymentMode = Input.PaymentMode,
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