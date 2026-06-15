using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class Payhead
    {
        public long ID { get; set; }
        [StringLength(50)]
        public string Name { get; set; }

        public PayHeadType Type { get; set; }

        public long Accountgroup { get; set; }

        public string IncomeType { get; set; }

        public bool affectnetsalary { get; set; }
        [StringLength(50)]
        public string NameinSlip { get; set; }
        public CalcTypePayHead? CalculationType { get; set; } //As computed value, As user defined value, Flate Rate, On Attendance, On Production
        public string AttendanceType { get; set; }//when CalculationType is on attendance Present, Not aPPLICABLE
        public string Leave { get; set; }//Absent, Present
        public string CalculationPeriod { get; set; }//Days, Fortnight, Months, Weeks
        public CalcBasisPayHead? CalculationBasis { get; set; }
        public long? days { get; set; }
        public ComputPayHead? Compute { get; set; }//Days, Fortnight, Months, Weeks
        public string Specifiedformula { get; set; }

        public long Account { get; set; }
        public long? ProductionType { get; set; }
        public bool UseGratuity { get; set; }
        public long? GratuityDays { get; set; }
        public Status Status { get; set; }
    }
    public class SpecifiedFormula
    {
        public long ID { get; set; }

        public string Name { get; set; }
    }
    public class Computeinfo
    {
        public long ID { get; set; }

        public long Payhead { get; set; }

        public DateTime? Effectivefrom { get; set; }

        public decimal? Amountgreatethan { get; set; }

        public decimal? Amountupto { get; set; }

        public string Slabtype { get; set; }

        public decimal? value { get; set; }
    }
    public class GratuityDetails
    {
        public long ID { get; set; }
        public long Payhead { get; set; }
        public long? From { get; set; }
        public long? To { get; set; }
        public long? Days { get; set; }
    }
}