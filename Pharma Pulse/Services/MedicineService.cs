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

        // ✅ Get Medicines for CURRENT pharmacy ONLY
        public List<Medicine> GetAllMedicines(int pharmacyId)
        {
            return _context.Medicines
                .Where(m => m.PharmacyId == pharmacyId)
                .ToList();
        }

        // ✅ Add Medicine (PharmacyId already set from PageModel)
        public void AddMedicine(Medicine medicine)
        {
            _context.Medicines.Add(medicine);
            _context.SaveChanges();
        }

        // ✅ Find Medicine by Name (only for current pharmacy)
        public Medicine? GetMedicineByName(string name, int pharmacyId)
        {
            return _context.Medicines
                .FirstOrDefault(m => m.MedicineName == name && m.PharmacyId == pharmacyId);
        }

        // ✅ Update Medicine (only if belongs to current pharmacy)
        public void UpdateMedicine(Medicine medicine, int pharmacyId)
        {
            var existing = _context.Medicines
                .FirstOrDefault(m => m.Id == medicine.Id && m.PharmacyId == pharmacyId);

            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(medicine);
                _context.SaveChanges();
            }
        }

        // ✅ Delete Medicine (only from current pharmacy)
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