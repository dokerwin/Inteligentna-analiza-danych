using System;
using System.Collections.Generic;
using System.Text;

namespace Lab01.Analysis
{
    class CountryDictionaryUtil
    {
        public static List<Country_Dictionary> CreateDictionarsFromArticles(List<Article> articles)
        {
            Dictionary<string, List<string>> country_Dictionaries = new Dictionary<string, List<string>>();

            foreach (Article article in articles)
            {
                foreach (var word in article.Words)
                {
                    if (word.Length <= 3)
                    {
                        continue;
                    }

                    if (country_Dictionaries.ContainsKey(article.Place.Tag))
                    {
                        if (!country_Dictionaries[article.Place.Tag].Contains(word))
                        {
                            country_Dictionaries[article.Place.Tag].Add(word);
                        }
                    }
                    else
                    {
                        country_Dictionaries.Add(article.Place.Tag, new List<string> { word });
                    }
                }
            }

            List<Country_Dictionary> temp = new List<Country_Dictionary>();
            foreach (var a in country_Dictionaries)
            {
                temp.Add(new Country_Dictionary(a.Key, a.Value));
            }

            return temp;
        }
    }
}
