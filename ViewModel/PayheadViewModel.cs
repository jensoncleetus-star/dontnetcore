using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class PayheadViewModel
    {
        public long? ID { get; set; }
        [Required]
        public string Name { get; set; }

        public PayHeadType Type { get; set; }
        [Display(Name = "Under")]
        public long Accountgroup { get; set; }

        public string IncomeType { get; set; }

        public string affectnetsalary { get; set; }

        public string NameinSlip { get; set; }
        public CalcTypePayHead? CalculationType { get; set; } //As computed value, As user defined value, Flate Rate, On Attendance, On Production
        public string AttendanceType { get; set; }//when CalculationType is on attendance Present, Not aPPLICABLE
        public string Leave { get; set; }//Absent, Present
        public string CalculationPeriod { get; set; }//Days, Fortnight, Months, Weeks
        public CalcBasisPayHead? CalculationBasis { get; set; }
        public long? days { get; set; }
        public ComputPayHead? Compute { get; set; }//Days, Fortnight, Months, Weeks
        public string Specifiedformula { get; set; }

        public List<ComputeinfoViewModel> compinfo { get; set; }
        [Display(Name = "Opening balance")]
        public decimal OpnBalance { get; set; }
        public DC DC { get; set; }
        public long? ProductionType { get; set; }
        public string UseGratuity { get; set; }
        public long? GratuityDays { get; set; }
        public ICollection<GratuityViewModel> GratModel { get; set; }
    }

    public class ComputeinfoViewModel
    {
        public long ID { get; set; }

        public /*DateTime?*/string Effectivefrom { get; set; }

        public decimal? Amountgreatethan { get; set; }

        public decimal? Amountupto { get; set; }

        public string Slabtype { get; set; }

        public decimal? value { get; set; }
    }

    public class GratuityViewModel
    {
        public long? datefrom { get; set; }

        public long? dateto { get; set; }

        public long? days { get; set; }

    }
}