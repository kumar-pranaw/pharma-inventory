using System;

namespace Pharmacy.ViewModels
{
    public class ListOfInvoice
    {
        public int Id { get; set; }
        public DateTime? DateOfPurchase { get; set; }
        public string Quantity { get; set; }
        public string TotalAmount { get; set; }
        public string ProductName { get; set; }
        public string GstPercent { get; set; }
        public int? SellingPrice { get; set; }
        public int? Costprice { get; set; }
        public string ProductPrice { get; set; }
        public string HSNNumber { get; set; }
        public string InvoiceId { get; set; }
        public string ExpiryDate { get; set; }
        public string Pack { get; set; }
        public string Batch { get; set; }
    }
}