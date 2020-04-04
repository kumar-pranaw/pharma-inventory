﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pharmacy.ViewModels
{
    public class SupplierProduct
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string HSNNumber { get; set; }
        public string BatchNumber { get; set; }
        public string ExpiryDate { get; set; }
    }
}