using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pharmacy.ViewModels
{
    public class DistributorViewModel
    {
        public int id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerPhoneNumber { get; set; }
    }
}