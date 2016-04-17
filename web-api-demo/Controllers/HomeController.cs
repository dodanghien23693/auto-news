using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace web_api_demo.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
