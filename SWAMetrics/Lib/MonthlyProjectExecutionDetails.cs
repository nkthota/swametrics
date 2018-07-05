using System;
using SWAMetrics.Models;
using System.Data;
using System.Linq;

namespace SWAMetrics.Lib
{
    public class MonthlyProjectExecutionDetails
    {
        public string Project { get; set; }
        public string ProjectDatbaseName { get; set; }
        public string MonthStartDate { get; set; }
        public string MonthEndDate { get; set; }
        public string MonthYear { get; set; }
        public string Sql;
        private DataTable _table;
        public readonly ls_dashboardEntities Db = new ls_dashboardEntities();

        public MonthlyProjectExecutionDetails(string project, string projectDatbaseName)
        {
            Project = project;
            ProjectDatbaseName = projectDatbaseName;
        }       

        // run sql and return records in datatable
        public DataTable RunSql(string sql)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", ProjectDatbaseName);
            return projectDb.SqlQuery(Sql);
        }


        public void UpdateProjectMonthlyMetrics()
        {

            InitMonthlyProjectMetricsRecord();

            ProjectMonthlyExecutionMetric recordMonth = Db.ProjectMonthlyExecutionMetrics.SingleOrDefault(p =>p.MonthYear == MonthYear && p.Project == Project);

            // get total test instance counts in the period
            GetTotalRuns(recordMonth);

            // get first time counts
            GetFirstRuns(recordMonth);

            // get re-run counts
            GetReRuns(recordMonth);

        }

        // get the list of first time runs - only passed and failed

        private void GetFirstRuns(ProjectMonthlyExecutionMetric recordMonth)
        {
            Sql =
                $"select Count(c.TS_TEST_ID) as AppCount, d.RN_STATUS as RunStatus from td.TEST c inner join (select a.RN_TEST_ID, a.RN_STATUS from td.RUN a inner join (select RN_TESTCYCL_ID, min(RN_RUN_ID) as MinRunId from td.RUN group by RN_TESTCYCL_ID having min(RN_EXECUTION_DATE) BETWEEN '{MonthStartDate}' and '{MonthEndDate}') b on a.RN_RUN_ID = b.MinRunId) d on c.TS_TEST_ID = d.RN_TEST_ID where d.RN_STATUS in ('Passed', 'Failed', 'Blocked') group by d.RN_STATUS";
            _table = RunSql(Sql);
            var passedCount = 0;
            var failedCount = 0;
            var blockedCount = 0;
            if (_table != null)
            {
                foreach (DataRow row in _table.Rows)
                {
                    if (row["RunStatus"].ToString() == "Passed")
                    {
                        if (recordMonth != null) recordMonth.TotalFirstTimePassedRuns = Convert.ToInt32(row["AppCount"]);
                        passedCount = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Failed")
                    {
                        failedCount = Convert.ToInt32(row["AppCount"]);                                              
                    }

                    if (row["RunStatus"].ToString() == "Blocked")
                    {
                        blockedCount = Convert.ToInt32(row["AppCount"]);                        
                    }

                    if (recordMonth != null) recordMonth.TotalFirstTimeFailedRuns = failedCount + blockedCount;

                    if (recordMonth != null) recordMonth.TotalFirstTimeRuns = failedCount + passedCount;

                    if (failedCount + passedCount != 0)
                    {
                        var passPercentage = (double)passedCount * 100 / (failedCount + passedCount);
                        if (recordMonth != null)
                            recordMonth.PercentFirstRunPassed = Convert.ToDecimal(passPercentage);

                        var failPercentage = (double)failedCount * 100 / (failedCount + passedCount);
                        if (recordMonth != null)
                            recordMonth.PercentFirstRunFailed = Convert.ToDecimal(failPercentage);
                    }

                    Db.SaveChanges();
                }
            }
        }

        private void GetTotalRuns(ProjectMonthlyExecutionMetric recordMonth)
        {
            Sql =
                $"select Count(t2.TS_TEST_ID) as AppCount, t1.RN_STATUS as RunStatus from td.TEST t2 inner join (select RN_RUN_ID, RN_TEST_ID, RN_STATUS from td.RUN where RN_EXECUTION_DATE BETWEEN '{MonthStartDate}' and '{MonthEndDate}' and RN_STATUS in ('Passed', 'Failed','Blocked')) t1 on t2.TS_TEST_ID = t1.RN_TEST_ID group by t1.RN_STATUS";
            _table = RunSql(Sql);
            var passedCount = 0;
            var failedCount = 0;
            var blockedCount = 0;

            if (_table != null)
            {
                foreach (DataRow row in _table.Rows)
                {
                    if (row["RunStatus"].ToString() == "Passed")
                    {
                        passedCount = Convert.ToInt32(row["AppCount"]);
                        if (recordMonth != null) recordMonth.TotalPassedRuns = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Failed")
                    {
                        failedCount = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Blocked")
                    {
                        blockedCount = Convert.ToInt32(row["AppCount"]);
                    }

                    if (recordMonth != null) recordMonth.TotalFailedRuns = failedCount + blockedCount;

                    if (recordMonth != null) recordMonth.TotalRuns = failedCount + passedCount;

                    if (failedCount + passedCount != 0)
                    {
                        var passPercentage = (double)passedCount * 100 / (failedCount + passedCount);
                        if (recordMonth != null)
                            recordMonth.PercentageTotalRunsPassed = Convert.ToDecimal(passPercentage);

                        var failPercentage = (double)failedCount * 100 / (failedCount + passedCount);
                        if (recordMonth != null)
                            recordMonth.PercentageTotalRunsFailed = Convert.ToDecimal(failPercentage);
                    }

                    Db.SaveChanges();
                }
            }

        }

        private void GetReRuns(ProjectMonthlyExecutionMetric recordMonth)
        {
            foreach (var record in Db.ProjectMonthlyExecutionMetrics.ToList())
            {
                int? totalReRuns = 0;
                int? passedCount = 0;
                int? failedCount = 0;

                passedCount = record.TotalPassedRuns - record.TotalFirstTimePassedRuns;
                record.TotalReRunPassed = passedCount;

                failedCount = record.TotalFailedRuns - record.TotalFirstTimeFailedRuns;
                record.TotalReRunFailed = failedCount;

                totalReRuns = passedCount + failedCount;
                record.TotalReRuns = totalReRuns;

                if (totalReRuns != 0)
                {
                    if (passedCount != null)
                    {
                        var passPercentage = (double)passedCount * 100 / (totalReRuns);
                        record.PercentageReRunPassed = Convert.ToDecimal(passPercentage);
                    }

                    if (failedCount != null)
                    {
                        var failPercentage = (double)failedCount * 100 / (totalReRuns);
                        record.PercentageReRunFailed = Convert.ToDecimal(failPercentage);
                    }
                }
                else
                {
                    record.PercentageReRunPassed = 0;
                    record.PercentageReRunFailed = 0;
                }

                Db.SaveChanges();
            }
        }

        private void InitMonthlyProjectMetricsRecord()
        {
            var records = Db.ProjectMonthlyExecutionMetrics.Where(p =>
                p.MonthYear == MonthYear && p.Project == Project).ToList();

            if (records.Count == 0)
            {
                Db.ProjectMonthlyExecutionMetrics.Add(new ProjectMonthlyExecutionMetric()
                {
                    PercentFirstRunFailed = 0,
                    PercentFirstRunPassed = 0,
                    PercentageReRunFailed = 0,
                    PercentageReRunPassed = 0,
                    PercentageTotalRunsFailed = 0,
                    PercentageTotalRunsPassed = 0,
                    TotalFailedRuns = 0,
                    TotalFirstTimeFailedRuns = 0,
                    TotalFirstTimePassedRuns = 0,
                    TotalFirstTimeRuns = 0,
                    TotalPassedRuns = 0,
                    TotalReRunFailed = 0,
                    TotalReRunPassed = 0,
                    TotalReRuns = 0,
                    TotalRuns = 0,
                    MonthYear = MonthYear,
                    MonthEndDate = MonthEndDate,
                    MonthStartDate = MonthStartDate,
                    Project = Project
                });
                Db.SaveChanges();
            }
        }
    }
}