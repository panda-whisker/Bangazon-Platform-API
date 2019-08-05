using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    public class PaymentType
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Required]
        public int AccountNumber { get; set; }

        [Required]
        public Customer Customer { get; set; }
    }
}
