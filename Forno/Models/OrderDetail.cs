namespace Forno.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("OrderDetail")]
    public partial class OrderDetail
    {
        public int OrderDetailID { get; set; }

        public int OrderrID { get; set; }

        [Display(Name = "Product identify")]
        public int ProductID { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [StringLength(1000)]
        public string ModifiedIngredients { get; set; }

        public virtual Orderr Orderr { get; set; }

        public virtual Product Product { get; set; }
    }
}
