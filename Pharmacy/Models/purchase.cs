//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Pharmacy.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class purchase
    {
        public int id { get; set; }
        public Nullable<int> productId { get; set; }
        public string Pack { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<double> price { get; set; }
        public Nullable<int> CGST { get; set; }
        public Nullable<int> SupplierId { get; set; }
        public Nullable<System.DateTime> DateOfPurchase { get; set; }
        public Nullable<int> InvoiceId { get; set; }
        public Nullable<int> IGST { get; set; }
        public Nullable<int> UTGST { get; set; }
        public Nullable<int> Free { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual PurchaseInvoice PurchaseInvoice { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
