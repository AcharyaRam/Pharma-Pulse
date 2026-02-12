using Pharma_Pulse.Data;
using Pharma_Pulse.Models;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Services
{
    public class SalesService
    {
        private readonly AppDbContext _context;

        public SalesService(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Add Sale Record into Database
        public void AddSale(Sale sale)
        {
            _context.Sales.Add(sale);
            _context.SaveChanges();
        }

        // ✅ Get All Sales from Database
        public List<Sale> GetAllSales()
        {
            return _context.Sales.ToList();
        }
    }
}
