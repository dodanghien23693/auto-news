using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawl_Config
{
    public class CrawlLinkConfig
    {
        public string LinkQuery { get; set; }
        public string Attribute { get; set; }
    }
    public class LinkConfig
    {
        public string Url { get; set; }
        public int CategoryId { get; set; }
    }

    public class CrawlArticle
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
    }

    public class CrawlConfigJson
    {
        public int Id { get; set; }
        public int NewsSourceId { get; set; }
        public CrawlMethod Method { get; set; }

        public int CategoryId { get; set; }
        public CrawlPageConfig CrawlPageConfig { get; set; }

        public ScheduleConfig ScheduleConfig { get; set; }
    }

    public class ScheduleConfig
    {
        public string Id { get; set; }
        public dynamic Type { get; set; }
        public string StartAt { get; set; }
    }
    public class CrawlMethod
    {
        public int MethodId { get; set; }
        public dynamic Description { get; set; }
    }

    public class CrawlPageConfig
    {
        public string Title { get; set; }
        public List<ContentQuery> Contents { get; set; }
    }

    public class ContentQuery
    {
        public string Query { get; set; }
        public string Excludes { get; set; }
    }

}
