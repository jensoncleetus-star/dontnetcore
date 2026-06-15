using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class paramclass
    {
        public string statusname { get; set; }
        public long satusid { get; set; }
        public int count { get; set; }
        public long customerid { get; set; }

    }
    public class LeadDashboardviewmodel
    {
        public long? TotalSuccess { get; set; }
        public long? Totalfaled { get; set; }
        public long? Totalbalance { get; set; }

        public long? GrandTotal { get; set; }
        public List<LeadDashboardemp> empdash { get; set; }
        public List<LeadDashboardsource> srcdash { get; set; }

    }

    public class Leaveviewmodel
    {
        public long? TotalEmployees { get; set; }
        public long? MedicalLeave { get; set; }
        public long? Leave { get; set; }

        public long? AnualLeave { get; set; }
        public long? EmergencyLeave { get; set; }
        public long? Dayoff { get; set; }
        public long? Suspenstion { get; set; }
        public List<Models.employeetimesheet> et { get; set; }

    }
    public class LeadDashboardemp
    {
        public long? TotalSuccess { get; set; }
        public long? Totalfaled { get; set; }
        public long? taskid { get; set; }
        public long? Totalbalance { get; set; }
        public long? GrandTotal { get; set; }
        public string EmployeeName { get; set; }

        public long? empid { get; set; }
        public long? sourseid { get; set; }
    }
    public class LeadDashboardsource
    {
        public long? TotalSuccess { get; set; }
        public long? Totalfaled { get; set; }
        public long? Totalbalance { get; set; }
        public long? GrandTotal { get; set; }
        public string SourceName { get; set; }


    }
    public class HomeViewModel
    {
        public string totCustomerCount { get; set; }
        public string totSupplierCount { get; set; }
        public string totUsersCount { get; set; }
        public string totSalesExecCount { get; set; }
        public string totSaleEntryCount { get; set; }
        public string totSalesOrder { get; set; }
        public string totQuotCount { get; set; }
        public string totPurchaseEntryCount { get; set; }

        public string totSalesReturnCount { get; set; }
        public string totPurchaseReturnCount { get; set; }

        public string todCustomerCount { get; set; }
        public string todVendorCount { get; set; }
        public string todQuotationCount { get; set; }
        public string todSalesEntryCount { get; set; }
        public string todPurchaseEntryCount { get; set; }
        public string todExpenseCount { get; set; }
        public string todaySales { get; set; }
        public string Last30DaysSales { get; set; }
        public string ThisMonthSales { get; set; }
        public string LastMonthSales { get; set; }
        public string LastTwoMonthSales { get; set; }
        public string LastThreeMonthSales { get; set; }
        public string LastFourMonthSales { get; set; }
        public string LastFiveMonthSales { get; set; }
        public string LastSixMonthSales { get; set; }
        public string ThisMonthLeadsCount { get; set; }
        public string LastMonthLeadsCount { get; set; }
        public string LastSecondMonthLeadsCount { get; set; }
        public string LastThirdMonthLeadsCount { get; set; }
        public string LastForthMonthLeadsCount { get; set; }
        public string LastFifthMonthLeadsCount { get; set; }



        public string SalesCredit { get; set; }
        public string PurchaseCredit { get; set; }
        public string totReciepts { get; set; }
        public string totPayments { get; set; }
        public string cashinhand { get; set; }

        public string totPayment { get; set; }
        public string totReceipt { get; set; }
        public string HireList { get; set; }
        public string dailySalesrtn { get; set; }
        public string ThismnthSalesRtn { get; set; }
        public string ThisYearSalesRtn { get; set; }
        public string TodayPurchase { get; set; }
        public string ThisMonthPurchase { get; set; }
        public string ThisYearPurchase { get; set; }
        public string LastMonthPurchase { get; set; }
        public string LastTwoMonthPurchase { get; set; }
        public string LastThreeMonthPurchase { get; set; }
        public string LastFourMonthPurchase { get; set; }
        public string LastFiveMonthPurchase { get; set; }


        public string TodayPurchasertn { get; set; }
        public string ThisMonthPurchasertn { get; set; }
        public string ThisYearPurchasertn { get; set; }

        public string LastMonthSalesrtn { get; set; }
        public string LastTwoMonthSalesrtn { get; set; }
        public string LastThreeMonthSalesrtn { get; set; }
        public string LastFourMonthSalesrtn { get; set; }
        public string LastFiveMonthSalesrtn { get; set; }
    }
    public class followupviewmodel
    {
        public string totalleads { get; set; }
        public string totaltask { get; set; }
        public string totalcustomerfollowups { get; set; }
        public string totalamc { get; set; }
        public long empid { get; set; }
        public string dates { get; set; }
       
    }
    public class customerdashboards
    {
        public string StartDate { get; set; }
        public string totalbonus { get; set; }
        public string invoices { get; set; }
        public string pricesearch { get; set; }


    }
    public class VRDashboardViewModel
    {
        public string totSupplierCount { get; set; }
        public string TodayPurchase { get; set; }
        public string ThisMonthPurchase { get; set; }
        public string ThisYearPurchase { get; set; }
        public string LastFiveMonthPurchase { get; set; }
        public string LastFourMonthPurchase { get; set; }
        public string LastThreeMonthPurchase { get; set; }
        public string LastTwoMonthPurchase { get; set; }
        public string LastMonthPurchase { get; set; }
        public string ThisYearPurchaseReturn { get; set; }
        public string TodayPurchaseReturn { get; set; }
        public string ThisMonthPurchaseReturn { get; set; }
        public string LastMonthPurchaseReturn { get; set; }
        public string LastTwoMonthPurchaseReturn { get; set; }
        public string LastThreeMonthPurchaseReturn { get; set; }
        public string LastFourMonthPurchaseReturn { get; set; }
        public string LastFiveMonthPurchaseReturn { get; set; }

    }
}