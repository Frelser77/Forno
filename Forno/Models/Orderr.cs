namespace Forno.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Orderr")]
    public partial class Orderr
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Orderr()
        {
            OrderDetail = new HashSet<OrderDetail>();
        }

        [Key]
        public int OrderID { get; set; }

        public int AppUserID { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public decimal TotalPrice { get; set; }

        public virtual AppUser AppUser { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetail { get; set; }
    }
}
