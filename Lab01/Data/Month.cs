using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lab01.Data
{
    public static class Month
    {
        private static string[] months = new string[] 
        { 
            "january", 
            "february", 
            "march", 
            "april", 
            "may", 
            "july", 
            "juny", 
            "august", 
            "september", 
            "october", 
            "november", 
            "december"
        };

        public static bool IsMonth(string monthName)
        {
            return months.Any(m => m.Equals(monthName.ToLower()));
        }


        



    }
}
