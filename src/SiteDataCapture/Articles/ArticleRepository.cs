using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace SiteDataCapture.Articles
{
    public class ArticleRepository
    {
        private const string _splitString = "|-|-|";

        private const string _articlesFileName = "Articles.txt";

        private List<Article> _articles { get; set; }

        public IEnumerable<Article> Articles => _articles;

        public ArticleRepository() {
            _articles = GetArticlesFromFile();
        }

        public void Insert(Article article) {
            _articles.Add(article);
        }

        public void SaveArticlesToFile() {
            StringBuilder fileString = new StringBuilder();
            _articles.ForEach(article =>
            {
                fileString.Append(CreateArticleStr(article));
                fileString.Append("\n");
            });

            File.WriteAllText(_articlesFileName, fileString.ToString());
        }

        private List<Article> GetArticlesFromFile() {
            List<Article> articles = new List<Article>();

            if (!File.Exists(_articlesFileName)) {
                return articles;
            }

            foreach (string articleStr in File.ReadAllLines(_articlesFileName)) {
                if (string.IsNullOrWhiteSpace(articleStr)) {
                    continue;
                }

                articles.Add(CreateArticle(articleStr));
            }

            return articles;
        }

        private Article CreateArticle(string articleStr)
        {
            var infos = articleStr.Split(_splitString);

            string[] articleInfos = infos.Skip(1).ToArray();

            Article article = new Article(infos[0], articleInfos);

            return article;
        }

        private string CreateArticleStr(Article article) 
        {
            if (article.ArticleInfos.Count() == 0) {
                return article.Url;
            }

            return article.Url + _splitString + string.Join(_splitString, article.ArticleInfos);
        }
    }
}
