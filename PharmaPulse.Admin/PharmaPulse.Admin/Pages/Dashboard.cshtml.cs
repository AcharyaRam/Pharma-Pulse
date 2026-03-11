using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace PharmaPulse.Admin.Pages
{
    public class DashboardModel : PageModel
    {
        public int TotalPharmacies { get; set; }
        public int ActivePharmacies { get; set; }
        public int SuspendedPharmacies { get; set; }
        public int TotalUsers { get; set; }

        public List<string> RecentActivities { get; set; }

        public void OnGet()
        {
            // Temporary Static Data (Later connect database)

            TotalPharmacies = 30;
            ActivePharmacies = 24;
            SuspendedPharmacies = 6;
            TotalUsers = 120;

            RecentActivities = new List<string>
            {
                "New Pharmacy Registered",
                "Admin Account Created",
                "Pharmacy Suspended",
                "User Password Reset",
                "System Backup Completed"
            };
        }
    }
}