using System.Linq;

using Lab01.Analysis;

namespace Lab01.Data
{
    public static class Places
    {
        public static readonly Place WestGermany = new Place("west-germany");
        public static readonly Place Germany = new Place("Germany");
        public static readonly Place USA = new Place("usa");
        public static readonly Place UnitedStates = new Place("UnitedStates");
        public static readonly Place France = new Place("france");
        public static readonly Place UK = new Place("uk");
        public static readonly Place UnitedKingdom = new Place("United Kingdom");
        public static readonly Place Canada = new Place("canada");
        public static readonly Place Japan = new Place("japan");

        public static Place[] All
        {
            get
            {
                return new Place[] { WestGermany, USA, France, UK, Canada, Japan };
            }
        }

        public static bool CheckTag(Place place)
        {
            return All.Any(p => p.Equals(place));
        }

        public static bool CheckTag(string tag)
        {
            return All.Any(t => t.Tag.Equals(tag));
        }
    }
}
