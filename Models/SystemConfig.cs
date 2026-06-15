using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class SystemConfig
    {
        public int SystemConfigId { get; set; }
        // add encrypted code
        public string SystemCode { get; set; }
        // add encrypted value to systemtypes by enum type of SystemType
        public string SystemTypes { get; set; }

        public string LicenseKey { get; set; }
        public string LicenseType { get; set; }

        // add encrypted date
        public string StartDate { get; set; }

        // add encrypted date
        public string EndDate { get; set; }

        // add encrypted count of days
        public string Extentdays { get; set; }

        public int? Year { get; set; }

        public FinancialEnd FinancialEnd { get; set; }
        public string MACID { get; set; }

        public Status status { get; set; }
        // system last updated date
        public string   sld { get; set; }

        public string NumberOfUsers { get; set; }

        public SystemConfig(){
        }
    }
    public class AppVersion
    {
        public int AppVersionId { get; set; }
        public DateTime InstallDate { get; set; }
        public string Versions { get; set; }
    }
    }