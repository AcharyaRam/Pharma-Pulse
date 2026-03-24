using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Data;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _db;

        public LoginModel(AppDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            var pharmacy = _db.Pharmacies
                .FirstOrDefault(p => p.Username == Username && p.IsActive);

            // ❌ Invalid login
            if (pharmacy == null || !BCrypt.Net.BCrypt.Verify(Password, pharmacy.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid Username or Password");
                return Page();
            }

            // ✅ Session store
            HttpContext.Session.SetInt32("PharmacyId", pharmacy.Id);
            HttpContext.Session.SetString("PharmacyName", pharmacy.PharmacyName);

            return RedirectToPage("/Dashboard");
        }
    }
}