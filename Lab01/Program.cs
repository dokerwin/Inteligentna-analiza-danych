//#define TEST
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
using Nager.Country;

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
                ICountryProvider countryProvider = new CountryProvider();
                var countryInfo = countryProvider.GetCountryByName("United Kingdom");

                foreach (var Abbreviation in countryInfo.Currencies)
                {

                }


                while (true)
                {




                    int learn = 0;
                    int research = 0;
                    Console.WriteLine("Repeat?");
                    int repeat = 0;
                    while (!Int32.TryParse(Console.ReadLine(), out repeat))
                    {
                        Console.WriteLine("Wrong input! Enter choice number again:");
                    }
                    if (repeat == 9)
                    {
                        break;
                    }

                    else
                    {
                        Console.WriteLine("Please write percent of text for learning and researching");
                        Console.WriteLine("Learn - :");
                 
                        while (!Int32.TryParse(Console.ReadLine(), out learn))
                        {
                            Console.WriteLine("Wrong input! Enter choice number again:");
                        }
                        Console.WriteLine("Research - :");
                        while (!Int32.TryParse(Console.ReadLine(), out research))
                        {
                            Console.WriteLine("Wrong input! Enter choice number again:");
                        }
                    }

               

                    Dictionary<string, Dictionary<string, int>> coinsidence = new Dictionary<string, Dictionary<string, int>>();
                    var data = ReadArticleFiles(BasePath);

                    List<Article> articles = Article.GetArticlesFromData(data);



                    List<Article> articlesForLearning = articlesForLearning = Article.getArticleForLearning(articles, learn);

                    List<Article> articlesForTesting = new List<Article>();
                    Thread thread1 = new Thread(() => { articlesForTesting = Article.getArticleForTesting(articles, research); });
                    thread1.Start();

                    DictionaryAllCountryCollection = CreateDictionarsFromArticles(articlesForLearning);

                    List<Vector> vectorsFromLearningArticles = Vector.getVectrorsFromArticles(articlesForLearning, DictionaryAllCountryCollection);
                    List<Vector> vectorsFromTestArticles = Vector.getVectrorsFromArticles(articlesForTesting, DictionaryAllCountryCollection);




                    Console.WriteLine("//////////////////////////PRECISION////////////////////////////////");




                    Dictionary<string, double> precis = new Dictionary<string, double>
                {
                    { metrics.ChebyshevMetric.ToString(), GetPrecision(metrics.ChebyshevMetric, vectorsFromTestArticles, vectorsFromLearningArticles) },

                    {  metrics.EuclideaMetric.ToString(),  GetPrecision(metrics.EuclideaMetric, vectorsFromTestArticles, vectorsFromLearningArticles) },

                    { metrics.StreetMetric.ToString(), GetPrecision(metrics.StreetMetric, vectorsFromTestArticles, vectorsFromLearningArticles) }

                };

                    var bestPrecision = precis.Where(e => e.Value == precis.Max(e2 => e2.Value)).First();

                    Console.WriteLine("The best algorithm is: " + bestPrecision.Key + " Precision is: " + bestPrecision.Value);
                    Console.WriteLine("///////////////////////////////////////////////////////////////////");

                    Console.WriteLine("\n\n");




                    Console.WriteLine("//////////////////////////RECAL/////////////////////////////////");
                    List<Tuple<string, double>> AVGReacal = new List<Tuple<string, double>>();
                    foreach (var place in Places.All)
                    {
                        Console.WriteLine("/////////////////////////Recal for place: " + place.Tag + " /////////////////////////////");
                        Dictionary<string, double> recall = new Dictionary<string, double>
                {
                    { metrics.ChebyshevMetric.ToString(), Math.Round(GetRecall(5,metrics.ChebyshevMetric, vectorsFromTestArticles, vectorsFromLearningArticles, place.Tag),3) },

                    {  metrics.EuclideaMetric.ToString(),  Math.Round(GetRecall(5,metrics.EuclideaMetric, vectorsFromTestArticles, vectorsFromLearningArticles, place.Tag),3)},

                    { metrics.StreetMetric.ToString(), Math.Round(GetRecall(5,metrics.StreetMetric, vectorsFromTestArticles, vectorsFromLearningArticles, place.Tag) ,3)}

                };
                        AVGReacal.Add(new Tuple<string, double>(recall.Where(e => e.Value == recall.Max(e2 => e2.Value)).First().Key,
                          recall.Where(e => e.Value == recall.Max(e2 => e2.Value)).First().Value));
                        Console.WriteLine("////////////////////////////////////////////////////////////");
                    }
                    var result = AVGReacal.Where(e => e.Item2 == AVGReacal.Max(e2 => e2.Item2)).First();
                    Console.WriteLine("The best algorithm is: " + result.Item1 + "AVG Recall for all countries is: " + result.Item2);
                    Console.WriteLine("////////////////////////////////////////////////////////////////");


                    Console.WriteLine("\n\n");




                    Console.WriteLine("//////////////////////////ACCURACY///////////////////////////////////");
                    Dictionary<string, double> accuracy = new Dictionary<string, double>
                {
                    { metrics.ChebyshevMetric.ToString(), GetAccuracy(metrics.ChebyshevMetric, vectorsFromTestArticles, vectorsFromLearningArticles) },

                    {  metrics.EuclideaMetric.ToString(),  GetAccuracy(metrics.EuclideaMetric, vectorsFromTestArticles, vectorsFromLearningArticles) },

                    { metrics.StreetMetric.ToString(), GetAccuracy(metrics.StreetMetric, vectorsFromTestArticles, vectorsFromLearningArticles) }

                };

                    var bestAccuracy = accuracy.Where(e => e.Value == accuracy.Max(e2 => e2.Value)).First();

                    Console.WriteLine("The best algorithm is: " + bestAccuracy.Key + " Accuracy is: " + bestAccuracy.Value);
                    Console.WriteLine("//////////////////////////////////////////////////////////////////////");
                    Console.WriteLine("\n\n");
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





        private static double GetPrecision(metrics metric, List<Vector> vectorsFromTestArticles, List<Vector> vectorsFromLearningArticles)
        {

            Console.WriteLine("Used metric: " + metric.ToString());

         //   Console.Clear();
            //Precision = TruePositives / (TruePositives + FalsePositives)
            double correct = 0;
            double fall = 0;
            int k = 5;
            foreach (var val in vectorsFromTestArticles)
            {
             
                if (analyzise(metric, k, vectorsFromLearningArticles, val))
                {
                    correct += 1;
                }
                else
                {
                    fall += 1;
                }
            }

            double precision = correct / (correct + fall);

            Console.WriteLine("Precision of " +metric.ToString() + " is: " + Math.Round(precision,4));
            return Math.Round(precision, 4);

        }




        private static double GetRecall(int k, metrics metric, List<Vector> vectorsFromTestArticles, List<Vector> vectorsFromLearningArticles, string place)
        {

            Console.WriteLine("Used metric: " + metric.ToString());
     
            double TruePositive =  0;
            double TrueNegative =  0; 
            double FalseNegative = 0;
       
            foreach (var val in vectorsFromTestArticles)
            {
               Vector vec = Vector.getRandomVector(vectorsFromTestArticles);
                bool predictedCountry = analyzise(metric, k, vectorsFromLearningArticles, vec);
             //   bool predictedCountry =  PredictCounty(metrics.EuclideaMetric, k, vectorsFromLearningArticles, val, place);
                if (predictedCountry) 
                {
                    TruePositive += 1;
                }
                else if (vec.GetCountry() == place)
                {
                        FalseNegative += 1;
                }
                else
                {
                    TrueNegative += 1;
                }
               
            }

            double recall = TruePositive / (TruePositive + FalseNegative);

            Console.WriteLine("Recal of " + metric.ToString() + " is: " + Math.Round(recall, 4));
            return Math.Round(recall,4);

        }







        private static double GetAccuracy(metrics metric, List<Vector> vectorsFromTestArticles, List<Vector> vectorsFromLearningArticles)
        {

            Console.WriteLine("Used metric: " + metric.ToString());

            //   Console.Clear();
            //Precision = TruePositives / (TruePositives + FalsePositives)
            double correct = 0;
            double fall = 0;
            int k = 5;
            foreach (var val in vectorsFromTestArticles)
            {

                if (analyzise(metric, k, vectorsFromLearningArticles, val))
                {
                    correct += 1;
                }
                else
                {
                    fall += 1;
                }
            }
            double precision = 0;
            if (vectorsFromLearningArticles.Count != 0)
            {
                  precision = correct / vectorsFromLearningArticles.Count;
            }
            Console.WriteLine("Accuracy of " + metric.ToString() + " is: " + Math.Round(precision, 4));
            return Math.Round(precision, 4);

        }




















        public static bool analyzise(Metrics.metrics metrics, int k, List<Vector> vectorsFromLearningArticles, Vector vectorFromAnalyzise)
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
                            var a1 = new Tuple<Vector, int>(vectors, Metrics.analysisChebyshevMetric(vectorLearning, vectorAnalize));
                            countryMetric.Add(a1);
                            break;

                        }
                    case metrics.EuclideaMetric:
                        {
                            var a2 = new Tuple<Vector, int>(vectors, Metrics.analysisEuclideaMetric(vectorLearning, vectorAnalize));
                            countryMetric.Add(a2);
                            break;

                        }
                    case metrics.StreetMetric:
                        {
                            var a3 = new Tuple<Vector, int>(vectors, Metrics.analysisStreetMetric(vectorLearning, vectorAnalize));
                            countryMetric.Add(a3);
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
#if TEST
            
            Console.WriteLine("Used metric: " + metrics.ToString());
            foreach (var m in minDistance)
            {
                Console.WriteLine("Country: " + m.Key + " Distance: " + m.Value);
            }
#endif
            KeyValuePair<string, int> result = minDistance.Where(e => e.Value == minDistance.Min(e2 => e2.Value)).First();

            if (result.Key == vectorFromAnalyzise.GetCountry())
            {
                return true;
            }
            else return false;

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