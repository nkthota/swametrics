﻿using Itenso.TimePeriod;
using SWAMetrics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

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

        public ActionResult CycleFirstTimePass()
        {
            return View();
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
                        weeksdata = weeksdata + mMatches[0].PercentFirstTimeInstancesPassed.ToString() + ",";
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
                        weeksdata = weeksdata + mMatches[0].PercentFirstTimeInstancesFailed.ToString() + ",";
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
}