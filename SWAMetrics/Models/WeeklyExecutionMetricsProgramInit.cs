//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SWAMetrics.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class WeeklyExecutionMetricsProgramInit
    {
        public int ID { get; set; }
        public string WeekStartDate { get; set; }
        public string WeekEndDate { get; set; }
        public int CalenderWeek { get; set; }
        public string Project { get; set; }
        public string Application { get; set; }
        public string ProgramInitiative { get; set; }
        public Nullable<int> TotalFirstTimeTestInstances { get; set; }
        public Nullable<int> TotalFirstTimePassed { get; set; }
        public Nullable<int> TotalFirstTimeFailed { get; set; }
        public Nullable<int> TotalInstances { get; set; }
        public Nullable<int> TotalPassedInstances { get; set; }
        public Nullable<int> TotalFailedInstances { get; set; }
        public Nullable<int> TotalReTestInstances { get; set; }
        public Nullable<int> ReTestPassedInstances { get; set; }
        public Nullable<int> ReTestFailedInstances { get; set; }
        public Nullable<decimal> PercentFirstTimeInstancesPassed { get; set; }
        public Nullable<decimal> PercentFirstTimeInstancesFailed { get; set; }
        public Nullable<decimal> PercentageTotalInstancesPassed { get; set; }
        public Nullable<decimal> PercentageTotalInstancesFailed { get; set; }
        public Nullable<decimal> PercentageReTestInstancesPassed { get; set; }
        public Nullable<decimal> PercentageReTestInstancesFailed { get; set; }
    }
}
