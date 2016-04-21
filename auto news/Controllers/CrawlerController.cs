using auto_news.Models;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace auto_news.Controllers
{
    [Authorize(Roles = "admin")]
    public class CrawlerController : Controller
    {
        private AutoNewsDbContext _db = new AutoNewsDbContext();
        public ActionResult Index()
        {   
            return View("SourcesConfig");
        }


        #region News Sources CRUD

        [HttpPost]
        public JsonResult GetAllNewsSource()
        {
            try
            {
                var sources = _db.NewsSources.Select(i => new { Id = i.Id, Name = i.Name, Description = i.Description }).ToList();
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
        #endregion News Sources

        #region Category CRUD

        public JsonResult GetAllCategory()
        {
            try
            {
                var sources = _db.Categories.Select(i => new { Id = i.Id, Name = i.Name, Description = i.Description }).ToList();
                return Json(new { Result = "OK", Records = sources }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult CreateCategory(Category category)
        {
            try
            {

                Category s = _db.Categories.Add(category);
                _db.SaveChanges();
                return Json(new { Result = "OK", Record = s });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }

        }

        [HttpPost]
        public ActionResult UpdateCategory(Category category)
        {
            try
            {
                _db.Entry(category).State = EntityState.Modified;
                _db.SaveChanges();

                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteCategory(int Id)
        {
            try
            {
                _db.Categories.Remove(_db.Categories.Find(Id));
                _db.SaveChanges();
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
        #endregion Category CRUD


        #region CrawlConfig CRUD
        [HttpPost]
        public JsonResult CrawlConfigList(int sourceId)
        {
            try
            {
                var crawlConfigs = _db.CrawlConfigs.Where(c=>c.NewsSourceId==sourceId);
                return Json(new { Result = "OK", Records = crawlConfigs });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteCrawlConfig(int id)
        {
            try
            {
                _db.CrawlConfigs.Remove(_db.CrawlConfigs.Find(id));
                _db.SaveChanges();
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult UpdateCrawlConfig(CrawlConfig crawlConfig,string type)
        {
            try
            {
                _db.Entry(crawlConfig).State = EntityState.Modified;
                _db.SaveChanges();
                if (type == "update")
                {
                    return Redirect("/Crawler/PageCrawlConfig?type=update&id=" + crawlConfig.Id);

                }
                else return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult CreateCrawlConfig(CrawlConfig crawlConfig,string type)
        {
            try
            {
                var c = _db.CrawlConfigs.Add(crawlConfig);
                _db.SaveChanges();
                if (type == "add")
                {
                    return Redirect("/Crawler/PageCrawlConfig?type=update&id=" + c.Id);
                }
                else return Json(new { Result = "OK", Record = c });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        #endregion CrawlConfig CRUD
        public ActionResult PageCrawlConfig(int? id,string type)
        {    
            try
            {
                ViewBag.Type = type;

                var categories = _db.Categories.Select(i => i).OrderBy(i => i.Id);
                ViewBag.CategoriesId = JsonConvert.SerializeObject(categories.Select(i => i.Id).ToArray());
                ViewBag.CategoriesName = JsonConvert.SerializeObject(categories.Select(i => i.Name).ToArray());

                var newsSources = _db.NewsSources.Select(i => i).OrderBy(i => i.Id);

                ViewBag.NewsSourceIds = JsonConvert.SerializeObject(newsSources.Select(i => i.Id).ToArray());
                ViewBag.NewsSourceNames = JsonConvert.SerializeObject(newsSources.Select(i => i.Name).ToArray());

                if (type == "add")
                {
                    return View(new CrawlConfig());
                }
                else if(type=="update")
                {
                    var config = _db.CrawlConfigs.Find(id);
                    if (config != null)
                    {
                        return View(config);
                    }
                }
                return HttpNotFound();                
            }
            catch(Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
            
        }

        public ActionResult CategoriesConfig()
        {
            return View();
        }
     
        public ActionResult ScheduleJob()
        {
            MvcApplication.AutoNewsServiceInstance.ScheduleJob();
            return Json(new { status = "Ok" }, JsonRequestBehavior.AllowGet);   
        }

    }
}
