using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lab01.Data
{
    public static class Abbreviation
    {
        private static string[] abbrValues = new string[] 
        {
            "Inc",
            "Corp",
            "Co",
            "TV",
            "Ltd",
            "CEO",
            "B2B",
            "ASAP",
            "VAT",
            "PR",
            "LLC",
            "ISP",
            "ETA",
            "QC",
            "QA", 
            "VPN"
        };

        public static bool IsAbbreviation(string abbrValue)
        {
            return abbrValues.Any(abbr => abbr.ToLower().Equals(abbrValue.ToLower()));
        }
    }
}
