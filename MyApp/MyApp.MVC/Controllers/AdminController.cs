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
        public ActionResult Notification()
        {
            return View();
        }

        public ActionResult GetDocuments()
        {
            return View();
        }

        public ActionResult GetPendingUser()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult TaskCrud()
        {
            return View();
        }
        public IActionResult UserList()
        {
            return View();
        }
        public IActionResult Chat()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}