using Lab01.Analysis;
using Lab01.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lab01
{
    public class Characteristics
    {

        public static Dictionary<Place, List<string>> characteristic = new Dictionary<Place, List<string>>
        {
            { Places.Canada,        new Characteristic(Places.Canada).getCharactiristic()},
            { Places.France,        new Characteristic(Places.France).getCharactiristic()},
            { Places.UnitedKingdom, new Characteristic(Places.UnitedKingdom).getCharactiristic()},
            { Places.UnitedStates,  new Characteristic(Places.UnitedStates).getCharactiristic()},
            { Places.WestGermany,   new Characteristic(Places.Germany).getCharactiristic()},
            { Places.Japan,         new Characteristic(Places.Japan).getCharactiristic()},
        };


    }
}
