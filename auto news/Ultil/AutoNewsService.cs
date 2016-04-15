using auto_news.Models;
using Crawl_Config;
using Crawl_News_Module;
using Hangfire;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace auto_news.Ultil
{
    public class AutoNewsService
    {

        public static AutoNewsDbContext AutoNewsDb = new AutoNewsDbContext();
        public static CrawlerService Crawler = new CrawlerService();


        public void DoCrawl(CrawlConfigJson configObject)
        {

            if (configObject.Method.MethodId == CrawlMethodId.SingleUrl)
            {
                //dynamic description = JsonConvert.DeserializeObject(configObject.Description);
                string url = configObject.Method.Description.Url;
                var article =  Crawler.DoCrawlSingleUrl(url, configObject.CrawlPageConfig);

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
                        var article =  Crawler.DoCrawlSingleUrl(url, configObject.CrawlPageConfig);
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
                Crawler.GenerateLinkFromParams(0, Params, new string[Params.Count], configObject.CategoryId, links);

                List<LinkConfig> crawledLinks = new List<LinkConfig>();

                var i = 0;
                
                while (i < links.Count)
                {
                    var HasNewArticle = true;
                    //var link = links[i];
                    List<LinkConfig> linksConfig =  Crawler.CrawlLinkFromUrl(links[i].Url, configObject.Method.Description.CrawlLinkConfig.ToObject<CrawlLinkConfig>(),links[i].CategoryId);
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
                            var article =  Crawler.DoCrawlSingleUrl(link.Url, configObject.CrawlPageConfig);

                            if (article != null)
                            {
                                 article.ImageUrl = link.ImageUrl;
                                 AddArticle(article, configObject, link);
                            }
                        }
                        j++;
                    }
                    i++;
                }
            }
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
                MvcApplication.log.Error("Add Article Error", ex);
            }
            return 0;

        }
        private bool IsCrawled(string url, CrawlConfigJson configObject)
        {
            if (AutoNewsDb.Articles.Where(a => a.OriginUrl.Equals(url)).FirstOrDefault() == null) return false;
            else return true;
        }

        public void ScheduleJob()
        {
            var _db = new AutoNewsDbContext();
            var crawlConfigs = _db.CrawlConfigs.Where(c => c.IsScheduled == false).ToList();
            foreach (var crawlCf in crawlConfigs)
            {
                CrawlConfigJson config = JsonConvert.DeserializeObject<CrawlConfigJson>(crawlCf.Description);
                var newsSource = _db.NewsSources.Where(i => i.Id == config.NewsSourceId).FirstOrDefault();
                var sourceName = "Unknown";
                if (newsSource != null)
                {
                    sourceName = newsSource.Name;
                }
                config.NewsSourceId = crawlCf.NewsSourceId;
                config.Id = crawlCf.Id;
                ScheduleConfig schedule = config.ScheduleConfig;

                if (schedule.Type.TypeId == CrawlScheduleType.OneTime)
                {
                    var hours = DateTime.Now.Hour - int.Parse((string)schedule.Type.StartAt.Split(':')[0]);
                    var minutes = DateTime.Now.Minute - int.Parse((string)schedule.Type.StartAt.Split(':')[1]);
                    BackgroundJob.Schedule(() => DoCrawl(config), new TimeSpan(hours, minutes, 0));
                }
                else
                {
                    int type = schedule.Type.TypeId;
                    string cron = "";
                    if (type == CrawlScheduleType.Minutes)
                    {
                        cron = "0/" + schedule.Type.Minutes + " * * * *";
                    }
                    else if (type == CrawlScheduleType.Hourly)
                    {
                        cron = "0 0/" + schedule.Type.Hours + " * * *";
                    }
                    else if (type == CrawlScheduleType.Daily || type == CrawlScheduleType.Weekly)
                    {
                        var hours = "0";
                        var minutes = "0";
                        try
                        {
                            if (((string)schedule.Type.StartAt.Split(':')[0]).Trim() != "")
                            {
                                hours = ((string)schedule.Type.StartAt.Split(':')[0]).Trim();
                            };
                            if (((string)schedule.Type.StartAt.Split(':')[1]).Trim() != "")
                            {
                                minutes = ((string)schedule.Type.StartAt.Split(':')[1]).Trim();
                            };
                        }
                        catch (Exception ex) { }

                        if (type == CrawlScheduleType.Daily)
                        {
                            cron = minutes + " " + hours + " 1/" + schedule.Type.Days + " * *";
                        }
                        else if (type == CrawlScheduleType.Daily)
                        {
                            cron = minutes + " " + hours + " * * " + String.Join(",", schedule.Type.Weekdays);
                        }
                    }

                    RecurringJob.AddOrUpdate(sourceName+"-"+config.Id.ToString(), () => DoCrawl(config),cron);
                }

                crawlCf.IsScheduled = true;
                //_db.Entry(crawlCf).State = EntityState.Modified;
                _db.SaveChanges();

            }
        }
    }

}