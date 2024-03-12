namespace Forno.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Product")]
    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            OrderDetail = new HashSet<OrderDetail>();
            Ingredient = new HashSet<Ingredient>();
        }

        public int ProductID { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public decimal Price { get; set; }

        [Required]
        [StringLength(50)]
        public string DeliveryTime { get; set; }
        public string ImageUrl { get; set; }

        [NotMapped]
        public int[] SelectedIngredientIDs { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Ingredient> Ingredient { get; set; }
    }
}
