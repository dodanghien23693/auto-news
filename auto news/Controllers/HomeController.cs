using auto_news.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace auto_news.Controllers
{
    public class HomeController : Controller
    {
        private AutoNewsDbContext _db = new AutoNewsDbContext();
        public ActionResult Index()
        {
            return View();
        }

        [Route("detailArticle/{id:int}")]
        public ActionResult DetailArticle(int id)
        {
            var article = _db.Articles.Find(id);
            if (article != null)
            {
                return View(article);
            }
            else return HttpNotFound();
        }
    }
}
