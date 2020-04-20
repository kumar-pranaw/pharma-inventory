namespace Pharmacy.ViewModels
{
    public class ListProductViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string HSNNumber { get; set; }
        public string BatchNumber { get; set; }
        public string ExpiryDate { get; set; }
        public double? SellingPrice { get; set; }
        public double? CostPrice { get; set; }
    }
}