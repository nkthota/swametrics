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
    
    public partial class ProjectMonthlyExecutionMetric
    {
        public int ID { get; set; }
        public string MonthStartDate { get; set; }
        public string MonthEndDate { get; set; }
        public string MonthYear { get; set; }
        public string Project { get; set; }
        public Nullable<int> TotalFirstTimeRuns { get; set; }
        public Nullable<int> TotalFirstTimePassedRuns { get; set; }
        public Nullable<int> TotalFirstTimeFailedRuns { get; set; }
        public Nullable<int> TotalRuns { get; set; }
        public Nullable<int> TotalPassedRuns { get; set; }
        public Nullable<int> TotalFailedRuns { get; set; }
        public Nullable<int> TotalReRuns { get; set; }
        public Nullable<int> TotalReRunPassed { get; set; }
        public Nullable<int> TotalReRunFailed { get; set; }
        public Nullable<decimal> PercentFirstRunPassed { get; set; }
        public Nullable<decimal> PercentFirstRunFailed { get; set; }
        public Nullable<decimal> PercentageTotalRunsPassed { get; set; }
        public Nullable<decimal> PercentageTotalRunsFailed { get; set; }
        public Nullable<decimal> PercentageReRunPassed { get; set; }
        public Nullable<decimal> PercentageReRunFailed { get; set; }
    }
}
