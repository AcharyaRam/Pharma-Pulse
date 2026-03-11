using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PharmaPulse.Admin.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        public string? ErrorMessage { get; set; }

        public IActionResult OnPost()
        {
            // Temporary Hardcoded Login
            if (Email == "admin@gmail.com" && Password == "123456")
            {
                return RedirectToPage("/Dashboard");
            }

            ErrorMessage = "Invalid Email or Password";
            return Page();
        }
    }
}