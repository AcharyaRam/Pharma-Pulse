using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Pharma_Pulse.Pages.SuperAdmin
{
    public class SuperAdminLoginModel : PageModel
    {
        [BindProperty] public string Username { get; set; } = string.Empty;
        [BindProperty] public string Password { get; set; } = string.Empty;

        private const string AdminUser = "superadmin";
        private const string AdminPass = "Admin@123";

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (Username == AdminUser && Password == AdminPass)
            {
                HttpContext.Session.SetString("SuperAdmin", "true");
                return RedirectToPage("/SuperAdmin/SuperAdminDashboard");
            }
            ModelState.AddModelError("", "Invalid credentials");
            return Page();
        }
    }
}