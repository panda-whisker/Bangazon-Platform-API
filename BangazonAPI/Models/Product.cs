using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }
        public int Quantity { get; }
        public double Price { get; set; }
        public ProductType ProductType { get; set; }
        public Customer CustomerSeller { get; set; }
    }
}
