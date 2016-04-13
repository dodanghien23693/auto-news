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

namespace auto_news.ApiServices
{
    [RoutePrefix("api")]
    public class ArticlesController : ApiController
    {
        /// <summary>
        /// get Article by id
        /// </summary>

        /// <param name="id">Article id to get</param>
        /// <response code="200">successful operation</response>
        /// <response code="400">Invalid ID supplied</response>
        /// <response code="404">Article not found</response>

        private AutoNewsDbContext _db = new AutoNewsDbContext();

        //GET api/articles/id
        [Route("articles/{id}")]
        public async Task<IHttpActionResult> GetArticleById(int id)
        {
            var a = _db.Articles.Find(id);
            if (a == null) return NotFound();
            else return Ok(a);

        }

        //string ids,int? categoryId,int? sourceId,string fields, DateTime? fromDateTime,DateTime? toDateTime, string sortBy,string order = "asc", int limit = 10,int page = 1

        [Route("articles")]
        public IHttpActionResult GetArticles([FromUri] ArticleQuery query)
        {
            if (query == null) query = new ArticleQuery();
            var articles = GetFilteredArticles(query);
            
            if (query.order == "desc")
            {
                articles = articles.OrderBy(query.sortBy + " descending");
                //from a in articles orderby(query.sortBy) descending select a;
            }
            else
            {
                articles = articles.OrderBy(query.sortBy);
                
            }

            
            if (query.ids != null)
            {
                var listId = Array.ConvertAll(query.ids.Split(','), int.Parse);
                articles = from a in articles where listId.Contains(a.Id) select a;

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
        public int limit { get; set; } = 10;

        public int page { get; set; } = 1;
    }
}