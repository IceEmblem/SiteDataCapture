using SiteDataCapture.Articles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Net.Http;
using System.Web;

namespace SiteDataCapture
{
    public class ArticleAccessController
    {
        private ArticleRepository _articleRepository { get; }

        /// <summary>
        /// 已访问量
        /// </summary>
        public int AccessedNum { get; protected set; }

        /// <summary>
        /// 访问量
        /// </summary>
        public int AccessNum { get; protected set; }


        /// <summary>
        /// 访问的持续时间，秒
        /// </summary>
        public int AccessContinueTime { get; protected set; }

        /// <summary>
        /// 每天访问的开始时间，从 0 点到开始时间的秒
        /// </summary>
        public int DayAccessStartTime { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessNum"></param>
        /// <param name="accessContinueTime"></param>
        /// <param name="dayAccessStartTime">每天访问的开始时间，从 0 点到开始时间的秒数，默认值-1，表示不开启每日循环访问</param>
        public ArticleAccessController(int accessNum, int accessContinueTime, int dayAccessStartTime = -1) 
        {
            _articleRepository = new ArticleRepository();
            AccessNum = accessNum;
            AccessContinueTime = accessContinueTime;
            DayAccessStartTime = dayAccessStartTime;
        }

        private void Access(string url) 
        {
            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
            // 假装来源为360搜索（为什么不是百度，因为百度太常用了）
            string referer = "https://www.so.com/s?src=360chrome_newtab_search&q=" + HttpUtility.UrlEncode(url);
            httpClient.DefaultRequestHeaders.Add("Referer", referer);

            httpClient.GetAsync(url);
        }

        /// <summary>
        /// 获取下一次访问的时间
        /// </summary>
        /// <returns></returns>
        private int GetNextAccessTime() 
        {
            return (AccessContinueTime / AccessNum) * 1000;
        }

        /// <summary>
        /// 开始访问
        /// </summary>
        public void Start() 
        {
            if (_articleRepository.Articles.Count() == 0) {
                throw new Exception("文章不存在，请先抓取文章");
            }

            Random random = new Random();

            while (true) {
                int index = random.Next(0, _articleRepository.Articles.Count() - 1);
                Article article = _articleRepository.Articles.ElementAt(index);

                Access(article.Url);
                AccessedNum++;

                if (AccessedNum < AccessNum) {
                    Thread.Sleep(GetNextAccessTime());
                    continue;
                }

                if (DayAccessStartTime < 0) {
                    break;
                }

                AccessedNum = 0;

                DateTime currentTime = DateTime.Now;
                DateTime tomorrow = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day);
                tomorrow.AddDays(1);
                tomorrow.AddSeconds(DayAccessStartTime);

                Thread.Sleep((int)(tomorrow - currentTime).TotalMilliseconds);
            }
        }
    }
}
