namespace Crawler
{
    using Crawler.CrawlTaskManages;
    using Crawler.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 爬行
    /// </summary>
    public class CrawlMaster
    {
        private CrawlTaskManage _crawlTaskManage { get; set; }

        public CrawlMaster(int threadCount, IEnumerable<string> seedsAddress, CrawlSettings settings)
        {
            _crawlTaskManage = new CrawlTaskManage(threadCount);
            foreach (var item in seedsAddress) {
                _crawlTaskManage.AddNewTask(new DefaultCrawlTask(item, 0, settings));
            }
        }

        /// <summary>
        /// 开始爬行
        /// </summary>
        public void Crawl()
        {
            _crawlTaskManage.Start();
        }
    }
}