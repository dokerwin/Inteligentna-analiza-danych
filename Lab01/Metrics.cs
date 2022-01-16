using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lab01
{
    class Metrics
    {

        public static int analysisEuclideaMetric(List<int> vectorLearning, List<int> vectorAnalize)
        {
            int square = 0;

            for (int i = 0; i < vectorLearning.Count; i++)
            {
                square += (vectorLearning.ElementAt(i) - vectorAnalize.ElementAt(i)) * (vectorLearning.ElementAt(i) - vectorAnalize.ElementAt(i));
            }

            return Convert.ToInt32(Math.Sqrt(square));
        }

        public static int analysisStreetMetric(List<int> vectorLearning, List<int> vectorAnalize)
        {
            int result = 0;

            for (int i = 0; i < vectorLearning.Count; i++)
            {
                result += Math.Abs(vectorLearning.ElementAt(i) - vectorAnalize.ElementAt(i));
            }

            return result;
        }

        public static int analysisChebyshevMetric(List<int> vectorLearning, List<int> vectorAnalize)
        {
            int result = 0;

            for (int i = 0; i < vectorLearning.Count; i++)
            {
                int curentResult = Math.Abs(vectorLearning.ElementAt(i) - vectorAnalize.ElementAt(i));
                if (curentResult > result)
                {
                    result = curentResult;
                }
            }

            return result;
        }

    }
}
