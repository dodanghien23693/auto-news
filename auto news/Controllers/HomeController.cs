using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace auto_news.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Thread.CurrentPrincipal = User;
            return View();

        }
    }
}
