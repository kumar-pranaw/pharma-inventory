using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pharmacy.ViewModels
{
    public class ProductSaleViewModel
    {
        public int id { get; set; }
        public string HSNNumber { get; set; }
        public string BatchNumber { get; set; }
        public string ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public float Discount { get; set; }
        public double? SP { get; set; }
        public int? MRP { get; set; }
        public int CGST { get; set; }
        public int IGST { get; set; }
        public int UTGST { get; set; }
        public double? Amount { get; set; }
        public string Pack { get; set; }
    }
}