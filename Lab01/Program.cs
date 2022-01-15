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
                Console.WriteLine(String.Format("{0:0.0}", (double) counter / filesCount * 100) + "%");

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
            // directory with text files
            var basePath = @"C:\Users\nikna\source\repos\FilesToAnalize";
            var outputDir = @"C:\Users\nikna\source\repos\output";

            CreateOrEmptyDir(outputDir);

            var summaryFreqs = new Frequencies(Places.All);
            try
            {
                Dictionary<string, Dictionary<string, int>> coinsidence = new Dictionary<string, Dictionary<string, int>>();
                var data = ReadArticleFiles(basePath);

                // Prepare articles with specific tag frequency table

                foreach (var keyValuePair in data)
                {

                    var file = keyValuePair.Key;
                    var articles = keyValuePair.Value;

                    var freqs = new Frequencies(Places.All);
                    

                    // Display articles count
                    Console.WriteLine("[" + file + "]: Znaleziono " + articles.REUTERS.Length + "artykułów.");

                    foreach (var articleRaw in articles.REUTERS)
                    {
                        // find the articles with exactly 1 tag in countryTags
                        // with non-empty body
                        if (Article.SelectArticle(articleRaw))
                        {
                            var article = Article.CreateArticle(articleRaw);
                            freqs.Increment(article.Place);

                            StringBuilder builder = new StringBuilder();

                            String path = Path.Join(outputDir, $"{file}_output.txt");
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
                            string a = article.Text;

                            string b = new string(a.Where(c => !char.IsPunctuation(c)).ToArray());
                            var stemmer = new EnglishPorter2Stemmer();

                            string stemmed = stemmer.Stem(a).Value;
                            List<string> list = new List<string>();
                            foreach (var s in stemmed.Split(' ').ToList())
                            {
                                if(s.Length > 1)
                                {
                                    list.Add(s);
                                }
                            }

                            var result = list.GroupBy(x => x)
                                               .ToDictionary(y => y.Key, y => y.Count())
                                               .OrderByDescending(z => z.Value);

                            List<int> array = new List<int>();


                            if (coinsidence.ContainsKey(article.Text))
                            {
                               
                                foreach(var d in (Dictionary<string, int>)result )
                                {
                                    /// тут треба зробити векторизацію
                                    /// 
                                }

                            }
                            else
                            {
                                coinsidence.Add(article.Text, (Dictionary<string, int>)result);
                            }


                            // list =  list.Distinct().ToList();

                            builder.Append("\n");
                            builder.Append("\n");

                            File.AppendAllText(path, builder.ToString(), System.Text.Encoding.UTF8);
                        }
                    }

                    // Display tag count for each file
                    foreach (var tagCountPair in freqs.Data)
                    {
                        Console.WriteLine("     " + tagCountPair.Key + ": " + tagCountPair.Value);
                    }

                    summaryFreqs.ReduceWith(freqs);
                }

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
    }
}
