using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class PropertyHomeViewModel
    {
        public string totunitcount { get; set; }

        public string totemptyunitcount { get; set; }
        public string totPropertycount { get; set; }
        public string totUnitscount { get; set; }
        public string totLandloardscount { get; set; }
        public string totTenantscount { get; set; }
        public string totDeveloperscount { get; set; }
        public string totContractorscount { get; set; }
        public string totBrokerscount { get; set; }

        public string totPropertRegistrationscount { get; set; }
        public string tottenentcontractscount { get; set; }
        public string totRentalInvoicescount { get; set; }

        public string totexpense { get; set; }
        public string totincome { get; set; }

    }
}