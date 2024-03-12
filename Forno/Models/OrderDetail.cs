namespace Forno.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("OrderDetail")]
    public partial class OrderDetail
    {
        public int OrderDetailID { get; set; }

        public int OrderrID { get; set; }

        public int ProductID { get; set; }

        public int Quantity { get; set; }

        [StringLength(1000)]
        public string ModifiedIngredients { get; set; }

        public virtual Orderr Orderr { get; set; }

        public virtual Product Product { get; set; }
    }
}
