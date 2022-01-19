using Lab01.Analysis;
using Nager.Country;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lab01.Data
{
    public  class Characteristic
    {

        public Characteristic(Place place)
        {
            this.place = place;

        }
        public Place place { set; get; }
        private  List<string> features { set; get; }

        public List<string> getCharactiristic()
        {
            ICountryProvider countryProvider = new CountryProvider();
            var countryInfo = countryProvider.GetCountryByName(place.Tag);

            foreach (var Abbreviation in countryInfo.Currencies)
            {
                features.Add(Abbreviation.Name);
            }


            foreach (var Abbreviation in countryInfo.Currencies)
            {
                features.Add(Abbreviation.IsoCode);
            }

            features.Add(countryInfo.CommonName);
            features.Add(countryInfo.NativeName);
            features.Add(countryInfo.OfficialName);
            foreach (var Abbreviation in countryInfo.CallingCodes)
            {
                features.Add(Abbreviation);
            }


            return features;

        }







    }
}
