using System;
namespace QuickSoft.Models
{
    public class StickyLabel
    {
        public long Id { get; set; }

        public string LabelName { get; set; }

        public string LabelColor { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public Status Status { get; set; }

        public long Branch { get; set; }

    }
    public class StickyNote
    {
        public long Id { get; set; }

        public string NoteName { get; set; }

        public string NoteContent { get; set; }

        public long Label { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public Status Status { get; set; }

        public long Branch { get; set; }

    }
}