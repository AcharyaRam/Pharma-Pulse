using Microsoft.AspNetCore.Mvc.RazorPages;
using Pharma_Pulse.Data;
using Pharma_Pulse.Models;
using System;
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

        // ✅ Search Term
        public string SearchTerm { get; set; } = "";

        public void OnGet(string search)
        {
            SearchTerm = search;

            var query = _context.Customers.AsQueryable();

            // ============================
            // ✅ SEARCH FILTER (Name + Mobile)
            // ============================
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();

                query = query.Where(c =>
                    (
                        !string.IsNullOrEmpty(c.FirstName) &&
                        c.FirstName.ToLower().StartsWith(term)
                    )
                    ||
                    (
                        !string.IsNullOrEmpty(c.MiddleName) &&
                        c.MiddleName.ToLower().StartsWith(term)
                    )
                    ||
                    (
                        !string.IsNullOrEmpty(c.Surname) &&
                        c.Surname.ToLower().StartsWith(term)
                    )
                    ||
                    (
                        !string.IsNullOrEmpty(c.MobileNumber) &&
                        c.MobileNumber.StartsWith(term)
                    )
                );
            }

            // ✅ Final Customers List
            Customers = query.ToList();
        }
    }
}
