using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lab01.Analysis
{
    class Vector
    {
        private string country;

        public string GetCountry()
        {
            return this.country;
        }

        private void SetCountry(string value)
        {
            this.country = value;
        }

        public Dictionary<string, int> Characteristic { get; private set; }
        public Vector(string country, Dictionary<string, int> characteristic)
        {
            this.country = country;
            this.Characteristic = characteristic;
        }


        //static 
        public static Vector getRandomVector(List<Vector> vectors)
        {
            return vectors.ElementAt(new Random(DateTime.Now.Millisecond).Next(vectors.Count()));
        }


        public static List<Vector> getVectrorsFromArticles(List<Article> articles, List<Country_Dictionary> DictionaryAllCountryCollection)
        {
            List<Vector> vectors = new List<Vector>();

            foreach (Article article in articles)
            {
                vectors.Add(createVectorForArticle(article, DictionaryAllCountryCollection));
            }

            return vectors;
        }

        public static Vector createVectorForArticle(Article article, List<Country_Dictionary> DictionaryAllCountryCollection)
        {
            Dictionary<string, int> characteristic = new Dictionary<string, int>();

            foreach (var Dictionary in DictionaryAllCountryCollection)
            {
                characteristic.Add(Dictionary.PlaceName + "dictionary", 0);
            }

            foreach (var Dictionary in DictionaryAllCountryCollection)
            {
                characteristic[Dictionary.PlaceName + "dictionary"] = Util.CountCoincidence(article.Words, Dictionary);
            }

            return new Vector(article.Place.Tag, characteristic);
        }
    }

}