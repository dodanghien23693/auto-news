using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Config_Model
{
    public class CrawlLinkConfig
    {
        public string LinkQuery { get; set; }
        public string Attribute { get; set; }
        public string ImageContainer { get; set; }
    }
    public class LinkConfig
    {
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; }
    }

    public class CrawlArticle
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
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
        public dynamic Description { get; set; }
        public List<ContentQuery> Contents { get; set; }
    }

    public class ContentQuery
    {
        public string Query { get; set; }
        public string Excludes { get; set; }
    }


    public static class CrawlMethodId
    {
        public const int SingleUrl = 0;
        public const int MultiUrl = 1;
        public const int GeneratedLinks = 2;

    }

    public static class CrawlConfigParamType
    {
        public const int Fix = 0;
        public const int Range = 1;
        public const int ListValue = 2;
        public const int Categories = 3;
    }

    public static class CrawlScheduleType
    {
        public const int OneTime = 0;
        public const int Minutes = 1;
        public const int Hourly = 2;
        public const int Daily = 3;
        public const int Weekly = 4;
        public const int Monthly = 5;
        public const int Yearly = 6;
    }


    public class RelatedListJson
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public List<SimilarityJson> Similarities { get; set; }
        public RelatedListJson()
        {
            Similarities = new List<SimilarityJson>();
        }
    }

    public class SimilarityJson
    {
        public int ArticleId { get; set; }
        public string Title { get; set; }
        public double Similarity { get; set; }
        public string Link { get; set; }
    }
}
