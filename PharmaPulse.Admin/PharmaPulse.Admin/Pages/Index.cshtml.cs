using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace PharmaPulse.Admin.Pages.Users
{
    public class IndexModel : PageModel
    {
        public List<UserViewModel> Users { get; set; }

        public void OnGet()
        {
            // Static data for now

            Users = new List<UserViewModel>
            {
                new UserViewModel
                {
                    PharmacyName = "Meet Medical",
                    OwnerName = "Borade",
                    Email = "meet@gmail.com",
                    Plan = "Pro",
                    ExpiryDate = DateTime.Now.AddMonths(1),
                    IsActive = true,
                    LastLogin = DateTime.Now.AddDays(-1)
                },
                new UserViewModel
                {
                    PharmacyName = "Health Plus",
                    OwnerName = "Raj Patel",
                    Email = "health@gmail.com",
                    Plan = "Basic",
                    ExpiryDate = DateTime.Now.AddDays(10),
                    IsActive = false,
                    LastLogin = DateTime.Now.AddDays(-5)
                }
            };
        }

        public class UserViewModel
        {
            public string PharmacyName { get; set; }
            public string OwnerName { get; set; }
            public string Email { get; set; }
            public string Plan { get; set; }
            public DateTime ExpiryDate { get; set; }
            public bool IsActive { get; set; }
            public DateTime LastLogin { get; set; }
        }
    }
}