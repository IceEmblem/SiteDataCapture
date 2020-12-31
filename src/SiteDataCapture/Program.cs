using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SiteDataCapture
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("欢迎使用 IceEmblem 数据抓取程序");
            Console.WriteLine("请选择你要使用的功能");
            Console.WriteLine("1）数据抓取保存     2）文章随机访问");

            int selectFunc = 0;
            while (true) {
                var key = Console.ReadKey();
                Console.WriteLine();

                if (key.KeyChar == 49) {
                    selectFunc = 1;
                    break;
                }

                if (key.KeyChar == 50) {
                    selectFunc = 2;
                    break;
                }

                Console.WriteLine("无效的输入，请重新输入");
            }

            if (selectFunc == 1)
            {
                ArticleListCrawl();
            }
            else {
                ArticleAccess();
            }

            Console.WriteLine();
            Console.WriteLine("程序已退出");
        }

        static void ArticleListCrawl() {
            Console.WriteLine("你已选择数据抓取保存功能");

            Console.WriteLine("请输入你要抓取的地址");
            string rootUrl = Console.ReadLine();

            int crawlerDepth = 0;
            Console.WriteLine("请输入抓取深度，如不限制抓取深度，请输入 0");
            while (true) {
                try
                {
                    crawlerDepth = Convert.ToInt32(Console.ReadLine());
                    break;
                }
                catch {
                    Console.WriteLine("无效的输入，请重新输入");
                }
            }

            Regex nextPageUrlMatchRegex;
            Console.WriteLine("请输入匹配下一页Url的正则");
            while (true)
            {
                try
                {
                    nextPageUrlMatchRegex = new Regex(Console.ReadLine());
                    break;
                }
                catch
                {
                    Console.WriteLine("无效的正则，请重新输入");
                }
            }

            Regex articleUrlMatchRegex;
            Console.WriteLine("请输入匹配文章Url的正则");
            while (true)
            {
                try
                {
                    articleUrlMatchRegex = new Regex(Console.ReadLine());
                    break;
                }
                catch
                {
                    Console.WriteLine("无效的正则，请重新输入");
                }
            }

            Console.WriteLine("程序准备就绪，现在开始抓取，请耐心等待抓取完成");
            ArticleListCrawler articleListCrawler = new ArticleListCrawler(rootUrl, crawlerDepth, nextPageUrlMatchRegex, articleUrlMatchRegex);
            var task = Task.Run(() =>
            {
                try
                {
                    articleListCrawler.Crawl();
                    Console.WriteLine("抓取完成");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("抓取出错，错误信息如下：");
                    Console.WriteLine(ex.Message);
                }
            });

            int crawledArticleNum = 0;
            while (true)
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    break;
                }

                int curCrawledArticleNum = articleListCrawler.GetCrawledArticleNum();
                if (crawledArticleNum != curCrawledArticleNum)
                {
                    crawledArticleNum = curCrawledArticleNum;
                    Console.WriteLine($"已抓取的文章数：{crawledArticleNum}");
                }

                Thread.Sleep(5000);
            }
        }

        static void ArticleAccess() {
            Console.WriteLine("你已选择文章随机访问功能");

            Console.WriteLine("请输入你想要增加的访问量");
            int accessNum;
            while (true) {
                try
                {
                    accessNum = Convert.ToInt32(Console.ReadLine());
                    if (accessNum <= 0) {
                        Console.WriteLine("访问量必须大于 0");
                    }
                    break;
                }
                catch 
                {
                    Console.WriteLine("无效的输入，请重新输入");
                }
            }

            Console.WriteLine("请输入访问的总用时，单位：秒");
            int accessContinueTime;
            while (true)
            {
                try
                {
                    accessContinueTime = Convert.ToInt32(Console.ReadLine());
                    if (accessContinueTime <= 0)
                    {
                        Console.WriteLine("访问总用时必须大于 0");
                    }
                    break;
                }
                catch
                {
                    Console.WriteLine("无效的输入，请重新输入");
                }
            }

            Console.WriteLine("是否每天循环访问");
            Console.WriteLine("如果是请输入每天开始访问的时间，从0点到开始时间的秒数（如果开启每天循环访问，则需要你不能停止程序）");
            Console.WriteLine("如果否，请输入 -1");
            int dayAccessStartTime;
            while (true)
            {
                try
                {
                    dayAccessStartTime = Convert.ToInt32(Console.ReadLine());
                    break;
                }
                catch
                {
                    Console.WriteLine("无效的输入，请重新输入");
                }
            }

            Console.WriteLine("程序准备就绪，现在随机访问");
            ArticleAccessController articleAccessController =  new ArticleAccessController(accessNum, accessContinueTime, dayAccessStartTime);
            Task task = Task.Run(() =>
            {
                try
                {
                    articleAccessController.Start();
                    Console.WriteLine("随机访问结束");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("程序运行错误，错误消息如下：");
                    Console.WriteLine(ex.Message);
                }
            });

            while (true) {
                if (task.Status == TaskStatus.RanToCompletion) {
                    break;
                }

                int sleepTime = accessContinueTime / 50;
                if (sleepTime < 5) {
                    sleepTime = 5;
                }

                Console.WriteLine($"已访问文章量：{articleAccessController.AccessedNum}");

                Thread.Sleep(sleepTime * 1000);
            }
        }
    }
}
