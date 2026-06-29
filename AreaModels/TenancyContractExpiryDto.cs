using System;

namespace QuickSoft.Models
{
    public class TenancyContractExpiryDto
    {
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
