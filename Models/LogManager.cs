using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class LogManager
    {
        public long LogManagerID { get; set; }

        public LogTypes LogType { get; set; }

        public string User { get; set; }

        [StringLength(200)]
        public string LogDetails { get; set; }

       // [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime LogTime { get; set; }

        // to identify section(eg: usermanager)
        public string LogSection { get; set; }

        // to identify section main table(eg: usermanager)
        public string LogTable { get; set; }

        // log table id
        public string LogID { get; set; }

        [StringLength(20)]
        public string LogIP { get; set; }

        public int Status { get; set; }
    }
}