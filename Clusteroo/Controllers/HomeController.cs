﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concordia.Spimi;
using Clusteroo.Models;

namespace Clusteroo.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to Clusteroo!";

            if (Session["spimi"] == null)
            {
                Spimi spimi = new Spimi();
                spimi.Index();
                Session["spimi"] = spimi;
            }

            return View();
        }
        
        public JsonResult Query(string query)
        {
            Spimi spimi = (Spimi)Session["spimi"];

            List<QueryResult> results = spimi.Query(query).ToList();
            JsonResult res = Json(results, JsonRequestBehavior.AllowGet);
            return res;
        }

        public JsonResult Cluster(int k)
        {
            Spimi spimi = (Spimi)Session["spimi"];

            List<ClusterResult> results = spimi.Cluster(k).ToList();
            JsonResult res = Json(results, JsonRequestBehavior.AllowGet);
            return res;
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
