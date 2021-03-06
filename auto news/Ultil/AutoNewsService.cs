﻿using auto_news.Models;
using Config_Model;
using Crawl_News_Module;
using Hangfire;
using Newtonsoft.Json;
using NLog;
using System;
using System.Linq;

namespace auto_news
{
    public class AutoNewsService
    {

        public static CrawlerService Crawler = new CrawlerService();
        public static Logger logger = LogManager.GetCurrentClassLogger();
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
                    BackgroundJob.Schedule(() => Crawler.DoCrawl(config), new TimeSpan(hours, minutes, 0));
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
                        catch (Exception ex) {
                            logger.Error(ex.Message);
                        }

                        if (type == CrawlScheduleType.Daily)
                        {
                            cron = minutes + " " + hours + " 1/" + schedule.Type.Days + " * *";
                        }
                        else if (type == CrawlScheduleType.Daily)
                        {
                            cron = minutes + " " + hours + " * * " + String.Join(",", schedule.Type.Weekdays);
                        }
                    }

                    RecurringJob.AddOrUpdate(sourceName+"-"+config.Id.ToString(), () => Crawler.DoCrawl(config),cron);
                }

                crawlCf.IsScheduled = true;
                //_db.Entry(crawlCf).State = EntityState.Modified;
                _db.SaveChanges();

            }
        }
    }

}