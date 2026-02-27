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

        // ✅ Get All Medicines from Database
        public List<Medicine> GetAllMedicines()
        {
            return _context.Medicines.ToList();
        }

        // ✅ Add Medicine into Database
        public void AddMedicine(Medicine medicine)
        {
            _context.Medicines.Add(medicine);
            _context.SaveChanges();
        }

        // ✅ Find Medicine by Name
        public Medicine? GetMedicineByName(string name)
        {
            return _context.Medicines.FirstOrDefault(m => m.MedicineName == name);
        }

        // ✅ Update Medicine Stock
        public void UpdateMedicine(Medicine medicine)
        {
            _context.Medicines.Update(medicine);
            _context.SaveChanges();
        }
        public void DeleteMedicine(int id)
        {
            var medicine = _context.Medicines.FirstOrDefault(m => m.Id == id);
            if (medicine != null)
            {
                _context.Medicines.Remove(medicine);
                _context.SaveChanges();
            }
        }
    }
}
