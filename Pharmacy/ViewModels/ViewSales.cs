using System;

namespace Pharmacy.ViewModels
{
    public class ViewSales
    {
        public int Id { get; set; }
        public string InvoiceId { get; set; }
        public DateTime? DateOfPurchase { get; set; }
        public decimal? PurchaseAmount { get; set; }
        public int? CustomerId { get; set; }
    }
}