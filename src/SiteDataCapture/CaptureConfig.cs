using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SiteDataCapture
{
    public class CaptureConfig
    {
        public int SelectFunc { get; set; }

        public Func1 Func1 { get; set; }

        public Func2 Func2 { get; set; }

        public CaptureConfig() {
            Func1 = new Func1();
            Func2 = new Func2();
        }

        public CaptureConfig(IConfigurationRoot config) {
            SelectFunc = Convert.ToInt32(config["SelectFunc"]);
            Func1 = new Func1(config.GetSection("Func1"));
            Func2 = new Func2(config.GetSection("Func2"));
        }
    }

    public class Func1 
    { 
        public int CrawlerDepth { get; set; }

        public List<string> RootUrls { get; set; } = new List<string>();

        public List<Regex> NextPageUrlMatchRegexs { get; set; } = new List<Regex>();

        public List<Regex> ArticleUrlMatchRegexs { get; set; } = new List<Regex>();

        public List<Regex> ArticleInfoMatchRegexs { get; set; } = new List<Regex>();

        public List<Regex> ArticleFileMatchRegexs { get; set; } = new List<Regex>();

        public Func1() { }

        public Func1(IConfiguration config)
        {
            CrawlerDepth = Convert.ToInt32(config["CrawlerDepth"]);

            foreach (var item in config.GetSection("RootUrls").GetChildren()) {
                RootUrls.Add(item.Value);
            }

            foreach (var item in config.GetSection("NextPageUrlMatchRegexs").GetChildren())
            {
                NextPageUrlMatchRegexs.Add(new Regex(item.Value));
            }

            foreach (var item in config.GetSection("ArticleUrlMatchRegexs").GetChildren())
            {
                ArticleUrlMatchRegexs.Add(new Regex(item.Value));
            }

            foreach (var item in config.GetSection("ArticleInfoMatchRegexs").GetChildren())
            {
                ArticleInfoMatchRegexs.Add(new Regex(item.Value));
            }

            foreach (var item in config.GetSection("ArticleFileMatchRegexs").GetChildren())
            {
                ArticleFileMatchRegexs.Add(new Regex(item.Value));
            }
        }
    }

    public class Func2 { 
        public int AccessNum { get; set; }

        public int AccessContinueTime { get; set; }

        public int DayAccessStartTime { get; set; }

        public Func2() { }

        public Func2(IConfiguration config)
        {
            AccessNum = Convert.ToInt32(config["AccessNum"]);
            AccessContinueTime = Convert.ToInt32(config["AccessContinueTime"]);
            DayAccessStartTime = Convert.ToInt32(config["DayAccessStartTime"]);
        }
    }
}
