using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class MenuViewModel
    {
        public virtual ICollection<AppModules> Menu { get; set; }
    }
    public class MenuStructure
    {
        public string Name { get; set; }
        public long ModulesID { get; set; }
        public string viewName { get; set; }
        public string Link { get; set; }
        public string iconClass { get; set; }
        public long Parent { get; set; }
        public string Description { get; set; }
        public choice IsParent { get; set; }
        public choice addMenu { get; set; }
        public String Employee { get; set; }
        public Status Status { get; set; }
        public choice Editable { get; set; }
    }
}