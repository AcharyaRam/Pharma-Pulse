using Pharma_Pulse.Models;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Services
{
    public static class SalesService
    {
        private static List<Sale> SalesList = new List<Sale>();

        // Add Sale Record (Billing ke baad call होगा)
        public static void AddSale(Sale sale)
        {
            SalesList.Add(sale);
        }

        // Get All Sales
        public static List<Sale> GetAllSales()
        {
            return SalesList;
        }
    }
}
