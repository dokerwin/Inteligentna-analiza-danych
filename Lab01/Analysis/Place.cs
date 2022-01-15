using System;
using System.Linq;

namespace Lab01.Analysis
{
    public class Place
    {
        public string Tag { get; private set; }

        public Place(string place)
        {
            Tag = place;
        }

        public override bool Equals(Object obj)
        {
            Place another = obj as Place;
            if (another == null)
            {
                throw new Exception("Invalid PlaceTag provided in Equals()");
            }

            return Tag.Equals(another.Tag);
        }

        public override int GetHashCode()
        {
            return Tag.GetHashCode();
        }

        public override string ToString()
        {
            return Tag;
        }
    }
}
