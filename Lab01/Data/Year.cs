using System;
using System.Collections.Generic;
using System.Text;

namespace Lab01.Data
{
    public static class Year
    {
        public static Func<int, bool> IsYear = (yearValue) => 1890 < yearValue && yearValue < 2021;
    }
}
