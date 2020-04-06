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
        public int? SP { get; set; }
        public int GST { get; set; }
        public int? Amount { get; set; }
        public string Pack { get; set; }
    }
}