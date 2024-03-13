using System.Collections.Generic;

namespace Forno.Models
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public List<Ingredient> AllIngredients { get; set; }
    }
}