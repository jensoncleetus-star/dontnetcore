using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class Config
    {
        public long id { get; set; }
        [StringLength(50)]
        public string Section { get; set; }
        [StringLength(50)]
        public string Field { get; set; }
        [StringLength(150)]
        public string Value { get; set; }
    }
}