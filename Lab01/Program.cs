using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;
using System.Text.RegularExpressions;

using Lab01.Analysis;
using Lab01.Data;
using static Lab01.Util;
using static Lab01.Analysis.CountryDictionaryUtil;
using static Lab01.Metrics;
using Porter2Stemmer;
using System.Linq;
using System.Threading;

namespace Lab01
{
    class Program
    {
        // directory with text files
        public const string BasePath = @"C:\Users\nikna\source\repos\FilesToAnalize";
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

                List<Article> articles = Article.GetArticlesFromData(data);



                List<Article> articlesForLearning = articlesForLearning = getArticleForLearning(articles, 50);
           
                List<Article> articlesForTesting = new List<Article>();
                Thread thread1 = new Thread(() => { articlesForTesting= getArticleForTesting(articles, 50); });
                thread1.Start();

                DictionaryAllCountryCollection = CreateDictionarsFromArticles(articlesForLearning);

                List<Vector> vectorsFromLearningArticles = Vector.getVectrorsFromArticles(articlesForLearning, DictionaryAllCountryCollection);
                List<Vector> vectorsFromTestArticles  =    Vector.getVectrorsFromArticles(articlesForTesting, DictionaryAllCountryCollection);
              
                
                Console.Clear();

                while (true)
                {

                    Vector randVector = Vector.getRandomVector(vectorsFromTestArticles);

                    Console.WriteLine("Please write 'K'");
                    int a = 0;
                    a = Console.Read();
                    if (a > 10 || a <= 0)
                    {
                        Console.WriteLine("Please write correct 'K'");
                    }

                    Console.WriteLine("Try to recognize an arcticlle by alorithhm. The rael text place is: " + randVector.GetCountry());

                    analyzise(Metrics.metrics.ChebyshevMetric, a, vectorsFromLearningArticles, randVector);


                    Console.WriteLine("\n\n");
                    Console.WriteLine("Try to recognize another random text? Yes enter 1\nExit enter 2");

                    if (Console.ReadLine() == "2")
                    {
                        break;
                    }
                }

                //Article artic = getRandomArticle(articlesForTesting);
                // Console.WriteLine("Try to parse:" + artic.Place.Tag);
                //Console.WriteLine("What is it country ? -It is: " + IsItCountry(getTokensFromArticle(artic), "usa"));

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

        public static void analyzise(Metrics.metrics metrics, int k, List<Vector> vectorsFromLearningArticles, Vector vectorFromAnalyzise)
        {
            List<Tuple<Vector, int>> countryMetric = new List<Tuple<Vector, int>>();
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
                    vectorLearning.Add(Abbreviation.Value);
                }

                switch (metrics)
                {
                    case metrics.ChebyshevMetric:
                        {
                            var a = new Tuple<Vector, int>(vectors, Metrics.analysisChebyshevMetric(vectorLearning, vectorAnalize));
                            countryMetric.Add(a);
                            break;

                        }
                    case metrics.EuclideaMetric:
                        {
                            var a = new Tuple<Vector, int>(vectors, Metrics.analysisEuclideaMetric(vectorLearning, vectorAnalize));
                            countryMetric.Add(a);
                            break;

                        }
                    case metrics.StreetMetric:
                        {
                            var a = new Tuple<Vector, int>(vectors, Metrics.analysisStreetMetric(vectorLearning, vectorAnalize));
                            countryMetric.Add(a);
                            break;
                        }
                }
               
            }

            Dictionary<string, int> distanceK = new Dictionary<string, int>();
            foreach (var country in Places.All)
            {
                var maxNumber = countryMetric.OrderBy(x => x.Item2).Where(x => x.Item1.GetCountry() == country.Tag).Take(k); //only take 1 item
                foreach (var m in maxNumber)
                {
                    if (distanceK.ContainsKey(m.Item1.GetCountry()))
                    {
                        if (distanceK[m.Item1.GetCountry()] > m.Item2)
                              distanceK[m.Item1.GetCountry()] = m.Item2;
                    }
                    else
                    {
                        distanceK.Add(m.Item1.GetCountry(), m.Item2);
                    }
                }
            }
            var minDistance = distanceK.OrderBy(x => x.Value).Take(k);

            Console.WriteLine("Used metric: " + metrics.ToString());
            foreach (var m in minDistance)
            {
                Console.WriteLine("Country: " + m.Key + " Distance: " + m.Value);
            }
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




    }
}