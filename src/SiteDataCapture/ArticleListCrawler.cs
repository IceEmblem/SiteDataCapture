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
        const string ArticleFileDir = "ArticleFiles";

        static Regex FileNameRegex = new Regex("([^/?]*?)(\\?.*)?$");

        static Regex SrcRegex = new Regex("src=\"(.*?)\"");

        public int CrawlerDepth { get; }

        public IEnumerable<string> RootUrls { get; }

        public IEnumerable<Regex> NextPageUrlMatchRegexs { get; }

        public IEnumerable<Regex> ArticleUrlMatchRegexs { get; }

        public List<Regex> ArticleInfoMatchRegexs { get; } = new List<Regex>();

        public List<Regex> ArticleFileMatchRegexs { get; } = new List<Regex>();

        private ArticleRepository _articleRepository { get; } = new ArticleRepository();

        public ArticleListCrawler(int crawlerDepth, IEnumerable<string> rootUrls, IEnumerable<Regex> nextPageUrlMatchRegexs, IEnumerable<Regex> articleUrlMatchRegexs) 
        {
            CrawlerDepth = crawlerDepth;
            if (!rootUrls.Any()) 
            {
                throw new Exception("必须输入种子地址");
            }
            RootUrls = rootUrls;

            if (!nextPageUrlMatchRegexs.Any())
            {
                throw new Exception("必须输入下一页Url匹配正则");
            }
            NextPageUrlMatchRegexs = nextPageUrlMatchRegexs;

            if (!articleUrlMatchRegexs.Any()) {
                throw new Exception("必须输入文章Url匹配正则");
            }
            ArticleUrlMatchRegexs = articleUrlMatchRegexs;
        }

        /// <summary>
        /// 开始抓取
        /// </summary>
        public void Crawl() {
            CrawlSettings crawlSettings = new CrawlSettings();
            crawlSettings.RegularFilterExpressions.AddRange(NextPageUrlMatchRegexs);
            crawlSettings.DataReceivedEvent = CrawlDataReceivedEvent;

            CrawlMaster crawlMaster = new CrawlMaster(10, CrawlerDepth, RootUrls, crawlSettings);
            crawlMaster.Crawl();

            _articleRepository.SaveArticlesToFile();
        }

        /// <summary>
        /// 获取已抓取的文章数量
        /// </summary>
        /// <returns></returns>
        public int GetCrawledArticleNum() {
            return _articleRepository.Articles.Count();
        }

        protected IEnumerable<ICrawlTask> CrawlDataReceivedEvent(ICrawlTask crawlTask, string html, IEnumerable<string> urls, MemoryStream memoryStream) 
        {
            // 如果任务类型是空的
            if (crawlTask.TaskType == null) {
                List<ICrawlTask> crawlTasks = new List<ICrawlTask>();

                foreach (var ArticleUrlMatchRegex in ArticleUrlMatchRegexs) 
                {
                    foreach (string url in urls)
                    {
                        if (ArticleUrlMatchRegex.IsMatch(url))
                        {
                            ICrawlTask newTask = crawlTask.CreateNextTask(url);
                            newTask.TaskType = "ArticleUrl";
                            crawlTasks.Add(newTask);
                        }
                    }
                }

                return crawlTasks;
            }

            if (crawlTask.TaskType == "ArticleUrl") {
                // 插入文章
                List<string> articleInfo = new List<string>();

                ArticleInfoMatchRegexs.ForEach(infoMatchRegex =>
                {
                    foreach (var item in infoMatchRegex.Matches(html)) {
                        articleInfo.Add(item.ToString().Replace("\n", "").Replace("\r", ""));
                    }
                });

                Article article = new Article(crawlTask.Url, articleInfo.ToArray());

                _articleRepository.Insert(article);

                // 抓取文件
                List<ICrawlTask> crawlTasks = new List<ICrawlTask>();

                var dirNameMatch = FileNameRegex.Match(crawlTask.Url);
                if (!dirNameMatch.Success)
                {
                    return new ICrawlTask[] { };
                }

                foreach (Match srcMatch in SrcRegex.Matches(html)) {
                    var url = srcMatch.Groups[1].Value;
                    ArticleFileMatchRegexs.ForEach(fileMatchRegex =>
                    {
                        if (fileMatchRegex.IsMatch(url))
                        {
                            Uri uri = url.StartsWith("http") ? new Uri("url") : new Uri(new Uri(crawlTask.Url), url);
                            ICrawlTask newTask = crawlTask.CreateNextTask(uri.AbsoluteUri);
                            newTask.TaskType = "FileUrl-" + dirNameMatch.Groups[1].Value;
                            crawlTasks.Add(newTask);
                        }
                    });
                }

                return crawlTasks;
            }

            // 如果任务既不是 null 也不是 ArticleUrl，则任务是 FileUrl

            Match match = FileNameRegex.Match(crawlTask.Url);

            if (!match.Success) {
                return new ICrawlTask[] { };
            }

            var dirPath = ArticleFileDir + "\\" + crawlTask.TaskType;
            var filePath = dirPath + "\\" + match.Groups[1].Value;

            if (!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }

            File.WriteAllBytes(filePath, memoryStream.ToArray());

            return new ICrawlTask[] { };
        }
    }
}
