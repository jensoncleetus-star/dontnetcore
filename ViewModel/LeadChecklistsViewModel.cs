using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class LeadChecklistsViewModel
    {
        public string CheckNo { get; set; }
        public long Stage { get; set; }

        public string Name { get; set; }
        public long Id { get; set; }
        public string Note { get; set; }
        public long Remark { get; set; }
        public bool Chck { get; set; }

    }

    public class LeadOptionalfieldViewModel
    {
        public long Section { get; set; }
    }
    public class LeadfieldnameViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

    }
}