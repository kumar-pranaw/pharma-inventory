namespace Pharmacy.ViewModels
{
    public class AddProductViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string HSNNumber { get; set; }
        public string BatchNumber { get; set; }
        public string ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public double? MRP { get; set; }
        public float? Rate { get; set; }
        public int? CostPrice { get; set; }
        public int IGST { get; set; }
        public int CGST { get; set; }
        public int UTGST { get; set; }
        public double Amount { get; set; }
        public string Pack { get; set; }
        public double? SellingPrice { get; set; }
    }
}