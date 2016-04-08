using auto_news.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace auto_news.Controllers
{
    [Authorize]
    public class CrawlerController : Controller
    {
        private AutoNewsDbContext _db = new AutoNewsDbContext();
        public ActionResult Index()
        {       
            return View("SourcesConfig");
        }


        public ActionResult GetAllSources()
        {
            
            var sources = _db.NewsSources.Select(i => i).ToList();

            return Json(new { data = sources },JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetAllNewsSource()
        {
            try
            {
                var sources = _db.NewsSources.Select(i => i).ToList();
                return Json(new { Result = "OK", Records = sources });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult CreateNewsSource(NewsSource source)
        {
            try
            {
                
                NewsSource s = _db.NewsSources.Add(source);
                _db.SaveChanges();
                return Json(new { Result = "OK", Record = s });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }

        }

        [HttpPost]
        public ActionResult UpdateNewsSource(NewsSource source)
        {
            try
            {
                _db.Entry(source).State = EntityState.Modified;
                _db.SaveChanges();
                
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteNewsSource(int Id)
        {
            try
            {
                _db.NewsSources.Remove(_db.NewsSources.Find(Id));
                _db.SaveChanges();
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }


        public ActionResult CrawlPageConfig()
        {
            return View();
        }

    }
}
