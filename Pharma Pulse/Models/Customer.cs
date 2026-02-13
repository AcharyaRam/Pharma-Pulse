namespace Pharma_Pulse.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = "";

        public string MobileNumber { get; set; } = "";

        public string Gender { get; set; } = "";

        public int Age { get; set; }

        public string City { get; set; } = "";

        public string CustomerType { get; set; } = "Walk-in";

        public string Email { get; set; } = "";

        public string DoctorReference { get; set; } = "";

        public string MedicalNotes { get; set; } =  "N/A";
    }
}
