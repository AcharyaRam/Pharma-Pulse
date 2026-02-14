using System;

namespace Pharma_Pulse.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        // ✅ Required Fields
        public string FirstName { get; set; } = "";
        public string Surname { get; set; } = "";
        public string MobileNumber { get; set; } = "";
        public int Age { get; set; }

        // ✅ Optional Fields (Nullable)
        public string? MiddleName { get; set; }
        public string? GSTNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? City { get; set; }
        public string? CustomerType { get; set; }
        public string? Email { get; set; }
        public string? DoctorReference { get; set; }
        public string? MedicalNotes { get; set; }
    }
}
