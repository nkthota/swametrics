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
            var _table = projectDb.SqlQuery($"select db_name from [qcsiteadmin_db].[td].[PROJECTS] where project_name = '{project}'");
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
    }
}   