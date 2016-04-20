using auto_news.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Filters;
using System.Linq.Dynamic;
using Microsoft.AspNet.Identity;
using System.Threading;
using Microsoft.AspNet.Identity.Owin;

namespace auto_news.ApiServices
{
    [RoutePrefix("api")]
    [AllowAnonymous]
    public class AutoNewsApiController : ApiController
    {

        private AutoNewsDbContext _db = new AutoNewsDbContext();
       

        #region Articles Api

        /// <summary>
        /// get Article by id
        /// </summary>
        /// <param name="id">Article id to get</param>
        /// <response code="200">successful operation</response>
        /// <response code="400">Invalid ID supplied</response>
        /// <response code="404">Article not found</response>
        //GET api/articles/id
        [Route("articles/{id:int}")]
        public IHttpActionResult GetArticleById(int id)
        {
            var a = _db.Articles.Find(id);
            if (a == null) return NotFound();
            else return Ok(a);
        }

        [Route("articles",Name = "articles")]
        public IHttpActionResult GetArticles([FromUri] ArticleQuery query)
        {


            if (query == null) query = new ArticleQuery();

            IQueryable<Article> articles;
            

            articles = GetFilteredArticles(query);
            
            
            
            if (query.order == "desc")
            {
                articles = articles.OrderBy(query.sortBy + " descending");
                //from a in articles orderby(query.sortBy) descending select a;
            }
            else
            {
                articles = articles.OrderBy(query.sortBy);
                
            }

            if (query.searchString != null)
            {
                articles = articles.Where(i => i.Title.ToLower().Contains(query.searchString.ToLower()));                
            }

            if (query.ids != null)
            {
                var listId = Array.ConvertAll(query.ids.Split(','), int.Parse);
                articles = from a in articles where listId.Contains(a.Id) select a;

            }
            
            
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                var config = _db.UserSourceConfigs.Where(i => i.UserId == userId).FirstOrDefault();
                if (config == null)
                {
                    config = CreateDefaultUserSourceConfig(User.Identity.GetUserId());

                }

                var sources = JsonConvert.DeserializeObject<UserSourceConfigObject>(config.ObjectConfig);

                var sourceIds = sources.Sources.Select(i => i.SourceId).ToList();
                articles = articles.Where(i => sourceIds.Contains(i.NewsSourceId));
                //foreach(var source in sources.Sources)
                //{

                //}
            }

            articles = articles.Skip(query.limit * (query.page-1)).Take(query.limit);

            if (query.fields != null)
            {
                var listField = query.fields.Split(',');

                //return Ok(articles.Select((query.fields));
                return Ok(articles.ToList().Select((i => GetFilteredArticle(i, listField))));

            }

            return Ok(articles.ToList());

        }

        [Route("articles/count")]
        public IHttpActionResult GetCountArticles([FromUri] ArticleQuery query)
        {
            if (query == null) query = new ArticleQuery();
            var articles = GetFilteredArticles(query);

            //if (query.ids != null)
            //{
            //    var listId = Array.ConvertAll(query.ids.Split(','), int.Parse);
            //    articles = from a in articles where listId.Contains(a.Id) select a;

            //}

            return Ok(articles.Count());

        }

        #endregion Articles Api

        #region Categories Api
        [Route("categories")]
        public IHttpActionResult GetCategories()
        {
            var result = _db.Categories.Select(i => new { i.Id, i.Name, i.Description });
            if (result == null) return NotFound();
            else return Ok(result);
        }

        [Route("categories/count")]
        public IHttpActionResult GetCountCategories()
        {
            return Ok(_db.Categories.Count());

        }


        [Route("categories/{id:int}")]
        public IHttpActionResult GetCategoryById(int id)
        {
            var result = _db.Categories.Where(i => i.Id == id).Select(i => new { i.Id, i.Name, i.Description }).FirstOrDefault();
            if (result == null) return NotFound();
            else return Ok(result);
        }

        [Route("categories/{categoryId:int}/articles")]
        public IHttpActionResult GetArticlesByCategory(int categoryId, [FromUri] ArticleQuery query)
        {
            if (query == null) query = new ArticleQuery();
            query.categoryId = categoryId;
            return GetArticles(query);
        }

        [Route("categories/{categoryId:int}/articles/count")]
        public IHttpActionResult GetCountArticlesByCategory(int categoryId, [FromUri] ArticleQuery query)
        {
            if (query == null) query = new ArticleQuery();
            query.categoryId = categoryId;
            return GetCountArticles(query);
        }

        #endregion Categories Api

        #region NewsSources Api

        [Route("sources")]
        public IHttpActionResult GetSources()
        {
            var result = _db.NewsSources.Select(i => new { i.Id, i.Name, i.Description });
            if (result == null) return NotFound();
            else return Ok(result);
        }

        [Route("sources/count")]
        public IHttpActionResult GetCountSources()
        {
            return Ok(_db.NewsSources.Count());
        }

        [Route("sources/{id:int}")]
        public IHttpActionResult GetSourceById(int id)
        {
            var result = _db.NewsSources.Where(i => i.Id == id).Select(i => new { i.Id, i.Name, i.Description }).FirstOrDefault();
            if (result == null) return NotFound();
            else return Ok(result);
        }

        [Route("sources/{sourceId:int}/articles")]
        public IHttpActionResult GetArticlesBySource(int sourceId, [FromUri] ArticleQuery query)
        {
            if (query == null) query = new ArticleQuery();
            query.sourceId = sourceId;
            return GetArticles(query);
        }

        [Route("sources/{sourceId:int}/articles/count")]
        public IHttpActionResult GetCountArticlesBySource(int sourceId,[FromUri] ArticleQuery query)
        {
            if (query == null) query = new ArticleQuery();
            query.sourceId = sourceId;
            return GetCountArticles(query);
        }

        [Authorize]
        [Route("sources/selected")]
        public IHttpActionResult GetSelectedSourcesByCurrentUser()
        {
            var userId = User.Identity.GetUserId();
            var config = _db.UserSourceConfigs.Where(i => i.UserId == userId).FirstOrDefault();
            if (config == null)
            {
                config = CreateDefaultUserSourceConfig(userId);
            }

            var sources = JsonConvert.DeserializeObject<UserSourceConfigObject>(config.ObjectConfig);
            if (sources != null)
            {
                int[] ids = sources.Sources.Select(i => i.SourceId).ToArray();
                return Ok(_db.NewsSources.Where(i => ids.Contains(i.Id)).ToList());
            }
            
            
            return Ok(new { });
        }

        [Authorize]
        [HttpPost]
        [Route("newsSourceConfig/Add/{sourceId:int}")]
        public IHttpActionResult AddFavoriteSource(int sourceId)
        {
            var userId = User.Identity.GetUserId();
            var config = _db.UserSourceConfigs.Where(i => i.UserId == userId).FirstOrDefault();
            if (config != null)
            {
                var sources = JsonConvert.DeserializeObject<UserSourceConfigObject>(config.ObjectConfig);
                if (sources != null)
                {
                    if (sources.Sources.Where(i => i.SourceId == sourceId).FirstOrDefault() == null)
                    {
                        sources.Sources.Add(new SourceFrequencyConfig() { SourceId = sourceId });

                        config.ObjectConfig = JsonConvert.SerializeObject(sources);
                        _db.SaveChanges();
                        return Ok();
                    }
                }
            }
            return NotFound();
        }

        [Authorize]
        [HttpPost]
        [Route("newsSourceConfig/Remove/{sourceId:int}")]
        public IHttpActionResult RemoveFavoriteSource(int sourceId)
        {
            var userId = User.Identity.GetUserId();
            var config = _db.UserSourceConfigs.Where(i => i.UserId == userId).FirstOrDefault();
            if (config != null)
            {
                var sources = JsonConvert.DeserializeObject<UserSourceConfigObject>(config.ObjectConfig);
                if (sources != null)
                {
                    var s = sources.Sources.Where(i => i.SourceId == sourceId).FirstOrDefault();
                    if (s != null)
                    {
                        sources.Sources.Remove(s);
                        config.ObjectConfig = JsonConvert.SerializeObject(sources);
                        _db.SaveChanges();
                        return Ok();
                    }   
                }
            }
            return NotFound();
        }

        #endregion NewsSources Api



        private IQueryable<Article> GetFilteredArticles(ArticleQuery query)
        {
            var articles = _db.Articles.Select(i => i);
            if (query.sourceId != null)
            {
                articles = articles.Where(i => i.NewsSourceId == query.sourceId);
            }
            if (query.categoryId != null)
            {
                articles = articles.Where(i => i.CategoryId == query.categoryId);
            }
            if (query.fromDateTime != null)
            {
                articles = articles.Where(i => i.CreateTime >= query.fromDateTime);
            }
            if (query.toDateTime != null)
            {
                articles = articles.Where(i => i.CreateTime <= query.toDateTime);
            }
            return articles;
        }

        private UserSourceConfig CreateDefaultUserSourceConfig(string userId)
        {
            UserSourceConfig config = new UserSourceConfig();

            config.UserId = userId;
            var defaultConfig = _db.AutonewsConfigs.Where(i => i.KeyName == "DefaultUserSourceConfig").FirstOrDefault();
            if (defaultConfig != null)
            {
                config.ObjectConfig = defaultConfig.Value;
            }
            _db.UserSourceConfigs.Add(config);
            _db.SaveChanges();
            return config;
        }


        private IQueryable<Article> GetFilteredArticlesByCurrentUser(ArticleQuery query)
        {
            var config = _db.UserSourceConfigs.Where(i => i.UserId == User.Identity.GetUserId()).FirstOrDefault();
            if (config == null)
            {
                config = CreateDefaultUserSourceConfig(User.Identity.GetUserId());
            }

            var articles = _db.Articles.Select(i => i);
            if (query.sourceId != null)
            {
                articles = articles.Where(i => i.NewsSourceId == query.sourceId);
            }
            if (query.categoryId != null)
            {
                articles = articles.Where(i => i.CategoryId == query.categoryId);
            }
            if (query.fromDateTime != null)
            {
                articles = articles.Where(i => i.CreateTime >= query.fromDateTime);
            }
            if (query.toDateTime != null)
            {
                articles = articles.Where(i => i.CreateTime <= query.toDateTime);
            }
            return articles;
        }

        
        public static object GetFilteredObject(string[] fields)
        {
            dynamic result = new ExpandoObject();
            var article = result as IDictionary<String, object>;
            foreach (var f in fields)
            {
                article[f] = null;
            }
            return result;
        }

        public static object GetFilteredArticle(Article a,string[] fields)
        {
            dynamic result  = new ExpandoObject();
            var article = result as IDictionary<String, object>;
            foreach(var f in fields)
            {
                var field = Char.ToUpper(f[0]) + f.Substring(1,f.Length-1);
                var prop = typeof(Article).GetProperty(field);
                if (prop != null)
                {
                    article[field] = prop.GetValue(a, null);
                }
            }
            return result;
        }

    }


    public class ArticleQuery
    {
        //public ArticleQuery()
        //{
        //    sortBy = "Id";
        //    order = "desc";
        //    limit = 10;
        //    page = 1;
        //}
        public string ids { get; set; }
        public int? categoryId { get; set; }
        public int? sourceId { get; set; }
        public string fields { get; set; }
        public DateTime? fromDateTime { get; set; }
        public DateTime? toDateTime { get; set; }
        public string sortBy { get; set; } = "Id";
        public string order { get; set; } = "desc";

        public string searchString { get; set; }
        public int limit { get; set; } = 10;

        public int page { get; set; } = 1;
    }

    public class UserSourceConfigObject
    {
        public List<SourceFrequencyConfig> Sources { get; set; }
    }

    public class SourceFrequencyConfig
    {
        public int SourceId { get; set; }
        public int Frequency { get; set; }
    }

}