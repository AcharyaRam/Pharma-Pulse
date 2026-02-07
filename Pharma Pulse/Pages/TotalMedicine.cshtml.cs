using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

public class MedicinesModel : PageModel
{
    public List<Medicine> Medicines { get; set; }

    public void OnGet()
    {
        Medicines = new List<Medicine>
        {
            new Medicine { MedicineName="Paracetamol", Category="Pain Relief", Stock=120, ExpiryDate=new DateTime(2026,10,12)},
            new Medicine { MedicineName="Amoxicillin", Category="Antibiotic", Stock=50, ExpiryDate=new DateTime(2026,9,15)},
            new Medicine { MedicineName="Cetirizine", Category="Anti-Allergy", Stock=80, ExpiryDate=new DateTime(2026,11,20)},
            new Medicine { MedicineName="Metformin", Category="Diabetes", Stock=200, ExpiryDate=new DateTime(2026,12,5)},
            new Medicine { MedicineName="Aspirin", Category="Cardiac", Stock=150, ExpiryDate=new DateTime(2026,8,18)},
            new Medicine {MedicineName="Rebbis", Category="Dog", Stock=100, ExpiryDate=new DateTime(2026, 8, 17)}
        };
    }
}

public class Medicine
{
    public string MedicineName { get; set; }
    public string Category { get; set; }
    public int Stock { get; set; }
    public DateTime ExpiryDate { get; set; }
}
