using Microsoft.EntityFrameworkCore;
using Pharma_Pulse.Data;
using Pharma_Pulse.Models;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Services
{
    public class MedicineService
    {
        private readonly AppDbContext _context;

        public MedicineService(AppDbContext context)
        {
            _context = context;
        }

        public List<Medicine> GetAllMedicines(int pharmacyId)
        {
            return _context.Medicines
                .Where(m => m.PharmacyId == pharmacyId)
                .ToList();
        }

        public void AddMedicine(Medicine medicine)
        {
            _context.Medicines.Add(medicine);
            _context.SaveChanges();
        }

        public Medicine? GetMedicineByName(string name, int pharmacyId)
        {
            return _context.Medicines
                .FirstOrDefault(m => m.MedicineName == name && m.PharmacyId == pharmacyId);
        }

        public Medicine? GetMedicineById(int id, int pharmacyId)
        {
            return _context.Medicines
                .FirstOrDefault(m => m.Id == id && m.PharmacyId == pharmacyId);
        }

        // ✅ FINAL FIX: Raw SQL — EF Core tracking bypass
        public void UpdateMedicine(Medicine medicine, int pharmacyId)
        {
            _context.Database.ExecuteSqlRaw(
                @"UPDATE Medicines SET
                    MedicineName = {0},
                    Category = {1},
                    BatchNo = {2},
                    MfgDate = {3},
                    StockUnits = {4},
                    LowStockLimit = {5},
                    BuyingPrice = {6},
                    SellingPrice = {7},
                    HsnSac = {8},
                    SellType = {9},
                    ExpiryDate = {10},
                    UnitsPerStrip = {11},
                    GstPercent = {12},
                    SupplierName = {13},
                    IsActive = {14}
                  WHERE Id = {15} AND PharmacyId = {16}",
                medicine.MedicineName,
                medicine.Category,
                medicine.BatchNo,
                medicine.MfgDate,
                medicine.StockUnits,
                medicine.LowStockLimit,
                medicine.BuyingPrice,
                medicine.SellingPrice,
                medicine.HsnSac,
                medicine.SellType,
                medicine.ExpiryDate,
                medicine.UnitsPerStrip,
                medicine.GstPercent,
                medicine.SupplierName,
                medicine.IsActive,
                medicine.Id,
                pharmacyId
            );
        }

        public void DeleteMedicine(int id, int pharmacyId)
        {
            var medicine = _context.Medicines
                .FirstOrDefault(m => m.Id == id && m.PharmacyId == pharmacyId);

            if (medicine != null)
            {
                _context.Medicines.Remove(medicine);
                _context.SaveChanges();
            }
        }
    }
}