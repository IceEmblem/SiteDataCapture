// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrawlSettings.cs" company="pzcast">
//   (C) 2015 pzcast. All rights reserved.
// </copyright>
// <summary>
//   The crawl settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Crawler.CrawlTaskManages
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;

    /// <summary>
    /// The crawl settings.
    /// </summary>
    [Serializable]
    public class CrawlSettings
    {
        /// <summary>
        /// 指示是否自动限速
        /// </summary>
        public bool IsAutoSpeedLimit { get; set; } = false;

        /// <summary>
        /// 1~5秒随机限速
        /// </summary>
        public void AutoSpeedLimit()
        {
            int span = new Random().Next(1000, 5000);
            Thread.Sleep(span);
        }

        /// <summary>
        /// 抓取深度（0为不限制）
        /// </summary>
        public int Depth { get; set; } = 0;

        /// <summary>
        /// 不抓取的 Link 后缀
        /// </summary>
        public List<string> EscapeLinks { get; private set; } = new List<string>();

        /// <summary>
        /// 抓取的 Href 关键字
        /// </summary>
        public List<string> HrefKeywords { get; private set; } = new List<string>();

        /// <summary>
        /// 指定过滤规则
        /// </summary>
        public List<Regex> RegularFilterExpressions { get; private set; } = new List<Regex>();

        public bool IsCrawlUrl(string url)
        {
            if (HrefKeywords.Count > 0)
            {
                if (!HrefKeywords.Any(url.Contains))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            if (EscapeLinks.Count > 0)
            {
                if (EscapeLinks.Any(suffix => url.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            if (RegularFilterExpressions.Count > 0)
            {
                if (RegularFilterExpressions.Any(pattern => pattern.IsMatch(url)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 是否保存cookie
        /// </summary>
        public bool KeepCookie { get; set; } = true;

        public CookieContainer CookieContainer { get; } = new CookieContainer();

        /// <summary>
        /// 是否只抓取当前站点的信息
        /// </summary>
        public bool LockHost { get; set; } = true;

        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout { get; set; } = 15000;

        /// <summary>
        /// Gets or sets the user agent.
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11";

        #region 爬虫事件
        /// <summary>
        /// 添加任务事件
        /// </summary>
        public Func<ICrawlTask, bool> AddUrlEvent { get; set; }

        public bool CallAddUrlEvent(ICrawlTask crawlTask)
        {
            if (AddUrlEvent == null) {
                return true;
            }

            return AddUrlEvent(crawlTask);
        }

        /// <summary>
        /// 抓取错误事件
        /// </summary>
        public Action<ICrawlTask, Exception> CrawlErrorEvent { get; set; }

        public void CallCrawlErrorEvent(ICrawlTask crawlTask, Exception ex)
        {
            if (CrawlErrorEvent == null) {
                return;
            }

            CrawlErrorEvent(crawlTask, ex);
        }

        /// <summary>
        /// 抓取成功事件，第1个参数为抓取任务，第2个参数为抓取到的html字符串，第3个参数为抓取到的内存流，返回值为接下来要执行的任务
        /// </summary>
        public Func<ICrawlTask, string, MemoryStream, IEnumerable<ICrawlTask>> DataReceivedEvent { get; set; }

        public IEnumerable<ICrawlTask> CallDataReceivedEvent(ICrawlTask crawlTask, string html, MemoryStream memoryStream)
        {
            if (DataReceivedEvent == null) {
                return new ICrawlTask[] { };
            }

            return DataReceivedEvent(crawlTask, html, memoryStream);
        }
        #endregion
    }
}