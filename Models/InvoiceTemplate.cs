using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.Models
{
    // User-designed (drag-and-drop) invoice template. NEW table — independent of the legacy
    // InvoiceLayout (.cshtml file names) system; does not affect existing print flows until
    // a template is explicitly selected at print time.
    [Table("InvoiceTemplate")]
    public class InvoiceTemplate
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(30)]
        public string DocType { get; set; }      // Sale, Quotation, Purchase, ... (Sale for now)

        [StringLength(12)]
        public string PaperSize { get; set; }     // A4, A5, A3, Letter, Legal

        [StringLength(12)]
        public string Orientation { get; set; }   // portrait | landscape

        // Drag-and-drop layout definition (JSON): { paper, orientation, elements:[{type,x,y,w,h,...}] }
        public string DesignJson { get; set; }

        public bool IsDefault { get; set; }

        public Status Status { get; set; }        // active | inactive (soft-delete)

        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public InvoiceTemplate()
        {
            Status = Status.active;
            DocType = "Sale";
            PaperSize = "A4";
            Orientation = "portrait";
            CreatedDate = DateTime.Now;
        }
    }
}
