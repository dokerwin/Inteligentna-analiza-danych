using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;
using System.Text.RegularExpressions;

using Lab01.Analysis;
using Lab01.Data;
using static Lab01.Util;
using Porter2Stemmer;
using System.Linq;

namespace Lab01
{
    class Program
    {
        // directory with text files
        public const string BasePath = @"C:\Analize\FilesToAnalize";
        private static List<Country_Dictionary> DictionaryAllCountryCollection = new List<Country_Dictionary>();


        static Dictionary<string, Articles> ReadArticleFiles(string basePath)
        {
            var match = new Regex(@"reut.*\.sgm");

            var result = new Dictionary<string, Articles>();

            var filesCount = Directory.GetFiles(basePath).Length;
            var counter = 0;

            Console.WriteLine("Reading files...");

            foreach (var file in Directory.EnumerateFiles(basePath))
            {
                if (counter > 0)
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                }
                counter++;
                Console.WriteLine(String.Format("{0:0.0}", (double)counter / filesCount * 100) + "%");

                if (match.IsMatch(Path.GetFileName(file)))
                {
                    // Read XML
                    string xmlString = File.ReadAllText(file);
                    xmlString = ReplaceHexSymbols(
                            "<Articles>" +
                            xmlString.Replace("<!DOCTYPE lewis SYSTEM \"lewis.dtd\">", string.Empty) +
                            "</Articles>"
                        );
                    XmlSerializer serializer = new XmlSerializer(typeof(Articles), new XmlRootAttribute("Articles"));

                    StringReader sRead = new StringReader(xmlString);

                    Articles articles = (Articles)serializer.Deserialize(sRead);

                    result.Add(Path.GetFileName(file), articles);
                }
            }

            return result;
        }

        static void Main(string[] args)
        {
            var summaryFreqs = new Frequencies(Places.All);
            try
            {
                Dictionary<string, Dictionary<string, int>> coinsidence = new Dictionary<string, Dictionary<string, int>>();
                var data = ReadArticleFiles(BasePath);

                List<Article> articles = GetArticlesFromData(data);

                List<Article> articlesForLearning = getArticleForLearning(articles, 50);
                List<Article> articlesForTesting = getArticleForTesting(articles, 50);

                DictionaryAllCountryCollection = CreateDictionarsFromArticles(articlesForLearning);

                List<Vector> vectorsFromLearningArticles = getVectrorsFromArticles(articlesForLearning);


                Vector randVector = getRandomVector(vectorsFromLearningArticles);

                analyzise(3,vectorsFromLearningArticles, randVector);


                Article artic = getRandomArticle(articlesForTesting);
                Console.WriteLine("Try to parse:" + artic.Place.Tag);

                Console.WriteLine("What is it country ? -It is: " + IsItCountry(getTokensFromArticle(artic), "usa"));


                // Display summary
                Console.WriteLine("-------------------------------");
                Console.WriteLine("\n");
                Console.WriteLine("Podsumowanie");
                foreach (var tagCountPair in summaryFreqs.Data)
                {
                    Console.WriteLine("     " + tagCountPair.Key + ": " + tagCountPair.Value);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error reading files: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unknown error occurred: " + ex.Message);
            }
        }

        public static void analyzise(int k, List<Vector> vectorsFromLearningArticles, Vector vectorFromAnalyzise)
        {
            Dictionary<string, int> countryMetric = new Dictionary<string, int>();
            List<int> vectorAnalize = new List<int>();

            foreach (var Abbreviation in vectorFromAnalyzise.Characteristic)
            {
                vectorAnalize.Add(Abbreviation.Value);
            }

            foreach (var vectors in vectorsFromLearningArticles)
            {
                List<int> vectorLearning = new List<int>();
                foreach (var Abbreviation in vectors.Characteristic)
                {
                    vectorAnalize.Add(Abbreviation.Value);
                    countryMetric.Add(vectors.GetCountry() ,analysisEuclideaMetric(vectorLearning, vectorAnalize));
                }
            }

            var sortedDict = (from entry in countryMetric orderby entry.Value ascending select entry)
            .ToDictionary(pair => pair.Key, pair => pair.Value).Take(k);


            foreach (var res in sortedDict)
            {
                Console.WriteLine("Country: " + res.Key + " Value: " + res.Value);
            }

        }

        public static int analysisEuclideaMetric(List<int> vectorLearning, List<int> vectorAnalize)
        {
            int square = 0;

            for (int i = 0; i < vectorLearning.Count; i++)
            {
                square += (vectorLearning.ElementAt(i) - vectorAnalize.ElementAt(i)) * (vectorLearning.ElementAt(i) - vectorAnalize.ElementAt(i));
            }

            return Convert.ToInt32(Math.Sqrt(square));
        }

        public static int analysisStreetMetric(List<int> vectorLearning, List<int> vectorAnalize)
        {
            int result = 0;
            return result;
        }

        public static int analysisChebyshevMetric(List<int> vectorLearning, List<int> vectorAnalize)
        {
            int result = 0;

            for (int i = 0; i < vectorLearning.Count; i++)
            {
                int curentResult = Math.Abs(vectorAnalize.ElementAt(i) - vectorLearning.ElementAt(i)); 
                if (curentResult > result)
                {
                    result = curentResult;
                }
            }

            return result;
        }

        private static List<Article> getArticleForLearning(List<Article> articles, int percent)
        {
            List<Article> articlesForLearning = new List<Article>();

            for (int i = 0; i < articles.Count() / 100 * percent; i++)
            {
                articlesForLearning.Add(articles.ElementAt(i));
            }

            return articlesForLearning;
        }

        private static List<Article> getArticleForTesting(List<Article> articles, int percent)
        {
            List<Article> articlesForTesting = new List<Article>();

            for (int i = articles.Count() / 100 * (100 - percent); i < articles.Count(); i++)
            {
                articlesForTesting.Add(articles.ElementAt(i));
            }

            return articlesForTesting;
        }

        private static List<Article> GetArticlesFromData(Dictionary<string, Articles> data)
        {
            List<Article> articles = new List<Article>();
            var summaryFreqs = new Frequencies(Places.All);

            foreach (var keyValuePair in data)
            {
                var file = keyValuePair.Key;
                var articlesFromData = keyValuePair.Value;

                var freqs = new Frequencies(Places.All);

                Console.WriteLine("[" + file + "]: Znaleziono " + articlesFromData.REUTERS.Length + "artykułów.");

                foreach (var articleRaw in articlesFromData.REUTERS)
                {
                    if (Article.SelectArticle(articleRaw))
                    {
                        var article = Article.CreateArticle(articleRaw);
                        //article.Words = getTokensFromArticle(article);

                        freqs.Increment(article.Place);
                        articles.Add(article);
                    }
                }

                summaryFreqs.ReduceWith(freqs);

                Console.WriteLine("[" + file + "]: Znaleziono " + articlesFromData.REUTERS.Length + "artykułów.");
            }

            return articles;
        }

        private static List<Country_Dictionary> CreateDictionarsFromArticles(List<Article> articles)
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

        private static string RefactoringText(string text)
        {
            char[] charsToTrim = { '*', '"', '@', '#', '%', '&', '$', '>', '<' };
            string refactoringText = text.Replace(Environment.NewLine, " ");
            refactoringText = refactoringText.Replace("\n", " ");
            refactoringText = refactoringText.Replace("\r\n", " ");
            refactoringText = refactoringText.Trim(charsToTrim);

            refactoringText = new string(refactoringText.Where(c => !char.IsPunctuation(c)).ToArray());
            refactoringText = new string(refactoringText.Where(c => !char.IsDigit(c)).ToArray());
            refactoringText = new string(refactoringText.Where(c => !char.IsNumber(c)).ToArray());

            return refactoringText;
        }

        private static int CountCoincidence(List<String> toResearch, Country_Dictionary country_Dictionary)
        {

            int counter = 0;

            foreach (string word in toResearch)
            {
                if (country_Dictionary.Words.Contains(word))
                {
                    counter++;
                }
            }

            return counter;
        }

        private static string IsItCountry(List<String> toResearch, string country)
        {

            Dictionary<string, int> coincedence = new Dictionary<string, int>();


            foreach (var place in DictionaryAllCountryCollection)
            {
                coincedence.Add(place.PlaceName, 0);
            }

            foreach (var sd in DictionaryAllCountryCollection)
            {
                coincedence[sd.PlaceName] = CountCoincidence(toResearch, sd);
                Console.WriteLine("Place:" + sd.PlaceName + " Coincedence: " + coincedence[sd.PlaceName]);
            }

            return coincedence.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        }

        public static List<string> getTokensFromArticle(Article article)
        {
            string textFromArticle = article.Text;
            string refactoringTextFromArticle = RefactoringText(textFromArticle);

            var stemmer = new EnglishPorter2Stemmer();

            string stemmed = stemmer.Stem(refactoringTextFromArticle).Value;

            return stemmed.Split(' ').ToList();
        }

        public static Article getRandomArticle(List<Article> articles)
        {
            return articles.ElementAt(new Random(DateTime.Now.Millisecond).Next(articles.Count()));
        }


        public static Vector getRandomVector(List<Vector> vectors)
        {
            return vectors.ElementAt(new Random(DateTime.Now.Millisecond).Next(vectors.Count()));
        }
       

        public static List<Vector> getVectrorsFromArticles(List<Article> articles)
        {
            List<Vector> vectors = new List<Vector>();

            foreach (Article article in articles)
            {
                vectors.Add(createVectorForArticle(article));
            }

            return vectors;
        }

        public static Vector createVectorForArticle(Article article)
        {
            Dictionary<string, int> characteristic = new Dictionary<string, int>();

            foreach (var Dictionary in DictionaryAllCountryCollection)
            {
                characteristic.Add(Dictionary.PlaceName + "dictionary", 0);
            }

            foreach (var Dictionary in DictionaryAllCountryCollection)
            {
                characteristic[Dictionary.PlaceName + "dictionary"] = CountCoincidence(article.Words, Dictionary);
            }

            return new Vector(article.Place.Tag, characteristic);
        }
    }
}