using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SiteDataCapture
{
    class Program
    {
        const string ConfigFilePath = "CaptureConfig.json";

        static CaptureConfig CaptureConfig;

        static void Main(string[] args)
        {
            IConfigurationRoot config =
                new ConfigurationBuilder()
                .AddJsonFile(ConfigFilePath)
                .Build();

            Console.WriteLine("欢迎使用 IceEmblem 数据抓取程序");

            if (Convert.ToBoolean(config["UseConfigFile"]) == true)
            {
                CaptureConfig = new CaptureConfig(config);
            }
            else 
            {
                CaptureConfig = new CaptureConfig();
                Config();
            }

            if (CaptureConfig.SelectFunc == 1)
            {
                ArticleListCrawl();
            }
            else
            {
                ArticleAccess();
            }

            Console.WriteLine();
            Console.WriteLine("程序已退出");
        }

        static void Config() {
            Console.WriteLine("请选择你要使用的功能");
            Console.WriteLine("1）数据抓取保存     2）文章随机访问");

            while (true)
            {
                Console.Write("> ");
                var key = Console.ReadKey();
                Console.WriteLine();

                if (key.KeyChar == 49)
                {
                    CaptureConfig.SelectFunc = 1;
                    break;
                }

                if (key.KeyChar == 50)
                {
                    CaptureConfig.SelectFunc = 2;
                    break;
                }

                Console.WriteLine("无效的输入，请重新输入");
            }

            if (CaptureConfig.SelectFunc == 1)
            {
                ArticleListCrawlConfigInput();
            }
            else
            {
                ArticleAccessConfigInput();
            }
        }

        static void ArticleListCrawlConfigInput() {
            Console.WriteLine("你已选择数据抓取保存功能");

            Console.WriteLine("请输入抓取深度，如不限制抓取深度，请输入 0");
            while (true)
            {
                try
                {
                    Console.Write("> ");
                    CaptureConfig.Func1.CrawlerDepth = Convert.ToInt32(Console.ReadLine());
                    break;
                }
                catch
                {
                    Console.WriteLine("无效的输入，请重新输入");
                }
            }

            Console.WriteLine("请输入你要抓取的地址");
            Console.WriteLine("此处为循环输入，想要退出循环输入\"break;\"");
            while (true)
            {
                try
                {
                    Console.Write("> ");
                    string input = Console.ReadLine();
                    if (input == "break;")
                    {
                        break;
                    }
                    CaptureConfig.Func1.RootUrls.Add(input);
                }
                catch
                {
                    Console.WriteLine("无效的输入，请重新输入");
                }
            }

            Console.WriteLine("请输入匹配下一页Url的正则");
            Console.WriteLine("此处为循环输入正则，想要退出循环输入\"break;\"");
            while (true)
            {
                try
                {
                    Console.Write("> ");
                    string input = Console.ReadLine();
                    if (input == "break;")
                    {
                        break;
                    }
                    CaptureConfig.Func1.NextPageUrlMatchRegexs.Add(new Regex(input));
                }
                catch
                {
                    Console.WriteLine("无效的正则，请重新输入");
                }
            }


            Console.WriteLine("请输入匹配文章Url的正则");
            Console.WriteLine("此处为循环输入正则，想要退出循环输入\"break;\"");
            while (true)
            {
                try
                {
                    Console.Write("> ");
                    string input = Console.ReadLine();
                    if (input == "break;")
                    {
                        break;
                    }
                    CaptureConfig.Func1.ArticleUrlMatchRegexs.Add(new Regex(input));
                }
                catch
                {
                    Console.WriteLine("无效的正则，请重新输入");
                }
            }

            Console.WriteLine("请输入需要保存的文章信息的正则");
            Console.WriteLine("此处为循环输入正则，想要退出循环输入\"break;\"");
            while (true)
            {
                try
                {
                    Console.Write("> ");
                    string input = Console.ReadLine();
                    if (input == "break;")
                    {
                        break;
                    }
                    CaptureConfig.Func1.ArticleInfoMatchRegexs.Add(new Regex(input));
                }
                catch
                {
                    Console.WriteLine("无效的正则，请重新输入");
                }
            }

            Console.WriteLine("请输入需要保存的文件的正则");
            Console.WriteLine("此处为循环输入正则，想要退出循环输入\"break;\"");
            while (true)
            {
                try
                {
                    Console.Write("> ");
                    string input = Console.ReadLine();
                    if (input == "break;")
                    {
                        break;
                    }
                    CaptureConfig.Func1.ArticleFileMatchRegexs.Add(new Regex(input));
                }
                catch
                {
                    Console.WriteLine("无效的正则，请重新输入");
                }
            }

        }

        static void ArticleAccessConfigInput()
        {
            Console.WriteLine("你已选择文章随机访问功能");

            Console.WriteLine("请输入你想要增加的访问量");
            while (true)
            {
                try
                {
                    Console.Write("> ");
                    CaptureConfig.Func2.AccessNum = Convert.ToInt32(Console.ReadLine());
                    if (CaptureConfig.Func2.AccessNum <= 0)
                    {
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
            while (true)
            {
                try
                {
                    Console.Write("> ");
                    CaptureConfig.Func2.AccessContinueTime = Convert.ToInt32(Console.ReadLine());
                    if (CaptureConfig.Func2.AccessContinueTime <= 0)
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
            while (true)
            {
                try
                {
                    Console.Write("> ");
                    CaptureConfig.Func2.DayAccessStartTime = Convert.ToInt32(Console.ReadLine());
                    break;
                }
                catch
                {
                    Console.WriteLine("无效的输入，请重新输入");
                }
            }

        }

        static void ArticleListCrawl() {
            Console.WriteLine("程序准备就绪，现在开始抓取，请耐心等待抓取完成");
            ArticleListCrawler articleListCrawler = new ArticleListCrawler(CaptureConfig.Func1.CrawlerDepth, CaptureConfig.Func1.RootUrls, CaptureConfig.Func1.NextPageUrlMatchRegexs, CaptureConfig.Func1.ArticleUrlMatchRegexs);
            articleListCrawler.ArticleInfoMatchRegexs.AddRange(CaptureConfig.Func1.ArticleInfoMatchRegexs);
            articleListCrawler.ArticleFileMatchRegexs.AddRange(CaptureConfig.Func1.ArticleFileMatchRegexs);
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
            Console.WriteLine("程序准备就绪，现在随机访问");
            ArticleAccessController articleAccessController =  new ArticleAccessController(CaptureConfig.Func2.AccessNum, CaptureConfig.Func2.AccessContinueTime, CaptureConfig.Func2.DayAccessStartTime);
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

            int currentAccessedNum = 0;
            while (true) {
                if (task.Status == TaskStatus.RanToCompletion) {
                    break;
                }

                int sleepTime = CaptureConfig.Func2.AccessContinueTime / 50;
                if (sleepTime < 5) {
                    sleepTime = 5;
                }

                if (currentAccessedNum != articleAccessController.AccessedNum) {
                    currentAccessedNum = articleAccessController.AccessedNum;
                    Console.WriteLine($"已访问文章量：{articleAccessController.AccessedNum}");
                }

                Thread.Sleep(sleepTime * 1000);
            }
        }
    }
}
