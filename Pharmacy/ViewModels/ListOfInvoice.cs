using System;

namespace Pharmacy.ViewModels
{
    public class ListOfInvoice
    {
        public int Id { get; set; }
        public DateTime? DateOfPurchase { get; set; }
        public int? Quantity { get; set; }
        public double? TotalAmount { get; set; }
        public string ProductName { get; set; }
        public int? GstPercent { get; set; }
        public double? SellingPrice { get; set; }
        public double? MRP { get; set; }
        public double? Rate { get; set; }
        public string HSNNumber { get; set; }
        public double? DiscountAmount { get; set; }
        public double? DiscountedAmount  { get; set; }
        public string InvoiceId { get; set; }
        public string ExpiryDate { get; set; }
        public string Pack { get; set; }
        public int? Discount { get; set; }
        public string Batch { get; set; }
    }
}