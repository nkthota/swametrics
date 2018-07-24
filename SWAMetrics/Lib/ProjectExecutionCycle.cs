using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SWAMetrics.Models;

namespace SWAMetrics.Lib
{
    public class ProjectExecutionCycle
    {
        public string Project { get; set; }
        public string ProjectDatbaseName { get; set; }
        public string ReleaseCycle { get; set; }
        public string Sql;
        private DataTable _table;
        public readonly ls_dashboardEntities Db = new ls_dashboardEntities();

        public ProjectExecutionCycle(string project, string projectDatbaseName)
        {
            Project = project;
            ProjectDatbaseName = projectDatbaseName;
        }

        public ProjectExecutionCycle()
        {

        }

        // run sql and return records in datatable
        public DataTable RunSql(string sql)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", ProjectDatbaseName);
            return projectDb.SqlQuery(Sql);
        }

        public void UpdateNumbers()
        {
            foreach (var record in Db.ExecutionMetricsCycles.ToList())
            {
                // update first time details
                if ((record.TotalFirstTimePassedRuns + record.TotalFirstTimeFailedRuns) != 0)
                {
                    record.TotalFirstTimeRuns = record.TotalFirstTimePassedRuns + record.TotalFirstTimeFailedRuns;
                    Db.SaveChanges();
                    if (record.TotalFirstTimePassedRuns != null)
                    {
                        var firstPercentPass = (double)record.TotalFirstTimePassedRuns * 100 /
                                                         (record.TotalFirstTimePassedRuns + record.TotalFirstTimeFailedRuns);
                        record.PercentFirstRunPassed = Convert.ToDecimal(firstPercentPass);
                    }
                    Db.SaveChanges();
                    if (record.TotalFirstTimeFailedRuns != null)
                    {
                        var firstPercentFail = (double)record.TotalFirstTimeFailedRuns * 100 /
                                               (record.TotalFirstTimePassedRuns + record.TotalFirstTimeFailedRuns);
                        record.PercentFirstRunFailed = Convert.ToDecimal(firstPercentFail);
                    }
                    Db.SaveChanges();
                }

                // update re-run counts
                if ((record.TotalPassedRuns + record.TotalFailedRuns) != 0)
                {
                    record.TotalRuns = record.TotalPassedRuns + record.TotalFailedRuns;
                    Db.SaveChanges();

                    record.TotalReRunPassed = (record.TotalPassedRuns - record.TotalFirstTimePassedRuns);
                    record.TotalReRunFailed = (record.TotalFailedRuns - record.TotalFirstTimeFailedRuns);
                    Db.SaveChanges();
                    record.TotalReRuns = record.TotalReRunPassed + record.TotalReRunFailed;
                    Db.SaveChanges();


                    if (record.TotalReRunPassed != null && record.TotalReRunPassed != 0)
                    {
                        var firstPercentPass = (double)record.TotalReRunPassed * 100 /
                                               (record.TotalReRunPassed + record.TotalReRunFailed);
                        record.PercentageReRunPassed = Convert.ToDecimal(firstPercentPass);
                    }

                    if (record.TotalReRunFailed != null && record.TotalReRunFailed != 0)
                    {
                        var firstPercentFail = (double)record.TotalReRunFailed * 100 /
                                               (record.TotalReRunPassed + record.TotalReRunFailed);
                        record.PercentageReRunFailed = Convert.ToDecimal(firstPercentFail);
                    }
                    Db.SaveChanges();
                }

                // update test instance counts
                if ((record.NoRunTestInstances + record.BlockedTestInstances + record.FailedTestInstances +
                    record.NotCompletedTestInstances + record.PassedTestInstances + record.FailedTestInstances) != 0)
                {
                    record.TotalTestInstances = record.NoRunTestInstances + record.BlockedTestInstances +
                                                record.FailedTestInstances +
                                                record.NotCompletedTestInstances + record.PassedTestInstances;
                    Db.SaveChanges();


                    if (record.PassedTestInstances != null && record.PassedTestInstances != 0)
                    {
                        var firstPercentPass = (double)record.PassedTestInstances * 100 /
                                               (record.PassedTestInstances + record.FailedTestInstances + record.BlockedTestInstances);
                        record.PercentTotalInstancesPassed = Convert.ToDecimal(firstPercentPass);
                    }

                    if (record.FailedTestInstances != null && record.FailedTestInstances != 0)
                    {
                        var firstPercentFail = (double)record.FailedTestInstances * 100 /
                                               (record.PassedTestInstances + record.FailedTestInstances + record.BlockedTestInstances);
                        record.PercentTotalInstancesFailed = Convert.ToDecimal(firstPercentFail);
                    }

                    Db.SaveChanges();
                }
            }
        }

        // get the unique cycles in the last 6 months
        public void GetProjectCycles()
        {
            Sql = $"select distinct t2.RCYC_ID, t3.REL_NAME,t2.RCYC_NAME, t2.RCYC_START_DATE, t2.RCYC_END_DATE from td.RUN t1 inner join td.RELEASE_CYCLES t2 on t1.RN_ASSIGN_RCYC = t2.RCYC_ID inner join td.RELEASES t3 on t2.RCYC_PARENT_ID = t3.REL_ID where RN_EXECUTION_DATE BETWEEN '{DateTime.Now.AddMonths(-6).Date.ToString(CultureInfo.InvariantCulture)}' and '{DateTime.Now.ToString(CultureInfo.InvariantCulture)}'";
            _table = RunSql(Sql);
            if (_table != null)
            {
                
                foreach (DataRow row in _table.Rows)
                {
                    // for each cycle in the project get the first time passed details
                    GetFirstRuns(Convert.ToInt32(row["RCYC_ID"]), row["RCYC_NAME"].ToString(),
                        row["REL_NAME"].ToString(), Convert.ToDateTime(row["RCYC_START_DATE"]),
                        Convert.ToDateTime(row["RCYC_END_DATE"]));

                    // for each cycle in the project get the total run details
                    GetTotalRuns(Convert.ToInt32(row["RCYC_ID"]), row["RCYC_NAME"].ToString(),
                        row["REL_NAME"].ToString(), Convert.ToDateTime(row["RCYC_START_DATE"]),
                        Convert.ToDateTime(row["RCYC_END_DATE"]));

                    GetTestInstanceCounts(Convert.ToInt32(row["RCYC_ID"]), row["RCYC_NAME"].ToString(),
                        row["REL_NAME"].ToString(), Convert.ToDateTime(row["RCYC_START_DATE"]),
                        Convert.ToDateTime(row["RCYC_END_DATE"]));
                }
            }   
        }

        private void GetFirstRuns(int releasecycleid, string releasecycle, string release, DateTime startdate, DateTime enddate)
        {
            Sql = $"select Count(t5.Application) as AppCount, t5.Application, t4.RN_STATUS as RunStatus from td.RUN t4 inner join (select t2.TS_USER_TEMPLATE_02 as Application, t3.RN_TESTCYCL_ID, t3.RunId from td.TEST t2 inner join (select distinct(t1.RN_TESTCYCL_ID), min(t1.RN_RUN_ID) as RunId, t1.RN_TEST_ID from td.RUN t1 where t1.RN_ASSIGN_RCYC = {releasecycleid} group by RN_TESTCYCL_ID, t1.RN_TEST_ID) t3 on t2.TS_TEST_ID = t3.RN_TEST_ID) t5 on t4.RN_RUN_ID = t5.RunId where t4.RN_STATUS in ('Passed', 'Failed', 'Blocked') group by t4.RN_STATUS, t5.Application";
            _table = RunSql(Sql);
            if (_table != null)
            {
                // for each application in the cycle populate first time details
                foreach (DataRow row in _table.Rows)
                {
                    var application = row["Application"].ToString();
                    InitProjectMetricsRecord(releasecycleid, releasecycle, application, release, startdate, enddate);

                    var record = Db.ExecutionMetricsCycles.FirstOrDefault(p => p.ReleaseCycle == releasecycle && p.Project == Project && p.Application == application &&
                        p.ReleaseCycle == releasecycle && p.ReleaseCycleId == releasecycleid);

                    if (row["RunStatus"].ToString() == "Passed")
                    {
                        if (record != null)
                        {
                            record.TotalFirstTimePassedRuns = Convert.ToInt32(row["AppCount"]);
                            Db.SaveChanges();
                        }
                    }

                    if (row["RunStatus"].ToString() == "Failed")
                    {
                        if (record != null)
                        {
                            record.TotalFirstTimeFailedRuns = Convert.ToInt32(row["AppCount"]) + record.TotalFirstTimeFailedRuns;
                            Db.SaveChanges();
                        }
                    }

                    if (row["RunStatus"].ToString() == "Blocked")
                    {
                        if (record != null)
                        {
                            record.TotalFirstTimeFailedRuns = Convert.ToInt32(row["AppCount"]) + record.TotalFirstTimeFailedRuns;
                            Db.SaveChanges();
                        }
                    }
                }
            }
        }

        private void GetTestInstanceCounts(int releasecycleid, string releasecycle, string release, DateTime startdate, DateTime enddate)
        {
            Sql = $"select Count(t2.TS_USER_TEMPLATE_02) as AppCount, t2.TS_USER_TEMPLATE_02 as Application, t3.TC_STATUS as RunStatus from td.TEST t2 inner join (select * from td.TESTCYCL t1 where t1.TC_ASSIGN_RCYC = {releasecycleid} and t1.TC_STATUS in ('Failed','Blocked','Passed','Not Completed','No Run') ) t3 on t2.TS_TEST_ID = t3.TC_TEST_ID group by t3.TC_STATUS, t2.TS_USER_TEMPLATE_02";
            _table = RunSql(Sql);
            if (_table != null)
            {
                // for each application in the cycle populate first time details
                foreach (DataRow row in _table.Rows)
                {
                    var application = row["Application"].ToString();
                    InitProjectMetricsRecord(releasecycleid, releasecycle, application, release, startdate, enddate);

                    var record = Db.ExecutionMetricsCycles.FirstOrDefault(p => p.ReleaseCycle == releasecycle && p.Project == Project && p.Application == application &&
                                    p.ReleaseCycle == releasecycle && p.ReleaseCycleId == releasecycleid);

                    if (row["RunStatus"].ToString() == "Passed")
                    {
                        if (record != null) record.PassedTestInstances = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Failed")
                    {
                        if (record != null) record.FailedTestInstances = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Blocked")
                    {
                        if (record != null) record.BlockedTestInstances = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Not Completed")
                    {
                        if (record != null) record.NotCompletedTestInstances = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "No Run")
                    {
                        if (record != null) record.NoRunTestInstances = Convert.ToInt32(row["AppCount"]);
                    }

                    Db.SaveChanges();
                }
            }
        }

        private void GetTotalRuns(int releasecycleid, string releasecycle, string release, DateTime startdate, DateTime enddate)
        {
            Sql = $"select Count(t2.TS_USER_TEMPLATE_02) as AppCount, t2.TS_USER_TEMPLATE_02 as Application, t3.RN_STATUS as RunStatus from td.TEST t2 inner join (select * from td.RUN t1 where t1.RN_ASSIGN_RCYC = {releasecycleid} and t1.RN_STATUS in ('Failed','Blocked','Passed')) t3 on t3.RN_TEST_ID = t2.TS_TEST_ID group by t3.RN_STATUS, t2.TS_USER_TEMPLATE_02";
            _table = RunSql(Sql);
            if (_table != null)
            {
                // for each application in the cycle populate first time details
                foreach (DataRow row in _table.Rows)
                {
                    var application = row["Application"].ToString();
                    InitProjectMetricsRecord(releasecycleid, releasecycle, application, release, startdate, enddate);

                    var record = Db.ExecutionMetricsCycles.FirstOrDefault(p => p.ReleaseCycle == releasecycle && p.Project == Project && p.Application == application &&
                                p.ReleaseCycle == releasecycle && p.ReleaseCycleId == releasecycleid);

                    if (row["RunStatus"].ToString() == "Passed")
                    {
                        if (record != null)
                        {
                            record.TotalPassedRuns = Convert.ToInt32(row["AppCount"]);
                            Db.SaveChanges();
                        }
                    }

                    if (row["RunStatus"].ToString() == "Failed")
                    {
                        if (record != null)
                        {
                            record.TotalFailedRuns = Convert.ToInt32(row["AppCount"]) + record.TotalFailedRuns;
                            Db.SaveChanges();
                        }

                    }

                    if (row["RunStatus"].ToString() == "Blocked")
                    {
                        if (record != null)
                        {
                            record.TotalFailedRuns = Convert.ToInt32(row["AppCount"]) + record.TotalFailedRuns;
                            Db.SaveChanges();
                        }

                    }
                }
            }
        }

        private void InitProjectMetricsRecord(int releasecycleid, string releasecycle, string application, string release, DateTime startdate, DateTime enddate)
        {
            var records = Db.ExecutionMetricsCycles.Where(p =>
                p.ReleaseCycle == releasecycle && p.Project == Project && p.Application == application && p.ReleaseCycle ==releasecycle && p.ReleaseCycleId == releasecycleid).ToList();

            if (records.Count == 0)
            {
                Db.ExecutionMetricsCycles.Add(new ExecutionMetricsCycle()
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
                    Application = application,
                    Release = release,
                    ReleaseCycle = releasecycle,
                    ReleaseCycleStartDate = startdate,
                    ReleaseCycleEnddate = enddate,
                    Project = Project,
                    BlockedTestInstances = 0,
                    FailedTestInstances = 0,
                    NoRunTestInstances = 0,
                    NotCompletedTestInstances = 0,
                    PassedTestInstances = 0,
                    PercentTotalInstancesFailed = 0,
                    PercentTotalInstancesPassed = 0,
                    TotalTestInstances = 0,
                    ReleaseCycleId = releasecycleid
                });
                Db.SaveChanges();
            }
        }
    }
}