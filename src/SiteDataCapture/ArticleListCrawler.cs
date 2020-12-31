using Crawler;
using Crawler.CrawlTaskManages;
using SiteDataCapture.Articles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace SiteDataCapture
{
    public class ArticleListCrawler
    {
        public string RootUrl { get; }

        public int CrawlerDepth { get; }

        public Regex NextPageUrlMatchRegex { get; }

        public Regex ArticleUrlMatchRegex { get; }

        public List<Regex> ArticleInfoMatchRegexs { get; } = new List<Regex>();

        public List<Regex> ArticleFileMatchRegexs { get; } = new List<Regex>();

        private ArticleRepository _articleRepository { get; } = new ArticleRepository();

        public ArticleListCrawler(string rootUrl, int crawlerDepth, Regex nextPageUrlMatchRegex, Regex articleUrlMatchRegex) 
        {
            RootUrl = rootUrl;
            CrawlerDepth = crawlerDepth;
            NextPageUrlMatchRegex = nextPageUrlMatchRegex;
            ArticleUrlMatchRegex = articleUrlMatchRegex;
        }

        /// <summary>
        /// 开始抓取
        /// </summary>
        public void Crawl() {
            CrawlSettings crawlSettings = new CrawlSettings();
            crawlSettings.RegularFilterExpressions.Add(NextPageUrlMatchRegex);
            crawlSettings.Depth = CrawlerDepth;
            crawlSettings.DataReceivedEvent = CrawlDataReceivedEvent;

            CrawlMaster crawlMaster = new CrawlMaster(10, new string[] { RootUrl }, crawlSettings);
            crawlMaster.Crawl();
        }

        /// <summary>
        /// 获取已抓取的文章数量
        /// </summary>
        /// <returns></returns>
        public int GetCrawledArticleNum() {
            return _articleRepository.Articles.Count();
        }

        protected IEnumerable<ICrawlTask> CrawlDataReceivedEvent(ICrawlTask crawlTask, string html, MemoryStream memoryStream) 
        {
            // 如果任务类型是空的
            if (crawlTask.TaskType == null) {
                List<ICrawlTask> crawlTasks = new List<ICrawlTask>();

                foreach (var match in ArticleUrlMatchRegex.Matches(html)) {
                    ICrawlTask newTask = crawlTask.CreateNextTask(match.ToString());
                    newTask.TaskType = "ArticleUrl";
                    crawlTasks.Add(newTask);
                }

                return crawlTasks;
            }

            if (crawlTask.TaskType == "ArticleUrl") {
                // 插入文章
                List<string> articleInfo = new List<string>();

                ArticleInfoMatchRegexs.ForEach(infoMatchRegex =>
                {
                    foreach (var item in infoMatchRegex.Matches(html)) {
                        articleInfo.Add(item.ToString());
                    }
                });

                Article article = new Article(crawlTask.Url, articleInfo.ToArray());

                _articleRepository.Insert(article);

                // 抓取文件
                List<ICrawlTask> crawlTasks = new List<ICrawlTask>();

                ArticleFileMatchRegexs.ForEach(fileMatchRegex =>
                {
                    foreach (var item in fileMatchRegex.Matches(html))
                    {
                        ICrawlTask newTask = crawlTask.CreateNextTask(item.ToString());
                        newTask.TaskType = "FileUrl";
                        crawlTasks.Add(newTask);
                    }
                });

                return crawlTasks;
            }

            // 如果任务既不是 null 也不是 ArticleUrl，则任务是 FileUrl
            Uri uri = new Uri(crawlTask.Url);

            File.WriteAllBytes(uri.LocalPath, memoryStream.ToArray());

            return new ICrawlTask[] { };
        }
    }
}
