using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pharma_Pulse.Models;

namespace Pharma_Pulse.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Pharmacy> Pharmacies { get; set; }
        public DbSet<Medicine> Medicines { get; set; }

        // ✅ GST Setting Table (New)
        public DbSet<GstSetting> GstSettings { get; set; }

        // sale 
        public DbSet<Sale> Sales { get; set; }

        //bill details
        public DbSet<Bill> Bills { get; set; }
        public DbSet<BillDetail> BillDetails { get; set; }

        //customer details
        public DbSet<Customer> Customers { get; set; }




    }
}
