using System.Collections.Generic;
using System.Linq;
using QuickSoft.ViewModel;

namespace BindAcc
{
    public static class BindAccounts
    {
        
        public static decimal? getAmountDr(ICollection<BalanceSheet> bsheet, long? accGpId, decimal? Amt)
        {
            var bsheet2 = bsheet.Where(a => a.Parent == accGpId && a.Parent != 0).ToList();
            if (bsheet2 != null && bsheet2.Count > 0)
            {
                foreach (var item in bsheet2)
                {
                    if (bsheet2 != null)
                    {
                        Amt = (decimal)item.Debit;
                        Amt = Amt + (decimal)getAmountDr(bsheet, item.AccountsGroupID, 0);
                    }
                }

            }
            return Amt;
        }

        public static decimal? getAmountCr(ICollection<BalanceSheet> bsheet, long? accGpId, decimal? Amt)
        {
            var bsheet2 = bsheet.Where(a => a.Parent == accGpId && a.Parent != 0).ToList();
            if (bsheet2 != null && bsheet2.Count > 0)
            {
                foreach (var item in bsheet2)
                {
                    if (bsheet2 != null)
                    {
                        Amt = (decimal)item.Credit;
                        Amt = Amt + (decimal)getAmountCr(bsheet, item.AccountsGroupID, 0);
                    }
                }

            }
            return Amt;
        }
        //public static decimal? getAllAmount(ICollection<BalanceSheet> bsheet, long? accGpId, decimal? Total)
        //{
        //    decimal Amt = 0;
        //    var bsheet2 = bsheet.Where(a => a.Parent == accGpId).ToList();
        //    if (bsheet2 != null && bsheet2.Count > 0)
        //    {
        //        foreach (var item in bsheet2)
        //        {
        //            if (bsheet2 != null)
        //            {
        //                Amt = (decimal)item.Debit - (decimal)item.Credit;
        //                Amt = (decimal)getAllAmount(bsheet, item.AccountsGroupID, 0);
        //            }
        //        }

        //    }
        //    return Total + Amt;
        //}
    }

}