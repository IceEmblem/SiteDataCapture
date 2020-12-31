using System;
using System.Collections.Generic;
using System.Text;

namespace SiteDataCapture.Articles
{
    public class Article
    {
        public string Url { get; protected set; }

        public string[] ArticleInfos { get; protected set; }

        public Article(string url, string[] articleInfos) {
            Url = url;
            ArticleInfos = articleInfos;
        }
    }
}
