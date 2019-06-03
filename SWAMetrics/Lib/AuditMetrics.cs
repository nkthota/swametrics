using System;
using System.Data;

namespace SWAMetrics.Lib
{
    public class AuditMetrics
    {
        private DataTable _table;
        public string Sql;

        public AuditMetrics(string startDate, string endDate)
        {
            StartDate = Convert.ToDateTime(startDate).ToShortDateString();
            EndDate = Convert.ToDateTime(endDate).ToShortDateString();
        }

        public string Project { get; set; }
        public string ProjectDatbaseName { get; set; }
        public string Release { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public DataTable GetTemplateProjects()
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", "qcsiteadmin_db_126");
            return projectDb.SqlQuery(
                "select t1.PROJECT_NAME from [td].[PROJECTS] t1 inner join [td].[PROJECT_LINKS] t2 on t2.PRL_TO_PROJECT_UID = t1.PROJECT_UID where t2.PRL_FROM_PROJECT_UID = '33edb5f0-3364-48b4-ae3c-44c2076c152e' and t1.DOMAIN_NAME = 'SWA_TECHNOLOGY'");
        }

        public DataTable GetProjectReleases(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery(
                "select REL_NAME from td.RELEASES");
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

        public RecordValues GetTestSteps(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(*) as RecCount from td.Test where TS_CREATION_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0]["RecCount"]);
            var dta = projectDb.SqlQuery(
                $"select t2.TS_TEST_ID, count(t1.DS_ID) from td.DESSTEPS t1 inner join td.TEST t2 on t1.DS_TEST_ID = t2.TS_TEST_ID where t2.TS_CREATION_DATE BETWEEN '{StartDate}' and '{EndDate}' group by t2.TS_TEST_ID having count(t1.DS_ID) > 1");
            var criticalRec = Convert.ToInt32(dta.Rows.Count);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;
            return recValues;
        }

        public RecordValues GetTestDuplicate(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(*) as RecCount from td.Test where TS_CREATION_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0]["RecCount"]);
            var dta = projectDb.SqlQuery(
                $"select TS_NAME from td.TEST where TS_NAME in (select distinct(TS_NAME) from td.TEST where TS_CREATION_DATE BETWEEN '{StartDate}' and '{EndDate}') group by TS_NAME having count(ts_name) > 1");
            var criticalRec = Convert.ToInt32(dta.Rows.Count);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = sampleRec - criticalRec;
            return recValues;
        }

        public RecordValues GetTestReqLinked(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(*) as RecCount from td.Test where TS_CREATION_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0]["RecCount"]);
            var dta = projectDb.SqlQuery(
                $"select count(*) as RecCount from td.REQ_COVER where RC_ENTITY_TYPE = 'TEST' and RC_ENTITY_ID in (select TS_TEST_ID from td.TEST where TS_CREATION_DATE BETWEEN '{StartDate}' and '{EndDate}')");
            var criticalRec = Convert.ToInt32(dta.Rows[0]["RecCount"]);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;
            return recValues;
        }

        public RecordValues GetAssociatedCycles(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(*) from td.RELEASE_CYCLES where cast(RCYC_VTS as date) BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select count(distinct(t1.CY_ASSIGN_RCYC)) from td.CYCLE t1 inner join td.RELEASE_CYCLES t2 on t1.CY_ASSIGN_RCYC = t2.RCYC_ID where cast(RCYC_VTS as date) BETWEEN '{StartDate}' and '{EndDate}'");
            var criticalRec = Convert.ToInt32(dta.Rows[0][0]);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;
            return recValues;
        }

        public RecordValues GetReqCoverage(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(*) from td.REQ where RQ_TYPE_ID IN (select TPR_TYPE_ID from td.REQ_TYPE where TPR_NAME in('Functional','Testing','Undefined','Performance','Story','Feature','Quality','Compliance','Security','System')) and RQ_REQ_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select count(distinct(t1.RC_REQ_ID)) from [td].[REQ_COVER] t1 inner join td.REQ t2 on t2.RQ_REQ_ID = t1.RC_REQ_ID where t2.RQ_REQ_ID in (select RQ_REQ_ID from td.REQ where RQ_TYPE_ID IN (select TPR_TYPE_ID from td.REQ_TYPE where TPR_NAME in('Functional','Testing','Undefined','Performance','Story','Feature','Quality','Compliance','Security','System')) and RQ_REQ_DATE BETWEEN '{StartDate}' and '{EndDate}')");
            var criticalRec = Convert.ToInt32(dta.Rows[0][0]);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;
            return recValues;
        }

        public RecordValues GetReqSourceId(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(*) from td.REQ where RQ_TYPE_ID IN (select TPR_TYPE_ID from td.REQ_TYPE where TPR_NAME in('Functional','Testing','Undefined','Performance','Story','Feature','Quality','Compliance','Security','System')) and RQ_REQ_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select count(*) from td.REQ where RQ_USER_TEMPLATE_08 IS NOT NULL and RQ_TYPE_ID IN (select TPR_TYPE_ID from td.REQ_TYPE where TPR_NAME in('Functional','Testing','Undefined','Performance','Story','Feature','Quality','Compliance','Security','System')) and RQ_REQ_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var criticalRec = Convert.ToInt32(dta.Rows[0][0]);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;
            return recValues;
        }

        public RecordValues GetReqCycle(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(*) from td.REQ where RQ_TYPE_ID IN (select TPR_TYPE_ID from td.REQ_TYPE where TPR_NAME in('Functional','Testing','Undefined','Performance','Story','Feature','Quality','Compliance','Security','System')) and RQ_REQ_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select count(*) from td.REQ where RQ_TARGET_RCYC_VARCHAR IS NOT NULL and RQ_TYPE_ID IN (select TPR_TYPE_ID from td.REQ_TYPE where TPR_NAME in('Functional','Testing','Undefined','Performance','Story','Feature','Quality','Compliance','Security','System')) and RQ_REQ_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var criticalRec = Convert.ToInt32(dta.Rows[0][0]);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;
            return recValues;
        }

        public RecordValues GetDefectInstanceLink(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(*) from td.BUG where BG_DETECTION_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select count(distinct(LN_BUG_ID)) from td.LINK where LN_BUG_ID IN (select BG_BUG_ID from td.BUG where BG_DETECTION_DATE BETWEEN '{StartDate}' and '{EndDate}')");
            var criticalRec = Convert.ToInt32(dta.Rows[0][0]);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;
            return recValues;
        }

        public RecordValues GetDefectSWAId(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(*) from td.BUG where BG_DETECTION_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select count(bg_bug_id) from td.bug where BG_USER_TEMPLATE_26 is NOT NULL and BG_DETECTION_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var criticalRec = Convert.ToInt32(dta.Rows[0][0]);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;
            return recValues;
        }

        public RecordValues GetDefectPtr(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(bg_bug_id) from td.bug where BG_USER_TEMPLATE_29 = 'PTR' and cast(BG_VTS as date) BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select count(distinct(LN_BUG_ID)) from td.LINK where LN_ENTITY_TYPE in ('TESTCYCL') and LN_BUG_ID IN (select BG_BUG_ID from td.BUG where BG_USER_TEMPLATE_29 = 'PTR' and cast(BG_VTS as date) BETWEEN '{StartDate}' and '{EndDate}')");
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
                $"select count(TC_USER_TEMPLATE_72) from td.TESTCYCL where TC_USER_TEMPLATE_72 like '%1[0-9][0-9][0-9][0-9][0-9][0-9][0-9]' and TC_EXEC_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select TC_USER_TEMPLATE_72 as PTR, TC_TEST_ID, TS_NAME from td.TESTCYCL inner join td.TEST on td.TESTCYCL.TC_TEST_ID = td.TEST.TS_TEST_ID where TC_USER_TEMPLATE_72 like '%1[0-9][0-9][0-9][0-9][0-9][0-9][0-9]' and TC_EXEC_DATE BETWEEN '{StartDate}' and '{EndDate}' and TC_TESTCYCL_ID IN (select LN_ENTITY_ID from td.LINK where LN_ENTITY_TYPE = 'TESTCYCL')");
            var criticalRec = Convert.ToInt32(dta.Rows.Count);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;
            return recValues;
        }

        public RecordValues GetTestSetCycle(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(*) from td.CYCLE where cast(CY_VTS as date) BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select count(*) from td.CYCLE where CY_ASSIGN_RCYC IS NOT NULL and  cast(CY_VTS as date) BETWEEN '{StartDate}' and '{EndDate}'");
            var criticalRec = Convert.ToInt32(dta.Rows[0][0]);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;
            return recValues;
        }

        public RecordValues GetTestSetNotCompleted(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(distinct(RN_TESTCYCL_ID)) from td.RUN where RN_EXECUTION_DATE BETWEEN '{StartDate}' and '{EndDate}' group by RN_TESTCYCL_ID");
            var sampleRec = dts.Rows.Count;
            var dta = projectDb.SqlQuery(
                $"select RN_TESTCYCL_ID, count(RN_TESTCYCL_ID) from td.RUN where RN_STATUS = 'Not Completed' and RN_EXECUTION_DATE BETWEEN '{StartDate}' and '{EndDate}' group by RN_TESTCYCL_ID having count(RN_TESTCYCL_ID) > 1");
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
                $"select count(*) from td.TESTCYCL where TC_EXEC_DATE BETWEEN '{StartDate}' and '{EndDate}' and TC_STATUS in ('Failed', 'Blocked')");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select count(distinct(LN_ENTITY_ID)) from td.LINK where LN_ENTITY_TYPE in ('TESTCYCL') and LN_ENTITY_ID IN (select TC_TESTCYCL_ID from td.TESTCYCL where TC_EXEC_DATE BETWEEN '{StartDate}' and '{EndDate}' and TC_STATUS in ('Failed', 'Blocked'))");
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
                $"select count(*) from td.RUN where RN_EXECUTION_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var dta = projectDb.SqlQuery(
                $"select count(*) from td.RUN where RN_RUN_NAME not like 'Fast_Run%' and RN_EXECUTION_DATE BETWEEN '{StartDate}' and '{EndDate}'");
            var criticalRec = Convert.ToInt32(dta.Rows[0][0]);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = criticalRec;
            return recValues;
        }

        public RecordValues GetExcelReport(string project)
        {
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var dts = projectDb.SqlQuery(
                $"select count(*) FROM [td].[ANALYSIS_ITEMS] where AI_OWNER <> 'N/A' and  AI_TYPE = 'ExcelReport' and cast(AI_VTS as date) BETWEEN '{StartDate}' and '{EndDate}'");
            var sampleRec = Convert.ToInt32(dts.Rows[0][0]);
            var recValues = new RecordValues();
            recValues.SampleSize = sampleRec;
            recValues.CriteriaSize = 0;
            return recValues;
        }

        // get the criteria records for display

        public DataTable GetMissingData(string project, string metric)
        {
            var dta = new DataTable();
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            switch (metric)
            {
                case "tpds":
                    dta = projectDb.SqlQuery(
                        $"select TS_TEST_ID, TS_NAME from td.TEST where TS_CREATION_DATE BETWEEN '{StartDate}' and '{EndDate}' and TS_TEST_ID not in ( select t2.TS_TEST_ID from td.DESSTEPS t1 inner join td.TEST t2 on t1.DS_TEST_ID = t2.TS_TEST_ID where t2.TS_CREATION_DATE BETWEEN '{StartDate}' and '{EndDate}' group by t2.TS_TEST_ID having count(t1.DS_ID) > 1)");
                    break;
                case "tpdr":
                    dta = projectDb.SqlQuery(
                        $"select t1.TS_TEST_ID, t1.TS_NAME from td.TEST t1 inner join (select TS_NAME from td.TEST where TS_NAME in (select distinct(TS_NAME) from td.TEST where TS_CREATION_DATE BETWEEN '{StartDate}' and '{EndDate}') group by TS_NAME having count(ts_name) > 1) t2 on t1.TS_NAME = t2.TS_NAME");
                    break;
                case "tnlr":
                    dta = projectDb.SqlQuery(
                        $"select TS_TEST_ID, TS_NAME from td.TEST where TS_CREATION_DATE BETWEEN '{StartDate}' and '{EndDate}' and TS_TEST_ID NOT IN (select RC_ENTITY_ID from td.REQ_COVER where RC_ENTITY_TYPE = 'TEST')");
                    break;
                case "rcts":
                    dta = projectDb.SqlQuery(
                        $"select RCYC_ID, RCYC_NAME from td.RELEASE_CYCLES where cast(RCYC_VTS as date) BETWEEN '{StartDate}' and '{EndDate}' and RCYC_ID not in ( select distinct t1.CY_ASSIGN_RCYC from td.CYCLE t1 inner join td.RELEASE_CYCLES t2 on t1.CY_ASSIGN_RCYC = t2.RCYC_ID where cast(RCYC_VTS as date) BETWEEN '{StartDate}' and '{EndDate}')");
                    break;
                case "rqtc":
                    dta = projectDb.SqlQuery(
                        $"select RQ_REQ_ID, RQ_REQ_NAME from td.REQ where RQ_TYPE_ID IN (select TPR_TYPE_ID from td.REQ_TYPE where TPR_NAME in('Functional','Testing','Undefined','Performance','Story','Feature','Quality','Compliance','Security','System')) and RQ_REQ_DATE BETWEEN '{StartDate}' and '{EndDate}' and RQ_REQ_ID not in ( select distinct(t1.RC_REQ_ID) from [td].[REQ_COVER] t1 inner join td.REQ t2 on t2.RQ_REQ_ID = t1.RC_REQ_ID where t2.RQ_REQ_ID in (select RQ_REQ_ID from td.REQ where RQ_REQ_DATE BETWEEN '{StartDate}' and '{EndDate}'))");
                    break;
                case "rqsid":
                    dta = projectDb.SqlQuery(
                        $"select RQ_REQ_ID, RQ_REQ_NAME from td.REQ where RQ_USER_TEMPLATE_08 IS NULL and RQ_TYPE_ID IN (select TPR_TYPE_ID from td.REQ_TYPE where TPR_NAME in('Functional','Testing','Undefined','Performance','Story','Feature','Quality','Compliance','Security','System')) and RQ_REQ_DATE BETWEEN '{StartDate}' and '{EndDate}'");
                    break;
                case "rqcy":
                    dta = projectDb.SqlQuery(
                        $"select RQ_REQ_ID, RQ_REQ_NAME from td.REQ where RQ_TARGET_RCYC_VARCHAR IS NULL and RQ_TYPE_ID IN (select TPR_TYPE_ID from td.REQ_TYPE where TPR_NAME in('Functional','Testing','Undefined','Performance','Story','Feature','Quality','Compliance','Security','System')) and RQ_REQ_DATE BETWEEN '{StartDate}' and '{EndDate}'");
                    break;
                case "dfti":
                    dta = projectDb.SqlQuery(
                        $"select BG_BUG_ID, BG_SUMMARY from td.BUG where BG_DETECTION_DATE BETWEEN '{StartDate}' and '{EndDate}' and BG_BUG_ID not in ( select distinct(LN_BUG_ID) from td.LINK where LN_BUG_ID IN (select BG_BUG_ID from td.BUG where BG_DETECTION_DATE BETWEEN '{StartDate}' and '{EndDate}'))");
                    break;
                case "dfid":
                    dta = projectDb.SqlQuery(
                        $"select BG_BUG_ID, BG_SUMMARY from td.bug where BG_USER_TEMPLATE_26 is NULL and BG_DETECTION_DATE BETWEEN '{StartDate}' and '{EndDate}'");
                    break;
                case "dfptr":
                    dta = projectDb.SqlQuery(
                        $"select BG_BUG_ID, BG_SUMMARY from td.bug where BG_USER_TEMPLATE_29 = 'PTR' and cast(BG_VTS as date) BETWEEN '{StartDate}' and '{EndDate}' and BG_BUG_ID not in ( select distinct(LN_BUG_ID) from td.LINK where LN_ENTITY_TYPE in ('TESTCYCL') and LN_BUG_ID IN (select BG_BUG_ID from td.BUG where cast(BG_VTS as date) BETWEEN '{StartDate}' and '{EndDate}'))");
                    break;
                case "tsrc":
                    dta = projectDb.SqlQuery(
                        $"select CY_CYCLE_ID, CY_CYCLE from td.CYCLE where CY_ASSIGN_RCYC IS NULL and  cast(CY_VTS as date) BETWEEN '{StartDate}' and '{EndDate}'");
                    break;
                case "tiptr":
                    dta = projectDb.SqlQuery(
                        $"select TC_TESTCYCL_ID, TC_USER_TEMPLATE_72 as PTR, TC_TEST_ID, TS_NAME from td.TESTCYCL inner join td.TEST on td.TESTCYCL.TC_TEST_ID = td.TEST.TS_TEST_ID where TC_USER_TEMPLATE_72 like '%1[0-9][0-9][0-9][0-9][0-9][0-9][0-9]' and TC_EXEC_DATE BETWEEN '{StartDate}' and '{EndDate}' and TC_TESTCYCL_ID NOT IN (select LN_ENTITY_ID from td.LINK where LN_ENTITY_TYPE = 'TESTCYCL')");
                    break;
                case "tifd":
                    dta = projectDb.SqlQuery(
                        $"select TC_TESTCYCL_ID, TC_TEST_ID, TS_NAME from td.TESTCYCL t1 inner join td.TEST t2 on t1.TC_TEST_ID = t2.TS_TEST_ID where t1.TC_EXEC_DATE BETWEEN '{StartDate}' and '{EndDate}' and t1.TC_STATUS in ('Failed', 'Blocked') and TC_TESTCYCL_ID not in ( select distinct(LN_ENTITY_ID) from td.LINK where LN_ENTITY_TYPE in ('TESTCYCL') and LN_ENTITY_ID IN (select TC_TESTCYCL_ID from td.TESTCYCL where TC_EXEC_DATE BETWEEN '{StartDate}' and '{EndDate}' and TC_STATUS in ('Failed', 'Blocked')))");
                    break;
            }

            return dta;
        }


        //// Richard report

        public DataTable GetLatestTestStatusByCycle(string cycles)
        {
            // get the list of cycle id based on cycle names
            var projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName("SWAMX"));
            return projectDb.SqlQuery(
                $"select t6.RN_TEST_ID, t5.TS_NAME, t6.RN_STATUS from swa_technology_wizard_db.td.TEST t5 inner join (select t4.RN_TEST_ID, t3.RN_STATUS from swa_technology_wizard_db.td.RUN t3 inner join (select t1.RN_TEST_ID, MAX(t1.RN_RUN_ID) as LastRunId from swa_technology_wizard_db.td.RUN t1 inner join swa_technology_wizard_db.td.RELEASE_CYCLES t2 on t1.RN_ASSIGN_RCYC = t2.RCYC_ID where t2.RCYC_NAME in ('{cycles}') group by t1.RN_TEST_ID) t4 on t4.LastRunId = t3.RN_RUN_ID) t6 on t6.RN_TEST_ID = t5.TS_TEST_ID");
        }
    }

    public class RecordValues
    {
        public int SampleSize { get; set; }
        public int CriteriaSize { get; set; }
    }
}