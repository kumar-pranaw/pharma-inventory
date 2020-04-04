using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pharmacy.ViewModels
{
    public class ViewPurchases
    {
        public int Id { get; set; }
        public string InvoiceId { get; set; }
        public DateTime? DateOfPurchase { get; set; }
        public decimal? PurchaseAmount { get; set; }
        public int? SupplierId { get; set; }
    }
}