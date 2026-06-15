using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class ContractorType
    {
        public long ID { get; set; }

        public string Name { get; set; }
    }
    public class ContractType
    {
        public long ID { get; set; }

        public string Name { get; set; }
        public long Account { get; set; }
    }
    public class sop
    {
        [Key]
        public long sopid { get; set; }
        public string title { get; set; }
      
        public string note { get; set; }
        public DateTime? logtime { get; set; }
    }
    public class sopdet
    {
        [Key]
        public long sopdetid { get; set; }
        public long sopid { get; set; }

        public long employeeid { get; set; }

    }


    public class servicetype
    {
        [Key]
        public long servicetypeid { get; set; }
        public string title { get; set; }

        public string note { get; set; }
        public DateTime? logtime { get; set; }
    }
    public class passworddetailsas
    {
        [Key]
        public long passdetailidass { get; set; }
        public long passworddetailid { get; set; }

        public long employeeid { get; set; }

    }
    public class assigncommon
    {
        [Key]
        public long mykey { get; set; }
        public long parentid { get; set; }
        public string type { get; set; }
        public long employeeid { get; set; }

    }
    public class filedocumentdetailsa
    {
        [Key]
        public long filedocumentass { get; set; }
        public long filedocumentdetailid { get; set; }

        public long employeeid { get; set; }

    }
}