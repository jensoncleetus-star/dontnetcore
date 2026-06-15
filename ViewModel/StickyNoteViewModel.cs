using QuickSoft.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class StickyNoteViewModel
    {
        [Required]
        public string NoteName { get; set; }

        [Required]
        public string NoteContent { get; set; }

        [Required]
        public long Label { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public Status Status { get; set; }

        public long Branch { get; set; }
    }
}