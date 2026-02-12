using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Data;
using Pharma_Pulse.Models;
using System;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class GstSettingsModel : PageModel
    {
        private readonly AppDbContext _context;

        public GstSettingsModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public decimal GstPercent { get; set; }

        public string LastUpdated { get; set; }

        public void OnGet()
        {
            var setting = _context.GstSettings.FirstOrDefault();

            if (setting != null)
            {
                GstPercent = setting.GstPercent;
                LastUpdated = setting.UpdatedOn.ToString("dd-MMM-yyyy");
            }
            else
            {
                GstPercent = 5;
                LastUpdated = "Not Set";
            }
        }

        public IActionResult OnPost()
        {
            var setting = _context.GstSettings.FirstOrDefault();

            if (setting != null)
            {
                setting.GstPercent = GstPercent;
                setting.UpdatedOn = DateTime.Now;
            }

            _context.SaveChanges();

            TempData["Success"] = "GST Updated Successfully!";

            return RedirectToPage("/GstSettings");
        }
    }
}
