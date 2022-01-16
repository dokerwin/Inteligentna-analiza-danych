using System;
using System.Collections.Generic;
using System.Text;

namespace Lab01.Analysis
{
    class Vector
    {
        private string country;

        public string GetCountry()
        {
            return this.country;
        }

        private void SetCountry(string value)
        {
            this.country = value;
        }

        public Dictionary<string, int> Characteristic { get; private set; }
        public Vector(string country, Dictionary<string, int> characteristic)
        {
            this.country = country;
            this.Characteristic = characteristic;
        }
    }
}