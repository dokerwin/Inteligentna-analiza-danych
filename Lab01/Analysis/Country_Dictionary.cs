using System;
using System.Collections.Generic;
using System.Text;

namespace Lab01.Analysis
{
    class Country_Dictionary
    {

       public  string PlaceName { get; set; }
       public List<string> Words { get; set; }


        public Country_Dictionary()
        {

        }
        public Country_Dictionary(string place,List<string> words)
        {
            PlaceName = place;
            Words = words;
        }

    }
}
