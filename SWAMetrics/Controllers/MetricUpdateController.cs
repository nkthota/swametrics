using Itenso.TimePeriod;
using SWAMetrics.Models;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using SWAMetrics.Lib;

namespace SWAMetrics.Controllers
{
    public class MetricUpdateController : Controller
    {
        private readonly ls_dashboardEntities _db = new ls_dashboardEntities();

        //public ViewResult UpdateWeeklyMetrics(int? id)
        //{
        //    //DateTime start = new DateTime(DateTime.Today.Year, 1, 1);
        //    //DateTime start = new DateTime(DateTime.Today.Year, 6, 1);
        //    //DateTime end = start.AddYears(1);
        //    DateTime start = DateTime.Now.AddDays(Convert.ToDouble(-id));
        //    DateTime end = DateTime.Now;
        //    Week week = new Week(start);
        //    while (week.Start < end)
        //    {
        //        foreach (var project in _db.Projects.ToList())
        //        {
        //            WeeklyExecutionDetails weeklyUpdates =
        //                new WeeklyExecutionDetails(project.ProjectName, project.DatabaseName)
        //                {
        //                    WeekStartDate = week.Start.ToShortDateString(),
        //                    WeekEndDate = week.End.ToShortDateString(),
        //                    CalenderWeek = week.WeekOfYear,
        //                };
        //            weeklyUpdates.UpdatedByApplciation();        
        //            weeklyUpdates.DefectDetails();                                        
        //        }
        //        week = week.GetNextWeek();
        //    }

        //    _db.DataRefreshes.Add(new DataRefresh
        //    {
        //        LastUpdated = DateTime.Now.ToString(CultureInfo.CurrentCulture)
        //    });
        //    _db.SaveChanges();

        //    return View("Update");
        //}

        public ViewResult UpdateWeeklyMetricsProgramInit(int? id)
        {
            //DateTime start = new DateTime(DateTime.Today.Year, 1, 1);
            //DateTime start = new DateTime(DateTime.Today.Year, 6, 1);
            //DateTime end = start.AddYears(1);
            DateTime start = DateTime.Now.AddDays(Convert.ToDouble(-id));
            DateTime end = DateTime.Now;
            Week week = new Week(start);
            while (week.Start < end)
            {
                foreach (var project in _db.Projects.ToList())
                {
                    WeeklyExecutionDetailsProgramInit weeklyUpdates =
                        new WeeklyExecutionDetailsProgramInit(project.ProjectName, project.DatabaseName)
                        {
                            WeekStartDate = week.Start.ToShortDateString(),
                            WeekEndDate = week.End.ToShortDateString(),
                            CalenderWeek = week.WeekOfYear,
                        };
                    weeklyUpdates.UpdatedByApplciationProgramInit();
                    weeklyUpdates.DefectDetails();
                }
                week = week.GetNextWeek();
            }

            _db.DataRefreshes.Add(new DataRefresh
            {
                LastUpdated = DateTime.Now.ToString(CultureInfo.CurrentCulture)
            }); 
            _db.SaveChanges();

            return View("Update");
        }

        public ViewResult UpdateMonthlyMetrics(int? id)
        {
            //DateTime start = new DateTime(DateTime.Today.Year, 1, 1);
            //DateTime start = new DateTime(DateTime.Today.Year, 6, 1);
            //DateTime end = start.AddYears(1);
            //DateTime start = DateTime.Now.AddDays(Convert.ToDouble(-id));
            DateTime start = DateTime.Now.AddYears(-1);
            DateTime end = DateTime.Now;
            Month month = new Month(start);
            while (month.Start < end)
            {
                foreach (var project in _db.Projects.ToList())
                {
                    MonthlyProjectExecutionDetails monthlyUpdates =
                        new MonthlyProjectExecutionDetails(project.ProjectName, project.DatabaseName)
                        {
                            MonthStartDate = month.Start.ToShortDateString(),
                            MonthEndDate = month.End.ToShortDateString(),
                            MonthYear = month.MonthName + "-" + month.Year
                        };
                    monthlyUpdates.UpdateProjectMonthlyMetrics();
                }
                month = month.GetNextMonth();
            }

            //_db.DataRefreshes.Add(new DataRefresh
            //{
            //    LastUpdated = DateTime.Now.ToString(CultureInfo.CurrentCulture)
            //});
            //_db.SaveChanges();

            return View("Update");
        }   

        public ViewResult UpdateReleaseMetrics(int? id)
        {
            foreach (var project in _db.Projects.ToList())
            {
                ProjectExecutionCycle monthlyUpdates =
                    new ProjectExecutionCycle(project.ProjectName, project.DatabaseName)
                    {
                    };
                monthlyUpdates.GetProjectCycles();
            }
            new ProjectExecutionCycle().UpdateNumbers();
            return View("Update");
        }
    }

    public class ExecutionDetails
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

}