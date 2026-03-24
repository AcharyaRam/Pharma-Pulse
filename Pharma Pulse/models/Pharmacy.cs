using System;
using System.ComponentModel.DataAnnotations;

namespace Pharma_Pulse.Models
{
    public class Pharmacy
    {
        public int Id { get; set; }

        [Required]
        public string PharmacyName { get; set; } = string.Empty;

        public string? OwnerName { get; set; }

        public string? MobileNumber { get; set; }

        public string? Email { get; set; }

        public string? GSTNumber { get; set; }

        public string? DrugLicenseNo { get; set; }

        public string? Address { get; set; }

        public string PlainPassword { get; set; } = string.Empty; // Stored for SuperAdmin view

        // NEW — LOGIN SYSTEM
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // Subscription & Settings
        public string? PrintFormat { get; set; }

        public string? PlanName { get; set; }

        public decimal PlanPrice { get; set; } = 0;

        public DateTime PlanValidTill { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // NEW — TRACKING
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}