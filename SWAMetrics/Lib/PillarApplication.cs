using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SWAMetrics.Lib
{
    public class Pillar
    {
        public string Project { get; set; }
        public string ProjectDatbaseName { get; set; }
        public string Release { get; set; }
        public string Sql;
        private DataTable _table;

        public Pillar()
        {
        }

        public DataTable GetTemplateProjects()
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", "qcsiteadmin_db");
            return projectDb.SqlQuery("select t1.PROJECT_NAME from [td].[PROJECTS] t1 inner join [td].[PROJECT_LINKS] t2 on t2.PRL_TO_PROJECT_UID = t1.PROJECT_UID where t2.PRL_FROM_PROJECT_UID = '33edb5f0-3364-48b4-ae3c-44c2076c152e' and t1.DOMAIN_NAME = 'SWA_TECHNOLOGY'");
        }

        private string GetProjectDbName(string project)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", "qcsiteadmin_db");
            var _table = projectDb.SqlQuery($"select db_name from [qcsiteadmin_db].[td].[PROJECTS] where project_name = '{project}' and domain_name='swa_technology'");
            if (_table != null)
            {
                ProjectDatbaseName = _table.Rows[0]["db_name"].ToString();
            }
            return ProjectDatbaseName;
        }

        public List<string> GetActiveReleases(string project)
        {
            List<string> releases = new List<string>();
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            var _table = projectDb.SqlQuery("select REL_NAME from td.RELEASES where REL_END_DATE > GETDATE() and REL_START_DATE < GETDATE()");
            if (_table != null)
            {
                releases = _table.AsEnumerable().Select(p => p.Field<string>("REL_NAME")).ToList();
            }
            return releases;
        }

        public DataTable GetApplicationFTP(string project, string release)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"select Count(t4.RN_STATUS) as AppCount, t4.RN_STATUS as RunStatus, t5.Application from td.RUN t4 inner join (select t2.TS_USER_TEMPLATE_02 as Application, t3.RN_TESTCYCL_ID, t3.RunId from td.TEST t2 inner join (select distinct(t1.RN_TESTCYCL_ID), min(t1.RN_RUN_ID) as RunId, t1.RN_TEST_ID from td.RUN t1 where t1.RN_ASSIGN_RCYC in (select a1.RCYC_ID from td.RELEASE_CYCLES a1 inner join td.RELEASES a2 on a1.RCYC_PARENT_ID = a2.REL_ID where a2.REL_NAME in ('{release}')) group by RN_TESTCYCL_ID, t1.RN_TEST_ID) t3 on t2.TS_TEST_ID = t3.RN_TEST_ID where t2.TS_USER_TEMPLATE_02 IS NOT NULL) t5 on t4.RN_RUN_ID = t5.RunId where t4.RN_STATUS in ('Passed', 'Failed', 'Blocked') group by t4.RN_STATUS, t5.Application");
        }

        public DataTable GetApplicationTotalRuns(string project, string release)    
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"select Count(t2.TS_USER_TEMPLATE_02) as AppCount, t2.TS_USER_TEMPLATE_02 as Application, t3.RN_STATUS as RunStatus from td.TEST t2 inner join (select * from td.RUN t1 where t1.RN_ASSIGN_RCYC in (select a1.RCYC_ID from td.RELEASE_CYCLES a1 inner join td.RELEASES a2 on a1.RCYC_PARENT_ID = a2.REL_ID where a2.REL_NAME in ('{release}')) and t1.RN_STATUS in ('Failed','Blocked','Passed')) t3 on t3.RN_TEST_ID = t2.TS_TEST_ID where t2.TS_USER_TEMPLATE_02 IS NOT NULL group by t3.RN_STATUS, t2.TS_USER_TEMPLATE_02");
        }

        public DataTable GetApplicationTotalInstances(string project, string release)   
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"select Count(t2.TS_USER_TEMPLATE_02) as AppCount, t2.TS_USER_TEMPLATE_02 as Application, t3.TC_STATUS as RunStatus from td.TEST t2 inner join (select * from td.TESTCYCL t1 where t1.TC_ASSIGN_RCYC in (select a1.RCYC_ID from td.RELEASE_CYCLES a1 inner join td.RELEASES a2 on a1.RCYC_PARENT_ID = a2.REL_ID where a2.REL_NAME in ('{release}')) and t1.TC_STATUS in ('Failed','Blocked','Passed','Not Completed','No Run') ) t3 on t2.TS_TEST_ID = t3.TC_TEST_ID where t2.TS_USER_TEMPLATE_02 IS NOT NULL group by t3.TC_STATUS, t2.TS_USER_TEMPLATE_02");
        }


        

        // Defects

        public DataTable GetApplicationTotalDefects(string project, string release, string application)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"Select Count(*) as TotalDefects from td.BUG where BUG.BG_DETECTED_IN_REL in (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
        }

        public DataTable GetApplicationTotalOpenCriticalDefects(string project, string release, string application) 
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"SELECT Count(BG_SEVERITY) as OpenCritical FROM td.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) and (BUG.BG_SEVERITY='1 - Critical' or BUG.BG_USER_TEMPLATE_42 ='Serious') AND (BUG.BG_USER_TEMPLATE_27 NOT IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod') or BUG.BG_USER_TEMPLATE_47 NOT IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested')) and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
        }

        public DataTable GetApplicationTotalAvgAgingOpenCriticalDefects(string project, string release, string application)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"SELECT (CAST((avg(datediff(hour, BUG.BG_DETECTION_DATE, CURRENT_TIMESTAMP)))as decimal(10,2))/24)as AVGCriticalAgingOpen from TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) and ( BUG.BG_SEVERITY='1 - Critical' or BUG.BG_USER_TEMPLATE_42 ='Serious') AND (BUG.BG_USER_TEMPLATE_27 NOT IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod') or BUG.BG_USER_TEMPLATE_47 NOT IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested')) and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
        }

        public DataTable GetApplicationTotalOpenMajorDefects(string project, string release, string application)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"SELECT Count(BG_SEVERITY) as OpenMajorDefects FROM td.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) and (BUG.BG_SEVERITY='2 - Major' or BUG.BG_USER_TEMPLATE_42 ='Medium') AND (BUG.BG_USER_TEMPLATE_27 NOT IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod')or BUG.BG_USER_TEMPLATE_47 NOT IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested'))and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
        }

        public DataTable GetApplicationTotalAvgAgingOpenMajorDefects(string project, string release, string application)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"SELECT (CAST((avg(datediff(hour, BUG.BG_DETECTION_DATE, CURRENT_TIMESTAMP)))as decimal(10,2))/24) as AvgNumDaysOpenMajorDefects from TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) and ( BUG.BG_SEVERITY='2 - Major' or BUG.BG_USER_TEMPLATE_42 ='Medium') AND (BUG.BG_USER_TEMPLATE_27 NOT IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod') or BUG.BG_USER_TEMPLATE_47 NOT IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested'))and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
        }

        public DataTable GetApplicationTotalOpenMinorDefects(string project, string release, string application)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"SELECT Count(BG_SEVERITY) as OpenMinorDefects FROM td.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) and (BUG.BG_SEVERITY='3 - Minor' or BUG.BG_USER_TEMPLATE_42 ='Minor')AND (BUG.BG_USER_TEMPLATE_27 NOT IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod') or BUG.BG_USER_TEMPLATE_47 NOT IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested')) and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
        }

        public DataTable GetApplicationTotalAvgAgingOpenMinorDefects(string project, string release, string application)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"SELECT (CAST((avg(datediff(hour, BUG.BG_DETECTION_DATE, CURRENT_TIMESTAMP)))as decimal(10,2))/24) as AVGDaysOpenMinorDefect from TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) and ( BUG.BG_SEVERITY='3 - Minor' or BUG.BG_USER_TEMPLATE_42 ='Minor')AND (BUG.BG_USER_TEMPLATE_27 NOT IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod') or BUG.BG_USER_TEMPLATE_47 NOT IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested')) and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
        }

        public DataTable GetApplicationTotalOpenTrivialDefects(string project, string release, string application)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"SELECT Count(BG_SEVERITY)as TotalNumberofTrivialDefects FROM td.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) and (BUG.BG_SEVERITY='4 - Trivial') AND (BUG.BG_USER_TEMPLATE_27 NOT IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod') or BUG.BG_USER_TEMPLATE_47 NOT IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested'))and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
        }

        public DataTable GetApplicationTotalAvgAgingOpenTrivialDefects(string project, string release, string application)  
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"SELECT (CAST((avg(datediff(hour, BUG.BG_DETECTION_DATE, CURRENT_TIMESTAMP)))as decimal(10,2))/24) as AVGDaysTrivialDefectAging from TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) and BUG.BG_SEVERITY in ('4 - Trivial') AND BUG.BG_USER_TEMPLATE_27 NOT IN ( '18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod') and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
        }

        public DataTable GetApplicationNewDefects(string project, string release, string application)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"Select count(*) as NewDefects FROM td.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) AND (BUG.BG_USER_TEMPLATE_27 = '01 - New'or BUG.BG_USER_TEMPLATE_47 = 'New')and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
        }

        public DataTable GetApplicationAssignedDefects(string project, string release, string application)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"SELECT Count(*) as TotalNumberofAssignedDefects FROM TD.BUG where BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) and (BUG.BG_USER_TEMPLATE_27 IN ('02 - Assigned', '03 - In Analysis', '04 - Story Review', '05 - Ready for Dev', '06 - In Progress', '07 - In Code Review', '08 - Desk Check') or BUG.BG_USER_TEMPLATE_47 IN ('Assigned', 'Confirmed', 'Load toTest Requested', 'QA Deployed'))and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
        }

        public DataTable GetApplicationReadyToDeployDefects(string project, string release, string application)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"SELECT count(*) as InQAorDeloptoTest FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN(select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) AND(BUG.BG_USER_TEMPLATE_27 IN('09 - Ready for QA', '10 - In QA', '11 - Ready for Sign Off', '12 - Resolved', '13 - Deploying to Cert', '15 - In Cert') or BUG.BG_USER_TEMPLATE_47 IN('Solved''Loaded in Test', 'Verified in Test', 'Rejected as Invalid', 'Rejected as Unreproducible', 'Rejected as Change Required', 'Rejected as Duplicate'))and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
        }

        public DataTable GetApplicationClosedDefects(string project, string release, string application)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"SELECT Count(*) as ClosedDefects FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) AND ( BUG.BG_USER_TEMPLATE_27 IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod') or BUG.BG_USER_TEMPLATE_47 IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested')) and BUG.BG_USER_TEMPLATE_58 in ('{application}') ");
        }

        public DataTable GetApplicationAvgAgeOpenDefects(string project, string release, string application)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            return projectDb.SqlQuery($"SELECT (CAST((avg(datediff(hour, BUG.BG_DETECTION_DATE, CURRENT_TIMESTAMP)))as decimal(10,2))/24) as AVGAgeOfOpenDefects from TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}'))AND (BUG.BG_USER_TEMPLATE_27 NOT IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod') or BUG.BG_USER_TEMPLATE_47 NOT IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested'))and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
        }

        public double GetApplicationClosedAgingDefects(string project, string release, string application)   
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            _table =  projectDb.SqlQuery($"SELECT Count(*) as JiraClosedDefects FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from td.RELEASES where RELEASES.REL_NAME in ('{release}')) AND BUG.BG_USER_TEMPLATE_27 IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod') and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
            var JiraClosedDefects = _table.Rows[0]["JiraClosedDefects"].Equals(DBNull.Value) ? 0 : System.Convert.ToDouble(_table.Rows[0]["JiraClosedDefects"]);

            _table = projectDb.SqlQuery($"SELECT (CAST((avg(datediff(hour, BUG.BG_DETECTION_DATE, BUG.BG_CLOSING_DATE)))as decimal(10,2))/24) as JiraAVGAgeofClosed FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) AND BUG.BG_USER_TEMPLATE_27 IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod') and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
            var JiraAVGAgeofClosed = _table.Rows[0]["JiraAVGAgeofClosed"].Equals(DBNull.Value) ? 0 : System.Convert.ToDouble(_table.Rows[0]["JiraAVGAgeofClosed"]);
            
            _table = projectDb.SqlQuery($"SELECT Count(*) as NumberofPTRClosed FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from td.RELEASES where RELEASES.REL_NAME in ('{release}'))AND Bug.BG_USER_TEMPLATE_47 IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested') and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
            var NumberofPTRClosed = _table.Rows[0]["NumberofPTRClosed"].Equals(DBNull.Value) ? 0 : System.Convert.ToDouble(_table.Rows[0]["NumberofPTRClosed"]);

            _table = projectDb.SqlQuery($"SELECT (CAST((avg(datediff(hour, BUG.BG_DETECTION_DATE, BUG.BG_VTS)))as decimal(10,2))/24) as AVGAgeofClosedPTR FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from td.RELEASES where RELEASES.REL_NAME in ('{release}')) AND BUG.BG_USER_TEMPLATE_47 IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested') and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
            var AVGAgeofClosedPTR = _table.Rows[0]["AVGAgeofClosedPTR"].Equals(DBNull.Value) ? 0 : System.Convert.ToDouble(_table.Rows[0]["AVGAgeofClosedPTR"]);
            if ((double)JiraClosedDefects + (double)NumberofPTRClosed > 0)
            {
                return  (((double)JiraClosedDefects * (double)JiraAVGAgeofClosed) + ((double)NumberofPTRClosed * (double)AVGAgeofClosedPTR)) / ((double)NumberofPTRClosed + (double)JiraClosedDefects);
            }
            else
            {
                return 0;
            }
        }

        public double GetApplicationCriticalClosedAgingDefects(string project, string release, string application)
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            _table = projectDb.SqlQuery($"SELECT Count(*) as JIRACriticalClosed FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) AND BUG.BG_USER_TEMPLATE_27 IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod') and BUG.BG_SEVERITY ='1 - Critical' and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
            var JiraClosedDefects = _table.Rows[0]["JIRACriticalClosed"].Equals(DBNull.Value) ? 0 : System.Convert.ToDouble(_table.Rows[0]["JIRACriticalClosed"]);

            _table = projectDb.SqlQuery($"SELECT (CAST((avg(datediff(hour, BUG.BG_DETECTION_DATE, BUG.BG_CLOSING_DATE)))as decimal(10,2))/24) as JIRACriticalClosedAVGAge FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) AND BUG.BG_USER_TEMPLATE_27 IN ('18 - Closed','16 - Ready for Prod', '17 - Deploying to Prod')and BUG.BG_SEVERITY ='1 - Critical'and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
            var JiraAVGAgeofClosed = _table.Rows[0]["JIRACriticalClosedAVGAge"].Equals(DBNull.Value) ? 0 : System.Convert.ToDouble(_table.Rows[0]["JIRACriticalClosedAVGAge"]);

            _table = projectDb.SqlQuery($"SELECT Count(*) AS PTRSeriousClosed FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) AND Bug.BG_USER_TEMPLATE_47 IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested') and BUG.BG_USER_TEMPLATE_42 ='Serious'and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
            var NumberofPTRClosed = _table.Rows[0]["PTRSeriousClosed"].Equals(DBNull.Value) ? 0 : System.Convert.ToDouble(_table.Rows[0]["PTRSeriousClosed"]);

            _table = projectDb.SqlQuery($"SELECT (CAST((avg(datediff(hour, BUG.BG_DETECTION_DATE, BUG.BG_VTS)))as decimal(10,2))/24)as PTRSeriousClosedAvgAging FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) AND BUG.BG_USER_TEMPLATE_47 IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested')and BUG.BG_USER_TEMPLATE_42 ='Serious'and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
            var AVGAgeofClosedPTR = _table.Rows[0]["PTRSeriousClosedAvgAging"].Equals(DBNull.Value) ? 0 : System.Convert.ToDouble(_table.Rows[0]["PTRSeriousClosedAvgAging"]);

            if ((double)JiraClosedDefects + (double)NumberofPTRClosed > 0)
            {
                return (((double)JiraClosedDefects * (double)JiraAVGAgeofClosed) + ((double)NumberofPTRClosed * (double)AVGAgeofClosedPTR)) / ((double)NumberofPTRClosed + (double)JiraClosedDefects);
            }
            else
            {
                return 0;
            }
        }

        public double GetApplicationMajorClosedAgingDefects(string project, string release, string application)    
        {
            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", GetProjectDbName(project));
            _table = projectDb.SqlQuery($"SELECT Count(*) as Jira2MajorClosed FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) AND BUG.BG_USER_TEMPLATE_27 IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod') and BUG.BG_SEVERITY ='2 - Major'and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
            var JiraClosedDefects = _table.Rows[0]["Jira2MajorClosed"].Equals(DBNull.Value) ? 0 : System.Convert.ToDouble(_table.Rows[0]["Jira2MajorClosed"]);

            _table = projectDb.SqlQuery($"SELECT (CAST((avg(datediff(hour, BUG.BG_DETECTION_DATE, BUG.BG_CLOSING_DATE)))as decimal(10,2))/24) as JiraAVGAge2MajorClosedDefects FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) AND BUG.BG_USER_TEMPLATE_27 IN ('18 - Closed', '16 - Ready for Prod', '17 - Deploying to Prod')and BUG.BG_SEVERITY IN ('2 - Major') and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
            var JiraAVGAgeofClosed = _table.Rows[0]["JiraAVGAge2MajorClosedDefects"].Equals(DBNull.Value) ? 0 : System.Convert.ToDouble(_table.Rows[0]["JiraAVGAge2MajorClosedDefects"]);

            _table = projectDb.SqlQuery($"SELECT Count(*) as PTRMediumClosedDefect FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) AND Bug.BG_USER_TEMPLATE_47 IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested') and BUG.BG_USER_TEMPLATE_42 ='Medium' and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
            var NumberofPTRClosed = _table.Rows[0]["PTRMediumClosedDefect"].Equals(DBNull.Value) ? 0 : System.Convert.ToDouble(_table.Rows[0]["PTRMediumClosedDefect"]);

            _table = projectDb.SqlQuery($"SELECT (CAST((avg(datediff(hour, BUG.BG_DETECTION_DATE, BUG.BG_VTS)))as decimal(10,2))/24) as PTRAvgAgeMediumClosedDefects FROM TD.BUG WHERE BUG.BG_DETECTED_IN_REL IN (select RELEASES.REL_ID from RELEASES where RELEASES.REL_NAME in ('{release}')) AND BUG.BG_USER_TEMPLATE_47 IN ('Closed', 'Closed as Duplicate', 'Closed for Technical Limitations', 'Closed as Unreproducible', 'Closed as No Provider Action', 'Closed as Invalid', 'Load to Production Requested')and BUG.BG_USER_TEMPLATE_42 ='Medium'and BUG.BG_USER_TEMPLATE_58 in ('{application}')");
            var AVGAgeofClosedPTR = _table.Rows[0]["PTRAvgAgeMediumClosedDefects"].Equals(DBNull.Value) ? 0 : System.Convert.ToDouble(_table.Rows[0]["PTRAvgAgeMediumClosedDefects"]);

            if ((double)JiraClosedDefects + (double)NumberofPTRClosed > 0)
            {
                return (((double)JiraClosedDefects * (double)JiraAVGAgeofClosed) + ((double)NumberofPTRClosed * (double)AVGAgeofClosedPTR)) / ((double)NumberofPTRClosed + (double)JiraClosedDefects);
            }
            else
            {
                return 0;
            }
        }

    }
}   