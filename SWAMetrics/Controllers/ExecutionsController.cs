using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SWAMetrics.Lib;

namespace SWAMetrics.Controllers
{
    public class ExecutionsController : Controller
    {
        // GET: Executions
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetProjects()
        {
            List<ProjectsData> projects = new List<ProjectsData>();
            DataTable dt = new AuditMetrics("1/1/2019","1/1/2019").GetTemplateProjects();
            foreach (DataRow dataRow in dt.Rows)
            {
                projects.Add(new ProjectsData()
                {
                    id = dataRow["PROJECT_NAME"].ToString(),
                    text = dataRow["PROJECT_NAME"].ToString()
                });
            }

            return Json(projects, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetReleases(string project)
        {
            List<ReleasesData> projects = new List<ReleasesData>();
            DataTable dt = new AuditMetrics("1/1/2019", "1/1/2019").GetProjectReleases(project);
            foreach (DataRow dataRow in dt.Rows)
            {
                projects.Add(new ReleasesData()
                {
                    id = dataRow["REL_NAME"].ToString(),
                    text = dataRow["REL_NAME"].ToString()
                });
            }

            return Json(projects);
        }
    }

    public class ProjectsData
    {
        public string id { get; set; }
        public string text { get; set; }
    }

    public class ReleasesData
    {
        public string id { get; set; }
        public string text { get; set; }
    }

}