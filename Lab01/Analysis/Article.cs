using System;
using System.Collections.Generic;
using System.Linq;

using Lab01.Data;

namespace Lab01.Analysis
{
    public class Article
    {
        public Place Place { get; private set; }
        public string Text { get; private set; }

        public List<string> Words 
        { 
            get
            {
                return Text.Split(" ").ToList();
            }
        }

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
    }
}
