using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

        [BindProperty]
        public Customer Customer { get; set; }

        // ============================
        // ✅ GET: Load Customers + Search
        // ============================
        public void OnGet(string search)
        {
            SearchTerm = search;

            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();

                query = query.Where(c =>
                    (!string.IsNullOrEmpty(c.FirstName) &&
                     c.FirstName.ToLower().StartsWith(term))

                    || (!string.IsNullOrEmpty(c.MiddleName) &&
                        c.MiddleName.ToLower().StartsWith(term))

                    || (!string.IsNullOrEmpty(c.Surname) &&
                        c.Surname.ToLower().StartsWith(term))

                    || (!string.IsNullOrEmpty(c.MobileNumber) &&
                        c.MobileNumber.StartsWith(term))
                );
            }

            Customers = query.ToList();
        }

        // ============================
        // ✅ POST: Save / Update Customer
        // ============================
        public async Task<IActionResult> OnPostSaveCustomerAsync()
        {
            if (!ModelState.IsValid)
            {
                Customers = _context.Customers.ToList();
                return Page();
            }

            if (Customer.CustomerId > 0)
            {
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == Customer.CustomerId);

                if (existingCustomer != null)
                {
                    existingCustomer.FirstName = Customer.FirstName;
                    existingCustomer.MiddleName = Customer.MiddleName;
                    existingCustomer.Surname = Customer.Surname;
                    existingCustomer.MobileNumber = Customer.MobileNumber;
                    existingCustomer.Email = Customer.Email;
                    existingCustomer.GSTNumber = Customer.GSTNumber;
                    existingCustomer.Gender = Customer.Gender;
                    existingCustomer.DateOfBirth = Customer.DateOfBirth;
                    existingCustomer.Age = Customer.Age;
                    existingCustomer.City = Customer.City;
                    existingCustomer.CustomerType = Customer.CustomerType;
                    existingCustomer.DoctorReference = Customer.DoctorReference;
                    existingCustomer.MedicalNotes = Customer.MedicalNotes;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Customer Updated Successfully!";
                }
            }
            else
            {
                await _context.Customers.AddAsync(Customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Customer Added Successfully!";
            }

            return RedirectToPage();
        }
    }
}