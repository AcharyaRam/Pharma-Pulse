using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Data;
using Pharma_Pulse.Models;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Pages
{
    public class TotalCustomersModel : PageModel
    {
        private readonly AppDbContext _context;

        public TotalCustomersModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Customer> Customers { get; set; } = new();

        public string SearchTerm { get; set; } = "";

        public void OnGet(string search)
        {
            SearchTerm = search;

            Customers = _context.Customers
                .Where(c =>
                    string.IsNullOrEmpty(search) ||
                    c.FirstName.Contains(search) ||
                    c.MobileNumber.Contains(search)
                )
                .ToList();
        }
    }
}
