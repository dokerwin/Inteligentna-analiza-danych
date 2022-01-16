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
        public const string BasePath = @"C:\Users\nikna\source\repos\FilesToAnalize";
        public const string OutputDir = @"C:\Users\nikna\source\repos\output";
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

        static void CreateOrEmptyDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                var match = new Regex(@"_output\.txt");
                foreach (var file in Directory.EnumerateFiles(path))
                {
                    if (match.IsMatch(Path.GetFileName(file)))
                    {
                        File.Delete(file);
                    }
                }
            }
        }


       
        static void Main(string[] args)
        {
            CreateOrEmptyDir(OutputDir);

            var summaryFreqs = new Frequencies(Places.All);
            try
            {
                Dictionary<string, Dictionary<string, int>> coinsidence = new Dictionary<string, Dictionary<string, int>>();
                var data = ReadArticleFiles(BasePath);

                List<Article> articles = GetArticlesFromData(data);

                List<Article> articlesForLearning = getArticleForLearning(articles, 50);
                List<Article> articlesForTesting = getArticleForTesting(articles, 50);

                DictionaryAllCountryCollection = CreateDictionarsFromArticles(articlesForLearning);

                Article artic = getRandomArticle(articlesForLearning);
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
                        freqs.Increment(article.Place);

                        StringBuilder builder = new StringBuilder();

                        String path = Path.Join(OutputDir, $"{file}_output.txt");
                        String separator = "\n_________\n";

                        builder.Append("Article: ");
                        builder.Append(separator);
                        builder.Append("Place: ");
                        builder.Append(article.Place);
                        builder.Append(separator);
                        builder.Append("Month name words count: ");
                        builder.Append(article.MonthNamesCount);
                        builder.Append(separator);
                        builder.Append("Abbreviation count: ");
                        builder.Append(article.AbbreviationCount);
                        builder.Append(separator);
                        builder.Append(article.Text);

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
                string textFromArticle = article.Text;
                string refactoringTextFromArticle = RefactoringText(textFromArticle);

                var stemmer = new EnglishPorter2Stemmer();

                string stemmed = stemmer.Stem(refactoringTextFromArticle).Value;

                foreach (var word in stemmed.Split(' ').ToList())
                {
                    if (word.Length <= 3)
                    {
                        continue;
                    }
                    if (country_Dictionaries.ContainsKey(article.Place.Tag))   //.Words.Contains(word))
                    {
                        if (!country_Dictionaries[article.Place.Tag].Contains(word))
                        {
                            country_Dictionaries[article.Place.Tag].Add(word);
                        }
                    }
                    else 
                    {
                        country_Dictionaries.Add(article.Place.Tag, new List<string> {word});
                    }
                }
            }

            List<Country_Dictionary> temp = new List<Country_Dictionary>();
            foreach(var a in country_Dictionaries)
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

        private static int CountCoincidence(List<String> toResearch  ,Country_Dictionary country_Dictionary)
        {
           
            int counter = 0;
            
            foreach ( string word in toResearch) 
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


            foreach(var place in DictionaryAllCountryCollection )
            {
                coincedence.Add(place.PlaceName, 0);
            }

            foreach (var countryDictionary in DictionaryAllCountryCollection)
            {
                coincedence[countryDictionary.PlaceName] = CountCoincidence(toResearch, countryDictionary);
                Console.WriteLine("Place:"+ countryDictionary.PlaceName + " Coincedence: " + coincedence[countryDictionary.PlaceName]);
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







    }
}