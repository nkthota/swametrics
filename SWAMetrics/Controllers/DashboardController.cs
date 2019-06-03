using Itenso.TimePeriod;
using SWAMetrics.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using SWAMetrics.Lib;
using Newtonsoft.Json;

namespace SWAMetrics.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ls_dashboardEntities _db = new ls_dashboardEntities();
        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Sample()
        {
            return View("Sample");
        }

        public ActionResult Calendar()
        {
            return View();
        }

        public ActionResult SyncFix()
        {
            return View("SyncFix");
        }

        public JsonResult IgnoreRecord(string project, string release, string application)
        {
            var retData = new SyncManager().UpdateRecord(project, release, application, "t");
            return Json(new { retData }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EnableRecord(string project, string release, string application)
        {
            var retData = new SyncManager().UpdateRecord(project, release, application, "f");
            return Json(new { retData }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateEp1(string project, string release, string application) 
        {
            var retData = new SyncManager().UpdateMappingRecord(project, release, application, "ep1");
            return Json(new { retData }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateEp2(string project, string release, string application)
        {
            var retData = new SyncManager().UpdateMappingRecord(project, release, application, "ep2");
            return Json(new { retData }, JsonRequestBehavior.AllowGet);
        }



        public ActionResult CycleFirstTimePass()
        {
            return View();
        }

        public ActionResult ProjectMonthlyMetrics()
        {
            return View(_db.ProjectMonthlyExecutionMetrics.ToList());
        }

        public ActionResult ProjectReleaseMetrics()
        {
            return View(_db.ExecutionMetricsCycles.ToList());
        }

        public ActionResult LastProjectActivity()
        {
            return View(new LastProjectAccessed().GetLastAccessedDetails().ToList());
        }

        public ActionResult FirstTimePass()
        {
            ViewBag.LastUpdated = _db.DataRefreshes.OrderByDescending(p => p.ID).FirstOrDefault()?.LastUpdated;
            return View();
        }

        public ActionResult FirstTimeFail()
        {
            ViewBag.LastUpdated = _db.DataRefreshes.OrderByDescending(p => p.ID).FirstOrDefault()?.LastUpdated;
            return View();
        }

        public ActionResult ReRunFail()
        {
            ViewBag.LastUpdated = _db.DataRefreshes.OrderByDescending(p => p.ID).FirstOrDefault()?.LastUpdated;
            return View();
        }

        public ActionResult ReRunPass()
        {
            ViewBag.LastUpdated = _db.DataRefreshes.OrderByDescending(p => p.ID).FirstOrDefault()?.LastUpdated;
            return View();
        }

        public ActionResult ReRun()
        {
            ViewBag.LastUpdated = _db.DataRefreshes.OrderByDescending(p => p.ID).FirstOrDefault()?.LastUpdated;
            return View();
        }

        public ActionResult ExecutionData()
        {
            ViewBag.LastUpdated = _db.DataRefreshes.OrderByDescending(p => p.ID).FirstOrDefault()?.LastUpdated;
            return View();
        }

        public ActionResult ExecutionDataPI()
        {
            ViewBag.LastUpdated = _db.DataRefreshes.OrderByDescending(p => p.ID).FirstOrDefault()?.LastUpdated;
            return View();
        }

        public ActionResult DefectData()
        {
            ViewBag.LastUpdated = _db.DataRefreshes.OrderByDescending(p => p.ID).FirstOrDefault()?.LastUpdated;
            return View();
        }

        public JsonResult LoadExecutionWeekly()
        {
            var data = _db.WeeklyExecutionMetrics;
            return Json(new { data = data.ToList() }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadPivotData()
        {
            var data = _db.WeeklyExecutionMetricsProgramInits;
            return Json(data.ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadExecutionPIWeekly()
        {
            var data = _db.WeeklyExecutionMetricsProgramInits;
            return Json(new { data = data.ToList() }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCycles()
        {
            List<ReleaseCycles> rc = new List<ReleaseCycles>();
            foreach (Cycle c in _db.Cycles)
            {
                rc.Add(new ReleaseCycles
                {
                    id = c.CycleName,
                    text = c.CycleName
                });
            }
            return Json(rc.ToList(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult LoadDefects()
        {
            var data = _db.DefectMetrics;
            return Json(new { data = data.ToList() }, JsonRequestBehavior.AllowGet);
        }

        public string LastRefreshedOn()
        {
            var json = _db.DataRefreshes.OrderByDescending(p => p.ID).FirstOrDefault()?.LastUpdated;
            return json;
        }

        public JsonResult FetchFtp()
        {
            List<string> cwdata = new List<string>();
            List<int> cwdataList = new List<int>();

            DateTime start = DateTime.Now;
            Week week = new Week(start);

            for (int i = 0; i < 8; i++)
            {
                cwdata.Add("\"W" + week.WeekOfYear + "\"");
                cwdataList.Add(week.WeekOfYear);
                week = week.GetPreviousWeek();
            }

            var appmatches = from t in _db.WeeklyExecutionMetricsProgramInits
                             where cwdataList.Contains(t.CalenderWeek)
                             select new
                             {
                                 t.Application
                             };
            var apps = appmatches.Distinct().ToArray();

            string weeksapplicationdata = string.Empty;

            foreach (var app in apps)
            {
                string weeksdata = string.Empty;

                foreach (var wk in cwdataList)
                {
                    var mMatches = _db.WeeklyExecutionMetricsProgramInits.Where(p => p.CalenderWeek == wk && p.Application == app.Application).ToList();
                    if (mMatches.Count != 0)
                    {
                        decimal passpercent = 0;
                        int counter = 0;
                        decimal currentvalue = 0;
                        foreach (var mMatch in mMatches)
                        {
                            if (mMatch.PercentFirstRunPassed != null && mMatch.PercentFirstRunPassed != 0)
                            {
                                passpercent = passpercent + (decimal)mMatch.PercentFirstRunPassed;
                                counter = counter + 1;
                            }
                        }

                        if (counter == 0)
                        {
                            currentvalue = (decimal) (passpercent);
                        }
                        else
                        {
                            currentvalue = (decimal)(passpercent / counter);
                        }
                        
                        weeksdata = weeksdata + currentvalue.ToString(CultureInfo.InvariantCulture) + ",";
                    }
                    else
                    {
                        weeksdata = weeksdata + "0" + ",";
                    }
                }

                weeksdata = weeksdata.Substring(0, weeksdata.Length - 1);
                if (weeksdata != "0,0,0,0,0,0,0,0")
                {
                    weeksapplicationdata = weeksapplicationdata + "{\"name\":\"" + app.Application + "\",\"data\":[" + weeksdata + "]},";
                }
            }
            weeksapplicationdata = "[" + weeksapplicationdata.Substring(0, weeksapplicationdata.Length - 1) + "]";
            var json = "[{\"name\":\"Weeks\",\"data\":[" + string.Join(",", cwdata.ToArray()) +
                        "]},{\"name\":\"FTPP\",\"data\":" + weeksapplicationdata + "}]";

            return new JsonResult { Data = json, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult FetchRrp()
        {
            List<string> cwdata = new List<string>();
            List<int> cwdataList = new List<int>();

            DateTime start = DateTime.Now;
            Week week = new Week(start);

            for (int i = 0; i < 8; i++)
            {
                cwdata.Add("\"W" + week.WeekOfYear + "\"");
                cwdataList.Add(week.WeekOfYear);
                week = week.GetPreviousWeek();
            }

            var appmatches = from t in _db.WeeklyExecutionMetricsProgramInits
                             where cwdataList.Contains(t.CalenderWeek)
                             select new
                             {
                                 t.Application
                             };
            var apps = appmatches.Distinct().ToArray();

            string weeksapplicationdata = string.Empty;

            foreach (var app in apps)
            {
                string weeksdata = string.Empty;

                foreach (var wk in cwdataList)
                {
                    var mMatches = _db.WeeklyExecutionMetricsProgramInits.Where(p => p.CalenderWeek == wk && p.Application == app.Application).ToList();
                    if (mMatches.Count != 0)
                    {
                        decimal passpercent = 0;
                        int counter = 0;
                        decimal currentvalue = 0;
                        foreach (var mMatch in mMatches)
                        {
                            if (mMatch.PercentageReRunPassed != null && mMatch.PercentageReRunPassed != 0)
                            {
                                passpercent = passpercent + (decimal)mMatch.PercentageReRunPassed;
                                counter = counter + 1;
                            }
                        }

                        if (counter == 0)
                        {
                            currentvalue = (decimal)(passpercent);
                        }
                        else
                        {
                            currentvalue = (decimal)(passpercent / counter);
                        }

                        weeksdata = weeksdata + currentvalue.ToString(CultureInfo.InvariantCulture) + ",";
                    }
                    else
                    {
                        weeksdata = weeksdata + "0" + ",";
                    }
                }

                weeksdata = weeksdata.Substring(0, weeksdata.Length - 1);
                if (weeksdata != "0,0,0,0,0,0,0,0")
                {
                    weeksapplicationdata = weeksapplicationdata + "{\"name\":\"" + app.Application + "\",\"data\":[" + weeksdata + "]},";
                }
            }
            weeksapplicationdata = "[" + weeksapplicationdata.Substring(0, weeksapplicationdata.Length - 1) + "]";
            var json = "[{\"name\":\"Weeks\",\"data\":[" + string.Join(",", cwdata.ToArray()) +
                        "]},{\"name\":\"FTPP\",\"data\":" + weeksapplicationdata + "}]";

            return new JsonResult { Data = json, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult FetchFtf()
        {
            List<string> cwdata = new List<string>();
            List<int> cwdataList = new List<int>();

            DateTime start = DateTime.Now;
            Week week = new Week(start);

            for (int i = 0; i < 8; i++)
            {
                cwdata.Add("\"W" + week.WeekOfYear + "\"");
                cwdataList.Add(week.WeekOfYear);
                week = week.GetPreviousWeek();
            }

            var appmatches = from t in _db.WeeklyExecutionMetricsProgramInits
                             where cwdataList.Contains(t.CalenderWeek)
                             select new
                             {
                                 t.Application
                             };
            var apps = appmatches.Distinct().ToArray();

            string weeksapplicationdata = string.Empty;

            foreach (var app in apps)
            {
                string weeksdata = string.Empty;

                foreach (var wk in cwdataList)
                {
                    var mMatches = _db.WeeklyExecutionMetricsProgramInits.Where(p => p.CalenderWeek == wk && p.Application == app.Application).ToList();
                    if (mMatches.Count != 0)
                    {
                        decimal failpercent = 0;
                        int counter = 0;
                        decimal currentvalue = 0;
                        foreach (var mMatch in mMatches)
                        {
                            if (mMatch.PercentFirstRunFailed != null && mMatch.PercentFirstRunFailed != 0)
                            {
                                failpercent = failpercent + (decimal)mMatch.PercentFirstRunFailed;
                                counter = counter + 1;
                            }
                        }

                        if (counter == 0)
                        {
                            currentvalue = (decimal)(failpercent);
                        }
                        else
                        {
                            currentvalue = (decimal)(failpercent / counter);
                        }

                        weeksdata = weeksdata + currentvalue.ToString(CultureInfo.InvariantCulture) + ",";
                    }
                    else
                    {
                        weeksdata = weeksdata + "0" + ",";
                    }
                }

                weeksdata = weeksdata.Substring(0, weeksdata.Length - 1);
                if (weeksdata != "0,0,0,0,0,0,0,0")
                {
                    weeksapplicationdata = weeksapplicationdata + "{\"name\":\"" + app.Application + "\",\"data\":[" + weeksdata + "]},";
                }
            }
            weeksapplicationdata = "[" + weeksapplicationdata.Substring(0, weeksapplicationdata.Length - 1) + "]";
            var json = "[{\"name\":\"Weeks\",\"data\":[" + string.Join(",", cwdata.ToArray()) +
                        "]},{\"name\":\"FTPP\",\"data\":" + weeksapplicationdata + "}]";

            return new JsonResult { Data = json, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult FetchRrf()
        {
            List<string> cwdata = new List<string>();
            List<int> cwdataList = new List<int>();

            DateTime start = DateTime.Now;
            Week week = new Week(start);

            for (int i = 0; i < 8; i++)
            {
                cwdata.Add("\"W" + week.WeekOfYear + "\"");
                cwdataList.Add(week.WeekOfYear);
                week = week.GetPreviousWeek();
            }

            var appmatches = from t in _db.WeeklyExecutionMetricsProgramInits
                             where cwdataList.Contains(t.CalenderWeek)
                             select new
                             {
                                 t.Application
                             };
            var apps = appmatches.Distinct().ToArray();

            string weeksapplicationdata = string.Empty;

            foreach (var app in apps)
            {
                string weeksdata = string.Empty;

                foreach (var wk in cwdataList)
                {
                    var mMatches = _db.WeeklyExecutionMetricsProgramInits.Where(p => p.CalenderWeek == wk && p.Application == app.Application).ToList();
                    if (mMatches.Count != 0)
                    {
                        decimal failpercent = 0;
                        int counter = 0;
                        decimal currentvalue = 0;
                        foreach (var mMatch in mMatches)
                        {
                            if (mMatch.PercentageReRunFailed != null && mMatch.PercentageReRunFailed != 0)
                            {
                                failpercent = failpercent + (decimal)mMatch.PercentageReRunFailed;
                                counter = counter + 1;
                            }
                        }

                        if (counter == 0)
                        {
                            currentvalue = (decimal)(failpercent);
                        }
                        else
                        {
                            currentvalue = (decimal)(failpercent / counter);
                        }

                        weeksdata = weeksdata + currentvalue.ToString(CultureInfo.InvariantCulture) + ",";
                    }
                    else
                    {
                        weeksdata = weeksdata + "0" + ",";
                    }
                }

                weeksdata = weeksdata.Substring(0, weeksdata.Length - 1);
                if (weeksdata != "0,0,0,0,0,0,0,0")
                {
                    weeksapplicationdata = weeksapplicationdata + "{\"name\":\"" + app.Application + "\",\"data\":[" + weeksdata + "]},";
                }
            }
            weeksapplicationdata = "[" + weeksapplicationdata.Substring(0, weeksapplicationdata.Length - 1) + "]";
            var json = "[{\"name\":\"Weeks\",\"data\":[" + string.Join(",", cwdata.ToArray()) +
                        "]},{\"name\":\"FTPP\",\"data\":" + weeksapplicationdata + "}]";

            return new JsonResult { Data = json, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public JsonResult FetchRt()
        {
            List<string> cwdata = new List<string>();
            List<int> cwdataList = new List<int>();

            DateTime start = DateTime.Now;
            Week week = new Week(start);

            for (int i = 0; i < 8; i++)
            {
                cwdata.Add("\"W" + week.WeekOfYear + "\"");
                cwdataList.Add(week.WeekOfYear);
                week = week.GetPreviousWeek();
            }

            var appmatches = from t in _db.WeeklyExecutionMetrics
                             where cwdataList.Contains(t.CalenderWeek)
                             select new
                             {
                                 t.Application
                             };
            var apps = appmatches.Distinct().ToArray();

            string weeksapplicationdata = string.Empty;

            foreach (var app in apps)
            {
                string weeksdata = string.Empty;

                foreach (var wk in cwdataList)
                {
                    var mMatches = _db.WeeklyExecutionMetrics.Where(p => p.CalenderWeek == wk && p.Application == app.Application).ToList();
                    if (mMatches.Count != 0)
                    {
                        weeksdata = weeksdata + mMatches[0].PercentageReTestInstancesFailed.ToString() + ",";
                    }
                    else
                    {
                        weeksdata = weeksdata + "0" + ",";
                    }
                }

                weeksdata = weeksdata.Substring(0, weeksdata.Length - 1);
                if (weeksdata != "0,0,0,0,0,0,0,0")
                {
                    weeksapplicationdata = weeksapplicationdata + "{\"name\":\"" + app.Application + "\",\"data\":[" + weeksdata + "]},";
                }
            }
            weeksapplicationdata = "[" + weeksapplicationdata.Substring(0, weeksapplicationdata.Length - 1) + "]";
            var json = "[{\"name\":\"Weeks\",\"data\":[" + string.Join(",", cwdata.ToArray()) +
                        "]},{\"name\":\"FTPP\",\"data\":" + weeksapplicationdata + "}]";

            return new JsonResult { Data = json, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        // New report services that are triggered from excel for Joel

        public JsonResult GetReleaseData(string project, string release)
        {
            ProjectExecutionRelease projectExecutionRelease = new ProjectExecutionRelease(project, release);
            var obj = Newtonsoft.Json.JsonConvert.SerializeObject(projectExecutionRelease.GetReleaseDetails(release));
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public string TemplateProjects()
        {
            return JsonConvert.SerializeObject(new Pillar().GetTemplateProjects());
        }

        public string ProjectReleases(string project)
        {
            if(project.Contains(","))
            {
                List<string> releases = new List<string>();
                string[] projects = project.Split(',');
                Pillar pillar = new Pillar();
                foreach(var proj in projects)
                {
                    List<string> currentReleases =  pillar.GetActiveReleases(proj);
                    foreach(var rel in currentReleases)
                    {
                        if(!releases.Contains(rel))
                        {
                            releases.Add(rel);
                        }
                    }
                }

                return string.Join(",", releases);
            }
            else
            {
                return string.Join(",", new Pillar().GetActiveReleases(project));
            }
        }

        public string ApplicationFTP(string project, string release)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationFTP(project, release));
        }

        public string ApplicationRuns(string project, string release)   
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationTotalRuns(project, release));
        }

        public string ApplicationInstances(string project, string release)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationTotalInstances(project, release));
        }

        // Defects

        public string ApplicationTotalDefects(string project, string release, string application)   
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationTotalDefects(project, release, application));
        }

        public string ApplicationTotalOpenCriticalDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationTotalOpenCriticalDefects(project, release, application));
        }

        public string ApplicationTotalAvgAgingOpenCriticalDefects(string project, string release, string application)   
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationTotalAvgAgingOpenCriticalDefects(project, release, application));
        }

        public string ApplicationTotalOpenMajorDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationTotalOpenMajorDefects(project, release, application));
        }

        public string ApplicationTotalAvgAgingOpenMajorDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationTotalAvgAgingOpenMajorDefects(project, release, application));
        }

        public string ApplicationTotalOpenMinorDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationTotalOpenMinorDefects(project, release, application));
        }

        public string ApplicationTotalAvgAgingOpenMinorDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationTotalAvgAgingOpenMinorDefects(project, release, application));
        }

        public string ApplicationTotalOpenTrivialDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationTotalOpenTrivialDefects(project, release, application));
        }

        public string ApplicationTotalAvgAgingOpenTrivialDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationTotalAvgAgingOpenTrivialDefects(project, release, application));
        }

        public string ApplicationNewDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationNewDefects(project, release, application));
        }

        public string ApplicationAssignedDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationAssignedDefects(project, release, application));
        }

        public string ApplicationReadyToDeployDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationReadyToDeployDefects(project, release, application));
        }

        public string ApplicationClosedDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationClosedDefects(project, release, application));
        }

        public string ApplicationAvgAgeOpenDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationAvgAgeOpenDefects(project, release, application));
        }

        public string ApplicationClosedAgingDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationClosedAgingDefects(project, release, application));
        }

        public string ApplicationCriticalClosedAgingDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationCriticalClosedAgingDefects(project, release, application));
        }

        public string ApplicationMajorClosedAgingDefects(string project, string release, string application)
        {
            return JsonConvert.SerializeObject(new Pillar().GetApplicationMajorClosedAgingDefects(project, release, application));
        }

        // main services using post

        [HttpPost]
        public void ProjectReleasesByDateRange(string project, string start, string end)
        {
            // Implementation
        }

        [HttpPost]
        public void ProjectReleaseApplications(string project, string start, string end)
        {
            // Implementation
        }

        // first time pass by project and time range
        [HttpPost]
        public string FTPProject(string project, string start, string end)   
        {
            return JsonConvert.SerializeObject(new Pillar().GetFTPProject(project, start, end));
        }

        // first time pass by project and time range - by release, application
        [HttpPost]
        public string FTPProjectEx(string project, string start, string end)
        {
            return JsonConvert.SerializeObject(new Pillar().GetFTPProjectEx(project, start, end));
        }

        // first time pass by project and time range - by release, application
        [HttpPost]
        public string FTPProjectNoCycle(string project, string start, string end)
        {
            return JsonConvert.SerializeObject(new Pillar().GetFTPProjectNoCycle(project, start, end));
        }

        // total runs project and time range
        [HttpPost]
        public string TotalRunProject(string project, string start, string end)
        {
            return JsonConvert.SerializeObject(new Pillar().GetTotalRunProject(project, start, end));
        }

        // total runs project and time range - by release, application
        [HttpPost]
        public string TotalRunProjectEx(string project, string start, string end)
        {
            return JsonConvert.SerializeObject(new Pillar().GetTotalRunProjectEx(project, start, end));
        }

        // total runs project and time range - by release, application
        [HttpPost]
        public string TotalRunProjectNoCycle(string project, string start, string end)
        {
            return JsonConvert.SerializeObject(new Pillar().GetTotalRunProjectNoCycle(project, start, end));
        }

        // Audit

        public ActionResult Audit()
        {
            return View();
        }

        public ActionResult AuditProject()
        {
            return View();
        }

        [HttpPost]
        public string TestDesignSteps(string project, string startdate, string enddate)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetTestSteps(project));
        }

        [HttpPost]
        public string TestDuplicate(string project, string startdate, string enddate) 
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetTestDuplicate(project));
        }

        [HttpPost]  
        public string TestReqLinked(string project, string startdate, string enddate)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetTestReqLinked(project));
        }

        [HttpPost]
        public string CyclesAssociated(string project, string startdate, string enddate)  
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetAssociatedCycles(project));
        }

        [HttpPost]
        public string ReqCoverage(string project, string startdate, string enddate)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetReqCoverage(project));
        }

        [HttpPost]
        public string ReqSourceId(string project, string startdate, string enddate)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetReqSourceId(project));
        }

        [HttpPost]
        public string ReqCycle(string project, string startdate, string enddate)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetReqCycle(project));
        }

        [HttpPost]
        public string DefectTestLinked(string project, string startdate, string enddate)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetDefectInstanceLink(project));
        }

        [HttpPost]
        public string DefectSWAId(string project, string startdate, string enddate)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetDefectSWAId(project));
        }

        [HttpPost]
        public string DefectPtr(string project, string startdate, string enddate)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetDefectPtr(project));
        }

        [HttpPost]
        public string TestInstancePtr(string project, string startdate, string enddate) 
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetTestInstancePtr(project));
        }

        [HttpPost]
        public string CycleTestInstancePtr(string project, string cycle)
        {
            return JsonConvert.SerializeObject(new AuditMetricsCycle(cycle).GetTestInstancePtr(project));
        }

        [HttpPost]
        public string TestSetCycle(string project, string startdate, string enddate)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetTestSetCycle(project));
        }

        [HttpPost]
        public string TestSetNotCompleted(string project, string startdate, string enddate)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetTestSetNotCompleted(project));
        }

        [HttpPost]
        public string CycleTestSetNotCompleted(string project, string cycle)
        {
            return JsonConvert.SerializeObject(new AuditMetricsCycle(cycle).GetTestSetNotCompleted(project));
        }

        [HttpPost]
        public string TestInstanceLinked(string project, string startdate, string enddate)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetTestInstanceLinked(project));
        }

        [HttpPost]
        public string CycleTestInstanceLinked(string project, string cycle)
        {
            return JsonConvert.SerializeObject(new AuditMetricsCycle(cycle).GetTestInstanceLinked(project));
        }

        [HttpPost]
        public string RunFastRun(string project, string startdate, string enddate)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetRunFastRun(project));
        }

        [HttpPost]
        public string CycleRunFastRun(string project, string cycle)
        {
            return JsonConvert.SerializeObject(new AuditMetricsCycle(cycle).GetRunFastRun(project));
        }

        [HttpPost]
        public string ExcelReport(string project, string startdate, string enddate)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetExcelReport(project));
        }
        
        // Get missing data details
        [HttpPost]
        public string MissingData(string project, string startdate, string enddate, string metric)
        {
            return JsonConvert.SerializeObject(new AuditMetrics(startdate, enddate).GetMissingData(project, metric));
        }

        [HttpPost]
        public string CycleMissingData(string project, string cycle, string metric)
        {
            return JsonConvert.SerializeObject(new AuditMetricsCycle(cycle).GetMissingData(project, metric));
        }

        // Get test instance status details
        [HttpPost]
        public string CycleTestInstanceStatus(string project, string cycle)
        {
            return JsonConvert.SerializeObject(new AuditMetricsCycle(cycle).GetTestInstanceStatus(project));
        }

        // Get latest run status if test in give cycles - Richard request
        [HttpPost]
        public string LatestTestStatusByCycle(string cycles)
        {
            return JsonConvert.SerializeObject(new AuditMetrics("1/1/2019", "1/1/2019").GetLatestTestStatusByCycle(cycles));
        }

    }   

    public class FirstTimePassed
    {
        public string Name { get; set; }
        public List<object> Data { get; set; }
    }

    public class ReleaseCycles
    {
        public string id { get; set; }
        public string text { get; set; }
    }

    public class SyncData
    {
        public string linkid { get; set; }
        public string ep1id { get; set; }
        public string ep2id { get; set; }
    }
}