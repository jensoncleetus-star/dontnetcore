using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class AccountGroupViewModel
    {
        public virtual ICollection<AccountsViewModel> Tree { get; set; }
    }
    public class TreeViewModel
    {
        public virtual ICollection<Tree> Menu { get; set; }
    }
    public class TreeItem
    {
        public long? ID { get; set; }
        public string text { get; set; }
        public string Type { get; set; }
        public IList<TreeItem> nodes { get; set; } // children
        public long Parent { get; set; }
        public long? AccountId { get; set; }
        public string icon { get; set; }
    }
    public class Tree
    {
        public long? ID { get; set; }
        public string text { get; set; }
        public long? Parent { get; set; }
        public string Type { get; set; }
        public string icon { get; set; }
        public long? AccountId { get; set; }

        //public Tree(long id, string name, long? parentID)
        //{
        //    this.ID = id;
        //    this.Name = name;
        //    this.ParentID = parentID;
        //}
    }

    
}