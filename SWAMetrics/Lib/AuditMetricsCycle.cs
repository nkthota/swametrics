using System;
using System.Data;

namespace SWAMetrics.Lib
{
    public class AuditMetricsCycle
    {
        private DataTable _table;
        public string Sql;

        public AuditMetricsCycle(string cycle)
        {
            Cycle = cycle;
        }

        public string Project { get; set; }
        public string ProjectDatbaseName { get; set; }
        public string Release { get; set; }
        public string Cycle { get; set; }


        public DataTable GetTemplateProjects()
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", "qcsiteadmin_db_126");
            return projectDb.SqlQuery(
                "select t1.PROJECT_NAME from [td].[PROJECTS] t1 inner join [td].[PROJECT_LINKS] t2 on t2.PRL_TO_PROJECT_UID = t1.PROJECT_UID where t2.PRL_FROM_PROJECT_UID = '33edb5f0-3364-48b4-ae3c-44c2076c152e' and t1.DOMAIN_NAME = 'SWA_TECHNOLOGY'");
        }

        private string GetProjectDbName(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", "qcsiteadmin_db_126");
            var _table =
                projectDb.SqlQuery(
                    $"select db_name from [qcsiteadmin_db_126].[td].[PROJECTS] where project_name = '{project}' and domain_name='swa_technology'");
            if (_table != null) ProjectDatbaseName = _table.Rows[0]["db_name"].ToString();
            return ProjectDatbaseName;
        }

        public RecordValues GetTestSetNotCompleted(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(distinct(RN_TESTCYCL_ID)) from td.RUN where RN_ASSIGN_RCYC in (select RCYC_ID from td.RELEASES inner join td.RELEASE_CYCLES on REL_ID = RCYC_PARENT_ID where REL_NAME = '{Cycle}') group by RN_TESTCYCL_ID");
            var sampleRec = dts.Rows.Count;
            var dta = projectDb.SqlQuery(
                $"select RN_TESTCYCL_ID, count(RN_TESTCYCL_ID) from td.RUN where RN_STATUS = 'Not Completed' and RN_ASSIGN_RCYC in (select RCYC_ID from td.RELEASES inner join td.RELEASE_CYCLES on REL_ID = RCYC_PARENT_ID where REL_NAME = '{Cycle}') group by RN_TESTCYCL_ID having count(RN_TESTCYCL_ID) > 1");
            var criticalRec = dta.Rows.Count;
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = sampleRec - criticalRec;
            return recValues;
        }

        public RecordValues GetTestInstanceLinked(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(*) from td.TESTCYCL where TC_ASSIGN_RCYC in (select RCYC_ID from td.RELEASES inner join td.RELEASE_CYCLES on REL_ID = RCYC_PARENT_ID where REL_NAME = '{Cycle}') and TC_STATUS in ('Failed', 'Blocked')");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select count(distinct(LN_ENTITY_ID)) from td.LINK where LN_ENTITY_TYPE in ('TESTCYCL') and LN_ENTITY_ID IN (select TC_TESTCYCL_ID from td.TESTCYCL where TC_ASSIGN_RCYC in (select RCYC_ID from td.RELEASES inner join td.RELEASE_CYCLES on REL_ID = RCYC_PARENT_ID where REL_NAME = '{Cycle}') and TC_STATUS in ('Failed', 'Blocked'))");
            var criticalRec = Convert.ToInt32(dta.Rows[0][0]);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;
            return recValues;
        }

        public RecordValues GetRunFastRun(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(*) from td.RUN where RN_ASSIGN_RCYC in (select RCYC_ID from td.RELEASES inner join td.RELEASE_CYCLES on REL_ID = RCYC_PARENT_ID where REL_NAME = '{Cycle}')");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select count(*) from td.RUN where RN_RUN_NAME not like 'Fast_Run%' and RN_ASSIGN_RCYC in (select RCYC_ID from td.RELEASES inner join td.RELEASE_CYCLES on REL_ID = RCYC_PARENT_ID where REL_NAME = '{Cycle}')");
            var criticalRec = Convert.ToInt32(dta.Rows[0][0]);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;
            return recValues;
        }

        public RecordValues GetTestInstancePtr(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(TC_USER_TEMPLATE_72) from td.TESTCYCL where TC_USER_TEMPLATE_72 like '%1[0-9][0-9][0-9][0-9][0-9][0-9][0-9]' and TC_ASSIGN_RCYC in (select RCYC_ID from td.RELEASES inner join td.RELEASE_CYCLES on REL_ID = RCYC_PARENT_ID where REL_NAME = '{Cycle}')");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select TC_USER_TEMPLATE_72 as PTR, TC_TEST_ID, TS_NAME from td.TESTCYCL inner join td.TEST on td.TESTCYCL.TC_TEST_ID = td.TEST.TS_TEST_ID where TC_USER_TEMPLATE_72 like '%1[0-9][0-9][0-9][0-9][0-9][0-9][0-9]' and TC_ASSIGN_RCYC in (select RCYC_ID from td.RELEASES inner join td.RELEASE_CYCLES on REL_ID = RCYC_PARENT_ID where REL_NAME = '{Cycle}') and TC_TESTCYCL_ID IN (select LN_ENTITY_ID from td.LINK where LN_ENTITY_TYPE = 'TESTCYCL')");
            var criticalRec = Convert.ToInt32(dta.Rows.Count);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;   
            return recValues;
        }

        public DataTable GetTestInstanceStatus(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select TC_STATUS, count(TC_TESTCYCL_ID) as AppCount from td.TESTCYCL t1 inner join td.TEST t2 on t1.TC_TEST_ID = t2.TS_TEST_ID where TC_ASSIGN_RCYC in (select RCYC_ID from td.RELEASES inner join td.RELEASE_CYCLES on REL_ID = RCYC_PARENT_ID where REL_NAME = '{Cycle}') group by TC_STATUS order by TC_STATUS desc");
            return dts;
        }

        public DataTable GetMissingData(string project, string metric)
        {
            var dta = new DataTable();
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            switch (metric)
            {               
                case "tiptr":
                    dta = projectDb.SqlQuery(
                        $"select TC_TESTCYCL_ID, TC_USER_TEMPLATE_72 as PTR, TC_TEST_ID, TS_NAME from td.TESTCYCL inner join td.TEST on td.TESTCYCL.TC_TEST_ID = td.TEST.TS_TEST_ID where TC_USER_TEMPLATE_72 like '%1[0-9][0-9][0-9][0-9][0-9][0-9][0-9]' and TC_ASSIGN_RCYC in (select RCYC_ID from td.RELEASES inner join td.RELEASE_CYCLES on REL_ID = RCYC_PARENT_ID where REL_NAME = '{Cycle}') and TC_TESTCYCL_ID NOT IN (select LN_ENTITY_ID from td.LINK where LN_ENTITY_TYPE = 'TESTCYCL')");
                    break;
                case "tifd":
                    dta = projectDb.SqlQuery(
                        $"select TC_TESTCYCL_ID, TC_TEST_ID, TS_NAME from td.TESTCYCL t1 inner join td.TEST t2 on t1.TC_TEST_ID = t2.TS_TEST_ID where t1.TC_ASSIGN_RCYC in (select RCYC_ID from td.RELEASES inner join td.RELEASE_CYCLES on REL_ID = RCYC_PARENT_ID where REL_NAME = '{Cycle}') and t1.TC_STATUS in ('Failed', 'Blocked') and TC_TESTCYCL_ID not in ( select distinct(LN_ENTITY_ID) from td.LINK where LN_ENTITY_TYPE in ('TESTCYCL') and LN_ENTITY_ID IN (select TC_TESTCYCL_ID from td.TESTCYCL where TC_ASSIGN_RCYC in (select RCYC_ID from td.RELEASES inner join td.RELEASE_CYCLES on REL_ID = RCYC_PARENT_ID where REL_NAME = '{Cycle}') and TC_STATUS in ('Failed', 'Blocked')))");
                    break;
            }

            return dta;
        }

    }
}