using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class InvoiceLayout
    {
        public byte Id { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
        public Status Status { get; set; }
    }
    public class InvoiceField
    {
        public int Id { get; set; }
        [StringLength(20)]
        public string Type { get; set; }
        [StringLength(20)]
        public string Position { get; set; }
        [StringLength(150)]
        public string Value { get; set; }
        [StringLength(150)]
        [Display(Name ="Other Language")]
        public string Lang { get; set; }
        public int Order { get; set; }
        public Status Status { get; set; }
        // default is sale
        [StringLength(20)]
        public string Section { get; set; }
        public InvoiceField(){
            Status = Status.active;
        }

    }
}