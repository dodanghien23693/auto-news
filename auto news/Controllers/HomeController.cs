using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace auto_news.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //var ran = new Random();

            //if(Session["loginId"]== null)
            //{
            //    Session["loginId"] = "hien";
            //}
            
            return View();
        }
    }
}
