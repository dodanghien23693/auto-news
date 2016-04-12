using AngleSharp.Parser.Html;
using auto_news.Ultil;
using Crawl_Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Crawl_News_Module
{
    public class CrawlerService
    {
        public List<string> CrawlLinkFromUrl(string url, CrawlLinkConfig config)
        {
            var parser = new HtmlParser();
            var document = parser.Parse(GetHtmlFromUrl(url));
            var links = document.QuerySelectorAll(config.LinkQuery);

            List<string> urls = new List<string>();
            foreach (var link in links)
            {
                urls.Add(link.GetAttribute("href"));
            }

            return urls;
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

        public List<CrawlArticle> DoCrawlMultiUrl(string[] urls, CrawlPageConfig crawlPageConfig)
        {
            List<CrawlArticle> articles = new List<CrawlArticle>();
            for (var i = 0; i < urls.Length; i++)
            {
                articles.Add(DoCrawlSingleUrl(urls[i], crawlPageConfig));
            }
            return articles;
        }

        public CrawlArticle DoCrawlSingleUrl(string url, CrawlPageConfig crawlPageConfig)
        {
            var parser = new HtmlParser();
            var document = parser.Parse(GetHtmlFromUrl(url));
            var title = document.QuerySelector(crawlPageConfig.Title).TextContent;

            var contents = "";
            string imageUrl = "";
            foreach (var contentQuery in crawlPageConfig.Contents)
            {
                var rawContent = document.QuerySelector(contentQuery.Query);
                if (contentQuery.Excludes != null)
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
                if (imageUrl == "")
                {
                    if (rawContent.QuerySelector("img") != null)
                    {
                        imageUrl = rawContent.QuerySelector("img").GetAttribute("src");
                    }
                }

            }

            //var imageUrl = parser.Parse(contents).QuerySelector("img").GetAttribute("src");

            return new CrawlArticle() { Content = contents, Title = title, ImageUrl = imageUrl };

        }

        public string GetHtmlFromUrl(string url)
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();
            StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            return sr.ReadToEnd();
        }
    }
}
