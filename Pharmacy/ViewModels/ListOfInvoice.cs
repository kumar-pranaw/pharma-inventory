using System;

namespace Pharmacy.ViewModels
{
    public class ListOfInvoice
    {
        public int Id { get; set; }
        public DateTime? DateOfPurchase { get; set; }
        public int? Quantity { get; set; }
        public int? TotalAmount { get; set; }
        public string ProductName { get; set; }
        public int? GstPercent { get; set; }
        public int? SellingPrice { get; set; }
        public int? Costprice { get; set; }
        public string ProductPrice { get; set; }
        public string HSNNumber { get; set; }
        public string InvoiceId { get; set; }
        public string ExpiryDate { get; set; }
        public string Pack { get; set; }
        public string Discount { get; set; }
        public string Batch { get; set; }
    }
}