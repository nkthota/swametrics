using System;
using SWAMetrics.Models;
using System.Data;
using System.Linq;

namespace SWAMetrics.Lib
{
    public class WeeklyExecutionDetailsProgramInit
    {
        public string Project { get; set; }
        public string ProjectDatbaseName { get; set; }
        public string WeekStartDate { get; set; }
        public string WeekEndDate { get; set; }
        public int CalenderWeek { get; set; }
        public string Sql;
        private DataTable _table;
        public readonly ls_dashboardEntities Db = new ls_dashboardEntities();

        public WeeklyExecutionDetailsProgramInit(string project, string projectDatbaseName)
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

        public void DefectDetails()
        {
            Sql =
                $"SELECT BG_USER_TEMPLATE_02 as Application, sum(case when [BG_DETECTION_DATE] >= dateadd(day,-7,convert(date, GETDATE ())) then 1 else 0 end) as [Age7], sum(case when [BG_DETECTION_DATE] between dateadd(day,-14, convert(date, GETDATE ())) and dateadd(day,-8, convert(date, GETDATE ())) then 1 else 0 end) as [Age8-14], sum(case when [BG_DETECTION_DATE] between dateadd(day,-15, convert(date, GETDATE ())) and dateadd(day,-90, convert(date, GETDATE ())) then 1 else 0 end) as [Age15-90], sum(case when [BG_DETECTION_DATE] < dateadd(day,-14,convert(date, GETDATE ())) then 1 else 0 end) as [AgeAbove90] FROM td.BUG where [BG_USER_TEMPLATE_27] IS NOT NULL and [BG_USER_TEMPLATE_27] <> '18 - Closed' and BG_SEVERITY in ('1 - Critical', '2 - Major') and BG_DETECTION_DATE > '1/1/2018' GROUP BY BG_USER_TEMPLATE_02";
            _table = RunSql(Sql);

            if (_table != null)
            {

                foreach (DataRow row in _table.Rows)
                {
                    InitDefects(row["Application"].ToString());
                    var currentApplication = row["Application"].ToString();
                    DefectMetric recordDefect =
                        Db.DefectMetrics.SingleOrDefault(p =>
                            p.Project == Project && p.Application == currentApplication);
                    if (recordDefect != null) recordDefect.AgeWeek = Convert.ToInt32(row["Age7"]);
                    if (recordDefect != null) recordDefect.AgeBiWeek = Convert.ToInt32(row["Age8-14"]);
                    if (recordDefect != null) recordDefect.AgeThreeMonth = Convert.ToInt32(row["Age15-90"]);
                    if (recordDefect != null) recordDefect.AgeOther = Convert.ToInt32(row["AgeAbove90"]);
                    Db.SaveChanges();
                }
            }

        }

        private void InitDefects(string application)
        {
            var records = Db.DefectMetrics.Where(p =>
                p.Application == application && p.Project == Project).ToList();

            if (records.Count == 0 && application.Trim() != "")
            {
                Db.DefectMetrics.Add(new DefectMetric
                {
                    AgeBiWeek = 0,
                    AgeOther = 0,
                    AgeWeek = 0,
                    Application = application,
                    Project = Project
                });
                Db.SaveChanges();
            }
        }

        //// Get the list of Test Instances that are passed and failed in a given time period. 
        //// TODO: ignored blocked test run for this time. focused more on passed and failed

        //private void UpdateTestInstaceCounts(WeeklyExecutionMetricsProgramInit recordWeek, string currentApplication, string currentProgramInit)
        //{
        //    Sql =
        //        $"select c.TS_USER_TEMPLATE_01 as ProgramInit, c.TS_USER_TEMPLATE_02 as Application, Count(c.TS_USER_TEMPLATE_02) as AppCount, d.RN_STATUS as RunStatus from td.TEST c inner join (select a.RN_TEST_ID, a.RN_STATUS from td.RUN a inner join (select RN_TESTCYCL_ID, max(RN_RUN_ID) as MaxRunId from td.RUN group by RN_TESTCYCL_ID having max(RN_EXECUTION_DATE) BETWEEN '{WeekStartDate}' and '{WeekEndDate}') b on a.RN_RUN_ID = b.MaxRunId) d on c.TS_TEST_ID = d.RN_TEST_ID where d.RN_STATUS in ('Passed', 'Failed') and c.TS_USER_TEMPLATE_02 = '{currentApplication}' and c.TS_USER_TEMPLATE_01 = '{currentProgramInit}' group by d.RN_STATUS, c.TS_USER_TEMPLATE_02, c.TS_USER_TEMPLATE_01";
        //    _table = RunSql(Sql);
        //    var passedCount = 0;
        //    var failedCount = 0;

        //    if (_table != null)
        //    {
        //        foreach (DataRow row in _table.Rows)
        //        {
        //            if (row["RunStatus"].ToString() == "Passed")
        //            {
        //                passedCount = Convert.ToInt32(row["AppCount"]);
        //                if (recordWeek != null) recordWeek.TotalPassedInstances = Convert.ToInt32(row["AppCount"]);
        //            }

        //            if (row["RunStatus"].ToString() == "Failed")
        //            {
        //                failedCount = Convert.ToInt32(row["AppCount"]);
        //                if (recordWeek != null) recordWeek.TotalFailedInstances = Convert.ToInt32(row["AppCount"]);
        //            }

        //            if (recordWeek != null) recordWeek.TotalInstances = failedCount + passedCount;

        //            if (failedCount + passedCount != 0)
        //            {
        //                var passPercentage = (double)passedCount * 100 / (failedCount + passedCount);
        //                if (recordWeek != null)
        //                    recordWeek.PercentageTotalInstancesPassed = Convert.ToDecimal(passPercentage);

        //                var failPercentage = (double)failedCount * 100 / (failedCount + passedCount);
        //                if (recordWeek != null)
        //                    recordWeek.PercentageTotalInstancesFailed = Convert.ToDecimal(failPercentage);
        //            }

        //            Db.SaveChanges();
        //        }
        //    }

        //}

        //// Get the listf of First time passed and first time failed
        //private void UpdateFirstTimeCounts(WeeklyExecutionMetricsProgramInit recordWeek, string currentApplication, string currentProgramInit)
        //{
        //    Sql =
        //        $"select c.TS_USER_TEMPLATE_01 as ProgramInit, c.TS_USER_TEMPLATE_02 as Application, Count(c.TS_USER_TEMPLATE_02) as AppCount, d.RN_STATUS as RunStatus from td.TEST c inner join (select a.RN_TEST_ID, a.RN_STATUS from td.RUN a inner join (select RN_TESTCYCL_ID, min(RN_RUN_ID) as MinRunId from td.RUN group by RN_TESTCYCL_ID having min(RN_EXECUTION_DATE) BETWEEN '{WeekStartDate}' and '{WeekEndDate}') b on a.RN_RUN_ID = b.MinRunId) d on c.TS_TEST_ID = d.RN_TEST_ID where d.RN_STATUS in ('Passed', 'Failed') and c.TS_USER_TEMPLATE_02 = '{currentApplication}' and c.TS_USER_TEMPLATE_01 = '{currentProgramInit}' group by d.RN_STATUS, c.TS_USER_TEMPLATE_02, c.TS_USER_TEMPLATE_01";
        //    _table = RunSql(Sql);
        //    var passedCount = 0;
        //    var failedCount = 0;
        //    if (_table != null)
        //    {
        //        foreach (DataRow row in _table.Rows)
        //        {
        //            if (row["RunStatus"].ToString() == "Passed")
        //            {
        //                if (recordWeek != null) recordWeek.TotalFirstTimePassed = Convert.ToInt32(row["AppCount"]);
        //                passedCount = Convert.ToInt32(row["AppCount"]);
        //            }

        //            if (row["RunStatus"].ToString() == "Failed")
        //            {
        //                if (recordWeek != null) recordWeek.TotalFirstTimeFailed = Convert.ToInt32(row["AppCount"]);
        //                failedCount = Convert.ToInt32(row["AppCount"]);
        //            }

        //            if (recordWeek != null) recordWeek.TotalFirstTimeTestInstances = failedCount + passedCount;

        //            if (failedCount + passedCount != 0)
        //            {
        //                var passPercentage = (double)passedCount * 100 / (failedCount + passedCount);
        //                if (recordWeek != null)
        //                    recordWeek.PercentFirstTimeInstancesPassed = Convert.ToDecimal(passPercentage);

        //                var failPercentage = (double)failedCount * 100 / (failedCount + passedCount);
        //                if (recordWeek != null)
        //                    recordWeek.PercentFirstTimeInstancesFailed = Convert.ToDecimal(failPercentage);
        //            }

        //            Db.SaveChanges();
        //        }
        //    }
        //}

        //// get the list of Re-run
        //private void UpdateReRunCounts(WeeklyExecutionMetricsProgramInit recordWeek, string currentApplication, string currentProgramInit)
        //{
        //    Sql =
        //        $"select t4.TS_USER_TEMPLATE_01 as ProgramInit, t4.TS_USER_TEMPLATE_02 as Application, Count(t4.TS_USER_TEMPLATE_02) as AppCount, t3.RN_STATUS as RunStatus from (select RN_TESTCYCL_ID, min(RN_RUN_ID) as MinRunId from td.RUN where (RN_STATUS NOT IN ('Blocked','No Run','N/A', 'Not Completed') Or RN_DURATION > 0) group by RN_TESTCYCL_ID) t1, (select RN_TESTCYCL_ID, max(RN_RUN_ID) as MaxRunId from td.RUN group by RN_TESTCYCL_ID having max(RN_EXECUTION_DATE) BETWEEN '{WeekStartDate}' and '{WeekEndDate}') t2, (select * from td.RUN) t3, (select * from td.TEST) t4 where t1.RN_TESTCYCL_ID = t2.RN_TESTCYCL_ID and t1.MinRunId < t2.MaxRunId and t2.MaxRunId = t3.RN_RUN_ID and t4.TS_TEST_ID = t3.RN_TEST_ID and (t3.RN_STATUS NOT IN ('Blocked','No Run','N/A','Not Completed') Or t3.RN_DURATION > 0) and t4.TS_USER_TEMPLATE_02 = '{currentApplication}' and t4.TS_USER_TEMPLATE_01 = '{currentProgramInit}' group by t3.RN_STATUS, t4.TS_USER_TEMPLATE_02, t4.TS_USER_TEMPLATE_01";
        //    _table = RunSql(Sql);
        //    var passedCount = 0;
        //    var failedCount = 0;
        //    if (_table != null)
        //    {
        //        foreach (DataRow row in _table.Rows)
        //        {
        //            if (row["RunStatus"].ToString() == "Passed")
        //            {
        //                if (recordWeek != null) recordWeek.ReTestPassedInstances = Convert.ToInt32(row["AppCount"]);
        //                passedCount = Convert.ToInt32(row["AppCount"]);
        //            }

        //            if (row["RunStatus"].ToString() == "Failed")
        //            {
        //                if (recordWeek != null) recordWeek.ReTestFailedInstances = Convert.ToInt32(row["AppCount"]);
        //                failedCount = Convert.ToInt32(row["AppCount"]);
        //            }

        //            if (recordWeek != null) recordWeek.TotalReTestInstances = failedCount + passedCount;

        //            if (failedCount + passedCount != 0)
        //            {
        //                var passPercentage = (double)passedCount * 100 / (failedCount + passedCount);
        //                if (recordWeek != null)
        //                    recordWeek.PercentageReTestInstancesPassed = Convert.ToDecimal(passPercentage);

        //                var failPercentage = (double)failedCount * 100 / (failedCount + passedCount);
        //                if (recordWeek != null)
        //                    recordWeek.PercentageReTestInstancesFailed = Convert.ToDecimal(failPercentage);
        //            }

        //            Db.SaveChanges();
        //        }
        //    }
        //}

        public void UpdatedByApplciationProgramInit()
        {
            // get the list of test instances in the given period and loop through unique applications
            Sql = $"select TS_USER_TEMPLATE_01 as ProgramInit,TS_USER_TEMPLATE_02 as Application, Count(RN_TESTCYCL_ID) as TestInstanceCount from td.Test t inner join (select distinct * from (select RN_TESTCYCL_ID, RN_TEST_ID from td.RUN where RN_EXECUTION_DATE BETWEEN '{WeekStartDate}' and '{WeekEndDate}' and RN_STATUS in ('Passed', 'Failed') union select RN_TESTCYCL_ID, RN_TEST_ID from td.RUN where RN_EXECUTION_DATE BETWEEN '{WeekStartDate}' and '{WeekEndDate}' and RN_STATUS = 'Blocked' and RN_DURATION > 0) x ) y on t.TS_TEST_ID = y.RN_TEST_ID group by TS_USER_TEMPLATE_02,TS_USER_TEMPLATE_01";
            _table = RunSql(Sql);
            if (_table != null)
            {
                foreach (DataRow row in _table.Rows)
                {
                    // make sure there is a record for calender week, application combination                    
                    var currentApplication = row["Application"].ToString();
                    var currentProgramInit = row["ProgramInit"].ToString();
                    InitWeeklyApplicationProgramInitRecord(currentApplication, currentProgramInit);


                    WeeklyExecutionMetricsProgramInit recordWeek = Db.WeeklyExecutionMetricsProgramInits.SingleOrDefault(p =>
                        p.Application.Equals(currentApplication) && p.ProgramInitiative.Equals(currentProgramInit) && p.CalenderWeek == CalenderWeek && p.Project == Project);

                    // get total test instance counts in the period
                    GetTotalRuns(recordWeek, currentApplication, currentProgramInit);

                    // get first time counts
                    GetFirstRuns(recordWeek, currentApplication, currentProgramInit);

                    // get re-run counts
                    GetReRuns(recordWeek, currentApplication, currentProgramInit);
                }
            }
        }

        // get the list of first time runs - only passed and failed

        private void GetFirstRuns(WeeklyExecutionMetricsProgramInit recordWeek, string currentApplication, string currentProgramInit)
        {
            Sql =
                $"select c.TS_USER_TEMPLATE_01 as ProgramInit, c.TS_USER_TEMPLATE_02 as Application, Count(c.TS_USER_TEMPLATE_02) as AppCount, d.RN_STATUS as RunStatus from td.TEST c inner join (select a.RN_TEST_ID, a.RN_STATUS from td.RUN a inner join (select RN_TESTCYCL_ID, min(RN_RUN_ID) as MinRunId from td.RUN group by RN_TESTCYCL_ID having min(RN_EXECUTION_DATE) BETWEEN '{WeekStartDate}' and '{WeekEndDate}') b on a.RN_RUN_ID = b.MinRunId) d on c.TS_TEST_ID = d.RN_TEST_ID where d.RN_STATUS in ('Passed', 'Failed') and c.TS_USER_TEMPLATE_02 = '{currentApplication}' and c.TS_USER_TEMPLATE_01 = '{currentProgramInit}' group by d.RN_STATUS, c.TS_USER_TEMPLATE_02, c.TS_USER_TEMPLATE_01";
            _table = RunSql(Sql);
            var passedCount = 0;
            var failedCount = 0;
            if (_table != null)
            {
                foreach (DataRow row in _table.Rows)
                {
                    if (row["RunStatus"].ToString() == "Passed")
                    {
                        if (recordWeek != null) recordWeek.TotalFirstTimePassedRuns = Convert.ToInt32(row["AppCount"]);
                        passedCount = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Failed")
                    {
                        if (recordWeek != null) recordWeek.TotalFirstTimeFailedRuns = Convert.ToInt32(row["AppCount"]);
                        failedCount = Convert.ToInt32(row["AppCount"]);
                    }

                    if (recordWeek != null) recordWeek.TotalFirstTimeRuns = failedCount + passedCount;

                    if (failedCount + passedCount != 0)
                    {
                        var passPercentage = (double)passedCount * 100 / (failedCount + passedCount);
                        if (recordWeek != null)
                            recordWeek.PercentFirstRunPassed = Convert.ToDecimal(passPercentage);

                        var failPercentage = (double)failedCount * 100 / (failedCount + passedCount);
                        if (recordWeek != null)
                            recordWeek.PercentFirstRunFailed = Convert.ToDecimal(failPercentage);
                    }

                    Db.SaveChanges();
                }
            }
        }

        private void GetTotalRuns(WeeklyExecutionMetricsProgramInit recordWeek, string currentApplication, string currentProgramInit)
        {
            Sql =
                $"select t2.TS_USER_TEMPLATE_01 as ProgramInit, t2.TS_USER_TEMPLATE_02 as Application, Count(t2.TS_USER_TEMPLATE_02) as AppCount, t1.RN_STATUS as RunStatus from td.TEST t2 inner join (select RN_RUN_ID, RN_TEST_ID, RN_STATUS from td.RUN where RN_EXECUTION_DATE BETWEEN '{WeekStartDate}' and '{WeekEndDate}' and RN_STATUS in ('Passed', 'Failed')) t1 on t2.TS_TEST_ID = t1.RN_TEST_ID where t2.TS_USER_TEMPLATE_02 = '{currentApplication}' and t2.TS_USER_TEMPLATE_01 = '{currentProgramInit}' group by t2.TS_USER_TEMPLATE_01, t2.TS_USER_TEMPLATE_02, t1.RN_STATUS";
            _table = RunSql(Sql);
            var passedCount = 0;
            var failedCount = 0;

            if (_table != null)
            {
                foreach (DataRow row in _table.Rows)
                {
                    if (row["RunStatus"].ToString() == "Passed")
                    {
                        passedCount = Convert.ToInt32(row["AppCount"]);
                        if (recordWeek != null) recordWeek.TotalPassedRuns = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Failed")
                    {
                        failedCount = Convert.ToInt32(row["AppCount"]);
                        if (recordWeek != null) recordWeek.TotalFailedRuns = Convert.ToInt32(row["AppCount"]);
                    }

                    if (recordWeek != null) recordWeek.TotalRuns = failedCount + passedCount;

                    if (failedCount + passedCount != 0)
                    {
                        var passPercentage = (double)passedCount * 100 / (failedCount + passedCount);
                        if (recordWeek != null)
                            recordWeek.PercentageTotalRunsPassed = Convert.ToDecimal(passPercentage);

                        var failPercentage = (double)failedCount * 100 / (failedCount + passedCount);
                        if (recordWeek != null)
                            recordWeek.PercentageTotalRunsFailed = Convert.ToDecimal(failPercentage);
                    }

                    Db.SaveChanges();
                }
            }

        }

        private void GetReRuns(WeeklyExecutionMetricsProgramInit recordWeek, string currentApplication,
            string currentProgramInit)
        {
            foreach (var record in Db.WeeklyExecutionMetricsProgramInits.ToList())
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
            }
        }

        private void InitWeeklyApplicationProgramInitRecord(string application, string programinit)
        {
            var records = Db.WeeklyExecutionMetricsProgramInits.Where(p =>
                p.Application == application && p.CalenderWeek == CalenderWeek && p.Project == Project).ToList();

            if (records.Count == 0 && application.Trim() != "" && programinit.Trim() != "")
            {
                Db.WeeklyExecutionMetricsProgramInits.Add(new WeeklyExecutionMetricsProgramInit()
                {
                    Application = application,
                    Project = Project,
                    WeekStartDate = WeekStartDate,
                    WeekEndDate = WeekEndDate,
                    CalenderWeek = CalenderWeek,
                    ProgramInitiative = programinit,
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
                    TotalRuns = 0
                });
                Db.SaveChanges();
            }
        }
    }
}