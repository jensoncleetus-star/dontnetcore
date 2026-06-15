using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class AssignedTeams
    {
        [Key]
        public long AssignedTeamId { set; get; }

        public long CustomerID { set; get; }

        public long TeamID { set; get; }
    }
    public class ScopeOfWorksData
    {
        [Key]
        public long ScopeOfWorkId { set; get; }

        public long CustomerID { set; get; }

        public long ScopeId { set; get; }
        public long? employeeid { get; set; }
    }
}