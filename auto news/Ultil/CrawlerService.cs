using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using auto_news;
using auto_news.Ultil;
using Crawl_Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Crawl_News_Module
{
    public class CrawlerService
    {
        public object MVCAppllication { get; private set; }

        public List<LinkConfig> CrawlLinkFromUrl(string url, CrawlLinkConfig crawlLinkConfig,int categoryId)
        {
            List<LinkConfig> listLinkConfig = new List<LinkConfig>();
            //CrawlLinkConfig crawlConfig = configObject.Method.Description.CrawlLinkConfig.ToObject<CrawlLinkConfig>();
            var parser = new HtmlParser();
            var document = parser.Parse(GetHtmlFromUrl(url));
            var links = document.QuerySelectorAll(crawlLinkConfig.LinkQuery);

            List<string> urls = new List<string>();
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
                
                listLinkConfig.Add(new LinkConfig() { ImageUrl = imageUrl, Url = articleUrl,CategoryId = categoryId });
                //urls.Add(link.GetAttribute(crawlLinkConfig.Attribute));

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

        public  CrawlArticle DoCrawlSingleUrl(string url, CrawlPageConfig crawlPageConfig)
        {
            try
            {
                //Thread.Sleep(300);
                var parser = new HtmlParser();
                var document = parser.Parse(GetHtmlFromUrl(url));
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
                    //if (imageUrl == "")
                    //{
                    //    if (rawContent.QuerySelector("img") != null)
                    //    {
                    //        imageUrl = rawContent.QuerySelector("img").GetAttribute("src");
                    //    }
                    //}
     
                }

                contents = RemoveExtraContent(contents);


                if (description == null || description == "" || description.Length < 100)
                {
                    //description = (contents.Length > 300) ? contents.Substring(0, 500) : contents;
                    description = TruncateAtWord(RemoveHtmlTag(contents).Replace(title,""), 200);
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
            catch(Exception ex)
            {
                Console.Write(ex.Message);
            }


            return null;
            

        }


        private static string[] _filterList = { "script", "input[type='hidden']", "iframe", "style" };

        public HtmlParser  _htmlParser = new HtmlParser();

        public string RemoveExtraContent(string content)
        {
            var document = _htmlParser.Parse("<div id='autonews-filter-content'>"+content+"</div>");
            foreach(var e in _filterList)
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
            catch(Exception ex)
            {

            }
            finally
            {
                sr.Dispose();
                myResponse.Dispose();
            }
            return "";
        }
    }
}
