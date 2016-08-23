using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealDebrid4DotNet
{
    public class Helpers
    {
        static string[] ordinals = new[] { "", "K", "M", "G", "T", "P", "E" };
        public static string FormatFilesize(int value)
        {
            int bandwidth = (int)value;
            decimal rate = (decimal)bandwidth;
            var ordinal = 0;

            while (rate > 1024)
            {
                rate /= 1024;
                ordinal++;
            }

            return String.Format("{0} {1}", Math.Round(rate, 2, MidpointRounding.AwayFromZero), ordinals[ordinal]);
        }
    }

}
