using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MyApp.MVC.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
          public ActionResult Notification()
        {
            return View();
        }
        public ActionResult GetPendingUser()
        {
            return View();
        }

         public ActionResult GetDocuments()
        {
            return View();
        }
    }
}