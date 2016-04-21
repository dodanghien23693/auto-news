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

    /// <summary>
    /// Quản lý comments và ratings
    /// </summary>
    [RoutePrefix("api")]
    [AllowAnonymous]
    public class CommentRateApiController : ApiController
    {

        private AutoNewsDbContext _db = new AutoNewsDbContext();


        /// <summary>
        /// Lấy danh sách các comment của một article(dựa vào articleId)
        /// </summary>
        [Route("articles/{articleId:int}/comments")]
        [HttpGet]
        public HttpResponseMessage GetCommentsByArticleId(int articleId)
        {
            //var comments = _db.Comments.Join(Where(i => i.ArticleId == articleId).ToList();
            var userId = User.Identity.GetUserId();

            var comments = from c in _db.Comments
                           from u in _db.AspNetUsers
                           where c.UserId == u.Id
                           where c.ArticleId == articleId
                           select new { c.Id,UserId=u.UserName, Content = c.Content, c.CreateDateTime, c.ModifyDateTime, u.Email,CreateByCurrentUser= (userId==u.Id) };

            return Request.CreateResponse(HttpStatusCode.OK, comments);
            
        }

        /// <summary>
        /// Lấy thông tin comment(dựa vào thuộc tính id)
        /// </summary>
        [Route("comments/{id}")]
        [HttpGet]
        public HttpResponseMessage GetCommentById(int id)
        {
            var comment = _db.Comments.Find(id);
            comment.UserId = User.Identity.GetUserName();
            return Request.CreateResponse(HttpStatusCode.OK, comment);
        }

        /// <summary>
        /// Tạo mới comment
        /// </summary>
        [Route("comments")]
        [HttpPost]
        public HttpResponseMessage AddComment(Comment comment)
        {
            if (_db.Articles.Find(comment.ArticleId) != null)
            {
                comment.UserId = User.Identity.GetUserId();
                comment.CreateDateTime = DateTime.UtcNow;
                _db.Comments.Add(comment);
                _db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK,comment);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, new HttpError("Không tìm thấy bài báo tương ứng"));
            }
        }

        /// <summary>
        /// Cập nhật comment
        /// </summary>
        [Route("comments/{id:int}")]
        [HttpPut]
        public HttpResponseMessage EditComment(int id,Comment comment)
        {
            var c = _db.Comments.Find(id);
            if (c != null)
            {
                c.Content = comment.Content;
                c.ModifyDateTime = DateTime.UtcNow;
                _db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK,comment);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, new HttpError("Không tìm thấy comment tương ứng"));
            }
        }

        /// <summary>
        /// Xóa comment
        /// </summary>
        [Route("comments/{id:int}")]
        [HttpDelete]
        public HttpResponseMessage DeleteComment(int id)
        {
            var c = _db.Comments.Find(id);
            if (c != null)
            {
                _db.Comments.Remove(c);
                _db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, new HttpError("Không tìm thấy comment tương ứng"));
            }
        }


        /// <summary>
        /// Lấy thông tin Rating của article(dựa trên articleId) của người dùng hiện tại
        /// </summary>
        [Route("articles/{articleId:int}/ratings")]
        [HttpGet]
        public HttpResponseMessage GetRatingByCurrentUser(int articleId)
        {
            var article = _db.Articles.Find(articleId);
            if (article != null)
            {
                var userId = User.Identity.GetUserId();
                var rate = _db.Ratings.Where(i => i.ArticleId == article.Id && i.UserId == userId).FirstOrDefault();
                if (rate == null)
                {
                    rate = new Rating();

                }
                return Request.CreateResponse(HttpStatusCode.OK, rate);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, new HttpError("Không tìm thấy article tương ứng"));
            }
        }


        /// <summary>
        /// Thêm mới hoặc cập nhật rating cho article(dựa vào articleId) của người dùng hiện tại
        /// </summary>
        [Authorize]
        [Route("articles/{articleId:int}/ratings")]
        [HttpPost]
        public HttpResponseMessage AddOrUpdateRating(Rating r)
        {
            var article = _db.Articles.Find(r.ArticleId);
            if (article != null)
            {
                var userId = User.Identity.GetUserId();
                var rate = _db.Ratings.Where(i => i.ArticleId == article.Id && i.UserId == userId).FirstOrDefault();
                if (rate != null)
                {
                    rate.RatePoint = r.RatePoint;

                }
                else
                {
                    _db.Ratings.Add(new Rating() { ArticleId = article.Id, RatePoint = r.RatePoint, UserId = User.Identity.GetUserId(), CreateDateTime = DateTime.UtcNow });
                }
                _db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK,rate);

            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, new HttpError("Không tìm thấy article tương ứng"));
            }
        }



    }



}