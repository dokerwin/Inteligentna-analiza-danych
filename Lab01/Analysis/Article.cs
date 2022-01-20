using System;
using System.Collections.Generic;
using System.Linq;

using Lab01.Data;
using Porter2Stemmer;

namespace Lab01.Analysis
{
    public class Article
    {
        public Place Place { get; private set; }
        public string Text { get; private set; }

        public List<string> Words { get; set; }

        public Article(Place place, String text)
        {
            Place = place;
            Text = text;
        }

        public int Length { get { return Text.Length; } }

        public int WordCount { get { return Words.Count; } }

        // TODO: Implement the rest of estimation params here

        public int MonthNamesCount
        {
            get
            {
                return Words.Where(w => Month.IsMonth(w)).Count();
            }
        }

        public int YearNumberCount
        {
            get
            {
                return Words.Where(w => Year.IsYear(Convert.ToInt32(w))).Count();
            }
        }

        public int AbbreviationCount
        {
            get
            {
                return Words.Where(w => Abbreviation.IsAbbreviation(w)).Count();
            }
        }



        // Static
        public static bool SelectArticle(ArticlesREUTERS article)
        {
            return article.PLACES.Length == 1 &&
                   Places.CheckTag(article.PLACES[0]) &&
                   article.TEXT.BODY != null;
        }

        public static Article CreateArticle(ArticlesREUTERS articleRaw)
        {
            return new Article(new Place(articleRaw.PLACES[0]), articleRaw.TEXT.BODY);
        }

        public static List<string> getTokensFromArticle(Article article)
        {
            string textFromArticle = article.Text;
            string refactoringTextFromArticle = Util.RefactoringText(textFromArticle);

            var stemmer = new EnglishPorter2Stemmer();

            string stemmed = stemmer.Stem(refactoringTextFromArticle).Value;

            return stemmed.Split(' ').ToList();
        }

        public static Article getRandomArticle(List<Article> articles)
        {
            return articles.ElementAt(new Random(DateTime.Now.Millisecond).Next(articles.Count()));
        }
        public static List<Article> getArticleForLearning(List<Article> articles, int percent)
        {
            List<Article> articlesForLearning = new List<Article>();

            for (int i = 0; i < articles.Count() / 100 * percent; i++)
            {
                articlesForLearning.Add(articles.ElementAt(i));
            }

            return articlesForLearning;
        }

        public static List<Article> getArticleForTesting(List<Article> articles, int percent)
        {
            List<Article> articlesForTesting = new List<Article>();

            for (int i = articles.Count() / 100 * (100 - percent); i < articles.Count(); i++)
            {
                articlesForTesting.Add(articles.ElementAt(i));
            }

            return articlesForTesting;
        }


        public static List<Article> GetArticlesFromData(Dictionary<string, Articles> data)
        {
            List<Article> articles = new List<Article>();
            var summaryFreqs = new Frequencies(Places.All);

            foreach (var keyValuePair in data)
            {
                var file = keyValuePair.Key;
                var articlesFromData = keyValuePair.Value;

                var freqs = new Frequencies(Places.All);

             //   Console.WriteLine("[" + file + "]: Znaleziono " + articlesFromData.REUTERS.Length + "artykułów.");

                foreach (var articleRaw in articlesFromData.REUTERS)
                {
                    if (Article.SelectArticle(articleRaw))
                    {
                        var article = Article.CreateArticle(articleRaw);
                        article.Words = Article.getTokensFromArticle(article);

                        freqs.Increment(article.Place);
                        articles.Add(article);
                    }
                }

                summaryFreqs.ReduceWith(freqs);

               // Console.WriteLine("[" + file + "]: Znaleziono " + articlesFromData.REUTERS.Length + "artykułów.");
            }

            return articles;
        }

    }
}
