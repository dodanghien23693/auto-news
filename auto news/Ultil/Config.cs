using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace auto_news.Config
{
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
}