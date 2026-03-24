using Pharma_Pulse.Data;
using Pharma_Pulse.Models;

public class SalesService
{
    private readonly AppDbContext _context;

    public SalesService(AppDbContext context)
    {
        _context = context;
    }

    // ✅ Add Sale
    public void AddSale(Sale sale, int pharmacyId)
    {
        sale.PharmacyId = pharmacyId;   // VERY IMPORTANT
        _context.Sales.Add(sale);
        _context.SaveChanges();
    }

    // ✅ Get Sales for current pharmacy ONLY
    public List<Sale> GetAllSales(int pharmacyId)
    {
        return _context.Sales
            .Where(s => s.PharmacyId == pharmacyId)
            .ToList();
    }
}