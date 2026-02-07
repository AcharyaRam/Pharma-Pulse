using Pharma_Pulse.Models;
using System;
using System.Collections.Generic;

namespace Pharma_Pulse.Services
{
    public static class MedicineService
    {
        // Common Medicines List (10 Medicines)
        public static List<Medicine> GetAllMedicines()
        {
            return new List<Medicine>
            {
                new Medicine
                {
                    MedicineName="Paracetamol",
                    Category="Pain Relief",
                    Stock=120,
                    ExpiryDate=DateTime.Now.AddMonths(6)
                },

                new Medicine
                {
                    MedicineName="Amoxicillin",
                    Category="Antibiotic",
                    Stock=5,
                    ExpiryDate=DateTime.Now.AddMonths(2)
                },

                new Medicine
                {
                    MedicineName="Cetirizine",
                    Category="Anti-Allergy",
                    Stock=8,
                    ExpiryDate=DateTime.Now.AddMonths(3)
                },

                new Medicine
                {
                    MedicineName="Metformin",
                    Category="Diabetes",
                    Stock=200,
                    ExpiryDate=DateTime.Now.AddMonths(10)
                },

                new Medicine
                {
                    MedicineName="Aspirin",
                    Category="Cardiac",
                    Stock=15,
                    ExpiryDate=DateTime.Now.AddMonths(7)
                },

                new Medicine
                {
                    MedicineName="Atorvastatin",
                    Category="Cholesterol",
                    Stock=30,
                    ExpiryDate=DateTime.Now.AddMonths(8)
                },

                new Medicine
                {
                    MedicineName="Omeprazole",
                    Category="Gastric",
                    Stock=9,
                    ExpiryDate=DateTime.Now.AddMonths(4)
                },

                new Medicine
                {
                    MedicineName="Azithromycin",
                    Category="Antibiotic",
                    Stock=50,
                    ExpiryDate=DateTime.Now.AddMonths(5)
                },

                new Medicine
                {
                    MedicineName="Lisinopril",
                    Category="Blood Pressure",
                    Stock=6,
                    ExpiryDate=DateTime.Now.AddMonths(1)
                },

                new Medicine
                {
                    MedicineName="Ibuprofen",
                    Category="Pain Relief",
                    Stock=90,
                    ExpiryDate=DateTime.Now.AddMonths(9)
                }
            };
        }
    }
}
