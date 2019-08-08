using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    public class ProductType
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public int  CustomerId { get; set; } 
        public int AcctNumber { get; set; }
    }
}
