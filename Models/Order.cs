using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace OrderManagementSystem.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductName { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; }

        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Pending";

        // These are filled automatically in controller
        public string? UserId { get; set; }

        public string? CreatedByEmail { get; set; }

        public IdentityUser? User { get; set; }
    }
}