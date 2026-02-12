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

        public DbSet<Medicine> Medicines { get; set; }
    }
}
