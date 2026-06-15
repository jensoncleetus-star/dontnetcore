using System.ComponentModel.DataAnnotations;
namespace QuickSoft.Models
{
    public static class General
    {
        public static string keyval = "ASDF67hg";
        public static string SecVal = "QSNY";
    }

    // for ye or no choice
    public enum choice
    {
        Yes,
        No
    }
    public enum alaramrepeat
    {
     
        Daily,
        Saturday,
        Sunday,
        Weekly,
        Monthly,
        Yearly,
           OnRemaiterDate,
    }

    // basic status active or inactive
    public enum Status
    {
        active,
        inactive
    }
    //log types for log status

    public enum LogTypes
    {
        Logged,
        Created,
        Updated,
        Deleted,
        Changed
    }

    //for gender
    public enum Gender
    {
        Male,
        Female
    }
    public enum CRMCustomerType
    {
        Customer,
        PipeLine,
        Leads,
        taskcontact
    }
    public enum CustomerMergePurposeId
    {
        accountstransaction,
        recieptpayto,
        recieptpayfrom,
        paymentpayfrom,
        paymentpayto,
        journalpayfrom,
        journalpayto,
        pdc,
        task,
       
        amc,
        
        quotation,
        leads,
        conactrelation,
        salesorder,
        salesreturn,
        boq,
        sales,
        performa,
        warrenty,
        workcompletion,
        materialreq,
        documents,
        remarks,
        activity,
        project,
        deliverynote
    }
    public enum Docbooktype
    {
        cheque,
        reciept,
        payment,

    }
    //branch access for user
    public enum BranchAccess
    {
        Current,
        All
    }

    // name prefix
    public enum Prefix
    {
        Mr,
        Mrs,
        Miss
    }

    // customer type prefix
    public enum CustomerType
    {
        [Display(Name = "Credit")]
        Customer,
        [Display(Name = "Cash")]
        Walking,
        Card,
        [Display(Name = "Online Sales")]
        Online,
        [Display(Name = "Online Transfer")]
        OnlineAccount
    }
    public enum JobType
    {
        [Display(Name = "AMC")]
        amc,
        [Display(Name = "Under Warrenty")]
        warrenty,
        [Display(Name = "Paid Job")]
        payed
    }
    public enum PaymentType
    {
        [Display(Name = "Cash")]
        cash,
        [Display(Name = "Cheque")]
        cheque,
        [Display(Name = "Pending")]
        pending,
        [Display(Name = "Bank")]
        Bank
    }
    public enum pricingstatagytype
    {
        [Display(Name = "Last Purchase Price")]
        LIFO,
        [Display(Name = "Average Purchase Price")]
        AVG,
        [Display(Name = "First Purchase Price")]
        FIFO,
        [Display(Name = "Absolute Amount")]
        ABS
    }

    public enum EmployeePaymentType
    {
        Cash,  
        Card,
        Account,
        BankTransfer

    }


    public enum StockType
    {
        [Display(Name = "Stock Transfer")]
        StockTransfer,
        [Display(Name = "Opening Stock")]
        OpeningStock
    }

    // supplier type prefix
    public enum SupplierType
    {
        [Display(Name = "Credit")]
        CreditSale,
        [Display(Name = "Cash")]
        CashSale
    }
    //sale return type
    public enum ReturnType
    {
        [Display(Name = "Against Bill")]
        AgainstBill,
        Direct
    }

    // sale type for salesentry
    public enum SaleType
    {
        POS,
        Sale,
        Hire,
        TaxExempt
    }

    // debit or credit

    public enum DC
    {
        Debit,
        Credit
    }
    public enum BonusBase
    {
        
        SalesProfit,
        InvoiceTotalAmount
    }
    // for paymnt mode
    public enum ModeOfPayment
    {
        Cash,
        PDC,
        CDC,
        OnlineTransfer
        //Credit
    }
    public enum ModeOfPayment2
    {
        Cash=0,
        CDC=2
        //Credit
    }
    public enum ManageMessageId
    {
        AddPhoneSuccess,
        ChangePasswordSuccess,
        SetTwoFactorSuccess,
        SetPasswordSuccess,
        RemoveLoginSuccess,
        RemovePhoneSuccess,
        Error
    }
    public enum BSType
    {
        Additive,
        Subtractive
    }
    public enum AmountType
    {
        [Display(Name = "Absolute Amount")]
        AbsoluteAmount,
        Percentage
    }

    public enum AdjustmentType
    {
        Less,
        Add
    }
    
    // system Configuration
    public enum SystemType
    {
        [Display(Name = "Demo Version")]
        Demo,
        [Display(Name = "Standard Version")]
        Standard


        //[Display(Name = "Offline Single")]
        //Offline_Single,
        //[Display(Name = "Offline Multy")]
        //Offline_Multy,
        //[Display(Name = "Online Single")]
        //Online_Single,
        //[Display(Name = "Online Multy")]
        //Online_Multy,
        //[Display(Name = "Online Offline Single")]
        //Online_Offline_single,
        //[Display(Name = "Online Offline Multy")]
        //Online_offline_multy,
        //[Display(Name = "Demo Version")]
        //Demo,
    }
    // financial year ending

    public enum FinancialEnd
    {
        [Display(Name = "March 31")]
        March31,
        [Display(Name = "December 31")]
        December31,
    }

    public enum TableStatus
    {
        [Display(Name = "In Use")]
        InUse,
        [Display(Name = "Out of Use")]
        OutofUse,
        Available,
        Reserved
    }
    public enum OrderStatus
    {
        [Display(Name = "Print KOT")]
        PrintKOT,
        [Display(Name = "Print Bill")]
        PrintBill,
        Hold,
        SaveOrder,
        Payment,
        VoidBill
    }
    public enum OrderType
    {
        [Display(Name = "Take Away")]
        TakeAway,
        Delivery,
        [Display(Name = "Dine In")]
        DineIn
    }

    public enum TaxType
    {
        ItemWise,
        Exempt,
        VoucherWise
    }

    public enum MtToType
    {
        [Display(Name = "Sale Challan")]
        SaleChall,

        [Display(Name = "Purchase Return Challan")]
        PRChall,

        [Display(Name = "Challan Reversal")]
        ReversChall,

        [Display(Name = "Issued After Job Work")]
        IAJobWork,

        [Display(Name = "Issued For Job Work")]
        IFJobWork,

        [Display(Name = "To Be Tracked With Party")]
        TackParty,

        [Display(Name = "Not To Be Tracked With Party")]
        NotTackParty,

    }

    public enum MtFromType
    {
        [Display(Name = "Purchase Challan")]
        PurChall,

        [Display(Name = "Sale Return Challan")]
        SRChall,

        [Display(Name = "Challan Reversal")]
        ReversChall,

        [Display(Name = "Received For Job Work")]
        RFJobWork,

        [Display(Name = "Received After Job Work")]
        RAJobWork,

        [Display(Name = "To Be Tracked With Party")]
        TackParty,

        [Display(Name = "Not To Be Tracked With Party")]
        NotTackParty,

    }
    public enum TRNType
    {
        Registered,
        Unregistered,
        Exempt
    }
    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        MaterialUpdation

    }
    //public enum Stat
    //{
    //    Open,
    //    Close
    //}
    public enum ApprovalStatus
    {
        Approved,
        Rejected,
        [Display(Name = "Pending Approval")]
        PendingApproval,
        Completed,
        [Display(Name = "Partial Approval")]
        PartialApproval

    }
    //public enum TaskStatus
    //{
    //    Assigned,
    //    Waiting,
    //    Started,
    //    Pending,
    //    Completed,
    //    Created,
    //    Canceled
    //}
    public enum TKUpdateStatus
    {
        Assigned,
        Accepted,
        Rejected,
        [Display(Name = "Work Started")]
        Started,
        [Display(Name = "Work Pending")]
        ClosedPending,
        [Display(Name = "Work Completed")]
        ClosedCompleted,
        Created,
        Canceled,
        // assign a work thats rejected
        Reassigned,
        // allocate work pending to another user
        Reallocation,
        // hold work due to some reasons  
        Hold,
        Verified,
        Reopen
    }
    public enum FMPrint
    {
        Yes,
        No
    }
    //public enum PayRecType
    //{
    //    [Display(Name = "New Reference")]
    //    NewReference,

    //    [Display(Name = "Against Reference")]
    //    AgainstReference,

    //    Advance,

    //    [Display(Name = "On Account")]
    //    OnAccount
    //}
    public enum PurchaseHireType
    {
        Purchase,
        CrossHire
    }
    public enum SalaryMode
    {
        Monthly,
        Daily,
        Production
    }

    public enum PayHeadType
    {
        [Display(Name = "Earnings for Employees")]
        EarningsforEmployees,
        [Display(Name = "Deductions from Employees")]
        DeductionsfromEmployees,
        [Display(Name = "Employees Statutory Deductions")]
        EmployeesStatutoryDeductions,
        [Display(Name = "Employees Statutory Contributions")]
        EmployeesStatutoryContributions,
        [Display(Name = "Employers Other Charges")]
        EmployersOtherCharges,
        [Display(Name = "Bonus")]
        Bonus,
        [Display(Name = "Gratuity")]
        Gratuity,
        [Display(Name = "Loans and Advances")]
        LoansandAdvances,
        [Display(Name = "Reimbursments to Employees")]
        ReimbursmentstoEmployees,

        [Display(Name = "Earnings In Annual Leave")]
        EarningsInAnnualLeave,
        [Display(Name = "Deduction In Annual Leave")]
        DeductionInAnnualLeave,
    }
    public enum CalcTypePayHead
    {
        [Display(Name = "As User Defined Value")]
        DefinedValue,
        [Display(Name = "As Computed Value")]
        AsComputedValue,
        [Display(Name = "Flat Rate")]
        FlatRate,
        [Display(Name = "On Attendance")]
        OnAttendance,
        [Display(Name = "On Production")]
        OnProduction,
    }
    public enum CalcBasisPayHead
    {
        [Display(Name = "As per Calender Period")]
        AsperCalenderPeriod,
        [Display(Name = "User Defined")]
        UserDefined,
        [Display(Name = "User Defined Calender Type")]
        UserDefinedCalenderType,
    }
    public enum ComputPayHead
    {
        [Display(Name = "On Current Deductions Total")]
        DeductionsTotal,

        [Display(Name = "On Current Earnings Total")]
        EarningsTotal,

        [Display(Name = "On Current Subtotal")]
        CurrentSubtotal,

        //[Display(Name = "On Specified Formula")]
        //SpecifiedFormula,
    }
   
    public enum Schedule
    {
       
 
        Monthly,
        [Display(Name = "Three Payment")]
        Month3,
        [Display(Name = "Two Payment")]
        Month6,
    [Display(Name = "Single Payment")]

    Yearly,
        [Display(Name = "Six Payment")]
        Month2,
        [Display(Name = "Four Payment")]
        Month4
    }
    //for Property Type
    public enum ProType
    {
        Company,
        Individual
    }

    //for Job Status
    public enum JobStatus
    {
        Working,
        Resigned,
        [Display(Name = "On Leave")]
        OnLeave,
        Absconding
    }

    public enum PAMeasurement
    {
        [Display(Name = "In Square feet")]
        Squarefeet,
        [Display(Name = "In Square meter")]
        Squaremeter
    }
    public enum BAMeasurement
    {
        [Display(Name = "In Square feet")]
        Squarefeet,
        [Display(Name = "In Square meter")]
        Squaremeter
    }

    public enum ContctRelation
    {
        Customer,
        supplier,
        lead,
        protask,
        Amc
    }
    public enum quotationstatus
    {
        open,
        close
    }

    public enum LeadApprovedPermanantValue
    {
        [Display(Name = "Lead Approved")]
        LeadApproved=500
    }

    public enum LeadApprovalStatus
    {
        [Display(Name = "PartialAppoved")]
        PartialAppoved,
        [Display(Name = "Approved")]
        Approved,
        [Display(Name = "Rejected")]
        Rejected
    }

}