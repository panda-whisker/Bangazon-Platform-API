﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BangazonAPI.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public int DepartmentId { get; set; }
        

        //public Department Department { get; set; }

        public Computer Computer { get; set; }

       

        public bool IsSupervisor { get; set; }
    }
}
