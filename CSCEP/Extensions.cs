using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dauros.Timango.CSCEP
{
    public static class Extensions
    {
        public static Double ToUnixTimestamp(this DateTime time)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var timeDiff = time - dtDateTime;
            return (double)timeDiff.TotalSeconds;
        }

        public static DateTime AsUnixTimestamp(this Double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static string ToHexString(this byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        public static Double Median(this IEnumerable<double> set)
        {
            if (set.Count() == 0)
                throw new InvalidOperationException("Set is empty.");
            
            var sortedList = set.OrderBy(d => d);
            int idx = (int)sortedList.Count() / 2;
            
            if (sortedList.Count() % 2 == 0)
                return (sortedList.ElementAt(idx) + sortedList.ElementAt(idx - 1)) / 2;
            else
                return sortedList.ElementAt(idx);
        }
    }
}
