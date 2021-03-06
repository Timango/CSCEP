﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dauros.Timango.CSCEP
{
    public static class Extensions
    {
        public static Double ToUnixTimestamp(this DateTime time)
        {
            var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var timeDiff = time - unixStart;
            return (double)timeDiff.TotalSeconds;
        }

        public static DateTime AsUnixTimestamp(this Double unixTimeStamp)
        {
            var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            unixStart = unixStart.AddSeconds(unixTimeStamp).ToLocalTime();
            return unixStart;
        }

        public static string ToHexString(this byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        public static void AddOptional(this Dictionary<String, String> dic, String key, String value)
        {
            if (value != null)
                dic.Add(key, value);
        }

        public static void AddOptional(this Dictionary<String, String> dic, String key, Double? value, CultureInfo culture = null)
        {
            if (value.HasValue)
            {
                culture = culture ?? CultureInfo.InvariantCulture;
                dic.Add(key, value.Value.ToString(culture));
            }
        }

        public static void AddOptional(this Dictionary<String, String> dic, String key, Decimal? value, CultureInfo culture = null)
        {
            if (value.HasValue)
            {
                culture = culture ?? CultureInfo.InvariantCulture;
                dic.Add(key, value.Value.ToString(culture));
            }
        }

        public static void AddOptional(this Dictionary<String, String> dic, String key, Int32? value, CultureInfo culture = null)
        {
            if (value.HasValue)
            {
                culture = culture ?? CultureInfo.InvariantCulture;
                dic.Add(key, value.Value.ToString(culture));
            }
        }

        public static void AddOptional(this Dictionary<String, String> dic, String key, Int64? value, CultureInfo culture = null)
        {
            if (value.HasValue)
            {
                culture = culture ?? CultureInfo.InvariantCulture;
                dic.Add(key, value.Value.ToString(culture));
            }
        }
        
        public static void AddOptional(this Dictionary<String, String> dic, String key, Boolean? value)
        {
            if (value.HasValue)
                dic.Add(key, value.Value.ToString());
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
