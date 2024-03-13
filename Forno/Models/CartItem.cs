using System.Collections.Generic;

namespace Forno.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public List<int> SelectedIngredientIds { get; set; }
    }

}