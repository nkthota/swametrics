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
    public class ProjectExecutionRelease
    {
        public string Project { get; set; }
        public string ProjectDatbaseName { get; set; }
        public string Release { get; set; }
        public string Sql;
        private DataTable _table;
        public readonly ls_dashboardEntities Db = new ls_dashboardEntities();

        public ProjectExecutionRelease(string project, string release)    
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", "qcsiteadmin_db");
            _table = projectDb.SqlQuery($"select db_name from [qcsiteadmin_db].[td].[PROJECTS] where project_name = '{project}'");
            if (_table != null)
            {
                ProjectDatbaseName = _table.Rows[0]["db_name"].ToString();
            }
            Release = release;
        }

        // run sql and return records in datatable
        public DataTable RunSql(string sql)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", ProjectDatbaseName);
            return projectDb.SqlQuery(Sql);
        }

        public MetricRecord GetReleaseDetails(string release)
        {

            MetricRecord metricRecord = new MetricRecord();

            // first run details
            Sql = $"select Count(t4.RN_STATUS) as AppCount, t4.RN_STATUS as RunStatus from td.RUN t4 inner join (select t2.TS_USER_TEMPLATE_02 as Application, t3.RN_TESTCYCL_ID, t3.RunId from td.TEST t2 inner join (select distinct(t1.RN_TESTCYCL_ID), min(t1.RN_RUN_ID) as RunId, t1.RN_TEST_ID from td.RUN t1 where t1.RN_ASSIGN_RCYC in (select a1.RCYC_ID from td.RELEASE_CYCLES a1 inner join td.RELEASES a2 on a1.RCYC_PARENT_ID = a2.REL_ID where a2.REL_NAME = '{release}') group by RN_TESTCYCL_ID, t1.RN_TEST_ID) t3 on t2.TS_TEST_ID = t3.RN_TEST_ID) t5 on t4.RN_RUN_ID = t5.RunId where t4.RN_STATUS in ('Passed', 'Failed', 'Blocked') group by t4.RN_STATUS";
            _table = RunSql(Sql);
            if (_table != null)
            {
                foreach (DataRow row in _table.Rows)
                {
                    if (row["RunStatus"].ToString() == "Passed")
                    {
                        metricRecord.FTRPassed = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Failed")
                    {
                        metricRecord.FTRFailed = metricRecord.FTRFailed + Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Blocked")
                    {
                        metricRecord.FTRFailed = metricRecord.FTRFailed + Convert.ToInt32(row["AppCount"]);
                    }
                }
            }

            // total run details
            Sql = $"select Count(t3.RN_STATUS) as AppCount, t3.RN_STATUS as RunStatus from td.TEST t2 inner join (select * from td.RUN t1 where t1.RN_ASSIGN_RCYC in (select a1.RCYC_ID from td.RELEASE_CYCLES a1 inner join td.RELEASES a2 on a1.RCYC_PARENT_ID = a2.REL_ID where a2.REL_NAME = '{release}') and t1.RN_STATUS in ('Failed','Blocked','Passed')) t3 on t3.RN_TEST_ID = t2.TS_TEST_ID group by t3.RN_STATUS";
            _table = RunSql(Sql);
            if (_table != null)
            {
                foreach (DataRow row in _table.Rows)
                {
                    if (row["RunStatus"].ToString() == "Passed")
                    {
                        metricRecord.TRPassed = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Failed")
                    {
                        metricRecord.TRFailed = metricRecord.TRFailed + Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Blocked")
                    {
                        metricRecord.TRFailed = metricRecord.TRFailed + Convert.ToInt32(row["AppCount"]);
                    }
                }
            }

            // tota test instance counts
            Sql = $"select Count(t3.TC_STATUS) as AppCount,t3.TC_STATUS as RunStatus from td.TEST t2 inner join (select * from td.TESTCYCL t1 where t1.TC_ASSIGN_RCYC in (select a1.RCYC_ID from td.RELEASE_CYCLES a1 inner join td.RELEASES a2 on a1.RCYC_PARENT_ID = a2.REL_ID where a2.REL_NAME = '{release}') and t1.TC_STATUS in ('Failed','Blocked','Passed','Not Completed','No Run') ) t3 on t2.TS_TEST_ID = t3.TC_TEST_ID group by t3.TC_STATUS";
            _table = RunSql(Sql);
            if (_table != null)
            {
                foreach (DataRow row in _table.Rows)
                {
                    if (row["RunStatus"].ToString() == "Passed")
                    {
                        metricRecord.TCPassed = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Failed")
                    {
                        metricRecord.TCFailed = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Blocked")
                    {
                        metricRecord.TCBlocked = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "Not Completed")
                    {
                        metricRecord.TCNotCompleted = Convert.ToInt32(row["AppCount"]);
                    }

                    if (row["RunStatus"].ToString() == "No Run")
                    {
                        metricRecord.TCNoRun = Convert.ToInt32(row["AppCount"]);
                    }
                }

                metricRecord.TotalTC = metricRecord.TCPassed + metricRecord.TCFailed + metricRecord.TCBlocked + metricRecord.TCNotCompleted + metricRecord.TCNoRun;
            }

            return metricRecord;
        }
    }

    public class MetricRecord
    {
        public int TotalTC { get; set; }
        public int TCPassed { get; set; }
        public int TCFailed { get; set; }
        public int TCBlocked { get; set; }
        public int TCNotCompleted { get; set; }
        public int TCNoRun { get; set; }
        public int FTRPassed { get; set; }
        public int FTRFailed { get; set; }
        public int TRPassed { get; set; }
        public int TRFailed { get; set; }
    }
}