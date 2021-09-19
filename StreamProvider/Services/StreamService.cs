using Models.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamProvider.Services
{
    public static class StreamService
    {

        //test comment
        internal static Random random = new Random();
        public static IEnumerable<Thingy> YieldThingies()
        {
            int count = 0;
            while (count++ < 50000)
            {
                yield return new Thingy() { Value1 = random.Next(), Value2 = (float)random.NextDouble(), Value3 = random.NextDouble(), SomeString = GenerateBiggun() };
            }
        }

        private static string GenerateBiggun()
        {
            int min = 0;
            int max = 500;
            string s = "";
            while (min++ < max)
            {
                s += (char)random.Next(0, 255);
            }
            return s;
        }
    }
}
