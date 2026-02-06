
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPage.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            // Hardcoded login
            if (Username == "admin" && Password == "admin123")
            {
                return RedirectToPage("/Login");
            }

            ModelState.AddModelError("", "Invalid Username or Password");
            return Page();
        }
    }
}

