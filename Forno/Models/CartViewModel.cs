namespace Forno.Models
{
    public class CartViewModel
    {
        public int OrderDetailID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
    }
}