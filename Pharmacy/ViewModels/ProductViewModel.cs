using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Pharmacy.ViewModels
{
    public class ProductViewModel
    {
        [Required(ErrorMessage ="This field is required*")]
        public string ProductName { get; set; }
        [Required(ErrorMessage = "This field is required*")]
        public string HSNNumber { get; set; }
        [Required(ErrorMessage = "This field is required*")]
        public string BatchNumber { get; set; }
        [Required(ErrorMessage = "This field is required*")]
        public string ExpiryDate { get; set; }
        [Required(ErrorMessage = "This field is required*")]
        public double? SP { get; set; }
        [Required(ErrorMessage = "This field is required*")]
        public double? MRP { get; set; }
        [Required(ErrorMessage = "This field is required*")]
        public double? Rate { get; set; }
        public int CGST { get; set; }
        public int IGST { get; set; }
        public int UTGST { get; set; }
       
      
    }
}