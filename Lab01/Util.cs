using Lab01.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lab01
{
    class Util
    {
        public static string ReplaceHexSymbols(string text)
        {
            return Regex.Replace(text, "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]", "", RegexOptions.Compiled);
        }


        public static string RefactoringText(string text)
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

        public static int CountCoincidence(List<String> toResearch, Country_Dictionary country_Dictionary)
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



    }
}
