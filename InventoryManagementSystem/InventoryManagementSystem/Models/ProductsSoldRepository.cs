using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryManagementSystem.Models
{
    public class ProductsSoldRepository
    {
        public string product { get; set; }
        public decimal price { get; set; }
        public int sales { get; set; }

    }
}