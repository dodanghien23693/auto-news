using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using auto_news;
using auto_news.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Config_Model;
using NLog;

namespace Crawl_News_Module
{
    public class CrawlerService
    {
        public static AutoNewsDbContext AutoNewsDb = new AutoNewsDbContext();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public void DoCrawl(CrawlConfigJson configObject)
        {
            if (configObject.Method.MethodId == CrawlMethodId.SingleUrl)
            {
                //dynamic description = JsonConvert.DeserializeObject(configObject.Description);
                string url = configObject.Method.Description.Url;
                var article = DoCrawlSingleUrl(url, configObject.CrawlPageConfig);

                if (article != null)
                {
                    AddArticle(article, configObject, new LinkConfig() { Url = url, CategoryId = configObject.CategoryId });
                }
            }
            else if (configObject.Method.MethodId == CrawlMethodId.MultiUrl)
            {
                JArray urls = configObject.Method.Description.Urls;

                for (var i = 0; i < urls.Count; i++)
                {
                    string url = urls[i].ToString();
                    if (IsCrawled(url, configObject)) break;
                    else
                    {
                        var article = DoCrawlSingleUrl(url, configObject.CrawlPageConfig);
                        if (article != null)
                        {
                            AddArticle(article, configObject, new LinkConfig() { Url = url, CategoryId = configObject.CategoryId });
                        }

                    }
                }
                //var articles = Crawler.DoCrawlMultiUrl(urls.Select(i => (string)i).ToArray(), configObject.CrawlPageConfig);
            }
            else if (configObject.Method.MethodId == CrawlMethodId.GeneratedLinks)
            {
                ScheduleConfig schedule = configObject.ScheduleConfig;

                dynamic Params = configObject.Method.Description.Params;

                List<LinkConfig> links = new List<LinkConfig>();
                GenerateLinkFromParams(0, Params, new string[Params.Count], configObject.CategoryId, links);

                List<LinkConfig> crawledLinks = new List<LinkConfig>();

                var i = 0;


                while (i < links.Count)
                {
                    var HasNewArticle = true;
                    //var link = links[i];
                    List<LinkConfig> linksConfig = CrawlLinkFromUrl(links[i].Url, configObject.Method.Description.CrawlLinkConfig.ToObject<CrawlLinkConfig>(), links[i].CategoryId);
                    
                    var j = 0;
                    while (HasNewArticle && j < linksConfig.Count)
                    {
                        var link = linksConfig[j];
                        link.Url = configObject.Method.Description.BaseUrl + link.Url;
                        if (IsCrawled(link.Url, configObject))
                        {
                            HasNewArticle = false;
                        }
                        else
                        {
                           
                            var article = DoCrawlSingleUrl(link.Url, configObject.CrawlPageConfig);

                            if (article != null)
                            {
                                article.ImageUrl = link.ImageUrl;
                                if (article.ImageUrl.Trim() == "")
                                {
                                    article.ImageUrl = GetFirstImageUrl(article.Content);
                                }
                                AddArticle(article, configObject, link);
                            }
                            
                            
                        }
                        j++;
                    }
                    i++;

                    RemoveDuplicateArticle(configObject.NewsSourceId);
                }
            }
        }

        public void RemoveDuplicateArticle(int sourceId)
        {
            var dup = AutoNewsDb.Articles.Where(i => i.NewsSourceId == sourceId).Take(200).GroupBy(i => i.OriginUrl).Where(i => i.Count() > 1);
            foreach(var g in dup)
            {
                var list = g.ToList();
                for(var i = 1; i < list.Count;i++)
                {
                    AutoNewsDb.Articles.Remove(list[i]);
                }
                
            }
            AutoNewsDb.SaveChanges();
        }
        private string GetFirstImageUrl(string content)
        {
            string imageUrl = "";
            Regex regex = new Regex("<img [^>]*src=\"([^ \"]+)");
            Match match = regex.Match(content);
            if (match.Success)
            {
                var s = match.Value;
                imageUrl = s.Substring(s.IndexOf("src=\"") + 5);
            }
            return imageUrl;
        }

        
        public int AddArticle(CrawlArticle article, CrawlConfigJson config, LinkConfig link)
        {
            try
            {
                AutoNewsDb.Articles.Add(new Article()
                {
                    CrawlConfigId = config.Id,
                    NewsSourceId = config.NewsSourceId,
                    Title = article.Title,
                    Description = article.Description,
                    ImageUrl = article.ImageUrl,
                    RawContent = article.Content,
                    CategoryId = link.CategoryId,
                    OriginUrl = link.Url,
                    CreateTime = DateTime.UtcNow,

                    //Category = AutoNewsDb.Categories.FirstOrDefault(c => c.Id == link.CategoryId),
                    //NewsSource = AutoNewsDb.NewsSources.FirstOrDefault(n => n.Id == config.NewsSourceId)
                });

                return AutoNewsDb.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Trace(ex.StackTrace,"loi them CrawlArticle:{0}", article);
            }
            return 0;

        }
        private bool IsCrawled(string url, CrawlConfigJson configObject)
        {
            try
            {
                if (AutoNewsDb.Articles.Where(a => a.NewsSourceId == configObject.NewsSourceId).Where(a => a.OriginUrl.ToLower().Equals(url.ToLower())).Count()==0) return false;
                else return true;
            }
            catch(Exception ex)
            {
                logger.Trace(ex, "loi khi thuc hien IsCrawled({0},configObject)", url);
                throw;         
            }

        }

        public List<LinkConfig> CrawlLinkFromUrl(string url, CrawlLinkConfig crawlLinkConfig, int categoryId)
        {
            List<LinkConfig> listLinkConfig = new List<LinkConfig>();
            //CrawlLinkConfig crawlConfig = configObject.Method.Description.CrawlLinkConfig.ToObject<CrawlLinkConfig>();
            var parser = new HtmlParser();
            var html = GetHtmlFromUrl(url);
            if (html != null)
            {
                var document = parser.Parse(html);
                var links = document.QuerySelectorAll(crawlLinkConfig.LinkQuery);

                foreach (var link in links)
                {
                    var articleUrl = link.GetAttribute(crawlLinkConfig.Attribute);

                    string imageContainer = "";
                    if (crawlLinkConfig.ImageContainer != null) imageContainer = crawlLinkConfig.ImageContainer;
                    var imageUrl = "";
                    foreach (var query in imageContainer.Split(','))
                    {
                        var imageElement = document.QuerySelector(query + " a[href='" + articleUrl + "'] img");

                        if (imageElement != null)
                        {
                            imageUrl = imageElement.GetAttribute("src");
                            break;
                        }
                    }

                    listLinkConfig.Add(new LinkConfig() { ImageUrl = imageUrl, Url = articleUrl, CategoryId = categoryId });
                    //urls.Add(link.GetAttribute(crawlLinkConfig.Attribute));

                }

                return listLinkConfig.GroupBy(i => i.Url).Select(i => i.First()).ToList();
            }

            return listLinkConfig;
            
        }

        public void GenerateLinkFromParams(int i, dynamic pas, string[] Urls, int categoryId, List<LinkConfig> links)
        {
            if (i == pas.Count)
            {
                links.Add(new LinkConfig() { Url = String.Join("", Urls), CategoryId = categoryId });
                return;
            }

            var param = pas[i];

            if (param.Type == CrawlConfigParamType.Fix)
            {
                Urls[i] = param.Description.Value;
                GenerateLinkFromParams(i + 1, pas, Urls, categoryId, links);
            }
            else if (param.Type == CrawlConfigParamType.Range)
            {
                for (var j = (int)param.Description.From; j <= (int)param.Description.To; j++)
                {
                    Urls[i] = j.ToString();
                    GenerateLinkFromParams(i + 1, pas, Urls, categoryId, links);
                }
            }
            else if (param.Type == CrawlConfigParamType.ListValue)
            {
                var values = param.Description.Values.Split(",");
                for (var j = 0; j < values.Length; j++)
                {
                    Urls[i] = values[j];
                    GenerateLinkFromParams(i + 1, pas, Urls, categoryId, links);
                }
            }
            else if (param.Type == CrawlConfigParamType.Categories)
            {
                dynamic categories = param.Description.Categories;
                for (var j = 0; j < categories.Count; j++)
                {
                    Urls[i] = categories[j].Value;
                    GenerateLinkFromParams(i + 1, pas, Urls, (int)categories[j].CategoryId, links);
                }
            }

        }

        static Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);
        public string RemoveHtmlTag(string input)
        {
            return _htmlRegex.Replace(input, string.Empty);
        }

        public string TruncateAtWord(string value, int length)
        {
            if (value == null || value.Trim().Length <= length)
                return value;

            int index = value.Trim().LastIndexOf(" ");

            while ((index + 3) > length)
                index = value.Substring(0, index).Trim().LastIndexOf(" ");

            if (index > 0)
                return value.Substring(0, index) + "...";

            return value.Substring(0, length - 3) + "...";
        }

        public CrawlArticle DoCrawlSingleUrl(string url, CrawlPageConfig crawlPageConfig)
        {
            try
            {
                //Thread.Sleep(300);
                var parser = new HtmlParser();
                var html = GetHtmlFromUrl(url);
                if (html != null)
                {
                    var document = parser.Parse(html);
                    var title = document.QuerySelector(crawlPageConfig.Title).TextContent;

                    string description = "";

                    var descriptionElement = document.QuerySelector((string)crawlPageConfig.Description.Query);
                    if (descriptionElement != null)
                    {
                        if ((string)crawlPageConfig.Description.Excludes != null && ((string)crawlPageConfig.Description.Excludes) != "")
                        {
                            foreach (var e in ((string)crawlPageConfig.Description.Excludes).Split(','))
                            {
                                foreach (var child in descriptionElement.QuerySelectorAll(e))
                                {
                                    child.Remove();
                                }
                            }
                        }
                        description = RemoveHtmlTag(descriptionElement.InnerHtml).Trim();
                    }



                    var contents = "";

                    foreach (var contentQuery in crawlPageConfig.Contents)
                    {
                        var rawContent = document.QuerySelector(contentQuery.Query);
                        if (contentQuery.Excludes != null && contentQuery.Excludes != "")
                        {
                            foreach (var e in contentQuery.Excludes.Split(','))
                            {

                                foreach (var child in rawContent.QuerySelectorAll(e))
                                {
                                    child.Remove();
                                }
                            }
                        }

                        contents += rawContent.InnerHtml;

                    }

                    contents = RemoveExtraContent(contents);


                    if (description == null || description == "" || description.Length < 100)
                    {
                        //description = (contents.Length > 300) ? contents.Substring(0, 500) : contents;
                        description = TruncateAtWord(RemoveHtmlTag(contents).Replace(title, ""), 200);
                    }

                    //string imageUrl = "";
                    //Regex regex = new Regex("<img [^>]*src=\"([^ \"]+)");
                    //Match match = regex.Match(contents);
                    //if (match.Success)
                    //{
                    //    var s = match.Value;
                    //    imageUrl = s.Substring(s.IndexOf("src=\"")+5);
                    //} 

                    return new CrawlArticle() { Content = contents, Title = title, Description = description };
                }
                
            }
            catch (Exception ex)
            {
                logger.Trace(ex.StackTrace, "Lỗi DoCrawlSingleUrl({0},crawlPageConfig", url);
               
            }
            return null;
        }


        private static string[] _filterList = { "script", "input[type='hidden']", "iframe", "style" };

        public HtmlParser _htmlParser = new HtmlParser();

        public string RemoveExtraContent(string content)
        {
            var document = _htmlParser.Parse("<div id='autonews-filter-content'>" + content + "</div>");
            foreach (var e in _filterList)
            {
                foreach (var child in document.QuerySelectorAll(e))
                {
                    child.Remove();
                }
            }
            return document.QuerySelector("#autonews-filter-content").InnerHtml;

        }


        public string GetHtmlFromUrl(string url)
        {
            WebResponse myResponse = null;
            StreamReader sr = null;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.Method = "GET";
                myResponse = myRequest.GetResponse();
                sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                logger.Trace(ex.StackTrace, "Lỗi GetHtmlFromUrl({0})", url);
            }
            finally
            {
                sr.Dispose();
                myResponse.Dispose();
            }
            return null;
        }
    }
}
