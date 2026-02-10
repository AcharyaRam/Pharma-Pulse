using Pharma_Pulse.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharma_Pulse.Services
{
    public static class MedicineService
    {
        // ✅ Permanent Medicine Storage (Memory List)
        private static List<Medicine> MedicinesList = new List<Medicine>()
        {
            new Medicine
            {
                MedicineName="Paracetamol",
                Category="Pain Relief",
                Stock=120,
                LowStockLimit=20,
                BuyingPrice=1.50m,
                SellingPrice=2.50m,
                ExpiryDate=DateTime.Now.AddMonths(6)
            },

            new Medicine
            {
                MedicineName="Amoxicillin",
                Category="Antibiotic",
                Stock=5,
                LowStockLimit=10,
                BuyingPrice=5.00m,
                SellingPrice=8.00m,
                ExpiryDate=DateTime.Now.AddMonths(2)
            },

            new Medicine
            {
                MedicineName="Cetirizine",
                Category="Anti-Allergy",
                Stock=8,
                LowStockLimit=15,
                BuyingPrice=2.00m,
                SellingPrice=4.00m,
                ExpiryDate=DateTime.Now.AddMonths(3)
            },

            new Medicine
            {
                MedicineName="Metformin",
                Category="Diabetes",
                Stock=200,
                LowStockLimit=30,
                BuyingPrice=3.50m,
                SellingPrice=6.50m,
                ExpiryDate=DateTime.Now.AddMonths(10)
            },

            new Medicine
            {
                MedicineName="Aspirin",
                Category="Cardiac",
                Stock=15,
                LowStockLimit=25,
                BuyingPrice=1.00m,
                SellingPrice=2.00m,
                ExpiryDate=DateTime.Now.AddMonths(7)
            },

            new Medicine
            {
                MedicineName="Atorvastatin",
                Category="Cholesterol",
                Stock=30,
                LowStockLimit=12,
                BuyingPrice=8.00m,
                SellingPrice=12.00m,
                ExpiryDate=DateTime.Now.AddMonths(8)
            },

            new Medicine
            {
                MedicineName="Omeprazole",
                Category="Gastric",
                Stock=9,
                LowStockLimit=10,
                BuyingPrice=4.00m,
                SellingPrice=7.00m,
                ExpiryDate=DateTime.Now.AddMonths(4)
            },

            new Medicine
            {
                MedicineName="Azithromycin",
                Category="Antibiotic",
                Stock=50,
                LowStockLimit=18,
                BuyingPrice=6.00m,
                SellingPrice=10.00m,
                ExpiryDate=DateTime.Now.AddMonths(5)
            },

            new Medicine
            {
                MedicineName="Lisinopril",
                Category="Blood Pressure",
                Stock=6,
                LowStockLimit=8,
                BuyingPrice=7.00m,
                SellingPrice=11.00m,
                ExpiryDate=DateTime.Now.AddMonths(1)
            },

            new Medicine
            {
                MedicineName="Ibuprofen",
                Category="Pain Relief",
                Stock=90,
                LowStockLimit=25,
                BuyingPrice=2.50m,
                SellingPrice=4.50m,
                ExpiryDate=DateTime.Now.AddMonths(9)
            }
        };

        // ✅ Return Same Permanent List
        public static List<Medicine> GetAllMedicines()
        {
            return MedicinesList;
        }

        // ✅ Add New Medicine (FIX for AddMedicine Page)
        public static void AddMedicine(Medicine newMedicine)
        {
            MedicinesList.Add(newMedicine);
        }

        // ✅ Optional: Find Medicine by Name
        public static Medicine? GetMedicineByName(string name)
        {
            return MedicinesList.FirstOrDefault(m => m.MedicineName == name);
        }
        // ✅ Update Stock
        public static void UpdateMedicine(Medicine updatedMedicine)
        {
            var med = MedicinesList
                .FirstOrDefault(m => m.MedicineName == updatedMedicine.MedicineName);

            if (med != null)
            {
                med.Stock = updatedMedicine.Stock;
            }
        }

    }
}
