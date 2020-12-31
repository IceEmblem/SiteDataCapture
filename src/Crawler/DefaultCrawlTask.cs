using Crawler.CrawlTaskManages;
using Crawler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawler
{
    public class DefaultCrawlTask : ICrawlTask
    {
        private int _taskFailNum { get; set; }

        private CrawlSettings _crawlSettings { get; }

        public string Url { get; protected set; }

        public string TaskType { get; set; }

        /// <summary>
        /// 当前抓取的深度
        /// </summary>
        public int Depth { get; protected set; }

        public DefaultCrawlTask(string url, int depth, CrawlSettings crawlSettings)
        {
            Url = url;

            Depth = depth;

            _crawlSettings = crawlSettings;
        }

        public ICrawlTask CreateNextTask(string url) {
            return new DefaultCrawlTask(url, Depth + 1, _crawlSettings);
        }

        public Task<IEnumerable<ICrawlTask>> Run()
        {
            return Task.Run(() => {
                return CrawlProcessHandle();
            });
        }


        /// <summary>
        /// 抓取
        /// </summary>
        private IEnumerable<ICrawlTask> CrawlProcessHandle()
        {
            _crawlSettings.AutoSpeedLimit();

            using (RequestData requestData = new RequestData(Url))
            {
                requestData.UserAgent = _crawlSettings.UserAgent;
                if (_crawlSettings.KeepCookie)
                {
                    requestData.CookieContainer = _crawlSettings.CookieContainer;
                }
                if (_crawlSettings.Timeout > 0)
                {
                    requestData.Timeout = _crawlSettings.Timeout;
                }

                RequestResult requestResult = null;
                try
                {
                    requestResult = requestData.Request();
                }
                catch (Exception ex)
                {
                    if (_taskFailNum > 4)
                    {
                        _crawlSettings.CallCrawlErrorEvent(this, ex);
                        return new ICrawlTask[] { };
                    }

                    _taskFailNum++;

                    return new ICrawlTask[] { this };
                }

                List<ICrawlTask> crawlTasks = new List<ICrawlTask>();

                crawlTasks.AddRange(_crawlSettings.CallDataReceivedEvent(this, requestResult.Html, requestResult.Stream));

                crawlTasks.AddRange(ParseLinks(requestResult.Html));

                return crawlTasks;
            }
        }

        /// <summary>
        /// 解析 links.
        /// </summary>
        private IEnumerable<ICrawlTask> ParseLinks(string html)
        {
            if (_crawlSettings.Depth > 0 && Depth >= _crawlSettings.Depth)
            {
                return new ICrawlTask[] { };
            }

            var urlDictionary = new Dictionary<string, string>();

            Match match = Regex.Match(html, @"(?s)<a.*?href=(""|')(?<href>.*?)(""|').*?>(?<text>.*?)</a>");
            while (match.Success)
            {
                // 以 href 作为 key
                string urlKey = match.Groups["href"].Value;

                // 以 text 作为 value
                string urlValue = Regex.Replace(match.Groups["text"].Value, "(?s)<.*?>", string.Empty);

                urlDictionary[urlKey] = urlValue;
                match = match.NextMatch();
            }

            List<ICrawlTask> crawlTasks = new List<ICrawlTask>();
            foreach (var item in urlDictionary)
            {
                string href = item.Key;
                string text = item.Value;

                if (string.IsNullOrEmpty(href))
                {
                    continue;
                }

                string url = href.Replace("%3f", "?")
                        .Replace("%3d", "=")
                        .Replace("%2f", "/")
                        .Replace("&amp;", "&");

                if (string.IsNullOrEmpty(url)
                    || url.StartsWith("#")
                    || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                    || url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var baseUri = new Uri(Url);
                // 如果 url 不是以 http 开头，则将 url 补充完整
                Uri currentUri = url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                     ? new Uri(url)
                                     : new Uri(baseUri, url);

                // 是否只抓取本站点的 url
                if (_crawlSettings.LockHost && baseUri.Host != currentUri.Host)
                {
                    continue;
                }

                if (!_crawlSettings.IsCrawlUrl(currentUri.AbsoluteUri)) 
                {
                    continue;
                }

                var newTask = CreateNextTask(currentUri.AbsoluteUri);
                // 如果添加Url事件返回false
                if (!_crawlSettings.CallAddUrlEvent(newTask))
                {
                    continue;
                }

                crawlTasks.Add(newTask);
            }

            return crawlTasks;
        }
    }
}
