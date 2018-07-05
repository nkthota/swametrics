using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace SWAMetrics.Lib
{
    public class LastProjectAccessed
    {
        public List<LastActivity> GetLastAccessedDetails()
        {
            var lastActivities = new List<LastActivity>();

            Database projectDb = new Database("td", "tdtdtd", "WPSQLQLC01", "qcsiteadmin_db");
            DataTable projects = projectDb.SqlQuery("select Project_Name,Domain_Name, Db_Name, PR_IS_ACTIVE from td.Projects");
            foreach (DataRow row in projects.Rows)
            {
                Database currentDb = new Database("td", "tdtdtd", "WPSQLQLC01", row["Db_Name"].ToString());
                DataTable logs = currentDb.SqlQuery("select MAX(AU_TIME) as LastUsed from [td].[AUDIT_LOG]");
                DateTime lastUseDateTime = Convert.ToDateTime(logs.Rows[0]["LastUsed"].ToString());
                int daysLastUsed = (DateTime.Now - lastUseDateTime).Days;

                lastActivities.Add(new LastActivity
                {
                    Domain = row["Domain_Name"].ToString(),
                    ProjectName = row["Project_Name"].ToString(),
                    DaysFromNow = daysLastUsed,
                    LastAccessed = lastUseDateTime.ToShortDateString(),
                    Active = row["PR_IS_ACTIVE"].ToString()
                });
            }

            return lastActivities;
        }
    }

        
    public class LastActivity
    {
        public string Domain { get; set; }
        public string ProjectName { get; set; }
        public string LastAccessed { get; set; }
        public int DaysFromNow { get; set; }
        public string Active { get; set; }
    }
}